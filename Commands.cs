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
using Discord.Addons.Interactive;

namespace MUNBot.Modules
{
    public class Commands : InteractiveBase<SocketCommandContext>
    {
        [Command("resetslowmode")]
        public async Task ResetSlowmode(int value, string all)
        {
            var staffRole = Context.Guild.GetRole(700057375394234399);
            var supervisorRole = Context.Guild.GetRole(700057375394234399);
            var user = Context.User as SocketGuildUser;

            if (!user.Roles.Contains(staffRole) && !user.Roles.Contains(supervisorRole))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            if (all == null)
            {
                var contextChannel = Context.Guild.GetTextChannel(Context.Channel.Id);
                await contextChannel.ModifyAsync(x =>
                {
                    x.SlowModeInterval = 0;
                });

                await Context.Channel.SendSuccessAsync($"Modified channels slowmode to {value} second(s)");
            }

            if (all.Equals("roleplay"))
            {
                var msg = await Context.Channel.SendMessageAsync($"Setting the slowmode to `{value}` in all the roleplay channels...");

                var newsfeed = Context.Guild.GetTextChannel(626782653399695401);
                var electionsfeed = Context.Guild.GetTextChannel(664922494037262445);
                var reformsfeed = Context.Guild.GetTextChannel(644521248436518932);
                var sanctionsfeed = Context.Guild.GetTextChannel(728680875860164709);
                var militaryfeed = Context.Guild.GetTextChannel(671751119478849556);
                var breakingnews = Context.Guild.GetTextChannel(629328875024941056);

                await newsfeed.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await electionsfeed.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await reformsfeed.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await sanctionsfeed.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await militaryfeed.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await breakingnews.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await msg.ModifyAsync(x =>
                {
                    x.Content = $"Successfully set the slowmode to `{value}` in all the roleplay channels!";
                });
            }

            if (all.Equals("production"))
            {
                var msg = await Context.Channel.SendMessageAsync("Setting the slowmode to on all the production channels...");

                var production = Context.Guild.GetTextChannel(626784835045687317);
                var research = Context.Guild.GetTextChannel(626784812656754709);

                await production.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await research.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await msg.ModifyAsync(x =>
                {
                    x.Content = $"Successfully set the slowmode to `{value}` in all the production channels!";
                });
            }

            if (all.Equals("all"))
            {
                var msg = await Context.Channel.SendMessageAsync($"Setting the slowmode to `{value}` in all channels...");

                var production = Context.Guild.GetTextChannel(626784835045687317);
                var research = Context.Guild.GetTextChannel(626784812656754709);

                var newsfeed = Context.Guild.GetTextChannel(626782653399695401);
                var electionsfeed = Context.Guild.GetTextChannel(664922494037262445);
                var reformsfeed = Context.Guild.GetTextChannel(644521248436518932);
                var sanctionsfeed = Context.Guild.GetTextChannel(728680875860164709);
                var militaryfeed = Context.Guild.GetTextChannel(671751119478849556);
                var breakingnews = Context.Guild.GetTextChannel(629328875024941056);

                await newsfeed.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await electionsfeed.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await reformsfeed.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await sanctionsfeed.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await militaryfeed.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await breakingnews.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await production.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await research.ModifyAsync(x =>
                {
                    x.SlowModeInterval = value;
                });

                await msg.ModifyAsync(x =>
                {
                    x.Content = $"Successfully set the slowmode to `{value}` in all the channels!";
                });
            }
        }

        [Command("investigate")]
        public async Task InvestigateUser(SocketGuildUser userAccount = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Under Investigation");
            var logChannel = Context.Guild.GetTextChannel(732307234989670411);
            var un = Context.Guild.GetRole(727905836331958373);

            if (!Context.Guild.Id.Equals(619319282777456641))
            {
                return;
            }
            else
            {
                if (!user.Roles.Contains(roleStaff))
                {
                    await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                    return;
                }
                else
                {
                    if (userAccount == null)
                    {
                        await Context.Channel.SendErrorAsync("Please provide a user!");
                        return;
                    }

                    if (userAccount.Roles.Contains(role))
                    {
                        await Context.Channel.SendErrorAsync("This user is already under investigation!");
                        return;
                    }
                    else
                    {
                        if (userAccount.Roles.Contains(un))
                        {
                            await userAccount.RemoveRoleAsync(un);
                        }

                        var icon = userAccount.GetAvatarUrl();

                        var embed = new EmbedBuilder();
                        embed.WithAuthor($"{userAccount} is under investigation!", $"{icon}");
                        embed.WithDescription($"{userAccount.Mention} has been placed under investigation and DMed instructions!");
                        embed.WithColor(Color.Red);
                        await ReplyAsync("", false, embed.Build());
                        await userAccount.AddRoleAsync(role);

                        var log = new EmbedBuilder();
                        log.WithTitle("Investigation");
                        log.WithDescription($"{userAccount.Mention} has been placed under investigation!");
                        log.WithCurrentTimestamp();
                        log.WithFooter($"Moderator: {user}");
                        log.WithColor(Color.DarkRed);
                        await logChannel.SendMessageAsync("", false, log.Build());

                        var dm = new EmbedBuilder();
                        dm.WithTitle("You have been placed under investigation!");
                        dm.WithDescription("A staff member has placed you under investigation in the **Model United Nations** server! Please read [here](https://discord.gg/sn9gQ5) for more info or use the `!verify` command.");
                        dm.WithColor(Color.Red);
                        await userAccount.SendMessageAsync("", false, dm.Build());



                    }
                }
            }
        }

        /*[Command("verify")]
        public async Task VerifyUser(SocketGuildUser userAccount = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Under Investigation");
            var logChannel = Context.Guild.GetTextChannel(732307234989670411);
            //var verified = Context.Guild.GetRole(630765674045636609);

            if (!Context.Guild.Id.Equals(619319282777456641))
            {
                return;
            }
            else
            {
                if (!user.Roles.Contains(roleStaff))
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Error");
                    embed.WithDescription("You do not have access to use this command!");
                    embed.WithColor(Color.Red);
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    if (userAccount == null)
                    {
                        var embed = new EmbedBuilder();
                        embed.WithTitle("Error");
                        embed.WithDescription("Please provide a user!");
                        embed.WithColor(Color.Red);
                        await ReplyAsync("", false, embed.Build());
                    }

                    if (!userAccount.Roles.Contains(role))
                    {
                        var embed = new EmbedBuilder();
                        embed.WithTitle("Error");
                        embed.WithDescription("This user is already verified!");
                        embed.WithColor(Color.Red);
                        await ReplyAsync("", false, embed.Build());
                    }
                    else
                    {
                        var icon = userAccount.GetAvatarUrl();

                        var embed = new EmbedBuilder();
                        embed.WithAuthor($"{userAccount} is verified!", $"{icon}");
                        embed.WithDescription($"{userAccount.Mention} has been verified and has been notified!");
                        embed.WithColor(Color.Green);
                        await ReplyAsync("", false, embed.Build());
                        await userAccount.RemoveRoleAsync(role);

                        var log = new EmbedBuilder();
                        log.WithTitle("Verification");
                        log.WithDescription($"{userAccount.Mention} has been verified!");
                        log.WithCurrentTimestamp();
                        log.WithFooter($"Moderator: {user}");
                        log.WithColor(Color.Blue);
                        await logChannel.SendMessageAsync("", false, log.Build());

                        var dm = new EmbedBuilder();
                        dm.WithTitle("You have been verified!");
                        dm.WithDescription("A staff member has verified you in the **Model United Nations** server! You can now select a country [here](https://discord.gg/XezctX6).");
                        dm.WithColor(Color.Green);
                        await userAccount.SendMessageAsync("", false, dm.Build());
                    }
                }
            }
        }*/

        [Command("verify", RunMode = RunMode.Async)]
        public async Task VerifyUser()
        {
            var user = Context.User as SocketUser;

            if (Context.Guild == null)
            {
                await ReplyAsync("**1)** How did you find the server?");
                var find = await NextMessageAsync();

                await ReplyAsync("**2)** Why is your account so new?");
                var newAcc = await NextMessageAsync();

                await ReplyAsync("**3)** Anything else to say?");
                var anything = await NextMessageAsync();

                var userId = Context.User.Id;

                var embed = new EmbedBuilder();
                embed.WithAuthor(user);
                embed.AddField("How did you find the server?", $"1{find}");
                embed.AddField("Why is your account so new?", $"2{newAcc}");
                embed.AddField("Anything else to say?", $"3{anything}");
                embed.WithColor(Color.Red);

                await ReplyAsync("Thank you for submitting your answers. The staff team will review these within the next hour.");

                DiscordSocketClient _client = new DiscordSocketClient();
                await _client.GetGuild(619319282777456641).GetTextChannel(742494136912969810).SendMessageAsync("", false, embed.Build());
            }

            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Under Investigation");
            var logChannel1 = Context.Guild.GetTextChannel(732307234989670411);
            //var verified = Context.Guild.GetRole(630765674045636609);
        }

        [Command("add")]
        public async Task AddCountry(SocketGuildUser userAccount = null, IRole country = null)
        {
            var supervisorRole = Context.Guild.GetRole(700057375394234399);
            var user = Context.User as SocketGuildUser;
            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
            var cc = Context.Guild.GetRole(626783654395379753);
            var uc = Context.Guild.GetRole(626783494508380160);
            var logChannel = Context.Guild.GetTextChannel(732307234989670411);
            var spectator = Context.Guild.GetRole(631219794007293995);

            if (!user.Roles.Contains(roleStaff) && !user.Roles.Contains(supervisorRole))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }
            else
            {
                if (userAccount == null)
                {
                    await Context.Channel.SendErrorAsync("Please provide a user!");
                    return;
                }

                if (country == null)
                {
                    await Context.Channel.SendErrorAsync("Please provide a country!");
                    return;
                }

                if (userAccount.Roles.Contains(country))
                {
                    await Context.Channel.SendErrorAsync($"This user already has {country.Mention}");
                    return;
                }

                if (userAccount.Roles.Contains(cc))
                {
                    await Context.Channel.SendErrorAsync("This user already has a country!");
                    return;
                }

                if (user.Roles.Contains(roleStaff) || user.Roles.Contains(supervisorRole))
                {
                    if (userAccount.Roles.Contains(spectator))
                    {
                        await userAccount.RemoveRoleAsync(spectator);
                    }
                    var icon = userAccount.GetAvatarUrl();
                    var countryColor = country.Color;
                    var onTopic = Context.Guild.GetTextChannel(629325734791348244);

                    await userAccount.AddRoleAsync(country);
                    await userAccount.AddRoleAsync(cc);
                    await userAccount.RemoveRoleAsync(uc);

                    var success = new EmbedBuilder();
                    success.WithTitle("Success");
                    success.WithDescription($"Gave {userAccount.Mention}, {country.Mention}");
                    success.WithColor(countryColor);
                    await ReplyAsync("", false, success.Build());

                    var dm = new EmbedBuilder();
                    dm.WithTitle($"Hey {userAccount}!");
                    dm.WithDescription("Congrats on choosing a country! Below, I have listed some options for help should you ever need it:\n - Check out the <#629675521357119508> page. It has some easily answered questions.\n - Use the `!ticket` command to get direct support from the staff team.\n - Use the `!ask <question>` command in the <#749257515996414032> channel.");
                    dm.WithColor(Color.Blue);
                    await userAccount.SendMessageAsync("", false, dm.Build());

                    var log = new EmbedBuilder();
                    log.WithTitle("Country Added");
                    log.WithDescription($"{userAccount.Mention} was added to {country.Mention}!");
                    log.WithCurrentTimestamp();
                    log.WithFooter($"Moderator: {user}");
                    log.WithColor(Color.Green);
                    await logChannel.SendMessageAsync("", false, log.Build());

                    var embed = new EmbedBuilder();
                    embed.WithAuthor($"{userAccount}", $"{icon}");
                    embed.WithDescription($"Congrats {userAccount.Mention}, you now have the country: {country.Mention}!");
                    embed.WithColor(countryColor);
                    await onTopic.SendMessageAsync($"{country.Mention}", false, embed.Build());
                }
            }
        }

        [Command("remove")]
        public async Task RemoveCountry(SocketGuildUser userAccount = null, IRole country = null)
        {
            var supervisorRole = Context.Guild.GetRole(700057375394234399);
            var user = Context.User as SocketGuildUser;
            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
            var cc = Context.Guild.GetRole(626783654395379753);
            var uc = Context.Guild.GetRole(626783494508380160);
            var un = Context.Guild.GetRole(727905836331958373);
            var logChannel = Context.Guild.GetTextChannel(732307234989670411);

            if (!user.Roles.Contains(roleStaff) && !user.Roles.Contains(supervisorRole))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }
            else
            {
                if (userAccount == null)
                {
                    await Context.Channel.SendErrorAsync("Please provide a user!");
                    return;
                }

                if (country == null)
                {
                    await Context.Channel.SendErrorAsync("Please provide a country!");
                    return;
                }

                if (!userAccount.Roles.Contains(cc))
                {
                    await Context.Channel.SendErrorAsync("This user does not have a country!");
                    return;
                }

                if (user.Roles.Contains(roleStaff) || user.Roles.Contains(supervisorRole))
                {
                    if (userAccount.Roles.Contains(un))
                    {
                        await userAccount.RemoveRoleAsync(un);
                    }

                    var icon = userAccount.GetAvatarUrl();
                    var countryColor = country.Color;
                    var onTopic = Context.Guild.GetTextChannel(629325734791348244);

                    await userAccount.RemoveRoleAsync(country);
                    await userAccount.RemoveRoleAsync(cc);
                    await userAccount.AddRoleAsync(uc);

                    var success = new EmbedBuilder();
                    success.WithTitle("Success");
                    success.WithDescription($"Removed {userAccount.Mention} from {country.Mention}");
                    success.WithColor(countryColor);
                    await ReplyAsync("", false, success.Build());

                    var log = new EmbedBuilder();
                    log.WithTitle("Country Removed");
                    log.WithDescription($"{userAccount.Mention} was removed from {country.Mention}");
                    log.WithCurrentTimestamp();
                    log.WithFooter($"Moderator: {user}");
                    log.WithColor(Color.Red);
                    await logChannel.SendMessageAsync("", false, log.Build());

                }

            }
        }

        [Command("spectator add")]
        public async Task AddSpectator(SocketGuildUser userAccount = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
            var specRole = Context.Guild.GetRole(631219794007293995);
            var uc = Context.Guild.GetRole(626783494508380160);
            if (!user.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }
            else
            {
                if (userAccount == null)
                {
                    await Context.Channel.SendErrorAsync("Please provide a user!");
                    return;
                }

                if (userAccount.Roles.Contains(specRole))
                {
                    await Context.Channel.SendErrorAsync($"This user already has the {specRole.Mention} role!");
                    return;
                }

                if (user.Roles.Contains(roleStaff))
                {
                    await userAccount.AddRoleAsync(specRole);

                    await Context.Channel.SendSuccessAsync($"Gave {userAccount.Mention} the {specRole.Mention} role!");
                    return;
                }
            }
        }

        [Command("spectator remove")]
        public async Task RemoveSpectator(SocketGuildUser userAccount = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
            var specRole = Context.Guild.GetRole(631219794007293995);
            var uc = Context.Guild.GetRole(626783494508380160);

            if (!user.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }
            else
            {
                if (userAccount == null)
                {
                    await Context.Channel.SendErrorAsync("Please provide a user!");
                    return;
                }

                if (!userAccount.Roles.Contains(specRole))
                {
                    await Context.Channel.SendErrorAsync($"This user does not have the {specRole.Mention} role!");
                    return;
                }


                if (user.Roles.Contains(roleStaff))
                {
                    await userAccount.RemoveRoleAsync(specRole);

                    await Context.Channel.SendSuccessAsync($"Remove {userAccount.Mention} from the {specRole.Mention} role!");
                    return;
                }
            }
        }

        [Command("rule")]
        public async Task Rules(int ruleNum = 0)
        {

            if (Context.Guild.Id.Equals(619319282777456641))
            {
                if (ruleNum.Equals(0))
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Error");
                    embed.WithDescription("Please enter a rule number!");
                    embed.WithColor(Color.Red);
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                if (ruleNum.Equals(1))
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Rule 1");
                    embed.WithDescription("If behavior is deemed \"inappropriate\" a punishment will follow");
                    embed.WithColor(new Discord.Color(114, 137, 218));
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                if (ruleNum.Equals(2))
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Rule 2");
                    embed.WithDescription("All alternate accounts are banned in accordance to discord TOS");
                    embed.WithColor(new Discord.Color(114, 137, 218));
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                if (ruleNum.Equals(3))
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Rule 3");
                    embed.WithDescription("Asking for staff roles/impersonating staff can result in a staff team blacklist");
                    embed.WithColor(new Discord.Color(114, 137, 218));
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                if (ruleNum.Equals(4))
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Rule 4");
                    embed.WithDescription("Threats to DDOS/DOX/Hurt someone/etc are prohibited and can result in a ban");
                    embed.WithColor(new Discord.Color(114, 137, 218));
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                if (ruleNum.Equals(5))
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Rule 5");
                    embed.WithDescription("Do NOT share a users DMs or personal information without their permission");
                    embed.WithColor(new Discord.Color(114, 137, 218));
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                if (ruleNum.Equals(6))
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Rule 6");
                    embed.WithDescription("Insulting ones sexuality/race/sex/religious beliefs/etc is discouraged and is punishable");
                    embed.WithColor(new Discord.Color(114, 137, 218));
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                if (ruleNum.Equals(7))
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Rule 7");
                    embed.WithDescription("Spamming, excessive mentioning, posting NSFW content, and self promoting/advertising is prohibited within the server and DMs");
                    embed.WithColor(new Discord.Color(114, 137, 218));
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                if (ruleNum.Equals(8))
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Rule 8");
                    embed.WithDescription("Respect all members within the server. Insults/Harassment/Bullying/Trolling is punishable.");
                    embed.WithColor(new Discord.Color(114, 137, 218));
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                if (ruleNum.Equals(9))
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Rule 9");
                    embed.WithDescription("Post content in the correct channels");
                    embed.WithColor(new Discord.Color(114, 137, 218));
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                if (ruleNum.Equals(10))
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Rule 10");
                    embed.WithDescription("Custom status's that break these rules are also punishable");
                    embed.WithColor(new Discord.Color(114, 137, 218));
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                if (ruleNum.Equals("all"))
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("All Rules");
                    embed.AddField("Rule 1", "1) If behavior is deemed \"inappropriate\" a punishment will follow");
                    embed.AddField("Rule 2", "2) All alternate discord accounts are banned in accordance to discord TOS");
                    embed.AddField("Rule 3", "3) Asking for staff roles/ impersonating staff can result in a staff team blacklist");
                    embed.AddField("Rule 4", "4) Threats / Actions that relate to DDOS/ DOX / Hurt someone / etc are prohibited and can result in a ban.");
                    embed.AddField("Rule 5", "5) Do NOT share a users DMs or personal information without their permission");
                    embed.AddField("Rule 6", "6) Insulting ones sexuality / race / sex / religious beliefs / etc is discouraged and is punishable");
                    embed.AddField("Rule 7", "7) Spamming, excessive mentioning, posting NSFW content, harassment, bullying and self promoting / advertising is prohibited within the server and DMs");
                    embed.AddField("Rule 8", "8) Communicate in English only(exception for #other-languages)");
                    embed.AddField("Rule 9", "9) Post content in the correct channels");
                    embed.AddField("Rule 10", "10) Custom status's that break these rules are also punishable");
                    embed.WithColor(new Discord.Color(114, 137, 218));
                    await ReplyAsync("", false, embed.Build());
                    return;
                }
            }
            else
            {
                return;
            }
        }

        [Command("war")]
        public async Task WarDeclaration(IRole countryName = null, IRole opposingCountry = null, string text = null)
        {
            var user = Context.User as SocketGuildUser;
            var country = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Confirmed Country");

            if (!user.Roles.Contains(country))
            {
                await Context.Channel.SendErrorAsync("You must have a country to use this command!");

                return;
            }

            if (countryName == null)
            {
                await Context.Channel.SendErrorAsync("Please provide your country role!");
                return;
            }

            if (opposingCountry == null)
            {
                await Context.Channel.SendErrorAsync("Please provide the opposing country's role!");
                return;
            }

            if (text == null)
            {
                await Context.Channel.SendErrorAsync("Please provide a link to your document!");
                return;
            }

            if (!user.Roles.Contains(countryName))
            {
                await Context.Channel.SendErrorAsync("Please make sure the first role you mention is your country and the second role you ping is the opposing country!");;
                return;
            }

            if (user.Roles.Contains(opposingCountry))
            {
                await Context.Channel.SendErrorAsync("Please make sure the first role you mention is your country and the second role you ping is the opposing country!");
                return;
            }

            if (countryName.Id.Equals(opposingCountry.Id))
            {
                await Context.Channel.SendErrorAsync("Please make sure the first role you mention is your country and the second role you ping is the opposing country!");
                return;
            }

            if (!text.Contains("docs.google.com"))
            {
                await Context.Channel.SendErrorAsync("The link you entered is invalid!");
                return;
            }

            if (user.Roles.Contains(country))
            {
                var channel = Context.Guild.GetTextChannel(629328771513712640);

                var doc = new EmbedBuilder();
                doc.WithTitle("Success!");
                doc.WithTitle($"Your war declaration has been sent!");
                doc.WithDescription($"See it in <#629328771513712640>!");
                doc.WithFooter($"{user}");
                doc.WithColor(Color.Green);

                await ReplyAsync("", false, doc.Build());

                var log = new EmbedBuilder();
                log.WithTitle("New War Declaration!");
                log.AddField("Countries", $"{countryName.Mention} vs {opposingCountry.Mention}", true);
                log.AddField("Declaration", $"[View the War Declaration]({text})", true);
                log.WithColor(Color.Blue);
                log.WithFooter($"Submitted by: {user}");
                log.WithCurrentTimestamp();

                await channel.SendMessageAsync("", false, log.Build());
            }
        }

        [Command("war1")]
        public async Task WarHelp()
        {
            var warHelp = new EmbedBuilder();
            warHelp.WithTitle("War Declartion");
            warHelp.WithDescription("Use the [template document](https://docs.google.com/document/d/1J4FUW2PTkEQTBsWaRO-RTENeLx6FWHY7lCUU1liawnc/edit?usp=sharing) as an example of your war declaration. When you have completed your declaration, please use the following command: `!war <your country role> <opposing country role> <link to document>`.");
            warHelp.WithColor(Color.Blue);
            await ReplyAsync("", false, warHelp.Build());
            return;
        }

        [Command("un add")]
        public async Task AddUNMember(SocketGuildUser userAccount = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
            var un = Context.Guild.GetRole(727905836331958373);
            var uc = Context.Guild.GetRole(626783494508380160);
            var cc = Context.Guild.GetRole(626783654395379753);
            var sg = Context.Guild.GetRole(633679080654372904);

            if (!user.Roles.Contains(roleStaff) && (!user.Roles.Contains(sg)))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }
            else
            {
                if (userAccount == null)
                {
                    await Context.Channel.SendErrorAsync("Please provide a user!");
                    return;
                }

                if (userAccount.Roles.Contains(uc))
                {
                    await Context.Channel.SendErrorAsync("This user does not have a country!");
                    return;
                }

                if (userAccount.Roles.Contains(un))
                {
                    await Context.Channel.SendErrorAsync($"This user already has the {un.Mention} role!");
                    return;
                }

                if (user.Roles.Contains(roleStaff) || user.Roles.Contains(sg))
                {
                    await userAccount.AddRoleAsync(un);

                    await Context.Channel.SendSuccessAsync($"Gave {userAccount.Mention} the {un.Mention} role!");
                    return;
                }
            }
        }

        [Command("un remove")]
        public async Task RemoveUNMember(SocketGuildUser userAccount = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
            var un = Context.Guild.GetRole(727905836331958373);
            var uc = Context.Guild.GetRole(626783494508380160);
            var cc = Context.Guild.GetRole(626783654395379753);
            var sg = Context.Guild.GetRole(633679080654372904);

            if (!user.Roles.Contains(roleStaff) && (!user.Roles.Contains(sg)))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }
            else
            {
                if (userAccount == null)
                {
                    await Context.Channel.SendErrorAsync("Please provide a user!");
                    return;
                }

                if (userAccount.Roles.Contains(uc))
                {
                    await Context.Channel.SendErrorAsync("This user does not have a country!");
                    return;
                }

                if (!userAccount.Roles.Contains(un))
                {
                    await Context.Channel.SendErrorAsync($"This user does not have the {un.Mention} role!");
                    return;
                }

                if (user.Roles.Contains(roleStaff) || user.Roles.Contains(sg))
                {
                    await userAccount.RemoveRoleAsync(un);

                    await Context.Channel.SendSuccessAsync($"Removed {userAccount.Mention} from the {un.Mention} role!");
                    return;
                }
            }
        }

        /*[Command("poll")]
        public async Task Suggestion(ITextChannel channel = null, [Remainder] string poll = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");

            if (!Context.Guild.Id.Equals(619319282777456641))
            {
                return;
            }
            else
            {
                if (!user.Roles.Contains(roleStaff))
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Error");
                    embed.WithDescription("You do not have access to use this command!");
                    embed.WithColor(Color.Red);
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                if (channel == null)
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Error");
                    embed.WithDescription("Please include a channel!");
                    embed.WithColor(Color.Red);
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                if (poll == null)
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Error");
                    embed.WithDescription("Please include a poll question!");
                    embed.WithColor(Color.Red);
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                if (user.Roles.Contains(roleStaff))
                {
                    var embed2 = new EmbedBuilder();
                    embed2.WithTitle("Success!");
                    embed2.WithDescription($"{user.Mention}, your poll has been posted in <#{channel.Id}>");
                    embed2.WithColor(Color.Green);
                    await ReplyAsync("", false, embed2.Build());

                    var embed = new EmbedBuilder();
                    embed.WithTitle("Poll");
                    embed.WithDescription($"{poll}");
                    embed.WithCurrentTimestamp();
                    //embed.WithFooter($"Submitted by: {Context.User}");
                    embed.WithColor(Color.Purple);

                    var reactMessage = await channel.SendMessageAsync("", false, embed.Build());
                    await reactMessage.AddReactionsAsync(new IEmote[] { new Emoji("✅"), new Emoji("❌") });

                    if (channel == Context.Guild.Channels)
                    {
                        var embed1 = new EmbedBuilder();
                        embed.WithAuthor(user);
                        embed.WithTitle("Poll");
                        embed.WithDescription($"{poll}");
                        embed.WithCurrentTimestamp();
                        //embed.WithFooter($"Submitted by: {Context.User}");
                        embed.WithColor(Color.Purple);

                        var reactMessage1 = await channel.SendMessageAsync("", false, embed1.Build());
                        await reactMessage.AddReactionsAsync(new IEmote[] { new Emoji("✅"), new Emoji("❌") });
                    }
                }
            }
        }*/

        /*[Command("void")]
        public async Task VoidMessage(SocketGuildUser userAccount = null, ulong messageId = 0, [Remainder] string reason = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
            var license = Context.Guild.GetRole(700057375394234399);

            if (!Context.Guild.Id.Equals(619319282777456641))
            {
                await ReplyAsync("", false);
            }
            else
            {
                if (!user.Roles.Contains(license))
                {
                    var noPerms = new EmbedBuilder();
                    noPerms.WithTitle("Error");
                    noPerms.WithDescription("You do not have access to use this command!");
                    noPerms.WithColor(Color.Red);

                    await ReplyAsync("", false, noPerms.Build());
                    return;
                }

                if (userAccount == null)
                {
                    var noString = new EmbedBuilder();
                    noString.WithTitle("Error");
                    noString.WithDescription("Please include a user, messageID and a reason. (arg userAccount)");
                    noString.WithFooter("!void <user> <message ID> <reason>");
                    noString.WithColor(Color.Red);

                    await ReplyAsync("", false, noString.Build());
                    return;
                }

                if (messageId == 0)
                {
                    var noString = new EmbedBuilder();
                    noString.WithTitle("Error");
                    noString.WithDescription("Please include a user, messageId and a reason (arg messageId)");
                    noString.WithFooter("!void <user> <message ID> <reason>");
                    noString.WithColor(Color.Red);

                    await ReplyAsync("", false, noString.Build());
                    return;
                }

                if (reason == null)
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Voided");
                    embed.AddField("Action", $"[Jump to action](https://discordapp.com/channels/" + $"{Context.Guild.Id}/{Context.Channel.Id}/{messageId})", true);
                    embed.WithFooter($"Voided By: {user.Username}");
                    embed.WithColor(Color.Red);

                    var reaction = Emote.Parse("<:Voided:762029395778207754>");
                    var getMessage = await Context.Channel.GetMessageAsync(messageId);
                    await getMessage.AddReactionAsync(reaction);

                    var voidchannel = (SocketTextChannel)Context.Guild.Channels.First(x => x.GetType() == typeof(SocketTextChannel) && x.Name == "voided-actions");

                    var success = new EmbedBuilder();
                    success.WithTitle("Success");
                    success.WithDescription($"Action has been voided. See in <#{voidchannel.Id}>!");
                    success.WithColor(Color.Green);
                    var tempSuccess = await ReplyAsync("", false, success.Build());

                    await Context.Channel.DeleteMessageAsync(Context.Message.Id);
                    await voidchannel.SendMessageAsync($"{userAccount.Mention}", false, embed.Build());

                    const int delay = 1000;
                    await Task.Delay(delay);
                    await tempSuccess.DeleteAsync();
                    return;
                }


                if (user.Roles.Contains(license))
                {
                    var embed = new EmbedBuilder();
                    embed.WithTitle("Voided");
                    embed.AddField("Action", $"[Jump to action](https://discordapp.com/channels/" + $"{Context.Guild.Id}/{Context.Channel.Id}/{messageId})", true);
                    embed.AddField("Reason", $"{reason}", true);
                    embed.WithFooter($"Voided By: {user.Username}");
                    embed.WithColor(Color.Red);

                    var reaction = Emote.Parse("<:Voided:762029395778207754>");
                    var getMessage = await Context.Channel.GetMessageAsync(messageId);
                    await getMessage.AddReactionAsync(reaction);

                    var voidchannel = (SocketTextChannel)Context.Guild.Channels.First(x => x.GetType() == typeof(SocketTextChannel) && x.Name == "voided-actions");

                    var success = new EmbedBuilder();
                    success.WithTitle("Success");
                    success.WithDescription($"Action has been voided. See in <#{voidchannel.Id}>!");
                    success.WithColor(Color.Green);
                    var tempSuccess = await ReplyAsync("", false, success.Build());

                    await Context.Channel.DeleteMessageAsync(Context.Message.Id);
                    await voidchannel.SendMessageAsync($"{userAccount.Mention}", false, embed.Build());

                    const int delay = 1000;
                    await Task.Delay(delay);
                    await tempSuccess.DeleteAsync();
                }
            }
        }*/

        [Command("ask")]
        public async Task AskStaff([Remainder] string question = null)
        {
            if (question == null)
            {
                await Context.Channel.SendErrorAsync("Please provide a question!");
            }
            var ruser = new List<string>();
            ruser.Add("<@694489223817986090>"); //Tyler
                                                //ruser.Add("<@585148506013171712>"); //Harry
                                                //ruser.Add("<@669791201846624258>"); //Nicky
            ruser.Add("<@412577190954139660>");
            ruser.Add("<@279307543862444033>"); //Will
            ruser.Add("<@619241308912877609>"); //Liege
                                                //ruser.Add("<@556622146181398559>"); //Jon Acon
                                                //ruser.Add("<@408033294857273355>"); //Buthy
            ruser.Add("<@655186837203189771>"); //BoredBot
            ruser.Add("<@526607551014502411>"); //Max
            ruser.Add("<@412577190954139660>"); //Mountain
            ruser.Add("<@562783110278676481>"); //EatThePorg
                                                //ruser.Add("<@713271371639291934>"); //Heather

            var answer = ruser[new Random().Next(ruser.Count)];

            var user = Context.User as SocketGuildUser;

            var embed = new EmbedBuilder();
            embed.WithAuthor(user + "Support");
            embed.WithDescription($"{user.Mention}, a staff member will assist you shortly :D\n\n**Question:** {question}");
            embed.WithColor(Color.Green);
            await ReplyAsync($"{answer}", false, embed.Build());
        }

        [Command("role create")]
        public async Task CreateRole([Remainder] string roleName = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");

            if (!user.Roles.Contains(roleStaff))
            {
                await Context.Channel.SendErrorAsync("You do not have access to use this command!");
                return;
            }

            if (roleName == null)
            {
                await Context.Channel.SendErrorAsync("Please provide a role name!");
                return;
            }

            if (user.Roles.Contains(roleStaff))
            {
                List<Color> colors = new List<Color>();
                colors.Add(new Color(26, 188, 156));
                colors.Add(new Color(155, 89, 182));
                colors.Add(new Color(113, 54, 138));
                colors.Add(new Color(241, 196, 15));
                colors.Add(new Color(194, 124, 14));
                colors.Add(new Color(168, 67, 0));
                colors.Add(new Color(230, 126, 34));
                colors.Add(new Color(231, 76, 60));
                var roleColor = colors[new Random().Next(colors.Count)];

                var embed = new EmbedBuilder();
                embed.WithTitle("Success");
                embed.WithDescription($"Created the `{roleName}` role!");
                embed.WithColor(Color.Green);
                await ReplyAsync("", false, embed.Build());

                await Context.Guild.CreateRoleAsync($"{roleName}", new GuildPermissions(104187969), roleColor, isHoisted: true, isMentionable: true);
            }

        }

        /*[Command("void1")]
        public async Task VoidInfo()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Void Command");
            embed.WithDescription("Use this command to void country's actions. \n\n `!void <user> <message ID> <reason>`");
            embed.WithColor(Color.Orange);

            await ReplyAsync("", false, embed.Build());
        }*/

        [Command("doc1")]
        public async Task DocMessage()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Document Format");
            embed.WithDescription("You can use this [template document](https://docs.google.com/document/d/1b_COImEKNCNdckpwI50DeVScDYqUMnbf8I7c6u3-rHo/edit?usp=sharing) to keep track of your contries production. \nUse the `!doc <ping country role> <link to document>`");
            embed.WithColor(Color.LightOrange);

            await ReplyAsync("", false, embed.Build());
        }



        static string BoolStatuspath = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}Settings.json";

        public static Settings settings = LoadSettings();

        public static bool openCall1 => settings.OpenCall1;
        public static bool openCall2 => settings.OpenCall2;
        public class Settings
        {
            public bool OpenCall1 { get; set; } = false;
            public bool OpenCall2 { get; set; } = false;
        }

        public static void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(BoolStatuspath, json);
        }

        public static Settings LoadSettings()
        {
            if (!File.Exists(BoolStatuspath))
            {
                return default;
            }
            else
            {
                string s = File.ReadAllText(BoolStatuspath);
                return JsonConvert.DeserializeObject<Settings>(s);
            }
        }


        //public bool openCall1 = false;
        //public bool openCall2 = false;

        [Command("call 1")]
        public async Task PhoneCall1(SocketGuildUser userAccount = null)
        { 
            var user = Context.User as SocketGuildUser;
            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
            var phoneChannel1 = Context.Guild.GetTextChannel(626782716255404042);
            var cc = Context.Guild.GetRole(626783654395379753);
            var pc1 = Context.Guild.GetRole(758909495861968907);

            if (!user.Roles.Contains(cc))
            {
                await Context.Channel.SendErrorAsync("You must have a country to use this command!");
                return;
            }

            if (userAccount == null)
            {
                await Context.Channel.SendErrorAsync("Please provide the user that you would like to call!");
                return;
            }

            if (!userAccount.Roles.Contains(cc))
            {
                await Context.Channel.SendErrorAsync($"{userAccount.Mention} must have a country!");
                return;
            }

            if (openCall1 == true)
            {
                await Context.Channel.SendErrorAsync("This phone call channel is already in use!"); ;
                return;
            }

            if (user.Roles.Contains(cc))
            {
                settings.OpenCall1 = true;
                SaveSettings();

                await Context.Channel.SendSuccessAsync($"Dialing {userAccount.Mention} in <#{phoneChannel1.Id}>");

                var embed1 = new EmbedBuilder();
                embed1.WithTitle("New Phone Call");
                embed1.WithDescription($"{user.Mention} called {userAccount.Mention}\nTo end this phone call, BOTH users must use the `!hang up` command.");
                embed1.WithCurrentTimestamp();
                embed1.WithColor(Color.Blue);
                await phoneChannel1.SendMessageAsync($"{user.Mention} {userAccount.Mention}", false, embed1.Build());

                //await phoneChannel1.ModifyAsync(x => x.Name = "phone-call-1_");
                await user.AddRoleAsync(pc1);
                await userAccount.AddRoleAsync(pc1);
            }
        }

        [Command("call 2")]
        public async Task PhoneCall2(SocketGuildUser userAccount = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
            var phoneChannel2 = Context.Guild.GetTextChannel(626782735243018240);
            var cc = Context.Guild.GetRole(626783654395379753);
            var pc2 = Context.Guild.GetRole(778991488189071430);

            if (!user.Roles.Contains(cc))
            {
                await Context.Channel.SendErrorAsync("You must have a country to use this command!");
                return;
            }

            if (userAccount == null)
            {
                await Context.Channel.SendErrorAsync("Please provide the user that you would like to call!");
                return;
            }

            if (!userAccount.Roles.Contains(cc))
            {
                await Context.Channel.SendErrorAsync($"{userAccount.Mention} must have a country!");
                return;
            }

            if (openCall2 == true)
            {
                await Context.Channel.SendErrorAsync("This phone call channel is already in use!"); ;
                return;
            }

            if (user.Roles.Contains(cc))
            {
                settings.OpenCall2 = true;
                SaveSettings();

                await Context.Channel.SendSuccessAsync($"Dialing {userAccount.Mention} in <#{phoneChannel2.Id}>");

                var embed1 = new EmbedBuilder();
                embed1.WithTitle("New Phone Call");
                embed1.WithDescription($"{user.Mention} called {userAccount.Mention}\nTo end this phone call, a user must use the `!hang up <other user>` command.");
                embed1.WithCurrentTimestamp();
                embed1.WithColor(Color.Blue);
                await phoneChannel2.SendMessageAsync($"{user.Mention} {userAccount.Mention}", false, embed1.Build());

                //await phoneChannel1.ModifyAsync(x => x.Name = "phone-call-2_");
                await user.AddRoleAsync(pc2);
                await userAccount.AddRoleAsync(pc2);
            }
        }

        [Command("hang up")]
        public async Task HangUpPhone(SocketGuildUser userAccount = null)
        {
            var user = Context.User as SocketGuildUser;
            var roleStaff = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Staff");
            var phoneChannel1 = Context.Guild.GetTextChannel(626782716255404042);
            var phoneChannel2 = Context.Guild.GetTextChannel(626782735243018240);
            var phoneChannel3 = Context.Guild.GetTextChannel(678643041363558468);
            var cc = Context.Guild.GetRole(626783654395379753);
            var pc1 = Context.Guild.GetRole(758909495861968907);
            var pc2 = Context.Guild.GetRole(778991488189071430);
            var pc3 = Context.Guild.GetRole(779339955243712513);

            if (!user.Roles.Contains(cc))
            {
                await Context.Channel.SendErrorAsync("You must have a country to use this command!");
                return;
            }

            if (userAccount == null)
            {
                await Context.Channel.SendErrorAsync("Please mention the other user!");
                return;
            }

            if (!Context.Channel.Name.Contains("phone"))
            {
                await Context.Channel.SendErrorAsync("This channel is not a phone call channel!");
                return;
            }

            if (user.Roles.Contains(cc))
            {
                if (phoneChannel1 == Context.Channel)
                {
                    await user.RemoveRoleAsync(pc1);
                    await userAccount.RemoveRoleAsync(pc1);
                    settings.OpenCall1 = false;
                    SaveSettings();
                }

                if (phoneChannel2 == Context.Channel)
                {
                    await user.RemoveRoleAsync(pc2);
                    await userAccount.RemoveRoleAsync(pc2);
                    settings.OpenCall2 = false;
                    SaveSettings();
                }

                if (phoneChannel3 == Context.Channel)
                {
                    await user.RemoveRoleAsync(pc3);
                    await userAccount.RemoveRoleAsync(pc3);
                    //openCall3 = false;
                    //await phoneChannel3.ModifyAsync(x => x.Name = "phone-call-3");
                }

                var embed = new EmbedBuilder();
                embed.WithTitle("Success");
                embed.WithDescription("This phone call has ended.");
                embed.WithCurrentTimestamp();
                embed.WithColor(Color.Blue);
                await ReplyAsync("", false, embed.Build());

                var help = new EmbedBuilder();
                help.WithTitle("How to start a new phone call");
                help.AddField("Dial", "`!call <line> <user>`\n`!call 2 @Liege#8888`", true);
                help.AddField("Hang Up", "`!hang up <user>`\n`!hang up @Liege#8888`", true);
                help.WithDescription("This feature is in testing and only works in the phone call 1 and 2 channels. Report and bugs to <@619241308912877609>.");
                help.WithColor(Color.Blue);
                await ReplyAsync("", false, help.Build());


            }
        }

        [Command("help call")]
        public async Task HelpCall()
        {
            var help = new EmbedBuilder();
            help.WithTitle("How to start a new phone call");
            help.AddField("Dial", "`!call <line> <user>`\n`!call 2 @Liege#8888`", true);
            help.AddField("Hang Up", "`!hang up <user>`\n`!hang up @Liege#8888`", true);
            help.WithDescription("This feature is in testing and only works in the phone call 1 and 2 channels. Report and bugs to <@619241308912877609>.");
            help.WithColor(Color.Blue);
            await ReplyAsync("", false, help.Build());
        }

        [Command("roleinfo")]
        public async Task BotInfo(SocketRole role = null)
        {
            if (role == null)
            {
                var error = new EmbedBuilder();
                error.WithTitle("Error");
                error.WithDescription("Please provide a role!");
                error.WithColor(Color.Red);
                await ReplyAsync("", false, error.Build());
                return;
            }

            var embed = new EmbedBuilder();
            embed.WithTitle($"{role} info");
            embed.AddField("Users", $"{role.Members.Count()}", true);
            embed.AddField("Color", role.Color, true);
            embed.AddField("Position", role.Position, true);
            embed.AddField("Permissions Integer", $"`{role.Permissions}`", true);
            embed.AddField("Is Hoisted", role.IsHoisted, true);
            embed.WithColor(role.Color);
            embed.WithFooter($"ID: {role.Id} | Created: {role.CreatedAt}");
            await ReplyAsync("", false, embed.Build());
        }

        [Command("doc")]
        public async Task LogCountryInfoDoc(IRole countryName = null, string text = null)
        {
            var user = Context.User as SocketGuildUser;
            var cc = Context.Guild.GetRole(626783654395379753);

            if (!user.Roles.Contains(cc))
            {
                await Context.Channel.SendErrorAsync("You must have a country to use this command!");
                return;
            }

            if (countryName == null)
            {
                await Context.Channel.SendErrorAsync("Please mention your country's role!");
                return;
            }

            if (text == null)
            {
                await Context.Channel.SendErrorAsync("Please provide a link to your document!");
                return;
            }

            if (!text.Contains("docs.google.com"))
            {
                await Context.Channel.SendErrorAsync("The link you entered is invalid!");
                return;
            }

            if (user.Roles.Contains(cc))
            {
                var channel = Context.Guild.GetTextChannel(725268300053086238);

                var doc = new EmbedBuilder();
                doc.WithTitle("Success!");
                doc.WithTitle($"Your country's info has been added!");
                doc.WithDescription($"See it in <#725268300053086238>!");
                doc.WithFooter($"{user}");
                doc.WithColor(Color.Green);

                await ReplyAsync("", false, doc.Build());

                var log = new EmbedBuilder();
                log.AddField("Country", $"{countryName.Mention}");
                log.AddField("Document", $"[[View Document]]({text})");
                log.WithFooter($"Submitted by: {user}");
                log.WithColor(Color.DarkOrange);
                log.WithCurrentTimestamp();

                await channel.SendMessageAsync("", false, log.Build());
            }
        }
    }
}