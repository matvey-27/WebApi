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
                saqura_id	INTEGER PRIMARY KEY AUTOINCREMENT,
                name	TEXT,
                login	TEXT,
                password	TEXT,
                discription	TEXT,
                status	TEXT,
                time	TEXT
            );
            
            CREATE TABLE IF NOT EXISTS Chats (
                chat_id	INTEGER PRIMARY KEY AUTOINCREMENT,
                saqura_id_one	INTEGER,
                saqura_id_two	INTEGER
            );

            CREATE TABLE IF NOT EXISTS Messeges (
                id	INTEGER,
                chat_id	INTEGER,
                text	TEXT,
                saqura_id	INTEGER,
                companion_status	TEXT,
                time	TEXT
            );

            CREATE TABLE IF NOT EXISTS Blocked (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                saqura_id INTEGER,
                blocked_saqura_id INTEGER
            );

            CREATE TABLE IF NOT EXISTS Token (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                saqura_id INTEGER,
                token TEXT,
                UNIQUE(saqura_id, token) 
            );

            ";
            command.ExecuteNonQuery();
        }
    }

    public static async Task<bool> FindUserAsync(string login, string password)
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            await connection.OpenAsync(); 
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

            using (SqliteDataReader reader = await command.ExecuteReaderAsync()) 
            {
                if (await reader.ReadAsync()) 
                {
                    return reader.GetInt32(0) == 1; 
                }
            }
        }

        return false;
    }

    public static async Task<bool> UserExists(string login)
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            await connection.OpenAsync(); 
            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = @"
                SELECT EXISTS (
                    SELECT 1 FROM Users
                    WHERE login = $login
                ) AS userExists;
            ";

            command.Parameters.AddWithValue("$login", login);

            using (SqliteDataReader reader = await command.ExecuteReaderAsync()) 
            {
                if (await reader.ReadAsync()) 
                {
                    return reader.GetInt32(0) == 1; 
                }
            }
        }

        return false;
    }

    public static string GenerateToken()
    {
        byte[] tokenBytes = new byte[32]; 
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }
        return Convert.ToBase64String(tokenBytes); 
    }

    public static string GetTokenByLogin(string login)
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            connection.Open();

            SqliteCommand findUserCommand = new SqliteCommand();
            findUserCommand.Connection = connection;
            findUserCommand.CommandText = @"
                SELECT saqura_id FROM Users
                WHERE login = $login;
            ";
            findUserCommand.Parameters.AddWithValue("$login", login);

            var userId = findUserCommand.ExecuteScalar();

            if (userId == null)
            {
                throw new Exception("Пользователь с таким логином не найден.");
            }

            string token = GenerateToken(); 

            SqliteCommand saveTokenCommand = new SqliteCommand();
            saveTokenCommand.Connection = connection;
            saveTokenCommand.CommandText = @"
                INSERT INTO Token (saqura_id, token)
                VALUES ($saqura_id, $token);
            ";
            saveTokenCommand.Parameters.AddWithValue("$saqura_id", userId);
            saveTokenCommand.Parameters.AddWithValue("$token", token);

            try
            {
                saveTokenCommand.ExecuteNonQuery();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19) 
            {
                
                return GetTokenByLogin(login); 
            }

           
            return token;
        }
    }

    public static string? GetIdByToken(string token)
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            connection.Open();

            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = @"
                SELECT saqura_id FROM Token
                WHERE token = $token;
            ";
            command.Parameters.AddWithValue("$token", token);

            var userId = command.ExecuteScalar();
            return userId?.ToString(); // Возвращаем null, если токен не найден
        }
    }

    public static bool ChatExists(int saquraIdOne, int saquraIdTwo)
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            connection.Open();

            // Убедимся, что saquraIdOne < saquraIdTwo
            if (saquraIdOne > saquraIdTwo)
            {
                (saquraIdOne, saquraIdTwo) = (saquraIdTwo, saquraIdOne);
            }

            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = @"
                SELECT COUNT(*) FROM Chats
                WHERE saqura_id_one = $saquraIdOne AND saqura_id_two = $saquraIdTwo;
            ";
            command.Parameters.AddWithValue("$saquraIdOne", saquraIdOne);
            command.Parameters.AddWithValue("$saquraIdTwo", saquraIdTwo);

            int count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0; // Если count > 0, чат существует
        }
    }

    public static async Task<bool> ChatExistsAsync(int saquraIdOne, int saquraIdTwo)
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            await connection.OpenAsync();

            // Убедимся, что saquraIdOne < saquraIdTwo
            if (saquraIdOne > saquraIdTwo)
            {
                (saquraIdOne, saquraIdTwo) = (saquraIdTwo, saquraIdOne);
            }

            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = @"
                SELECT COUNT(*) FROM Chats
                WHERE saqura_id_one = $saquraIdOne AND saqura_id_two = $saquraIdTwo;
            ";
            command.Parameters.AddWithValue("$saquraIdOne", saquraIdOne);
            command.Parameters.AddWithValue("$saquraIdTwo", saquraIdTwo);

            int count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0; // Если count > 0, чат существует
        }
    }

    public static void CreateChat(int saquraIdOne, int saquraIdTwo)
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            connection.Open();

           
            if (saquraIdOne > saquraIdTwo)
            {
                (saquraIdOne, saquraIdTwo) = (saquraIdTwo, saquraIdOne);
            }

           
            if (ChatExists(saquraIdOne, saquraIdTwo))
            {
                Console.WriteLine("Чат уже существует.");
                return;
            }

          
            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = @"
                INSERT INTO Chats (saqura_id_one, saqura_id_two)
                VALUES ($saquraIdOne, $saquraIdTwo);
            ";
            command.Parameters.AddWithValue("$saquraIdOne", saquraIdOne);
            command.Parameters.AddWithValue("$saquraIdTwo", saquraIdTwo);

            command.ExecuteNonQuery();
            // Console.WriteLine("Чат успешно создан.");
        }
        
    }

    public static int FindChat(int saquraIdOne, int saquraIdTwo)

    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            connection.Open();

            // Убедимся, что saquraIdOne < saquraIdTwo
            if (saquraIdOne > saquraIdTwo)
            {
                (saquraIdOne, saquraIdTwo) = (saquraIdTwo, saquraIdOne);
            }

            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = @"
                SELECT chat_id FROM Chats
                WHERE saqura_id_one = $saquraIdOne AND saqura_id_two = $saquraIdTwo;
            ";
            command.Parameters.AddWithValue("$saquraIdOne", saquraIdOne);
            command.Parameters.AddWithValue("$saquraIdTwo", saquraIdTwo);

            int res = Convert.ToInt32(command.ExecuteScalar());
            return res; // Если count > 0, чат существует
        }
    }
    
    public static void AddMessage(int chatId, int saquraId, string text, string companionStatus)
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            connection.Open();

            SqliteCommand getNextIdCommand = new SqliteCommand();
            getNextIdCommand.Connection = connection;
            getNextIdCommand.CommandText = @"
                SELECT COALESCE(MAX(id), 0) + 1 FROM Messeges
                WHERE chat_id = $chatId;
            ";
            getNextIdCommand.Parameters.AddWithValue("$chatId", chatId);

            int nextId = Convert.ToInt32(getNextIdCommand.ExecuteScalar());

          
            SqliteCommand addMessageCommand = new SqliteCommand();
            addMessageCommand.Connection = connection;
            addMessageCommand.CommandText = @"
                INSERT INTO Messeges (id, chat_id, text, saqura_id, companion_status, time)
                VALUES ($id, $chatId, $text, $saquraId, $companionStatus, $time);
            ";
            addMessageCommand.Parameters.AddWithValue("$id", nextId);
            addMessageCommand.Parameters.AddWithValue("$chatId", chatId);
            addMessageCommand.Parameters.AddWithValue("$text", text);
            addMessageCommand.Parameters.AddWithValue("$saquraId", saquraId);
            addMessageCommand.Parameters.AddWithValue("$companionStatus", companionStatus);
            addMessageCommand.Parameters.AddWithValue("$time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); // Текущее время

            addMessageCommand.ExecuteNonQuery();
            // Console.WriteLine("Сообщение успешно добавлено.");
        }
    }

    public static bool TokenExists(string token)
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            connection.Open();

            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = @"
                SELECT COUNT(*) FROM Token
                WHERE token = $token;
            ";
            command.Parameters.AddWithValue("$token", token);

            int count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0; // Если count > 0, токен существует
        }
    }

    public static void AddUser(string login, string password)
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            connection.Open();

            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = @"
                INSERT INTO Users (name, login, password, discription, status, time)
                VALUES ($name, $login, $password, $discription, $status, $time);
            ";

            // Значения по умолчанию
            string defaultName = "New User";
            string defaultDescription = "No description";
            string defaultStatus = "offline";
            string defaultTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Текущее время

            // Параметры
            command.Parameters.AddWithValue("$name", defaultName);
            command.Parameters.AddWithValue("$login", login);
            command.Parameters.AddWithValue("$password", password);
            command.Parameters.AddWithValue("$discription", defaultDescription);
            command.Parameters.AddWithValue("$status", defaultStatus);
            command.Parameters.AddWithValue("$time", defaultTime);

            command.ExecuteNonQuery();
            // Console.WriteLine("Пользователь успешно добавлен.");
        }
    }

    public static List<Chat> GetChatsByToken(string token)
    {
        using (var connection = new SqliteConnection("Data Source=DataBase/db.db;Mode=ReadWriteCreate"))
        {
            connection.Open();

            // Получаем saqura_id по токену
            SqliteCommand getUserIdCommand = new SqliteCommand();
            getUserIdCommand.Connection = connection;
            getUserIdCommand.CommandText = @"
                SELECT saqura_id FROM Token
                WHERE token = $token;
            ";
            getUserIdCommand.Parameters.AddWithValue("$token", token);

            var userId = getUserIdCommand.ExecuteScalar();
            if (userId == null)
            {
                throw new Exception("Токен не найден.");
            }

            int saquraId = Convert.ToInt32(userId);

            // Извлекаем чаты, где пользователь является участником
            SqliteCommand getChatsCommand = new SqliteCommand();
            getChatsCommand.Connection = connection;
            getChatsCommand.CommandText = @"
                SELECT * FROM Chats
                WHERE saqura_id_one = $saquraId OR saqura_id_two = $saquraId;
            ";
            getChatsCommand.Parameters.AddWithValue("$saquraId", saquraId);

            var reader = getChatsCommand.ExecuteReader();
            var chats = new List<Chat>();

            while (reader.Read())
            {
                chats.Add(new Chat
                {
                    ChatId = reader.GetInt32(0),
                    SaquraIdOne = reader.GetInt32(1),
                    SaquraIdTwo = reader.GetInt32(2)
                });
            }

            return chats;
        }
    }
}