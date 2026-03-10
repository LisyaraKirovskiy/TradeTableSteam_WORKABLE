using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Text;
using Avalonia.Media.Imaging;
using testgraph;
namespace Items.Model;

public class Item: INotifyPropertyChanged
{
    public int Id { get; set; }
    public string ItemName { get; set; }
    public string Count { get; set; }
    public string Url { get; set; }
    public string PurchasePrice { get; set; }
    public string SteamPrice { get; set; }
    public string Currency { get; set; }
    public string SteamPriceWithFee { get; set; }
    public string StatPrice { get; set; }
    public string StatPriceWithFee { get; set; }
    public string FullStatPrice { get; set; }
    public string FullStatPriceWithFee { get; set; }
    public string FullStatPricePercent { get; set; }
    public string Image { get; set; }
    public string Spent { get; set; }
    public string Game { get; set; }
    
    
    public string TextColor
    {
        get
        {
            var price = PriceToDouble(FullStatPrice);
            return price > 0 ? "#2cbf08" : (price < 0 ? "#bf1408" : "#808080"); // Красный < Серый (0) < Зеленый
        }
    }
    public Bitmap? GraphicImagePath => ImageHelper.LoadFromResource(new Uri($"avares://testgraph/PlotGraphics/{ItemName.Replace(" ","").Replace("|","").Replace(":","").Replace("(","").Replace(")","")}.png"));
    public Task<Bitmap?> ImageUrl => ImageHelper.LoadFromWeb(new Uri(Image));
    

public void UpdateFrom(Item source)
    {
        SteamPrice = source.SteamPrice;
        Count = source.Count;
        PurchasePrice = source.PurchasePrice;
        Currency = source.Currency;
        SteamPriceWithFee = source.SteamPriceWithFee;
        StatPrice = source.StatPrice;
        StatPriceWithFee = source.StatPriceWithFee;
        FullStatPrice = source.FullStatPrice;
        FullStatPriceWithFee = source.FullStatPriceWithFee;
        FullStatPricePercent = source.FullStatPricePercent;
        Spent = source.Spent;
    }
    public async Task TotalParse(string name,string count,string price, string game)
    {
        SetNameCountPrice(name,count,price);
        string setgame = null;
        switch (game)
        {
            case "Counter Strike 2":
                setgame = "cs2";
                this.Game = setgame;
                break;
            case "Dota 2":
                setgame = "dota2";
                this.Game = setgame;
                break;
            case "Team Fortress 2":
                setgame = "tf2";
                this.Game = setgame;
                break;
            case "RUST":
                setgame = "rust";
                this.Game = setgame;
                break;
        }
        SetUrl(Game);
        await SetSteamPrice();
        SetStatPrice();
        SetFullStatPrice();
        SetSpent();
    }

    public void SetSpent()
    {
        this.Spent = (PriceToDouble(PurchasePrice) * PriceToDouble(Count)).ToString()+$" {Currency}";
    }
    public void SetNameCountPrice(string name,string count,string price)
    {
        SetItemName(name);
        SetCount(count);
        SetPurchasePrice(price);
    }
    // ItemName
    public void SetItemName(string name)
    {
        this.ItemName = name;
    }
    // Count
    public void SetCount(string count)
    {
        this.Count = $"{count} шт.";
    }

    // URL
    public void SetUrl(string game)
    {
        string url = ItemName;
        url = url.Replace(" ","%20");
        url = url.Replace("|", "%7C");
        url = url.Replace("(","%28");
        url = url.Replace(")","%29");
        url = url.Replace("+", "%20");
        url = url.Replace("&", "%26");
        switch (game)
        {
            case "cs2":
                this.Url = $"https://steamcommunity.com/market/search?appid=730/&q={url}";
                break;
            case "dota2":
                this.Url = $"https://steamcommunity.com/market/search?appid=570/&q={url}";
                break;
            case "tf2":
                this.Url = $"https://steamcommunity.com/market/search?category_440_Collection%5B%5D=any&category_440_Type%5B%5D=any&appid=440&q={url}";
                break;
            case "rust":
            this.Url = $"https://steamcommunity.com/market/search?category_252490_itemclass%5B%5D=any&appid=252490&q={url}";
            break;
                
        }
        
    }
    public string GetUrl()
    {
        return this.Url;
    }
    // PurchasePrice
    public void SetPurchasePrice(string purchprice)
    {
        this.PurchasePrice = $"{purchprice} руб.";
    }
    
    // SteamPrice & Currency
    public async Task SetSteamPrice()
    {
        var config=Configuration.Default.WithDefaultLoader();
        var context=BrowsingContext.New(config);
        var document=await context.OpenAsync($"{GetUrl()}");
        var parseprice = document.QuerySelector("#resultlink_0")?.QuerySelector("span.normal_price")?.QuerySelector("span.normal_price");
        string? price = parseprice?.TextContent;
        var parseimage = document.QuerySelector("#result_0_image");
        double pricewithfee = Math.Round((PriceToDouble(price) * 0.85), 2); 
        this.SteamPrice = price;
        string currency = price;
        currency = Regex.Replace(currency, @"[\-\-\$\d.?,?\s?]", "");
        this.Image = parseimage?.GetAttribute("src")?.ToString();
        this.Currency = currency;
        this.SteamPriceWithFee = pricewithfee.ToString();
        this.SteamPrice = PriceToDouble(this.SteamPrice).ToString();
        await ConvertCurrency(this.Currency);
    }
    
    // StatPrice
    public void SetStatPrice()
    {
        double steampr = PriceToDouble(this.SteamPrice);
        double steamprwithfee = PriceToDouble(this.SteamPriceWithFee);
        double purchpr = PriceToDouble(this.PurchasePrice);
        
        double stat = Math.Round(steampr - purchpr,2);
        double statwithfee = Math.Round(( steamprwithfee - purchpr),2);
        double statperc = Math.Round((((steampr - purchpr)/purchpr) * 100),2);
        if(statperc>0)
        {
            this.StatPrice = $"{stat} {this.Currency}";  
            this.StatPriceWithFee = $"{statwithfee} {this.Currency}";
        }
        else
        {
            this.StatPrice = $"{stat} {this.Currency}";
            this.StatPriceWithFee = $"{statwithfee} {this.Currency}";
        }
    }
    
    // FullStatPrice
    public void SetFullStatPrice()
    {
        double steampr = PriceToDouble(this.SteamPrice);
        double steamprwithfee = PriceToDouble(this.SteamPriceWithFee);
        double purchpr = PriceToDouble(this.PurchasePrice);
        double count = PriceToDouble(this.Count);
        double stat = Math.Round((steampr*count) - (purchpr*count),2);
        double statwithfee = Math.Round((steamprwithfee*count - purchpr*count),2);
        double statperc = Math.Round((((steampr*count - purchpr*count)/(purchpr*count)) * 100),2);
        if(statperc>0)
        {
            this.FullStatPrice = $"{stat} {this.Currency}";  
            this.FullStatPriceWithFee = $"{statwithfee} {this.Currency}";
            this.FullStatPricePercent = $"{statperc}% \u2197";// ↗

        }
        else
        {
            this.FullStatPrice = $"{stat} {this.Currency}";
            this.FullStatPriceWithFee = $"{statwithfee} {this.Currency}";
            this.FullStatPricePercent = $"{statperc}% \u2198";// ↘
        }
    }
    
    // PriceToDouble
    public double PriceToDouble(string price)
    {
        string pr = price;
        bool IsNegative = pr.Contains("-");
        string pricewithoutcurrency = Regex.Replace(pr, @"[^\d\,?.?]", "");
        if (pricewithoutcurrency.EndsWith("."))
        {
            pricewithoutcurrency= pricewithoutcurrency.Substring(0, pricewithoutcurrency.Length - 1);
            pricewithoutcurrency = pricewithoutcurrency.Replace(',','.');
        }
        else
        {
            pricewithoutcurrency = pricewithoutcurrency.Replace(',','.');
        }
        double result=pricewithoutcurrency.ToDouble();
        return IsNegative?-result:result;
    }

    // Currency convert to rub
    public async Task ConvertCurrency(string currency)
    {

        switch (currency)
        {
            case "руб":
            {
                this.Currency = "руб.";
                break;
            }
            case "USD":
            {
                currency = "USD";
                var config=Configuration.Default.WithDefaultLoader();
                var context=BrowsingContext.New(config);
                var document=await context.OpenAsync($"https://yandex.ru/finance/currencies/{currency}_rub");
                var parse = document.QuerySelector("span.ConverterInput-TextinputBackground");
                string curse = parse.TextContent;
                double usd_rub = curse.Replace(",",".").ToDouble();
                this.Currency = "руб.";
                double stpr = PriceToDouble(this.SteamPrice);
                double stprfee = PriceToDouble(this.SteamPriceWithFee);
                this.SteamPrice = $"{Math.Round((stpr * usd_rub),2).ToString()} {Currency}";
                this.SteamPriceWithFee =$"{Math.Round((stprfee * usd_rub),2).ToString()} {Currency}";
                break;
            }
            case "\u20ac": // "€"
            {
                currency = "EUR";
                var config=Configuration.Default.WithDefaultLoader();
                var context=BrowsingContext.New(config);
                var document=await context.OpenAsync($"https://yandex.ru/finance/currencies/{currency}_rub");
                var parse = document.QuerySelector("span.ConverterInput-TextinputBackground");
                string curse = parse.TextContent;
                double eur_rub = curse.Replace(",",".").ToDouble();
                this.Currency = "руб.";
                double stpr = PriceToDouble(this.SteamPrice);
                double stprfee = PriceToDouble(this.SteamPriceWithFee);
                this.SteamPrice = $"{Math.Round((stpr * eur_rub),2).ToString()} {Currency}";
                this.SteamPriceWithFee =$"{Math.Round((stprfee * eur_rub),2).ToString()} {Currency}";
                break;
            }
            case "\u20b8": // "₸"
            {
                currency = "KZT";
                var config=Configuration.Default.WithDefaultLoader();
                var context=BrowsingContext.New(config);
                var document=await context.OpenAsync($"https://yandex.ru/finance/currencies/{currency}_rub");
                var parse = document.QuerySelector("span.ConverterInput-TextinputBackground");
                string curse = parse.TextContent;
                double kzt_rub = curse.Replace(",",".").ToDouble();
                this.Currency = "руб.";
                double stpr = PriceToDouble(this.SteamPrice);
                double stprfee = PriceToDouble(this.SteamPriceWithFee);
                this.SteamPrice = $"{Math.Round((stpr * kzt_rub),2).ToString()} {Currency}";
                this.SteamPriceWithFee =$"{Math.Round((stprfee * kzt_rub),2).ToString()} {Currency}";
                break;
            }
            case "\u00a3": // "£"
            {
                currency = "GBP";
                var config=Configuration.Default.WithDefaultLoader();
                var context=BrowsingContext.New(config);
                var document=await context.OpenAsync($"https://yandex.ru/finance/currencies/{currency}_rub");
                var parse = document.QuerySelector("span.ConverterInput-TextinputBackground");
                string curse = parse.TextContent;
                double gbr_rub = curse.Replace(",",".").ToDouble();
                this.Currency = "руб.";
                double stpr = PriceToDouble(this.SteamPrice);
                double stprfee = PriceToDouble(this.SteamPriceWithFee);
                this.SteamPrice = $"{Math.Round((stpr * gbr_rub),2).ToString()} {Currency}";
                this.SteamPriceWithFee =$"{Math.Round((stprfee * gbr_rub),2).ToString()} {Currency}";
                break;
            }
            case "\u20b4": // "₴"
            {
                currency = "UAH";
                var config=Configuration.Default.WithDefaultLoader();
                var context=BrowsingContext.New(config);
                var document=await context.OpenAsync($"https://yandex.ru/finance/currencies/{currency}_rub");
                var parse = document.QuerySelector("span.ConverterInput-TextinputBackground");
                string curse = parse.TextContent;
                double uah_rub = curse.Replace(",",".").ToDouble();
                this.Currency = "руб.";
                double stpr = PriceToDouble(this.SteamPrice);
                double stprfee = PriceToDouble(this.SteamPriceWithFee);
                this.SteamPrice = $"{Math.Round((stpr * uah_rub),2).ToString()} {Currency}";
                this.SteamPriceWithFee =$"{Math.Round((stprfee * uah_rub),2).ToString()} {Currency}";
                break;
            }
            case "ARS": // "ARS$"
            {
                currency = "ARS";
                var config=Configuration.Default.WithDefaultLoader();
                var context=BrowsingContext.New(config);
                var document=await context.OpenAsync($"https://yandex.ru/finance/currencies/{currency}_rub");
                var parse = document.QuerySelector("span.ConverterInput-TextinputBackground");
                string curse = parse.TextContent;
                double ars_rub = curse.Replace(",",".").ToDouble();
                this.Currency = "руб.";
                double stpr = PriceToDouble(this.SteamPrice);
                double stprfee = PriceToDouble(this.SteamPriceWithFee);
                this.SteamPrice = $"{Math.Round((stpr * ars_rub),2).ToString()} {Currency}";
                this.SteamPriceWithFee =$"{Math.Round((stprfee * ars_rub),2).ToString()} {Currency}";
                break;
            }
            case "TL": // "TL"
            {
                currency = "TRY";
                var config=Configuration.Default.WithDefaultLoader();
                var context=BrowsingContext.New(config);
                var document=await context.OpenAsync($"https://yandex.ru/finance/currencies/{currency}_rub");
                var parse = document.QuerySelector("span.ConverterInput-TextinputBackground");
                string curse = parse.TextContent;
                double try_rub = curse.Replace(",",".").ToDouble();
                this.Currency = "руб.";
                double stpr = PriceToDouble(this.SteamPrice);
                double stprfee = PriceToDouble(this.SteamPriceWithFee);
                this.SteamPrice = $"{Math.Round((stpr * try_rub),2).ToString()} {Currency}";
                this.SteamPriceWithFee =$"{Math.Round((stprfee * try_rub),2).ToString()} {Currency}";
                break;
            }
            case "\u00a5": // ¥ 
            {
                currency = "JPY";
                var config=Configuration.Default.WithDefaultLoader();
                var context=BrowsingContext.New(config);
                var document=await context.OpenAsync($"https://yandex.ru/finance/currencies/{currency}_rub");
                var parse = document.QuerySelector("span.ConverterInput-TextinputBackground");
                string curse = parse.TextContent;
                double jpy_rub = curse.Replace(",",".").ToDouble();
                this.Currency = "руб.";
                double stpr = PriceToDouble(this.SteamPrice);
                double stprfee = PriceToDouble(this.SteamPriceWithFee);
                this.SteamPrice = $"{Math.Round((stpr * jpy_rub),2).ToString()} {Currency}";
                this.SteamPriceWithFee =$"{Math.Round((stprfee * jpy_rub),2).ToString()} {Currency}";
                break;
            }
            case "R": // BRL
            {
                currency = "BRL";
                var config=Configuration.Default.WithDefaultLoader();
                var context=BrowsingContext.New(config);
                var document=await context.OpenAsync($"https://yandex.ru/finance/currencies/{currency}_rub");
                var parse = document.QuerySelector("span.ConverterInput-TextinputBackground");
                string curse = parse.TextContent;
                double brl_rub = curse.Replace(",",".").ToDouble();
                this.Currency = "руб.";
                double stpr = PriceToDouble(this.SteamPrice);
                double stprfee = PriceToDouble(this.SteamPriceWithFee);
                this.SteamPrice = $"{Math.Round((stpr * brl_rub),2).ToString()} {Currency}";
                this.SteamPriceWithFee =$"{Math.Round((stprfee * brl_rub),2).ToString()} {Currency}";
                break;
            }
            case "CDN": // CAD
            {
                currency = "CAD";
                var config=Configuration.Default.WithDefaultLoader();
                var context=BrowsingContext.New(config);
                var document=await context.OpenAsync($"https://yandex.ru/finance/currencies/{currency}_rub");
                var parse = document.QuerySelector("span.ConverterInput-TextinputBackground");
                string curse = parse.TextContent;
                double cad_rub = curse.Replace(",",".").ToDouble();
                this.Currency = "руб.";
                double stpr = PriceToDouble(this.SteamPrice);
                double stprfee = PriceToDouble(this.SteamPriceWithFee);
                this.SteamPrice = $"{Math.Round((stpr * cad_rub),2).ToString()} {Currency}";
                this.SteamPriceWithFee =$"{Math.Round((stprfee * cad_rub),2).ToString()} {Currency}";
                break;
            }
            case "NT": // TWD
            {
                currency = "TWD";
                var config=Configuration.Default.WithDefaultLoader();
                var context=BrowsingContext.New(config);
                var document=await context.OpenAsync($"https://yandex.ru/finance/currencies/{currency}_rub");
                var parse = document.QuerySelector("span.ConverterInput-TextinputBackground");
                string curse = parse.TextContent;
                double twd_rub = curse.Replace(",",".").ToDouble();
                this.Currency = "руб.";
                double stpr = PriceToDouble(this.SteamPrice);
                double stprfee = PriceToDouble(this.SteamPriceWithFee);
                this.SteamPrice = $"{Math.Round((stpr * twd_rub),2).ToString()} {Currency}";
                this.SteamPriceWithFee =$"{Math.Round((stprfee * twd_rub),2).ToString()} {Currency}";
                break;
            }
            case "\u20a9": // ₩
            {
                currency = "KRW";
                var config=Configuration.Default.WithDefaultLoader();
                var context=BrowsingContext.New(config);
                var document=await context.OpenAsync($"https://yandex.ru/finance/currencies/{currency}_rub");
                var parse = document.QuerySelector("span.ConverterInput-TextinputBackground");
                string curse = parse.TextContent;
                double krw_rub = curse.Replace(",",".").ToDouble();
                this.Currency = "руб.";
                double stpr = PriceToDouble(this.SteamPrice);
                double stprfee = PriceToDouble(this.SteamPriceWithFee);
                this.SteamPrice = $"{Math.Round((stpr * krw_rub),2).ToString()} {Currency}";
                this.SteamPriceWithFee =$"{Math.Round((stprfee * krw_rub),2).ToString()} {Currency}";
                break;
            }
            case "zł": // zł
            {
                currency = "PLN";
                var config=Configuration.Default.WithDefaultLoader();
                var context=BrowsingContext.New(config);
                var document=await context.OpenAsync($"https://yandex.ru/finance/currencies/{currency}_rub");
                var parse = document.QuerySelector("span.ConverterInput-TextinputBackground");
                string curse = parse.TextContent;
                double pln_rub = curse.Replace(",",".").ToDouble();
                this.Currency = "руб.";
                double stpr = PriceToDouble(this.SteamPrice);
                double stprfee = PriceToDouble(this.SteamPriceWithFee);
                this.SteamPrice = $"{Math.Round((stpr * pln_rub),2).ToString()} {Currency}";
                this.SteamPriceWithFee =$"{Math.Round((stprfee * pln_rub),2).ToString()} {Currency}";
                break;
            }
            case "A": // A$
            {
                currency = "AUD";
                var config=Configuration.Default.WithDefaultLoader();
                var context=BrowsingContext.New(config);
                var document=await context.OpenAsync($"https://yandex.ru/finance/currencies/{currency}_rub");
                var parse = document.QuerySelector("span.ConverterInput-TextinputBackground");
                string curse = parse.TextContent;
                double aud_rub = curse.Replace(",",".").ToDouble();
                this.Currency = "руб.";
                double stpr = PriceToDouble(this.SteamPrice);
                double stprfee = PriceToDouble(this.SteamPriceWithFee);
                this.SteamPrice = $"{Math.Round((stpr * aud_rub),2).ToString()} {Currency}";
                this.SteamPriceWithFee =$"{Math.Round((stprfee * aud_rub),2).ToString()} {Currency}";
                break;
            }
        }
        
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}