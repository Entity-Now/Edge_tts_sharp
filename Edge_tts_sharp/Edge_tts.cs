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


namespace Edge_tts_sharp
{
    public class Edge_tts
    {
        public static string GetGUID()
        {
            return Guid.NewGuid().ToString().Replace("-","");
        }
        static string ConvertToAudioFormatWebSocketString(string outputformat)
        {
            return "Content-Type:application/json; charset=utf-8\r\nPath:speech.config\r\n\r\n{\"context\":{\"synthesis\":{\"audio\":{\"metadataoptions\":{\"sentenceBoundaryEnabled\":\"false\",\"wordBoundaryEnabled\":\"false\"},\"outputFormat\":\"" + outputformat + "\"}}}}";
        }
        static string ConvertToSsmlText(string lang, string voice, string text)
        {
            return $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xmlns:mstts='https://www.w3.org/2001/mstts' xml:lang='{lang}'><voice name='{voice}'>{text}</voice></speak>";
        }
        static string ConvertToSsmlWebSocketString(string requestId, string lang, string voice, string msg)
        {
            return $"X-RequestId:{requestId}\r\nContent-Type:application/ssml+xml\r\nPath:ssml\r\n\r\n{ConvertToSsmlText(lang, voice, msg)}";
        }
        public static void PlayText(string msg, string lang, string voice, string audioOutPutFormat, string savePath = "")
        {
            var binary_delim = "Path:audio\r\n";
            var sendRequestId = GetGUID();
            var binary = new List<byte>();

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
                        // end of turn, close stream. 结束信号，可主动关闭socket
                        // dataBuffers[requestId] = null;
                        // 不要跟着MsEdgeTTS中用上面那句，音频发送完毕后，最后还会收到一个表示音频结束的文本信息
                        wss.Close();
                    }
                    else if (data.Contains("Path:response"))
                    {
                        // context response, ignore. 响应信号，无需处理
                    }
                    else
                    {
                        // 未知错误，通常不会发生
                    }
                    Console.WriteLine(e.Data);
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
                        binary.AddRange(data.Skip(index));
                    }
                }
            };
            wss.OnColse += (sender, e) =>
            {
                //File.WriteAllBytes($"{savePath}temp.mp3", binary.ToArray());
                if (binary.Count > 0)
                {
                    Audio.PlayToByte(binary.ToArray());
                }
            };
            wss.OnLog += (onmsg) =>
            {
                Console.WriteLine($"[{onmsg.level.ToString()}] {onmsg.msg}");
            };
            if (wss.Run())
            {
                wss.Send(ConvertToAudioFormatWebSocketString(audioOutPutFormat));
                wss.Send(ConvertToSsmlWebSocketString(sendRequestId, lang, voice, msg));
            }

        }
        public static List<eVoice> GetVoice()
        {
            var voiceList = Tools.GetEmbedText("Edge_tts_sharp.Source.VoiceList.json");
            return Tools.StringToJson<List<eVoice>>(voiceList);
        }
    }
}
