using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Botcore.Data;
using BotTools;

namespace Botcore.Content {
    public class GameContent {
        public static string PathToXmls = Environment.CurrentDirectory + "\\common";

        private static Dictionary<ushort, GameObjectData> TypeToGameObject;
        private static Dictionary<ushort, TileData> TypeToTile;
        private static Dictionary<ushort, ItemData> TypeToItem;
        private static Dictionary<ushort, ProjectileData> TypeToProjectile;
        private static Dictionary<ushort, PlayerData> TypeToPlayer;
        private static Dictionary<ushort, PetData> TypeToPet;
        private static Dictionary<ushort, PortalData> TypeToPortal;
        private static Dictionary<ushort, RegionData> TypeToRegion;

        private static Dictionary<string, GameObjectData> IdToGameObject;
        private static Dictionary<string, TileData> IdToTile;
        private static Dictionary<string, ItemData> IdToItem;
        private static Dictionary<string, ProjectileData> IdToProjectile;
        private static Dictionary<string, PlayerData> IdToPlayer;
        private static Dictionary<string, PetData> IdToPet;
        private static Dictionary<string, PortalData> IdToPortal;
        private static Dictionary<string, RegionData> IdToRegion;

        public static void Initialize() {
            Preload();
            Load();
        }

        private static void Preload() {
            TypeToGameObject = new Dictionary<ushort, GameObjectData>();
            TypeToTile = new Dictionary<ushort, TileData>();
            TypeToItem = new Dictionary<ushort, ItemData>();
            TypeToProjectile = new Dictionary<ushort, ProjectileData>();
            TypeToPlayer = new Dictionary<ushort, PlayerData>();
            TypeToPet = new Dictionary<ushort, PetData>();
            TypeToPortal = new Dictionary<ushort, PortalData>();
            TypeToRegion = new Dictionary<ushort, RegionData>();

            IdToGameObject = new Dictionary<string, GameObjectData>(StringComparer.OrdinalIgnoreCase);
            IdToTile = new Dictionary<string, TileData>(StringComparer.OrdinalIgnoreCase);
            IdToItem = new Dictionary<string, ItemData>(StringComparer.OrdinalIgnoreCase);
            IdToProjectile = new Dictionary<string, ProjectileData>(StringComparer.OrdinalIgnoreCase);
            IdToPlayer = new Dictionary<string, PlayerData>(StringComparer.OrdinalIgnoreCase);
            IdToPet = new Dictionary<string, PetData>(StringComparer.OrdinalIgnoreCase);
            IdToPortal = new Dictionary<string, PortalData>(StringComparer.OrdinalIgnoreCase);
            IdToRegion = new Dictionary<string, RegionData>(StringComparer.OrdinalIgnoreCase);
        }

        private static void Load() {
            Logger.Log("Content", $"Loading Content from: {PathToXmls}");
            string[] xmls = Directory.EnumerateFiles(PathToXmls, "*.xml", SearchOption.AllDirectories).ToArray();
            for (int i = 0; i < xmls.Length; i++) {
                Logger.Log("Content", $"Loading \"{Path.GetFileName(xmls[i])}\" ({i + 1}/{xmls.Length})");
                using (Stream stream = File.OpenRead(xmls[i])) {
                    LoadXml(XElement.Load(stream));
                }
            }

            xmls = Directory.EnumerateFiles(PathToXmls, "*.dat", SearchOption.AllDirectories).ToArray();
            for (int i = 0; i < xmls.Length; i++) {
                Logger.Log("Content", $"Loading \"{Path.GetFileName(xmls[i])}\" ({i + 1}/{xmls.Length})");
                using (Stream stream = File.OpenRead(xmls[i])) {
                    LoadXml(XElement.Load(stream));
                }
            }


            Logger.Log("Content", $"Loaded: {TypeToGameObject.Count} GameObjects", ConsoleColor.White);
            Logger.Log("Content", $"Loaded: {TypeToTile.Count} Tiles", ConsoleColor.White);
            Logger.Log("Content", $"Loaded: {TypeToItem.Count} Items", ConsoleColor.White);
            Logger.Log("Content", $"Loaded: {TypeToProjectile.Count} Projectiles", ConsoleColor.White);
            Logger.Log("Content", $"Loaded: {TypeToPlayer.Count} Players", ConsoleColor.White);
            Logger.Log("Content", $"Loaded: {TypeToPet.Count} Pets", ConsoleColor.White);
            Logger.Log("Content", $"Loaded: {TypeToPortal.Count} Portals", ConsoleColor.White);
            Logger.Log("Content", $"Loaded: {TypeToRegion.Count} Regions", ConsoleColor.White);
        }

        private static void LoadXml(XElement element) {
            LoadObjects(element);
            LoadTiles(element);
            LoadRegions(element);
        }

        private static void LoadObjects(XElement element) {
            foreach (XElement elem in element.XPathSelectElements("//Object"))
                switch (BasicObjectData.Resolve(elem)) {
                    case GameObjectData go:
                        switch (go) {
                            case CharacterData chr:
                                if (chr is PlayerData player) {
                                    player.Parse(elem);
                                    TypeToPlayer[player.Type] = player;
                                    IdToPlayer[player.Id] = player;

                                    TypeToGameObject[player.Type] = chr;
                                    IdToGameObject[player.Id] = chr;
                                }
                                else {
                                    chr.Parse(elem);
                                    TypeToGameObject[chr.Type] = chr;
                                    IdToGameObject[chr.Id] = chr;
                                }

                                break;
                            case PetData pet:
                                pet.Parse(elem);
                                TypeToPet[pet.Type] = pet;
                                IdToPet[pet.Id] = pet;

                                TypeToGameObject[pet.Type] = pet;
                                IdToGameObject[pet.Id] = pet;
                                break;
                            case StaticObjectData stat:
                                if (stat is PortalData portal) {
                                    portal.Parse(elem);
                                    TypeToPortal[portal.Type] = portal;
                                    IdToPortal[portal.Id] = portal;

                                    TypeToGameObject[portal.Type] = stat;
                                    IdToGameObject[portal.Id] = stat;
                                }
                                else {
                                    stat.Parse(elem);
                                    TypeToGameObject[stat.Type] = stat;
                                    IdToGameObject[stat.Id] = stat;
                                }

                                break;
                            default:
                                go.Parse(elem);
                                TypeToGameObject[go.Type] = go;
                                IdToGameObject[go.Id] = go;
                                break;
                        }

                        break;
                    case ProjectileData prj:
                        prj.Parse(elem);
                        TypeToProjectile[prj.Type] = prj;
                        IdToProjectile[prj.Id] = prj;
                        break;
                    case ItemData item:
                        item.Parse(elem);
                        TypeToItem[item.Type] = item;
                        IdToItem[item.Id] = item;
                        break;
                }
        }

        private static void LoadTiles(XElement element) {
            foreach (XElement elem in element.XPathSelectElements("//Ground")) {
                TileData tileData = new TileData();
                tileData.Parse(elem);
                TypeToTile[tileData.Type] = tileData;
                IdToTile[tileData.Id] = tileData;
            }
        }

        private static void LoadRegions(XElement element) {
            foreach (XElement elem in element.XPathSelectElements("//Region")) {
                RegionData regionData = new RegionData();
                regionData.Parse(elem);
                TypeToRegion[regionData.Type] = regionData;
                IdToRegion[regionData.Id] = regionData;
            }
        }

        public static GameObjectData GetGameObjectData(ushort type) {
            if (TypeToGameObject.TryGetValue(type, out GameObjectData data))
                return data;
            return null;
        }

        public static GameObjectData GetGameObjectData(string id) {
            if (IdToGameObject.TryGetValue(id, out GameObjectData data))
                return data;
            return null;
        }

        public static TileData GetTileData(ushort type) {
            if (TypeToTile.TryGetValue(type, out TileData data))
                return data;
            return null;
        }

        public static TileData GetTileData(string id) {
            if (IdToTile.TryGetValue(id, out TileData data))
                return data;
            return null;
        }

        public static ItemData GetItemData(ushort type) {
            if (TypeToItem.TryGetValue(type, out ItemData data))
                return data;
            return null;
        }

        public static ItemData GetItemData(string id) {
            if (IdToItem.TryGetValue(id, out ItemData data))
                return data;
            return null;
        }

        public static ProjectileData GetProjectieData(ushort type) {
            if (TypeToProjectile.TryGetValue(type, out ProjectileData data))
                return data;
            return null;
        }

        public static ProjectileData GetProjectieData(string id) {
            if (IdToProjectile.TryGetValue(id, out ProjectileData data))
                return data;
            return null;
        }

        public static PlayerData GetPlayerData(ushort type) {
            if (TypeToPlayer.TryGetValue(type, out PlayerData data))
                return data;
            return null;
        }

        public static PlayerData GetPlayerData(string id) {
            if (IdToPlayer.TryGetValue(id, out PlayerData data))
                return data;
            return null;
        }

        public static PetData GetPetData(ushort type) {
            if (TypeToPet.TryGetValue(type, out PetData data))
                return data;
            return null;
        }

        public static PetData GetPetData(string id) {
            if (IdToPet.TryGetValue(id, out PetData data))
                return data;
            return null;
        }

        public static PortalData GetPortalData(ushort type) {
            if (TypeToPortal.TryGetValue(type, out PortalData data))
                return data;
            return null;
        }

        public static PortalData GetPortalData(string id) {
            if (IdToPortal.TryGetValue(id, out PortalData data))
                return data;
            return null;
        }

        public static RegionData GetRegionData(ushort type) {
            if (TypeToRegion.TryGetValue(type, out RegionData data))
                return data;
            return null;
        }

        public static RegionData GetRegionData(string id) {
            if (IdToRegion.TryGetValue(id, out RegionData data))
                return data;
            return null;
        }
    }
}