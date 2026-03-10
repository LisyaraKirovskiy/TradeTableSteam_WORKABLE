
using System.Collections;
using System.Collections.Generic;
using Items.Model;
using Microsoft.Data.Sqlite;

namespace Items.DAL;

public static class SqliteHelper
{
    public static bool Exist(string sql, string connectionString)
    {
        var db = new SqliteConnection(connectionString);
        db.Open();
        
        var command = new SqliteCommand(sql, db);
        var result = command.ExecuteNonQuery();
        
        db.Close();
        return result > 0;   
    }

    public static IEnumerable<Item>? ExistSelect(string sql, string connectionString)
    {
        var db = new SqliteConnection(connectionString);
        db.Open();
        
        var command = db.CreateCommand();
        command.CommandText = sql;

        var reader = command.ExecuteReader();
        if (!reader.HasRows) return null;
        var items = new List<Item>();
        while (reader.Read())
        {
            var item = new Item()
            {
                Id = reader.GetInt32(0),
                ItemName = reader.GetString(1),
                Count = reader.GetString(2),
                Url = reader.GetString(3),
                PurchasePrice = reader.GetString(4),
                SteamPrice = reader.GetString(5),
                Currency = reader.GetString(6),
                SteamPriceWithFee = reader.GetString(7),
                StatPrice = reader.GetString(8),
                StatPriceWithFee = reader.GetString(9),
                FullStatPrice = reader.GetString(10),
                FullStatPriceWithFee = reader.GetString(11),
                FullStatPricePercent = reader.GetString(12),
                Image=reader.GetString(13),
                Spent = reader.GetString(14),
                Game=reader.GetString(15)
            };
            items.Add(item);
        }
        
        db.Close();
        return items;
    }
}