using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Security.Cryptography;

namespace StegoApp
{
    class User
    {

        public string FullName { get; }
        public string Nickname { get; }
        public string CertSerial { get; }
        
        // Actually salt + hash encoded in base64
        public string PasswHash { get; }
        public string UnreadFile { get; }

        public static Dictionary<string, User> AllUsers { get; private set; } = null;

        const string XML_PATH = "users.xml";
        const string XML_SCHEMA = "users.xsd";
        const int SALT_SIZE = 32;
        const int HASH_SIZE = 20;
        const int HASH_ITERATIONS = 10000;

        User(string fullName, string nickname, string certSerial, string passwHash,string unreadFile)
        {

            FullName = fullName;
            Nickname = nickname;
            CertSerial = certSerial;
            PasswHash = passwHash;
            UnreadFile = unreadFile;
            
        }

        // Loads users from XML_PATH, performs validation against XML_SCHEMA
        public static void LoadUsers()
        {

            Dictionary<string, User> allUsers = new Dictionary<string, User>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(XML_PATH);

            ValidateXmlDoc(xmlDoc);
            var rootElem = xmlDoc.DocumentElement;

            foreach(XmlNode childNode in rootElem.ChildNodes)
            {

                var user = GetUserFromXmlNode(childNode);
                allUsers.Add(user.Nickname, user);

            }

            AllUsers = allUsers;
        }

        static User GetUserFromXmlNode(XmlNode xmlNode)
        {

            string fullName;
            string nickname;
            string certSerial;
            string passwHash;
            string unreadFile;

            fullName = xmlNode.ChildNodes[0].FirstChild.Value;
            nickname = xmlNode.ChildNodes[1].FirstChild.Value;
            certSerial = xmlNode.ChildNodes[2].FirstChild.Value;
            passwHash = xmlNode.ChildNodes[3].FirstChild.Value;
            unreadFile = xmlNode.ChildNodes[4].FirstChild.Value;

            return new User(fullName, nickname, certSerial, passwHash,unreadFile);
        }


        static void ValidateXmlDoc(XmlDocument xmlDoc)
        {

            XmlSchema schema = new XmlSchema();
            xmlDoc.Schemas = new XmlSchemaSet();
            xmlDoc.Schemas.Add(null, XML_SCHEMA);
            
            try
            {

                xmlDoc.Validate(null);

            }
            catch (XmlSchemaValidationException ex)
            {
                
                //Todo: Add code to indentify which element produced error
                throw new XmlSchemaValidationException($"Schema validation error: {ex.Message}");

            }


        }

        public enum AuthStatus {Succesful,InvalidUsername,InvalidPassword};

        public static AuthStatus AuthenticateUser(string username, string password, out User user)
        {

            //Todo: Implement using SecureString
            user = null;
            User retrievedUser;
            if(!AllUsers.TryGetValue(username, out retrievedUser))
                return AuthStatus.InvalidUsername;

            byte[] salt = RetrieveSalt(retrievedUser);
            string hash = HashPassword(password, salt);

            if (hash != retrievedUser.PasswHash)
                return AuthStatus.InvalidPassword;

            user = retrievedUser;

            return AuthStatus.Succesful;

        }

        static byte[] RetrieveSalt(User user)
        {

            byte[] saltWithHash = Convert.FromBase64String(user.PasswHash);

            byte[] salt = new byte[SALT_SIZE];
            Array.Copy(saltWithHash, 0, salt, 0, salt.Length);

            return salt;
        }

        //Concatenates hash and salt, returns that in Base64 encoding
        static string HashPassword(string password,byte[] salt)
        {

            byte[] passwBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltWithHash = new byte[SALT_SIZE + HASH_SIZE];
            byte[] hash;

            using (Rfc2898DeriveBytes deriveObject = new Rfc2898DeriveBytes(passwBytes, salt, HASH_ITERATIONS))
            {

               hash = deriveObject.GetBytes(HASH_SIZE);

            }

            salt.CopyTo(saltWithHash, 0);
            hash.CopyTo(saltWithHash, SALT_SIZE);

            return Convert.ToBase64String(saltWithHash);
        }


    }
}
