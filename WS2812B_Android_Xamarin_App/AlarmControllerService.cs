using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OxyPlot;
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
        private long start;

        private Thread Thread1 { get; set; }
        private Thread Thread2 { get; set; }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        private void SetupRecording()
        {
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
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            start = DateTime.Now.Ticks;
            //Toast.MakeText(this, "Alarm is set!", ToastLength.Long).Show();

            try
            {
                SetupRecording();
            }
            catch(Exception)
            {
                Thread.Sleep(5000);
                SetupRecording();
            }
            MainLoop();

            return StartCommandResult.Sticky;
        }

        public void MainLoop()
        {
            Thread1 = new Thread(() =>
            {
                while (true)
                {
                    int numBytes = audioRecord.Read(audioBuffer, 0, audioBuffer.Length);

                    // calculate loudness from audio buffer
                    double sum = 0;
                    for (int i = 0; i < numBytes; i++)
                    {
                        sum += Math.Abs(audioBuffer[i]);
                    }
                    var level = sum / numBytes;
                    var db = 20.0 * Math.Log10(level / 32767.0) + 90;

                    // append loudness to list
                    AlarmClockActivity.AddPoint(new DataPoint(DateTime.Now.Ticks - start, db));

                    // check loundess every second
                    Thread.Sleep(1000);
                }
            });
            Thread1.Start();

            Thread2 = new Thread(() =>
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

                        // idea - establish level of "awake" from start of the sleep
                        // 55.8 is just arbitrary number, needs to be verified; TODO - check the datetime
                        if(avg >= 55.8)
                        {

                        }
                    }

                    // sleep for 50 seconds
                    Thread.Sleep(50000);
                }
            });
            Thread2.Start();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // terminate running threads
            Thread1.Abort();
            Thread2.Abort();
        }
    }
}