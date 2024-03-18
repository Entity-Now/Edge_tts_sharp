// See https://aka.ms/new-console-template for more information
using Edge_tts_sharp;
using Edge_tts_sharp.Model;

string msg = string.Empty;
Console.WriteLine("请输入文本内容.");
msg = Console.ReadLine();
// 获取xiaoxiao语音包
var voice = Edge_tts.GetVoice().FirstOrDefault(i=> i.Name == "Microsoft Server Speech Text to Speech Voice (zh-CN, XiaoxiaoNeural)");
// 文字转语音，并且设置语速
Edge_tts.PlayText(msg, voice);

Console.ReadLine();


// 保存音频
static void SaveAudio()
{
    // 获取xiaoxiao语音包
    var voice = Edge_tts.GetVoice().FirstOrDefault(i => i.Name == "Microsoft Server Speech Text to Speech Voice (zh-CN, XiaoxiaoNeural)");
    Edge_tts.SaveAudio("hello Edge~", voice, 0, "C:\\audio");
}
// 文本转语音
static void TextToAudio()
{
    var voice = Edge_tts.GetVoice().First();
    Edge_tts.PlayText("hello Edge~", voice);
}
// 自定义接口使用
static void MyFunc(string msg, eVoice voice)
{
    Edge_tts.Invoke(msg, voice, 0, libaray =>
    {
        // 写入自己的操作
        // ...
    } );
}
// 获取一个PlayerAudio对象
static void getPlayer(string msg, eVoice voice)
{
    var player = Edge_tts.GetPlayer(msg, voice);

    Console.WriteLine("开始播放");
    player.PlayAsync();
    Thread.Sleep(3000);


    Console.WriteLine("暂停播放");
    player.Pause();
    Thread.Sleep(3000);

    Console.WriteLine("继续播放");
    player.PlayAsync();
    Thread.Sleep(5000);

    player.Stop();
    Console.WriteLine("结束播放");
}