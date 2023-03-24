using Newtonsoft.Json;
using System;
using System.IO;

namespace Demo_Core
{
    class Program
    {
        static void Main(string[] args)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string friendlyName = AppDomain.CurrentDomain.FriendlyName;
            string dir= basePath.Split(friendlyName)[0];
            string json= File.ReadAllText($"{dir}/{friendlyName}/appsettings.json");
            dynamic dynamic= JsonConvert.DeserializeObject<dynamic>(json);
            string key = "MainDb";
            Console.WriteLine(dynamic.ConnectionStrings[key]);
            Console.ReadKey();
        }
    }
}
