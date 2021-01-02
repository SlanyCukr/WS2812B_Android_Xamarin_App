using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using Xamarin.Essentials;

namespace WS2812B_Android_Xamarin_App
{
    [Activity(Label = "SettingsActivity")]
    public class SettingsActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_settings);

            // Create your application here
            Button findServerButton = FindViewById<Button>(Resource.Id.FindServerButton);
            EditText serverIPAddress = FindViewById<EditText>(Resource.Id.ServerIPAddress);

            // find saved serverIPAddress in settings
            if (Preferences.ContainsKey("serverIPAddress"))
            {
                serverIPAddress.Text = Preferences.Get("serverIPAddress", "");
            }
           
            findServerButton.Click += async (sender, e) =>
            {
                serverIPAddress.SetTextColor(Android.Graphics.Color.Red);

                var myIPAddress = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault();

                if (myIPAddress != null)
                {
                    var client = new HttpClient();
                    client.Timeout = TimeSpan.FromMilliseconds(200);

                    string strIP = myIPAddress.ToString();
                    for(int i = 1; i < 255; i++)
                    {
                        var splitted = strIP.Split('.');
                        try
                        {
                            string searchedIP = string.Format("{0}.{1}.{2}.{3}", splitted[0], splitted[1], splitted[2], i);
                            serverIPAddress.Text = searchedIP;
                            var response = await client.GetAsync("http://" + searchedIP + ":5000/hello");

                            // found the server
                            if (response.Content.ReadAsStringAsync().Result == "Hello")
                            {
                                serverIPAddress.SetTextColor(Android.Graphics.Color.DarkGreen);

                                Preferences.Set("serverIPAddress", searchedIP);
                                break;
                            }
                        }
                        catch(Exception)
                        {
                        }
                    }
                }
            };
        }
    }
}