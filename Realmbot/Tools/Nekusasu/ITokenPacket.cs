namespace Networking {
    public interface ITokenPacket {
        int Token { get; set; }
        bool TokenResponse { get; set; }
    }
}