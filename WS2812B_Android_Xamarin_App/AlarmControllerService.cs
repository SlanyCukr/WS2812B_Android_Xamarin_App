﻿using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace WS2812B_Android_Xamarin_App
{

    [Service]
    class AlarmControllerService : Service
    {
        private List<VolumeRecord> volumeRecordList;
        private short[] audioBuffer = null;
        private AudioRecord audioRecord = null;
        
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Toast.MakeText(this, "Alarm is set!", ToastLength.Long).Show();

            volumeRecordList = new List<VolumeRecord>();
            var bufferSize = AudioRecord.GetMinBufferSize(8000, ChannelIn.Mono, Android.Media.Encoding.Pcm16bit);
            audioBuffer = new short[bufferSize];
            audioRecord = new AudioRecord(
                        // Hardware source of recording.
                        AudioSource.Mic,
                        // Frequency
                        8000,
                        // Mono or stereo
                        ChannelIn.Mono,
                        // Audio encoding
                        Android.Media.Encoding.Pcm16bit,
                        // Length of the audio clip.
                        audioBuffer.Length
                        );
                        audioRecord.StartRecording();
            
            MainLoop();

            return StartCommandResult.Sticky;
        }

        public void MainLoop()
        {
            Thread thread1 = new Thread(() =>
            {
                while (true)
                {
                    int numBytes = audioRecord.Read(audioBuffer, 0, audioBuffer.Length);

                    double sum = 0;
                    for (int i = 0; i < numBytes; i++)
                    {
                        sum += Math.Abs(audioBuffer[i]);
                    }
                    var level = sum / numBytes;
                    var db = 20.0 * Math.Log10(level / 32767.0) + 90;

                    volumeRecordList.Add(new VolumeRecord(db));

                    Thread.Sleep(1000);
                }
            });
            thread1.Start();

            Thread thread2 = new Thread(() =>
            {
                while (true)
                {
                    // check if on average user is in REM sleep and the time is right to wake him up
                    if (volumeRecordList.Count >= 50)
                    {
                        double sum = 0;
                        for (int i = volumeRecordList.Count - 50; i < volumeRecordList.Count; i++)
                            sum += volumeRecordList[i].Loudness;

                        double avg = sum / 50;

                        // 55.8 is just arbitrary number, needs to be verified; TODO - check the datetime
                        if(avg >= 55.8)
                        {

                        }
                    }

                    Thread.Sleep(50000);
                }
            });
            thread2.Start();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Toast.MakeText(this, "Alarm was destroyed.", ToastLength.Long).Show();
        }
    }
}