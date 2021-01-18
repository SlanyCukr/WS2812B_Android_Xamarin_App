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
    public static class Utils
    {
        public static IEnumerable<List<T>> SplitList<T>(System.Collections.ObjectModel.ReadOnlyCollection<T> locations, int nSize = 30)
        {
            var list_locations = locations.ToList();

            for (int i = 0; i < list_locations.Count; i += nSize)
            {
                yield return list_locations.GetRange(i, Math.Min(nSize, list_locations.Count - i));
            }
        }
    }
}