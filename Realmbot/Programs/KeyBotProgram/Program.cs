using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotProgram;

namespace KeyBotProgram
{
    internal class Program {
        private static void Main(string[] args) {
            ModularProgram.Run(true, typeof(KeyProgram));
        }
    }
}
