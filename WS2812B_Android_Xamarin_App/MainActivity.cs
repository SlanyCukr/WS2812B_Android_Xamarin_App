using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System.Net.Http;
using Xamarin.Essentials;

namespace WS2812B_Android_Xamarin_App
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            Button turnOnButton = FindViewById<Button>(Resource.Id.TurnOnButton);
            Button turnOffButton = FindViewById<Button>(Resource.Id.TurnOffButton);
            Button rainbowButton = FindViewById<Button>(Resource.Id.RainbowButton);

            turnOnButton.Click += async (sender, e) =>
            {
                var client = new HttpClient();
                await client.GetAsync("http://192.168.0.114/turn_on");
            };
            turnOffButton.Click += async (sender, e) =>
            {
                var client = new HttpClient();
                await client.GetAsync("http://192.168.0.114/turn_off");

            };
            rainbowButton.Click += (sender, e) =>
            {
                var client = new HttpClient();
                client.GetAsync("http://192.168.0.114/rainbow");
            };
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}