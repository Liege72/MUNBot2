//PHOENIXBOT PROGRAM
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using MUNBot.Services;
using System;
using System.Threading.Tasks;

namespace MUNBot
{
    class Program
    {
        // setup our fields we assign later
        private DiscordSocketClient _client;

        public int AnnounceJoinedUser { get; private set; }

        static void Main(string[] args)
        {

            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            // call ConfigureServices to create the ServiceCollection/Provider for passing around the services
            //using (var services = ConfigureServices())
            {
                // get the client and assign to client 
                // you get the services via GetRequiredService<T>
                var client = new DiscordSocketClient();
                _client = client;

                // setup logging and the ready event
                client.Log += LogAsync;
                client.Ready += ReadyAsync;
                var commandServeice = new CommandService();
                commandServeice.Log += LogAsync;

                // this is where we get the Token value from the configuration file, and start the bot
                //await client.LoginAsync(TokenType.Bot, _config["Token"]);
                await client.LoginAsync(TokenType.Bot, "NzYwNjk5MTIzNjk4NDk5NTk0.X3P2RA.BEZqcIONfsZIr-cophBeySOQA50");
                await client.StartAsync();
                
                //Set the game name and 
                string gameName = "!help | mundiscord.com";
                await _client.SetGameAsync(gameName);
                await _client.SetStatusAsync(UserStatus.Online);

                // we get the CommandHandler class here and call the InitializeAsync method to start things up for the CommandHandler service
                var handler = new CommandHandler(commandServeice, client);
                

                await Task.Delay(-1);
            }
        }


        /*private object client;
        private Task NewUser()
        {
            client.UserJoined += AnnounceJoinedUser;
        }*/

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            Console.WriteLine($"Connected as -> {Environment.UserName} :)");
            Console.WriteLine("[" + DateTime.Now.TimeOfDay + "] - " + $"Welcome User!");//"Welcome, " + Environment.UserName + ".");
            return Task.CompletedTask;
        }

        // this method handles the ServiceCollection creation/configuration, and builds out the service provider we can call on later
        //private ServiceProvider ConfigureServices()
        //{
        //    // this returns a ServiceProvider that is used later to call for those services
        //    // we can add types we have access to here, hence adding the new using statement:
        //    // using csharpi.Services;
        //    // the config we build is also added, which comes in handy for setting the command prefix!
        //    return new ServiceCollection()
        //        .AddSingleton(_config)
        //        .AddSingleton<DiscordSocketClient>()
        //        .AddSingleton<CommandService>()
        //        .AddSingleton<CommandHandler>()
        //        .AddSingleton<InteractiveService>()
        //        .BuildServiceProvider();
        //}
    }
}