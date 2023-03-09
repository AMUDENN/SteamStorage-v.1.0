using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using SteamStorage.ApplicationLogic;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using SteamStorage.Pages;
using OxyPlot.Wpf;

namespace SteamStorage.ControlElements
{
    public partial class RemainElementUC : UserControl
    {
        public PlotModel MyModel { get; private set; }
        public AdvancedRemain RemainElementFull { get; set; }
        public RemainElementUC(AdvancedRemain remainElementFull)
        {
            InitializeComponent();
            DataContext = this;
            RemainElementFull = remainElementFull;
            MyModel = GetPlotModel(RemainElementFull.UpdateCosts);
        }
        public PlotModel GetPlotModel(Dictionary<DateTime, double> UpdateCosts)
        {
            PlotModel ResultModel = new()
            {
                PlotMargins = new OxyThickness(-3, -3, -3, -3),
                PlotAreaBorderThickness = new OxyThickness(0)
            };

            LineSeries line = new()
            {
                Color = RemainElementFull.PercentForeground.ToOxyColor(),
                StrokeThickness = 1.5,
                CanTrackerInterpolatePoints = false
            };
            int i = 0;
            foreach (var item in UpdateCosts)
            {
                line.Points.Add(new DataPoint(i, item.Value));
                i++;
            }
            ResultModel.Series.Add(line);


            var XAxis = new CategoryAxis()
            {
                Minimum = 0,
                Maximum = i * 1.1,
                IsAxisVisible = false
            };

            double minimum = UpdateCosts.Min(x => x.Value) / 1.1;
            if (minimum < 1) minimum = 0;
            double maximum = UpdateCosts.Max(x => x.Value) * 1.1;
            if (maximum < 1) maximum = 1;

            var YAxis = new LinearAxis()
            {
                Minimum = minimum,
                Maximum = maximum,
                IsAxisVisible = false
            };

            ResultModel.Axes.Add(XAxis);
            ResultModel.Axes.Add(YAxis);

            return ResultModel;
        }
        private void ChangeClick(object sender, RoutedEventArgs e)
        {
            var childrens = MainWindow.RemainsPageInstance.ElementsStackPanel.Children;
            int index = childrens.IndexOf(this);
            childrens.Remove(this);
            childrens.Insert(index, new ChangeRemainElement(RemainElementFull));
        }
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            if (!Messages.ActionConfirmation($"Вы уверены, что хотите удалить скин {RemainElementFull.Title}?")) return;
            RemainsMethods.DeleteRemainElement(RemainElementFull);
            MainWindow.RemainsPageInstance.RefreshElements();
        }
        private void SellClick(object sender, RoutedEventArgs e)
        {
            var childrens = MainWindow.RemainsPageInstance.ElementsStackPanel.Children;
            int index = childrens.IndexOf(this);
            childrens.Remove(this);
            childrens.Insert(index, new SellRemainElement(RemainElementFull));
        }
    }
}
