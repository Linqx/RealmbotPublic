using System;
using System.Diagnostics;
using System.Threading;

namespace BotTools {
    public class UpdateTicker {
        public int TPS { get; set; } = 5;
        public long Tick { get; private set; }
        public long CurrentTime { get; private set; }
        public long LastUpdateTime { get; private set; }
        public bool Running { get; private set; } = true;

        private static Action<int, int> TickCallback { get; set; }

        public UpdateTicker(int tps, Action<int, int> tickCallback) {
            TPS = tps;
            TickCallback = tickCallback;
        }

        public void Start() {
            Thread worker = new Thread(Run);
            worker.Start();
        }

        public void Stop() {
            Running = false;
        }

        private void Run() {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            long targetMs = 1000 / TPS;
            long lastMs = -targetMs;
            long delay = 0;
            long tickTime = 0;

            while (Running) {
                Tick++;
                long currentMs;
                CurrentTime = currentMs = stopwatch.ElapsedMilliseconds;
                int step = (int) (currentMs - lastMs);

                TickCallback?.Invoke((int) CurrentTime, step);

                LastUpdateTime = CurrentTime;

                lastMs = currentMs;
                tickTime = stopwatch.ElapsedMilliseconds - currentMs;
                delay = targetMs - tickTime;

                if (delay > 0)
                    TaskHelper.Delay((int) delay).Wait();
                else
                    Logger.Log("Update Ticker", "lagged! " + Math.Abs(delay) + " ms", ConsoleColor.Yellow);
            }
        }
    }
}