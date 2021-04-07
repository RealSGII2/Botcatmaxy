using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotCatMaxy.Models
{
    public record ModerationHistoryItem
    {
        public ModerationHistoryItem(ActRecord tempAct, ulong userID)
        {
            UserID = userID;
            DateInitiated = tempAct.time;
            Reason = tempAct.reason;
            Type = (ModerationHistoryItemType) Enum.Parse(typeof(ModerationHistoryItemType), tempAct.reason);
        }

        public ModerationHistoryItem(ActRecord tempAct, UserRef userRef)
            => new ModerationHistoryItem(tempAct, userRef.ID);

        public ModerationHistoryItem(Infraction infraction, ulong userID)
        {
            UserID = userID;
            DateInitiated = infraction.Time;
            Reason = infraction.Reason;
            Type = ModerationHistoryItemType.infraction;
        }

        public ModerationHistoryItem(Infraction infraction, UserRef userRef)
            => new ModerationHistoryItem(infraction, userRef.ID);

        public ulong UserID { get; init; }
        public DateTime DateInitiated { get; init; }
        public string Reason { get; init; }
        public ModerationHistoryItemType Type { get; init; }
    }

    public enum ModerationHistoryItemType
    {
        tempban,
        tempmute,
        infraction
    }
}
