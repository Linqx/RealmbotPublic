using BotProgram;
using LocalBotProgram;

namespace RunLocalBot {
    internal class Program {
        private static void Main(string[] args) {
            ModularProgram.Run(true, typeof(LocalProgram));
        }
    }
}