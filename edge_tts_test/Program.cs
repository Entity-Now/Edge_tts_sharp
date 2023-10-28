// See https://aka.ms/new-console-template for more information
using Edge_tts_sharp;

string msg = string.Empty;
Console.WriteLine("请输入文本内容.");
msg = Console.ReadLine();

//Edge_tts.PlayText(msg, "zh-CN", "Microsoft Server Speech Text to Speech Voice (zh-CN, XiaoxiaoNeural)", "webm-24khz-16bit-mono-opus");
Edge_tts.GetVoice();
Console.ReadLine();