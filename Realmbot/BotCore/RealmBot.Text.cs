using System;
using System.Collections.Generic;
using BotCore.Networking;

namespace BotCore {
    public partial class RealmBot {
        public int NextText = 0;
        public Queue<PlayerTextPacket> TextsToSend = new Queue<PlayerTextPacket>();

        /// <summary>
        /// Sends a player text packet to the connected server.
        /// </summary>
        /// <param name="text">Text to send to the connected server.</param>
        public void SendText(string text) {
            TextsToSend.Enqueue(new PlayerTextPacket { Text = text });
        }

        /// <summary>
        /// Sends multiple player text packets to the connected servers.
        /// </summary>
        /// <param name="texts">Texts to send to the connected server.</param>
        public void SendTexts(string[] texts) {
            foreach (string text in texts)
                TextsToSend.Enqueue(new PlayerTextPacket { Text = text });
        }
    }
}