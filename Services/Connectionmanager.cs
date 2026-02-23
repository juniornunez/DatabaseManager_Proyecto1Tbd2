using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DatabaseManager.Models
{
    public class SavedConnection
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public bool UseWindowsAuth { get; set; }
        public string Username { get; set; }
        public DateTime LastUsed { get; set; }

        public SavedConnection()
        {
            Id = Guid.NewGuid().ToString();
            LastUsed = DateTime.Now;
        }
    }

    public class ConnectionManager
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DatabaseManager",
            "connections.json"
        );

        public static List<SavedConnection> LoadConnections()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return new List<SavedConnection>();

                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<List<SavedConnection>>(json) ?? new List<SavedConnection>();
            }
            catch
            {
                return new List<SavedConnection>();
            }
        }

        public static void SaveConnections(List<SavedConnection> connections)
        {
            try
            {
                string directory = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(connections, options);
                File.WriteAllText(FilePath, json);
            }
            catch
            {
            }
        }

        public static void AddConnection(SavedConnection connection)
        {
            var connections = LoadConnections();

            var existing = connections.Find(c =>
                c.Server == connection.Server &&
                c.Database == connection.Database &&
                c.UseWindowsAuth == connection.UseWindowsAuth &&
                c.Username == connection.Username);

            if (existing != null)
            {
                existing.Name = connection.Name;
                existing.LastUsed = DateTime.Now;
            }
            else
            {
                connections.Add(connection);
            }

            SaveConnections(connections);
        }

        public static void DeleteConnection(string id)
        {
            var connections = LoadConnections();
            connections.RemoveAll(c => c.Id == id);
            SaveConnections(connections);
        }

        public static void UpdateLastUsed(string id)
        {
            var connections = LoadConnections();
            var connection = connections.Find(c => c.Id == id);
            if (connection != null)
            {
                connection.LastUsed = DateTime.Now;
                SaveConnections(connections);
            }
        }
    }
}