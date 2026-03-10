using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngleSharp.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Items.BLL;
using Items.DAL;
using Items.Model;
using Microsoft.Data.Sqlite;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ScottPlot;
using ScottPlot.Avalonia;
using testgraph.ViewModels;

namespace testgraph.Views;

public partial class MainWindow : Window
{
    private readonly IService _service;
    public MainWindow()
    {
        var connectionString = File.ReadAllText("db.config");
        _service = new Service(new SqliteDb(connectionString));
        InitializeComponent();
        CreateOrUpdateGraphics(connectionString);
    }

    public void CreateOrUpdateGraphics(string connectionString)
    {
        string sql = @"SELECT * FROM ItemDatePrice";
        var db = new SqliteConnection(connectionString);
        db.Open();
        var command = new SqliteCommand(sql, db);
        var reader = command.ExecuteReader();
        if (!reader.HasRows)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Ошибка", $"Таблица данных для графиков пуста", ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
        }

        var itemsdatesprices = new List<ItemDatePrice.Model.ItemDatePrice>();
        while (reader.Read())
        {
            var item = new ItemDatePrice.Model.ItemDatePrice()
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Data = reader.GetDateTime(2),
                Price = reader.GetString(3),
            };
            itemsdatesprices.Add(item);
        }

        db.Close();
        var items = _service.GetAllItems();
        foreach (var item in items)
        {
            List<DateTime> dates = new List<DateTime>();
            List<double> prices = new List<double>();
            foreach (var x in itemsdatesprices)
            {
                if (x.Name == item.ItemName)
                {
                    dates.Add(x.Data);
                    prices.Add(Convert.ToDouble(x.Price.Replace(" ", "").Replace("р", "").Replace("у", "")
                        .Replace("б", "").Replace(".", "")));
                }
            }

            var AvaPlot = new AvaPlot();
            var scater = AvaPlot.Plot.Add.Scatter(dates, prices);
            AvaPlot.Plot.Axes.AutoScale();
            double[] uniqueYValues = prices.Distinct().OrderBy(y => y).ToArray();
            AvaPlot.Plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                positions: uniqueYValues,
                labels: uniqueYValues.Select(y => y.ToString("F1")).ToArray());
            ////////////////////////////////////////////////////////////////////////////////////////
            ///                              НАСТРОЙКА ВИЗУАЛА                                  ///
            ///////////////////////////////////////////////////////////////////////////////////////
            PixelPadding padding = new(50, 10, 50, 10);
            AvaPlot.Plot.Layout.Fixed(padding);
            AvaPlot.Plot.FigureBackground.Color = Color.FromHex("#212f3c"); //Цвет краев
            AvaPlot.Plot.DataBackground.Color = Color.FromHex("#004da6"); //Цвет таблицы
            AvaPlot.Plot.Axes.Left.Label.ForeColor = Color.FromHex("#dfebea");
            AvaPlot.Plot.ScaleFactor = 1;
            AvaPlot.Plot.Axes.DateTimeTicksBottom(); //Правильное отображение времени
            scater.Smooth = false; //Гладкие линии
            scater.Color = Color.FromHex("#00FFFB"); //Цвет линии 
            scater.LineWidth = 2; //Толщина линии
            var marker = AvaPlot.Plot.Add.Markers(dates, prices); //Добавление маркеров
            marker.Color = Color.FromHex("#5DADE2"); //Цвета маркеров
            marker.MarkerSize = 4; // Размер маркеров
            AvaPlot.Plot.Axes.Color(new("#fafafa")); //Цвет текста

            ////////////////////////////////////////////////////////////////////////////////////////
            ///                              НАСТРОЙКА ЦВЕТОВ                                   ///
            ///////////////////////////////////////////////////////////////////////////////////////

            AvaPlot.Plot.Grid.XAxisStyle.FillColor1 = new Color("#888888").WithAlpha(10);
            AvaPlot.Plot.Grid.YAxisStyle.FillColor1 = new Color("#888888").WithAlpha(10);
            AvaPlot.Plot.Grid.XAxisStyle.MajorLineStyle.Color = Colors.White.WithAlpha(15);
            AvaPlot.Plot.Grid.YAxisStyle.MajorLineStyle.Color = Colors.White.WithAlpha(15);
            AvaPlot.Plot.Grid.XAxisStyle.MinorLineStyle.Color = Colors.White.WithAlpha(5);
            AvaPlot.Plot.Grid.YAxisStyle.MinorLineStyle.Color = Colors.White.WithAlpha(5);
            AvaPlot.Plot.Grid.XAxisStyle.MinorLineStyle.Width = 1;
            AvaPlot.Plot.Grid.YAxisStyle.MinorLineStyle.Width = 1;
            AvaPlot.UserInputProcessor.Disable();
            AvaPlot.Refresh();
            
            string projectDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string graphicsDir = Path.Combine(projectDir, "PlotGraphics");
            string filePath = Path.Combine(graphicsDir, $"{item.ItemName.Replace(" ","").Replace("|","").Replace(":","").Replace("(","").Replace(")","")}.png");
            AvaPlot.Plot.SavePng(filePath, 1200, 300);
        }
    }

    private async void Add_OnClick(object? sender, RoutedEventArgs e)
    {
        
        if (InputName.Text != null &&
            InputCount.Text != null && InputCount.Text.ToDouble()>0 &&
            InputPrice.Text != null && InputPrice.Text.ToDouble()>0 &&
            ChooseGame.SelectedItem != null)
        {
            Item item = new Item();
            LoadingBar.IsVisible = true;
            if (InputPrice.Text.Contains(","))
            {
                InputPrice.Text.Replace(",", ".");
            }

            try
            {
                await item.TotalParse(InputName.Text, InputCount.Text, InputPrice.Text,
                    (ChooseGame.SelectedItem as ComboBoxItem)?.Content.ToString());

            }
            catch(Exception ex)
            {
                LoadingBar.IsVisible = false;
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Ошибка", $"Произошла ошибка добавления предмета.\nВозможные причины : \n-Плохое интернет-соединение \n-Ошибочный ввод данных\nКод ошибки: {ex}",
                        ButtonEnum.Ok,MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowAsPopupAsync(this);
            }
            if (item.SteamPrice != null)
            {
                _service.AddItem(item);
                ((MainWindowViewModel)DataContext).AddToExpanders(item);
            }
            ((MainWindowViewModel)DataContext).Load();
        }
        var connectionString = File.ReadAllText("db.config");
        CreateOrUpdateGraphics(connectionString);
        LoadingBar.IsVisible = false;
        Clear();
    }

    public void Clear()
    {
        InputName.Text=null;
        InputCount.Text = null;
        InputPrice.Text = null;
        ChooseGame.SelectedItem = null;
    }

    private void ButtonDelete_OnClick(object? sender, RoutedEventArgs e)
    {
        ((MainWindowViewModel)DataContext).DeleteAsync();
        var connectionString = File.ReadAllText("db.config");
        CreateOrUpdateGraphics(connectionString);
    }

    private void ButtonReload_OnClick(object? sender, RoutedEventArgs e)
    {
        RefreshProgress.IsVisible = true;
        ((MainWindowViewModel)DataContext).RefreshAsync();
        var connectionString = File.ReadAllText("db.config");
        CreateOrUpdateGraphics(connectionString);
        RefreshProgress.IsVisible = false;
    }

    private void ButtonSell_OnClick(object? sender, RoutedEventArgs e)
    {
        ((MainWindowViewModel)DataContext).SellAsync();
        var connectionString = File.ReadAllText("db.config");
        CreateOrUpdateGraphics(connectionString);
    }

    private void ButtonRefreshAll_OnClick(object? sender, RoutedEventArgs e)
    {
        ((MainWindowViewModel)DataContext).RefreshAllAsync();
        var connectionString = File.ReadAllText("db.config");
        CreateOrUpdateGraphics(connectionString);
    }

    private void ButtonSoldDelete_OnClick(object? sender, RoutedEventArgs e)
    {
        ((MainWindowViewModel)DataContext).NullSoldFor();
    }
}