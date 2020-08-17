using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordTest
{
	public class Program
	{
		private readonly DiscordSocketClient _client;
        private IConfiguration _config;
        private readonly CommandHandler _commandHandler;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public async static Task Main(string[] args)
        {
            Program prog = new Program();
            await prog.MainAsync();
        }

        public Program()
        {
            _client = new DiscordSocketClient();
            _config = BuildConfig();
            _client.Log += Log;
            _commands = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false
            });

            _services = ConfigureServices();
            _commandHandler = new CommandHandler(_client, _commands, _services);
        }

		public async Task MainAsync()
		{
            await _commandHandler.InstallCommandsAsync();
			// Remember to keep token private or to read it from an 
			// external source! In this case, we are reading the token 
			// from an environment variable. If you do not know how to set-up
			// environment variables, you may find more information on the 
			// Internet or by using other methods such as reading from 
			// a configuration.
			await _client.LoginAsync(TokenType.Bot, _config["token"]);
			await _client.StartAsync();
            
			// Block this task until the program is closed.
			await Task.Delay(-1);
        }

        private static IServiceProvider ConfigureServices()
        {
            var map = new ServiceCollection();

            return map.BuildServiceProvider();
        }

        private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
        }
        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();
        }
    }
}
