// See https://aka.ms/new-console-template for more information
using Edge_tts_sharp;
using Edge_tts_sharp.Model;
using Edge_tts_sharp.Utils;
using System.Web;

Edge_tts.Await = true;
PlayOption option = new PlayOption
{
    Rate = 1,
    Text = ""
};
string msg = string.Empty;
Console.WriteLine("请输入文本内容.");
option.Text = Console.ReadLine();
// 获取xiaoxiao语音包
var voice = Edge_tts.GetVoice().FirstOrDefault(i=> i.Name == "Microsoft Server Speech Text to Speech Voice (zh-CN, XiaoxiaoNeural)");
// 文字转语音，并且设置语速
Edge_tts.PlayText(option, voice);
Console.WriteLine("自动输出");
Console.ReadLine();


// 保存音频
static void SaveAudio()
{
    PlayOption option = new PlayOption
    {
        Rate = 0,
        Text = "Hello EdgeTTs",
        SavePath = "C:\\audio"
    };
    // 获取xiaoxiao语音包
    var voice = Edge_tts.GetVoice().FirstOrDefault(i => i.Name == "Microsoft Server Speech Text to Speech Voice (zh-CN, XiaoxiaoNeural)");
    Edge_tts.SaveAudio(option, voice);
}
// 文本转语音
static void TextToAudio()
{
    PlayOption option = new PlayOption
    {
        Rate = 0,
        Text = "Hello EdgeTTs",
    };
    var voice = Edge_tts.GetVoice().First();
    Edge_tts.PlayText(option, voice);
}
// 自定义接口使用
static void MyFunc(string msg, eVoice voice)
{
    PlayOption option = new PlayOption
    {
        Rate = 0,
        Text = msg,
    };
    Edge_tts.Invoke(option, voice, libaray =>
    {
        // 写入自己的操作
        // ...
    } );
}
// 获取一个PlayerAudio对象
static void getPlayer(string msg, eVoice voice)
{
    PlayOption option = new PlayOption
    {
        Rate = 0,
        Text = msg,
    };
    var player = Edge_tts.GetPlayer(option, voice);

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