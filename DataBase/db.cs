using System;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Data.Sqlite;

namespace DataBase;

class Data
{
    public static void CreateTable()
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            connection.Open();

            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                id	INTEGER PRIMARY KEY AUTOINCREMENT,
                name	TEXT,
                login	TEXT,
                password	TEXT,
                discription	TEXT,
                status	TEXT,
                time	INTEGER
            );
            
            CREATE TABLE IF NOT EXISTS Chats (
                id	INTEGER PRIMARY KEY AUTOINCREMENT,
                companion_1_id	TEXT,
                companion_2_id	TEXT
            );

            CREATE TABLE IF NOT EXISTS Messeges (
                id	INTEGER PRIMARY KEY AUTOINCREMENT,
                chat_id	TEXT,
                text	TEXT,
                user_id	TEXT,
                companion_status	TEXT,
                time	INTEGER
            );

            CREATE TABLE IF NOT EXISTS Token (
                id	INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id	TEXT,
                token	TEXT
            );

            ";
            command.ExecuteNonQuery();
        }
    }

    public static bool FindUser(string login, string password)
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            connection.Open();

            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = @"
                SELECT EXISTS (
                    SELECT 1 FROM Users
                    WHERE login = $login AND password = $password
                ) AS userExists;
            ";

            command.Parameters.AddWithValue("$login", login);
            command.Parameters.AddWithValue("$password", password);

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetInt32(0) == 1; // Возвращаем true или false
                }
            }
        }

        return false; // Если что-то пошло не так, возвращаем false
    }

    public static async Task<bool> FindUserAsync(string login, string password)
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            await connection.OpenAsync(); // Асинхронное открытие соединения

            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = @"
                SELECT EXISTS (
                    SELECT 1 FROM Users
                    WHERE login = $login AND password = $password
                ) AS userExists;
            ";

            command.Parameters.AddWithValue("$login", login);
            command.Parameters.AddWithValue("$password", password);

            using (SqliteDataReader reader = await command.ExecuteReaderAsync()) // Асинхронное выполнение запроса
            {
                if (await reader.ReadAsync()) // Асинхронное чтение данных
                {
                    return reader.GetInt32(0) == 1; // Возвращаем true или false
                }
            }
        }

        return false; // Если что-то пошло не так, возвращаем false
    }

    
}