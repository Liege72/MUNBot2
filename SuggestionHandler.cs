using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Template.Common;

namespace MUNBot.Modules
{
    public class SuggestionHandler : ModuleBase<SocketCommandContext>
    {
        [Command("suggest")]
        public async Task Suggest([Remainder] string suggestion)
        {
            if (suggestion == null)
            {
                await Context.Channel.SendErrorAsync("Please provide a suggestion!");
                return;
            }

            var suggestionsChannel = Context.Guild.GetTextChannel(631926875400437822);

            var msg = await suggestionsChannel.SendMessageAsync("Creating suggestion...");
            await msg.ModifyAsync(x =>
            {
                x.Content = "";
                x.Embed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = "Unreviewed",
                        IconUrl = "https://cdn.discordapp.com/emojis/787036714337566730.png?v=1"
                    },

                    Description = suggestion,

                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"Submitted by: {Context.User} | Suggestion Id: {msg.Id}",
                    },

                    Color = Color.DarkGrey
                }.Build();
            });

            await Context.Channel.SendSuccessAsync($"Created your suggestion in {suggestionsChannel.Mention}");

            var approveReaction = Emote.Parse("<:Approved:787034785583333426>");
            var denyReaction = Emote.Parse("<:Denied:787035973287542854>");
            await msg.AddReactionAsync(approveReaction);
            await msg.AddReactionAsync(denyReaction);
        }

        [Command("approve")]
        public async Task Approve(ulong suggestionId, [Remainder] string reason)
        {
            var suggestionsChannel = Context.Guild.GetTextChannel(631926875400437822);
            var staffRole = Context.Guild.GetRole(629698730509074462);
            var supervisorRole = Context.Guild.GetRole(700057375394234399);
            var user = Context.User as SocketGuildUser;

            if (!user.Roles.Contains(staffRole) && !user.Roles.Contains(supervisorRole))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            if (suggestionId == 0)
            {
                await Context.Channel.SendErrorAsync("Please provide a Suggestion Id!");
                return;
            }

            if (reason == null)
            {
                await Context.Channel.SendErrorAsync("Please provide a reason!");
                return;
            }

            await Context.Message.DeleteAsync();

            var msg = await suggestionsChannel.GetMessageAsync(suggestionId);
            var getMessage = (IUserMessage)msg;
            var getEmbed = getMessage.Embeds.First();
            var modifyEmbed = getEmbed.ToEmbedBuilder().WithAuthor("Approved", "https://cdn.discordapp.com/emojis/787034785583333426.png?v=1").AddField("Reason", reason).WithColor(Color.Green).Build();
            await getMessage.ModifyAsync(x => x.Embed = modifyEmbed);
            var embed = modifyEmbed.ToEmbedBuilder();
        }

        [Command("deny")]
        public async Task Deny(ulong suggestionId, [Remainder] string reason)
        {
            var suggestionsChannel = Context.Guild.GetTextChannel(631926875400437822);
            var staffRole = Context.Guild.GetRole(629698730509074462);
            var supervisorRole = Context.Guild.GetRole(700057375394234399);
            var user = Context.User as SocketGuildUser;

            if (!user.Roles.Contains(staffRole) && !user.Roles.Contains(supervisorRole))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            if (suggestionId == 0)
            {
                await Context.Channel.SendErrorAsync("Please provide a Suggestion Id!");
                return;
            }

            if (reason == null)
            {
                await Context.Channel.SendErrorAsync("Please provide a reason!");
                return;
            }

            await Context.Message.DeleteAsync();

            var msg = await suggestionsChannel.GetMessageAsync(suggestionId);
            var getMessage = (IUserMessage)msg;
            var getEmbed = getMessage.Embeds.First();
            var modifyEmbed = getEmbed.ToEmbedBuilder().WithAuthor("Denied", "https://cdn.discordapp.com/emojis/787035973287542854.png?v=1").AddField("Reason", reason).WithColor(Color.Red).Build();
            await getMessage.ModifyAsync(x => x.Embed = modifyEmbed);
            var embed = modifyEmbed.ToEmbedBuilder();
        }
    }
}