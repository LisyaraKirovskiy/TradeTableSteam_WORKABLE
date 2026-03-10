using System.Collections.Generic;
using System.Xml;
using Items.Model;
using Microsoft.Data.Sqlite;

namespace Items.DAL;

public class SqliteDb : ICrud
{
    private readonly string _connectionString;
    private SqliteConnection _db;

    public SqliteDb(string connectionstring)
    {
        _connectionString = connectionstring;
    }
    public IEnumerable<Item>? GetAll()
    {
        const string sql = @"SELECT * FROM Items";
        var items = SqliteHelper.ExistSelect(sql, _connectionString);
        return items;
    }
    
    public bool Add(Item item)
    {
     
        var sql = $"""
                    INSERT INTO Items (ItemName,Count,Url,PurchasePrice,SteamPrice,Currency,SteamPriceWithFee,StatPrice,StatPriceWithFee,FullStatPrice,FullStatPriceWithFee,FullStatPricePercent,Image,Spent,Game) 
                    VALUES ('{item.ItemName}','{item.Count}','{item.Url}','{item.PurchasePrice}','{item.SteamPrice}','{item.Currency}','{item.SteamPriceWithFee}','{item.StatPrice}','{item.StatPriceWithFee}','{item.FullStatPrice}','{item.FullStatPriceWithFee}','{item.FullStatPricePercent}','{item.Image}','{item.Spent}','{item.Game}');
                    INSERT INTO ItemDatePrice (Name,Price) 
                    VALUES ('{item.ItemName}','{item.SteamPrice}');
                    """;
        var result = SqliteHelper.Exist(sql,_connectionString);
        return result;
    }

    public bool Delete(Item item)
    {
        var sql = $"""
                   DELETE FROM Items 
                   WHERE Id={item.Id};
                   DELETE FROM ItemDatePrice 
                   WHERE Name="{item.ItemName}";
                   """;
        var result = SqliteHelper.Exist(sql,_connectionString);
        return result;
    }

    public bool Update(Item item)
    {
        var sql = $"""
                   UPDATE Items
                   SET ItemName = '{item.ItemName}',
                       Count = '{item.Count}',
                       Url = '{item.Url}',
                       PurchasePrice = '{item.PurchasePrice}',
                       SteamPrice = '{item.SteamPrice}',
                       Currency = '{item.Currency}',
                       SteamPriceWithFee = '{item.SteamPriceWithFee}',
                       StatPrice = '{item.StatPrice}',
                       StatPriceWithFee = '{item.StatPriceWithFee}',
                       FullStatPrice = '{item.FullStatPrice}',
                       FullStatPriceWithFee = '{item.FullStatPriceWithFee}',
                       FullStatPricePercent = '{item.FullStatPricePercent}',
                       Image = '{item.Image}',
                       Spent = '{item.Spent}',
                       Game = '{item.Game}'
                   WHERE Id = {item.Id};
                   INSERT INTO ItemDatePrice (Name,Price) 
                   VALUES ('{item.ItemName}','{item.SteamPrice}');
                   """;
        var result = SqliteHelper.Exist(sql,_connectionString);
        return result;
    }

    
}