using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text.RegularExpressions;
using AngleSharp.Text;
using Items.DAL;
using Items.Model;
using Microsoft.Data.Sqlite;

namespace testgraph;

public class SoldFor
{
    public int Id { get; set; }
    public double SldFor { get; set; }

    public double GetSoldFor()
    {
        const string sql = @"SELECT SoldPrice FROM SoldFor WHERE Id=1";
        var connection = File.ReadAllText("db.config");
        var db = new SqliteConnection(connection);
        db.Open();
        
        var command = db.CreateCommand();
        command.CommandText = sql;

        var reader = command.ExecuteReader();
        if (!reader.HasRows) return 0;
        while (reader.Read())
        {
            SldFor= reader.GetDouble(0);
        }
        db.Close();
        return SldFor;
    }

    public void UpdateSoldFor(double price)
    {
        var sql = $"""
                   UPDATE SoldFor
                   SET SoldPrice = SoldPrice +'{price}'
                   WHERE Id=1
                   """;
        var connection = File.ReadAllText("db.config");
        var db = new SqliteConnection(connection);
        db.Open();
        var command = db.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
        db.Close();
    }

    public void SoldToNull()
    {
        var sql = $"""
                   UPDATE SoldFor
                   SET SoldPrice = 0
                   WHERE Id=1
                   """;
        var connection = File.ReadAllText("db.config");
        var db = new SqliteConnection(connection);
        db.Open();
        var command = db.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
        db.Close();
    }
    
}