using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace Hidden {
    public class RC4 {
        private readonly RC4Engine _engine;

        public RC4(byte[] key) {
            _engine = new RC4Engine();
            _engine.Init(true, new KeyParameter(key));
        }

        public void Crypt(byte[] buffer, int offset, int length) {
            _engine.ProcessBytes(buffer, offset, length, buffer, offset);
        }
    }
}