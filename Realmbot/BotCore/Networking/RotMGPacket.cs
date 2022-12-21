using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BotCore.Structures;
using BotTools;
using Nekusasu;
using Shio;

namespace BotCore.Networking {
    public enum PacketType : byte {
        FAILURE,
        CREATE_SUCCESS,
        CREATE,
        PLAYERSHOOT,
        MOVE,
        PLAYERTEXT,
        TEXT,
        SERVERPLAYERSHOOT,
        DAMAGE,
        UPDATE,
        UPDATEACK,
        NOTIFICATION,
        NEWTICK,
        INVSWAP,
        USEITEM,
        SHOWEFFECT,
        HELLO,
        GOTO,
        INVDROP,
        INVRESULT,
        RECONNECT,
        PING,
        PONG,
        MAPINFO,
        LOAD,
        PIC,
        SETCONDITION,
        TELEPORT,
        USEPORTAL,
        DEATH,
        BUY,
        BUYRESULT,
        AOE,
        GROUNDDAMAGE,
        PLAYERHIT,
        ENEMYHIT,
        AOEACK,
        SHOOTACK,
        OTHERHIT,
        SQUAREHIT,
        GOTOACK,
        EDITACCOUNTLIST,
        ACCOUNTLIST,
        QUESTOBJID,
        CHOOSENAME,
        NAMERESULT,
        CREATEGUILD,
        GUILDRESULT,
        GUILDREMOVE,
        GUILDINVITE,
        ALLYSHOOT,
        ENEMYSHOOT,
        REQUESTTRADE,
        TRADEREQUESTED,
        TRADESTART,
        CHANGETRADE,
        TRADECHANGED,
        ACCEPTTRADE,
        CANCELTRADE,
        TRADEDONE,
        TRADEACCEPTED,
        CLIENTSTAT,
        CHECKCREDITS,
        ESCAPE,
        FILE,
        INVITEDTOGUILD,
        JOINGUILD,
        CHANGEGUILDRANK,
        PLAYSOUND,
        GLOBAL_NOTIFICATION,
        RESKIN,
        PETUPGRADEREQUEST,
        ACTIVE_PET_UPDATE_REQUEST,
        ACTIVEPETUPDATE,
        NEW_ABILITY,
        PETYARDUPDATE,
        EVOLVE_PET,
        DELETE_PET,
        HATCH_PET,
        ENTER_ARENA,
        IMMINENT_ARENA_WAVE,
        ARENA_DEATH,
        ACCEPT_ARENA_DEATH,
        VERIFY_EMAIL,
        RESKIN_UNLOCK,
        PASSWORD_PROMPT,
        QUEST_FETCH_ASK,
        QUEST_REDEEM,
        QUEST_FETCH_RESPONSE,
        QUEST_REDEEM_RESPONSE,
        PET_CHANGE_FORM_MSG,
        KEY_INFO_REQUEST,
        KEY_INFO_RESPONSE,
        CLAIM_LOGIN_REWARD_MSG,
        LOGIN_REWARD_MSG,
        QUEST_ROOM_MSG,
        PET_CHANGE_SKIN_MSG,
        REALM_HERO_LEFT_MSG,
        CHAT_HELLO_MSG,
        CHAT_TOKEN_MSG,
        CHAT_LOGOUT_MSG
    }

    public class RotMGPacket : Packet {
        public static Dictionary<PacketType, byte> PacketIds;

        public virtual PacketType PacketType => PacketType.FAILURE;
        public override byte Id => PacketIds[PacketType];

        protected override void ReadNetworkOrder(PacketReader r) {
        }

        protected sealed override void Read(BitReader r) {
        }

        protected override void WriteNetworkOrder(PacketWriter w) {
        }

        protected sealed override void Write(BitWriter w) {
        }

        public override string ToString()
        {
            StringBuilder ret = new StringBuilder("{");
            PropertyInfo[] arr = GetType().GetProperties();
            for (int i = 0; i < arr.Length; i++)
            {
                if (i != 0) ret.Append(", ");
                ret.AppendFormat("{0}: {1}", arr[i].Name, arr[i].GetValue(this, null));
            }
            ret.Append("}");
            return ret.ToString();
        }
    }

    #region Incoming

    public class AccountListPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.ACCOUNTLIST;
        public int AccountListId { get; set; }
        public string[] AccountIds { get; set; }
        public int LockAction { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            AccountListId = rdr.ReadInt32();

            AccountIds = new string[rdr.ReadInt16()];
            for (int i = 0; i < AccountIds.Length; i++)
                AccountIds[i] = rdr.ReadUTF();

            LockAction = rdr.ReadInt32();
        }
    }

    public class AllyShootPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.ALLYSHOOT;
        public byte BulletId { get; set; }
        public int OwnerId { get; set; }
        public short ContainerType { get; set; }
        public float Angle { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            BulletId = rdr.ReadByte();
            OwnerId = rdr.ReadInt32();
            ContainerType = rdr.ReadInt16();
            Angle = rdr.ReadSingle();
        }
    }

    public class ClientStatPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.CLIENTSTAT;
        public string Name { get; set; }
        public int Value { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            Name = rdr.ReadUTF();
            Value = rdr.ReadInt32();
        }
    }

    public class CreateSuccessPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.CREATE_SUCCESS;
        public int ObjectId { get; set; }
        public int CharId { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            ObjectId = rdr.ReadInt32();
            CharId = rdr.ReadInt32();
        }
    }

    public class DamagePacket : RotMGPacket {

        public override PacketType PacketType => PacketType.DAMAGE;
        public int TargetId { get; set; }
        public byte[] Effects { get; set; }
        public int DamageAmount { get; set; }
        public bool Kill { get; set; }
        public bool ArmorPierce { get; set; }
        public byte BulletId { get; set; }
        public int ObjectId { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            TargetId = rdr.ReadInt32();

            Effects = new byte[rdr.ReadByte()];
            for (int i = 0; i < Effects.Length; i++)
                Effects[i] = rdr.ReadByte();

            DamageAmount = rdr.ReadUInt16();
            Kill = rdr.ReadBoolean();
            ArmorPierce = rdr.ReadBoolean();
            BulletId = rdr.ReadByte();
            ObjectId = rdr.ReadInt32();
        }
    }

    public class FailurePacket : RotMGPacket {
        public const int IncorrectVersion = 4;
        public const int BadKey = 5;
        public const int InvalidTeleportTarget = 6;
        public const int EmailVerificationNeeded = 7;
        public const int TeleportRealmBlock = 9;

        public override PacketType PacketType => PacketType.FAILURE;

        public int ErrorId { get; set; }
        public string ErrorDescription { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            ErrorId = rdr.ReadInt32();
            ErrorDescription = rdr.ReadUTF();
        }
    }

    public class GlobalNotificationPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.GLOBAL_NOTIFICATION;
        public int NotificationType { get; set; }
        public string Text { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            NotificationType = rdr.ReadInt32();
            Text = rdr.ReadUTF();
        }
    }

    public class GoToPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.GOTO;
        public int ObjectId { get; set; }
        public WorldPosData Pos { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            ObjectId = rdr.ReadInt32();
            Pos = WorldPosData.Parse(rdr);
        }
    }

    public class MapInfoPacket : RotMGPacket {
        public override PacketType PacketType => PacketType.MAPINFO;

        public int Width { get; set; }
        public int Height { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string RealmName { get; set; }
        public uint Seed { get; set; }
        public int Difficulty { get; set; }
        public int Background { get; set; }
        public bool AllowTeleport { get; set; }
        public bool ShowDisplays { get; set; }
        public short MaxPlayers { get; set; }
        public string ConnectionGuid { get; set; }

        public string[] ClientXmls { get; set; }
        public string[] ExtraXmls { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            Width = rdr.ReadInt32();
            Height = rdr.ReadInt32();
            Name = rdr.ReadUTF();
            DisplayName = rdr.ReadUTF();
            RealmName = rdr.ReadUTF();
            Seed = rdr.ReadUInt32();
            Background = rdr.ReadInt32();
            Difficulty = rdr.ReadInt32();
            AllowTeleport = rdr.ReadBoolean();
            ShowDisplays = rdr.ReadBoolean();
            MaxPlayers = rdr.ReadInt16();
            ConnectionGuid = rdr.ReadUTF();

            ClientXmls = new string[rdr.ReadInt16()];
            for (int i = 0; i < ClientXmls.Length; i++)
                ClientXmls[i] = rdr.Read32UTF();

            ExtraXmls = new string[rdr.ReadInt16()];
            for (int i = 0; i < ExtraXmls.Length; i++)
                ExtraXmls[i] = rdr.Read32UTF();
        }
    }

    public class NewTickPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.NEWTICK;
        public int TickId { get; set; }
        public int TickTime { get; set; }
        public ObjectStatusData[] Status { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            TickId = rdr.ReadInt32();
            TickTime = rdr.ReadInt32();

            Status = new ObjectStatusData[rdr.ReadInt16()];
            for (int i = 0; i < Status.Length; i++) {
                ObjectStatusData status = new ObjectStatusData();
                status.Read(rdr);
                Status[i] = status;
            }
        }
    }

    public class NotificationPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.NOTIFICATION;
        public int ObjectId { get; set; }
        public string Message { get; set; }
        public int Color { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            ObjectId = rdr.ReadInt32();
            Message = rdr.ReadUTF();
            Color = rdr.ReadInt32();
        }
    }

    public class PingPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.PING;
        public int Serial { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            Serial = rdr.ReadInt32();
        }
    }

    public class ReconnectPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.RECONNECT;
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public int GameId { get; set; }
        public int KeyTime { get; set; }
        public byte[] Key { get; set; }
        public bool IsFromArena { get; set; }
        public string Stats { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            Name = rdr.ReadUTF();
            Host = rdr.ReadUTF();
            Stats = rdr.ReadUTF();
            Port = rdr.ReadInt32();
            GameId = rdr.ReadInt32();
            KeyTime = rdr.ReadInt32();
            IsFromArena = rdr.ReadBoolean();
            Key = rdr.ReadBytes(rdr.ReadInt16());
        }
    }

    public class ServerPlayerShootPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.SERVERPLAYERSHOOT;
        public byte BulletId { get; set; }
        public int OwnerId { get; set; }
        public int ContainerType { get; set; }
        public WorldPosData StartingPos { get; set; }
        public float Angle { get; set; }
        public short Damage { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            BulletId = rdr.ReadByte();
            OwnerId = rdr.ReadInt32();
            ContainerType = rdr.ReadInt32();
            StartingPos = WorldPosData.Parse(rdr);
            Angle = rdr.ReadSingle();
            Damage = rdr.ReadInt16();
        }
    }

    public class ShowEffectPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.SHOWEFFECT;
        public byte EffectType { get; set; }
        public int TargetObjId { get; set; }
        public WorldPosData Pos1 { get; set; }
        public WorldPosData Pos2 { get; set; }
        public int Color { get; set; }
        public float Duration { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            EffectType = rdr.ReadByte();
            TargetObjId = rdr.ReadInt32();
            Pos1 = WorldPosData.Parse(rdr);
            Pos2 = WorldPosData.Parse(rdr);
            Color = rdr.ReadInt32();
            Duration = rdr.ReadSingle();
        }
    }

    public class TextPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.TEXT;
        public string Name { get; set; }
        public int ObjectId { get; set; }
        public int NumStars { get; set; }
        public int BubbleTime { get; set; }
        public string Recipient { get; set; }
        public string Text { get; set; }
        public string CleanText { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            Name = rdr.ReadUTF();
            ObjectId = rdr.ReadInt32();
            NumStars = rdr.ReadInt32();
            BubbleTime = rdr.ReadByte();
            Recipient = rdr.ReadUTF();
            Text = rdr.ReadUTF();
            CleanText = rdr.ReadUTF();
        }
    }

    public class TradeAcceptedPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.TRADEACCEPTED;
        public bool[] MyOffer { get; set; }
        public bool[] YourOffers { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            MyOffer = new bool[rdr.ReadInt16()];
            for (int i = 0; i < MyOffer.Length; i++) MyOffer[i] = rdr.ReadBoolean();

            YourOffers = new bool[rdr.ReadInt16()];
            for (int i = 0; i < YourOffers.Length; i++) YourOffers[i] = rdr.ReadBoolean();
        }
    }

    public class TradeChangedPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.TRADECHANGED;
        public bool[] Offers { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            Offers = new bool[rdr.ReadInt16()];
            for (int i = 0; i < Offers.Length; i++) Offers[i] = rdr.ReadBoolean();
        }
    }

    public class TradeDonePacket : RotMGPacket {

        public override PacketType PacketType => PacketType.TRADEDONE;
        public int Result { get; set; }
        public string Message { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            Result = rdr.ReadInt32();
            Message = rdr.ReadUTF();
        }
    }

    public class TradeRequestedPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.TRADEREQUESTED;
        public string Name { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            Name = rdr.ReadUTF();
        }
    }

    public class TradeStartPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.TRADESTART;
        public TradeItem[] MyItems { get; set; }
        public string YourName { get; set; }
        public TradeItem[] YourItems { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            MyItems = new TradeItem[rdr.ReadInt16()];
            for (int i = 0; i < MyItems.Length; i++) {
                TradeItem trade = new TradeItem();
                trade.Read(rdr);

                MyItems[i] = trade;
            }

            YourName = rdr.ReadUTF();

            YourItems = new TradeItem[rdr.ReadInt16()];
            for (int i = 0; i < YourItems.Length; i++) {
                TradeItem trade = new TradeItem();
                trade.Read(rdr);

                YourItems[i] = trade;
            }
        }
    }

    public class UpdatePacket : RotMGPacket {

        public override PacketType PacketType => PacketType.UPDATE;
        public GroundTileData[] Tiles { get; set; }
        public ObjectData[] NewObjects { get; set; }
        public int[] Drops { get; set; }

        protected override void ReadNetworkOrder(PacketReader rdr) {
            Tiles = new GroundTileData[rdr.ReadInt16()];
            for (int i = 0; i < Tiles.Length; i++) {
                GroundTileData tile = new GroundTileData();
                tile.Read(rdr);
                Tiles[i] = tile;
            }

            NewObjects = new ObjectData[rdr.ReadInt16()];
            for (int i = 0; i < NewObjects.Length; i++) {
                ObjectData obj = new ObjectData();
                obj.Read(rdr);
                NewObjects[i] = obj;
            }

            Drops = new int[rdr.ReadInt16()];
            for (int i = 0; i < Drops.Length; i++)
                Drops[i] = rdr.ReadInt32();
        }
    }

    #endregion

    #region Outgoing

    public class HelloPacket : RotMGPacket {
        public override PacketType PacketType => PacketType.HELLO;

        public string BuildVersion { get; set; }
        public int GameId { get; set; }
        public string GUID { get; set; }
        public string Password { get; set; }
        public string Secret { get; set; }
        public int KeyTime { get; set; }
        public byte[] Key { get; set; }
        public string MapJson { get; set; }
        public string EntryTag { get; set; }
        public string GameNet { get; set; }
        public string GameUserNetId { get; set; }
        public string PlayPlatform { get; set; }
        public string PlatformToken { get; set; }
        public string UserToken { get; set; }
        public string PreviousConnectionGuid { get; set; }

        protected override void WriteNetworkOrder(PacketWriter w) {
            w.WriteUTF(BuildVersion);
            w.Write(GameId);
            w.WriteUTF(GUID);
            w.Write(Randum.Next(1000000000));
            w.WriteUTF(Password);
            w.Write(Randum.Next(1000000000));
            w.WriteUTF(Secret);
            w.Write(KeyTime);
            w.Write((short) Key.Length);
            w.Write(Key);
            w.Write32UTF(MapJson);
            w.WriteUTF(EntryTag);
            w.WriteUTF(GameNet);
            w.WriteUTF(GameUserNetId);
            w.WriteUTF(PlayPlatform);
            w.WriteUTF(PlatformToken);
            w.WriteUTF(UserToken);
            w.WriteUTF("XTeP7hERdchV5jrBZEYNebAqDPU6tKU6");
            w.WriteUTF(PreviousConnectionGuid);
        }
    }

    public class AcceptTradePacket : RotMGPacket {

        public override PacketType PacketType => PacketType.ACCEPTTRADE;
        public bool[] MyOffer { get; set; }
        public bool[] YourOffer { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write((short) MyOffer.Length);
            foreach (bool offer in MyOffer)
                wtr.Write(offer);

            wtr.Write((short) YourOffer.Length);
            foreach (bool offer in YourOffer)
                wtr.Write(offer);
        }
    }

    public class CancelTradePacket : RotMGPacket {
        public override PacketType PacketType => PacketType.CANCELTRADE;

        protected override void WriteNetworkOrder(PacketWriter wtr) {
        }
    }

    public class ChangeTradePacket : RotMGPacket {

        public override PacketType PacketType => PacketType.CHANGETRADE;
        public bool[] Offer { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write((short) Offer.Length);
            foreach (bool offer in Offer)
                wtr.Write(offer);
        }
    }

    public class CreatePacket : RotMGPacket {

        public override PacketType PacketType => PacketType.CREATE;
        public short ClassType { get; set; }
        public short SkinType { get; set; }
        public bool IsChallenger { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(ClassType);
            wtr.Write(SkinType);
            wtr.Write(IsChallenger);
        }
    }

    public class EnemyHitPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.ENEMYHIT;
        public int Time { get; set; }
        public byte BulletId { get; set; }
        public int TargetId { get; set; }
        public bool Kill { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(Time);
            wtr.Write(BulletId);
            wtr.Write(TargetId);
            wtr.Write(Kill);
        }
    }

    public class GoToAck : RotMGPacket {

        public override PacketType PacketType => PacketType.GOTOACK;
        public int Time { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(Time);
        }
    }

    public class GroundDamagePacket : RotMGPacket {

        public override PacketType PacketType => PacketType.GROUNDDAMAGE;
        public int Time { get; set; }
        public WorldPosData Position { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(Time);
            Position.Write(wtr);
        }
    }

    public class InvDropPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.INVDROP;
        public SlotObjectData SlotObject { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            SlotObject.Write(wtr);
        }
    }

    public class InvSwapPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.INVSWAP;
        public int Time { get; set; }
        public WorldPosData Position { get; set; }
        public SlotObjectData SlotObject1 { get; set; }
        public SlotObjectData SlotObject2 { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(Time);
            Position.Write(wtr);
            SlotObject1.Write(wtr);
            SlotObject2.Write(wtr);
        }
    }

    public class LoadPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.LOAD;
        public int CharId { get; set; }
        public bool FromArena { get; set; }
        public bool IsChallenger { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(CharId);
            wtr.Write(FromArena);
            wtr.Write(IsChallenger);
        }
    }

    public class MovePacket : RotMGPacket {

        public override PacketType PacketType => PacketType.MOVE;
        public int TickId { get; set; }
        public int Time { get; set; }
        public WorldPosData NewPosition { get; set; }
        public MoveRecord[] Records { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(TickId);
            wtr.Write(Time);
            NewPosition.Write(wtr);
            wtr.Write((short) Records.Length);
            foreach (MoveRecord record in Records)
                record.Write(wtr);
        }
    }

    public class OtherHitPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.OTHERHIT;
        public int Time { get; set; }
        public byte BulletId { get; set; }
        public int ObjectId { get; set; }
        public int TargetId { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(Time);
            wtr.Write(BulletId);
            wtr.Write(ObjectId);
            wtr.Write(TargetId);
        }
    }

    public class PlayerHitPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.PLAYERHIT;
        public byte BulletId { get; set; }
        public int ObjectId { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(BulletId);
            wtr.Write(ObjectId);
        }
    }

    public class PlayerShootPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.PLAYERSHOOT;
        public int Time { get; set; }
        public byte BulletId { get; set; }
        public int ContainerType { get; set; }
        public WorldPosData Position { get; set; }
        public double Angle { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(Time);
            wtr.Write(BulletId);
            wtr.Write((short) ContainerType);
            Position.Write(wtr);
            wtr.Write((float) Angle);
        }
    }

    public class PlayerTextPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.PLAYERTEXT;
        public string Text { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.WriteUTF(Text);
        }
    }

    public class PongPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.PONG;
        public int Serial { get; set; }
        public int Time { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(Serial);
            wtr.Write(Time);
        }
    }

    public class RequestTradePacket : RotMGPacket {

        public override PacketType PacketType => PacketType.REQUESTTRADE;
        public string Name { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.WriteUTF(Name);
        }
    }

    public class ShootAckPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.SHOOTACK;
        public int Time { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(Time);
        }
    }

    public class SquareHit : RotMGPacket {

        public override PacketType PacketType => PacketType.SQUAREHIT;
        public int Time { get; set; }
        public byte BulletId { get; set; }
        public int ObjectId { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(Time);
            wtr.Write(BulletId);
            wtr.Write(ObjectId);
        }
    }

    public class TeleportPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.TELEPORT;
        public int ObjectId { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(ObjectId);
        }
    }

    public class UpdateAckPacket : RotMGPacket {
        public override PacketType PacketType => PacketType.UPDATEACK;
    }

    public class UseItemPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.USEITEM;
        public int Time { get; set; }
        public SlotObjectData SlotObjectData { get; set; }
        public WorldPosData ItemUsePos { get; set; }
        public int UseType { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(Time);
            SlotObjectData.Write(wtr);
            ItemUsePos.Write(wtr);
            wtr.Write(UseType);
        }
    }

    public class UsePortalPacket : RotMGPacket {

        public override PacketType PacketType => PacketType.USEPORTAL;
        public int ObjectId { get; set; }

        protected override void WriteNetworkOrder(PacketWriter wtr) {
            wtr.Write(ObjectId);
        }
    }

    #endregion

}