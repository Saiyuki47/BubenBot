﻿using System;
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

namespace BubenBot
{
    class Program
    {
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private Config _config;
        private LavaNode _instanceOfLavaNode;
        private LavaConfig _instanceOfLavaConfig;

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _config = new Config();
            _instanceOfLavaConfig = new LavaConfig();
            _instanceOfLavaNode = new LavaNode(_client, _instanceOfLavaConfig);

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<LavaNode>()
                .AddSingleton<LavaConfig>()
                .AddLavaNode(x => {
                    x.SelfDeaf = false;
                })
                .BuildServiceProvider();

            await InitializeConfigDataAsync();

            _client.Log += _client_Log;

            _client.Ready += OnReadyAsync;

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

        private async Task OnReadyAsync()
        {
            // Avoid calling ConnectAsync again if it's already connected 
            // (It throws InvalidOperationException if it's already connected).
            if (!_instanceOfLavaNode.IsConnected)
            {
                await _instanceOfLavaNode.ConnectAsync();
            }

            // Other ready related stuff
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            if (message.Author.IsBot) return;

            int argPos = 0;
            if (message.HasStringPrefix(Config.ConfigProperties.Prefix, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
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