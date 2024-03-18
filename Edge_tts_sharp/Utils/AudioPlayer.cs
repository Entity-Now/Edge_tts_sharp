using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edge_tts_sharp.Utils
{
    public class AudioPlayer
    {
        private WaveOutEvent waveOut;
        private WaveStream streamReader;
        private bool isPaused;
        private long pausedPosition; // 记录暂停时的位置
        public AudioPlayer(byte[] source, float volume = 1.0f)
        {
            var ms = new MemoryStream(source);
            streamReader = new StreamMediaFoundationReader(ms);
            waveOut = new WaveOutEvent();
            waveOut.Init(streamReader);
            waveOut.Volume = volume;
        }
        public AudioPlayer(string path, float volume = 1.0f)
        {
            streamReader = new AudioFileReader(path);
            waveOut = new WaveOutEvent();
            waveOut.Init(streamReader);
            waveOut.Volume = volume;
        }

        /// <summary>
        /// 播放音频
        /// </summary>
        public void Play()
        {
            if (isPaused)
            {
                // 从暂停的位置继续播放
                streamReader.Position = pausedPosition;
                isPaused = false;
            }
            else
            {
                waveOut.Play();
            }

            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(50);
            }
        }
        /// <summary>
        /// 播放音频
        /// </summary>
        public async Task PlayAsync()
        {
            if (isPaused)
            {
                // 从暂停的位置继续播放
                streamReader.Position = pausedPosition;
                isPaused = false;
            }
            waveOut.Play();

            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                await Task.Delay(50);
            }
        }
        /// <summary>
        /// 暂停播放
        /// </summary>
        public void Pause()
        {
            if (!isPaused)
            {
                waveOut.Pause();
                isPaused = true;
                // 记录暂停时的位置
                pausedPosition = streamReader.Position;
            }
        }
        /// <summary>
        /// 重新播放
        /// </summary>
        public void Resume()
        {
            Stop();
            Play();

        }
        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            waveOut.Stop();
            pausedPosition = 0;
            isPaused = false;
        }
    }
}
