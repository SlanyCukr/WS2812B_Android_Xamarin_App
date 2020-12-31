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

namespace WS2812B_Android_Xamarin_App
{

    [Service]
    class AlarmControllerService : Service
    {
        private Timer timer;
        private List<double> NumBytesList;
        private Byte[] audioBuffer = null;
        private AudioRecord audioRecord = null; 

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Toast.MakeText(this, "Alarm is set!", ToastLength.Long).Show();

            NumBytesList = new List<double>();
            audioBuffer = new Byte[100000];
            audioRecord = new AudioRecord(
                        // Hardware source of recording.
                        AudioSource.Mic,
                        // Frequency
                        11025,
                        // Mono or stereo
                        ChannelIn.Mono,
                        // Audio encoding
                        Android.Media.Encoding.Pcm16bit,
                        // Length of the audio clip.
                        audioBuffer.Length
                        );
            timer = new Timer((e) => MainLoop(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            return StartCommandResult.Sticky;
        }

        public void MainLoop()
        {
            audioRecord.StartRecording();
            int numBytes = audioRecord.Read(audioBuffer, 0, audioBuffer.Length);
            audioRecord.Stop();
            audioRecord.Release();

            double sum = 0;
            for (int i = 0; i < numBytes; i++)
            {
                sum += audioBuffer[i];
            }
            var level = sum / numBytes;

            NumBytesList.Add(level);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Toast.MakeText(this, "Alarm was destroyed.", ToastLength.Long).Show();
        }
    }
}