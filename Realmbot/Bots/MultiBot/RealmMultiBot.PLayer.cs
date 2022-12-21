using System;
using MultiBot.Structures;

namespace MultiBot {
    public partial class RealmMultiBot {
        public double HpLog;
        public int ClientHp = 100;
        public int Hp2;
        public int HealBuffer;
        public int HealBufferTime;
        public int AutoNexusNumber;
        public int RequestHealNumber;
        public int AutoHpPotNumber;
        public int AutoHealNumber;
        public int LastHpPotTime;
        public int LastHealRequest;
        public int LastAutoAbilityAttempt;

        public Action<HealRequest> SendHealToProgram = null;

        public void ResetAutoNexus() {
            HpLog = 0;
            ClientHp = 100;
            Hp2 = 0;
            HealBuffer = 0;
            HealBufferTime = 0;
            AutoNexusNumber = 0;
            RequestHealNumber = 0;
            AutoHpPotNumber = 0;
            AutoHealNumber = 0;
            LastHpPotTime = 0;
            LastHealRequest = 0;
            LastAutoAbilityAttempt = 0;
        }

        public void SendHeal(HealRequests request, bool purification) {
            SendHealToProgram?.Invoke(new HealRequest(request, purification));
        }

        public bool SubtractDamage(int amount, int time = -1) {
            if (time == -1)
                time = World.CurrentUpdate;

            ClientHp -= amount;
            Hp2 -= amount;
            return CheckHealth(time);
        }

        public bool CheckHealth(int time) {
            if (!Map.IsSafeMap) {
                if (Player.Health <= AutoNexusNumber || ClientHp <= AutoNexusNumber || Hp2 <= AutoNexusNumber) {
                    World.Bot.Reconnect(ConnectionMode.VAULT);
                    return true;
                }

                if (!FameBlockSettings.Thirsty && !Player.IsSick && AutoHpPotNumber != 0 &&
                    (Player.Health <= AutoHpPotNumber || ClientHp <= AutoHpPotNumber || Hp2 <= AutoHpPotNumber)) {
                    if (time - LastHpPotTime > AutoSettings.AutoHpPotDelay) LastHpPotTime = time;
                }

                //TODO:
                // Auto Hp Pot Consumption

                if (!Player.IsSick && Player.Health <= RequestHealNumber || ClientHp <= RequestHealNumber ||
                    Hp2 <= RequestHealNumber)
                    if (time - LastHealRequest > 450) {
                        LastHealRequest = time;
                        SendHeal(HealRequests.I_NEED_HEAL, false);
                    }
            }

            return false;
        }
    }
}