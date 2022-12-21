using BotCore.Structures;
using RotMGCore.Structures.Game;

namespace BotCore.Game.Worlds.Entities {
    public class BasicObject {

        /// <summary>
        /// X position of the Game Object
        /// </summary>
        public double X {
            get => _x;
            set {
                _x = value;
                _position.X = _x;
                MapPosition.X = (int) _x;
            }
        }

        /// <summary>
        /// Y position of the Game Object
        /// </summary>
        public double Y {
            get => _y;
            set {
                _y = value;
                _position.Y = _y;
                MapPosition.Y = (int) _y;
            }
        }

        /// <summary>
        /// Current position of the Game Object
        /// </summary>
        public WorldPosData Position {
            get => _position;
            set {
                _position = value;

                _x = _position.X;
                _y = _position.Y;
                MapPosition.X = (int) _x;
                MapPosition.Y = (int) _y;
            }
        }

        protected Census census => World.Census;
        protected Map map => World.Map;

        /// <summary>
        /// Id of the object
        /// </summary>
        public int ObjectId;

        public IntPoint MapPosition = IntPoint.Zero;
        public IntPoint LastPosition = IntPoint.Zero;

        public World World;

        private WorldPosData _position = new WorldPosData(0);
        private double _x;
        private double _y;

        public virtual bool AddTo(World world, double x, double y) {
            //Don't think we need to worry about squares yet..

            World = world;
            X = x;
            Y = y;

            return true;
        }

        public virtual void RemoveFromMap() {
            World = null;
        }

        public virtual bool Update(int time, int dt) {
            return true;
        }
    }
}