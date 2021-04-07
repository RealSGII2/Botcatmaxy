using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotCatMaxy.Models
{
    public struct ModerationHistoryInfo
    {
        public int itemsToday;
        public int items7Days;
        public int items30Days;
        public int totalItems;
        public List<string> itemStrings;

        public ModerationHistoryInfo(List<ModerationHistoryItem> moderationHistoryItems, int amount = 5)
        {
            itemsToday = 0;
            items7Days = 0;
            items30Days = 0;
            totalItems = 0;
            itemStrings = new List<string> { "" };

            if (moderationHistoryItems.Count < amount)
            {
                amount = moderationHistoryItems.Count;
            }
            int n = 0;
            for (int i = 0; i < moderationHistoryItems.Count; i++)
            {
                ModerationHistoryItem item = moderationHistoryItems[i];

                TimeSpan dateAgo = DateTime.UtcNow.Subtract(item.DateInitiated);
                totalItems++;

                if (dateAgo.Days < 1)
                    itemsToday++;

                if (dateAgo.Days <= 7)
                    items7Days++;

                if (dateAgo.Days <= 30)
                    items30Days++;

                if (n < amount)
                {
                    // TODO: Add log link
                    string timeAgo = dateAgo.LimitedHumanize(2);

                    string itemInfo = $"[{MathF.Abs(i - moderationHistoryItems.Count)}] ({item.Type}) {item.Reason} - {timeAgo}";
                    n++;

                    //So we don't go over embed character limit of 9000
                    if (itemStrings.Select(str => str.Length).Sum() + itemInfo.Length >= 5800)
                        return;

                    if ((itemStrings.LastOrDefault() + itemInfo).Length < 1024)
                    {
                        if (itemStrings.LastOrDefault()?.Length is not null or 0) itemStrings[itemStrings.Count - 1] += "\n";
                        itemStrings[^1] += itemInfo;
                    }
                    else
                    {
                        itemStrings.Add(itemInfo);
                    }
                }
            }
        }
    }
}
