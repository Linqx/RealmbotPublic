using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Botcore.Data;
using BotCore.Game.Worlds.Entities;
using BotCore.Structures;

namespace BotCore {
	public partial class RealmBot {
		/// <summary>
		/// Drops all items in player's inventory.
		/// </summary>
		public void DropAllItems() {
		}

		/// <summary>
		/// Drops a item from the player's inventory.
		/// </summary>
		/// <param name="slot">Slot Id from the player's inventory.</param>
		public void DropItem(int slot) {
		}

		/// <summary>
		/// Swaps an inventory slot with another inventory.
		/// </summary>
		/// <param name="slot1">Slot from player's inventory.</param>
		/// <param name="slot2">Slot from other's inventory.</param>
		/// <param name="otherInventory">The inventory you are swapping slots with.</param>
		public void InvSwap(int slot1, int slot2, int otherObjId, Inventory otherInventory) {
		}

		/// <summary>
		/// Find an item within an inventory, returns -1 if not found
		/// </summary>
		/// <param name="items">Inventory array</param>
		/// <param name="type">Numerical id</param>
		/// <param name="inverse">Search for the item, or not the item (useful when type is -1)</param>
		/// <param name="startPosition">First Slot</param>
		/// <param name="endPosition">Last Slot</param>
		/// <returns>Slot Id</returns>
		public int FindItem(int[] items, int type, bool inverse = false, int startPosition = 0, int endPosition = -1) {
			if (endPosition == -1)
				endPosition = 8;

			if (startPosition > items.Length || endPosition > items.Length)
				return -1;

			if (!inverse) {
				for (int slot = startPosition; slot < endPosition; slot++) {
					if (items[slot] == type) {
						return slot;
					}
				}
			} else {
				for (int slot = startPosition; slot < endPosition; slot++) {
					if (items[slot] != type) {
						return slot;
					}
				}
			}

			//not found
			return -1;
		}

		public int CountFreeSlots(Inventory items) {
			int start = 0;
			switch (items.Items.Length) {
				case 8:
					start = 0;
					break;
				case 20:
					start = 4;
					break;
			}

			int count = 0;
			for (int slot = start; slot < items.Items.Length; slot++) {
				if (items.Items[slot] == null) {
					count++;
				}
			}

			return count;
		}

		public int CountItemInInventory(ItemData[] items, int type) {
			return items.Count(item => item.Type == type);
		}
	}
}