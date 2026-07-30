using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace BubenBot.Services;

/// <summary>
/// Manages CS:GO-style votekick sessions for voice channels.
/// Any member of the same voice channel can start a votekick.
/// A kick passes when >= 60% of the eligible voters have voted Yes
/// and at least 2 votes have been cast.
/// The vote expires after 60 seconds if no majority is reached.
/// </summary>
public sealed class VotekickService
{
    private const string YesEmote = "✅";
    private const string NoEmote  = "❌";
    private const int VoteTimeoutSeconds = 60;
    private const double PassThreshold = 0.6;
    private const int MinVotesToPass = 2;

    private readonly DiscordSocketClient _client;

    // Key: channel message ID of the vote message
    private readonly ConcurrentDictionary<ulong, VoteSession> _sessions = new();

    public VotekickService(DiscordSocketClient client)
    {

        _client = client;
        _client.ReactionAdded   += OnReactionAddedAsync;
        _client.ReactionRemoved += OnReactionRemovedAsync;
    }

    // ── Public API ──────────────────────────────────────────────────────────

    /// <summary>
    /// Starts a votekick for <paramref name="target"/> in the given voice channel.
    /// Returns an error string when the call is invalid, otherwise null.
    /// </summary>
    public async Task<string?> StartVoteAsync(
        ITextChannel textChannel,
        IVoiceChannel voiceChannel,
        IGuildUser initiator,
        IGuildUser target)
    {
        if (target.Id == initiator.Id)
            return "Du kannst nicht gegen dich selbst voten.";

        if (target.IsBot)
            return "Bots können nicht gekickt werden.";

        // Initiator must be in the same voice channel
        if (initiator.VoiceChannel?.Id != voiceChannel.Id)
            return "Du musst im selben Voice-Channel wie das Ziel sein.";

        // Prevent duplicate votes for the same target
        if (_sessions.Values.Any(s => s.VoiceChannelId == voiceChannel.Id && s.TargetId == target.Id))
            return $"Gegen {target.Mention} läuft bereits eine Abstimmung.";

        // Eligible voters = everyone in the voice channel except the target and bots
        var eligibleVoters = (await voiceChannel.GetUsersAsync().FlattenAsync())
            .Where(u => !u.IsBot && u.Id != target.Id)
            .Select(u => u.Id)
            .ToHashSet();

        if (eligibleVoters.Count == 0)
            return "Nicht genug Spieler im Channel für ein Votekick.";

        var embed = BuildVoteEmbed(target, initiator, 0, 0, eligibleVoters.Count, VoteTimeoutSeconds);
        var message = await textChannel.SendMessageAsync(embed: embed);

        await message.AddReactionAsync(new Emoji(YesEmote));
        await message.AddReactionAsync(new Emoji(NoEmote));

        var cts = new CancellationTokenSource();
        var session = new VoteSession(
            messageId:      message.Id,
            textChannelId:  textChannel.Id,
            voiceChannelId: voiceChannel.Id,
            guildId:        textChannel.GuildId,
            targetId:       target.Id,
            targetMention:  target.Mention,
            initiatorId:    initiator.Id,
            eligibleVoters: eligibleVoters,
            cts:            cts);

        _sessions[message.Id] = session;

        // Expire automatically after timeout
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(VoteTimeoutSeconds), cts.Token);
                await ExpireSessionAsync(session);
            }
            catch (TaskCanceledException) { /* vote resolved early */ }
        }, cts.Token);

        return null;
    }

    // ── Reaction handlers ────────────────────────────────────────────────────

    private async Task OnReactionAddedAsync(
        Cacheable<IUserMessage, ulong> cacheableMessage,
        Cacheable<IMessageChannel, ulong> cacheableChannel,
        SocketReaction reaction)
    {
        if (reaction.UserId == _client.CurrentUser.Id) return;
        if (!_sessions.TryGetValue(reaction.MessageId, out var session)) return;
        if (!session.EligibleVoters.Contains(reaction.UserId)) return;

        var emote = reaction.Emote.Name;
        if (emote != YesEmote && emote != NoEmote) return;

        session.RecordVote(reaction.UserId, emote == YesEmote);
        await UpdateOrResolveAsync(session, await cacheableMessage.GetOrDownloadAsync());
    }

    private async Task OnReactionRemovedAsync(
        Cacheable<IUserMessage, ulong> cacheableMessage,
        Cacheable<IMessageChannel, ulong> cacheableChannel,
        SocketReaction reaction)
    {
        if (reaction.UserId == _client.CurrentUser.Id) return;
        if (!_sessions.TryGetValue(reaction.MessageId, out var session)) return;

        session.RemoveVote(reaction.UserId);
        await UpdateOrResolveAsync(session, await cacheableMessage.GetOrDownloadAsync());
    }

    // ── Resolution logic ────────────────────────────────────────────────────

    private async Task UpdateOrResolveAsync(VoteSession session, IUserMessage? message)
    {
        if (message is null) return;

        var (yes, no, total) = session.Counts;
        double ratio = total == 0 ? 0 : (double)yes / total;

        // Pass: >= threshold AND minimum votes reached
        if (yes >= MinVotesToPass && ratio >= PassThreshold)
        {
            await ResolveAsync(session, passed: true, message);
            return;
        }

        // Fail: mathematically impossible to still pass
        int remaining = session.EligibleVoters.Count - total;
        if (no > 0 && (yes + remaining) < MinVotesToPass ||
            (double)(yes + remaining) / session.EligibleVoters.Count < PassThreshold)
        {
            await ResolveAsync(session, passed: false, message);
            return;
        }

        // Still undecided — update the embed
        var embed = BuildVoteEmbed(null, null, yes, no, session.EligibleVoters.Count,
            VoteTimeoutSeconds, session.TargetMention);
        await message.ModifyAsync(m => m.Embed = embed);
    }

    private async Task ExpireSessionAsync(VoteSession session)
    {
        if (!_sessions.TryRemove(session.MessageId, out _)) return;

        var guild   = _client.GetGuild(session.GuildId);
        var channel = guild?.GetTextChannel(session.TextChannelId);
        if (channel is null) return;

        var message = await channel.GetMessageAsync(session.MessageId) as IUserMessage;
        if (message is null) return;

        var embed = new EmbedBuilder()
            .WithTitle("Votekick abgelaufen")
            .WithDescription($"Die Abstimmung gegen {session.TargetMention} ist abgelaufen. **Kein Kick.**")
            .WithColor(Color.LightGrey)
            .WithCurrentTimestamp()
            .Build();

        await message.ModifyAsync(m => m.Embed = embed);
        await message.RemoveAllReactionsAsync();
    }

    private async Task ResolveAsync(VoteSession session, bool passed, IUserMessage message)
    {
        if (!_sessions.TryRemove(session.MessageId, out _)) return;
        session.Cts.Cancel();

        var (yes, no, _) = session.Counts;

        if (passed)
        {
            var guild  = _client.GetGuild(session.GuildId);
            var target = guild?.GetUser(session.TargetId);

            if (target?.VoiceChannel is not null)
            {
                // This only disconnects the user from the voice channel.
                // It does NOT kick them from the Discord server.
                await target.ModifyAsync(props => props.Channel = null);
            }

            var embed = new EmbedBuilder()
                .WithTitle("Votekick erfolgreich!")
                .WithDescription($"{session.TargetMention} wurde nur aus dem Voice-Channel entfernt. " +
                                 $"({yes}✅ / {no}❌)")
                .WithColor(Color.Red)
                .WithCurrentTimestamp()
                .Build();

            await message.ModifyAsync(m => m.Embed = embed);
        }
        else
        {
            var embed = new EmbedBuilder()
                .WithTitle("Votekick gescheitert")
                .WithDescription($"Der Votekick gegen {session.TargetMention} ist gescheitert. " +
                                 $"({yes}✅ / {no}❌)")
                .WithColor(Color.Green)
                .WithCurrentTimestamp()
                .Build();

            await message.ModifyAsync(m => m.Embed = embed);
        }

        await message.RemoveAllReactionsAsync();
    }

    // ── Embed builder ────────────────────────────────────────────────────────

    private static Embed BuildVoteEmbed(
        IGuildUser? target,
        IGuildUser? initiator,
        int yes, int no, int eligible, int timeoutSeconds,
        string? targetMention = null)
    {
        var mention = target?.Mention ?? targetMention ?? "Unbekannt";
        var starter = initiator is not null ? $"Gestartet von {initiator.Mention}" : "";

        return new EmbedBuilder()
            .WithTitle("🗳️ CS:GO Votekick")
            .WithDescription(
                $"**Kick {mention}?**\n\n" +
                $"{YesEmote} Ja – {yes} Stimme(n)\n" +
                $"{NoEmote} Nein – {no} Stimme(n)\n\n" +
                $"Berechtigte Wähler: **{eligible}**\n" +
                $"Abstimmung läuft **{timeoutSeconds}s**\n\n" +
                starter)
            .WithColor(Color.Orange)
            .WithCurrentTimestamp()
            .Build();
    }
}

// ── Vote session state ───────────────────────────────────────────────────────

internal sealed class VoteSession
{
    public ulong MessageId      { get; }
    public ulong TextChannelId  { get; }
    public ulong VoiceChannelId { get; }
    public ulong GuildId        { get; }
    public ulong TargetId       { get; }
    public string TargetMention { get; }
    public ulong InitiatorId    { get; }
    public HashSet<ulong> EligibleVoters { get; }
    public CancellationTokenSource Cts { get; }

    // voter ID → true = yes, false = no
    private readonly ConcurrentDictionary<ulong, bool> _votes = new();

    public VoteSession(
        ulong messageId, ulong textChannelId, ulong voiceChannelId,
        ulong guildId, ulong targetId, string targetMention,
        ulong initiatorId, HashSet<ulong> eligibleVoters,
        CancellationTokenSource cts)
    {
        MessageId      = messageId;
        TextChannelId  = textChannelId;
        VoiceChannelId = voiceChannelId;
        GuildId        = guildId;
        TargetId       = targetId;
        TargetMention  = targetMention;
        InitiatorId    = initiatorId;
        EligibleVoters = eligibleVoters;
        Cts            = cts;
    }

    public void RecordVote(ulong userId, bool yes) => _votes[userId] = yes;
    public void RemoveVote(ulong userId) => _votes.TryRemove(userId, out _);

    public (int Yes, int No, int Total) Counts
    {
        get
        {
            var yes = _votes.Values.Count(v => v);
            var no  = _votes.Values.Count(v => !v);
            return (yes, no, yes + no);
        }
    }
}
