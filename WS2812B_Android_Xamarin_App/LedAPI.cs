using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace WS2812B_Android_Xamarin_App
{
    static class LedAPI
    {
        private static HttpClient Client = new HttpClient();

        public async static Task<HttpResponseMessage> TurnOn()
        {
            return await Client.PostAsync(string.Format("http://{0}:5000/turn_on", Preferences.Get("serverIPAddress", "192.168.0.114")), null);
        }

        public async static Task<HttpResponseMessage> TurnOff()
        {
            return await Client.PostAsync(string.Format("http://{0}:5000/turn_off", Preferences.Get("serverIPAddress", "192.168.0.114")), null);

        }

        public async static Task<HttpResponseMessage> Rainbow()
        {
            return await Client.PostAsync(string.Format("http://{0}:5000/rainbow", Preferences.Get("serverIPAddress", "192.168.0.114")), null);
        }

        public async static Task<HttpResponseMessage> SetBrightness(int brightness)
        {
            var encodedValues = new FormUrlEncodedContent(new Dictionary<string, string> { { "brightness", brightness.ToString()} });
            return await Client.PostAsync(string.Format("http://{0}:5000/set_brightness", Preferences.Get("serverIPAddress", "192.168.0.114")), encodedValues);
        }
    }
}