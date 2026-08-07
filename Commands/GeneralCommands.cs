using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using BubenBot.Infrastructure;

namespace BubenBot.Commands;

public sealed class GeneralCommands : CommandModuleBase
{
    [Command("help")]
    [Remarks("Help")]
    [Summary("Finds all the modules and prints out their summary tags.")]
    public async Task Help()
    {
        var lines = new List<string> { "Hier sind meine Befehle:" };
        lines.AddRange(CommandCatalog.BaseCommands.Select(c => $"-{c}"));

        if (IsTargetOnServer(Context))
        {
            lines.AddRange(CommandCatalog.TargetUserCommands.Select(c => $"-{c}"));
        }

        await ReplyAsync(embed: await EmbedHandler.CreateBasicEmbed("Help", string.Join("\n", lines), Color.Green));
    }

    [Command("ping", RunMode = RunMode.Async)]
    public Task Ping() => ReplyAsync("pong");

    [Command("echo")]
    public Task SayAsync([Remainder] string echo) => ReplyAsync(echo);

    [Command("age")]
    public async Task Age(IGuildUser? user = null)
    {
        var message = user is null
            ? $"Dein Account wurde am {Context.User.CreatedAt} erstellt."
            : $"Der Account von {user.Username} wurde am {user.CreatedAt} erstellt.";

        await ReplyAsync(embed: await EmbedHandler.CreateBasicEmbed("Age", message, Color.Green));
    }
}
