using System;
using System.Collections.Generic;
using System.Text;

namespace Edge_tts_sharp.Model
{
    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum level
    {
        /// <summary>
        /// 信息级别日志
        /// </summary>
        info,
        /// <summary>
        /// 警告级别日志
        /// </summary>
        warning,
        /// <summary>
        /// 错误级别日志
        /// </summary>
        error
    }
    /// <summary>
    /// 日志类，包含日志消息和日志级别
    /// </summary>
    public class Log
    {
        /// <summary>
        /// 日志消息内容
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 日志级别
        /// </summary>
        public level level { get; set; }
    }
}
