using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Template.Common
{
    public static class Extensions
    {
        /*public static async Task<IMessage> SendSuccessAsync(this ISocketMessageChannel channel, string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(43, 182, 115))
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                    .WithIconUrl("https://icons-for-free.com/iconfiles/png/512/complete+done+green+success+valid+icon-1320183462969251652.png")
                    .WithName(title);
                })
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }*/

        public static async Task<IMessage> SendErrorAsync(this ISocketMessageChannel channel, string description)
        {
            var embed = new EmbedBuilder()
                .WithAuthor("Command Error", "https://cdn.discordapp.com/emojis/787035973287542854.png?v=1")
                .WithDescription(description)
                .WithColor(Color.Red)
                .Build();
            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }

        public static async Task<IMessage> SendSuccessAsync(this ISocketMessageChannel channel, string description)
        {
            var embed = new EmbedBuilder()
                .WithAuthor("Command Success", "https://cdn.discordapp.com/emojis/787034785583333426.png?v=1")
                .WithDescription(description)
                .WithColor(Color.Green)
                .Build();
            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }

        public static async Task<IMessage> SendInfractionAsync(this ISocketMessageChannel channel, string type, SocketGuildUser userAccount, SocketGuildUser moderator, string reason)
        {
            var embed = new EmbedBuilder()
                .WithAuthor($"{userAccount.Username} was {type}", "https://cdn.discordapp.com/emojis/787034785583333426.png?v=1")
                .AddField("Moderator", moderator.Mention, true)
                .AddField("Reason", reason, true)
                .WithColor(Color.Green)
                .Build();
            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }

        public static async Task<IMessage> ModlogAsync(this ISocketMessageChannel channel1, string type, SocketGuildUser userAccount, string reason, SocketGuildUser moderator, ISocketMessageChannel channel)
        {
            var embed = new EmbedBuilder()
                .WithTitle(type)
                .WithDescription($"**Offender:** {userAccount.Mention}\n**Reason:** {reason}\n**Moderator:** {moderator.Mention}\n**In:** <#{channel.Id}>")
                .WithColor(Color.Red)
                .Build();
            var message = await channel1.SendMessageAsync(embed: embed);
            return message;
        }


    }
}