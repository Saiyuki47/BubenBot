using BubenBot.Data;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BubenBot
{
    public class Config
    {
        public static string jsonpath { get; set; } = Directory.GetCurrentDirectory() + "\\config.json";
        public static ConfigModel ConfigProperties { get; set; } = StandardConfig();

        public async Task InitializeConfigData()
        {
            string json;

            if (!File.Exists(jsonpath))
            {
                json = JsonConvert.SerializeObject(StandardConfig(), Formatting.Indented);
                File.WriteAllText("config.json", json, new UTF8Encoding(false));
                await LoggingService.LogAsync(new Discord.LogMessage(LogSeverity.Error,"Source","No config File found! A new one was generated"));
            }

            json = File.ReadAllText(jsonpath, new UTF8Encoding(false));
            ConfigProperties = JsonConvert.DeserializeObject<ConfigModel>(json);
        }

        private static ConfigModel StandardConfig() => new ConfigModel
        {
            Token = "Null",
            Prefix = "!",
            Status = "Change me",
            Activity = ActivityType.Playing,
            TargetUserName = "Target UserName"
        };
    }
    
}
