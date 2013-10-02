using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Samba.Infrastructure.Settings;

namespace Samba.ApiServer.Lib
{
    public class Token
    {
        private static string TOKEN_PATH = string.Format(@"{0}\{1}",
                                                         LocalSettings.DataPath,
                                                         "token");

        private const string TOKEN_FILENAME_PATTERN = @"{0}\{1}.xml";

        //private static TimeSpan MAX_TIME_TOKEN = LocalSettings.TokenMax;
        public Token()
        {
        }

        public Token(int userId)
        {
            UserId = userId;
            Guid = CreateToken();
            LastUsed = DateTime.Now;
            SaveToken();
        }

        static Token()
        {
            if (!Directory.Exists(TOKEN_PATH))
            {
                Directory.CreateDirectory(TOKEN_PATH);
            }
        }

        public DateTime LastUsed { get; set; }
        public Guid Guid { get; set; }
        public int UserId { get; set; }

        public static void ApplicationExit(object sender, EventArgs e)
        {
            foreach (var item in Directory.EnumerateFiles(TOKEN_PATH))
            {
                File.Delete(item);
            }
        }

        public static void CollectGarbage(object sender, EventArgs e)
        {
            foreach (var item in GetAllTokens().Where(x => DateTime.Now - x.LastUsed > LocalSettings.TokenLifeTime))
            {
                item.Delete();
            }
        }

        public static IEnumerable<Token> GetAllTokens()
        {
            return Directory.EnumerateFiles(TOKEN_PATH).Select(SimpleSerializer.DeserializeFromXmlFile<Token>).ToList();
        }

        public static bool ValidateToken(string tokenString)
        {
            var t = GetToken(tokenString);
            if (t != null)
            {
                t.LastUsed = DateTime.Now;
                return true;
            }
            return false;
        }

        public static Token GetToken(string tokenString)
        {
            Token returnToken;
            try
            {
                returnToken = SimpleSerializer.DeserializeFromXmlFile<Token>(GetFilename(tokenString));
            }
            catch (Exception)
            {
                returnToken = null;
            }
            return returnToken;
        }

        public void Delete()
        {
            if (File.Exists(GetFilename(Guid)))
            {
                File.Delete(GetFilename(Guid));
            }
        }

        public override string ToString()
        {
            return Guid.ToString();
        }

        private static string GetFilename(Guid guid)
        {
            return GetFilename(guid.ToString());
        }

        private static string GetFilename(string guid)
        {
            return string.Format(string.Format(TOKEN_FILENAME_PATTERN, TOKEN_PATH, guid));
        }

        private static Guid CreateToken()
        {
            Guid newGuid;
            do
            {
                newGuid = Guid.NewGuid();
            }
            while (File.Exists(GetFilename(newGuid)));
            return newGuid;
        }

        private void SaveToken()
        {
            SimpleSerializer.SerializeToXmlFile(GetFilename(Guid), this);
        }
    }
}
