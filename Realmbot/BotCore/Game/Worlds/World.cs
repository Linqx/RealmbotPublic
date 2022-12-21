using System;
using Botcore.Content;
using Botcore.Data;
using BotCore.Game.Worlds.Entities;
using BotCore.Networking;
using BotCore.Structures;
using BotTools;

namespace BotCore.Game.Worlds {
    public class World {
        public Map Map;
        public Census Census;
        public RealmBot Bot;
        public Player Player;

        public DateTime StartTime;
        public int CurrentUpdate;
        public int LastUpdate;
        public int LastTickId;
        public int LastMoveTime;

        public World(int width, int height) {
            Census = new Census(this, width, height);

            StartTime = DateTime.Now;
        }

        public void Update(int time, int dt) {
            Census.Update(time, dt);
        }

        public void OnUpdate(UpdatePacket update) {
            foreach (GroundTileData tile in update.Tiles)
                Map.SetTile(tile);

            foreach (ObjectData data in update.NewObjects) {
                GameObjectData goData = GameContent.GetGameObjectData(data.Type);

                if (goData == null) {
                    Logger.Log("World", $"Unkown GameObject Received: {data.Type}", ConsoleColor.Yellow);
                    continue;
                }

                GameObject go = GameObject.Resolve(goData);
                go.ObjectId = data.Status.ObjectId;
                Census.AddObject(go, data.Status.Position.X, data.Status.Position.Y);
                go.ApplyObjectData(data, 0, -1);
            }

            foreach (int id in update.Drops)
                if (Census.GetGameObject(id) != null)
                    Census.RemoveObject(id);
        }

        public void OnNewTick(NewTickPacket newTick) {
            foreach (ObjectStatusData data in newTick.Status) {
                GameObject go = Census.GetGameObject(data.ObjectId);
                go?.ApplyStatusData(data, newTick.TickTime, newTick.TickId);
            }

            LastTickId = newTick.TickId;
        }

        public void GoTo(GoToPacket goTo) {
            GameObject go = Census.GetGameObject(goTo.ObjectId);
            if (go != null)
                go.OnGoTo(goTo.Pos, LastUpdate);
            else
                Logger.Log("World", $"Received GoTo for unkown GameObject! Id: {goTo.ObjectId}", ConsoleColor.Red);
        }
    }
}