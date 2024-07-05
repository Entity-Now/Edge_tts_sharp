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
            using (var sr = new StreamMediaFoundationReader(source))
            using (var directSoundOut = new DirectSoundOut())
            {
                directSoundOut.Init(sr);
                directSoundOut.Volume = volume;
                directSoundOut.Play();

                while (directSoundOut.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(10);
                }
            }
        }

        public static async Task PlayToByteAsync(byte[] source, float volume = 1.0f)
        {
            using (var ms = new MemoryStream(source))
            {
                ms.Position = 0;
                await PlayToStreamAsync(ms, volume);
            }
        }

        public static async Task PlayAudioAsync(string audioPath, float volume = 1.0f)
        {
            using (var audioFile = new AudioFileReader(audioPath))
            using (var directSoundOut = new DirectSoundOut())
            {
                directSoundOut.Init(audioFile);
                directSoundOut.Volume = volume;
                directSoundOut.Play();

                while (directSoundOut.PlaybackState == PlaybackState.Playing)
                {
                    await Task.Delay(500);
                }
            }
        }

        public static async Task PlayAudioFromUrlAsync(string url, float volume = 1.0f)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    var stream = await response.Content.ReadAsByteArrayAsync();
                    await PlayToByteAsync(stream, volume);
                    
                }
            }
        }



        
    }
}
