using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Hidden {
    public class SCry {
        public static readonly string serverPublicKey =
            "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDCKFctVrhfF3m2Kes0FBL/JFeOcmNg9eJz8k/hQy1kadD+XFUpluRqa//Uxp2s9W2qE0EoUCu59ugcf/p7lGuL99UoSGmQEynkBvZct+/M40L0E0rZ4BVgzLOJmIbXMp0J4PnPcb6VLZvxazGcmSfjauC7F3yWYqUbZd/HCBtawwIDAQAB\n";

        public static readonly byte[] serverPublicKeyBytes = Convert.FromBase64String(serverPublicKey);
        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();

        private static string HexString(byte[] buf) {
            char[] hexArray = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};
            char[] hexChars = new char[buf.Length * 2];
            uint dummy;
            for (int j = 0; j < buf.Length; j++) {
                dummy = (uint) buf[j] & 0xFF;
                hexChars[j * 2] = hexArray[dummy >> 4];
                hexChars[j * 2 + 1] = hexArray[dummy & 0x0F];
            }

            return new string(hexChars);
        }

        public static string Encrypt(string s) {
            byte[] buffer = Encoding.UTF8.GetBytes(s);
            try {
                AsymmetricKeyParameter akp = PublicKeyFactory.CreateKey(Convert.FromBase64String(serverPublicKey));
                RsaKeyParameters rkp = (RsaKeyParameters) akp;
                RSAParameters rp = new RSAParameters {
                    Modulus = rkp.Modulus.ToByteArrayUnsigned(),
                    Exponent = rkp.Exponent.ToByteArrayUnsigned()
                };
                RSACryptoServiceProvider rcsp = new RSACryptoServiceProvider();
                rcsp.ImportParameters(rp);

                byte[] plaintext = buffer;
                byte[] ciphertext = rcsp.Encrypt(plaintext, false);
                string cipherresult = Convert.ToBase64String(ciphertext);

                return Convert.ToBase64String(rcsp.Encrypt(Encoding.UTF8.GetBytes(s), false));
            }
            catch (Exception e) {
                Console.WriteLine("RSA Exception: " + e.StackTrace);
            }

            return null;
        }
    }
}