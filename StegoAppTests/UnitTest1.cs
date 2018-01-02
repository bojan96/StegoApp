using StegoApp;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography;
using System.IO;
using System.Linq;

namespace StegoApp.Tests
{
    [TestClass()]
    public class UnitTest1
    {
        [TestMethod()]
        public void MessageMethodTest()
        {

            User.LoadUsers();
            User user = User.AllUsers["mm"];
            string message = "My message";

            string xmlStr = Message.MakeXml(user, "My message");
            (string recvMessage, User recvUser, DateTime dt) = Message.ParseXml(xmlStr);


            Assert.AreEqual(message, recvMessage);
            Assert.AreEqual(user, recvUser);

        }

        [TestMethod()]
        public void TestCertFindFound()
        {

            User.LoadUsers();
            User user = User.AllUsers["mm"];

            Assert.IsTrue(CryptoService.FindCertificate(user) != null);

        }


        [TestMethod()]
        public void TestSymmetricEncryption()
        {

            var symmKey = CryptoService.GenerateSymmetricKey();
            var iV = CryptoService.GenerateIV();

            string data = "My Message";

            var encData = CryptoService.EncryptData(data, symmKey, iV);
            var decData = CryptoService.DecryptData(encData, symmKey, iV);

            Assert.AreEqual(data, decData);

        }

        [TestMethod()]
        public void TestPemReader()
        {

            var rsaObject = CryptoService.ReadPemFile("MM.pem");

        }

        [TestMethod()]
        public void TestDigitalEnvelope()
        {

            User.LoadUsers();
            User user = User.AllUsers["mm"];

            var symmKey = CryptoService.GenerateSymmetricKey();
            var iV = CryptoService.GenerateIV();

            var cert = CryptoService.FindCertificate(user);
            var encData = CryptoService.EncryptSymmetricData(symmKey, iV, cert.GetRSAPublicKey());

            var rsaObject = CryptoService.ReadPemFile("MM.pem");
            var decData = CryptoService.DecryptSymmetricData(encData, rsaObject);

            Assert.IsTrue(Enumerable.SequenceEqual(symmKey, decData.Item1));
            Assert.IsTrue(Enumerable.SequenceEqual(iV, decData.Item2));

        }

        [TestMethod()]
        public void TestDigitalSignature()
        {

            User.LoadUsers();
            User user = User.AllUsers["mm"];

            var cert = CryptoService.FindCertificate(user);
            var rsaObject = CryptoService.ReadPemFile("MM.pem");

            var data = new byte[] { 1, 2, 3, 4, 5, 6 };
            var signature = CryptoService.SignData(data, rsaObject);

            Assert.IsTrue(CryptoService.VerifyData(data, signature, cert.GetRSAPublicKey()));

        }

    }
}
