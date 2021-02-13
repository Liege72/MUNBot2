using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.Interactive;

namespace MUNBot.Modules
{
    public class BotCommands : InteractiveBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task ping()
        {
            var ping = new EmbedBuilder();
            ping.WithTitle("Pinging...");
            ping.WithColor(Color.Blue);
            var msg = await ReplyAsync("", false, ping.Build());
            await msg.ModifyAsync(x => x.Embed = new EmbedBuilder()
            {
                Description = $"🏓 Pong: `{ Context.Client.Latency}`ms!\n[Check Discord Status](https://status.discord.com)",
                Color = Color.Blue,
            }.Build());
        }

        [Command("help")]
        public async Task Help()
        {
            var uc = Context.Guild.GetRole(626783494508380160);
            //var cc = Context.Guild.GetRole(626783654395379753);
            var user = Context.User as SocketGuildUser;

            if (user.Roles.Contains(uc))
            {
                var embed1 = new EmbedBuilder();
                embed1.WithTitle("Heard you needed help!");
                embed1.WithDescription("You do not have a country yet so mention a staff member in <#626783002797670410> and choose one. Check out <#682667353527549960> for a map of available countries. If you need help with anything else, use the `!ticket` command.");
                embed1.WithColor(Color.Blue);
                await ReplyAsync($"{user.Mention}", false, embed1.Build());
                return;
            }

            var embed = new EmbedBuilder();
            embed.WithTitle("Heard you needed help!");
            embed.WithDescription("If you ever need help, you can try some of the following:\n- Check out the <#629675521357119508> page. It has some easily answered questions.\n- Use the `!ticket` command to get direct support from the staff team.\n- Use the `!ask <question>` command in the <#749257515996414032> channel.");
            embed.WithColor(Color.Blue);
            await ReplyAsync($"{user.Mention}", false, embed.Build());
            return;
        }

        [Command("nextmessage")]
        public async Task Test()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Test Embed");
            embed.WithDescription("This is a test embed");

            var response = await NextMessageAsync();
            var responseContent = response.Content;

            embed.AddField("Field", responseContent);

            await ReplyAsync("", false, embed.Build());


        }

        [Command("embed", RunMode = RunMode.Async)]
        public async Task TaskDeleteAfterAsync()
        {
            var embed = new EmbedBuilder();

            const int delay = 2000;
            await Task.Delay(delay);

            await Context.Channel.DeleteMessageAsync(Context.Message.Id);
            var awaitTitle = await ReplyAsync("What should the title be?");
            var title = await NextMessageAsync();
            embed.WithTitle(title.Content);
            await title.DeleteAsync();            
            await awaitTitle.DeleteAsync();
           
            var awaitDescription = await ReplyAsync("What should the description be?");
            var description = await NextMessageAsync();
            embed.WithDescription(description.Content);
            await description.DeleteAsync();
            await awaitDescription.DeleteAsync();
            await ReplyAsync("", false, embed.Build());
        }

        [Command("paginator")]
        public async Task Test_Paginator()
        {
            var pages = new[] { "Page 1", "Page 2", "Page 3", "aaaaaa", "Page 5" };
            await PagedReplyAsync(pages);
        }

        [Command("autodelete")]
        public async Task AutoDeleteInfo()
        {
            await ReplyAsync("https://canary.discord.com/channels/619319282777456641/619319282777456643/778853110747627520");
        }

        [Command("xuoban")]
        public async Task XuoBanInfo()
        {
            await ReplyAsync("https://canary.discord.com/channels/619319282777456641/629325734791348244/778778699091935253");
        }

        [Command("pins")]
        public async Task PinsInfo()
        {
            await ReplyAsync("https://canary.discord.com/channels/619319282777456641/629325734791348244/773273217582104576");
        }
    }
}
