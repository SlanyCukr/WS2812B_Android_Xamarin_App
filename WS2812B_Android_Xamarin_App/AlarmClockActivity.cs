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

namespace WS2812B_Android_Xamarin_App
{
    [Activity(Label = "AlarmClockActivity")]
    public class AlarmClockActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_alarm_clock);
            // Create your application here
            Button startClockButton = FindViewById<Button>(Resource.Id.StartClockButton);

            startClockButton.Click += (sender, e) =>
            {
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