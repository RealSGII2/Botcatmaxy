using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotCatMaxy.Models
{
    public struct InfractionsInDays
    {
        public float sum;
        public int count;
    }

    public struct InfractionInfo
    {
        public InfractionsInDays infractionsToday;
        public InfractionsInDays infractions30Days;
        public InfractionsInDays totalInfractions;
        public InfractionsInDays infractions7Days;
        public List<string> infractionStrings;
        public InfractionInfo(List<Infraction> infractions, int amount = 5, bool showLinks = false)
        {
            infractionsToday = new InfractionsInDays();
            infractions30Days = new InfractionsInDays();
            totalInfractions = new InfractionsInDays();
            infractions7Days = new InfractionsInDays();
            infractionStrings = new List<string> { "" };

            infractions.Reverse();
            if (infractions.Count < amount)
            {
                amount = infractions.Count;
            }
            int n = 0;
            for (int i = 0; i < infractions.Count; i++)
            {
                Infraction infraction = infractions[i];

                //Gets how long ago all the infractions were
                TimeSpan dateAgo = DateTime.UtcNow.Subtract(infraction.Time);
                totalInfractions.sum += infraction.Size;
                totalInfractions.count++;
                if (dateAgo.Days <= 7)
                {
                    infractions7Days.sum += infraction.Size;
                    infractions7Days.count++;
                }
                if (dateAgo.Days <= 30)
                {
                    infractions30Days.sum += infraction.Size;
                    infractions30Days.count++;
                    if (dateAgo.Days < 1)
                    {
                        infractionsToday.sum += infraction.Size;
                        infractionsToday.count++;
                    }
                }

                string size = "";
                if (infraction.Size != 1)
                {
                    size = "(" + infraction.Size + "x) ";
                }

                if (n < amount)
                {
                    string jumpLink = "";
                    string timeAgo = dateAgo.LimitedHumanize(2);
                    if (showLinks && !infraction.LogLink.IsNullOrEmpty()) jumpLink = $" [[Logged Here]({infraction.LogLink})]";
                    string infracInfo = $"[{MathF.Abs(i - infractions.Count)}] {size}{infraction.Reason}{jumpLink} - {timeAgo}";
                    n++;

                    //So we don't go over embed character limit of 9000
                    if (infractionStrings.Select(str => str.Length).Sum() + infracInfo.Length >= 5800)
                        return;

                    if ((infractionStrings.LastOrDefault() + infracInfo).Length < 1024)
                    {
                        if (infractionStrings.LastOrDefault()?.Length is not null or 0) infractionStrings[infractionStrings.Count - 1] += "\n";
                        infractionStrings[^1] += infracInfo;
                    }
                    else
                    {
                        infractionStrings.Add(infracInfo);
                    }
                }
            }
        }
    }
}
