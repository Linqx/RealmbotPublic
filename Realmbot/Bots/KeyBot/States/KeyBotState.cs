using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotCore.Task;

namespace KeyBot.States
{
    public enum KeyBotStates
    {
        Initialize,
        EnteringGuildHall,
        AcceptingCommands,
        GetKey,
        Duping,
    }

    public class KeyBotState : State<RealmKeyBot, KeyBotStates>
    {
        protected KeyBotState(RealmKeyBot bot, KeyBotStates states) : base(bot, states) {
        }
    }
}
