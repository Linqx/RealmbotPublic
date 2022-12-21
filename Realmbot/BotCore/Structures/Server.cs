using BotTools;

namespace BotCore.Structures {
    public enum Server : byte {
        [ParseableName("localhost")] [StringValue("127.0.0.1")]
        LocalHost,

        [ParseableName("use")] [StringValue("52.23.232.42")]
        USEast,

        [ParseableName("ase")] [StringValue("52.77.221.237")]
        AsiaSouthEast,

        [ParseableName("uss")] [StringValue("52.91.68.60")]
        USSouth,

        [ParseableName("ussw")] [StringValue("54.183.179.205")]
        USSouthWest,

        [ParseableName("use2")] [StringValue("52.91.203.118")]
        USEast2,

        [ParseableName("usnw")] [StringValue("54.234.151.78")]
        USNorthWest,

        [ParseableName("ae")] [StringValue("54.199.197.208")]
        AsiaEast,

        [ParseableName("eusw")] [StringValue("52.47.178.13")]
        EUSouthWest,

        [ParseableName("uss2")] [StringValue("54.183.236.213")]
        USSouth2,

        [ParseableName("eun2")] [StringValue("52.59.198.155")]
        EUNorth2,

        [ParseableName("eus")] [StringValue("52.47.150.186")]
        EUSouth,

        [ParseableName("uss3")] [StringValue("13.57.182.96")]
        USSouth3,

        [ParseableName("euw2")] [StringValue("34.243.37.98")]
        EUWest2,

        [ParseableName("usmw")] [StringValue("18.220.226.127")]
        USMidWest,

        [ParseableName("euw")] [StringValue("52.47.149.74")]
        EUWest,

        [ParseableName("use3")] [StringValue("54.157.6.58")]
        USEast3,

        [ParseableName("usw")] [StringValue("13.57.254.131")]
        USWest,

        [ParseableName("usw3")] [StringValue("54.67.119.179")]
        USWest3,

        [ParseableName("usmw2")] [StringValue("18.218.255.91")]
        USMidWest2,

        [ParseableName("eue")] [StringValue("18.195.167.79")]
        EUEast,

        [ParseableName("au")] [StringValue("54.252.165.65")]
        Australia,

        [ParseableName("eun")] [StringValue("54.93.78.148")]
        EUNorth,

        [ParseableName("usw2")] [StringValue("54.215.251.128")]
        USWest2
    }
}