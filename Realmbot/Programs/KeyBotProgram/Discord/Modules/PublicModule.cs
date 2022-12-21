using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotCore.Structures;
using BotTools;
using Discord;
using Discord.Commands;
using KeyBot;
using KeyBot.States;
using KeyBotProgram.Discord.Services;

namespace KeyBotProgram.Discord.Modules
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        private readonly string[] _confirmationMessages = {
            "Sure can do!",
            "Of course!",
            "No problemo!",
            "Affirmative!",
            "Kk",
            "Yes master!",
            "Ok senpai... UwU"
        };

        [Command("help")]
        [Alias("h")]
        public Task HelpAsync() {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(0x0099ff);
            embed.WithTitle("Key Bot Commands");
            embed.WithAuthor("Linqx", "https://i.imgur.com/9h8EZGU.png", "https://discordapp.com/users/157674479852584960");
            embed.WithDescription($"Commands used to control Key Bot. \nOnly users with the role <@&{CommandHandlingService.KEY_OPENER_ROLE_ID}> can access Key Bot.");
            embed.WithThumbnailUrl("https://i.imgur.com/16UnbrH.png");
            embed.WithFields(
                new EmbedFieldBuilder { Name = "\u200B", Value= "\u200B" },
                new EmbedFieldBuilder { Name = "Help", Value="Shows this command", IsInline = true },
                new EmbedFieldBuilder { Name = "\u200B", Value= "\u200B", IsInline = true },
                new EmbedFieldBuilder { Name = "Open <Key Type>", Value = "Opens the given key", IsInline = true },
                new EmbedFieldBuilder { Name = "Keys", Value = "Shows list of all available keys", IsInline = true },
                new EmbedFieldBuilder { Name = "\u200B", Value = "\u200B", IsInline = true },
                new EmbedFieldBuilder { Name = "Connect <Server Name>", Value = "Connects to the given server", IsInline = true },
                new EmbedFieldBuilder { Name = "Server", Value = "Shows the current server", IsInline = true }

            );
            embed.WithCurrentTimestamp();
            embed.WithFooter("Brought to you by RealmStock", "https://static.drips.pw/rotmg/wiki/Untiered/Crown.png");
            return ReplyAsync(embed: embed.Build());
        }

        // Dependency Injection will fill this value in for us

        [Command("open")]
        [Alias("pop")]
        public Task OpenKeyAsync([Remainder] string key) {
            RealmKeyBot keyBot = DiscordKeyBot.Instance.KeyBot;

            if (keyBot?.CurrentState == null || keyBot.CurrentState.States == KeyBotStates.Initialize) {
                return ReplyAsync("Key Bot is currently initializing.");
            }

            if (keyBot.CurrentState.States != KeyBotStates.AcceptingCommands) {
                return ReplyAsync("Key Bot is busy!.");
            }

            if (!ParseEnum.TryParse(key, out KeyType keyType)) {
                return ReplyAsync("Key Type not found! Use the keys command for a list of available keys.");
            }

            if (!keyBot.KeyToPostion.ContainsKey((ushort)keyType)) {
                return ReplyAsync("Sorry, that key is not available. If you'd like to buy me one, please dm Danny or Evan on discord!");
            }

            keyBot.Reconnect(ConnectionMode.VAULT);
            keyBot.SetState(new GetKeyState(keyBot, keyType));
            return ReplyAsync(_confirmationMessages[Randum.Next(_confirmationMessages.Length)]);
        }

        [Command("connect")]
        [Alias("conn", "con")]
        public Task ConnectAsync([Remainder] string svr)
        {
            RealmKeyBot keyBot = DiscordKeyBot.Instance.KeyBot;

            if (keyBot?.CurrentState == null || keyBot.CurrentState.States == KeyBotStates.Initialize) {
                return ReplyAsync("Key Bot is currently initializing.");
            }

            if (keyBot.CurrentState.States != KeyBotStates.AcceptingCommands) {
                return ReplyAsync("Key Bot is busy!.");
            }

            if (!ParseEnum.TryParse(svr, out Server server)) {
                return ReplyAsync("Invalid server name!");
            }

            keyBot.Reconnect(server, ConnectionMode.VAULT);
            keyBot.SetState(new EnterGuildHallState(keyBot));
            return ReplyAsync($"Roger! Connecting to: {server.ToString()} ({EnumHelper.GetStringValue(server)})");
        }

        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("pong!");
    }
}
