# Edge_tts_sharp

[Edge_tts_sharp](https://www.nuget.org/packages/Edge_tts_sharp) 是一个免费的C#库（版本1.1.6），调用Microsoft Edge Text to Speech接口生成高质量的语音音频。

## 项目特点

- **免费使用**：无需API密钥，直接调用Microsoft Edge的TTS服务
- **多语言支持**：支持包括中文在内的多种语言和声音
- **自定义设置**：可调整语速、音量等参数
- **灵活输出**：支持直接播放或保存为音频文件
- **播放控制**：支持播放、暂停、继续、停止等基本操作
- **跨平台**：基于.NET Standard 2.0，可在多种平台上运行

## 快速开始

### 安装

通过NuGet包管理器安装：

```sh
NuGet\Install-Package Edge_tts_sharp
```

### 基本用法示例

```csharp
using Edge_tts_sharp;
using Edge_tts_sharp.Model;

// 设置为同步模式，等待函数执行完毕
Edge_tts.Await = true;

// 创建播放选项
PlayOption option = new PlayOption
{
    Text = "你好，这是Edge TTSSharp的示例文本。",
    Rate = 0,      // 语速，范围：-100到100
    Volume = 1.0f  // 音量，范围：0到1
};

// 获取中文语音（例如：晓晓）
var voice = Edge_tts.GetVoice().FirstOrDefault(v => v.Name.Contains("Xiaoxiao"));

// 播放文本
Edge_tts.PlayText(option, voice);
```

## 核心功能

### 1. 文本转语音播放

```csharp
static void TextToAudio()
{
    PlayOption option = new PlayOption
    {
        Rate = 0,     // 语速
        Text = "Hello EdgeTTs",  // 要转换的文本
    };
    
    // 获取第一个可用的语音
    var voice = Edge_tts.GetVoice().First();
    
    // 播放文本
    Edge_tts.PlayText(option, voice);
}
```

### 2. 保存音频到本地

```csharp
static void SaveAudio()
{
    PlayOption option = new PlayOption
    {
        Rate = 0,                   // 语速
        Text = "Hello EdgeTTs",    // 要转换的文本
        SavePath = "C:\\audio.mp3"  // 保存路径
    };
    
    // 获取特定语音（例如：中文晓晓）
    var voice = Edge_tts.GetVoice().FirstOrDefault(i => 
        i.Name == "Microsoft Server Speech Text to Speech Voice (zh-CN, XiaoxiaoNeural)");
    
    // 保存音频
    Edge_tts.SaveAudio(option, voice);
}
```

### 3. 获取音频播放器对象

通过`GetPlayer`方法可以获取一个`AudioPlayer`对象，支持对音频进行更精细的控制，例如：开始、暂停、继续播放、重新播放、停止播放等：

```csharp
static void GetPlayerAndControl()
{
    PlayOption option = new PlayOption
    {
        Rate = 0,
        Text = "这是一段可以控制播放的文本内容。",
    };
    
    // 获取语音
    var voice = Edge_tts.GetVoice().FirstOrDefault(v => v.Name.Contains("Xiaoxiao"));
    
    // 获取播放器对象
    var player = Edge_tts.GetPlayer(option, voice);

    Console.WriteLine("开始播放");
    player.PlayAsync();
    Thread.Sleep(3000);

    Console.WriteLine("暂停播放");
    player.Pause();
    Thread.Sleep(3000);

    Console.WriteLine("继续播放（从暂停位置）");
    player.PlayAsync();
    Thread.Sleep(2000);

    Console.WriteLine("重新播放（从头开始）");
    player.Resume();
    Thread.Sleep(3000);

    Console.WriteLine("停止播放");
    player.Stop();
}
```

### 4. 自定义处理音频数据

使用`Invoke`方法可以自定义处理生成的音频数据：

```csharp
static void CustomProcessAudio()
{
    PlayOption option = new PlayOption
    {
        Rate = 0,
        Text = "这是一段将被自定义处理的文本。",
    };
    
    // 获取语音
    var voice = Edge_tts.GetVoice().FirstOrDefault(v => v.Name.Contains("Xiaoxiao"));
    
    // 自定义处理
    Edge_tts.Invoke(option, voice, audioData =>
    {
        // 这里可以对音频数据进行自定义处理
        Console.WriteLine($"接收到{audioData.Count}字节的音频数据");
        // 例如：保存到自定义位置、实时处理等
        // 注意：此回调函数在音频完全生成后才会被调用一次
    });
}
```

### 5. 获取支持的语音列表

```csharp
using Edge_tts_sharp;

// 获取所有支持的语音
var voices = Edge_tts.GetVoice();

// 遍历并显示语音信息
foreach(var voice in voices)
{
    Console.WriteLine($"语音名称: {voice.Name}");
    Console.WriteLine($"语言区域: {voice.Locale}");
    Console.WriteLine($"音频格式: {voice.SuggestedCodec}");
    Console.WriteLine($"友好名称: {voice.FriendlyName}");
    Console.WriteLine($"性别: {voice.Gender}");
    Console.WriteLine("------------------------");
}
```

## 音频播放工具（Audio类）

`Audio`类提供了一些静态方法，可以直接播放不同来源的音频：

```csharp
// 从流播放音频
await Audio.PlayToStreamAsync(stream, volume: 1.0f, speed: 0.0f);

// 从字节数组播放音频
await Audio.PlayToByteAsync(audioBytes, volume: 1.0f, speed: 0.0f);

// 从文件路径播放音频
await Audio.PlayAudioAsync("path/to/audio.mp3", volume: 1.0f, speed: 0.0f);

// 从URL播放音频
await Audio.PlayAudioFromUrlAsync("https://example.com/audio.mp3", volume: 1.0f, speed: 0.0f);
```

## 配置参数

### 全局配置

| 参数 | 类型 | 说明 |
|------|------|------|
| `Edge_tts.Debug` | `bool` | 调试模式，设为`true`时显示详细日志 |
| `Edge_tts.Await` | `bool` | 同步模式，设为`true`时会等待函数执行完毕 |

### PlayOption 配置项

| 属性 | 类型 | 说明 | 默认值 |
|------|------|------|--------|
| `Text` | `string` | 要转换为语音的文本内容 | `string.Empty` |
| `Rate` | `int` | 语速，范围：-100（最慢）到100（最快） | 0 |
| `Volume` | `float` | 音量，范围：0（静音）到1（最大） | 1.0f |
| `SavePath` | `string` | 音频保存路径，为空时不保存文件 | `string.Empty` |

## 支持的中文语音

下表列出了部分常用的中文语音选项：

| ShortName | Locale | 地区 |
|-----------|--------|------|
| zh-HK-HiuGaaiNeural | zh-HK | 香港 |
| zh-HK-HiuMaanNeural | zh-HK | 香港 |
| zh-HK-WanLungNeural | zh-HK | 香港 |
| zh-CN-XiaoxiaoNeural | zh-CN | 中国大陆 |
| zh-CN-XiaoyiNeural | zh-CN | 中国大陆 |
| zh-CN-YunjianNeural | zh-CN | 中国大陆 |
| zh-CN-YunxiNeural | zh-CN | 中国大陆 |
| zh-CN-YunxiaNeural | zh-CN | 中国大陆 |
| zh-CN-YunyangNeural | zh-CN | 中国大陆 |
| zh-CN-liaoning-XiaobeiNeural | zh-CN-liaoning | 中国辽宁 |
| zh-TW-HsiaoChenNeural | zh-TW | 台湾 |
| zh-TW-YunJheNeural | zh-TW | 台湾 |
| zh-TW-HsiaoYuNeural | zh-TW | 台湾 |
| zh-CN-shaanxi-XiaoniNeural | zh-CN-shaanxi | 中国陕西 |

## 技术原理

Edge_tts_sharp 通过 WebSocket 连接到 Microsoft Edge 的 TTS 服务，发送 SSML (Speech Synthesis Markup Language) 格式的请求，并接收返回的音频流。整个流程包括：

1. 建立与 Microsoft Edge TTS 服务的 WebSocket 连接
2. 发送音频格式配置
3. 将文本转换为 SSML 格式并发送
4. 接收并处理返回的音频数据
5. 根据配置进行播放或保存

## 项目结构

```
Edge_tts_sharp/
├── Edge_tts.cs           # 核心功能实现
├── Model/
│   ├── Log.cs            # 日志模型
│   ├── PlayOption.cs     # 播放选项模型
│   └── eVoice.cs         # 语音模型
├── Source/
│   └── VoiceList.json    # 内置语音列表
├── Utils/
│   ├── Audio.cs          # 音频处理工具
│   ├── AudioPlayer.cs    # 音频播放器
│   └── AudioStreamer.cs  # 音频流处理
├── Tools.cs              # 通用工具类
└── Wss.cs                # WebSocket封装
```

## 依赖项

- [WebSocketSharp 1.0.3-rc11](https://www.nuget.org/packages/WebSocketSharp/) - 用于WebSocket通信
- [NAudio 2.0.0](https://www.nuget.org/packages/NAudio/) - 用于音频播放和处理
- [System.Text.Json 4.6.0](https://www.nuget.org/packages/System.Text.Json/) - 用于JSON序列化和反序列化

## 注意事项

1. 本库依赖于Microsoft Edge TTS服务，需要保持网络连接
2. 请合理使用服务，避免过度请求
3. 如需在生产环境中大规模使用，建议考虑Microsoft的官方Azure TTS服务
4. 保存音频文件时，请确保应用程序有足够的文件系统权限

## 更新历史

- 2026.1.6 - 修复无法播放的问题，将WebSocket—Sharp改为ClientWebSocket
- 2023.10.28 - 首次发布
- 2023.10.30 - 更新调用接口方式

## 贡献指南

欢迎提交问题和改进建议。如果您希望为项目做出贡献，请遵循以下步骤：

1. Fork 本仓库
2. 创建您的功能分支
3. 提交您的更改
4. 推送到分支
5. 创建新的 Pull Request

## 许可证

[MIT License](https://github.com/Entity-Now/Edge_tts_sharp/blob/main/LICENSE)

## 联系方式

如有任何问题或建议，请通过[GitHub Issues](https://github.com/Entity-Now/Edge_tts_sharp/issues)联系我们。

友情链接 [莫欺客鞋帽优选](https://www.moqistar.com)
