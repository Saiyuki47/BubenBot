using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace BubenBot;

public static class LoggingService
{
    public static Task LogAsync(LogMessage message)
    {
        if (message.Exception is CommandException cmdException)
        {
            var commandName = cmdException.Command?.Aliases.FirstOrDefault() ?? "unknown";
            Console.WriteLine($"[Command/{message.Severity}] {commandName} failed to execute in {cmdException.Context.Channel}.");
            Console.WriteLine(cmdException);
        }
        else
        {
            Console.WriteLine($"[General/{message.Severity}] {message}");
        }

        return Task.CompletedTask;
    }
}