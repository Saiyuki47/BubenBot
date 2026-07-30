using System.Threading.Tasks;
using BubenBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BubenBot.Commands;

public sealed class VotekickCommands : CommandModuleBase
{
    private readonly VotekickService _votekick;

    public VotekickCommands(VotekickService votekick)
    {
        _votekick = votekick;
    }

    [Command("votekick")]
    [Summary("Startet einen CS:GO-Style Votekick gegen einen Nutzer im Voice-Channel.")]
    [RequireBotPermission(GuildPermission.MoveMembers)]
    public async Task VotekickAsync(IGuildUser? target = null)
    {
        if (target is null)
        {
            await ReplyAsync("Bitte gib einen Nutzer an. Beispiel: `!votekick @Nutzer`");
            return;
        }

        var initiator = (IGuildUser)Context.User;
        var voiceChannel = initiator.VoiceChannel;

        if (voiceChannel is null)
        {
            await ReplyAsync("Du musst in einem Voice-Channel sein, um einen Votekick zu starten.");
            return;
        }

        var error = await _votekick.StartVoteAsync(
            (ITextChannel)Context.Channel,
            voiceChannel,
            initiator,
            target);

        if (error is not null)
        {
            await ReplyAsync(error);
        }
    }
}
