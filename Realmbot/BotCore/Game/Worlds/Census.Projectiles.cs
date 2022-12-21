using System.Collections.Generic;
using BotCore.Structures;

namespace BotCore.Game.Worlds {
    public partial class Census {
        public int GetNextFakeObjectId => 2130706432 | NextFakeObjectId++;

        private readonly Dictionary<int, int> fake_object_ids;
        private readonly List<HitItem> hit_queue;
        private int NextFakeObjectId;

        public HitItem[] GetHitsAndClear() {
            HitItem[] hits = hit_queue.ToArray();
            hit_queue.Clear();

            return hits;
        }

        public void ObjectHit(byte bulletId, int objectId) {
            hit_queue.Add(new HitItem(bulletId, objectId));
        }

        public int FindObjectId(int ownerId, byte bulletId) {
            return fake_object_ids[(bulletId << 24) | ownerId];
        }

        public int GetNewObjectId(int ownerId, byte bulletId) {
            int objId = GetNextFakeObjectId;
            fake_object_ids[(bulletId << 24) | ownerId] = objId;
            return objId;
        }

        public void RemoveObjectId(int ownerId, byte bulletId) {
            int index = (bulletId << 24) | ownerId;

            if (fake_object_ids.ContainsKey(index))
                fake_object_ids.Remove(index);
        }

        public void ClearBasicObjectIds() {
            fake_object_ids.Clear();
        }
    }
}