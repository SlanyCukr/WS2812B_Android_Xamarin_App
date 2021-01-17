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
        private Button StopClockButton;

        private static Intent ControllerServiceIntent;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Preferences.Set("MOVING_AVERAGE_PERIOD", 100);

            SetContentView(Resource.Layout.activity_alarm_clock);

            StartClockButton = FindViewById<Button>(Resource.Id.StartClockButton);
            SleepTextView = FindViewById<TextView>(Resource.Id.SleepTextView);
            PickTime = FindViewById<TimePicker>(Resource.Id.PickTime);
            StopClockButton = FindViewById<Button>(Resource.Id.StopClockButton);

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

                // Go to LoudnessGraph activity
                StartActivity(typeof(LoudnessGraphActivity));

                // only for developing purposes - to play around with data in external application
                // await LedAPI.Log(Points);

                Preferences.Remove("wakeUpAt");
                HandleVisibility();
            };
        }

        private void HandleVisibility()
        {
            if (Preferences.ContainsKey("wakeUpAt"))
            {
                StartClockButton.Visibility = ViewStates.Gone;
                PickTime.Visibility = ViewStates.Gone;

                SleepTextView.Visibility = ViewStates.Visible;
                StopClockButton.Visibility = ViewStates.Visible;
            }
            else
            {
                StartClockButton.Visibility = ViewStates.Visible;
                PickTime.Visibility = ViewStates.Visible;

                SleepTextView.Visibility = ViewStates.Gone;
                StopClockButton.Visibility = ViewStates.Gone;
            }
        }
    }
}