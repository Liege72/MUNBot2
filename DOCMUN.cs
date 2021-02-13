using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Official_Bot.Modules
{
    public class DOCMUN : ModuleBase<SocketCommandContext>
    {

        [Command("motion")]
        public async Task Motion(string motion = null, [Remainder] string extra = null)
        {
            var user = Context.User as SocketGuildUser;
            var motionChannel = Context.Guild.GetTextChannel(765246917345017856);
            //var pointChannel = Context.Guild.GetTextChannel(765246959396454400);

            var success = new EmbedBuilder();
            success.WithTitle("Success");
            success.WithDescription("Submitted your request!");
            success.WithColor(Color.Green);

            if (motion == null)
            {
                var nomotion = new EmbedBuilder();
                nomotion.WithTitle("Error");
                nomotion.WithDescription("Please provide a motion type!");
                nomotion.WithColor(Color.Red);
                await ReplyAsync("", false, nomotion.Build());
                return;
            }
            else
            {
                if (motion.Equals("open"))
                {
                    await ReplyAsync("", false, success.Build());

                    var embed = new EmbedBuilder();
                    embed.WithTitle("Motion to Open the Speaker’s List");
                    //embed.WithDescription("Open the speakers list. Usually used at the beginning of each session.");
                    embed.WithAuthor(user);
                    embed.WithCurrentTimestamp();
                    embed.WithColor(Color.Blue);
                    await motionChannel.SendMessageAsync("", false, embed.Build());
                    return;
                }

                if (motion.Equals("table"))
                {
                    await ReplyAsync("", false, success.Build());

                    var embed = new EmbedBuilder();
                    embed.WithTitle("Motion to Table the Topic");
                    //embed.WithDescription("End the current topic/discussion. Move on the the next topic on the agenda.");
                    embed.WithAuthor(user);
                    embed.WithCurrentTimestamp();
                    embed.WithColor(Color.Blue);
                    await motionChannel.SendMessageAsync("", false, embed.Build());
                    return;
                }

                if (motion.Equals("close"))
                {
                    await ReplyAsync("", false, success.Build());

                    var embed = new EmbedBuilder();
                    embed.WithTitle("Motion to Close the Speaker’s List");
                    //embed.WithDescription("Close the current speakers list. Used to close the current discussion.");
                    embed.WithAuthor(user);
                    embed.WithCurrentTimestamp();
                    embed.WithColor(Color.Blue);
                    await motionChannel.SendMessageAsync("", false, embed.Build());
                    return;
                }

                if (motion.Equals("vote"))
                {
                    await ReplyAsync("", false, success.Build());

                    var embed = new EmbedBuilder();
                    embed.WithTitle("Motion to Close the Debate and Move into Voting Procedure");
                    //embed.WithDescription("End current discussion and move on to voting.");
                    embed.WithAuthor(user);
                    embed.WithCurrentTimestamp();
                    embed.WithColor(Color.Blue);
                    await motionChannel.SendMessageAsync("", false, embed.Build());
                    return;
                }

                if (motion.Equals("recess"))
                {
                    await ReplyAsync("", false, success.Build());

                    var embed = new EmbedBuilder();
                    embed.WithTitle("Motion to Recess");
                    //embed.WithDescription("Pause the current session for a brief recess.");
                    embed.WithAuthor(user);
                    embed.WithCurrentTimestamp();
                    embed.WithColor(Color.Blue);
                    await motionChannel.SendMessageAsync("", false, embed.Build());
                    return;
                }
            }

            if(extra != null)
            {
                if (motion.Equals("mod"))
                {
                    await ReplyAsync("", false, success.Build());

                    var embed = new EmbedBuilder();
                    embed.WithTitle("Motion for a Moderated Caucus");
                    //embed.WithDescription("Begin a discussion moderated by the chairs. Commonly used during sessions.");
                    embed.AddField("Duration", $"{extra}");
                    embed.WithAuthor(user);
                    embed.WithCurrentTimestamp();
                    embed.WithColor(Color.Blue);
                    await motionChannel.SendMessageAsync("", false, embed.Build());
                    return;
                }

                if (motion.Equals("unmod"))
                {
                    await ReplyAsync("", false, success.Build());

                    var embed = new EmbedBuilder();
                    embed.WithTitle("Motion for an Unmoderated Caucus");
                    //embed.WithDescription("Begin an open discussion not moderated by the chairs. Also commonly used during sessions.");
                    embed.AddField("Duration", $"{extra}");
                    embed.WithAuthor(user);
                    embed.WithCurrentTimestamp();
                    embed.WithColor(Color.Blue);
                    await motionChannel.SendMessageAsync("", false, embed.Build());
                    return;
                }

                if (motion.Equals("agenda"))
                {
                    if (extra == null)
                    {
                        var error1 = new EmbedBuilder();
                        error1.WithTitle("Error");
                        error1.WithDescription("Please provide what you would like to add to the agenda!");
                        error1.WithColor(Color.Red);
                        await ReplyAsync("", false, error1.Build());
                        return;
                    }
                    await ReplyAsync("", false, success.Build());

                    var embed = new EmbedBuilder();
                    embed.WithTitle("Motion to Set the Agenda");
                    //embed.WithDescription("Set the order in which topics will be discussed. Usually used after debate.");
                    embed.AddField("Adding", extra);
                    embed.WithAuthor(user);
                    embed.WithCurrentTimestamp();
                    embed.WithColor(Color.Blue);
                    await motionChannel.SendMessageAsync("", false, embed.Build());
                    return;
                }

                if (motion.Equals("paper"))
                {
                    if (extra == null)
                    {
                        var error1 = new EmbedBuilder();
                        error1.WithTitle("Error");
                        error1.WithDescription("Please provide a link!");
                        error1.WithColor(Color.Red);
                        await ReplyAsync("", false, error1.Build());
                        return;
                    }
                    if (!extra.Contains("docs.google.com"))
                    {
                        var error1 = new EmbedBuilder();
                        error1.WithTitle("Error");
                        error1.WithDescription("Please provide a valid link!");
                        error1.WithColor(Color.Red);
                        await ReplyAsync("", false, error1.Build());
                        return;
                    }

                    await ReplyAsync("", false, success.Build());

                    var embed = new EmbedBuilder();
                    embed.WithTitle("Motion to Introduce Working Paper/Resolution/Amendment");
                    //embed.WithDescription("Ask to introduce “Working Paper/Resolution/Amendment” to the committee.");
                    embed.AddField("Paper", $"[View Paper]({extra})");
                    embed.WithAuthor(user);
                    embed.WithCurrentTimestamp();
                    embed.WithColor(Color.Blue);
                    await motionChannel.SendMessageAsync("", false, embed.Build());
                    return;
                }
            }

            var error = new EmbedBuilder();
            error.WithTitle("Error");
            error.WithDescription("There was an error, please refer to the DOCMUN Member Guide!");
            error.WithColor(Color.Red);
            await ReplyAsync("", false, error.Build());
            return;
        }

        [Command("motion reply")]
        public async Task MotionReply(SocketGuildUser userAccount = null)
        {
            var user = Context.User as SocketGuildUser;
            var motionChannel = Context.Guild.GetTextChannel(765246917345017856);
            //var pointChannel = Context.Guild.GetTextChannel(765246959396454400);

            var success = new EmbedBuilder();
            success.WithTitle("Success");
            success.WithDescription("Submitted your request!");
            success.WithColor(Color.Green);
            await ReplyAsync("", false, success.Build());

            var embed = new EmbedBuilder();
            embed.WithTitle("Motion for a Right of Reply");
            embed.WithDescription("Ask to reply to a delegate. Usually to counter something they said.");
            embed.AddField("Reply To", user);
            embed.WithAuthor(user);
            embed.WithCurrentTimestamp();
            embed.WithColor(Color.Blue);
            await motionChannel.SendMessageAsync("", false, embed.Build());
        }

        [Command("help motions")]
        public async Task HelpMotions()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Motions Help");
            embed.AddField("Motion to Open the Speaker’s List", "Open the speakers list. Usually used at the beginning of each session.\n**Usage:** `!motion open`");
            embed.AddField("Motion to Set the Agenda", "Set the order in which topics will be discussed. Usually used after debate.\n**Usage:** `!motion agenda`");
            embed.AddField("Motion for a Moderated Caucus", "Begin a discussion moderated by the chairs. Commonly used during sessions.\n**Usage:** `!motion mod <time>`");
            embed.AddField("Motion for an Unmoderated Caucus", "Begin an open discussion not moderated by the chairs. Also commonly used during sessions.\n**Usage:** `!motion unmod <time>`");
            embed.AddField("Motion for a Right of Reply", "Ask to reply to a delegate. Usually to counter something they said.\n**Usage:** `!motion reply <user>`");
            embed.AddField("Motion to Introduce Working Paper/Resolution/Amendment", "Ask to introduce “Working Paper/Resolution/Amendment” to the committee.\n**Usage:** `!motion paper`");
            embed.AddField("Motion to Table the Topic", "End the current topic/discussion. Move on the the next topic on the agenda.\n**Usage:** `!motion table`");
            embed.AddField("Motion to Close the Speaker’s List", "Close the current speakers list. Used to close the current discussion.\n**Usage:** `!motion close`");
            embed.AddField("Motion to Close the Debate and Move into Voting Procedure", "End current discussion and move on to voting.\n**Usage:** `!motion vote`");
            embed.AddField("Motion to Recess", "Pause the current session for a brief recess.");
            embed.WithColor(Color.Blue);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("point")]
        public async Task Point(string point)
        {
            var user = Context.User as SocketGuildUser;
            var pointsChannel = Context.Guild.GetTextChannel(765246959396454400);
            //var pointChannel = Context.Guild.GetTextChannel(765246959396454400);

            var success = new EmbedBuilder();
            success.WithTitle("Success");
            success.WithDescription("Submitted your request!");
            success.WithColor(Color.Green);

            if (point == null)
            {
                var nopoint = new EmbedBuilder();
                nopoint.WithTitle("Error");
                nopoint.WithDescription("Please provide a point type!");
                nopoint.WithColor(Color.Red);
                await ReplyAsync("", false, nopoint.Build());
                return;
            }

            if (point.Equals("inquiry"))
            {
                await ReplyAsync("", false, success.Build());

                var embed = new EmbedBuilder();
                embed.WithTitle("Point of Inquiry");
                //embed.WithDescription("Clarify rules/procedures.");
                embed.WithAuthor(user);
                embed.WithCurrentTimestamp();
                embed.WithColor(Color.Blue);
                await pointsChannel.SendMessageAsync("", false, embed.Build());
                return;
            }

            if (point.Equals("clarify"))
            {
                await ReplyAsync("", false, success.Build());

                var embed = new EmbedBuilder();
                embed.WithTitle("Point of Clarification");
                //embed.WithDescription("Used to clarify a previous statement.");
                embed.WithAuthor(user);
                embed.WithCurrentTimestamp();
                embed.WithColor(Color.Blue);
                await pointsChannel.SendMessageAsync("", false, embed.Build());
                return;
            }

            if (point.Equals("info"))
            {
                await ReplyAsync("", false, success.Build());

                var embed = new EmbedBuilder();
                embed.WithTitle("Point of Information");
                //embed.WithDescription("Used to question a delegate regarding his/her speech.");
                embed.WithAuthor(user);
                embed.WithCurrentTimestamp();
                embed.WithColor(Color.Blue);
                await pointsChannel.SendMessageAsync("", false, embed.Build());
                return;
            }

            if (point.Equals("order"))
            {
                await ReplyAsync("", false, success.Build());

                var embed = new EmbedBuilder();
                embed.WithTitle("Point of Order");
                //embed.WithDescription("Point out a violation of rules/procedures.");
                embed.WithAuthor(user);
                embed.WithCurrentTimestamp();
                embed.WithColor(Color.Blue);
                await pointsChannel.SendMessageAsync("", false, embed.Build());
                return;
            }

            var error = new EmbedBuilder();
            error.WithTitle("Error");
            error.WithDescription("There was an error, please refer to the DOCMUN Member Guide!");
            error.WithColor(Color.Red);
            await ReplyAsync("", false, error.Build());
            return;
        }

        [Command("help points")]
        public async Task HelpPoints()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Points Help");
            embed.AddField("Point of Inquiry", "Clarify rules/procedures.\n**Usage:** `!point inquiry`");
            embed.AddField("Point of Clarification", "Used to clarify a previous statement.\n**Usage:** `!point clarify`");
            embed.AddField("Point of Information", "Used to question a delegate regarding his/her speech.\n**Usage:** `!point info`");
            embed.AddField("Point of Order", "Point out a violation of rules/procedures.\n**Usage:** `!point order`");
            embed.WithColor(Color.Blue);
            await ReplyAsync("", false, embed.Build());
        }
    }
}
