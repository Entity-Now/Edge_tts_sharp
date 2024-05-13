# Edge_tts_sharp 
[Edge_tts_sharp](https://www.nuget.org/packages/Edge_tts_sharp)，是一个免费的C#库，调用Microsoft Edge Text to Speech接口生成音频。

## install
```sh
NuGet\Install-Package Edge_tts_sharp
```

## 文字转语言
```cs
# 文本转语音
static void TextToAudio()
{
    var voice = Edge_tts.GetVoice().First();
    Edge_tts.PlayText("hello Edge~", voice);
}
```

## 设置语速和音量
| 属性 | 说明 |
| --- | --- |
| msg | 文本 |
| voice | voice对象 | 
| rate | （可选）调整语速，是一个-100 - 100的数值 |
| volume | （可选）调整音量，是一个0 - 1的数值 |
```cs
# 文字转语音，并且设置语速
var voice = Edge_tts.GetVoice().First();
Edge_tts.PlayText("hello Edge~", voice， -25， 0.5f);
```

## 保存到本地
```cs
// 保存音频
static void SaveAudio()
{
    // 获取xiaoxiao语音包
    var voice = Edge_tts.GetVoice().FirstOrDefault(i => i.Name == "Microsoft Server Speech Text to Speech Voice (zh-CN, XiaoxiaoNeural)");
    Edge_tts.SaveAudio("hello Edge~", voice, 0, "C:\\audio");
}
```

## 自定义操作
```cs
// 回调函数的第一个参数是binary数据
static void MyFunc(string msg, eVoice voice)
{
    Edge_tts.Invoke(msg, voice, 0, libaray =>
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

## 更新内容

- 2023.10.28
    - 第一次上传。
- 2023.10.30
    - 更新调用接口的方式