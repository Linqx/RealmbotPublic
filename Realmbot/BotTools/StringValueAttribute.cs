using System;
using System.Collections;
using System.Reflection;

namespace BotTools {
    public class StringValue : Attribute {
        public string Value { get; }

        public StringValue(string value) {
            Value = value;
        }
    }

    public class EnumHelper {
        private static readonly Hashtable _stringValues = new Hashtable();

        public static string GetStringValue(Enum value) {
            string output = null;
            Type type = value.GetType();

            //Check first in our cached results...
            if (_stringValues.ContainsKey(value)) {
                output = (_stringValues[value] as StringValue).Value;
            }
            else {
                //Look for our 'StringValueAttribute' 
                //in the field's custom attributes
                FieldInfo fi = type.GetField(value.ToString());
                StringValue[] attrs =
                    fi.GetCustomAttributes(typeof(StringValue),
                        false) as StringValue[];
                if (attrs.Length > 0) {
                    _stringValues.Add(value, attrs[0]);
                    output = attrs[0].Value;
                }
            }

            return output;
        }
    }
}