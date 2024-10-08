using System;
using System.Collections.Generic;
using System.Text;

namespace Edge_tts_sharp.Model
{
    /// <summary>
    /// 播放音频配置参数
    /// </summary>
    public class PlayOption
    {
        /// <summary>
        /// 播放内容
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 语速，是一个-100 - 100的数值
        /// </summary>
        public int Rate { get; set; } = 0;
        /// <summary>
        /// 音量，是一个0 - 1的浮点数值
        /// </summary>
        public float Volume { get; set; } = 1.0f;
        /// <summary>
        /// 音频保存地址
        /// </summary>
        public string SavePath { get; set; } = string.Empty;
    }
}
