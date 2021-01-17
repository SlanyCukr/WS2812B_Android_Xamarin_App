using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OxyPlot.Xamarin.Android;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WS2812B_Android_Xamarin_App
{
    [Activity(Label = "LoudnessGraphActivity")]
    public class LoudnessGraphActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_graph);

            PlotView LoudnessGraph = FindViewById<PlotView>(Resource.Id.LoudnessGraph);

            GraphDataHolder.Instance.AlterGraph(LoudnessGraph);
        }
    }
}