using System;
using System.Security.Cryptography;
using System.Text;
using Stream;

namespace Hidden {
    namespace Utils.NET.Crypto {
        public class RsaKeyPair {
            public string publicKey;
            public string privateKey;
        }

        public class Rsa {
            private readonly RSACryptoServiceProvider serviceProvider;

            public Rsa(string keyString) {
                serviceProvider = new RSACryptoServiceProvider();
                serviceProvider.ImportParameters(ParamsFromString(keyString));
            }

            public static RsaKeyPair GenerateKeyPair() {
                RsaKeyPair keyPair = new RsaKeyPair();
                using (RSA rsa = RSA.Create()) {
                    rsa.KeySize = 1024;
                    keyPair.privateKey = ParamsToString(rsa.ExportParameters(true));
                    keyPair.publicKey = ParamsToString(rsa.ExportParameters(false));
                }

                return keyPair;
            }

            private static string ParamsToString(RSAParameters key) {
                BitWriter w = new BitWriter();
                w.Write(key.P == null);

                w.Write(key.Modulus.Length);
                w.Write(key.Modulus);

                w.Write(key.Exponent.Length);
                w.Write(key.Exponent);

                if (key.P != null) // is private
                {
                    w.Write(key.D.Length);
                    w.Write(key.D);

                    w.Write(key.DP.Length);
                    w.Write(key.DP);

                    w.Write(key.DQ.Length);
                    w.Write(key.DQ);

                    w.Write(key.InverseQ.Length);
                    w.Write(key.InverseQ);

                    w.Write(key.P.Length);
                    w.Write(key.P);

                    w.Write(key.Q.Length);
                    w.Write(key.Q);
                }

                return Convert.ToBase64String(w.GetData().Data);
            }

            private static RSAParameters ParamsFromString(string s) {
                byte[] data = Convert.FromBase64String(s);
                BitReader r = new BitReader(data, data.Length);
                bool isPublic = r.ReadBool();

                RSAParameters param = new RSAParameters();
                param.Modulus = r.ReadBytes(r.ReadInt32());
                param.Exponent = r.ReadBytes(r.ReadInt32());

                if (!isPublic) {
                    param.D = r.ReadBytes(r.ReadInt32());
                    param.DP = r.ReadBytes(r.ReadInt32());
                    param.DQ = r.ReadBytes(r.ReadInt32());
                    param.InverseQ = r.ReadBytes(r.ReadInt32());
                    param.P = r.ReadBytes(r.ReadInt32());
                    param.Q = r.ReadBytes(r.ReadInt32());
                }

                return param;
            }

            public byte[] Decrypt(byte[] data) {
                return serviceProvider.Decrypt(data, false);
            }

            public byte[] Encrypt(byte[] data) {
                return serviceProvider.Encrypt(data, false);
            }

            public string Encrypt(string data) {
                return Encoding.UTF8.GetString(Encrypt(Encoding.UTF8.GetBytes(data)));
            }

            public string Decrypt(string data) {
                return Encoding.UTF8.GetString(Decrypt(Encoding.UTF8.GetBytes(data)));
            }
        }
    }

}