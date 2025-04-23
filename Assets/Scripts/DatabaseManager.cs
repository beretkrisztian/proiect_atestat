using UnityEngine;
using System.Data;
using Mono.Data.Sqlite; // Ai nevoie de acest DLL
using System.IO;

public class DatabaseManager : MonoBehaviour
{
    private string dbPath;

    void Awake()
    {
        dbPath = "URI=file:" + Application.persistentDataPath + "/rouletteDB.db";
        CreateTableIfNotExists();
    }

    private void CreateTableIfNotExists()
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS PlayerData (id INTEGER PRIMARY KEY, money INTEGER);";
                command.ExecuteNonQuery();

                // Inițializează cu bani dacă nu există
                command.CommandText = "SELECT COUNT(*) FROM PlayerData;";
                int count = int.Parse(command.ExecuteScalar().ToString());

                if (count == 0)
                {
                    command.CommandText = "INSERT INTO PlayerData (money) VALUES (1000);";
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    public int GetMoney()
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT money FROM PlayerData WHERE id = 1;";
                return int.Parse(command.ExecuteScalar().ToString());
            }
        }
    }

    public void UpdateMoney(int newAmount)
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE PlayerData SET money = @money WHERE id = 1;";
                command.Parameters.AddWithValue("@money", newAmount);
                command.ExecuteNonQuery();
            }
        }
    }
}