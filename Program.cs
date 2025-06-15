using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace VulnerableProject
{
    class Program
    {

        private const string API_KEY = "super-secret-api-key"; // CodeQL1
        static string userInput = "'; DROP TABLE users; --";
        static string filenameInput = "../../etc/passwd";
        static string commandInput = "& del C:\\Windows\\System32";

        public class Payload
        {
            public string Command { get; set; }
        }

        static void Main(string[] args)
        {
            SqlInjection(userInput);
            CommandInjection(commandInput);
            PathTraversal(filenameInput);
            InsecureHash("password123");
            UnsafeDeserialization("{\"Command\": \"format C:\"}");
        }

        static void SqlInjection(string username)
        {
            string connStr = "Server=localhost;Database=TestDB;Trusted_Connection=True;";
            using SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            string query = "SELECT * FROM Users WHERE Username = '" + username + "'"; // CodeQL2
            using SqlCommand cmd = new SqlCommand(query, conn);
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine(reader["Username"]);
            }
        }

        static void CommandInjection(string userCommand)
        {
            Process.Start("cmd.exe", "/C echo " + userCommand); // CodeQL3
        }

        static void PathTraversal(string filename)
        {
            string fullPath = "C:\\data\\" + filename; // CodeQL4
            if (File.Exists(fullPath))
            {
                Console.WriteLine(File.ReadAllText(fullPath));
            }
        }

        static void InsecureHash(string data)
        {
            using (var md5 = MD5.Create()) // CodeQL5
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
                Console.WriteLine(Convert.ToHexString(hash));
            }

            using (var sha1 = SHA1.Create()) // CodeQL6
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(data));
                Console.WriteLine(Convert.ToHexString(hash));
            }
        }

        static void UnsafeDeserialization(string json)
        {
            var result = JsonConvert.DeserializeObject<Payload>(json); // CodeQL7
            Console.WriteLine("Command: " + result?.Command);
        }
    }
}