using System.Threading.Tasks;
using Discord.Commands;

namespace BubenBot.Commands;

public sealed class FunCommands : CommandModuleBase
{
    [Command("hs")]
    public async Task HS(string? usr = null)
    {
        if (usr is null)
        {
            await ReplyAsync("Bitte gib einen User an (mit oder ohne @)");
            return;
        }

        if (Context.User.Username.Equals(TargetUserName, System.StringComparison.OrdinalIgnoreCase)
            && System.Random.Shared.Next(1, 100) == 1)
        {
            await ReplyAsync("Selber Huso!!!!", isTTS: true);
            return;
        }

        await ReplyAsync($"{usr} ist ein Hurensohn!", isTTS: true);
    }
}
