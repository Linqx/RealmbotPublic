using System;
using System.Threading.Tasks;

namespace BotTools {
    public class Delay {
        public static void Run(int delay, Action callback) {
            Task.Run(async () => {
                await Task.Delay(delay);
                callback.Invoke();
            });
        }

        public static void RunSeconds(double delay, Action callback) {
            Task.Run(async () => {
                await Task.Delay((int) (delay * 1000));
                callback.Invoke();
            });
        }
    }
}