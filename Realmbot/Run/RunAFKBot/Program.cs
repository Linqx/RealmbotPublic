using AFKBotProgram;
using BotProgram;

namespace RunAFKBot {
    internal class Program {
        private static void Main(string[] args) {
            ModularProgram.Run(true, typeof(AFKProgram));
        }
    }
}