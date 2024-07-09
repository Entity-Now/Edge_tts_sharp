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
using NAudio.Wave.SampleProviders;

namespace Edge_tts_sharp.Utils
{
    public static class Audio
    {
        public static async Task PlayToStreamAsync(Stream source, float volume = 1.0f, float speed = 0.0f, CancellationToken cancellationToken = default)
        {
            using (var sr = new StreamMediaFoundationReader(source))
            {
                var sampleProvider = sr.ToSampleProvider();
                var pitchShiftingProvider = new SmbPitchShiftingSampleProvider(sampleProvider);
                using (var directSoundOut = new DirectSoundOut())
                {
                    pitchShiftingProvider.PitchFactor = (float)Math.Pow(2.0, speed / 100.0);
                    var waveProvider = pitchShiftingProvider.ToWaveProvider();
                    directSoundOut.Init(waveProvider);
                    directSoundOut.Volume = volume;
                    directSoundOut.Play();

                    while (directSoundOut.PlaybackState == PlaybackState.Playing)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            directSoundOut.Stop();
                            break;
                        }
                        await Task.Delay(1000, cancellationToken);
                    }
                }
            }
        }

        public static async Task PlayToByteAsync(byte[] source, float volume = 1.0f, float speed = 0.0f, CancellationToken cancellationToken = default)
        {
            using (var ms = new MemoryStream(source))
            {
                ms.Position = 0;
                await PlayToStreamAsync(ms, volume, speed, cancellationToken);
            }
        }

        public static async Task PlayAudioAsync(string audioPath, float volume = 1.0f, float speed = 0.0f, CancellationToken cancellationToken = default)
        {
            using (var audioFile = new AudioFileReader(audioPath))
            {
                var sampleProvider = audioFile.ToSampleProvider();
                var pitchShiftingProvider = new SmbPitchShiftingSampleProvider(sampleProvider);
                using (var directSoundOut = new DirectSoundOut())
                {
                    pitchShiftingProvider.PitchFactor = (float)Math.Pow(2.0, speed / 100.0);
                    var waveProvider = pitchShiftingProvider.ToWaveProvider();
                    directSoundOut.Init(waveProvider);
                    directSoundOut.Volume = volume;
                    directSoundOut.Play();

                    while (directSoundOut.PlaybackState == PlaybackState.Playing)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            directSoundOut.Stop();
                            break;
                        }
                        await Task.Delay(1000, cancellationToken);
                    }
                }
            }
        }

        public static async Task PlayAudioFromUrlAsync(string url, float volume = 1.0f, float speed = 0.0f, CancellationToken cancellationToken = default)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    var stream = await response.Content.ReadAsByteArrayAsync();
                    await PlayToByteAsync(stream, volume, speed, cancellationToken);
                }
            }
        }
    }
}
