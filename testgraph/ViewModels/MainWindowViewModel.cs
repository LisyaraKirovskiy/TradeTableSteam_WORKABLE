using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Items.BLL;
using Items.DAL;
using Items.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace testgraph.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<Item> _expanders { get; set; } = [];
    private readonly IService _service;
    [Reactive] public Item SelectedItem { get; set; }
    [Reactive] public string FullCost { get; set; }
    [Reactive] public string NowCost { get; set; }
    [Reactive] public string Profit { get; set; }
    [Reactive] public string ClearProfit { get; set; }
    public SoldFor SoldFor=new SoldFor();
    [Reactive] public string SoldForString { get; set; }
    private string _SoldForColor;
    public string SoldForColor
    {
        get => this._SoldForColor;
        set
        {
            this.RaiseAndSetIfChanged(ref _SoldForColor, value);
        }
    }
    private string _TextColorUpper;
    public string TextColorUpper
    {
        get => this._TextColorUpper;
        set
        {
            this.RaiseAndSetIfChanged(ref _TextColorUpper, value);
        }
    }
    private bool _isRefreshing;
    public bool IsRefreshing 
    {
        get => _isRefreshing;
        set => this.RaiseAndSetIfChanged(ref _isRefreshing, value);
    }
   
    
public MainWindowViewModel()
    {
        var connectionString = File.ReadAllText("db.config");
        _service = new Service(new SqliteDb(connectionString));
        this.WhenAnyValue(vm => vm.SelectedItem).WhereNotNull().Subscribe(async s =>
        {
            SelectedItem.Id = s.Id;
            SelectedItem.ItemName = s.ItemName;
            SelectedItem.Count = s.Count;
            SelectedItem.Url = s.Url;
            SelectedItem.PurchasePrice = s.PurchasePrice;
            SelectedItem.SteamPrice = s.SteamPrice;
            SelectedItem.Currency = s.Currency;
            SelectedItem.SteamPriceWithFee = s.SteamPriceWithFee;
            SelectedItem.StatPrice = s.StatPrice;
            SelectedItem.StatPriceWithFee = s.StatPriceWithFee;
            SelectedItem.FullStatPrice = s.FullStatPrice;
            SelectedItem.FullStatPriceWithFee = s.FullStatPriceWithFee;
            SelectedItem.FullStatPricePercent = s.FullStatPricePercent;
            SelectedItem.Image = s.Image;
            SelectedItem.Spent = s.Spent;
            SelectedItem.Game = s.Game;
        });
        Load();
    }

    public void Load()
    {
        var items = _service.GetAllItems();
        if (items == null)
        {
            _expanders.Clear();
            FullCost = $"0 руб.";
            NowCost = $"0 руб.";
            Profit = $"0 руб.";
            ClearProfit = $"0 руб.";
            SoldFor.GetSoldFor();
            if (SoldFor.SldFor >= 0) SoldForColor = "#2cbf08";
            else SoldForColor = "#bf1408";
            SoldForString = $"{SoldFor.SldFor} руб.";
            TextColorUpper = "#424d4f";
            return;
        }
        _expanders.Clear();
        double spent = 0;
        double nowcost = 0;
        foreach (var item in items)
        {
            _expanders.Add(item);
            spent += item.PriceToDouble(item.Spent);
            nowcost += (item.PriceToDouble(item.SteamPrice))*(item.PriceToDouble(item.Count));
        }
        FullCost = $"{Math.Round(spent,2)} руб.";
        NowCost = $"{Math.Round(nowcost,2)} руб.";
        Profit = $"{Math.Round(nowcost - spent,2)} руб.";
        double cl = Math.Round(nowcost * 0.85 - spent, 2);
        ClearProfit = $"{Math.Round(nowcost*0.85-spent,2)} руб.";
        if (cl>0)TextColorUpper = "#2cbf08";
        else TextColorUpper = "#bf1408";
        SoldFor.GetSoldFor();
        if (SoldFor.SldFor >= 0) SoldForColor = "#2cbf08";
        else SoldForColor = "#bf1408";
        SoldForString = $"{SoldFor.SldFor} руб.";
    }

    
    public async Task DeleteAsync()
    {
        if (SelectedItem is null)return;
        var item = _service.GetAllItems().SingleOrDefault(i => i.Id == SelectedItem.Id);
        if (item is not null)
        {
            _service.DeleteItem(item);
            _expanders.Remove(item);
        }
        string projectDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        string graphicsDir = Path.Combine(projectDir, "PlotGraphics");
        string fileName = $"{item.ItemName.Replace(" ", "").Replace("|", "").Replace(":", "").Replace("(", "").Replace(")", "")}.png";
        string filePath = Path.Combine(graphicsDir, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Thread.Sleep(100);
        }
        Load();
    }

    public async Task SellAsync()
    {
        double doubleprice = SelectedItem.PriceToDouble(SelectedItem.FullStatPriceWithFee);
        SoldFor.UpdateSoldFor(doubleprice);
        await DeleteAsync();
        Load();
    }

    public async Task RefreshAllAsync()
    {
        if(_expanders.Count==0)return;
        IsRefreshing = true;
        List<Item> updatedItems = new List<Item>();
        foreach (var item in _expanders)
        {
            try
            {
                Item updatedItem = new Item();
                await updatedItem.TotalParse(item.ItemName, item.PriceToDouble(item.Count).ToString(),
                    item.PriceToDouble(item.PurchasePrice).ToString(), GameRewrite(item.Game));

                if (updatedItem.SteamPrice != item.SteamPrice || updatedItem.Count != item.Count ||
                    updatedItem.PurchasePrice != item.PurchasePrice)
                {
                    item.UpdateFrom(updatedItem);
                    updatedItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
        foreach (var item2 in updatedItems)
        {
            _service.UpdateItem(item2);
            _expanders.Remove(item2);
        }
        IsRefreshing = false;
        Load();
        
    }
    public async Task RefreshAsync()
    {
        if (SelectedItem is null)return;
        var item = _service.GetAllItems()?.SingleOrDefault(i => i.Id == SelectedItem.Id);
        Item it = new Item(); 
        await it.TotalParse(item.ItemName, item.PriceToDouble(SelectedItem.Count).ToString(), item.PriceToDouble(SelectedItem.PurchasePrice).ToString(), GameRewrite(item.Game));
        if (it.SteamPrice != item.SteamPrice || it.Count!=item.Count || it.PurchasePrice!=item.PurchasePrice)
        {
            item.PurchasePrice = it.PurchasePrice;
            item.Count = it.Count;
            item.SteamPrice = it.SteamPrice;
            item.Currency = it.Currency;
            item.SteamPriceWithFee = it.SteamPriceWithFee;
            item.StatPrice = it.StatPrice;
            item.StatPriceWithFee = it.StatPriceWithFee;
            item.FullStatPrice = it.FullStatPrice;
            item.FullStatPriceWithFee = it.FullStatPriceWithFee;
            item.FullStatPricePercent = it.FullStatPricePercent;
            item.Spent = it.Spent;
            _service.UpdateItem(item);
        }
        _expanders.Clear();
        Load();
    }
    public void AddToExpanders(Item item)
    {
        _expanders.Add(item);
    }

    public async Task NullSoldFor()
    {
        SoldFor.SoldToNull();
        Load();
    }
    public string GameRewrite(string game)
    {
        if (game == "cs2") game = "Counter Strike 2";
        if (game == "dota2") game = "Dota 2";
        if (game == "tf2") game = "Team Fortress 2";
        if (game == "rust") game = "rust";
        return game;
    }
}
