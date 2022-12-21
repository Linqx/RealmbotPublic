namespace BotCore.Structures {
    public struct HitItem {
        public byte BulletId;
        public int ObjectId;

        public HitItem(byte bulletId, int objectId) {
            BulletId = bulletId;
            ObjectId = objectId;
        }
    }
}