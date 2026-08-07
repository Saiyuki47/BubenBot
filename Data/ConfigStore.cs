using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord;
using BubenBot.Infrastructure;

namespace BubenBot.Data;

public sealed class ConfigStore
{
    public async Task<ConfigModel> LoadAsync()
    {
        if (!File.Exists(BotPaths.ConfigFilePath))
        {
            var defaults = CreateDefault();
            var json = JsonConvert.SerializeObject(defaults, Formatting.Indented);
            await File.WriteAllTextAsync(BotPaths.ConfigFilePath, json, new UTF8Encoding(false));
            await LoggingService.LogAsync(new LogMessage(LogSeverity.Warning, "Config", $"No config file found. A new one was generated at {BotPaths.ConfigFilePath}"));
            return defaults;
        }

        var content = await File.ReadAllTextAsync(BotPaths.ConfigFilePath, new UTF8Encoding(false));
        return JsonConvert.DeserializeObject<ConfigModel>(content) ?? CreateDefault();
    }

    private static ConfigModel CreateDefault() => new()
    {
        Token = "Null",
        Prefix = "!",
        Status = "Change me",
        Activity = ActivityType.Playing,
        TargetUserName = "Target UserName"
    };
}
