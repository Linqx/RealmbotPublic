using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KeyBot;
using Discord.WebSocket;
using KeyBotProgram.Discord.Services;
using Microsoft.Extensions.DependencyInjection;
using Rekishi;

namespace KeyBotProgram.Discord
{
    public class DiscordKeyBot {

        private const string BOT_TOKEN = "";

        public static DiscordKeyBot Instance;

        public RealmKeyBot KeyBot;

        public DiscordKeyBot(RealmKeyBot keyBot) {
            Instance = this;
            KeyBot = keyBot;
            Task.Factory.StartNew(MainAsync, TaskCreationOptions.LongRunning);
        }

        public async Task MainAsync() {
            using (ServiceProvider services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();

                client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                await client.LoginAsync(TokenType.Bot, BOT_TOKEN);
                await client.StartAsync();

                // Here we initialize the logic required to register our commands.
                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

                await Task.Delay(-1);
            }
        }

        private Task LogAsync(LogMessage log) {
            if (log.Exception != null) {
                Log.Error($"[DiscordKeyBot] {log.Exception}");
            }
            else {
                Log.Write($"[DiscordKeyBot] {log.Message}");
            }

            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .BuildServiceProvider();
        }
    }
}
