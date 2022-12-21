using System;
using System.Collections.Generic;
using System.Linq;
using BotCore.Game.Worlds.Entities;
using BotCore.Structures;
using BotTools;
using Rekishi;
using RotMGCore.Structures.Game;

namespace BotCore.Game.Worlds {
    public partial class Census {

        private readonly World world;
        private readonly Dictionary<int, BasicObject> _basicObjects;
        private readonly Dictionary<int, GameObject> _gameObjects;
        private readonly List<GameObject> _vulnerableGameObjects;
        private readonly GameObject[,] _staticObjectsMap;
        private readonly List<BasicObject> _objectsToAdd;
        private readonly List<int> _objectsToRemove;
        private readonly List<GameObject>[,] _objectsMap;
        private readonly List<GameObject>[,] _vulnerableObjectsMap;

        private Map _map => world.Map;
        private RealmBot _bot => world.Bot;
        private bool _updatingPhysics;

        public Census(World world, int width, int height) {
            this.world = world;

            _basicObjects = new Dictionary<int, BasicObject>();
            _gameObjects = new Dictionary<int, GameObject>();
            _vulnerableGameObjects = new List<GameObject>();
            _staticObjectsMap = new GameObject[width, height];
            _objectsToAdd = new List<BasicObject>();
            _objectsToRemove = new List<int>();
            _objectsMap = new List<GameObject>[width, height];
            _vulnerableObjectsMap = new List<GameObject>[width, height];

            fake_object_ids = new Dictionary<int, int>();
            hit_queue = new List<HitItem>();
        }

        public void Update(int time, int dt) {
            _updatingPhysics = true;

            foreach (GameObject go in _gameObjects.Values)
                if (!go.Update(time, dt))
                    _objectsToRemove.Add(go.ObjectId);

            foreach (BasicObject bo in _basicObjects.Values)
                if (!bo.Update(time, dt))
                    _objectsToRemove.Add(bo.ObjectId);

            HitItem[] hits = GetHitsAndClear();
            foreach (HitItem hit in hits)
                world.Bot.PlayerHit(hit.BulletId, hit.ObjectId);

            _updatingPhysics = false;

            foreach (BasicObject bo in _objectsToAdd)
                AddObjectImmediately(bo);
            _objectsToAdd.Clear();

            foreach (int objectId in _objectsToRemove)
                RemoveObjectImmediately(objectId);
            _objectsToRemove.Clear();
        }

        public void AddObject(BasicObject bo, double x, double y) {
            /* If physics are current updating, add object post frame, else remove immediately. */

            bo.X = x;
            bo.Y = y;

            if (_updatingPhysics)
                _objectsToAdd.Add(bo);
            else
                AddObjectImmediately(bo);

            if (!(bo is Player player)) return;
            if (player.ObjectId != _bot.ObjectId) return;

            world.Bot.Player = player;
            player.FullOccupy = _map.FullOccupy;
        }

        public void AddObjectImmediately(BasicObject bo) {
            /*
             * Stop if BasicObject cannot be added.
             * If GameObject, add to collection of game objects.
             * If not a GameObject, add to collection of basic objects.
             */

            if (!bo.AddTo(world, bo.X, bo.Y)) {
                Logger.Log("Census", $"Failure when adding [{bo}] X: {bo.X} Y: {bo.Y}", ConsoleColor.Red);
                return;
            }

            bool isGameObject = bo is GameObject;

            if (isGameObject) {
                if (_gameObjects.ContainsKey(bo.ObjectId)) {
                    Logger.Log("Census", "Tried to add a duplicate game object! Not adding object...", ConsoleColor.Red);
                    return;
                }

                GameObject go = (GameObject) bo;

                if (go.Static)
                    UpdateStaticMap(go, (int) bo.X, (int) bo.Y);

                _gameObjects[bo.ObjectId] = go;
                _bot.GameObjectAdded(go);
            }
            else {
                if (_basicObjects.ContainsKey(bo.ObjectId)) {
                    Logger.Log("Census", "Tried to add a duplicate basic object! Not adding object...", ConsoleColor.Red);
                    return;
                }

                _basicObjects[bo.ObjectId] = bo;
                _bot.BasicObjectAdded(bo);
            }
        }

        public void RemoveObject(int objectId) {
            /* If physics are current updating, remove object post frame, else remove immediately. */

            if (_updatingPhysics)
                _objectsToRemove.Add(objectId);
            else
                RemoveObjectImmediately(objectId);
        }

        public void RemoveObjectImmediately(int objectId) {
            /* If game objects collection doesn't contain ObjectId, automatically assume it is a BasicObject. */

            if (_gameObjects.ContainsKey(objectId)) {
                /*
                 * Remove object from proper object maps.
                 * Tell game object it has been removed from map.
                 * Remove game object from game objects collection.
                 */

                GameObject go = _gameObjects[objectId];

                if (go.Static)
                    RemoveFromStaticMap((int) go.X, (int) go.Y);

                RemoveFromObjectMap(go, go.LastPosition.X, go.LastPosition.Y);

                if (go.ShouldUpdateVulnerable)
                    TryRemoveObjectFromVulnerableMap(go, go.LastPosition.X, go.LastPosition.Y);

                go.RemoveFromMap();
                _gameObjects.Remove(objectId);
            }
            else {
                if (!_basicObjects.TryGetValue(objectId, out BasicObject bo)) return;

                /*
                * Tell basic object it has been removed from map.
                * Remove basic object from basic objects collection.
                */

                bo.RemoveFromMap();
                _basicObjects.Remove(objectId);
            }
        }

        public void UpdateStaticMap(GameObject so, int x, int y) {
            _staticObjectsMap[x, y] = so;
        }

        public void RemoveFromStaticMap(int x, int y) {
            _staticObjectsMap[x, y] = null;
        }

        public GameObject GetStaticObject(int x, int y) {
            return !_map.TileExists(x, y) ? null : _staticObjectsMap[x, y];
        }

        /// <summary>
        /// Gets a GameObject from Map.
        /// </summary>
        /// <param name="objId">Object Id of GameObject.</param>
        /// <returns></returns>
        public T GetGameObject<T>(int objId) where T : GameObject {
            if (!_gameObjects.TryGetValue(objId, out GameObject go)) return null;
            if (go is T gameObject)
                return gameObject;

            return null;
        }

        public GameObject GetGameObject(int objId) {
            return _gameObjects.TryGetValue(objId, out GameObject go) ? go : null;
        }

        /// <summary>
        /// Gets all GameObjects of a certain subclass.
        /// </summary>
        /// <typeparam name="Obj">GameObjects you want to receive.</typeparam>
        /// <returns></returns>
        public Obj[] GetGameObjects<Obj>() where Obj : GameObject {
            List<Obj> objects = new List<Obj>();

            foreach (GameObject i in _gameObjects.Values)
                if (i is Obj obj)
                    objects.Add(obj);

            return objects.ToArray();
        }

        public Obj[] GetGameObjects<Obj>(int x, int y) where Obj : GameObject {
            List<Obj> objs = new List<Obj>();
            List<GameObject> gameObjects = _objectsMap[x, y] ?? new List<GameObject>();

            foreach (GameObject go in gameObjects)
                if (go is Obj obj)
                    objs.Add(obj);

            return objs.ToArray();
        }

        public List<GameObject> GetObjects(int x, int y) {
            return _objectsMap[x, y];
        }

        public GameObject[] GetObjectsAndSurrounding(int x, int y) {
            IEnumerable<GameObject> objects = _objectsMap[x, y];

            foreach (IntPoint p in IntPoint.SurroundingPoints) {
                int px = x + p.X;
                int py = y + p.Y;

                IEnumerable<GameObject> gos = _objectsMap[px, py];

                if (gos != null) {
                    if (objects == null)
                        objects = new List<GameObject>();

                    objects = objects.Concat(gos);
                }
            }

            return objects.ToArray();
        }

        public GameObject[] GetVulnerableObjects() {
            return _vulnerableGameObjects.ToArray();
        }

        public GameObject[] GetSurroundingVulnerable(int x, int y) {
            IEnumerable<GameObject> objects = _vulnerableObjectsMap[x, y] ?? new List<GameObject>();

            foreach (IntPoint p in IntPoint.SurroundingPoints) {
                int px = x + p.X;
                int py = y + p.Y;

                IEnumerable<GameObject> gos = _vulnerableObjectsMap[px, py];

                if (gos != null)
                    objects = objects.Concat(gos);
            }

            return objects.ToArray();
        }

        public void UpdateObjectMap(GameObject go, int x, int y, int oldX, int oldY, bool first) {
            /*
             * If not the first update, of Object from previous position.
             * Create list for positions on coordinate if it doesn't exist.
             * Add object to collection of positions on map and update vulnerable map if necessary.
             * Set previous position of GameObject.
             */

            if (!first) RemoveFromObjectMap(go, oldX, oldY);

            List<GameObject> list = _objectsMap[x, y];
            if (list == null) {
                list = new List<GameObject>();
                _objectsMap[x, y] = list;
            }

            list.Add(go);

            if (go.ShouldUpdateVulnerable)
                UpdateVulnerableObjectMap(go, x, y, oldX, oldY, go.Vulnerable);

            go.LastPosition = go.MapPosition;
        }

        public void UpdateVulnerableObjectMap(GameObject go, int x, int y, int oldX, int oldY, bool vulnerable) {
            /* 
             * Tries to remove object from Vulnerable map. 
             * If object is vulnerable, add to vulnerable map on new position.
             */

            TryRemoveObjectFromVulnerableMap(go, oldX, oldY);

            if (vulnerable) {
                List<GameObject> list = _vulnerableObjectsMap[x, y];
                if (list == null) {
                    list = new List<GameObject>();
                    _vulnerableObjectsMap[x, y] = list;
                }

                list.Add(go);
                _vulnerableGameObjects.Add(go);
            }
            else {
                if (_vulnerableGameObjects.Contains(go))
                    _vulnerableGameObjects.Remove(go);
            }
        }

        public void RemoveFromObjectMap(GameObject go, int x, int y) {
            List<GameObject> list = _objectsMap[x, y];

            list.Remove(go);
            if (list.Count == 0)
                _objectsMap[x, y] = null;
        }

        public void TryRemoveObjectFromVulnerableMap(GameObject go, int x, int y) {
            List<GameObject> list = _vulnerableObjectsMap[x, y];

            // Not a memory leak.
            if (list == null) return;

            list.Remove(go);
            if (list.Count == 0)
                _vulnerableObjectsMap[x, y] = null;
        }

        public Player SearchForPlayer(string name) {
            foreach (GameObject go in _gameObjects.Values)
                if (go.Name == name && go is Player player)
                    return player;
            return null;
        }
    }
}