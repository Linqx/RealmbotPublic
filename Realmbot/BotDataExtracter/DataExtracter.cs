using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using BotDataExtracter.Urls;
using BotTools;

namespace BotDataExtracter {
    public class DataExtracter {
        public delegate void ExtractionComplete();

        private const string LOGGING_SENDER = "Data Extracter";

        public static Dictionary<string, byte> PacketTypes;
        public static string BuildVersion;

        private static readonly string COMMON_PATH = $"{Environment.CurrentDirectory}\\common";
        private static readonly string OBJECTS_PATH = $"{COMMON_PATH}\\objects.xml";
        private static readonly string TEMP_PATH = $"{COMMON_PATH}\\temp";
        private static readonly string TILES_PATH = $"{COMMON_PATH}\\tiles.xml";
        private static readonly string PACKETS_PATH = $"{COMMON_PATH}\\packets.xml";
        private static readonly string VERSION_PATH = $"{COMMON_PATH}\\version.txt";
        private static readonly string CLIENT_PATH = $"{COMMON_PATH}\\client.swf";

        private static readonly string GSC_PATH =
            $"{COMMON_PATH}\\scripts\\kabam\\rotmg\\messaging\\impl\\GameServerConnection.as";

        private static readonly string PARAMETERS_PATH =
            $"{COMMON_PATH}\\scripts\\com\\company\\assembleegameclient\\parameters\\Parameters.as";

        private static readonly string BUILD_VERSION_PATH = $"{COMMON_PATH}\\build_version.txt";
        private static readonly WebRequester WebRequester;

        private static string LocalVersionCache;

        private static string LocalVersion {
            get {
                if (string.IsNullOrEmpty(LocalVersionCache))
                    LocalVersionCache = File.ReadAllText(VERSION_PATH);
                return LocalVersionCache;
            }
        }

        public ExtractionComplete OnExtractionComplete;
        private readonly ExtractionType _extrationType;

        static DataExtracter() {
            WebRequester = new WebRequester();
        }

        public DataExtracter(ExtractionType extrationType) {
            _extrationType = extrationType;
        }

        public void Extract() {
            if ((_extrationType & (ExtractionType.OBJECTS | ExtractionType.TILES)) != 0) ExtractBinaryData();
            if ((_extrationType & ExtractionType.OBJECTS) != 0) ExtractObjects();
            if ((_extrationType & ExtractionType.TILES) != 0) ExtractTiles();
            if ((_extrationType & ExtractionType.PACKETS) != 0) ExtractPackets();

            OnComplete();
        }

        private void OnComplete() {
            /*
             * Call given callback action.
             * Delete downloaded client folder.
             * Dispose all resources.
             */

            OnExtractionComplete?.Invoke();
            DeleteFile(CLIENT_PATH);
            DeleteDirectory(TEMP_PATH);
        }

        public static void SetPacketIds() {
            /*
             * Load the packets xml file.
             * Create a dictionary of packet name to packet id.
             */

            using (Stream str = File.OpenRead(PACKETS_PATH)) {
                XElement packetsElement = XElement.Load(str);
                PacketTypes = packetsElement.XPathSelectElements("//Packet")
                    .ToDictionary(_ => _.Value,
                        _ => byte.Parse(_.Attribute("type")?.Value ?? throw new ArgumentNullException()));
            }
        }

        public static void SetBuildVersion() {
            /* Set the build version used to connect to Realm of the Mad God. */

            BuildVersion = File.ReadAllText(BUILD_VERSION_PATH);
        }

        private static void ExtractBinaryData() {
            CheckDirectory(TEMP_PATH);

            Logger.Log(LOGGING_SENDER, "Extracting binary data from client...", ConsoleColor.White);

            ProcessStartInfo startInfo = new ProcessStartInfo("java", "-jar ffdec.jar -export" +
                                                                      $" binaryData {TEMP_PATH} {CLIENT_PATH}") {
                UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden
            };

            // Program writes to this program instead of opening a new one.
            // Start ffdec and export classes: Parameters and GameServerConnection from client.
            Process process = Process.Start(startInfo);
            process?.WaitForExit();
        }

        private static void ExtractObjects() {
            CheckFile(OBJECTS_PATH);

            StringBuilder objectsXML = new StringBuilder();
            objectsXML.Append("<Objects>");

            string[] xmls = Directory.EnumerateFiles(TEMP_PATH, "*.bin", SearchOption.AllDirectories).ToArray();
            foreach (string path in xmls) {
                if (path.Contains("EmbeddedData_objectPatchMXL")) continue;

                string[] lines = File.ReadAllLines(path);
                string currentXml = "";
                int start = -1;
                int end = -1;
                foreach (string line in lines) {
                    if (line.Contains("<Object ")) start = 1;
                    if (line.Contains("</Object>")) end = 1;

                    if (start != -1)
                        currentXml += line.Trim();

                    if (end == -1) continue;
                    if (!string.IsNullOrEmpty(currentXml))
                        objectsXML.Append($"{currentXml}");

                    currentXml = "";
                    start = -1;
                    end = -1;
                }
            }

            objectsXML.Append("</Objects>");

            File.WriteAllText(OBJECTS_PATH, objectsXML.ToString());
            Logger.Log(LOGGING_SENDER, "Successfully extracted objects from client!");
        }

        private static void ExtractTiles() {
            CheckFile(TILES_PATH);

            StringBuilder objectsXML = new StringBuilder();
            objectsXML.Append("<GroundTypes>");

            string[] xmls = Directory.EnumerateFiles(TEMP_PATH, "*.bin", SearchOption.AllDirectories).ToArray();
            foreach (string path in xmls) {
                if (path.Contains("EmbeddedData_objectPatchMXL")) continue;

                string[] lines = File.ReadAllLines(path);
                string currentXml = "";
                int start = -1;
                int end = -1;
                foreach (string line in lines) {
                    if (line.Contains("<Ground ")) start = 1;
                    if (line.Contains("</Ground>")) end = 1;

                    if (start != -1)
                        currentXml += line.Trim();

                    if (end == -1) continue;
                    if (!string.IsNullOrEmpty(currentXml))
                        objectsXML.Append($"{currentXml}");

                    currentXml = "";
                    start = -1;
                    end = -1;
                }
            }

            objectsXML.Append("</GroundTypes>");

            File.WriteAllText(TILES_PATH, objectsXML.ToString());
            Logger.Log(LOGGING_SENDER, "Successfully extracted tiles from client!");
        }

        /// <summary>
        /// Extracts packets and build version from client. Assumes cleint is not obfuscated. 
        /// </summary>
        private void ExtractPackets() {
            CheckFile(PACKETS_PATH);
            CheckFile(BUILD_VERSION_PATH);

            Logger.Log(LOGGING_SENDER, "Extracting packets and build version from client...", ConsoleColor.White);

            ProcessStartInfo startInfo = new ProcessStartInfo("java",
                "-jar ffdec.jar -selectclass com.company.assembleegameclient.parameters.Parameters,kabam.rotmg.messaging.impl.GameServerConnection -export" +
                $" script {COMMON_PATH} {CLIENT_PATH}") {
                UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden
            };

            // Program writes to this program instead of opening a new one.
            // Start ffdec and export classes: Parameters and GameServerConnection from client.
            Process process = Process.Start(startInfo);
            process?.WaitForExit();

            WritePackets();
            WriteParameters();

            ClearFolder($"{COMMON_PATH}\\scripts"); // Clear scripts folder and all content in it.
        }

        private static void WritePackets() {
            if (File.Exists(GSC_PATH)) // Export and Write Packet information needed
            {
                List<PacketData> packetDatas = new List<PacketData>();
                StringBuilder packetsEnumBuilder = new StringBuilder();

                foreach (string line in File.ReadAllLines(GSC_PATH)) {
                    string lineTrimmed = line.Trim();

                    if (lineTrimmed.Contains("public static const")) {
                        // Name of packet is in index 3 of the split (including :int)
                        // Id of packet is in index 5 (indcluding the semicolon)
                        string[] split = lineTrimmed.Split(' ');
                        string id = split[3].Substring(0, split[3].Length - 4);
                        string type = split[5].Substring(0, split[5].Length - 1);

                        packetDatas.Add(new PacketData(id, byte.Parse(type)));
                    }
                }

                XElement packetsElement = new XElement("Packets", packetDatas.Select(_ => _.ToXml()));
                File.WriteAllText(PACKETS_PATH, packetsElement.ToString());
                Logger.Log(LOGGING_SENDER, "Successfully extracted packets from client!");
            }
            else {
                Logger.Log(LOGGING_SENDER, "Unable to locate class GameServerConnection.as from client!",
                    ConsoleColor.Red);
            }
        }

        private static void WriteParameters() {
            if (File.Exists(PARAMETERS_PATH)) // Export and Write information from Parameters that is needed.
            {
                string buildVersion = null;
                string minorVersion = null;
                int buildVersionLength = "public static const BUILD_VERSION:String = ".Length;
                int minorVersionLength = "public static const MINOR_VERSION:String = ".Length;

                foreach (string line in File.ReadAllLines(PARAMETERS_PATH)) {
                    string lineTrimmed = line.Trim();
                    //Logger.Log(SENDER, $"Line Length: {lineTrimmed.Length}");

                    if (lineTrimmed.Contains("public static const BUILD_VERSION:String")) {
                        buildVersion = lineTrimmed.Substring(buildVersionLength + 1,
                            lineTrimmed.Length - buildVersionLength - 3);
                        continue;
                    }

                    if (lineTrimmed.Contains("public static const MINOR_VERSION:String"))
                        minorVersion = lineTrimmed.Substring(minorVersionLength + 1,
                            lineTrimmed.Length - minorVersionLength - 3);

                    if (!string.IsNullOrEmpty(buildVersion) && !string.IsNullOrEmpty(minorVersion))
                        break;
                }

                File.WriteAllText(BUILD_VERSION_PATH, $"{buildVersion}.{minorVersion}");
                Logger.Log(LOGGING_SENDER, "Successfully extracted build version from client!");
            }
            else {
                Logger.Log(LOGGING_SENDER, "Unable to locate class Parameters.as from client!", ConsoleColor.Red);
            }
        }

        private static void CheckDirectory(string path) {
            /*
             * Check if given path exists.
             * If path doesn't exist, create given path.
             */

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path).Refresh();
        }

        private static void CheckFile(string path) {
            /*
             * Check if given path exists.
             * If path doesn't exist, create given path.
             */

            if (!File.Exists(path))
                File.Create(path).Close();
        }

        public static bool NewVersion() {
            bool newVersion = false;

            CheckDirectory(COMMON_PATH);

            if (File.Exists(VERSION_PATH)) {
                Logger.Log(LOGGING_SENDER, $"Checking if new version exists. Current local version: {LocalVersion}");

                WebRequester.SendRequest(DripsPw.VERSION_URL, response => {
                    if (string.IsNullOrEmpty(response)) {
                        Logger.Log(LOGGING_SENDER, "Failed to grab production version!", ConsoleColor.Red);
                    }
                    else {
                        if (response != LocalVersion) {
                            Logger.Log(LOGGING_SENDER, "New version found!");
                            File.WriteAllText(VERSION_PATH, response); // Write new version to cached version.
                            DownloadProductionClient(); // Download latest RotMG client
                            newVersion = true;
                        }
                        else {
                            Logger.Log(LOGGING_SENDER, "No new version found.");
                        }
                    }
                });
            }
            else {
                File.Create(VERSION_PATH).Close(); // Create new version
                Logger.Log(LOGGING_SENDER, "No local version exists, downloading production version!");

                WebRequester.SendRequest(DripsPw.VERSION_URL, response => {
                    if (string.IsNullOrEmpty(response)) {
                        Logger.Log(LOGGING_SENDER, "Failed to grab production version!", ConsoleColor.Red);
                    }
                    else {
                        File.WriteAllText(VERSION_PATH,
                            response); // Write new version to cached version.                       
                        DownloadProductionClient(); // Download latest RotMG client
                        newVersion = true;
                    }
                });
            }

            return newVersion;
        }

        private static void DownloadProductionClient() {
            AutoResetEvent wait = new AutoResetEvent(false);
            WebRequester.DownloadFileAsync(DripsPw.CLIENT_URL, CLIENT_PATH, () => wait.Set());
            wait.WaitOne();
        }

        private static void ClearFolder(string directory) {
            DirectoryInfo di = new DirectoryInfo(directory);
            Empty(di);
        }

        public static void Empty(DirectoryInfo directory) {
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
            foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            directory.Delete();
        }

        private static void DeleteFile(string path) {
            if (File.Exists(path))
                File.Delete(path);
        }

        private static void DeleteDirectory(string path) {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        public void Dispose() {
            WebRequester.Dispose();
        }
    }
}