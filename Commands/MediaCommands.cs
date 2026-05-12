using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using BubenBot.Infrastructure;

namespace BubenBot.Commands;

public sealed class MediaCommands : CommandModuleBase
{
    [Command("madi", RunMode = RunMode.Async)]
    public async Task JoinChannel([Remainder] IVoiceChannel? channel = null)
    {
        channel ??= (Context.User as IGuildUser)?.VoiceChannel;
        if (channel is null)
        {
            await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument.");
            return;
        }

        await EnsureDirectoryExistsAsync(
            BotPaths.VoiceLinesDirectory,
            "VoiceLines",
            "VoiceLines Folder found :D",
            "No VoiceLines Folder found! A new one was generated");

        var files = Directory.GetFiles(BotPaths.VoiceLinesDirectory, "*.mp3");
        if (files.Length == 0)
        {
            await Context.Channel.SendMessageAsync("No voice lines were found.");
            return;
        }

        var audioClient = await channel.ConnectAsync();
        await SendAsync(audioClient, files[Random.Shared.Next(files.Length)]);
        await channel.DisconnectAsync();
    }

    [Command("meme")]
    public async Task Meme(long index = 0)
    {
        await EnsureDirectoryExistsAsync(
            BotPaths.MemesDirectory,
            "Memes",
            "Memes folder found :D",
            "No memes folder found! A new one was generated");

        var files = Directory.GetFiles(BotPaths.MemesDirectory);
        if (files.Length == 0)
        {
            await ReplyAsync(embed: await EmbedHandler.CreateErrorEmbed("MEME Command", "No memes are available"));
            return;
        }

        if (index > files.Length)
        {
            await ReplyAsync(embed: await EmbedHandler.CreateErrorEmbed("MEME Command", "Number exceeds the number of memes available"));
            return;
        }

        var sends = index > 0 ? index : 1;
        for (var i = 0; i < sends; i++)
        {
            await Context.Channel.SendFileAsync(files[Random.Shared.Next(files.Length)]);
        }
    }

    private static Process StartFfmpeg(string path)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true
        });

        return process ?? throw new InvalidOperationException("Failed to start ffmpeg.");
    }

    private static async Task SendAsync(IAudioClient client, string path)
    {
        using var ffmpeg = StartFfmpeg(path);
        await using var output = ffmpeg.StandardOutput.BaseStream;
        await using var discord = client.CreatePCMStream(AudioApplication.Mixed);

        try
        {
            await output.CopyToAsync(discord);
        }
        finally
        {
            await discord.FlushAsync();
        }
    }
}
