namespace MultiBot.Structures {
    public struct HealRequest {
        public HealRequests Request;
        public bool Purification;

        public HealRequest(HealRequests request, bool purification) {
            Request = request;
            Purification = purification;
        }
    }

    public enum HealRequests {
        I_NEED_HEAL = 0,
        WHO_CAN_HEAL = 1,
        I_CAN_HEAL = 3,
        I_CANT_HEAL = 4,
        YOU_SHALL_HEAL = 5
    }
}