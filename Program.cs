using System;
using System.Reflection;
using System.Threading.Tasks;
using BubenBot.Data;
using BubenBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace BubenBot;

class Program
{
    static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

    private DiscordSocketClient _client = null!;
    private CommandService _commandService = null!;
    private IServiceProvider _services = null!;
    private readonly Config _config = new();

    public async Task RunBotAsync()
    {
        var config = new DiscordSocketConfig
        {
            EnableVoiceDaveEncryption = true,
            GatewayIntents = GatewayIntents.Guilds
                           | GatewayIntents.GuildMessages
                           | GatewayIntents.GuildVoiceStates
                           | GatewayIntents.GuildMessageReactions
                           | GatewayIntents.GuildMembers
                           | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);
        _commandService = new CommandService();

        _services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commandService)
            .AddSingleton<VotekickService>()
            .BuildServiceProvider();

        await _config.InitializeConfigData();

        _client.Log += LogAsync;
        _client.MessageReceived += HandleCommandAsync;

        await _commandService.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);

        await _client.LoginAsync(TokenType.Bot, Config.ConfigProperties.Token);
        await _client.SetActivityAsync(new Game(Config.ConfigProperties.Status, Config.ConfigProperties.Activity));
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private static Task LogAsync(LogMessage arg)
    {
        Console.WriteLine(arg);
        return Task.CompletedTask;
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        if (arg is not SocketUserMessage message || message.Author.IsBot)
        {
            return;
        }

        var context = new SocketCommandContext(_client, message);
        var argPos = 0;

        if (!message.HasStringPrefix(Config.ConfigProperties.Prefix, ref argPos))
        {
            return;
        }

        var result = await _commandService.ExecuteAsync(context, argPos, _services);
        if (!result.IsSuccess)
        {
            Console.WriteLine(result.ErrorReason);
        }

        if (result.Error == CommandError.UnmetPrecondition)
        {
            await message.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
