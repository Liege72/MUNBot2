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
using MongoDB.Driver;
using Template.Common;
using MUNBot.Services;

namespace MUNBot.Modules
{
    public class ModCommands : ModuleBase<SocketCommandContext>
    {
        static string ModLogsPath = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}Modlogs.json";

        public async void AddModlogs(ulong userID, Action action, ulong ModeratorID, string reason, string username)
        {
            var id = Context.Guild.Id;
            if (currentLogs.Any(x => x.GuildID == id))
            {
                var guildLogs = currentLogs.Find(x => x.GuildID == id);
                if (guildLogs.Users.Any(x => x.userId == userID))
                {
                    guildLogs.Users[guildLogs.Users.FindIndex(x => x.userId == userID)].Logs.Add(new UserModLogs()
                    {
                        Action = action,
                        ModeratorID = ModeratorID,
                        Reason = reason,
                        Date = DateTime.UtcNow.ToString("r")
                    });
                }
                else
                {
                    guildLogs.Users.Add(new User()
                    {
                        Logs = new List<UserModLogs>()
                        {
                            { new UserModLogs(){
                                Action = action,
                                ModeratorID = ModeratorID,
                                Reason = reason,
                            Date = DateTime.UtcNow.ToString("r")
                        } }
                        },
                        userId = userID,
                        username = username
                    });
                }

            }
            else
            {
                currentLogs.Add(new ModlogsJson()
                {
                    GuildID = id,
                    Users = new List<User>()
                    {
                        new User()
                        {
                            userId = userID,
                            username = username,
                            Logs = new List<UserModLogs>()
                            {
                                new UserModLogs()
                                {
                                    Action = action,
                                    ModeratorID = ModeratorID,
                                    Reason = reason,
                                    Date = DateTime.UtcNow.ToString("r")
                                }
                            }
                        }
                    }
                });
            }

            if (currentLogs.Count >= 5)
            {
                var modlogs = Context.Guild.GetTextChannel(709805453655277578);
                var embed = new EmbedBuilder();
                embed.WithTitle("5 Modlogs Reached!");
                embed.WithDescription($"<@{userID}> has reached 5 infractions!");
                embed.WithColor(Color.Red);
                embed.WithCurrentTimestamp();
                await modlogs.SendMessageAsync("<@&629698730509074462>", false, embed.Build());
            }

            SaveModLogs();
        }
        public static List<ModlogsJson> LoadModLogs()
        {
            try
            {
                var d = JsonConvert.DeserializeObject<List<ModlogsJson>>(File.ReadAllText(ModLogsPath));
                if (d == null) { throw new Exception(); }
                return d;
            }
            catch //(Exception ex)
            {
                return new List<ModlogsJson>();
            }


        }
        public static List<ModlogsJson> currentLogs { get; set; } = LoadModLogs();
        static public void SaveModLogs()
        {
            string json = JsonConvert.SerializeObject(currentLogs);
            File.WriteAllText(ModLogsPath, json);
        }
        public class ModlogsJson
        {
            public List<User> Users { get; set; }
            public ulong GuildID { get; set; }
        }
        public class User
        {
            public List<UserModLogs> Logs { get; set; }
            public ulong userId { get; set; }
            public string username { get; set; }
        }
        public class UserModLogs
        {
            public string Reason { get; set; }
            public Action Action { get; set; }
            public ulong ModeratorID { get; set; }
            public string Date { get; set; }
        }
        public enum Action
        {
            Warned,
            Kicked,
            Banned,
            Muted,
            TempBan
        }

        [Command("warn")]
        [Alias("w")]
        public async Task Warn(SocketGuildUser userAccount = null, [Remainder] string reason = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = Context.Guild.GetRole(629698730509074462);

            if (!user.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            if (userAccount == null)
            {
                await Context.Channel.SendErrorAsync("Please mention a user!");
                return;
            }

            if (reason == null)
            {
                await Context.Channel.SendErrorAsync("Please provide a reason!");
                return;
            }

            if (user.Roles.Contains(roleStaff))
            {
                AddModlogs(userAccount.Id, Action.Warned, Context.Message.Author.Id, reason, userAccount.Username);

                await Context.Channel.SendInfractionAsync("Warned", userAccount, user, reason);

                var modlogschannel = Context.Guild.GetTextChannel(709805453655277578);
                await modlogschannel.ModlogAsync("Warning", userAccount, reason, user, Context.Channel);

                var warnMsg = new EmbedBuilder();
                warnMsg.WithTitle($"You were warned in `{Context.Guild}`");
                warnMsg.AddField($"Reason", reason, true);
                warnMsg.WithFooter($"Warned by {Context.Message.Author.Username}");
                warnMsg.WithCurrentTimestamp();
                await userAccount.SendMessageAsync("", false, warnMsg.Build());
            }
        }

        [Command("mute")]
        [Alias("m")]
        public async Task Mute(SocketGuildUser userAccount = null, string time = null, [Remainder] string reason = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = Context.Guild.GetRole(629698730509074462);
            var muteRole = Context.Guild.GetRole(693504845080035338);

            if (!user.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            if (userAccount == null)
            {
                await Context.Channel.SendErrorAsync("Please mention a user!");
                return;
            }

            if (time == null)
            {
                await Context.Channel.SendErrorAsync("Please provide a time!");
                return;
            }

            if (reason == null)
            {
                await Context.Channel.SendErrorAsync("Please provide a reason!");
                return;
            }

            if (userAccount.Roles.Contains(muteRole))
            {
                await Context.Channel.SendErrorAsync("This user is already muted!");
                return;
            }

            if (user.Roles.Contains(roleStaff))
            {
                string adjustedTime = time.Remove(time.Length - 1, 1);
                int timeInt = int.Parse(adjustedTime);

                if (time.EndsWith("s"))
                {
                    CommandHandler.Mutes.Add(new Mute { Guild = Context.Guild, User = userAccount, End = DateTime.Now + TimeSpan.FromSeconds(timeInt), Role = muteRole });
                }

                if (time.EndsWith("m"))
                {
                    CommandHandler.Mutes.Add(new Mute { Guild = Context.Guild, User = userAccount, End = DateTime.Now + TimeSpan.FromMinutes(timeInt), Role = muteRole });
                }

                if (time.EndsWith("h"))
                {
                    CommandHandler.Mutes.Add(new Mute { Guild = Context.Guild, User = userAccount, End = DateTime.Now + TimeSpan.FromHours(timeInt), Role = muteRole });
                }

                await userAccount.AddRoleAsync(muteRole);
                AddModlogs(userAccount.Id, Action.Muted, Context.Message.Author.Id, reason, userAccount.Username);

                await Context.Channel.SendInfractionAsync("Muted", userAccount, user, reason);

                var modlogschannel = Context.Guild.GetTextChannel(709805453655277578);
                await modlogschannel.ModlogAsync("Mute", userAccount, reason, user, Context.Channel);

                var muteMsg = new EmbedBuilder();
                muteMsg.WithTitle($"You were muted in `{Context.Guild}`");
                muteMsg.AddField($"Reason", reason, true);
                muteMsg.WithFooter($"Muted by {Context.Message.Author.ToString()}");
                muteMsg.WithCurrentTimestamp();
                await userAccount.SendMessageAsync("", false, muteMsg.Build());
            }
        }

        [Command("unmute")]
        [Alias("um")]
        public async Task Unmute(SocketGuildUser userAccount = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = Context.Guild.GetRole(629698730509074462);
            var muteRole = Context.Guild.GetRole(693504845080035338);

            if (!user.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            if (userAccount == null)
            {
                await Context.Channel.SendErrorAsync("Please mention a user!");
                return;
            }

            if (!userAccount.Roles.Contains(muteRole))
            {
                await Context.Channel.SendErrorAsync("This user is aleady unmuted!");
                return;
            }

            if (user.Roles.Contains(roleStaff))
            {
                await userAccount.RemoveRoleAsync(muteRole);

                var mute = new EmbedBuilder();
                mute.WithTitle($"{userAccount.Username} has been unmuted.");
                mute.AddField("Moderator", user.Mention);
                mute.WithColor(Color.Green);
                mute.WithCurrentTimestamp();
                await ReplyAsync("", false, mute.Build());

                var log = new EmbedBuilder();
                log.WithTitle("Unmute");
                log.WithDescription($"**Offender:** {userAccount.Mention} \n **Moderator:** {user} \n **In:** <#{Context.Channel.Id}>");
                log.WithCurrentTimestamp();
                log.WithColor(Color.Red);

                var modlogschannel = Context.Guild.GetTextChannel(709805453655277578);
                await modlogschannel.SendMessageAsync("", false, log.Build());

                var muteMsg = new EmbedBuilder();
                muteMsg.WithTitle($"You were unmuted in `{Context.Guild}`");
                muteMsg.WithFooter($"Unmuted by {Context.Message.Author.Username}");
                muteMsg.WithCurrentTimestamp();
                await userAccount.SendMessageAsync("", false, muteMsg.Build());
            }
        }

        [Command("kick")]
        [Alias("k")]
        public async Task Kick(SocketGuildUser userAccount = null, [Remainder] string reason = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = Context.Guild.GetRole(629698730509074462);

            if (!user.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            if (userAccount == null)
            {
                await Context.Channel.SendErrorAsync("Please mention a user!");
                return;
            }

            if (reason == null)
            {
                await Context.Channel.SendErrorAsync("Please provide a reason!");
                return;
            }

            if (user.Id == userAccount.Id)
            {
                await Context.Channel.SendErrorAsync("You cannot kick youself!");
                return;
            }

            if (user.Roles.Contains(roleStaff) && userAccount.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You cannot kick a staff member!");
                return;
            }

            if (user.Roles.Contains(roleStaff))
            {
                AddModlogs(userAccount.Id, Action.Kicked, Context.Message.Author.Id, reason, userAccount.Username);

                await Context.Channel.SendInfractionAsync("Kicked", userAccount, user, reason);

                var modlogschannel = Context.Guild.GetTextChannel(709805453655277578);
                await modlogschannel.ModlogAsync("Kick", userAccount, reason, user, Context.Channel);

                var kickMsg = new EmbedBuilder();
                kickMsg.WithTitle($"You are kicked from `{Context.Guild}`");
                kickMsg.AddField($"Reason", reason);
                kickMsg.WithFooter($"Kicked by {user.Username}");
                kickMsg.WithCurrentTimestamp();
                await userAccount.SendMessageAsync($"", false, kickMsg.Build());

                await userAccount.KickAsync(reason);
            }


        }

        [Command("ban")]
        [Alias("b")]
        public async Task Ban(SocketGuildUser userAccount = null, [Remainder] string reason = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = Context.Guild.GetRole(629698730509074462);

            if (!user.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            if (userAccount == null)
            {
                await Context.Channel.SendErrorAsync("Please mention a user!");
                return;
            }

            if (reason == null)
            {
                await Context.Channel.SendErrorAsync("Please provide a reason!");
                return;
            }

            if (user.Id == userAccount.Id)
            {
                await Context.Channel.SendErrorAsync("You cannot ban youself!");
                return;
            }

            if (user.Roles.Contains(roleStaff) && userAccount.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You cannot ban a staff member!");
                return;
            }

            if (user.Roles.Contains(roleStaff))
            {
                AddModlogs(userAccount.Id, Action.Banned, Context.Message.Author.Id, reason, userAccount.Username);

                await Context.Channel.SendInfractionAsync("Banned", userAccount, user, reason);

                var log = new EmbedBuilder();
                log.WithTitle("Ban");
                log.WithDescription($"**Offender:** {userAccount.Mention} \n **Reason:** `{reason}` \n **Moderator:** {user} \n **In:** <#{Context.Channel.Id}>");
                log.WithCurrentTimestamp();
                log.WithColor(Color.Red);

                var modlogschannel = Context.Guild.GetTextChannel(709805453655277578);
                await modlogschannel.ModlogAsync("Ban", userAccount, reason, user, Context.Channel);

                var banMsg = new EmbedBuilder();
                banMsg.WithTitle($"You are banned from `{Context.Guild}`");
                banMsg.AddField($"Reason", reason);
                banMsg.WithFooter($"Banned by {Context.Message.Author.Username}");
                banMsg.WithCurrentTimestamp();
                await userAccount.SendMessageAsync("", false, banMsg.Build());

                await Context.Guild.AddBanAsync(userAccount, 7, reason);
            }

        }

        [Command("tempban")]
        public async Task TempBan(SocketGuildUser userAccount = null, [Remainder] string reason = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = Context.Guild.GetRole(629698730509074462);
            var banRole = Context.Guild.GetRole(694056967542407168);

            if (!user.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            if (userAccount == null)
            {
                await Context.Channel.SendErrorAsync("Please mention a user!");
                return;
            }

            if (reason == null)
            {
                await Context.Channel.SendErrorAsync("Please provide a reason!");
                return;
            }

            if (user.Id == userAccount.Id)
            {
                await Context.Channel.SendErrorAsync("You cannot tempban youself!");
                return;
            }

            if (user.Roles.Contains(roleStaff) && userAccount.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You cannot tempban a staff member!");
                return;
            }

            if (user.Roles.Contains(roleStaff))
            {
                await userAccount.AddRoleAsync(banRole);
                AddModlogs(userAccount.Id, Action.TempBan, Context.Message.Author.Id, reason, userAccount.Username);

                var unRole = Context.Guild.GetRole(727905836331958373);
                if (userAccount.Roles.Contains(unRole))
                {
                    await userAccount.RemoveRoleAsync(unRole);
                }

                await Context.Channel.SendInfractionAsync("Temporarily Banned", userAccount, user, reason);

                var banChannel = Context.Guild.GetTextChannel(694208736830423101);
                await banChannel.ModlogAsync("Temporary Ban", userAccount, reason, user, Context.Channel);

                var modlogs = Context.Guild.GetTextChannel(709805453655277578);
                await modlogs.ModlogAsync("Temporary Ban", userAccount, reason, user, Context.Channel);

                var banMsg = new EmbedBuilder();
                banMsg.WithTitle($"You were temporarily banned from `{Context.Guild}`");
                banMsg.WithDescription("This means you will not have access to view or type in any channels for 24 hours.");
                banMsg.AddField($"Reason", reason);
                banMsg.WithFooter($"Banned by {Context.Message.Author.Username}");
                banMsg.WithCurrentTimestamp();
                await userAccount.SendMessageAsync("", false, banMsg.Build());
            }

        }

        [Command("modlogs")]
        [Alias("ml")]
        public async Task Modlogs(string mention = null)
        {
            if (Context.Guild == null)
            {
                var embed = new EmbedBuilder();
                embed.WithTitle("Error");
                embed.WithDescription("You cannot use this command in Direct Messages!");
                embed.WithColor(Color.Red);
                await ReplyAsync("", false, embed.Build());
            }

            var user = Context.User as SocketGuildUser;
            var roleStaff = Context.Guild.GetRole(629698730509074462);
            var mentions = Context.Message.MentionedUsers;

            if (!user.Roles.Contains(roleStaff) && !user.GuildPermissions.ManageMessages)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = "Powered by Luminous",
                        Url = "https://luminousbot.com",
                        IconUrl = "https://cdn.discordapp.com/avatars/722435272532426783/314f820ffc45a70755892eb34fe4110a.png?size=256"
                    },
                    Title = "You do not have permission to execute this command",
                    Description = "You do not have the valid permission to execute this command",
                    Color = Color.Red

                }.Build());
                return;
            }

            if (mentions.Count == 0)
            {
                var noUser = new EmbedBuilder();
                noUser.WithTitle("Error");
                noUser.WithDescription("Please mention a user!");
                noUser.WithColor(Color.Red);
                await Context.Channel.SendMessageAsync("", false, noUser.Build());
                return;
            }

            var user1 = mentions.First();
            if (currentLogs.Any(x => x.GuildID == Context.Guild.Id))
            {
                var modlogs = currentLogs.Find(x => x.GuildID == Context.Guild.Id);
                if (modlogs.Users.Any(x => x.userId == user1.Id))
                {
                    var userAccount = modlogs.Users[modlogs.Users.FindIndex(x => x.userId == user1.Id)];
                    var logs = userAccount.Logs;
                    string usrnm = Context.Guild.GetUser(userAccount.userId) == null ? userAccount.username : Context.Guild.GetUser(userAccount.userId).ToString();
                    EmbedBuilder b = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = "Powered by Luminous",
                            Url = "https://luminousbot.com",
                            IconUrl = "https://cdn.discordapp.com/avatars/722435272532426783/314f820ffc45a70755892eb34fe4110a.png?size=256"
                        },
                        Title = $"Modlogs for **{usrnm}** ({user1.Id})",
                        Description = $"To remove a log type `!clearlog <user> <log number>`",
                        Color = Color.Green,
                        Fields = new List<EmbedFieldBuilder>()
                    };
                    foreach (var log in logs)
                    {
                        b.Fields.Add(new EmbedFieldBuilder()
                        {
                            IsInline = false,
                            Name = Enum.GetName(typeof(Action), log.Action),
                            Value = $"Reason: {log.Reason}\nModerator: <@{log.ModeratorID}>\nDate: {log.Date}"
                        });
                    }
                    if (logs.Count == 0)
                    {
                        b.Description = "This user has not logs!";
                    }
                    await Context.Channel.SendMessageAsync("", false, b.Build());

                }
                else
                {
                    await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = "Powered by Luminous",
                            Url = "https://luminousbot.com",
                            IconUrl = "https://cdn.discordapp.com/avatars/722435272532426783/314f820ffc45a70755892eb34fe4110a.png?size=256"
                        },
                        Title = $"Modlogs for ({user1.Id})",
                        Description = "This user has no logs! :D",
                        Color = Color.Green
                    }.Build());
                    return;
                }
            }
        }

        [Command("clearlog")]
        [Alias("cl")]
        public async Task Clearwarn(string user1 = null, int number = 999)
        {
            if (Context.Guild == null)
            {
                var embed = new EmbedBuilder();
                embed.WithTitle("Error");
                embed.WithDescription("You cannot use this command in Direct Messages!");
                embed.WithColor(Color.Red);
                await ReplyAsync("", false, embed.Build());
            }

            var user = Context.User as SocketGuildUser;
            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
            var mentions = Context.Message.MentionedUsers;

            if (!user.Roles.Contains(roleStaff) && !user.GuildPermissions.ManageMessages)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = "Powered by Luminous",
                        Url = "https://luminousbot.com",
                        IconUrl = "https://cdn.discordapp.com/avatars/722435272532426783/314f820ffc45a70755892eb34fe4110a.png?size=256"
                    },
                    Title = "You do not have permission to execute this command",
                    Description = "You do not have the valid permission to execute this command",
                    Color = Color.Red
                }.Build());
                return;
            }

            if (user1 == null)
            {
                var noUser = new EmbedBuilder();
                noUser.WithTitle("Error");
                noUser.WithDescription("Please mention a user!");
                noUser.WithColor(Color.Red);
                await ReplyAsync("", false, noUser.Build());
            }

            Regex r = new Regex("(\\d{18}|\\d{17})");
            if (!r.IsMatch(user1))
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = "Powered by Luminous",
                        Url = "https://luminousbot.com",
                        IconUrl = "https://cdn.discordapp.com/avatars/722435272532426783/314f820ffc45a70755892eb34fe4110a.png?size=256"
                    },
                    Title = "Invalid ID",
                    Description = "The ID you provided is invalid! (1)",
                    Color = Color.Red
                }.Build());
                return;
            }
            ulong id;
            try
            {
                id = Convert.ToUInt64(r.Match(user1).Groups[1].Value);
            }
            catch
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = "Powered by Luminous",
                        Url = "https://luminousbot.com",
                        IconUrl = "https://cdn.discordapp.com/avatars/722435272532426783/314f820ffc45a70755892eb34fe4110a.png?size=256"
                    },
                    Title = "Invalid ID",
                    Description = "The ID you provided is invalid! (2)",
                    Color = Color.Red
                }.Build());
                return;
            }

            var num = number.ToString();

            if (currentLogs.Any(x => x.GuildID == Context.Guild.Id))
            {
                var modlogs = currentLogs.Find(x => x.GuildID == Context.Guild.Id);
                if (num == "999")
                {
                    if (modlogs.Users.Any(x => x.userId == id))
                    {
                        var usrlogs = modlogs.Users[modlogs.Users.FindIndex(x => x.userId == id)];
                        string usrnm = Context.Guild.GetUser(usrlogs.userId) == null ? usrlogs.username : Context.Guild.GetUser(usrlogs.userId).ToString();
                        EmbedBuilder b = new EmbedBuilder()
                        {
                            Title = $"Modlogs for **{usrnm}**",
                            Color = Color.DarkMagenta,
                            Description = $"Modlogs for {usrlogs.username},\nTo remove a log type `!clearlog <user> <log number>`\n",
                            Fields = new List<EmbedFieldBuilder>()
                        };
                        for (int i = 0; i != usrlogs.Logs.Count; i++)
                        {
                            var log = usrlogs.Logs[i];
                            b.Fields.Add(new EmbedFieldBuilder()
                            {
                                IsInline = false,
                                Name = (i + 1).ToString(),
                                Value =
                                $"**{log.Action}**\n" +
                                $"Reason: {log.Reason}\n" +
                                $"Moderator: <@{log.ModeratorID}> ({log.ModeratorID.ToString()}\n" +
                                $"Date: {log.Date}"
                            });
                        }
                        await Context.Channel.SendMessageAsync("", false, b.Build());
                        return;
                    }
                    else
                    {
                        EmbedBuilder b = new EmbedBuilder()
                        {
                            Author = new EmbedAuthorBuilder()
                            {
                                Name = "Powered by Luminous",
                                Url = "https://luminousbot.com",
                                IconUrl = "https://cdn.discordapp.com/avatars/722435272532426783/314f820ffc45a70755892eb34fe4110a.png?size=256"
                            },
                            Title = "User has no logs!",
                            Description = $"The user <@{id}> has no logs!",
                            Color = Color.Red,
                        };
                        await Context.Channel.SendMessageAsync("", false, b.Build());
                        return;
                    }
                }

                if (modlogs.Users.Any(x => x.userId == id))
                {
                    var usrlogs = modlogs.Users[modlogs.Users.FindIndex(x => x.userId == id)];
                    usrlogs.Logs.RemoveAt(number - 1);

                    var filter = Builders<ModlogsJson>.Filter.Eq("GuildID", modlogs.GuildID);
                    //Collection.ReplaceOne(filter, modlogs);

                    string usrnm = Context.Guild.GetUser(usrlogs.userId) == null ? usrlogs.username : Context.Guild.GetUser(usrlogs.userId).ToString();
                    EmbedBuilder b = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = "Powered by Luminous",
                            Url = "https://luminousbot.com",
                            IconUrl = "https://cdn.discordapp.com/avatars/722435272532426783/314f820ffc45a70755892eb34fe4110a.png?size=256"
                        },
                        Title = $"Successfully cleared a log for **{usrnm}**",
                        Color = Color.DarkMagenta,
                        Description = $"Modlogs for {usrlogs.username},\nTo remove a log type `!clearlog <user> <log number>`\n",
                        Fields = new List<EmbedFieldBuilder>()
                    };
                    for (int i = 0; i != usrlogs.Logs.Count; i++)
                    {
                        var log = usrlogs.Logs[i];
                        b.Fields.Add(new EmbedFieldBuilder()
                        {
                            IsInline = false,
                            Name = (i + 1).ToString(),
                            Value =
                            $"**{log.Action}**\n" +
                            $"Reason: {log.Reason}\n" +
                            $"Moderator: <@{log.ModeratorID}> ({log.ModeratorID.ToString()}\n" +
                            $"Date: {log.Date}"
                        });
                    }
                    await Context.Channel.SendMessageAsync("", false, b.Build());
                }
            }

            else
            {
                EmbedBuilder b = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = "Powered by Luminous",
                        Url = "https://luminousbot.com",
                        IconUrl = "https://cdn.discordapp.com/avatars/722435272532426783/314f820ffc45a70755892eb34fe4110a.png?size=256"
                    },
                    Title = "User has no logs!",
                    Description = $"The user <@{id}> has no logs!",
                    Color = Color.Red,
                };
                await Context.Channel.SendMessageAsync("", false, b.Build());
            }

        }

        [Command("slowmode")]
        public async Task Slowmode(int value = 0)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = Context.Guild.GetRole(629698730509074462);

            if (!user.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            var channel = Context.Guild.GetTextChannel(Context.Channel.Id);
            await channel.ModifyAsync(x =>
            {
                x.SlowModeInterval = value;
            });

            var log = new EmbedBuilder();
            log.WithTitle("Slowmode");
            log.WithDescription($"<#{Context.Channel.Id}>s slowmode was set to `{value}`");
            log.WithFooter($"Set by: {user}");
            log.WithCurrentTimestamp();
            log.WithColor(Color.Blue);

            var modlogsChannel = Context.Guild.GetTextChannel(709805453655277578);
            await modlogsChannel.SendMessageAsync("", false, log.Build());

            await Context.Channel.SendSuccessAsync($"Set the slowmode to `{value}`!");
            return;
        }

        [Command("lock")]
        public async Task LockChannel(SocketGuildChannel channelName = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = Context.Guild.GetRole(629698730509074462);

            if (!user.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            if (channelName == null)
            {
                await Context.Channel.SendSuccessAsync($"<#{Context.Channel.Id}> has been locked!");

                var lockedChannel = Context.Channel as SocketTextChannel;
                var newPerms = new OverwritePermissions(sendMessages: PermValue.Deny);
                var cc = Context.Guild.GetRole(626783654395379753);
                await lockedChannel.AddPermissionOverwriteAsync(cc, newPerms);

                var log = new EmbedBuilder();
                log.WithTitle("Channel Locked");
                log.WithDescription($"<#{Context.Channel.Id}> was locked");
                log.WithFooter($"Locked by: {user}");
                log.WithCurrentTimestamp();
                log.WithColor(Color.Blue);

                var modlogsChannel = Context.Guild.GetTextChannel(709805453655277578);
                await modlogsChannel.SendMessageAsync("", false, log.Build());
            }
            else
            {
                await Context.Channel.SendSuccessAsync($"<#{channelName.Id}> has been locked!");

                var lockedChannel = Context.Channel as SocketTextChannel;
                var newPerms = new OverwritePermissions(sendMessages: PermValue.Deny);
                var cc = Context.Guild.GetRole(626783654395379753);
                await lockedChannel.AddPermissionOverwriteAsync(cc, newPerms);

                var log = new EmbedBuilder();
                log.WithTitle("Channel Locked");
                log.WithDescription($"<#{channelName.Id}> was locked");
                log.WithFooter($"Locked by: {user}");
                log.WithCurrentTimestamp();
                log.WithColor(Color.Blue);

                var modlogsChannel = Context.Guild.GetTextChannel(709805453655277578);
                await modlogsChannel.SendMessageAsync("", false, log.Build());
            }
        }

        [Command("unlock")]
        public async Task UnlockChannel(SocketGuildChannel channelName = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = Context.Guild.GetRole(629698730509074462);

            if (!user.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            if (channelName == null)
            {
                await Context.Channel.SendSuccessAsync($"<#{Context.Channel.Id}> has been unlocked!");

                var lockedChannel = Context.Channel as SocketTextChannel;
                var newPerms = new OverwritePermissions(sendMessages: PermValue.Inherit);
                var cc = Context.Guild.GetRole(626783654395379753);
                await lockedChannel.AddPermissionOverwriteAsync(cc, newPerms);

                var log = new EmbedBuilder();
                log.WithTitle("Channel Unlocked");
                log.WithDescription($"<#{Context.Channel.Id}> was unlocked");
                log.WithFooter($"Unlocked by: {user}");
                log.WithCurrentTimestamp();
                log.WithColor(Color.Blue);

                var modlogsChannel = Context.Guild.GetTextChannel(709805453655277578);
                await modlogsChannel.SendMessageAsync("", false, log.Build());
            }
            else
            {
                await Context.Channel.SendSuccessAsync($"<#{channelName.Id}> has been locked!");

                var lockedChannel = Context.Channel as SocketTextChannel;
                var newPerms = new OverwritePermissions(sendMessages: PermValue.Inherit);
                var cc = Context.Guild.GetRole(626783654395379753);
                await lockedChannel.AddPermissionOverwriteAsync(cc, newPerms);

                var log = new EmbedBuilder();
                log.WithTitle("Channel Unlocked");
                log.WithDescription($"<#{channelName.Id}> was unlocked");
                log.WithFooter($"Unlocked by: {user}");
                log.WithCurrentTimestamp();
                log.WithColor(Color.Blue);

                var modlogsChannel = Context.Guild.GetTextChannel(709805453655277578);
                await modlogsChannel.SendMessageAsync("", false, log.Build());
            }

        }

        [Command("purge")]
        [Alias("p")]
        public async Task PurgeEmbedThing(int amount)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = Context.Guild.GetRole(629698730509074462);

            if (!user.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            if (amount <= 0)
            {
                await Context.Channel.SendErrorAsync("The number of messages you want to delete needs to be positive!");
                return;
            }

            var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync();
            var filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);
            var count = filteredMessages.Count();

            if (count == 0)
            {
                await Context.Channel.SendErrorAsync("There was an error executing this command!");
                return;
            }
            else
            {
                await (Context.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
                //await ReplyAsync($"Removed {count} {(count > 1 ? "messages" : "message")}.");
                await Context.Channel.DeleteMessageAsync(Context.Message.Id);

                var success = await ReplyAsync("Purge Complete!");
                const int delay = 2000;
                await Task.Delay(delay);
                await success.DeleteAsync();

                var log = new EmbedBuilder();
                log.WithTitle("Purged Messages");
                log.WithDescription($"`{amount}` message(s) were purged in <#{Context.Channel.Id}>");
                log.WithFooter($"Purged by: {user}");
                log.WithCurrentTimestamp();
                log.WithColor(Color.Blue);

                var modlogsChannel = Context.Guild.GetTextChannel(709805453655277578);
                await modlogsChannel.SendMessageAsync("", false, log.Build());

            }
        }

        [Command("purges")]
        public async Task PurgeCharacters(string substring, int amount)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = Context.Guild.GetRole(629698730509074462);

            if (!user.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            if (substring == null)
            {
                await Context.Channel.SendErrorAsync("Please provide the substring you would like to delete!");
                return;
            }

            if (amount <= 0)
            {
                await Context.Channel.SendErrorAsync("The number of messages you want to delete needs to be positive!");
                return;
            }

            var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync();
            var filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);
            var count = filteredMessages.Count();
            var substringFilter = messages.Where(x => !x.Content.Contains(substring));

            if (count == 0)
            {
                await Context.Channel.SendErrorAsync("There was an error executing this command!");
                return;
            }
            else
            {
                await (Context.Channel as ITextChannel).DeleteMessagesAsync(substringFilter);
                await Context.Channel.DeleteMessageAsync(Context.Message.Id);

                var success = await ReplyAsync("Purge Complete!");
                const int delay = 2000;
                await Task.Delay(delay);
                await success.DeleteAsync();
            }
        }

        [Command("rpfilter")]
        public async Task RolePlayFilter(string value = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = Context.Guild.GetRole(629698730509074462);
            var approvedReaction = Emote.Parse("<:Approved:787034785583333426>");
            var errorReaction = Emote.Parse("<:Error:787036714337566730>");

            if (!user.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            if (value == null)
            {
                await Context.Channel.SendErrorAsync("Please enter a value. `true/false` `on/off`");
                return;
            }
            else
            {
                switch (value.ToLower())
                {
                    case "on":
                        CommandHandler.rpFilter = true;
                        await Context.Message.AddReactionAsync(approvedReaction);
                        return;

                    case "true":
                        CommandHandler.rpFilter = true;
                        await Context.Message.AddReactionAsync(approvedReaction);
                        return;

                    case "off":
                        CommandHandler.rpFilter = false;
                        await Context.Message.AddReactionAsync(approvedReaction);
                        return;

                    case "false":
                        CommandHandler.rpFilter = false;
                        await Context.Message.AddReactionAsync(approvedReaction);
                        return;

                    default:
                        {
                            await Context.Channel.SendErrorAsync("Please enter a valid value. `true/false` `on/off`");
                            return;
                        }
                }
            }
        }

        [Command("usercount")]
        public async Task CountMessages(SocketGuildUser userAccount = null)
        {
            var user = Context.User as SocketGuildUser;

            var weekFilter = SnowflakeUtils.ToSnowflake(DateTimeOffset.UtcNow - TimeSpan.FromDays(7));
            var monthFilter = SnowflakeUtils.ToSnowflake(DateTimeOffset.UtcNow - TimeSpan.FromDays(31));
            var getWeek = await Context.Channel.GetMessagesAsync(weekFilter, Direction.After, 1000, CacheMode.AllowDownload).FlattenAsync();
            var getMonth = await Context.Channel.GetMessagesAsync(monthFilter, Direction.After, 1000, CacheMode.AllowDownload).FlattenAsync();

            if (userAccount == null)
            {
                var userWeekMessages = getWeek.Where(x => x.Author.Id == Context.User.Id);
                var userMonthMessages = getMonth.Where(x => x.Author.Id == Context.User.Id);

                var weekCount = userWeekMessages.Count();
                var monthCount = userMonthMessages.Count();

                var embed = new EmbedBuilder();
                embed.WithAuthor(user);
                embed.WithDescription($"**User Message Counts**\n7 Days: `{weekCount} messages`\n31 Days: `{monthCount} messages`");
                embed.WithColor(Color.Blue);
                embed.WithCurrentTimestamp();
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                var userWeekMessages = getWeek.Where(x => x.Author.Id == userAccount.Id);
                var userMonthMessages = getMonth.Where(x => x.Author.Id == userAccount.Id);

                var weekCount = userWeekMessages.Count();
                var monthCount = userMonthMessages.Count();

                var embed = new EmbedBuilder();
                embed.WithAuthor(userAccount);
                embed.WithDescription($"**User Message Counts**\n7 Days: `{weekCount} messages`\n31 Days: `{monthCount} messages`");
                embed.WithColor(Color.Blue);
                embed.WithCurrentTimestamp();
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        /*[Command("available")]
        public async Task AvailableCountries()
        {
            var filter = Context.Guild.Roles.Where(a => a.Name.Contains("|") && a.Members.Count() >= 1).ToList();

            var embed = new EmbedBuilder();
            embed.WithTitle("Available Counntries");
            embed.WithDescription(string.Join("\n", filter));
            embed.WithColor(Color.Green);
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }*/

        [RequireOwner]
        [Command("terminate")]
        public async Task TerminateClient()
        {
            await Context.Client.LogoutAsync();
        }
    }
}
