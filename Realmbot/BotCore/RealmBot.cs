using System.Collections.Generic;
using BotCore.Game.Worlds;
using BotCore.Game.Worlds.Entities;
using BotCore.Networking;
using BotCore.Structures;
using BotCore.Task;
using BotTools;

namespace BotCore {
    public partial class RealmBot {

        public const int MAX_PLAYER_TEXT_LENGTH = 128;

        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public byte BulletId { get; set; } = 1;
        public RandomSync RandomSync { get; set; }
        public World World { get; set; }
        public int CharId { get; set; }
        public int ObjectId { get; set; }
        public Player Player { get; set; }
        public bool VerboseLogging { get; set; }
        public string Name { get; set; } = "Realm Bot";
        public Census Census => World.Census;
        public Map Map => World.Map;

        private readonly List<RotMGPacket> _packetsQueue;
        private readonly object _packetSync = new object();

        public RealmBot(string email, string password) {
            /* Construct RealmBot. */

            Email = email;
            Password = password;

            _moveActions = new List<MoveAction>();
            _packetsQueue = new List<RotMGPacket>();
        }

        /// <summary>
        /// Grabs the current bullet id and sets the next bullet id.
        /// </summary>
        /// <returns>Current bullet id.</returns>
        private byte GetBulletId() {
            /* Get the current bullet id and set the next bullet id. */

            byte id = BulletId;
            BulletId = (byte) ((BulletId + 1) % 128);
            return id;
        }

        public virtual void GameObjectAdded(GameObject go) {
            /* Base method. */
        }

        public virtual void BasicObjectAdded(BasicObject bo) {
            /* Base method. */
        }
    }
}