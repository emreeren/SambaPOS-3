using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Settings;
using Samba.Persistance.Data;

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

    public Token(User user)
      : this(user.Id)
    {
    }

    static Token()
    {
      if (!Directory.Exists(TOKEN_PATH))
      {
        Directory.CreateDirectory(TOKEN_PATH);
      }
    }

    public DateTime LastUsed
    {
      get;
      set;
    }

    public Guid Guid
    {
      get;
      set;
    }

    public int UserId
    {
      get;
      set;
    }
    
    public static void ApplicationExit(object sender, EventArgs e)
    {
      foreach (string item in Directory.EnumerateFiles(TOKEN_PATH))
      {
        File.Delete(item);
      }
    }

    public static void CollectGarbage(object sender, EventArgs e)
    {
      IEnumerable<Token> tokens = from token in GetAllToken()
                                  where DateTime.Now - token.LastUsed >
                                    LocalSettings.TokenLifeTime
                                  select token;
      foreach (Token item in tokens)
      {
        item.Delete();
      }
    }
  
    public static IEnumerable<Token> GetAllToken()
    {
      List<Token> ret = new List<Token>();

      foreach (string item in Directory.EnumerateFiles(TOKEN_PATH))
      {
        ret.Add(SimpleSerializer.DeserializeFromXmlFile<Token>(item));
      }
      return ret;
    }

    public static bool ValidateToken(string tokenString)
    {
      Token t = GetToken(tokenString);
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

    public User GetUserFromToken()
    {
      IWorkspace workspace = WorkspaceFactory.Create();

      User u = (from user in Dao.Query<User>()
                where user.Id == UserId
                select user).First();

      return u;
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

    private Guid CreateToken()
    {
      Guid newGuid ;
      do
      {
        newGuid = Guid.NewGuid();
      }
      while (File.Exists(GetFilename(newGuid)));
      return newGuid;
    }

    private void SaveToken()
    {
      SimpleSerializer.SerializeToXmlFile<Token>(GetFilename(Guid), this);
    }
  }
}
