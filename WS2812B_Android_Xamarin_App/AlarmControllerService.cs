using Android.App;
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
        private Timer timer;
        private List<double> NumBytesList;
        private short[] audioBuffer = null;
        private AudioRecord audioRecord = null; 

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Toast.MakeText(this, "Alarm is set!", ToastLength.Long).Show();

            NumBytesList = new List<double>();
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
            //timer = new Timer((e) => MainLoop(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            MainLoop();

            return StartCommandResult.Sticky;
        }

        public void MainLoop()
        {
            Thread tread = new Thread(() =>
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

                    NumBytesList.Add(db);

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Toast.MakeText(this, db.ToString(), ToastLength.Long).Show();
                    });

                    Thread.Sleep(1000);
                }
            });
            tread.Start();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Toast.MakeText(this, "Alarm was destroyed.", ToastLength.Long).Show();
        }
    }
}