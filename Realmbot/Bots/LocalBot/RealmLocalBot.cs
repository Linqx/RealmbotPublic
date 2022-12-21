using BotTools;

namespace LocalBot {
    public class RealmLocalBot : RealmBot {
        public LocalBotHandlers Handlers;

        public RealmLocalBot(string email, string password) : base(email, password) {
            MapInfoPacket.ReadMusic = true;
            IncomingRC4 = "BE30D622A3D80F3AD6ACF3D8C7";
            OutgoingRC4 = "1895A77D5FD41A696F4328089F";
            RSA = "MIGeMA0GCSqGSIb3DQEBAQUAA4GMADCBiAKBgHMe3ogI6rrJ6ixmPm69VDvzCFhn" +
                  "2qRlSVy/Am1KMTrD8AWb7SOeVxelaaJNApQfu9QDRLIp+nzMFbMjfFHRgd0iHOI0" +
                  "gKm76ElskAdMZXpkcktPb4kiNeuDeZQS/caP0KGO0abgulis/JcnJRrZWDjkF/6J" +
                  "rpzdo3OsKPTVeF37AgMBAAE=\n";

            Handlers = new LocalBotHandlers();
            HandlePacket = HandleAfterUpdate;
        }

        public void Wander(RealmBot bot) {
            if (Player == null)
                return;

            var pos = Player.Position;
            pos.X += Randum.Next(2) > 0 ? 1 : -1;
            pos.Y += Randum.Next(2) > 0 ? 1 : -1;

            AddMoveAction(pos, Wander);
        }

        public override void HandleAfterUpdate(Packet packet) {
            Handlers.HandlePacketPostFrame(packet, this);
        }

        public override void Handle(Packet packet) {
            Handlers.HandlePacketPostFrame(packet, this);
        }
    }
}