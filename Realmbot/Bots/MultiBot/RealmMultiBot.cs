using System;
using MultiBot.Handlers;

namespace MultiBot {
    public partial class RealmMultiBot : RealmBot {
        public MultiBotHandlers Handlers;
        public bool Following;
        public WorldPosData FollowPos = WorldPosData.Zero;
        public WorldPosData FollowVec;
        public bool FollowLanded;

        //TEMP
        public bool GuildHallPortalFound = false;
        public int GuildHallPortalObjId = -1;
        public Aimbot Aimbot;

        public RealmMultiBot(string email, string password) : base(email, password) {
            Name = "Multi Bot";
            Handlers = new MultiBotHandlers();
            HandlePacket = HandleAfterUpdate;
        }

        public override void GameObjectAdded(GameObject go) {
            switch (go) {
                case Player player:
                    if (player.ObjectId == ObjectId) {
                        player.UpdatePlayer = OnUpdatePlayer;
                        Aimbot = new Aimbot(World.Census);
                    }

                    break;
            }
        }

        public override void BasicObjectAdded(BasicObject bo) {
            switch (bo) {
                case Projectile projectile:
                    projectile.OnUpdateProjectile = OnUpdateProjectile;
                    break;
            }
        }

        public bool OnUpdatePlayer(int time, int dt, Player player) {
            Following = false;
            //TEST
            //
            Player linqx = World.Census.SearchForPlayer("Linqx");

            if (linqx != null)
                FollowPos = linqx.Position;
            else
                FollowPos = WorldPosData.Zero;

            if (FollowPos != WorldPosData.Zero) {
                if (FollowLanded) {
                    FollowVec = WorldPosData.Zero;
                    FollowLanded = false;
                }
                else {
                    Following = true;
                    Follow(player.Position);
                }
            }

            if (!Following) FollowVec = WorldPosData.Zero;

            if (Following && FollowPos != WorldPosData.Zero && FollowVec != WorldPosData.Zero) {
                double moveSpeed = player.GetMoveSpeed();
                double moveVecAngle = Math.Atan2(FollowVec.Y, FollowVec.X);
                player.MoveVector =
                    new WorldPosData(moveSpeed * Math.Cos(moveVecAngle), moveSpeed * Math.Sin(moveVecAngle));

                WalkToFollow(player.Position + dt * player.MoveVector, player);
            }

            if (linqx != null)
                if (Player?.Inventory[0] != null) {
                    Pet linqxPet = World.Census.GetGameObject<Pet>(linqx.ObjectId + 1);

                    if (linqxPet != null) {
                        var aimbotPos = CalcAimbotAngle(Player.Inventory[0].Type, linqxPet.Position);
                        AttemptShoot(Math.Atan2(aimbotPos.Y - Player.Y, aimbotPos.X - Player.X));
                    }
                }

            return true;
        }

        public bool WalkToFollow(WorldPosData pos, Player player) {
            WorldPosData modifyMove = player.ModifyMove(pos);

            if (!FollowLanded && player.ValidPosition(FollowPos.X, FollowPos.Y)) {
                WorldPosData followDiff = Player.Position - FollowPos;
                followDiff = followDiff.Absolute();

                WorldPosData clientDiff = Player.Position - modifyMove;
                clientDiff = clientDiff.Absolute();

                if (clientDiff.X >= followDiff.X && clientDiff.Y >= followDiff.Y) {
                    modifyMove = FollowPos;
                    FollowLanded = true;
                }
            }

            return player.MoveTo(modifyMove.X, modifyMove.Y);
        }

        public void Follow(WorldPosData pos) {
            FollowVec = FollowPos - pos;
        }

        public override void HandleAfterUpdate(Packet packet) {
            Handlers.HandlePacketPostFrame(packet, this);
        }

        public override void Handle(Packet packet) {
            Handlers.HandlePacket(packet, this);
        }
    }
}