using System;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;

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
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id TEXT NOT NULL,
                token TEXT NOT NULL,
                UNIQUE(user_id, token) -- Уникальность для комбинации user_id и token
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

    public static string GenerateToken()
    {
        byte[] tokenBytes = new byte[32]; // 32 байта = 256 бит
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }
        return Convert.ToBase64String(tokenBytes); // Преобразуем в строку Base64
    }

    public static string GetTokenByLogin(string login)
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            connection.Open();

            // 1. Находим user_id по логину
            SqliteCommand findUserCommand = new SqliteCommand();
            findUserCommand.Connection = connection;
            findUserCommand.CommandText = @"
                SELECT id FROM Users
                WHERE login = $login;
            ";
            findUserCommand.Parameters.AddWithValue("$login", login);

            var userId = findUserCommand.ExecuteScalar();

            if (userId == null)
            {
                throw new Exception("Пользователь с таким логином не найден.");
            }

            // 2. Генерация токена
            string token = GenerateToken(); // Пример генерации токена

            // 3. Сохраняем токен в таблице Token
            SqliteCommand saveTokenCommand = new SqliteCommand();
            saveTokenCommand.Connection = connection;
            saveTokenCommand.CommandText = @"
                INSERT INTO Token (user_id, token)
                VALUES ($user_id, $token);
            ";
            saveTokenCommand.Parameters.AddWithValue("$user_id", userId);
            saveTokenCommand.Parameters.AddWithValue("$token", token);

            try
            {
                saveTokenCommand.ExecuteNonQuery();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // Ошибка UNIQUE constraint failed
            {
                // Если токен уже существует, генерируем новый
                return GetTokenByLogin(login); // Рекурсивный вызов
            }

            // 4. Возвращаем токен
            return token;
        }
    }
}