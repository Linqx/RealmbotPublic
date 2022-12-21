using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BotTools {
    public class Logger {
        private static readonly BlockingCollection<LoggingItem> LoggingQueue;
        private static DateTime CurrentTime => DateTime.Now;

        static Logger() {
            LoggingQueue = new BlockingCollection<LoggingItem>();
            Task.Factory.StartNew(Consume, TaskCreationOptions.LongRunning);
        }

        public static void Log(string sender, object value, ConsoleColor color = ConsoleColor.Gray) {
            LoggingQueue.Add(new LoggingItem(sender, value.ToString(), color));
        }

        public static void LogSameLine(string sender, object value, ConsoleColor color = ConsoleColor.Gray) {
            LoggingQueue.Add(new LoggingItem(sender, value.ToString(), color, true));
        }

        public static void SkipLine() {
            LoggingQueue.Add(LoggingItem.Empty);
        }

        private static void Consume() {
            foreach (LoggingItem item in LoggingQueue.GetConsumingEnumerable()) {
                ConsoleColor color = Console.ForegroundColor;

                if (item.Color != color) {
                    Console.ForegroundColor = item.Color;

                    if (item == LoggingItem.Empty) {
                        Console.WriteLine();
                    }
                    else {
                        if (item.SameLine)
                            Console.Write($"\r[{CurrentTime.ToString()}] [{item.Prefix}] {item.Message}");
                        else
                            Console.WriteLine($"[{CurrentTime.ToString()}] [{item.Prefix}] {item.Message}");
                    }

                    Console.ForegroundColor = color;
                }
                else {
                    if (item == LoggingItem.Empty) {
                        Console.WriteLine();
                    }
                    else {
                        if (item.SameLine)
                            Console.Write($"\r[{CurrentTime.ToString()}] [{item.Prefix}] {item.Message}");
                        else
                            Console.WriteLine($"[{CurrentTime.ToString()}] [{item.Prefix}] {item.Message}");
                    }
                }
            }
        }

        private struct LoggingItem {
            public readonly string Prefix;
            public readonly string Message;
            public readonly ConsoleColor Color;
            public readonly bool SameLine;

            public LoggingItem(string prefix, string message, ConsoleColor color = ConsoleColor.Gray,
                bool sameLine = false) {
                Prefix = prefix;
                Message = message;
                Color = color;
                SameLine = sameLine;
            }

            private bool Equals(LoggingItem other) {
                return Prefix == other.Prefix && Message == other.Message && Color == other.Color &&
                       SameLine == other.SameLine;
            }

            public override bool Equals(object obj) {
                return obj is LoggingItem other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    int hashCode = Prefix != null ? Prefix.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ (Message != null ? Message.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (int) Color;
                    hashCode = (hashCode * 397) ^ SameLine.GetHashCode();
                    return hashCode;
                }
            }

            public static bool operator ==(LoggingItem a, LoggingItem b) {
                return a.Equals(b);
            }

            public static bool operator !=(LoggingItem a, LoggingItem b) {
                return !(a == b);
            }

            public static readonly LoggingItem Empty = new LoggingItem(null, null);
        }
    }
}