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
        private short[] audioBuffer = null;
        private AudioRecord audioRecord = null;
        private long start;
        private double awakeLoudness;

        private Thread Thread1 { get; set; }
        private Thread Thread2 { get; set; }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        private void SetupRecording()
        {
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
            var alarm = Preferences.Get("wakeUpAt", new DateTime());

            var notification = new Notification.Builder(this)
            .SetContentTitle(Resources.GetString(Resource.String.app_name))
            .SetContentText(string.Format("Alarm will go off at: {0}.", alarm.ToString("HH:mm")))
            .SetSmallIcon(Resource.Drawable.abc_list_focused_holo)
            .SetOngoing(true)
            .Build();

            start = DateTime.Now.Ticks;
            awakeLoudness = 10000;
            
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

            StartForeground(10000, notification);
            return StartCommandResult.Sticky;
        }

        public void MainLoop()
        {
            /// TODO -> Create function that will create the threads; or just function that the threads will use

            // read loudness
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

                    // add the point
                    GraphDataHolder.Instance.AddPoint(db, start);

                    // check loudness every second
                    Thread.Sleep(1000);
                }
            });
            Thread1.Start();

            // waking up
            Thread2 = new Thread(() =>
            {
                int movingAveragePeriod = Preferences.Get("MOVING_AVERAGE_PERIOD", 5000);

                while (true)
                {
                    var pointsCount = GraphDataHolder.Instance.GetPointsCount();
                    var pointsAsArr = GraphDataHolder.Instance.GetPointsAsArr();

                    // check if we already calculated awake loudness
                    if (awakeLoudness == 10000)
                    {
                        // check if we have enough information to calcute it
                        if(pointsCount >= movingAveragePeriod)
                        {
                            // calculate it
                            double sum = 0;
                            for (int i = 0; i < movingAveragePeriod; i++)
                                sum += pointsAsArr[i];

                            awakeLoudness = sum / movingAveragePeriod;
                        }
                    }

                    // check if on average user is in REM sleep and the time is right to wake him up
                    if (pointsCount >= movingAveragePeriod)
                    {
                        double sum = 0;
                        for (int i = pointsCount - movingAveragePeriod; i < pointsCount; i++)
                            sum += pointsAsArr[i];

                        double avg = sum / movingAveragePeriod;

                        // if its the right time to be awaken and person is in REM, wake him up
                        if (IsInREM(avg) && IsTimeToWakeUp())
                        {
                            // turn on the LED stripe, stop adding data to graph
                            LedAPI.TurnOn();
                            StopSelf();
                        }
                    }

                    // sleep for 50 seconds
                    Thread.Sleep(50000);
                }
            });
            Thread2.Start();
        }

        /// <summary>
        /// If the loudness difference between "awake" state and most current state is low, we say that the subject is in REM.
        /// </summary>
        /// <param name="avgLoudness">Calculated current average loudness</param>
        /// <returns>Boolean</returns>
        private bool IsInREM(double avgLoudness)
        {
            if (Math.Abs(awakeLoudness - avgLoudness) <= 3)
                return true;
            return false;
        }

        /// <summary>
        /// Calculates if it's time for the subject to be awoken.
        /// </summary>
        /// <returns>Boolean</returns>
        private bool IsTimeToWakeUp()
        {
            if(Math.Abs((DateTime.Now - Preferences.Get("wakeUpAt", new DateTime())).TotalMinutes) <= 30)
                    return true;
            return false;
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