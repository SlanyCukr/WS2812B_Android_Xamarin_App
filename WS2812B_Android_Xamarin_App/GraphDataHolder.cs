using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Android;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Essentials;

namespace WS2812B_Android_Xamarin_App
{
    public class GraphDataHolder
    {
        private List<double> Points { get; set; }
        private List<DataPoint> DataPoints { get; set; }
        private GraphDataHolder() 
        {
            Points = new List<double>();
            DataPoints = new List<DataPoint>();
        }

        private static GraphDataHolder instance;

        public static GraphDataHolder Instance
        {
            get
            {
                if (instance == null)
                    instance = new GraphDataHolder();

                return instance;
            }
        }

        public void AddPoint(double point)
        {
            Points.Add(point);
        }

        public void AlterGraph(PlotView plotView)
        {
            LineSeries series = new LineSeries
            {
                MarkerSize = 0,
            };
            series.Points.AddRange(DataPoints);

            PlotModel model = new PlotModel { Title = "Sleep graph" };
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, IsZoomEnabled = false, IsPanEnabled = false });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, IsZoomEnabled = false, IsPanEnabled = false });
            model.Series.Add(series);
            plotView.Model = model;
        }

        public void AddPoint(double loudness, long start)
        {
            Points.Add(loudness);

            int movingAveragePeriod = Preferences.Get("MOVING_AVERAGE_PERIOD", 5000);

            // only calculate moving average if we have enough elements
            if (Points.Count >= movingAveragePeriod)
            {
                // calculating moving average
                double sumMA = 0;

                int indexInPast = Points.Count - movingAveragePeriod;
                for (int i = Points.Count - 1; i >= indexInPast; i--)
                    sumMA += Points[i];

                double MA = sumMA / movingAveragePeriod;

                var point = new DataPoint(((DateTime.Now.Ticks - start) / 10000000) - movingAveragePeriod, MA);
                DataPoints.Add(point);
            }
        }

        public int GetPointsCount()
        {
            return Points.Count;
        }

        public double[] GetPointsAsArr()
        {
            return Points.ToArray();
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<double> GetPoints()
        {
            return Points.AsReadOnly();
        }

        public void ResetData()
        {
            Points.Clear();
            DataPoints.Clear();
        }
    }
}