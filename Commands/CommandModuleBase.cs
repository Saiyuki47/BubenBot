using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace BubenBot.Commands;

public abstract class CommandModuleBase : ModuleBase<SocketCommandContext>
{
    protected static string TargetUserName => Config.ConfigProperties.TargetUserName;

    protected static bool IsTargetOnServer(SocketCommandContext context)
        => context.Guild.Users.Any(user => string.Equals(user.Username, TargetUserName, StringComparison.OrdinalIgnoreCase));

    protected static Task EnsureDirectoryExistsAsync(string directoryPath, string logScope, string successMessage, string createdMessage)
    {
        if (Directory.Exists(directoryPath))
        {
            return LoggingService.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Info, logScope, successMessage));
        }

        Directory.CreateDirectory(directoryPath);
        return LoggingService.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Warning, logScope, createdMessage));
    }
}
