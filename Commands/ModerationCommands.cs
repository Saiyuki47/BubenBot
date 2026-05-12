using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace BubenBot.Commands;

public sealed class ModerationCommands : CommandModuleBase
{
    [Command("ban")]
    [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "Du hast kein Rechte, um Leute zu bannen lol")]
    public async Task BanMember(IGuildUser? user = null, [Remainder] string? reason = null)
    {
        if (user is null)
        {
            await ReplyAsync("Bitte gib einen User an");
            return;
        }

        reason ??= "nicht angegeben";
        await Context.Guild.AddBanAsync(user, 0, reason);

        var embed = new EmbedBuilder()
            .WithDescription($":white_check_mark: {user.Mention} wurde gebannt\n**Grund** {reason}")
            .WithFooter(footer => footer.WithText("Lmao ez").WithIconUrl("https://i.imgur.com/6Bi17B3.png"))
            .Build();

        await ReplyAsync(embed: embed);
    }

    [Command("lsuser")]
    [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "Du hast kein Rechte, um Leute aufzulisten lol")]
    public async Task LsUserAsync()
    {
        foreach (var user in Context.Guild.Users)
        {
            await ReplyAsync($"{user.Username}, {user.Id}");

            if (string.Equals(user.Username, "Sir Salafist", System.StringComparison.OrdinalIgnoreCase))
            {
                await ReplyAsync("Alter Madi ist ja hier");
            }
        }

        await ReplyAsync("done");
    }
}
