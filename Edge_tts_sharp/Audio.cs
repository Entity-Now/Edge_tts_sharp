using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using NAudio;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Edge_tts_sharp
{
    public class Audio
    {
        public static void PlayToByte(byte[] source)
        {
            using (var ms = new MemoryStream(source))
                using (var sr = new StreamMediaFoundationReader(ms))
                    using (var waveOut = new WaveOutEvent())
                    {
                        waveOut.Init(sr);
                        waveOut.Play();
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(1000);
                        }
                    }
        }
    }
}
