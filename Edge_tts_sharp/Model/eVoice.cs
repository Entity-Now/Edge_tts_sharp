using System;
using System.Collections.Generic;
using System.Text;

namespace Edge_tts_sharp.Model
{
    /// <summary>
    /// 语音对象模型，包含语音的各种属性
    /// </summary>
    public class eVoice
    {
        /// <summary>
        /// 语音的完整名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 语音的简称
        /// </summary>
        public string ShortName { get; set; }
        /// <summary>
        /// 语音的性别
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// 语音的区域设置
        /// </summary>
        public string Locale { get; set; }
        /// <summary>
        /// 建议使用的编解码器
        /// </summary>
        public string SuggestedCodec { get; set; }
        /// <summary>
        /// 语音的友好名称
        /// </summary>
        public string FriendlyName { get; set; }
        /// <summary>
        /// 语音的状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 语音标签，包含内容类别和语音个性
        /// </summary>
        public Voicetag VoiceTag { get; set; }
    }

    /// <summary>
    /// 语音标签类，包含语音的内容类别和个性特征
    /// </summary>
    public class Voicetag
    {
        /// <summary>
        /// 内容类别数组
        /// </summary>
        public string[] ContentCategories { get; set; }
        /// <summary>
        /// 语音个性特征数组
        /// </summary>
        public string[] VoicePersonalities { get; set; }
    }
}
