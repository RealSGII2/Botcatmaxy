using BotCatMaxy;
using BotCatMaxy.Components.Logging;
using BotCatMaxy.Data;
using BotCatMaxy.Models;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace BotCatMaxy.Moderation
{
    public struct WarnResult
    {
        public readonly bool success;
        public readonly string description;
        public readonly int warnsAmount;

        public WarnResult(int warnsAmount)
        {
            success = true;
            description = null;
            this.warnsAmount = warnsAmount;
        }

        public WarnResult(string error)
        {
            success = false;
            description = error;
            warnsAmount = -1; //to denote error
        }
    }

    public static class PunishFunctions
    {
        public static async Task<WarnResult> Warn(this UserRef userRef, float size, string reason, ITextChannel channel, string logLink = null)
        {
            Contract.Requires(userRef != null);
            if (userRef.GuildUser != null)
                return await userRef.GuildUser.Warn(size, reason, channel, logLink);
            else
                return await userRef.ID.Warn(size, reason, channel, userRef.User, logLink);
        }

        public static async Task<WarnResult> Warn(this IGuildUser user, float size, string reason, ITextChannel channel, string logLink = null)
        {

            if (user.CantBeWarned())
            {
                return new WarnResult("This person can't be warned");
            }

            return await user.Id.Warn(size, reason, channel, user, logLink);

        }

        public static async Task<WarnResult> Warn(this ulong userID, float size, string reason, ITextChannel channel, IUser warnee = null, string logLink = null)
        {
            if (size > 999 || size < 0.01)
            {
                return new WarnResult("Why would you need to warn someone with that size?");
            }

            try
            {
                List<Infraction> infractions = userID.AddWarn(size, reason, channel.Guild, logLink);

                //Try to message but will fail if user has DMs blocked
                try
                {
                    if (warnee != null)
                    {
                        LogSettings logSettings = channel.Guild.LoadFromFile<LogSettings>(false);
                        IUser[] users = null;
                        if (logSettings?.pubLogChannel != null && channel.Guild.TryGetChannel(logSettings.pubLogChannel.Value, out IGuildChannel logChannel))
                            users = await (logChannel as IMessageChannel).GetUsersAsync().Flatten().ToArrayAsync();
                        else
                            users = await (channel as IMessageChannel).GetUsersAsync().Flatten().ToArrayAsync();
                        if (!users.Any(xUser => xUser.Id == userID))
                        {
                            warnee.TryNotify($"You have been warned in {channel.Guild.Name} discord for \"{reason}\" in a channel you can't view");
                        }
                    }
                }
                catch { }
                return new WarnResult(infractions.Count);
            }
            catch (Exception e)
            {
                List<Infraction> infractions = userID.LoadInfractions(channel.Guild, true);
                await new LogMessage(LogSeverity.Error, "Warn", $"An exception has happened while warning a user ({userID}) with {infractions.Count} warns in {await channel.Guild.Describe()}", e).Log();
                return new WarnResult(("Something has gone wrong with trying to warn. Try again in a while, if it's still not working email blackcatmaxy@gmail.com or leave an issue on the GitHub" + e.ToString()).Truncate(1500));
            }
        }

        public static List<Infraction> AddWarn(this ulong userID, float size, string reason, IGuild guild, string logLink)
        {
            List<Infraction> infractions = userID.LoadInfractions(guild, true);
            Infraction newInfraction = new Infraction
            {
                Reason = reason,
                Time = DateTime.UtcNow,
                Size = size,
                LogLink = logLink
            };
            infractions.Add(newInfraction);
            userID.SaveInfractions(guild, infractions);
            return infractions;
        }

        public static Embed GetEmbed(this List<Infraction> infractions, UserRef userRef, IGuild guild, int amount = 5, bool showLinks = false)
        {
            InfractionInfo data = new(infractions, amount, showLinks);

            //Builds infraction embed
            var embed = new EmbedBuilder();
            embed.AddField("Today",
                $"{data.infractionsToday.sum} sum**|**{data.infractionsToday.count} count", true);
            embed.AddField("Last 7 days",
                $"{data.infractions7Days.sum} sum**|**{data.infractions7Days.count} count", true);
            embed.AddField("Last 30 days",
                $"{data.infractions30Days.sum} sum**|**{data.infractions30Days.count} count", true);
            embed.AddField("Warning".Pluralize(data.totalInfractions.count) + " (total " + data.totalInfractions.sum + " sum of size & " + infractions.Count + " individual)",
                data.infractionStrings[0]);
            data.infractionStrings.RemoveAt(0);
            foreach (string s in data.infractionStrings)
                embed.AddField("------------------------------------------------------------", s);
            embed.WithAuthor(userRef)
                .WithGuildAsFooter(guild, "ID: " + userRef.ID)
                .WithColor(Color.Blue)
                .WithCurrentTimestamp();

            return embed.Build();
        }

        public static Embed GetEmbed(this List<ModerationHistoryItem> moderationHistoryItems, UserRef userRef, IGuild guild, int amount = 5)
        {
            ModerationHistoryInfo data = new(moderationHistoryItems, amount);

            EmbedBuilder embed = new();

                // Guild/User Info
            embed.WithAuthor(userRef)
                .WithGuildAsFooter(guild, "ID: " + userRef.ID)
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                // Items in Time Frame Info
                .AddField("Today", data.itemsToday)
                .AddField("Last 7 days", data.items7Days)
                .AddField("Last 30 days", data.items30Days)
                // Actual history info
                .AddField($"Moderation History ({data.totalItems} {"Items".Pluralize(data.totalItems)}", data.itemStrings[0]);

            data.itemStrings.RemoveAt(0);

            foreach (string s in data.itemStrings)
                embed.AddField("------------------------------------------------------------", s);

            return embed.Build();
        }

        public static List<ModerationHistoryItem> GetModerationHistory(this UserRef userRef, IGuild guild)
        {
            List<ModerationHistoryItem> moderationHistory = new();

            List<ActRecord> userTempActs = userRef.LoadActRecord(guild);

            foreach (ActRecord userTempAct in userTempActs)
                moderationHistory.Add(new ModerationHistoryItem(userTempAct, userRef));

            foreach (Infraction infraction in userRef.LoadInfractions(guild, false))
                moderationHistory.Add(new ModerationHistoryItem(infraction, userRef));

            moderationHistory.Sort((a, b) => DateTime.Compare(a.DateInitiated, b.DateInitiated));

            return moderationHistory;
        }

        public static async Task TempBan(this UserRef userRef, TimeSpan time, string reason, ICommandContext context, TempActionList actions = null)
        {
            TempAct tempBan = new TempAct(userRef, time, reason);
            if (actions == null) actions = context.Guild.LoadFromFile<TempActionList>(true);
            actions.tempBans.Add(tempBan);
            actions.SaveToFile();
            await context.Guild.AddBanAsync(userRef.ID, reason: reason);
            DiscordLogging.LogTempAct(context.Guild, context.User, userRef, "bann", reason, context.Message.GetJumpUrl(), time);
            if (userRef.User != null)
            {
                try
                {
                    await userRef.User.Notify($"tempbanned for {time.LimitedHumanize()}", reason, context.Guild, context.Message.Author);
                }
                catch (Exception e)
                {
                    if (e is NullReferenceException) await new LogMessage(LogSeverity.Error, "TempAct", "Something went wrong notifying person", e).Log();
                }
            }
            userRef.ID.RecordAct(context.Guild, tempBan, "tempban", context.Message.GetJumpUrl());
        }

        public static async Task TempMute(this UserRef userRef, TimeSpan time, string reason, ICommandContext context, ModerationSettings settings, TempActionList actions = null)
        {
            TempAct tempMute = new TempAct(userRef.ID, time, reason);
            if (actions == null) actions = context.Guild.LoadFromFile<TempActionList>(true);
            actions.tempMutes.Add(tempMute);
            actions.SaveToFile();
            await userRef.GuildUser?.AddRoleAsync(context.Guild.GetRole(settings.mutedRole));
            DiscordLogging.LogTempAct(context.Guild, context.User, userRef, "mut", reason, context.Message.GetJumpUrl(), time);
            if (userRef.User != null)
            {
                try
                {
                    await userRef.User?.Notify($"tempmuted for {time.LimitedHumanize()}", reason, context.Guild, context.Message.Author);
                }
                catch (Exception e)
                {
                    if (e is NullReferenceException) await new LogMessage(LogSeverity.Error, "TempAct", "Something went wrong notifying person", e).Log();
                }
            }
            userRef.ID.RecordAct(context.Guild, tempMute, "tempmute", context.Message.GetJumpUrl());
        }
    }
}
