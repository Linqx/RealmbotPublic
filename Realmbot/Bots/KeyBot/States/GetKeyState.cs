using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotCore;
using BotCore.Game.Worlds.Entities;
using BotCore.Networking;
using BotCore.Structures;
using Rekishi;

namespace KeyBot.States {
	public class GetKeyState : KeyBotState {
		private KeyType TargetKey;
		private WorldPosData TargetPosition;

		public GetKeyState(RealmKeyBot bot, KeyType keyType) : base(bot, KeyBotStates.GetKey) {
			TargetKey = keyType;
		}

		public override void Start() {
			Subcribe(PacketType.NEWTICK, OnNewTick);
		}

		private void OnNewTick(RotMGPacket packet) {
			if (packet.Id < 3)
				return;

			TargetPosition = Bot.KeyToPostion[(ushort) TargetKey];
			Bot.AddMoveAction(TargetPosition, OnArrived);
		}

		private void OnArrived(RealmBot bot) {
			var targetChest = bot.Census.GetGameObjects<Vault>((int) TargetPosition.X, (int) TargetPosition.Y);
			if (targetChest.Length != 1) {
				Log.Error($"Somehow have {targetChest.Length} chests on tile {TargetPosition}");
				return;
			}

			var chest = targetChest[0];

			var keyCount = bot.CountItemInInventory(chest.Inventory.Items, (ushort) TargetKey);
			Log.Debug($"We have {keyCount} keys {TargetKey}");

			switch (keyCount) {
				case 0:
					//chest has not key
					break;
				case 1:
					//dupe it
					//try right here, have to walk back to this chest again to verify
					break;
				case 2:
					//ghall
					//Loot it! Once we have key, then switch to ghall state
					break;
			}
		}

		public override void Run() {
		}

		public override void End() {
		}
	}
}