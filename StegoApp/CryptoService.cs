﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;


namespace StegoApp
{
    public static class CryptoService
    {


        const int SYMM_KEY_SIZE = 16; // in bytes
        const int IV_SIZE = 16;
        const string CA_DN = "CN=StegoAppCA, O=StegoAppCA, L=Banja Luka, S=RS, C=BA";
        static readonly HashAlgorithmName HASH_ALGORITHM = HashAlgorithmName.SHA256;
        static readonly RSASignaturePadding RSA_SIGNATURE_PADDING = RSASignaturePadding.Pkcs1;
        static readonly RSAEncryptionPadding RSA_ENCRYPTION_PADDING = RSAEncryptionPadding.OaepSHA1;

        static byte[] GenerateRandomBytes(int size)
        {

            byte[] key = new byte[size];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                rng.GetBytes(key);

            return key;

        }

        public static byte[] GenerateSymmetricKey()
        {

            return GenerateRandomBytes(SYMM_KEY_SIZE);

        }

        public static byte[] GenerateIV()
        {

            return GenerateRandomBytes(IV_SIZE);

        }

        // Search Personal store
        public static X509Certificate2 FindCertificate(User user)
        {

            X509Certificate2 userCert = null;

            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {

                store.Open(OpenFlags.ReadOnly);
                var certColl = store.Certificates.Find(X509FindType.FindBySerialNumber, user.CertSerial, false)
                    .Find(X509FindType.FindByIssuerDistinguishedName, CA_DN, false);

                if (certColl.Count == 1)
                    userCert = certColl[0];

            }

            return userCert;

        }

        // Using symmetric algorithm
        public static byte[] EncryptData(string data, byte[] key, byte[] iV)
        {

            using (Aes aes = Aes.Create())
            using (MemoryStream memStream = new MemoryStream())
            {

                using (var encryptor = aes.CreateEncryptor(key, iV))
                using (CryptoStream cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    streamWriter.Write(data);


                // Stream is flushed upon closing CrytpoStream to MemoryStream
                return memStream.ToArray();

            }


        }

        public static string DecryptData(byte[] data, byte[] key, byte[] iV)
        {

            using (Aes aes = Aes.Create())
            using (MemoryStream memStream = new MemoryStream(data))
            using (var decryptor = aes.CreateDecryptor(key, iV))
            using (CryptoStream cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read))
            using (StreamReader streamReader = new StreamReader(cryptoStream))
            {

                string decData = streamReader.ReadToEnd();
                return decData;

            }
        }

        // Creates a envelope
        public static byte[] EncryptSymmetricData(byte[] symmetricKey, byte[] iV, RSA rsa)
        {

            byte[] keyIVPair = new byte[symmetricKey.Length + iV.Length];
            symmetricKey.CopyTo(keyIVPair, 0);
            iV.CopyTo(keyIVPair, symmetricKey.Length);

            return rsa.Encrypt(keyIVPair, RSA_ENCRYPTION_PADDING);

        }

        public static (byte[], byte[]) DecryptSymmetricData(byte[] envelope, RSA rsa)
        {

            byte[] keyIVPair = rsa.Decrypt(envelope, RSA_ENCRYPTION_PADDING);
            byte[] symmetricKey = new byte[SYMM_KEY_SIZE];
            byte[] iV = new byte[IV_SIZE];

            Array.Copy(keyIVPair, 0, symmetricKey, 0, SYMM_KEY_SIZE);
            Array.Copy(keyIVPair, SYMM_KEY_SIZE, iV, 0, IV_SIZE);

            return (symmetricKey, iV);

        }

        public static byte[] SignData(byte[] data, RSA rsa)
        {

            return rsa.SignData(data, HASH_ALGORITHM, RSA_SIGNATURE_PADDING);

        }

        public static bool VerifyData(byte[] data, byte[] signature, RSA rsa)
        {

            return rsa.VerifyData(data, signature, HASH_ALGORITHM, RSA_SIGNATURE_PADDING);

        }

        // Also checks if there is RSA private key
        public static RSA ReadPemFile(string path)
        {

            var lines = File.ReadLines(path);

            if (lines.First() != "-----BEGIN RSA PRIVATE KEY-----"
                || lines.Last() != "-----END RSA PRIVATE KEY-----")
                throw new FileFormatException("File is not valid pem file or does not contain rsa private key");

            using (StreamReader streamReader = new StreamReader(path))
            {

                PemReader pemReader = new PemReader(streamReader);
                var keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
                var privKey = (RsaPrivateCrtKeyParameters)keyPair.Private;
                var rsaParam = new RSAParameters();

                rsaParam.Exponent = privKey.PublicExponent.ToByteArrayUnsigned();
                rsaParam.D = privKey.Exponent.ToByteArrayUnsigned();
                rsaParam.DP = privKey.DP.ToByteArrayUnsigned();
                rsaParam.DQ = privKey.DQ.ToByteArrayUnsigned();
                rsaParam.InverseQ = privKey.QInv.ToByteArrayUnsigned();
                rsaParam.P = privKey.P.ToByteArrayUnsigned();
                rsaParam.Q = privKey.Q.ToByteArrayUnsigned();
                rsaParam.Modulus = privKey.Modulus.ToByteArrayUnsigned();

                RSA rsa = RSA.Create();
                rsa.ImportParameters(rsaParam);

                return rsa;

            }

        }

    }
}