using System;
using System.Collections.Generic;
using System.Xml.Linq;
using BotTools;

namespace Botcore.Data {
    public class BasicObjectData {

        private static readonly Dictionary<string, BasicObjectData> _classes;

        public virtual string Class => "";
        public ushort Type;
        public string Id;

        static BasicObjectData() {
            _classes = new Dictionary<string, BasicObjectData>();
            foreach (Type type in typeof(BasicObjectData).Assembly.GetTypes())
                if (typeof(BasicObjectData).IsAssignableFrom(type) && type.IsSubclassOf(typeof(BasicObjectData))) {
                    BasicObjectData data = (BasicObjectData) Activator.CreateInstance(type);
                    foreach (string cls in data.Class.Replace(" ", "").Split(','))
                        if (!_classes.ContainsKey(cls))
                            _classes.Add(cls, data);
                }
        }

        public static BasicObjectData Resolve(XElement elem) {
            if (elem.Element("Class") == null) {
                if (!ContentUtils.HasElement(elem, "PetProjectile"))
                    Logger.Log("BasicObjectData", "Tried to parse an object without a class element." +
                                                  $"{(elem.Attribute("type") != null ? $" Type: {elem.Attribute("type").Value}" : "")}" +
                                                  $"{(elem.Attribute("id") != null ? $" Id: {elem.Attribute("id").Value}" : "")}",
                        ConsoleColor.Yellow);
                return new GameObjectData();
            }

            string classType = elem.Element("Class").Value;
            if (_classes.ContainsKey(classType)) {
                if (elem.Element("Static") != null)
                    return new StaticObjectData();

                return (BasicObjectData) Activator.CreateInstance(_classes[classType].GetType());
            }

            if (elem.Element("Static") != null)
                return new StaticObjectData();
            return new GameObjectData();
        }

        public virtual void Parse(XElement elem) {
            Type = ContentUtils.ParseType(ContentUtils.GetAttribute(elem, "type", null));
            Id = ContentUtils.GetAttribute(elem, "id", null);
        }
    }
}