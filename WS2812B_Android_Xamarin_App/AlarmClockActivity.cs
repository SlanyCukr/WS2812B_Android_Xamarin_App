using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xamarin.Essentials;

namespace WS2812B_Android_Xamarin_App
{
    [Activity(Label = "AlarmClockActivity")]
    public class AlarmClockActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_alarm_clock);
            
            Button startClockButton = FindViewById<Button>(Resource.Id.StartClockButton);
            TimePicker pickTime = FindViewById<TimePicker>(Resource.Id.PickTime);

            // set default time to 6 AM
            pickTime.CurrentHour = (Java.Lang.Integer)6;
            pickTime.CurrentMinute = (Java.Lang.Integer)0;

            startClockButton.Click += (sender, e) =>
            {
                // save current time for alarm clock
                var now = DateTime.Now;
                var alarm = new DateTime(now.Year, now.Month, now.Day, (int)pickTime.CurrentHour, (int)pickTime.CurrentMinute, 0);

                // check if we dont need to add 1 day, because the time still could be in this day, or if it must be tomorrow and we need to add 1 day
                if (now.Hour > (int)pickTime.CurrentHour)
                    alarm = alarm.AddDays(1);
                else if (now.Hour == (int)pickTime.CurrentHour && now.Minute > (int)pickTime.CurrentMinute)
                    alarm = alarm.AddDays(1);

                var difference = alarm - now;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Toast.MakeText(this, string.Format("Time to sleep: {0} hours {1} minutes.", difference.Hours, difference.Minutes), ToastLength.Long).Show();
                });

                Preferences.Set("wakeUpAt", alarm);

                // check microphone permission
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.RecordAudio) != (int)Permission.Granted)
                {
                    RequestPermissions(new string[] { Manifest.Permission.RecordAudio }, 0);
                }

                Intent intent = new Intent(this, typeof(AlarmControllerService));
                StartService(intent);
            };
        }
    }
}