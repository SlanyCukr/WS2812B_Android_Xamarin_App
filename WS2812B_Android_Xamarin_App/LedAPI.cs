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
        private static readonly HttpClient Client = new HttpClient();
        private static readonly HttpClient HelloClient = new HttpClient { Timeout=TimeSpan.FromMilliseconds(400) };

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
            var encodedValues = new FormUrlEncodedContent(new Dictionary<string, string> { { "brightness", brightness.ToString() } });
            return await Client.PostAsync(string.Format("http://{0}:5000/set_brightness", Preferences.Get("serverIPAddress", "192.168.0.114")), encodedValues);
        }
        public async static Task<HttpResponseMessage> Log(List<double> valuesList)
        {
            string text = "";
            foreach (var l in valuesList)
                text += l.ToString() + '\n';

            var encodedValues = new FormUrlEncodedContent(new Dictionary<string, string> { { "value", text } });
            return await Client.PostAsync(string.Format("http://{0}:5000/log", Preferences.Get("serverIPAddress", "192.168.0.114")), encodedValues);
        }

        public async static Task<HttpResponseMessage> Hello(string ip)
        {
            return await HelloClient.GetAsync("http://" + ip + ":5000/hello");
        }
    }
}