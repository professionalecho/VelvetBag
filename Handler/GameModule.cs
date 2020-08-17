using Discord.Commands;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using System.Net.Sockets;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using Discord.Net;

namespace DiscordTest
{
    public class GameModule : ModuleBase<SocketCommandContext>
    {
        private static Dictionary<string, Game> _gameMapping = new Dictionary<string, Game>();
        public static Game activeGame;
        
        public GameModule()
        {

        }

        [Command("start")]
        [Summary("Starts a game and counts players.")]
        public async Task Start(string script)
        {
            IGuildUser user = Context.User as IGuildUser;
            bool isST = user.Nickname.StartsWith("!ST");
            if (isST) // Checks that the person typing the command is a Storyteller.
            {
                string st = Context.User.Username + "#" + Context.User.Discriminator;
                if (_gameMapping.TryAdd(st, new Game(Context.User, script.ToLower()))) // Add a new game to the list of games, removed when DMs are sent.
                {
                    activeGame = _gameMapping[st];
                    await activeGame.Start(Context);
                    await ReplyAsync($"Game starting... {activeGame.Players.Count} players ready!");
                    await activeGame.Storyteller.SendMessageAsync("Ready to start! Use !set (and, optionally, any number of mandatory roles) to generate a playset.");
                } else { throw new InvalidOperationException("You already have a setup in progress!"); }
            }
            else { await ReplyAsync("You're not a Storyteller!"); }
        }
        [Command("set")]
        [Summary("Creates a set of roles and players.")]
        public async Task Set(params string[] args)
        {
            string st = Context.User.Username + "#" + Context.User.Discriminator;
            if (_gameMapping.TryGetValue(st, value: out Game thisGame))
            {
                thisGame.Set(args);

                string result = "Roles set:\n";
                foreach (KeyValuePair<IUser, Player> p in thisGame._players)
                {
                    result += $"**{p.Key.Username}** is the {p.Value.Role.Name}";
                    if (p.Value.Role.Name == "Drunk" || p.Value.Role.Name == "Lunatic")
                    {
                        result += $" but thinks they are the {p.Value.KnownRole.Name}";
                    }
                    result += "\n";
                }
                await thisGame.Storyteller.SendMessageAsync(result + "\nUse !send to send DMs to all players, or !set to create a new playset");
            } else { Console.WriteLine("You haven't used !start to start a game yet!"); }
        }
        [Command("send")]
        [Summary("Sends DMs to all players regarding their role.")]
        public async Task Send()
        {
            string st = Context.User.Username + "#" + Context.User.Discriminator;
            if (_gameMapping.TryGetValue(st, value: out Game thisGame))
            {
                foreach (KeyValuePair<IUser, Player> p in thisGame._players)
                {
                    try
                    {
                        await p.Value.User.SendMessageAsync($"You are the {p.Value.KnownRole.Name}!");
                    }
                    catch (HttpException)
                    {
                        await activeGame.Storyteller.SendMessageAsync(p.Value.User.Username + " can't receive a DM (they may not allow DMs from users of this server)");
                    }
                }
                _gameMapping.Remove(st);
            } 
            else { Console.WriteLine("You haven't used !start to start a game yet!"); }
        }
    }
}
