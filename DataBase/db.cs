using System;
using Microsoft.Data.Sqlite;

namespace DataBase;

class DataBase
{
    public static void CreateTable()
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            connection.Open();

            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS users (
                id	INTEGER PRIMARY KEY AUTOINCREMENT,
                name	TEXT,
                login	TEXT,
                phone	TEXT,
                discription	TEXT,
                status	TEXT,
                time	INTEGER
            );
            
            CREATE TABLE IF NOT EXISTS chats (
                id	INTEGER PRIMARY KEY AUTOINCREMENT,
                companion_1_id	TEXT,
                companion_2_id	TEXT
            );

            CREATE TABLE IF NOT EXISTS messeges (
                id	INTEGER PRIMARY KEY AUTOINCREMENT,
                chat_id	TEXT,
                text	TEXT,
                user_id	TEXT,
                companion_status	TEXT,
                time	INTEGER
            );

            CREATE TABLE IF NOT EXISTS token (
                id	INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id	TEXT,
                token	TEXT
            );

            ";
            command.ExecuteNonQuery();
        }
    }
}