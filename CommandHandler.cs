using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using MUNBot.Modules;
using System.Timers;

namespace MUNBot.Services
{
    public class CommandHandler
    {
        // setup fields to be set later in the constructor
        //private readonly IConfiguration _config;
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        public static List<Mute> Mutes = new List<Mute>();
        public Timer t = new Timer();

        // public fields

        public string commandHelp { get; set; }
        public object Global { get; private set; }

        public ulong guildId = 619319282777456641;

        public static bool rpFilter = true;
        public static int autoModMessageCounter = 5;
        private Dictionary<ulong, (int messageCount, ulong channelId)> channelMessageCount = new Dictionary<ulong, (int messageCount, ulong channelId)>();

        class messageCounts
        {
            public int weekCount { get; set; }
            public int monthCount { get; set; }
        }

        public CommandHandler(CommandService service, DiscordSocketClient client)
        {
            // juice up the fields with these services
            // since we passed the services in, we can use GetRequiredService to pass them into the fields set earlier
            //_config = services.GetRequiredService<IConfiguration>();
            _commands = service;
            _client = client;

            // take action when we execute a command
            _commands.CommandExecuted += CommandExecutedAsync;

            // take action when we receive a message (so we can process it, and see if it is a valid command)
            _client.MessageReceived += MessageReceivedAsync;

            //RoleplayChannelsFormatFilters
            _client.MessageReceived += RoleplayChannelsFormatFilter1;
            _client.MessageReceived += AutoModeration;
            //-----------------------------

            _client.UserJoined += _client_UserJoined;

            //_client.RoleUpdated += _client_RoleUpdated;

            _client.ReactionAdded += _client_ReactionAdded;

            //_client.MessageUpdated += MessageUpdated;

            //Audit Logs
            //_client.ChannelCreated += ChannelCreated;
            //_client.ChannelDestroyed += ChannelDeleted;
            //_client.ChannelUpdated += ChannelUpdated;
            //_client.RoleCreated += RoleCreated;
            //_client.RoleDeleted += RoleDeleted;

            t.Interval = 2000;
            t.Start();
            t.Elapsed += _elapsed;

            _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);   
        }

        public void _elapsed(object s, EventArgs a)
        {
            foreach(var item in channelMessageCount)
            {
                if(item.Value.messageCount >= autoModMessageCounter)
                {
                    // Mute the user
                    var userId = item.Key;
                    Task.Run(async () =>
                    {
                        await AutomodMute(userId, item.Value.channelId).ConfigureAwait(false);

                    }).ConfigureAwait(false);
                }
            }
            channelMessageCount.Clear();
        }

        public async Task AutomodMute(ulong userId, ulong channelId)
        {
            var guild = _client.GetGuild(guildId);
            var user = guild.GetUser(userId);

            var muted = guild.GetRole(693504845080035338);
            await user.AddRoleAsync(muted);

            var embed = new EmbedBuilder();
            embed.WithTitle("Autmoderation Mute!");
            embed.WithDescription("MUNBot AutoModeration detected a potential raid!");
            embed.AddField("User", user.Mention, true);
            embed.AddField("Unmute", "To unmute the user, `!unmute <user>`", true);
            embed.WithColor(Color.Green);
            await guild.GetTextChannel(channelId).SendMessageAsync("", false, embed.Build());

            var modlogschannel = _client.GetGuild(guildId).GetTextChannel(709805453655277578);
            var log = new EmbedBuilder();
            log.WithTitle("AutoModeration Mute");
            log.WithDescription($"**Offender:** {user.Mention}\n**Reason:** Detected Raid\n**Moderator:** MUNBot Automoderation\n**In:** {guild.GetTextChannel(channelId).Mention}");
            log.WithColor(Color.Red);
            await modlogschannel.SendMessageAsync("", false, log.Build());
        }

        private async Task MuteHandler()
        {
            List<Mute> Remove = new List<Mute>();

            foreach (var mute in Mutes)
            {
                if (DateTime.Now < mute.End)
                    continue;

                var guild = _client.GetGuild(mute.Guild.Id);

                if (guild.GetRole(mute.Role.Id) == null)
                {
                    Remove.Add(mute);
                    continue;
                }

                var role = guild.GetRole(mute.Role.Id);

                if (guild.GetUser(mute.User.Id) == null)
                {
                    Remove.Add(mute);
                    continue;
                }

                var user = guild.GetUser(mute.User.Id);

                if (role.Position > guild.CurrentUser.Hierarchy)
                {
                    Remove.Add(mute);
                    continue;
                }

                await user.RemoveRoleAsync(mute.Role);
                Remove.Add(mute);
            }

            Mutes = Mutes.Except(Remove).ToList();

            await Task.Delay(1 * 60 * 1000);
            await MuteHandler();
        }
        private async Task _client_ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var staffRole = _client.GetGuild(619319282777456641).GetRole(700057375394234399);
            var supervisorRole = _client.GetGuild(619319282777456641).GetRole(700057375394234399);

            var messageId = message.Id;
            var channelId = channel.Id;   

            var contextChannel = _client.GetGuild(619319282777456641).GetTextChannel(channelId);
            var getMessageUser = await message.GetOrDownloadAsync();
            var author = getMessageUser.Author;

            SocketGuildUser user = _client.GetGuild(619319282777456641).GetUser(reaction.UserId);
            SocketGuildUser userAccount = _client.GetGuild(619319282777456641).GetUser(author.Id);

            string messageLink = $"https://discord.com/channels/619319282777456641/{channelId}/{messageId}";

            var approvedReaction = Emote.Parse("<:Approved:787034785583333426>");
            var deniedReaction = Emote.Parse("<:Denied:787035973287542854>");
            var voidedReaction = Emote.Parse("<:Voided:762029395778207754>");
            var errorReaction = Emote.Parse("<:Error:787036714337566730>");

            /*if (userAccount.IsBot)
            {
                return;
            }*/

            if (reaction.Emote.Equals(voidedReaction))
            {
                if (user.Roles.Contains(supervisorRole))
                {
                    var msg = await channel.GetMessageAsync(messageId);
                    var getMessage = (IUserMessage)msg;

                    var embed = new EmbedBuilder();
                    embed.WithAuthor("Your action has been voided", "https://media.discordapp.net/attachments/629325734791348244/762029303100604416/Voided.png");
                    embed.WithDescription($"{getMessage}\n\n[Jump to message timeline]({messageLink})");
                    embed.WithFooter($"Voided by: {user.Username}");
                    embed.WithCurrentTimestamp();
                    embed.WithColor(Color.Red);
                    var voidedChannel = _client.GetGuild(619319282777456641).GetTextChannel(730194884895572052);
                    await voidedChannel.SendMessageAsync($"{author.Mention}", false, embed.Build());

                    await getMessage.DeleteAsync();
                }
                else
                {
                    var getMessage = await channel.GetMessageAsync(messageId);
                    await getMessage.RemoveReactionAsync(voidedReaction, user);
                    return;
                }
            }

            if (reaction.Emote.Equals(approvedReaction))
            {
                if (user.Roles.Contains(supervisorRole))
                {
                    //research
                    if (channel.Id.Equals(626784812656754709))
                    {
                        if (user.Id == author.Id)
                        {
                            var getMessage = await channel.GetMessageAsync(messageId);
                            await getMessage.RemoveReactionAsync(approvedReaction, author);
                            return;
                        }

                        var embed = new EmbedBuilder();
                        embed.WithDescription($"{approvedReaction} Your [`Research`]({messageLink}) was `Approved` by: {user.Mention}");
                        //embed.WithAuthor($"{author.Username}: Your `Research` was `Approved` by: {user.Username}", "https://cdn.discordapp.com/emojis/629329938532532249.png?v=1", $"{messageLink}");
                        embed.WithColor(Color.Green);
                        var logChannel = _client.GetGuild(619319282777456641).GetTextChannel(633670082652012545);
                        await logChannel.SendMessageAsync($"{author.Mention}", false, embed.Build());
                    }

                    //production
                    if (channel.Id.Equals(626784835045687317))
                    {
                        if (user.Id == author.Id)
                        {
                            var getMessage = await channel.GetMessageAsync(messageId);
                            await getMessage.RemoveReactionAsync(approvedReaction, author);
                            return;
                        }

                        var embed = new EmbedBuilder();
                        embed.WithDescription($"{approvedReaction} Your [`Production`]({messageLink}) was `Approved` by: {user.Mention}");
                        //embed.WithAuthor($"{author.Username}: Your `Production` was `Approved` by: {user.Username}", "https://cdn.discordapp.com/emojis/629329938532532249.png?v=1", $"{messageLink}");
                        embed.WithColor(Color.Green);
                        var logChannel = _client.GetGuild(619319282777456641).GetTextChannel(633670082652012545);
                        await logChannel.SendMessageAsync($"{author.Mention}", false, embed.Build());
                    }

                    //join un
                    if (channel.Id.Equals(629331128477548554))
                    {
                        var un = _client.GetGuild(619319282777456641).GetRole(727905836331958373);
                        await userAccount.AddRoleAsync(un);
                    }

                    //war declarations
                    if (channel.Id.Equals(629328771513712640))
                    {
                        if (user.Id == author.Id)
                        {
                            var getMessage = await channel.GetMessageAsync(messageId);
                            await getMessage.RemoveReactionAsync(approvedReaction, author);
                            return;
                        }

                        var msg = await channel.GetMessageAsync(message.Id);
                        var msg1 = (IUserMessage)msg;
                        var msg2 = msg1.Embeds.First();
                        var msg3 = msg2.ToEmbedBuilder().WithTitle("Approved").WithColor(Color.Green).Build();
                        await msg1.ModifyAsync(x => x.Embed = msg3);
                        var embed = msg3.ToEmbedBuilder();

                        var slChannel = _client.GetGuild(619319282777456641).GetTextChannel(732307234989670411);
                        await slChannel.SendMessageAsync("", false, embed.Build());
                    }
                }
                else
                {
                    var getMessage = await channel.GetMessageAsync(messageId);
                    await getMessage.RemoveReactionAsync(approvedReaction, user);
                    return;
                }
            }

            if (reaction.Emote.Equals(deniedReaction))
            {
                if (user.Roles.Contains(supervisorRole))
                {
                    //research
                    if (channel.Id.Equals(626784812656754709))
                    {
                        if (user.Id == author.Id)
                        {
                            var getMessage = await channel.GetMessageAsync(messageId);
                            await getMessage.RemoveReactionAsync(deniedReaction, author);
                            return;
                        }

                        var embed = new EmbedBuilder();
                        embed.WithDescription($"{deniedReaction} Your [`Research`]({messageLink}) was `Denied` by: {user.Mention}");
                        //embed.WithAuthor($"{author.Username}: Your `Research` was `Denied` by: {user.Username}", "https://cdn.discordapp.com/emojis/629329435350269962.png?v=1", $"{messageLink}");
                        embed.WithColor(Color.Red);
                        var logChannel = _client.GetGuild(619319282777456641).GetTextChannel(633670082652012545);
                        await logChannel.SendMessageAsync($"{author.Mention}", false, embed.Build());
                    }

                    //production
                    if (channel.Id.Equals(626784835045687317))
                    {
                        if (user.Id == author.Id)
                        {
                            var getMessage = await channel.GetMessageAsync(messageId);
                            await getMessage.RemoveReactionAsync(deniedReaction, author);
                            return;
                        }

                        var embed = new EmbedBuilder();
                        embed.WithDescription($"{deniedReaction} Your [`Production`]({messageLink}) was `Denied` by: {user.Mention}");
                        //embed.WithAuthor($"{author.Username}: Your `Production` was `Denied` by: {user.Username}", "https://cdn.discordapp.com/emojis/629329435350269962.png?v=1", $"{messageLink}");
                        embed.WithColor(Color.Red);
                        var logChannel = _client.GetGuild(619319282777456641).GetTextChannel(633670082652012545);
                        await logChannel.SendMessageAsync($"{author.Mention}", false, embed.Build());
                    }

                    //war declarations
                    if (channel.Id.Equals(629328771513712640))
                    {
                        if (user.Id == author.Id)
                        {
                            var getMessage = await channel.GetMessageAsync(messageId);
                            await getMessage.RemoveReactionAsync(deniedReaction, author);
                            return;
                        }

                        var msg = await channel.GetMessageAsync(message.Id);
                        var msg1 = (IUserMessage)msg;
                        var msg2 = msg1.Embeds.First();
                        var msg3 = msg2.ToEmbedBuilder().WithTitle("Denied").WithColor(Color.Red).Build();
                        await msg1.ModifyAsync(x => x.Embed = msg3);
                        var embed = msg3.ToEmbedBuilder();

                        var slChannel = _client.GetGuild(619319282777456641).GetTextChannel(732307234989670411);
                        await slChannel.SendMessageAsync("", false, embed.Build());
                    }
                }
                else
                {
                    var getMessage = await channel.GetMessageAsync(messageId);
                    await getMessage.RemoveReactionAsync(deniedReaction, user);
                    return;
                }
            }

            if (reaction.Emote.Equals(errorReaction))
            {
                if (user.Roles.Contains(supervisorRole))
                { 
                    //production
                    if (channel.Id == 626784835045687317)
                    {
                        var discussionChannel = _client.GetGuild(619319282777456641).GetTextChannel(713132586016440410);
                        var embed = new EmbedBuilder();
                        embed.WithDescription($"{errorReaction} {user.Mention} has a question about your [`production`]({messageLink})!");
                        embed.WithColor(new Color(255, 198, 0));
                        await discussionChannel.SendMessageAsync($"{userAccount.Mention}", false, embed.Build());
                    }

                    //research
                    if (channel.Id == 626784812656754709)
                    {
                        var discussionChannel = _client.GetGuild(619319282777456641).GetTextChannel(713132586016440410);
                        var embed = new EmbedBuilder();
                        embed.WithDescription($"{errorReaction} {user.Mention} has a question about your [`research`]({messageLink})!");
                        embed.WithColor(new Color(255, 198, 0));
                        await discussionChannel.SendMessageAsync($"{userAccount.Mention}", false, embed.Build());
                    }
                } 
                else
                {
                    var getMessage = await channel.GetMessageAsync(messageId);
                    await getMessage.RemoveReactionAsync(errorReaction, user);
                    return;
                }
            }
        }

        private async Task _client_UserJoined(SocketGuildUser newUser)
        {
            if ((DateTime.UtcNow - newUser.CreatedAt.UtcDateTime).TotalDays <= 7)
            {
                var investigate = _client.GetGuild(guildId).GetRole(645011653850824730);
                await newUser.AddRoleAsync(investigate);
                var alerts = _client.GetGuild(guildId).GetTextChannel(742494136912969810);

                var log = new EmbedBuilder();
                log.WithTitle("Investigation");
                log.WithDescription($"{newUser.Mention} has been placed under investigation!");
                log.WithCurrentTimestamp();
                log.WithFooter($"Moderator: MUNBot Automoderation");
                log.WithColor(Color.DarkRed);
                await alerts.SendMessageAsync("", false, log.Build());

                var dm = new EmbedBuilder();
                dm.WithTitle("You have been placed under investigation!");
                dm.WithDescription("A staff member has placed you under investigation in the **Model United Nations** server! Please read [here](https://discord.gg/sn9gQ5) for more info or use the `!verify` command.");
                dm.WithColor(Color.Red);
                await newUser.SendMessageAsync("", false, dm.Build());
            }

            var dm1 = new EmbedBuilder();
            dm1.WithTitle("Welcome! Please read this quick message.");
            dm1.WithDescription("Hey! Welcome to the server! Please make sure to read the <#626782330136297482> before choosing a country. Have a great time in the Model United Nations server!\n\nIf you need help <#682667353527549960>, you can check out the <#629675521357119508> channel or use the `!ticket` command.\n\nAll bans can be appealed [here](https://forms.gle/yonuRLR6fEFz3vcs5).");
            dm1.WithColor(Color.Blue);
            await newUser.SendMessageAsync("", false, dm1.Build());

            var embed = new EmbedBuilder();
            embed.WithTitle($"Welcome {newUser.Username}!");
            embed.WithDescription("Hello and welcome! Make sure to read the <#626782330136297482> before getting started!\n\nTo check if a country is available, please use the `!roleinfo <country role>` command or you can see the map in <#682667353527549960>.\n\nMention a staff member when you are ready to choose your country. Enjoy!");
            embed.WithColor(Color.Blue);
            var channel = _client.GetGuild(619319282777456641).GetTextChannel(626783002797670410);
            await channel.SendMessageAsync($"{newUser.Mention}", false, embed.Build());

            var newMember = new EmbedBuilder();
            newMember.WithAuthor(newUser);
            newMember.AddField("Account Created", $"{newUser.CreatedAt.DateTime.ToString("MM/dd/yyyy hh:mm tt")}");
            newMember.AddField("Username/Id", $"{newUser.Mention} ({newUser.Id})");
            newMember.WithColor(Color.Blue);
            var newMemberChannel = _client.GetGuild(619319282777456641).GetTextChannel(700007476325515334);
            await newMemberChannel.SendMessageAsync("<@&629698730509074462>", false, newMember.Build());

            var uc = _client.GetGuild(619319282777456641).GetRole(626783494508380160);
            await newUser.AddRoleAsync(uc);
        }

        private async Task RoleplayChannelsFormatFilter1(SocketMessage rawMessage)
        {
            var staffRole = _client.GetGuild(619319282777456641).GetRole(700057375394234399);
            var supervisorRole = _client.GetGuild(619319282777456641).GetRole(700057375394234399);
            var user = rawMessage.Author;
            var socketUser = (SocketGuildUser)user;

            if (rpFilter == true)
            {
                if (rawMessage.Author.Id == _client.CurrentUser.Id)
                {
                    return;
                }

                if (rawMessage.IsPinned)
                {
                    return;
                }

                if (socketUser.Roles.Contains(staffRole) || socketUser.Roles.Contains(supervisorRole))
                {
                    return;
                }

                //roleplay channels
                if (rawMessage.Channel.Id == 626782653399695401 || rawMessage.Channel.Id == 664922494037262445 || rawMessage.Channel.Id == 644521248436518932 || rawMessage.Channel.Id == 728680875860164709 || rawMessage.Channel.Id == 671751119478849556 || rawMessage.Channel.Id == 697751152099328000 || rawMessage.Channel.Id == 629328875024941056)
                {
                    if (rawMessage.Content.Contains("[]"))
                    {
                        var channel = _client.GetGuild(619319282777456641).GetTextChannel(rawMessage.Channel.Id);
                        var value = channel.SlowModeInterval;

                        if (value >= 30)
                        {
                            await channel.ModifyAsync(x =>
                            {
                                x.SlowModeInterval = value + 0;
                            });
                        }
                        else
                        {
                            await channel.ModifyAsync(x =>
                            {
                                x.SlowModeInterval = value + 1;
                            });
                        }

                        await rawMessage.DeleteAsync();
                        var msg = await rawMessage.Channel.SendMessageAsync("Please do not chat in this channel. Use <#629325734791348244> instead!");

                        await Task.Delay(2000);
                        await msg.DeleteAsync();
                    }

                    //if (!Regex.IsMatch(rawMessage.Content, @"^\[(.*?)\](.*?)$"))
                    if (!rawMessage.Content.Contains("[") || !rawMessage.Content.Contains("]"))
                    {
                        var channel = _client.GetGuild(619319282777456641).GetTextChannel(rawMessage.Channel.Id);
                        var value = channel.SlowModeInterval;

                        if (user.IsBot)
                        {
                            return;
                        }

                        if (value >= 30)
                        {
                            await channel.ModifyAsync(x =>
                            {
                                x.SlowModeInterval = value + 0;
                            });
                        }
                        else
                        {
                            await channel.ModifyAsync(x =>
                            {
                                x.SlowModeInterval = value + 1;
                            });
                        }

                        await rawMessage.DeleteAsync();
                        var msg = await rawMessage.Channel.SendMessageAsync("Please use the correct formatting when sending messages in roleplay channels!");

                        /*var embed = new EmbedBuilder();
                        embed.WithTitle("Here is your message");
                        embed.WithDescription($"{rawMessage}");
                        embed.WithFooter("Your message was deleted because it did not follow the correct format.");
                        embed.WithColor(Color.DarkGrey);
                        await user.SendMessageAsync("", false, embed.Build());*/

                        await Task.Delay(2000);
                        await msg.DeleteAsync();
                    }
                }

                //production channels
                if (rawMessage.Channel.Id == 626784835045687317 || rawMessage.Channel.Id == 626784812656754709)
                {
                    if (!rawMessage.Content.Contains("```"))
                    {
                        var channel = _client.GetGuild(619319282777456641).GetTextChannel(rawMessage.Channel.Id);
                        var value = channel.SlowModeInterval;

                        if (value >= 30)
                        {
                            await channel.ModifyAsync(x =>
                            {
                                x.SlowModeInterval = value + 0;
                            });
                        }
                        else
                        {
                            await channel.ModifyAsync(x =>
                            {
                                x.SlowModeInterval = value + 1;
                            });
                        }

                        await rawMessage.DeleteAsync();
                        var msg = await rawMessage.Channel.SendMessageAsync("Please do not chat in this channel. To discuss research or production, please use <#713132586016440410>!");

                        /*var embed = new EmbedBuilder();
                        embed.WithTitle("Here is your message");
                        embed.WithDescription($"{rawMessage}");
                        embed.WithFooter("Your message was deleted because it did not follow the correct format.");
                        embed.WithColor(Color.DarkGrey);
                        await user.SendMessageAsync("", false, embed.Build());*/

                        await Task.Delay(2000);
                        await msg.DeleteAsync();
                    }
                }
            }          
        }

        private async Task AutoModeration(SocketMessage rawMessage)
        {
            var user = rawMessage.Author;
            var socketUser = (SocketGuildUser)user;

            //roleplay channels
            if (rawMessage.Channel.Id == 626782653399695401 || rawMessage.Channel.Id == 664922494037262445 || rawMessage.Channel.Id == 644521248436518932 || rawMessage.Channel.Id == 728680875860164709 || rawMessage.Channel.Id == 671751119478849556 || rawMessage.Channel.Id == 697751152099328000 || rawMessage.Channel.Id == 629328875024941056)
            {
                var lowerCaseRawMessage = rawMessage.Content.ToLower();
                string[] censoredWords = new string[]
                {
                    "fuck",
                    "shit",
                    "cunt",
                    "nigger",
                    "nigga",
                    "cum",
                    "retard",
                    "cunt",
                    "penis",
                    "vagina",
                    "fag",
                    "faggot",
                    "fagot",
                    "porn",
                    "bitch",
                    "asshole",
                    "sex",
                    "anal",
                    "pussy",
                    "dick",
                    "cock",
                    "tits",
                    "boobs",
                };

                if (censoredWords.Any(x => x == lowerCaseRawMessage))
                {
                    var alertschannel = _client.GetGuild(guildId).GetTextChannel(742494136912969810);
                    var log = new EmbedBuilder();
                    log.WithTitle("AutoModeration Message Deleted");
                    log.AddField("Message", rawMessage.Content);
                    log.WithDescription($"**Offender:** {socketUser.Mention}\n**Reason:** Censored Word\n**Moderator:** MUNBot Automoderation\n**In:** <#{rawMessage.Channel.Id}>");
                    log.WithColor(Color.Red);
                    await alertschannel.SendMessageAsync("", false, log.Build());

                    await rawMessage.DeleteAsync();
                    return;
                }
            }

            if (rawMessage.MentionedUsers.Count >= 6)
            {
                await AutomodMute(user.Id, rawMessage.Channel.Id);
            }

            if (rawMessage.MentionedRoles.Count >= 6)
            {
                await AutomodMute(user.Id, rawMessage.Channel.Id);
            }

            // We need to add this to our channel message count.
            // ulong, int
            if (channelMessageCount.ContainsKey(user.Id))
            {
                var val = channelMessageCount[user.Id];
                channelMessageCount[user.Id] = (val.messageCount + 1, rawMessage.Channel.Id) ;
            }
            else
            {
                channelMessageCount.Add(user.Id, (1, rawMessage.Channel.Id));
            }
        } 

        /*private async Task MessageUpdated(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel channel1)
        {
            SocketMessage rawMessage = channel1.GetMessageAsync(arg1);

            //roleplay channels
            if (rawMessage.Channel.Id == 626782653399695401 || rawMessage.Channel.Id == 664922494037262445 || rawMessage.Channel.Id == 644521248436518932 || rawMessage.Channel.Id == 728680875860164709 || rawMessage.Channel.Id == 671751119478849556 || rawMessage.Channel.Id == 697751152099328000 || rawMessage.Channel.Id == 629328875024941056)
            {
                var lowerCaseRawMessage = rawMessage.Content.ToLower();
                string[] censoredWords = new string[]
                {
                    "fuck",
                    "shit",
                    "cunt",
                    "nigger",
                    "nigga",
                    "cum",
                    "retard",
                    "cunt",
                    "penis",
                    "vagina",
                    "fag",
                    "faggot",
                    "fagot",
                    "porn",
                    "bitch",
                    "asshole",
                    "sex",
                    "anal",
                    "pussy",
                    "dick",
                    "cock",
                    "tits",
                    "boobs",
                };

                if (censoredWords.Any(x => x == lowerCaseRawMessage))
                {
                    await rawMessage.DeleteAsync();
                    return;
                }
            }

            var staffRole = _client.GetGuild(619319282777456641).GetRole(700057375394234399);
            var supervisorRole = _client.GetGuild(619319282777456641).GetRole(700057375394234399);
            var user = rawMessage.Author;
            var socketUser = (SocketGuildUser)user;

            if (rawMessage.Author.Id == _client.CurrentUser.Id)
            {
                return;
            }

            if (rawMessage.IsPinned)
            {
                return;
            }

            if (socketUser.Roles.Contains(staffRole) || socketUser.Roles.Contains(supervisorRole))
            {
                return;
            }

            //roleplay channels
            if (rawMessage.Channel.Id == 626782653399695401 || rawMessage.Channel.Id == 664922494037262445 || rawMessage.Channel.Id == 644521248436518932 || rawMessage.Channel.Id == 728680875860164709 || rawMessage.Channel.Id == 671751119478849556 || rawMessage.Channel.Id == 697751152099328000 || rawMessage.Channel.Id == 629328875024941056)
            {
                if (rawMessage.Content.Contains("[]"))
                {
                    var channel = _client.GetGuild(619319282777456641).GetTextChannel(rawMessage.Channel.Id);
                    var value = channel.SlowModeInterval;

                    if (value >= 30)
                    {
                        await channel.ModifyAsync(x =>
                        {
                            x.SlowModeInterval = value + 0;
                        });
                    }
                    else
                    {
                        await channel.ModifyAsync(x =>
                        {
                            x.SlowModeInterval = value + 1;
                        });
                    }

                    await rawMessage.DeleteAsync();
                    var msg = await rawMessage.Channel.SendMessageAsync("Please do not chat in this channel. Use <#629325734791348244> instead!");

                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }

                //if (!Regex.IsMatch(rawMessage.Content, @"^\[(.*?)\](.*?)$"))
                if (!rawMessage.Content.Contains("[") || !rawMessage.Content.Contains("]"))
                {
                    var channel = _client.GetGuild(619319282777456641).GetTextChannel(rawMessage.Channel.Id);
                    var value = channel.SlowModeInterval;

                    if (user.IsBot)
                    {
                        return;
                    }

                    if (value >= 30)
                    {
                        await channel.ModifyAsync(x =>
                        {
                            x.SlowModeInterval = value + 0;
                        });
                    }
                    else
                    {
                        await channel.ModifyAsync(x =>
                        {
                            x.SlowModeInterval = value + 1;
                        });
                    }

                    await rawMessage.DeleteAsync();
                    var msg = await rawMessage.Channel.SendMessageAsync("Please use the correct formatting when sending messages in roleplay channels!");

                    var embed = new EmbedBuilder();
                    embed.WithTitle("Here is your message");
                    embed.WithDescription($"{rawMessage}");
                    embed.WithFooter("Your message was deleted because it did not follow the correct format.");
                    embed.WithColor(Color.DarkGrey);
                    await user.SendMessageAsync("", false, embed.Build());

                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
            }

            //production channels
            if (rawMessage.Channel.Id == 626784835045687317 || rawMessage.Channel.Id == 626784812656754709)
            {
                if (!rawMessage.Content.Contains("```"))
                {
                    var channel = _client.GetGuild(619319282777456641).GetTextChannel(rawMessage.Channel.Id);
                    var value = channel.SlowModeInterval;

                    if (value >= 30)
                    {
                        await channel.ModifyAsync(x =>
                        {
                            x.SlowModeInterval = value + 0;
                        });
                    }
                    else
                    {
                        await channel.ModifyAsync(x =>
                        {
                            x.SlowModeInterval = value + 1;
                        });
                    }

                    await rawMessage.DeleteAsync();
                    var msg = await rawMessage.Channel.SendMessageAsync("Please do not chat in this channel. To discuss research or production, please use <#713132586016440410>!");

                    var embed = new EmbedBuilder();
                    embed.WithTitle("Here is your message");
                    embed.WithDescription($"{rawMessage}");
                    embed.WithFooter("Your message was deleted because it did not follow the correct format.");
                    embed.WithColor(Color.DarkGrey);
                    await user.SendMessageAsync("", false, embed.Build());

                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
            }

        }*/

        // Audit Logs
        /*private async Task ChannelCreated(SocketChannel channel)
        {
            var logs = _client.GetGuild(619319282777456641).GetTextChannel(626785804672565249);

            var embed = new EmbedBuilder();
            embed.WithTitle("Channel Created");
            embed.AddField("Name/Id", $"{channel.ToString()} {channel.Id}", true);
            embed.WithColor(Color.Green);
            embed.WithFooter($"User: {}")

            if (channel is SocketTextChannel)
            {
                embed.AddField("Type", "Text Channel", true);
            }

            if (channel is SocketVoiceChannel)
            {
                embed.AddField("Type", "Voice Channel", true);
            }

            if (channel is SocketNewsChannel)
            {
                embed.AddField("Type", "Announcement Channel", true);
            }

            await logs.SendMessageAsync("", false, embed.Build());

        }
        private async Task ChannelDeleted(SocketChannel channel)
        {
            var logs = _client.GetGuild(619319282777456641).GetTextChannel(626785804672565249);

            var embed = new EmbedBuilder();
            embed.WithTitle("Channel Deleted");
            embed.AddField("Name/Id", $"{channel.ToString()} {channel.Id}");
            embed.WithColor(Color.Red);
            await logs.SendMessageAsync("", false, embed.Build());
        }
        private async Task RoleCreated(SocketRole role)
        {
            var logs = _client.GetGuild(619319282777456641).GetTextChannel(626785804672565249);

            var embed = new EmbedBuilder();
            embed.WithTitle("Role Created");
            embed.AddField("Name/Id", $"{role.Name} ({role.Id})", true);
            embed.AddField("Permissions", role.Permissions, true);
            embed.WithColor(role.Color);
            await logs.SendMessageAsync("", false, embed.Build());
        }
        private async Task RoleDeleted(SocketRole role)
        {
            var logs = _client.GetGuild(619319282777456641).GetTextChannel(626785804672565249);

            var embed = new EmbedBuilder();
            embed.WithTitle("Role Deleted");
            embed.AddField("Name/Id", $"{role.Name} ({role.Id})", true);

        }*/

        public async Task InitializeAsync()
        {
            // register modules that are public and inherit ModuleBase<T>.
            //await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        // this class is where the magic starts, and takes actions upon receiving messages
        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // ensures we don't process system/other bot messages
            if (!(rawMessage is SocketUserMessage message))
            {
                return;
            }

            if (message.Source != MessageSource.User)
            {
                return;
            }

            // sets the argument position away from the prefix we set
            var argPos = 0;

            // get prefix from the configuration file
            char prefix = Char.Parse("!");

            // determine if the message has a valid prefix, and adjust argPos based on prefix
            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasCharPrefix(prefix, ref argPos)))
            {
                return;
            }

            var context = new SocketCommandContext(_client, message);

            // execute command if one is found that matches
            await _commands.ExecuteAsync(context, argPos, null, MultiMatchHandling.Best);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // if a command isn't found, log that info to console and exit this method

            if (!command.IsSpecified)
            {
                System.Console.WriteLine("Unknown Command Was Used");

                return;
            }

            // log success to the console and exit this method
            if (result.IsSuccess)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("Command Success\n------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;
                string time = DateTime.Now.ToString("hh:mm:ss tt");
                System.Console.WriteLine($"Command: [{command.Value.Name}]\nUser: [{context.User.Username}]\nTime: {time}");
                Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.WithAuthor("Command Error", "https://cdn.discordapp.com/emojis/787035973287542854.png?v=1");
                embed.WithDescription(result.ErrorReason);
                embed.WithColor(Color.Red);
                await context.Channel.SendMessageAsync("", false, embed.Build());

                Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Command Error\n------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;
                string time = DateTime.Now.ToString("hh:mm:ss tt");
                System.Console.WriteLine($"Command: [{command.Value.Name}]\nUser: [{context.User.Username}]\nTime: {time}\nError: {result.ErrorReason}");
                Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}