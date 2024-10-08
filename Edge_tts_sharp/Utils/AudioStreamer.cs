using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Edge_tts_sharp.Utils
{
    public class Mp3AudioStreamer
    {
        private BufferedWaveProvider _bufferedWaveProvider;
        private WaveOutEvent _waveOut;

        public Mp3AudioStreamer()
        {
            // 设定音频格式，确保与解码后的PCM数据格式一致
            _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(44100, 16, 2));
            _bufferedWaveProvider.BufferLength = 1024 * 1024; // 设置1MB缓冲区
            _bufferedWaveProvider.DiscardOnBufferOverflow = true; // 避免缓冲区溢出

            _waveOut = new WaveOutEvent
            {
                DesiredLatency = 100 // 减少播放延迟
            };
            _waveOut.Init(_bufferedWaveProvider);
            _waveOut.Play();
        }

        // 处理WebSocket的音频数据
        public void OnAudioReceived(byte[] mp3Data)
        {
            // 将 MP3 数据写入临时文件
            string tempFilePath = Path.GetTempFileName() + ".mp3";
            File.WriteAllBytes(tempFilePath, mp3Data);

            // 使用 MediaFoundationReader 解码临时文件
            using (var reader = new MediaFoundationReader(tempFilePath))
            {
                var buffer = new byte[16384]; // 16KB 缓冲区
                int bytesRead;

                while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    _bufferedWaveProvider.AddSamples(buffer, 0, bytesRead);
                }
            }

            // 删除临时文件
            File.Delete(tempFilePath);
        }



        public void Stop()
        {
            _waveOut.Stop();
        }
    }


}
