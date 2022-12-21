using System.Collections;
using System.Collections.Generic;
using Botcore.Data;

namespace BotCore.Structures {
    public class Inventory : IEnumerable<ItemData> {

        public ItemData this[int index] {
            get => Items[index];
            set => Items[index] = value;
        }

        public ItemData[] Items;
        public bool IncludesBackpack;

        public Inventory(int size) {
            Items = new ItemData[size];
        }

        public void Upgrade() {
            Resize(20);
            IncludesBackpack = true;
        }

        public void Resize(int size) {
            ItemData[] items = new ItemData[size];

            for (int i = 0; i < Items.Length; i++)
                items[i] = Items[i];

            Items = items;
        }

        #region IEnumerable

        public IEnumerator<ItemData> GetEnumerator() {
            return ((IEnumerable<ItemData>) Items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return Items.GetEnumerator();
        }

        #endregion

    }
}