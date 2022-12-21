using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace BotTools {
    public class ContentUtils {
        public static string[] SplitInParts(string s, int length, char charBeforeSplit, string join) {
            List<string> strings = new List<string>();
            int previousCharIndex = 0;
            int charIndex = 0;

            for (int i = 0; i < s.Length; i++) {
                if (i == 0)
                    continue;

                char c = s[i];
                if (c == charBeforeSplit) {
                    if (previousCharIndex != charIndex && charIndex != 0)
                        previousCharIndex = charIndex;

                    charIndex = i;
                }

                if (i % (length - join.Length) == 0) {
                    int removeAt = Math.Min(previousCharIndex, i);
                    strings.Add($"{join}{s.Substring(0, removeAt - join.Length)}");
                    s = s.Substring(removeAt - join.Length);
                    i = 0;
                }
                else if (i == s.Length - 1) {
                    strings.Add($"{join}{s}");
                }
            }

            return strings.ToArray();
        }

        public static string GetAttribute(XElement element, string name, string defaultValue) {
            return element.Attribute(name)?.Value ?? defaultValue;
        }

        public static string GetElement(XElement element, string name, string defaultValue) {
            return element.Element(name)?.Value ?? defaultValue;
        }

        public static bool HasElement(XElement element, string name) {
            return element.Element(name) != null;
        }

        public static bool HasAttribute(XElement element, string name) {
            return element.Attribute(name) != null;
        }

        public static ushort ParseType(string value) {
            value = value.Trim();
            if (value.StartsWith("0x")) return ushort.Parse(value.Substring(2), NumberStyles.HexNumber);
            if (ushort.TryParse(value, NumberStyles.HexNumber, null, out ushort parsedValue))
                return parsedValue;
            return ushort.Parse(value);
        }

        public static int ParseIntHex(string value) {
            value = value.Trim();
            if (value.StartsWith("0x")) return int.Parse(value.Substring(2), NumberStyles.HexNumber);
            if (int.TryParse(value, NumberStyles.HexNumber, null, out int parsedValue))
                return parsedValue;
            return int.Parse(value);
        }

        public static uint ParseUIntHex(string value) {
            value = value.Trim();
            if (value.StartsWith("0x")) return uint.Parse(value.Substring(2), NumberStyles.HexNumber);
            if (uint.TryParse(value, NumberStyles.HexNumber, null, out uint parsedValue))
                return parsedValue;
            return uint.Parse(value);
        }

        public static int[] ParseIntArray(string value) {
            string[] values = value.Split(',');
            values = values.Select(_ => _.Trim()).ToArray();
            return values.Select(_ => ParseIntHex(_)).ToArray();
        }
    }

    //Edited version of https://stackoverflow.com/a/30527234

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ParseableNameAttribute : Attribute {
        public readonly string[] Names;

        public ParseableNameAttribute(string name) {
            if (name == null)
                throw new ArgumentNullException();

            Names = new[] {name};
        }

        public ParseableNameAttribute(params string[] names) {
            if (names == null || names.Any(x => x == null))
                throw new ArgumentNullException();

            Names = names;
        }
    }

    public static class ParseEnum {
        public static TEnum Parse<TEnum>(string value) where TEnum : struct {
            if (Enum.TryParse(value, out TEnum result))
                return result;

            return ParseEnumImpl<TEnum>.Values[value];
        }

        public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct {
            if (Enum.TryParse(value, true, out result))
                return true;

            return ParseEnumImpl<TEnum>.Values.TryGetValue(value, out result);
        }

        private static class ParseEnumImpl<TEnum> where TEnum : struct {
            public static readonly Dictionary<string, TEnum> Values = new Dictionary<string, TEnum>();

            static ParseEnumImpl() {
                var nameAttributes = typeof(TEnum)
                    .GetFields()
                    .Select(x => new {
                        Value = x,
                        Names = x.GetCustomAttributes(typeof(ParseableNameAttribute), false)
                            .Cast<ParseableNameAttribute>()
                    });

                var degrouped = nameAttributes.SelectMany(
                    x => x.Names.SelectMany(y => y.Names),
                    (x, y) => new {x.Value, Name = y});

                Values = degrouped.ToDictionary(
                    x => x.Name,
                    x => (TEnum) x.Value.GetValue(null));
            }
        }
    }
}