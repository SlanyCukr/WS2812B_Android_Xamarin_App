using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Xamarin.Essentials;
using Android.Content;

namespace WS2812B_Android_Xamarin_App
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            // set brightness of led to stored value
            LedAPI.SetBrightness(Preferences.Get("ledBrightness", 64));

            Button controlCenterButton = FindViewById<Button>(Resource.Id.ControlCenterButton);
            Button alarmClockButton = FindViewById<Button>(Resource.Id.AlarmClockButton);
            Button settingsButton = FindViewById<Button>(Resource.Id.SettingsButton);

            // create new activities from menu
            controlCenterButton.Click += (sender, e) =>
            {
                StartActivity(typeof(ControlCenterActivity));
            };
            alarmClockButton.Click += (sender, e) =>
            {
                StartActivity(typeof(AlarmClockActivity));
            };
            settingsButton.Click += (sender, e) =>
            {
                StartActivity(typeof(SettingsActivity));
            };

        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}