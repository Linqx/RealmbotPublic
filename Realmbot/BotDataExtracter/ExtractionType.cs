using System;

namespace BotDataExtracter {
    [Flags]
    public enum ExtractionType {
        OBJECTS = 1 << 1,
        TILES = 1 << 2,
        PACKETS = 1 << 3
    }
}