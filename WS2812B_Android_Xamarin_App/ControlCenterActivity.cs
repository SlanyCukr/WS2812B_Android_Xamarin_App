using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Xamarin.Essentials;

namespace WS2812B_Android_Xamarin_App
{
    [Activity(Label = "Test", Theme = "@style/AppTheme")]
    public class ControlCenterActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_control_center);

            // Create your application here
            Button turnOnButton = FindViewById<Button>(Resource.Id.TurnOnButton);
            Button turnOffButton = FindViewById<Button>(Resource.Id.TurnOffButton);
            Button rainbowButton = FindViewById<Button>(Resource.Id.RainbowButton);

            turnOnButton.Click += async (sender, e) =>
            {
                var client = new HttpClient();
                await client.GetAsync(string.Format("http://{0}:5000/turn_on", Preferences.Get("serverIPAddress", "192.168.0.114")));
            };
            turnOffButton.Click += async (sender, e) =>
            {
                var client = new HttpClient();
                await client.GetAsync(string.Format("http://{0}:5000/turn_off", Preferences.Get("serverIPAddress", "192.168.0.114")));
            };
            rainbowButton.Click += (sender, e) =>
            {
                var client = new HttpClient();
                client.GetAsync(string.Format("http://{0}:5000/rainbow", Preferences.Get("serverIPAddress", "192.168.0.114")));
            };
        }
    }
}