using System;
using System.Collections.Generic;

namespace Bookkeeping {
    public class LogWrite {
        public string Text;
        public ConsoleColor Color;

        public LogWrite(string text, ConsoleColor color) {
            Text = text;
            Color = color;
        }
    }

    public class LogEntry {


        public LinkedList<LogWrite> writes = new LinkedList<LogWrite>();

        public static LogEntry Init(object obj) {
            return Init(obj.ToString(), ConsoleColor.White);
        }

        public static LogEntry Init(object obj, ConsoleColor color) {
            return Init(new LogWrite(obj.ToString(), color));
        }

        public static LogEntry Init(string text) {
            return Init(text, ConsoleColor.White);
        }

        public static LogEntry Init(string text, ConsoleColor color) {
            return Init(new LogWrite(text, color));
        }

        public static LogEntry Init(LogWrite write) {
            return new LogEntry().Append(write);
        }

        public LogEntry Append(string text) {
            return Append(text, ConsoleColor.White);
        }

        public LogEntry Append(string text, ConsoleColor color) {
            return Append(new LogWrite(text, color));
        }

        public LogEntry Append(LogWrite write) {
            writes.AddLast(write);
            return this;
        }
    }
}