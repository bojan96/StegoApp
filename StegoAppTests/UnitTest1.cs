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
using System.Text;

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

            string inStr = "My Message";

            var encData = CryptoService.EncryptData(Encoding.UTF8.GetBytes(inStr), symmKey, iV);
            var decData = CryptoService.DecryptData(encData, symmKey, iV);
            string decStr = Encoding.UTF8.GetString(decData);

            Assert.AreEqual(inStr, decStr);

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

        [TestMethod()]
        public void TestSteganography()
        {

            var data = Enumerable.Repeat<byte>(0, 189302);
            Steganography.Embed("testInput.png", "testOutput.png", data.ToArray());
            var embededData = Steganography.Extract("testOutput.png");

            Assert.IsTrue(Enumerable.SequenceEqual(data, embededData));

        }

        [TestMethod()]
        public void TestUnreadList()
        {

            //Contains "Path"
            UnreadList unreadList = new UnreadList("Unread.txt");
            unreadList.Add("Path1", "Hash1");
            unreadList.Add("Path2", "Hash2");
            unreadList.Write("Unread1.txt");
            
            unreadList.Remove("Path1");
            unreadList.Remove("Path2");

            Assert.AreEqual("Path", unreadList.Messages.First().Key);

        }

        [TestMethod()]
        public void TestCertValidation()
        {

            User.LoadUsers();
            var mmUser = User.AllUsers["mm"];
            var jjUser = User.AllUsers["jj"];

            var mmCert = CryptoService.FindCertificate(mmUser);
            var jjCert = CryptoService.FindCertificate(jjUser);

            Assert.IsTrue(CryptoService.ValidateCertificate(mmCert));
            Assert.IsFalse(CryptoService.ValidateCertificate(jjCert));

        }

    }
}
