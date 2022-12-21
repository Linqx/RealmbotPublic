using BotProgram;
using MultiBotProgram;

namespace RunMultiBot {
    internal class Program {
        private static void Main() {
            ModularProgram.Run(true, typeof(MultiProgram));
        }
    }
}