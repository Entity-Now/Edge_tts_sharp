
using Edge_tts_sharp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edge_tts_sharp
{
    public enum SslProtocolsHack
    {
        Tls = 192,
        Tls11 = 768,
        Tls12 = 3072
    }

    public class Wss
    {
        // ======================
        // 内嵌事件参数类型（替代 WebSocketSharp）
        // ======================
        public class MessageEventArgs : EventArgs
        {
            private readonly bool _isText;
            private readonly string _data;
            private readonly byte[] _rawData;

            public bool IsText { get { return _isText; } }
            public bool IsBinary { get { return !_isText; } }
            public string Data { get { return _data; } }
            public byte[] RawData { get { return _rawData; } }

            public MessageEventArgs(string data)
            {
                _isText = true;
                _data = data;
                _rawData = data == null ? new byte[0] : Encoding.UTF8.GetBytes(data);
            }

            public MessageEventArgs(byte[] rawData)
            {
                _isText = false;
                _rawData = rawData ?? new byte[0];
                _data = null;
            }
        }

        public class CloseEventArgs : EventArgs
        {
            private readonly ushort _code;
            private readonly string _reason;

            public ushort Code { get { return _code; } }
            public string Reason { get { return _reason; } }

            public CloseEventArgs(ushort code, string reason)
            {
                _code = code;
                _reason = reason;
            }
        }

        // ======================
        // 保持原有签名的事件与属性
        // ======================
        public event Action<Log> OnLog;
        public event EventHandler<MessageEventArgs> OnMessage;
        public event EventHandler<CloseEventArgs> OnColse; // 保持原拼写

        public string wssAddress { get; set; }

        // 兼容旧代码可能访问 wss 属性：保留同名属性但使用 object 类型
        public object wss { get; private set; }

        // ======================
        // 内部实现字段
        // ======================
        private ClientWebSocket _client;
        private CancellationTokenSource _cts;
        private Task _receiveLoopTask;

        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly CookieContainer _cookieContainer = new CookieContainer();

        // TLS/证书控制
        private SslProtocols _enabledSslProtocols =
            SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
        private RemoteCertificateValidationCallback _certValidation;

        private bool _isSecure;

        public Wss(string url)
        {
            try
            {
                wssAddress = url;
                var uri = new Uri(url);
                _isSecure = string.Equals(uri.Scheme, "wss", StringComparison.OrdinalIgnoreCase);

                // 保持原逻辑：wss 默认忽略证书错误（如需严格验证可修改此处）
                if (_isSecure)
                {
                    _certValidation = new RemoteCertificateValidationCallback(
                        (sender, certificate, chain, sslPolicyErrors) => true
                    );
                }

                // 根据旧枚举设定 TLS 可用版本（与原代码行为一致）
                var hack = (SslProtocolsHack.Tls12 | SslProtocolsHack.Tls11 | SslProtocolsHack.Tls);
                _enabledSslProtocols = (SslProtocols)hack;

                // 初始化 ClientWebSocket
                _client = new ClientWebSocket();
                _client.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
                _client.Options.Cookies = _cookieContainer;

                // 将内部 client 通过 wss 暴露为 object（保持原属性名）
                wss = _client;

                // 应用证书验证（尽可能在 Options 级别；否则退回到全局）
                ApplyCertificateValidation(_client, _certValidation);

                // 应用 TLS 版本（某些运行时需要通过全局 SecurityProtocolType）
                TrySetGlobalSecurityProtocol(_enabledSslProtocols);
            }
            catch (Exception e)
            {
                SafeLog(new Log { level = level.error, msg = "WebSocket Exception:" + e });
                throw;
            }
        }

        // ======================
        // 公共方法（签名保持一致）
        // ======================
        public bool Run()
        {
            EnsureClient();

            var uri = new Uri(wssAddress);
            _cts = new CancellationTokenSource();

            // 先应用 Cookie（必须在 Connect 之前）
            // 注意：为了兼容某些运行时握手只读取 http(s) 容器的情况，
            // 我们将 Cookie 同时写入 ws/wss 原地址与其对应的 http/https 握手地址。
            // 用户通过 AddCookie 已经把值写入 _cookieContainer；此处无需额外处理。

            // 应用 Header（如果运行时支持）
            ApplyHeaders(_client, _headers);

            try
            {
                _client.ConnectAsync(uri, _cts.Token).GetAwaiter().GetResult();

                SafeLog(new Log { level = level.info, msg = "WebSocket 连接成功." });
                if (_isSecure)
                {
                    SafeLog(new Log { level = level.info, msg = "WebSocket 是安全的." });
                }

                // 启动接收循环
                _receiveLoopTask = Task.Run(new Func<Task>(ReceiveLoopAsync));
                return IsAlive();
            }
            catch (WebSocketException wse)
            {
                // 模拟原逻辑：TLS 握手失败视为 1015
                SafeLog(new Log { level = level.error, msg = "连接异常: " + wse.Message });
                SafeCloseEvent(1015, "TLS handshake failure: " + wse.Message);

                // 尝试回退并重连一次（兼容原代码行为）
                TryTlsFallbackAndReconnect(uri);
                return IsAlive();
            }
            catch (AuthenticationException ae)
            {
                SafeLog(new Log { level = level.error, msg = "证书异常: " + ae.Message });
                SafeCloseEvent(1015, "TLS handshake failure: " + ae.Message);
                TryTlsFallbackAndReconnect(uri);
                return IsAlive();
            }
            catch (Exception ex)
            {
                SafeLog(new Log { level = level.error, msg = "连接异常: " + ex.Message });
                return false;
            }
        }

        public void Close()
        {
            try
            {
                if (_client != null &&
                    (_client.State == WebSocketState.Open || _client.State == WebSocketState.CloseReceived))
                {
                    _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client close", CancellationToken.None)
                           .GetAwaiter().GetResult();
                }
            }
            catch
            {
                // 忽略关闭异常
            }
            finally
            {
                if (_cts != null) _cts.Cancel();
                if (_client != null) _client.Dispose();
                _client = null;
                wss = null;
            }
        }

        public void Send(string msg)
        {
            if (!IsAlive()) return;
            var data = Encoding.UTF8.GetBytes(msg ?? string.Empty);
            _client.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, _cts.Token)
                   .GetAwaiter().GetResult();

            SafeLog(new Log { level = level.info, msg = "WebSocket send msg:" + msg });
        }

        public void SendByte(byte[] msg)
        {
            if (!IsAlive()) return;
            var data = msg ?? new byte[0];
            _client.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, _cts.Token)
                   .GetAwaiter().GetResult();

            SafeLog(new Log { level = level.info, msg = "WebSocket send msg: binary" });
        }

        // 新增：添加请求头（在 Run() 之前调用）
        public void AddHeader(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;

            // 专门处理：子协议（所有版本均支持，确保一定生效）
            if (string.Equals(key, "Sec-WebSocket-Protocol", StringComparison.OrdinalIgnoreCase))
            {
                // 支持逗号分隔多个子协议
                var protocols = (value ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < protocols.Length; i++)
                {
                    string p = protocols[i].Trim();
                    if (p.Length > 0)
                    {
                        _client.Options.AddSubProtocol(p);
                        SafeLog(new Log { level = level.info, msg = "AddSubProtocol: " + p });
                    }
                }
                return;
            }

            // 其它头部：尝试后续在 Connect 前应用（如果运行时支持）
            _headers[key] = value ?? string.Empty;
            SafeLog(new Log { level = level.info, msg = "AddHeader: " + key + "=" + value });
        }

        // 新增：添加 Cookie（默认 path="/"，domain 为 URL 主机）
        public void AddCookie(string name, string value)
        {
            AddCookie(name, value, "/", null);
        }

        // 可选重载：自定义 path 与 domain
        public void AddCookie(string name, string value, string path, string domain)
        {
            if (string.IsNullOrEmpty(name)) return;

            Uri wsUri = new Uri(wssAddress);
            string host = (domain != null && domain.Length > 0) ? domain : wsUri.Host;
            string cookiePath = (path != null && path.Length > 0) ? path : "/";

            Cookie cookie = new Cookie(name, value ?? string.Empty, cookiePath, host);

            // 关键：为确保握手阶段一定能带上 Cookie，
            // 同时向 ws/wss 原地址与对应的 http/https 握手地址都写入 Cookie。
            // 部分运行时仅从 http(s) 容器读取 Cookie。
            Uri handshakeUri = BuildHandshakeUri(wsUri);

            try
            {
                _cookieContainer.Add(wsUri, cookie);
            }
            catch (Exception ex)
            {
                SafeLog(new Log { level = level.warning, msg = "向 WS 容器写入 Cookie 失败: " + ex.Message });
            }

            try
            {
                _cookieContainer.Add(handshakeUri, cookie);
            }
            catch (Exception ex)
            {
                SafeLog(new Log { level = level.warning, msg = "向 HTTP(S) 容器写入 Cookie 失败: " + ex.Message });
            }

            SafeLog(new Log { level = level.info, msg = "AddCookie: " + name + "=" + value + "; path=" + cookiePath + "; domain=" + host });
        }

        // ======================
        // 内部辅助
        // ======================
        private bool IsAlive()
        {
            return _client != null && _client.State == WebSocketState.Open;
        }

        private void EnsureClient()
        {
            if (_client == null)
            {
                _client = new ClientWebSocket();
                _client.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
                _client.Options.Cookies = _cookieContainer;
                wss = _client;

                ApplyCertificateValidation(_client, _certValidation);
                TrySetGlobalSecurityProtocol(_enabledSslProtocols);
            }
        }

        private Task ReceiveLoopAsync()
        {
            return Task.Run(async () =>
            {
                byte[] buffer = new byte[8192];

                try
                {
                    while (_client != null && _client.State == WebSocketState.Open && (_cts != null && !_cts.IsCancellationRequested))
                    {
                        MemoryStream ms = new MemoryStream();
                        try
                        {
                            WebSocketReceiveResult result;

                            do
                            {
                                var seg = new ArraySegment<byte>(buffer);
                                result = await _client.ReceiveAsync(seg, _cts.Token).ConfigureAwait(false);

                                if (result.MessageType == WebSocketMessageType.Close)
                                {
                                    ushort code = (ushort)(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure);
                                    string reason = result.CloseStatusDescription;
                                    SafeCloseEvent(code, reason);

                                    await _client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Close acknowledged", _cts.Token).ConfigureAwait(false);
                                    ms.Dispose();
                                    return;
                                }

                                ms.Write(buffer, 0, result.Count);
                            }
                            while (!result.EndOfMessage);

                            ms.Position = 0;
                            if (result.MessageType == WebSocketMessageType.Text)
                            {
                                using (var reader = new StreamReader(ms, Encoding.UTF8, true))
                                {
                                    string text = await reader.ReadToEndAsync().ConfigureAwait(false);
                                    SafeMessageEvent(new MessageEventArgs(text));
                                }
                            }
                            else if (result.MessageType == WebSocketMessageType.Binary)
                            {
                                byte[] bytes = ms.ToArray();
                                SafeMessageEvent(new MessageEventArgs(bytes));
                            }
                        }
                        finally
                        {
                            ms.Dispose();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // 正常取消
                }
                catch (Exception ex)
                {
                    // 异常时视为异常关闭
                    SafeCloseEvent(1006, "Abnormal closure: " + ex.Message);
                }
            });
        }

        private static void ApplyCertificateValidation(ClientWebSocket client, RemoteCertificateValidationCallback callback)
        {
            if (callback == null) return;
            try
            {
                // 若运行时支持 Options.RemoteCertificateValidationCallback（如 .NET 8），则优先设置
                var prop = client.Options.GetType().GetProperty("RemoteCertificateValidationCallback");
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(client.Options, callback, null);
                }
                else
                {
                    // 退路：进程级别（可能影响其他请求，谨慎）
                    ServicePointManager.ServerCertificateValidationCallback = callback;
                }
            }
            catch
            {
                // 忽略设置异常
            }
        }

        private static void TrySetGlobalSecurityProtocol(SslProtocols protocols)
        {
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)protocols;
            }
            catch
            {
                // 在 .NET Core/Linux 等环境可能不可用，忽略
            }
        }

        private void ApplyHeaders(ClientWebSocket client, Dictionary<string, string> headers)
        {
            bool setHeaderSupported = false;
            System.Reflection.MethodInfo method = null;

            try
            {
                method = client.Options.GetType().GetMethod("SetRequestHeader");
                if (method != null)
                {
                    setHeaderSupported = true;
                }
            }
            catch
            {
                setHeaderSupported = false;
            }

            foreach (var kv in headers)
            {
                string key = kv.Key;
                string val = kv.Value;

                // 已在 AddHeader 中处理过 Sec-WebSocket-Protocol，这里不再重复
                if (string.Equals(key, "Sec-WebSocket-Protocol", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (setHeaderSupported)
                {
                    try
                    {
                        method.Invoke(client.Options, new object[] { key, val });
                        SafeLog(new Log { level = level.info, msg = "SetRequestHeader 应用成功: " + key + "=" + val });
                    }
                    catch (Exception ex)
                    {
                        SafeLog(new Log { level = level.warning, msg = "SetRequestHeader 应用失败: " + key + "，" + ex.Message });
                    }
                }
                else
                {
                    // 对于不支持任意请求头的运行时，给出明确提示
                    SafeLog(new Log
                    {
                        level = level.warning,
                        msg = "当前运行时不支持设置自定义请求头（" + key + "）。如需携带认证信息，建议改用 Cookie 或 Sec-WebSocket-Protocol。"
                    });
                }
            }
        }

        private Uri BuildHandshakeUri(Uri wsUri)
        {
            // 将 ws/wss 转换为 http/https，以适配握手阶段读取 Cookie 的运行时差异
            string scheme = wsUri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase) ? "https" : "http";
            UriBuilder b = new UriBuilder(wsUri);
            b.Scheme = scheme;
            // 保持端口一致（如果原 URI 显式指定端口）
            return b.Uri;
        }

        private void TryTlsFallbackAndReconnect(Uri uri)
        {
            try
            {
                _enabledSslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
                TrySetGlobalSecurityProtocol(_enabledSslProtocols);

                if (_client != null) _client.Dispose();
                _client = new ClientWebSocket();
                _client.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
                _client.Options.Cookies = _cookieContainer;
                wss = _client;

                ApplyCertificateValidation(_client, _certValidation);
                ApplyHeaders(_client, _headers);

                _client.ConnectAsync(uri, _cts.Token).GetAwaiter().GetResult();
                SafeLog(new Log { level = level.info, msg = "重试连接成功." });

                _receiveLoopTask = Task.Run(new Func<Task>(ReceiveLoopAsync));
            }
            catch (Exception ex)
            {
                SafeLog(new Log { level = level.error, msg = "重试连接失败: " + ex.Message });
            }
        }

        // 事件触发的安全封装（避免空引用）
        private void SafeLog(Log log)
        {
            var handler = OnLog;
            if (handler != null) handler(log);
        }

        private void SafeMessageEvent(MessageEventArgs e)
        {
            var handler = OnMessage;
            if (handler != null) handler(this, e);
        }

        private void SafeCloseEvent(ushort code, string reason)
        {
            var handler = OnColse;
            if (handler != null) handler(this, new CloseEventArgs(code, reason));
        }
    }
}
