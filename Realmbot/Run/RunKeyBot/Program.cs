using BotProgram;
using KeyBotProgram;

namespace RunKeyBot {
    internal class Program {
        private static void Main(string[] args) {
            ModularProgram.Run(true, typeof(KeyProgram));
        }
    }
}