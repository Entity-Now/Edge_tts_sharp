# Edge_tts_sharp 
此类库用于免费使用微软edge浏览器大声朗读

## 文字转语言
```cs
using Edge_tts_sharp;

var msg = "你好啊？";
Edge_tts.PlayText(msg, "zh-CN", "Microsoft Server Speech Text to Speech Voice (zh-CN, XiaoxiaoNeural)", "webm-24khz-16bit-mono-opus");


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