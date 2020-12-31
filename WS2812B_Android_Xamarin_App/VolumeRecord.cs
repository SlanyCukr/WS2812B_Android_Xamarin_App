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

namespace WS2812B_Android_Xamarin_App
{
    class VolumeRecord
    {
        public double Loudness { get; set; }
        public DateTime Time { get; set; }

        public VolumeRecord(double loudness)
        {
            Loudness = loudness;
            Time = DateTime.Now;
        }
    }
}