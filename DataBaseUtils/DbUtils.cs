using System.Text;
using Dalorian_Bot.Service;
using MySql.Data.MySqlClient;
using Tomlyn;

namespace Dalorian_Bot.DataBaseUtils;

public class DbUtils
{
    public static MySqlConnection GetDBConnection()
    {
        string host;
        string database;
        string username;
        string password;
        
        using (FileStream fstream = File.OpenRead("config-dal.toml"))
        {
            byte[] buffer = new byte[fstream.Length];
            fstream.Read(buffer, 0, buffer.Length);
            string textFromFile = Encoding.Default.GetString(buffer);

            var model = Toml.ToModel(textFromFile);
            host = (string) model["addressDatabase"]!;
            database = (string) model["nameDatabase"]!;
            username = (string) model["nameUserDatabase"]!;
            password = (string) model["passwordUserDatabase"]!;
        }

        
        // Connection String.
        String connString = $"Server={host};Database={database};User Id={username};password={password}";

        MySqlConnection conn = new MySqlConnection(connString);

        return conn;
    }
    
    public static void CreateTableOrNo()
    {
        var conn = GetDBConnection();
        
        conn.Open();
        try
        {
            string query = "CREATE TABLE `users` (" +
                           "`id` BIGINT NULL DEFAULT NULL ," +
                           "`karma` INT NULL DEFAULT NULL," +
                           "`lastdate` DATETIME NULL DEFAULT NULL " +
                           ") ENGINE = InnoDB;";
            MySqlCommand command = new MySqlCommand(query, conn);
            command.ExecuteNonQuery();
        }
        catch (MySqlException) { }
        conn.Close();
    }
    
    public static int GetKarma(long id)
    {
        string sql = $"SELECT karma FROM `users` WHERE id = {id};";
        
        var conn = GetDBConnection();
        
        conn.Open();
        using var cmd = new MySqlCommand(sql, conn);
        using MySqlDataReader rdr = cmd.ExecuteReader();
        int karma = 0;
        if (rdr.HasRows)
        {
            if (rdr.Read())
            {
                karma = rdr.GetInt32(0);
                rdr.Close();
            }
        }
        else
        {
            karma = 0;
        }
        
        conn.Close();
        return karma;
    }

    public static List<UserData> GetAllKarma()
    {
        var listItems = new List<UserData>() { };
        string sql = "SELECT id, karma FROM `users`;";

        var conn = GetDBConnection();
        
        conn.Open();
        using var cmd = new MySqlCommand(sql, conn);
        using MySqlDataReader rdr = cmd.ExecuteReader();

        if (rdr.HasRows)
        {
            while (rdr.Read())
            {
                long id = rdr.GetInt64(0);
                int karma = rdr.GetInt32(1);
                Console.WriteLine("\nID: " + id + "\nKarma: " + karma);
                var item = new UserData() { Id = id, Karma = karma };
                listItems.Add(item);
            }
        }
        rdr.Close();
        conn.Close();
        
        List<UserData> results = listItems.GroupBy(x => x.Id).Select(x => x.First()).ToList();
        var users = results.OrderByDescending(x => x.Karma).ToList();
        if (users.Count >= 10)
        {
            return users.GetRange(0, 9);
        }
        else
        {
            return users.GetRange(0, users.Count);   
        }
    }

    public static void InsertUser(long id)
    {
        var conn = GetDBConnection();

        conn.Open();
        var sql = $"INSERT INTO users VALUES({id}, 1, NOW());";
        using var command = new MySqlCommand(sql, conn);
        command.ExecuteNonQuery();
        conn.Close();
    }
    
    public static int GetDate(long id)
    {
        var conn = GetDBConnection();

        var listItems = new List<DateTime>() { };
        string sql = "SELECT lastdate FROM `users`;";
        
        conn.Open();
        using var cmd = new MySqlCommand(sql, conn);
        using MySqlDataReader rdr = cmd.ExecuteReader();

        if (rdr.HasRows)
        {
            while (rdr.Read())
            {
                DateTime date = rdr.GetDateTime(0);
                listItems.Add(date);
            }
        }
        rdr.Close();
        conn.Close();
        if (listItems.Count == 0)
        {
            InsertUser(id);
            return 15;
        }
        else
        {
            List<DateTime> results = listItems
                .OrderByDescending(x => x)
                .ToList();
            int returned = -(int)(results[0] - DateTime.Now).TotalSeconds;
            return returned;
        }
    }

    public static void AddKarma( int currentKarma, long id)
    {
        var conn = GetDBConnection();

        conn.Open();
        string sql = $"UPDATE users SET karma={currentKarma + 1}, lastdate=NOW() WHERE id={id}";
        using var command = new MySqlCommand(sql, conn);
        command.ExecuteNonQuery();
        conn.Close();
    }

    public static bool checkUser(long id)
    {
        var conn = GetDBConnection();
        MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM users WHERE id={id}";
        try
        {
            conn.Open();                
        } catch (Exception ex) {
            conn.Close();
        }  
        MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            reader.Close();
            return true;
        }
        else
        {
            reader.Close();
            return false;
        }
    }

}