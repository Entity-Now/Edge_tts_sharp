using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Edge_tts_sharp.Utils
{
    public static class Audio
    {
        public static async Task PlayToStreamAsync(Stream source, float volume = 1.0f)
        {
            var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat());
            using (var waveOut = new WaveOutEvent())
            {
                waveOut.Init(bufferedWaveProvider);
                waveOut.Volume = volume;
                waveOut.Play();

                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    bufferedWaveProvider.AddSamples(buffer, 0, bytesRead);
                }

                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    await Task.Delay(1000);
                }
            }
        }

        public static async Task PlayToByteAsync(byte[] source, float volume = 1.0f)
        {
            using (var ms = new MemoryStream(source))
            {
                await PlayToStreamAsync(ms, volume);
            }
        }

        public static async Task PlayAudioAsync(string audioPath, float volume = 1.0f)
        {
            using (var audioFile = new AudioFileReader(audioPath))
            using (var waveOut = new WaveOutEvent())
            {
                waveOut.Init(audioFile);
                waveOut.Volume = volume;
                waveOut.Play();

                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    await Task.Delay(1000);
                }
            }
        }

        public static async Task PlayAudioFromUrlAsync(string url, float volume = 1.0f)
        {
            using (HttpClient client = new HttpClient())
            using (Stream stream = await client.GetStreamAsync(url))
            {
                await PlayToStreamAsync(stream, volume);
            }
        }

        public static async Task PlayNetworkAudio(this HttpClient client, string url, float volume = 1.0f)
        {
            try
            {
                using (var responseStream = await client.GetStreamAsync(url))
                {
                    var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat());
                    using (var waveOut = new WaveOutEvent())
                    {
                        waveOut.Init(bufferedWaveProvider);
                        waveOut.Volume = volume;
                        waveOut.Play();

                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            bufferedWaveProvider.AddSamples(buffer, 0, bytesRead);
                            // 调整这个延迟时间以确保缓冲区有足够的数据播放
                            await Task.Delay(10);
                        }

                        // 等待播放完成
                        while (waveOut.PlaybackState == PlaybackState.Playing && bufferedWaveProvider.BufferedDuration.TotalMilliseconds > 0)
                        {
                            await Task.Delay(50);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing network audio: {ex.Message}");
            }
        }
        public static async Task PlayNetworkAudio(this HttpClient client, string url, float volume = 1.0f, int bufferSize = 4096, int delay = 10, Action callBack = null)
        {
            try
            {
                using (var responseStream = await client.GetStreamAsync(url))
                {
                    var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat());
                    using (var waveOut = new WaveOutEvent())
                    {
                        waveOut.Init(bufferedWaveProvider);
                        waveOut.Volume = volume;
                        waveOut.Play();

                        byte[] buffer = new byte[bufferSize];
                        int bytesRead;
                        while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            bufferedWaveProvider.AddSamples(buffer, 0, bytesRead);
                            await Task.Delay(delay);
                        }

                        waveOut.PlaybackStopped += (s, e) =>
                        {
                            // 处理播放停止的情况
                            callBack();
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing network audio: {ex.Message}");
            }
        }

    }
}
