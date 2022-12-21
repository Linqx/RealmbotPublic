using System.Collections.Generic;
using Botcore.Content;
using BotCore.Content.Data;
using BotCore.Structures;
using RotMGCore.Structures.Game;

namespace BotCore.Game.Worlds.Entities {
    public class Container : GameObject {
        public Inventory Inventory;
        public new ContainerData Data;

        public Container(ContainerData data) : base(data) {
            Data = data;
            Inventory = new Inventory(data.SlotTypes.Length);
        }

        public override void ApplyStatusData(ObjectStatusData data, int serverDt, int serverTickId) {
            base.ApplyStatusData(data, serverDt, serverTickId);

            int value = 0;
            string strValue = null;

            foreach (KeyValuePair<StatDataType, object> pair in data.Stats) {
                if (StatDataUtils.IsUTF(pair.Key))
                    strValue = (string) pair.Value;
                else
                    value = (int) pair.Value;

                switch (pair.Key) {
                    case StatDataType.Inventory0:
                        Inventory[0] = GameContent.GetItemData((ushort) value);
                        break;
                    case StatDataType.Inventory1:
                        Inventory[1] = GameContent.GetItemData((ushort) value);
                        break;
                    case StatDataType.Inventory2:
                        Inventory[2] = GameContent.GetItemData((ushort) value);
                        break;
                    case StatDataType.Inventory3:
                        Inventory[3] = GameContent.GetItemData((ushort) value);
                        break;
                    case StatDataType.Inventory4:
                        Inventory[4] = GameContent.GetItemData((ushort) value);
                        break;
                    case StatDataType.Inventory5:
                        Inventory[5] = GameContent.GetItemData((ushort) value);
                        break;
                    case StatDataType.Inventory6:
                        Inventory[6] = GameContent.GetItemData((ushort) value);
                        break;
                    case StatDataType.Inventory7:
                        Inventory[7] = GameContent.GetItemData((ushort) value);
                        break;
                }

                value = 0;
                strValue = null;
            }
        }
    }
}