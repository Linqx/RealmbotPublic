//using BotTools;

//namespace KeyBot.Handlers {
//    public class MapInfoHandler : KeyBotPacketHandler {
//        public override PacketType Handles => PacketType.MAPINFO;

//        public override void Handle(Packet packet, RealmKeyBot bot) {
//            MapInfoPacket mapInfo = (MapInfoPacket) packet;
//            //Logger.Log("Key Map Info Handler", mapInfo.ToString());

//            bot.RandomSync = new RandomSync(mapInfo.Seed);
//            bot.World = new World(mapInfo.Width, mapInfo.Height);
//            bot.World.Map = Map.FromMapInfo(bot.World, mapInfo);
//            bot.ChangeTitle?.Invoke($"Key Bot Program - {bot.ConnectedServerName}, {mapInfo.Name}");
//            bot.World.Bot = bot;
//            bot.Player = null; // Super important when reconnecting

//            bot.GuildHallPortalObjId = -1;
//            bot.GuildHallPortalFound = false;

//            //TODO: Send Create if no Char (Usually is tho)
//            bot.Send(new LoadPacket {
//                CharId = bot.CharId
//            });
//        }
//    }
//}