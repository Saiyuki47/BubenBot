using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Victoria;
using System.IO;
using BubenBot; 

namespace BubenBot
{
    class Program
    {
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _commandService;
        private IServiceProvider _services;
        private Config _config;
        private Commands _commands;


        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commandService = new CommandService();
            _config = new Config();
            _commands = new Commands();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commandService)
                .BuildServiceProvider();

            await InitializeConfigDataAsync();

            await InitializeVoiceLinesFolderAsync();

            _client.Log += _client_Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, Config.ConfigProperties.Token);

            await _client.SetActivityAsync(new Game(Config.ConfigProperties.Status, Config.ConfigProperties.Activity));

            await _client.StartAsync();

            await Task.Delay(-1);

        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }



        public async Task InitializeVoiceLinesFolderAsync()
        {
            await _commands.InitializeVoiceLinesFolder();
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            if (message.Author.IsBot) return;

            int argPos = 0;
            if (message.HasStringPrefix(Config.ConfigProperties.Prefix, ref argPos))
            {
                var result = await _commandService.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
                if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        private async Task InitializeConfigDataAsync()
        {
            await _config.InitializeConfigData();
        }







    }
}
