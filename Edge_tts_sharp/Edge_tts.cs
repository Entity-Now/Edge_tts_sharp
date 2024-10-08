using Edge_tts_sharp.Model;
using System;
using System.Resources;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebSocketSharp;
using Edge_tts_sharp.Utils;
using System.Threading;


namespace Edge_tts_sharp
{
    public class Edge_tts
    {
        /// <summary>
        /// 调试模式
        /// </summary>
        public static bool Debug = false;
        /// <summary>
        /// 同步模式
        /// </summary>
        public static bool Await = false;
        static string GetGUID()
        {
            return Guid.NewGuid().ToString().Replace("-","");
        }
        /// <summary>
        /// 讲一个浮点型数值转换为百分比数值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static string FromatPercentage(double input)
        {
            string output;

            if (input < 0)
            {
                output = input.ToString("+#;-#;0") + "%";
            }
            else
            {
                output = input.ToString("+#;-#;0") + "%";
            }
            return output;
        }
        static string ConvertToAudioFormatWebSocketString(string outputformat)
        {
            return "Content-Type:application/json; charset=utf-8\r\nPath:speech.config\r\n\r\n{\"context\":{\"synthesis\":{\"audio\":{\"metadataoptions\":{\"sentenceBoundaryEnabled\":\"false\",\"wordBoundaryEnabled\":\"false\"},\"outputFormat\":\"" + outputformat + "\"}}}}";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lang">输出语言</param>
        /// <param name="voice">音源名</param>
        /// <param name="rate">语速，-100% - 100% 之间的值，无需传递百分号</param>
        /// <param name="text"></param>
        /// <returns></returns>
        static string ConvertToSsmlText(string lang, string voice, int rate, int volume, string text)
        {
            return $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis'  xml:lang='{lang}'><voice name='{voice}'><prosody pitch='+0Hz' rate ='{FromatPercentage(rate)}' volume='{volume}'>{text}</prosody></voice></speak>";
        }
        static string ConvertToSsmlWebSocketString(string requestId, string lang, string voice,int rate, int volume, string msg)
        {
            return $"X-RequestId:{requestId}\r\nContent-Type:application/ssml+xml\r\nPath:ssml\r\n\r\n{ConvertToSsmlText(lang, voice, rate, volume, msg)}";
        }
        /// <summary>
        /// 语言转文本，将结果返回到回调函数中
        /// </summary>
        /// <param name="option">播放参数</param>
        /// <param name="voice">音源参数</param>
        public static void Invoke(PlayOption option, eVoice voice, Action<List<byte>> callback, IProgress<List<byte>> progress = null)
        {
            var binary_delim = "Path:audio\r\n";
            var sendRequestId = GetGUID();
            var binary = new List<byte>();
            bool IsTurnEnd = false;

            var wss = new Wss("wss://speech.platform.bing.com/consumer/speech/synthesize/readaloud/edge/v1?TrustedClientToken=6A5AA1D4EAFF4E9FB37E23D68491D6F4");
            wss.OnMessage += (sender, e) =>
            {
                if (e.IsText)
                {
                    var data = e.Data;
                    var requestId = Regex.Match(data, @"X-RequestId:(?<requestId>.*?)\r\n").Groups["requestId"].Value;
                    if (data.Contains("Path:turn.start"))
                    {
                        // start of turn, ignore. 开始信号，不用处理
                    }
                    else if (data.Contains("Path:turn.end"))
                    {
                        // 返回内容
                        if (binary.Count > 0)
                        {
                            callback?.Invoke(binary);
                        }
                        else
                        {
                            throw new Exception("返回值为空！");
                        }
                        // end of turn, close stream. 结束信号，可主动关闭socket
                        // 音频发送完毕后，最后还会收到一个表示音频结束的文本信息
                        //wss.Close();
                    }
                    else if (data.Contains("Path:response"))
                    {
                        // context response, ignore. 响应信号，无需处理
                    }
                    else
                    {
                        // 未知错误，通常不会发生
                    }
                    if (Debug) Console.WriteLine(e.Data);
                    IsTurnEnd = true;
                }
                else if (e.IsBinary)
                {
                    var data = e.RawData;
                    var requestId = Regex.Match(e.Data, @"X-RequestId:(?<requestId>.*?)\r\n").Groups["requestId"].Value;
                    if (data[0] == 0x00 && data[1] == 0x67 && data[2] == 0x58)
                    {
                        // Last (empty) audio fragment. 空音频片段，代表音频发送结束
                    }
                    else
                    {
                        var index = Encoding.UTF8.GetString(data).IndexOf(binary_delim) + binary_delim.Length;
                        var curVal = data.Skip(index);
                        binary.AddRange(curVal);
                        // 传出
                        progress?.Report(curVal.ToList());
                    }
                }
            };
            wss.OnColse += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(option.SavePath))
                {
                    File.WriteAllBytes(option.SavePath, binary.ToArray());
                }
            };
            wss.OnLog += (onmsg) =>
            {
                if(Debug) Console.WriteLine($"[{onmsg.level.ToString()}] {onmsg.msg}");
            };
            if (wss.Run())
            {
                wss.Send(ConvertToAudioFormatWebSocketString(voice.SuggestedCodec));
                wss.Send(ConvertToSsmlWebSocketString(sendRequestId, voice.Locale, voice.Name, option.Rate, ((int)option.Volume * 100), option.Text));
            }
            while (Await && !IsTurnEnd)
            {
                Thread.Sleep(10);
            }
        }
        /// <summary>
        /// 另存为mp3文件
        /// </summary>
        /// <param name="option">播放参数</param>
        /// <param name="voice">音源参数</param>
        public static void SaveAudio(PlayOption option, eVoice voice)
        {
            if (string.IsNullOrEmpty(option.SavePath))
            {
                throw new Exception("保存路径为空，请核对参数后重试.");
            }
            Invoke(option, voice, null);
        }
        /// <summary>
        /// 调用微软Edge接口，文字转语音
        /// </summary>
        /// <param name="option">播放参数</param>
        /// <param name="voice">音源参数</param>
        public static void PlayText(PlayOption option, eVoice voice)
        {
            Invoke(option, voice, (_binary) =>
            {
                Audio.PlayToByteAsync(_binary.ToArray(), option.Volume);
            });
        }
        /// <summary>
        /// 获取一个`AudioPlayer`的对象
        /// </summary>
        /// <param name="option">播放参数</param>
        /// <param name="voice">音源参数</param>
        /// <returns></returns>
        public static AudioPlayer GetPlayer(PlayOption option, eVoice voice)
        {
            AudioPlayer player = null;
            Invoke(option, voice, (_binary) =>
            {
                player = new AudioPlayer(_binary.ToArray(), option.Volume);
            });
            while (player == null) 
            {
                Thread.Sleep(10);
            }
            return player;
        }
        /// <summary>
        /// 同步等待播放音频结束
        /// </summary>
        /// <param name="option">播放参数</param>
        /// <param name="voice">音源参数</param>
        //public static void PlayTextAsync(PlayOption option, eVoice voice)
        //{
        //    List<byte> buffer = new List<byte>();
        //    var audioStreamer = new Mp3AudioStreamer();
        //    var report = new Progress<List<byte>>((binary) =>
        //    {
        //        audioStreamer.OnAudioReceived(binary.ToArray());
        //    });
        //    Invoke(option, voice, null, report);

        //    audioStreamer.Stop();
        //}
        
        /// <summary>
        /// 获取支持的音频列表
        /// </summary>
        /// <returns></returns>
        public static List<eVoice> GetVoice()
        {
            var voiceList = Tools.GetEmbedText("Edge_tts_sharp.Source.VoiceList.json");
            return Tools.StringToJson<List<eVoice>>(voiceList);
        }
    }
}
