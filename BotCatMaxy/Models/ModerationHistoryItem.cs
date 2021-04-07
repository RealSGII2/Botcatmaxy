using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotCatMaxy.Models
{
    public record ModerationHistoryItem
    {
        public ModerationHistoryItem(TempAct tempAct)
        {
            UserID = tempAct.User;
            DateInitiated = tempAct.DateBanned;
            Reason = tempAct.Reason;
        }

        public ModerationHistoryItem(Infraction infraction, ulong userID)
        {
            UserID = userID;
            DateInitiated = infraction.Time;
            Reason = infraction.Reason;
        }

        public ModerationHistoryItem(Infraction infraction, UserRef userRef)
        {
            UserID = userRef.ID;
            DateInitiated = infraction.Time;
            Reason = infraction.Reason;
        }

        public ulong UserID { get; init; }
        public DateTime DateInitiated { get; init; }
        public string Reason { get; init; }
        public ModerationHistoryItemType Type { get; init; }
    }

    public enum ModerationHistoryItemType
    {
        Ban,
        Mute,
        Infraction
    }
}
