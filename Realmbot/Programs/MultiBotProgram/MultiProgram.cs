using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using BotProgram;
using BotTools;
using MultiBot;

namespace MultiBotProgram {
    public class MultiProgram : RealmBotProgram {

        public static Settings Settings;
        public override string ModuleName => "Multi";

        private Dictionary<int, RealmMultiBot> bots;
        private RealmMultiBot leader;
        private Socket socket;
        private ProxyClient proxy;

        protected override void PreStart() {
            /*
             * Load Settings from file.
             * Call base method which creates the BotManager and starts it.
             * Iterate through all accounts found on file and create a bot for each account.
             * The first account found will be set as the leader client.
             */

            FPS = 60;
            Settings = Settings.FromResource("settings.json");
            bots = new Dictionary<int, RealmMultiBot>();

            base.PreStart();

            for (int i = 0; i < Settings.Accounts.Length; i++) {
                AccountDetails details = Settings.Accounts[i];
                RealmMultiBot multiBot = manager.AddBot<RealmMultiBot>(details.Email, details.Password);
                multiBot.VerboseLogging = false;
                multiBot.RSA = Settings.EncryptionData.RSA;
                multiBot.IncomingRC4 = Settings.EncryptionData.IncomingRC4;
                multiBot.OutgoingRC4 = Settings.EncryptionData.OutgoingRC4;
                multiBot.CharId = details.CharId;
                bots.Add(i, multiBot);

                if (i == 0)
                    leader = multiBot;
            }
        }

        protected override void Start() {
            /*
             * Connect all MultiBots into the Vault.
             * Create a socket and start accepting connections for Proxy Client.
             */

            ParseEnum.TryParse(Settings.PreferredServer, out Server server);

            for (int i = 0; i < bots.Count; i++) {
                RealmMultiBot multiBot = bots[i];
                multiBot.Connect(server, ConnectionMode.VAULT);
            }

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 2050));
            socket.Listen(0xff);
            socket.BeginAccept(AcceptCallback, null);
        }

        protected override void Stop() {
            /* Stop the MultiProgram */

            for (int i = 0; i < bots.Count; i++) {
                RealmMultiBot multiBot = bots[i];
                multiBot.Dispose();
            }

            bots.Clear();
            socket.Dispose();
            proxy.Dispose();
            proxy = null;

            base.Stop();
        }

        protected override void Update(int time, int dt) {
            /*
             * Called 60 times a second. FPS simulation set at PreStart.
             * Update in game physics.
             * Handle all received packets.
             */

            // Logger.Log("MultiProgram", $"Update: Time Elapsed: {time} Delta: {dt}");

            base.Update(time, dt);
        }

        private void ProxyDisconnected() {
            /*
             * Dispose the current Proxy Client.
             * Begin accepting connections for another Proxy Client.
             */

            Logger.Log("MultiProgram", "Proxy Client has disconnected...", ConsoleColor.Red);

            proxy.Dispose();
            proxy = null;
            socket.BeginAccept(AcceptCallback, null);
        }

        private void AcceptCallback(IAsyncResult ar) {
            /*
             * Accept socket and if it's not null, create Proxy Client.
             * Only listen for another Proxy Client after current Proxy Client has disconnected.
             * MultiProgram will accept connections again on Proxy Client disconnect action.
             */

            try {
                Socket accepted_socket = socket.EndAccept(ar);
                if (accepted_socket != null) {
                    Logger.Log("MultiProgram", "Proxy Client has connected!", ConsoleColor.Green);

                    ProxyClient proxy = new ProxyClient(accepted_socket, ProxyDisconnected);
                    this.proxy = proxy;
                }
            }
            catch (Exception e) {
                Logger.Log("MultiProgram", e, ConsoleColor.Red);
            }
        }
    }
}