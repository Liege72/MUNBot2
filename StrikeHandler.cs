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
using System.Timers;

namespace Official_Bot
{
    public class StrikeHandler : ModuleBase<SocketCommandContext>
    {
        /*[Command("strikelist"]
        public async Task StrikeList()
        {
           

            }*/

        [Command("strike")]
        public async Task Strike(SocketGuildUser userAccount = null, [Remainder] string reason = null)
        {
            if (Context.Guild.Id.Equals(619319282777456641))
            {
                //user that uses the command below
                var user = Context.User as SocketGuildUser;
                //var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
                var roleStaff = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
                var license = Context.Guild.GetRole(700057375394234399);

                if (!user.Roles.Contains(license))
                {
                    var staffStrike = new EmbedBuilder();
                    staffStrike.WithTitle("Strike Error");
                    staffStrike.WithDescription("You do not have access to strike this user!");
                    staffStrike.WithColor(Color.Red);
                    await ReplyAsync("", false, staffStrike.Build());
                    return;
                }

                if (userAccount == null)
                {
                    var noUser = new EmbedBuilder();
                    noUser.WithTitle("Strike Error");
                    noUser.WithDescription("Please mention a user!");
                    noUser.WithColor(Color.Red);
                    await ReplyAsync("", false, noUser.Build());
                    return;
                }

                if (reason == null)
                {
                    var noReason = new EmbedBuilder();
                    noReason.WithTitle("Strike Error");
                    noReason.WithDescription("Please provide a reason!");
                    noReason.WithColor(Color.Red);
                    await ReplyAsync("", false, noReason.Build());
                    return;
                }

                if (user.Roles.Contains(license))
                {
                    var Strike = new EmbedBuilder();
                    Strike.WithTitle($"{userAccount.ToString()} has been sriked.");
                    Strike.AddField("Reason", reason, true);
                    Strike.AddField("Moderator", user.Mention, true);
                    Strike.AddField("Strike List", "```2   Strikes     Loose 5 % of your GDP\n3   Strikes     Production starved for 48h\n4   Strikes     Production starved for 72h\n5+  Strikes     Production starve for 48h + 2 % GDP loss```");
                    Strike.WithColor(Color.Green);

                    await ReplyAsync("", false, Strike.Build());

                    /*var StrikeMsg = new EmbedBuilder();
                    StrikeMsg.WithTitle($"You are striked in `{Context.Guild}`");
                    StrikeMsg.AddField($"Reason", reason, true);
                    StrikeMsg.WithFooter($"Striked by {Context.Message.Author.ToString()}");
                    StrikeMsg.WithCurrentTimestamp();
                    await userAccount.SendMessageAsync("", false, StrikeMsg.Build());*/

                    //alerts
                    var log = new EmbedBuilder();
                    log.WithTitle("Strike");
                    log.WithDescription($"**Offender:** {userAccount.Mention} \n **Reason:** `{reason}` \n **Moderator:** {user} \n **In:** <#{Context.Channel.Id}>");
                    log.WithCurrentTimestamp();
                    log.WithColor(Color.Red);

                    var guild = Context.Guild;

                    if (Context.Guild.Channels.Any(x => x.GetType() == typeof(SocketTextChannel) && x.Name == "server-logs"))
                    {
                        var alerts = (SocketTextChannel)Context.Guild.Channels.First(x => x.GetType() == typeof(SocketTextChannel) && x.Name == "server-logs");
                        await alerts.SendMessageAsync("", false, log.Build());
                    }

                    /*else
                    {
                        var embed = new EmbedBuilder();
                        embed.WithTitle("Error");
                        embed.WithDescription("Use the `!create modlogs` command to create a modlogs channel.");
                        embed.WithColor(Color.Red);

                        await ReplyAsync("", false, embed.Build());
                    }*/

                    AddStrikeLogs(userAccount.Id, Action.Strike, Context.Message.Author.Id, reason, userAccount.Username);

                }
            }
            else
            {
            }
        }

        static string StrikeLogsPath = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}StrikeLogs.json";

        public async void AddStrikeLogs(ulong userID, Action action, ulong ModeratorID, string reason, string username)
        {
            var id = Context.Guild.Id;
            if (currentLogs.Any(x => x.GuildID == id))
            {
                var guildLogs = currentLogs.Find(x => x.GuildID == id);
                if (guildLogs.Users.Any(x => x.userId == userID))
                {
                    guildLogs.Users[guildLogs.Users.FindIndex(x => x.userId == userID)].Logs.Add(new UserStrikeLogs()
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
                        Logs = new List<UserStrikeLogs>()
                        {
                            { new UserStrikeLogs(){
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
                currentLogs.Add(new StrikeLogsJson()
                {
                    GuildID = id,
                    Users = new List<User>()
                    {
                        new User()
                        {
                            userId = userID,
                            username = username,
                            Logs = new List<UserStrikeLogs>()
                            {
                                new UserStrikeLogs()
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

            if (currentLogs.Count == 3)
            {
                var role = Context.Guild.GetRole(636620470048915465);
                var user = Context.Guild.GetUser(userID);
                await user.AddRoleAsync(role);

                Timer t = new Timer();
                t.Interval = 3000;
                t.Elapsed += T_Elapsed;
                t.Start();
            }

            SaveStrikeLogs();
        }
        public async void T_Elapsed(object user, ElapsedEventArgs e)
        {
            var role = Context.Guild.GetRole(636620470048915465);
        }

        public static List<StrikeLogsJson> LoadStrikeLogs()
        {
            try
            {
                var d = JsonConvert.DeserializeObject<List<StrikeLogsJson>>(File.ReadAllText(StrikeLogsPath));
                if (d == null) { throw new Exception(); }
                return d;
            }
            catch //(Exception ex)
            {
                return new List<StrikeLogsJson>();
            }


        }
        public static List<StrikeLogsJson> currentLogs { get; set; } = LoadStrikeLogs();
        static public void SaveStrikeLogs()
        {
            string json = JsonConvert.SerializeObject(currentLogs);
            File.WriteAllText(StrikeLogsPath, json);
        }
        public class StrikeLogsJson
        {
            public List<User> Users { get; set; }
            public ulong GuildID { get; set; }
        }
        public class User
        {
            public List<UserStrikeLogs> Logs { get; set; }
            public ulong userId { get; set; }
            public string username { get; set; }
        }
        public class UserStrikeLogs
        {
            public string Reason { get; set; }
            public Action Action { get; set; }
            public ulong ModeratorID { get; set; }
            public string Date { get; set; }
            DateTime Timer { get; set; }
        }
        public enum Action
        {
            Strike
        }

        /*[Command("strikelogs")]
        [Alias("ml")]
        public async Task StrikeLogs(string mention)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
            var mentions = Context.Message.MentionedUsers;

            if (!user.Roles.Contains(roleStaff) && !user.GuildPermissions.ManageMessages)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "You do not have permission to execute this command",
                    Description = "You do not have the valid permission to execute this command",
                    Color = Color.Red
                }.Build());
                return;
            }

            if (mentions.Count == 0)
            {
                await Context.Channel.SendMessageAsync("", false);
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
                            Name = 
                            Enum.GetName(typeof(Action), log.Action),
                            Value = $"{log.Action}Reason: {log.Reason}\nModerator: <@{log.ModeratorID}>\nDate: {log.Date}"
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
                        Title = $"Modlogs for ({user1.Id})",
                        Description = "This user has no logs! :D",
                        Color = Color.Green
                    }.Build());
                    return;
                }
            }
        }*/

        [Command("strikes")]
        [Alias("str")]
        public async Task ClearStrikes(string user1, int number = 999)
        {
            if (Context.Guild.Id.Equals(619319282777456641))
            {
                var user = Context.User as SocketGuildUser;
                var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
                var mentions = Context.Message.MentionedUsers;
                var license = Context.Guild.GetRole(700057375394234399);

                if (!user.Roles.Contains(roleStaff) && !user.Roles.Contains(license))
                {
                    await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                    {
                        Title = "Error",
                        Description = "You do not have the valid permission to execute this command",
                        Color = Color.Red
                    }.Build());
                    return;
                }

                Regex r = new Regex("(\\d{18}|\\d{17})");
                if (!r.IsMatch(user1))
                {
                    await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                    {
                        Title = "Error",
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
                        Title = "Error",
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
                                Title = $"Strikes for **{usrnm}**",
                                Color = Color.Blue,
                                Description = $"Strikes for {usrlogs.username},\nTo remove a strike type `!strikes <user> <log number>`\n```2   Strikes     Loose 5 % of your GDP\n3   Strikes     Production starved for 48h\n4   Strikes     Production starved for 72h\n5+  Strikes     Production starve for 48h + 2 % GDP loss```",
                                Fields = new List<EmbedFieldBuilder>()
                            };
                            for (int i = 0; i != usrlogs.Logs.Count; i++)
                            {
                                var log = usrlogs.Logs[i];
                                b.Fields.Add(new EmbedFieldBuilder()
                                {
                                    IsInline = false,
                                    Name = $"{log.Action}" + $"[{(i + 1).ToString()}]",
                                    Value =
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
                                Title = "Error",
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
                        string usrnm = Context.Guild.GetUser(usrlogs.userId) == null ? usrlogs.username : Context.Guild.GetUser(usrlogs.userId).ToString();
                        EmbedBuilder b = new EmbedBuilder()
                        {
                            Title = $"Successfully cleared a strike for **{usrnm}**",
                            Color = Color.Green,
                            Description = $"Strikes for {usrlogs.username},\nTo remove a log type `!strikes <user> <log number>`\n",
                            Fields = new List<EmbedFieldBuilder>()
                        };
                        for (int i = 0; i != usrlogs.Logs.Count; i++)
                        {
                            var log = usrlogs.Logs[i];
                            b.Fields.Add(new EmbedFieldBuilder()
                            {
                                IsInline = false,
                                Name = $"{log.Action}" + $"[{(i + 1).ToString()}]",
                                Value =
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
                        Title = "Error",
                        Description = $"The user <@{id}> has no logs!",
                        Color = Color.Red,
                    };
                    await Context.Channel.SendMessageAsync("", false, b.Build());
                }
            }
            else
            {
            }
        }
    }
}
