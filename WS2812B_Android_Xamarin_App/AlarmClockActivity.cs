using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Android;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xamarin.Essentials;

namespace WS2812B_Android_Xamarin_App
{
    [Activity(Label = "AlarmClockActivity")]
    public class AlarmClockActivity : Activity
    {
        private Button StartClockButton;
        private TextView SleepTextView;
        private TimePicker PickTime;
        private PlotView LoudnessGraph;
        private Button StopClockButton;

        private static Intent ControllerServiceIntent;
        private static AlarmClockActivity Instance;
        private static PlotModel Model { get; set; }
        public static LineSeries Series { get; set; }
        private static List<double> Points { get; set; }

        public static void AddPoint(double loudness, long start)
        {
            Points.Add(loudness);

            int movingAveragePeriod = Preferences.Get("MOVING_AVERAGE_PERIOD", 5000);

            // update graph if we have enough points
            if (Points.Count >= movingAveragePeriod)
            {
                // calculating moving average
                double sumMA = 0;

                int indexInPast = Points.Count - movingAveragePeriod;
                for (int i = Points.Count - 1; i >= indexInPast; i--)
                    sumMA += Points[i];
            
                double MA = sumMA / movingAveragePeriod;

                var point = new DataPoint(((DateTime.Now.Ticks - start) / 10000000) - movingAveragePeriod, MA);

                Series.Points.Add(point);
                //Model.InvalidatePlot(true);
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Preferences.Set("MOVING_AVERAGE_PERIOD", 100);

            Instance = this;

            SetContentView(Resource.Layout.activity_alarm_clock);

            StartClockButton = FindViewById<Button>(Resource.Id.StartClockButton);
            SleepTextView = FindViewById<TextView>(Resource.Id.SleepTextView);
            PickTime = FindViewById<TimePicker>(Resource.Id.PickTime);
            // LoudnessGraph = FindViewById<PlotView>(Resource.Id.LoudnessGraph);
            StopClockButton = FindViewById<Button>(Resource.Id.StopClockButton);

            // try to gain access to stored data points
            List<DataPoint> temp = new List<DataPoint>();
            if(Series != null)
                temp = Series.Points;
            if (Points == null)
                Points = new List<double>();

            // setup graph
            Series = new LineSeries
            {
                MarkerSize = 0,
             };
            Series.Points.AddRange(temp);

            Model = new PlotModel { Title = "Sleep graph" };
            Model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, IsZoomEnabled=false, IsPanEnabled=false });
            Model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, IsZoomEnabled=false, IsPanEnabled=false });
            Model.Series.Add(Series);
            LoudnessGraph.Model = Model;
        
            // set visibility of startClockButton and pickTime
            HandleVisibility();

            // set default time to 6 AM
            PickTime.CurrentHour = (Java.Lang.Integer)6;
            PickTime.CurrentMinute = (Java.Lang.Integer)0;

            StartClockButton.Click += (sender, e) =>
            {
                // only stop the service if it is running
                if (ControllerServiceIntent != null)
                {
                    StopService(ControllerServiceIntent);
                    ControllerServiceIntent = null;
                    Series.Points.Clear();
                }

                // save current time for alarm clock
                var now = DateTime.Now;
                var alarm = new DateTime(now.Year, now.Month, now.Day, (int)PickTime.CurrentHour, (int)PickTime.CurrentMinute, 0);

                // check if we need to add 1 day
                if (now.Hour > (int)PickTime.CurrentHour)
                    alarm = alarm.AddDays(1);
                else if (now.Hour == (int)PickTime.CurrentHour && now.Minute > (int)PickTime.CurrentMinute)
                    alarm = alarm.AddDays(1);

                var difference = alarm - now;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Toast.MakeText(this, string.Format("Time to sleep: {0} hours {1} minutes.", difference.Hours, difference.Minutes), ToastLength.Long).Show();
                });

                Preferences.Set("wakeUpAt", alarm);

                HandleVisibility();

                // check microphone permission
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.RecordAudio) != (int)Permission.Granted)
                {
                    RequestPermissions(new string[] { Manifest.Permission.RecordAudio }, 0);
                }

                ControllerServiceIntent = new Intent(this, typeof(AlarmControllerService));
                StartForegroundService(ControllerServiceIntent);
            };
             
            StopClockButton.Click += async (sender, e) =>
            {
                // only stop the service if it is running
                if (ControllerServiceIntent != null)
                {
                    StopService(ControllerServiceIntent);
                    ControllerServiceIntent = null;
                }

                // only for developing purposes - to play around with data in external application
                // await LedAPI.Log(Points);

                LoudnessGraph.Visibility = ViewStates.Visible;

                Preferences.Remove("wakeUpAt");
                HandleVisibility();
                //Series.Points.Clear();
                //Points.Clear();
            };
        }

        private void HandleVisibility()
        {
            if (Preferences.ContainsKey("wakeUpAt"))
            {
                StartClockButton.Visibility = ViewStates.Gone;
                PickTime.Visibility = ViewStates.Gone;

                SleepTextView.Visibility = ViewStates.Visible;
                LoudnessGraph.Visibility = ViewStates.Gone;
                StopClockButton.Visibility = ViewStates.Visible;
            }
            else
            {
                StartClockButton.Visibility = ViewStates.Visible;
                PickTime.Visibility = ViewStates.Visible;

                SleepTextView.Visibility = ViewStates.Gone;
                StopClockButton.Visibility = ViewStates.Gone;

                // check if we should display loudness graph
                if(Points.Count >= Preferences.Get("MOVING_AVERAGE_PERIOD", 5000))
                    LoudnessGraph.Visibility = ViewStates.Visible;
            }
        }
    }
}