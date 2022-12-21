using System;
using Botcore.Content;
using Botcore.Data;
using BotCore.Game.Worlds;
using BotCore.Game.Worlds.Entities;
using BotCore.Networking;
using BotCore.Structures;
using BotTools;
using Rekishi;

namespace BotCore {
    public partial class RealmBot {

        public virtual void MapHandlers(RotMGClient client) {
            client.Map(PacketType.FAILURE, OnFailure);
            client.Map(PacketType.ACCOUNTLIST, OnAccountList);
            client.Map(PacketType.ALLYSHOOT, AllyShootHandler);
            client.Map(PacketType.CLIENTSTAT, OnClientStat);
            client.Map(PacketType.CREATE_SUCCESS, OnCreateSuccess);
            client.Map(PacketType.DAMAGE, OnDamage);
            client.Map(PacketType.GLOBAL_NOTIFICATION, OnGlobalNotification);
            client.Map(PacketType.GOTO, OnGoto);
            client.Map(PacketType.MAPINFO, OnMapInfo);
            client.Map(PacketType.NEWTICK, OnNewTick);
            client.Map(PacketType.NOTIFICATION, OnNotification);
            client.Map(PacketType.PING, OnPing);
            client.Map(PacketType.RECONNECT, OnReconnect);
            client.Map(PacketType.SERVERPLAYERSHOOT, OnServerPlayerShoot);
            client.Map(PacketType.SHOWEFFECT, OnShowEffect);
            client.Map(PacketType.TEXT, OnText);
            client.Map(PacketType.UPDATE, OnUpdate);
        }

        public virtual void UnmapHandlers() {
            Client.Unmap(PacketType.FAILURE);
            Client.Unmap(PacketType.ACCOUNTLIST);
            Client.Unmap(PacketType.ALLYSHOOT);
            Client.Unmap(PacketType.CREATE_SUCCESS);
            Client.Unmap(PacketType.DAMAGE);
            Client.Unmap(PacketType.GLOBAL_NOTIFICATION);
            Client.Unmap(PacketType.GOTO);
            Client.Unmap(PacketType.MAPINFO);
            Client.Unmap(PacketType.NEWTICK);
            Client.Unmap(PacketType.NOTIFICATION);
            Client.Unmap(PacketType.PING);
            Client.Unmap(PacketType.RECONNECT);
            Client.Unmap(PacketType.SERVERPLAYERSHOOT);
            Client.Unmap(PacketType.SHOWEFFECT);
            Client.Unmap(PacketType.TEXT);
            Client.Unmap(PacketType.UPDATE);
        }

        private void OnFailure(RotMGPacket packet) {
            FailurePacket failure = (FailurePacket) packet;
            Log.Write($"[Failure] Id: {failure.ErrorId}, Message: {failure.ErrorDescription}", ConsoleColor.Red);
            Delay.RunSeconds(5.0, Reconnect);
        }

        private void OnAccountList(RotMGPacket packet) {
            AccountListPacket accountList = (AccountListPacket) packet;
            // Do nothing? No use for it atm.
        }

        private void AllyShootHandler(RotMGPacket packet) {
            AllyShootPacket allyShoot = (AllyShootPacket) packet;
            // Do nothing? No use for it atm.
        }

        private void OnClientStat(RotMGPacket packet) {
            ClientStatPacket clientStat = (ClientStatPacket) packet;
            Log.Write($"[Client Stat] Name: {clientStat.Name} Value: {clientStat.Value}");
        }

        private void OnCreateSuccess(RotMGPacket packet) {
            CreateSuccessPacket createSuccess = (CreateSuccessPacket) packet;
            ObjectId = createSuccess.ObjectId;
        }

        private void OnDamage(RotMGPacket packet) {
            DamagePacket damage = (DamagePacket) packet;
            // Do nothing? No use for it atm.
        }

        private void OnGlobalNotification(RotMGPacket packet) {
            GlobalNotificationPacket globalNotification = (GlobalNotificationPacket) packet;
            // Do nothing? No use for it atm.
        }

        private void OnGoto(RotMGPacket packet) {
            GoToPacket goTo = (GoToPacket) packet;

            GoToAck ack = new GoToAck {
                Time = World.LastUpdate
            };
            Send(ack);

            GameObject go = World.Census.GetGameObject<GameObject>(goTo.ObjectId);
            go?.OnGoTo(goTo.Pos, World.LastUpdate);
        }

        private void OnMapInfo(RotMGPacket packet) {
            MapInfoPacket mapInfo = (MapInfoPacket) packet;
            RandomSync = new RandomSync(mapInfo.Seed);
            ConnectionGuid = mapInfo.ConnectionGuid;
            World = new World(mapInfo.Width, mapInfo.Height);
            World.Map = Map.FromMapInfo(World, mapInfo);
            World.Bot = this;
            Player = null; // Super important when reconnecting

            //TODO: Send Create if no Char (Usually is tho)
            LoadPacket load = new LoadPacket {
                CharId = CharId
            };
            Send(load);
        }

        private void OnNewTick(RotMGPacket packet) {
            NewTickPacket newTick = (NewTickPacket) packet;
            World.OnNewTick(newTick);
            #region Moving

            bool moveFinished = false;
            if (CurrentMoveAction != null)
            {
                WorldPosData newPos = GetNextLocation(out moveFinished);

                if (Math.Abs(newPos.X - Player.X) > 0.001)
                    Player.X = newPos.X;

                if (Math.Abs(newPos.Y - Player.Y) > 0.001)
                    Player.Y = newPos.Y;
            }

            MovePacket move = new MovePacket
            {
                TickId = newTick.TickId,
                Time = World.LastMoveTime = World.LastUpdate,
                NewPosition = new WorldPosData { X = Player.X, Y = Player.Y },
                Records = new MoveRecord[0]
            };

            Send(move);

            // Don't assume position until server knows the position.
            if (moveFinished)
                CurrentMoveAction?.OnActionFinished(this);

            ushort? tileType = Map.GetTile((int)Player.X, (int)Player.Y);
            if (!tileType.HasValue) {
                Player.MoveMultiplier = 1d;
                return;
            }

            TileData tile = GameContent.GetTileData(tileType.Value);
            if (tile == null)
            {
                Player.MoveMultiplier = 1d;
                Log.Warn($"[New Tick] Unkown tile! Type: {tileType.Value}");
                return;
            }

            Player.MoveMultiplier = tile.NoWalk ? 0d : tile.Speed;

            #endregion
        }

        private void OnNotification(RotMGPacket packet) {
            NotificationPacket notification = (NotificationPacket) packet;
            // Do nothing? No use for it atm.
        }

        private void OnPing(RotMGPacket packet) {
            PingPacket ping = (PingPacket) packet;

            PongPacket pong = new PongPacket
            {
                Serial = ping.Serial,
                Time = World.LastUpdate
            };

            Send(pong);
        }

        private void OnReconnect(RotMGPacket packet) {
            ReconnectPacket reconnect = (ReconnectPacket) packet;
            Reconnect(reconnect.Host, reconnect.Port, reconnect.GameId, reconnect.KeyTime, reconnect.Key);
        }

        private void OnServerPlayerShoot(RotMGPacket packet) {
            ServerPlayerShootPacket serverPlayerShoot = (ServerPlayerShootPacket)packet;
            bool ownShot = serverPlayerShoot.OwnerId == ObjectId;
            GameObject go = Census.GetGameObject(serverPlayerShoot.OwnerId);

            if (go == null || go.Dead)
            {
                if (ownShot)
                    ShootAck(-1);
                return;
            }

            /* Can ignore projectile if not owned by bot. Waste of physics calculation if not owned. */
            if (!ownShot) return;

            /*
             * Grab container data from GameContent.
             * Grab Properties of projectile and generate a projectile.
             * Add projectile to map and send acknowledgement for shoot packet.
             * If unable to update projectile, remove from map.
             */

            ItemData item = GameContent.GetItemData((ushort)serverPlayerShoot.ContainerType);
            ProjectileProperties props = item.Projectile;
            Projectile proj = GenerateProjectile(serverPlayerShoot, props);
            proj.Damage = serverPlayerShoot.Damage;
            Census.AddObject(proj, serverPlayerShoot.StartingPos.X, serverPlayerShoot.StartingPos.Y);
            ShootAck(World.LastUpdate);

            /*
             * Updating the projectile here should technically never fail.
             * This is because packets are never handled when the Map is updating.
             */

            if (!proj.Update(proj.StartTime, 0))
                Census.RemoveObject(proj.ObjectId);
        }

        private void OnShowEffect(RotMGPacket packet) {
            ShowEffectPacket showEffect = (ShowEffectPacket) packet;
            // Do nothing? No use for it atm.
        }

        private void OnText(RotMGPacket packet) {
            TextPacket text = (TextPacket) packet;
            // Do nothing? No use for it atm.
        }

        private void OnUpdate(RotMGPacket packet) {
            UpdatePacket update = (UpdatePacket) packet;
            World.OnUpdate(update);

            UpdateAckPacket ack = new UpdateAckPacket();
            Send(ack);
        }
    }
}