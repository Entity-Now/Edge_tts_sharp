# Edge_tts_sharp 
[Edge_tts_sharp](https://www.nuget.org/packages/Edge_tts_sharp)，是一个免费的C#库，调用Microsoft Edge Text to Speech接口生成音频。

## install
```sh
NuGet\Install-Package Edge_tts_sharp
```
## 方法

### 全局对象
| 参数 | 说明 |
| --- | --- |
| Edge_tts.Debug | 调试模式，为true则显示日志 |
| Edge_tts.Await | 同步模式，为true会等待函数执行完毕 | 

### Invoke/PlayText/SaveAudio方法
| 参数 | 说明 |
| --- | --- |
| PlayOption | 参数配置 |
| eVoice | 音源 |
| Action<List<callback>> | 回调函数，参数是一个binary数组 |

### PlayOption对象
| 名称  | 说明 |
| --- | --- |
| Text | 播放的文本 |
| Rate | 播放速度，是一个-100至+100的数值 |
| Volume | 音量，是一个0-1的浮点数值 |
| SavePath | 音频保存路径，为空不保存 |

## 获取一个Player对象
> **PlayerAudio**对象，支持对音频进行简单的控制，例如：开始、暂停、继续播放、停止播放等。
```cs
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
```

## 文字转语言
```cs
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
```

## 保存到本地
```cs
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
```

## 自定义操作
```cs
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
```

## 获取音频列表
```cs
using Edge_tts_sharp;

var voices = Edge_tts.GetVoice();
foreach(var item in voices){
    Console.WriteLine($"voice name is{item.Name}, locale（语言） is {item.Locale}, SuggestedCodec(音频类型) is {item.SuggestedCodec}");
}
```
## 汉语语音包有：

| ShortName              | Locale       | 地区         |
|------------------------|--------------|--------------|
| zh-HK-HiuGaaiNeural    | zh-HK        | 香港         |
| zh-HK-HiuMaanNeural    | zh-HK        | 香港         |
| zh-HK-WanLungNeural    | zh-HK        | 香港         |
| zh-CN-XiaoxiaoNeural   | zh-CN        | 中国（大陆） |
| zh-CN-XiaoyiNeural     | zh-CN        | 中国（大陆） |
| zh-CN-YunjianNeural    | zh-CN        | 中国（大陆） |
| zh-CN-YunxiNeural      | zh-CN        | 中国（大陆） |
| zh-CN-YunxiaNeural     | zh-CN        | 中国（大陆） |
| zh-CN-YunyangNeural    | zh-CN        | 中国（大陆） |
| zh-CN-liaoning-XiaobeiNeural | zh-CN-liaoning | 中国（辽宁） |
| zh-TW-HsiaoChenNeural  | zh-TW        | 台湾         |
| zh-TW-YunJheNeural     | zh-TW        | 台湾         |
| zh-TW-HsiaoYuNeural    | zh-TW        | 台湾         |
| zh-CN-shaanxi-XiaoniNeural | zh-CN-shaanxi | 中国（陕西） |


## 更新内容

- 2023.10.28
    - 第一次上传。
- 2023.10.30
    - 更新调用接口的方式
