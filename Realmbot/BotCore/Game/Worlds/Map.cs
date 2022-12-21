using System.Linq;
using Botcore.Content;
using Botcore.Data;
using BotCore.Game.Worlds.Entities;
using BotCore.Networking;
using BotCore.Structures;

namespace BotCore.Game.Worlds {
    public class Map {

        public static string[] SAFE_MAPS =
            {"Nexus", "Vault", "Guild Hall", "Cloth Bazaar", "Nexus Explanation", "Daily Quest Room"};

        private Census _census => world.Census;

        /// <summary>
        /// Width of the Map.
        /// </summary>
        public int Width;

        /// <summary>
        /// Height of the Map.
        /// </summary>
        public int Height;

        /// <summary>
        /// Name of the Map.
        /// </summary>
        public string Name;

        /// <summary>
        /// Difficulty of the Map.
        /// </summary>
        public int Difficulty;

        /// <summary>
        /// Depicts whether the Map allows teleports.
        /// </summary>
        public bool AllowTeleport;

        /// <summary>
        /// Shows whether this map is safe.
        /// </summary>
        public bool IsSafeMap;

        private World world;

        /// <summary>
        /// A 2 dimensional array that return a boolean whether the tile has been discovered.
        /// </summary>
        private bool[,] _tileDiscovered;

        /// <summary>
        /// A 2 dimensonal array that returns the type of tile or null whether the tile has been discovered.
        /// </summary>
        private ushort?[,] _tiles;

        /// <summary>
        /// Creates a new map from the information from <see cref="MapInfoPacket"/>
        /// </summary>
        /// <param name="mapInfo"><see cref="MapInfoPacket"/> used to create map.</param>
        /// <returns></returns>
        public static Map FromMapInfo(World world, MapInfoPacket mapInfo) {
            Map map = new Map {
                world = world,
                Width = mapInfo.Width,
                Height = mapInfo.Height,
                Name = mapInfo.Name,
                Difficulty = mapInfo.Difficulty,
                AllowTeleport = mapInfo.AllowTeleport,
                _tiles = new ushort?[mapInfo.Width, mapInfo.Height],
                _tileDiscovered = new bool[mapInfo.Width, mapInfo.Height],
                IsSafeMap = SAFE_MAPS.Contains(mapInfo.Name)
            };

            return map;
        }

        public void SetTile(GroundTileData tileData) {
            _tiles[tileData.X, tileData.Y] = tileData.Type;
            _tileDiscovered[tileData.X, tileData.Y] = true;
        }

        /// <summary>
        /// Shows if tile has been discovered.
        /// </summary>
        /// <param name="x">X position on map.</param>
        /// <param name="y">Y position on map.</param>update
        /// <returns></returns>
        private bool TileDiscovered(int x, int y) {
            if (x < 0 || x >= Width ||
                y < 0 || y >= Height || _tileDiscovered == null)
                return false;

            return _tileDiscovered[x, y];
        }

        public bool TileExists(int x, int y) {
            if (x < 0 || x >= Width ||
                y < 0 || y >= Height)
                return false;

            return true;
        }

        /// <summary>
        /// Returns the tile type of a position or null if it's not discovered.
        /// </summary>
        /// <param name="x">X position on map.</param>
        /// <param name="y">Y position on map.</param>
        /// <returns></returns>
        public ushort? GetTile(int x, int y) {
            if (!TileExists(x, y))
                return null;

            if (!TileDiscovered(x, y))
                return null;

            if (_tiles[x, y].HasValue) {
                if (_tiles[x, y].Value == 0xFFFF)
                    return null;

                return _tiles[x, y].Value;
            }

            return null;
        }

        public bool Walkable(int x, int y) {
            ushort? getTileType = GetTile(x, y);
            if (!getTileType.HasValue) return false;
            ushort tileType = getTileType.Value;
            TileData tileData = GameContent.GetTileData(tileType);

            if (tileData.NoWalk)
                return false;

            GameObject so = _census.GetStaticObject(x, y);
            if (so != null && ((StaticObjectData) so.Data).OccupySquare)
                return false;

            return true;
        }

        public bool FullOccupy(double x, double y) {
            int ix = (int) x;
            int iy = (int) y;
            ushort? getTileType = GetTile(ix, iy);
            if (!getTileType.HasValue)
                return true;

            ushort tileType = getTileType.Value;
            if (tileType == 0xff)
                return true;

            GameObject so = _census.GetStaticObject(ix, iy);
            if (so != null && ((StaticObjectData) so.Data).FullOccupy)
                return true;

            return false;
        }
    }
}