// Decompiled with JetBrains decompiler
// Type: mycloud.UserDb
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace mycloud
{
  public class UserDb
  {
    private static string dbkey = "35679043986038958303345";
    public static string[] fields = new string[4]
    {
      "groups",
      "quota",
      "notify_upload",
      "notify_download"
    };
    private static int maxhash = 1000;
    private static Dictionary<int, string> uid_to_user = new Dictionary<int, string>();
    private static Dictionary<string, int> user_to_uid = new Dictionary<string, int>();
    private static string mylock = "";
    private static string root = "./userdb";
    private static Dictionary<string, User> cache = new Dictionary<string, User>();
    private static string uidfile;

    private static string fname(string user)
    {
      return UserDb.fname(UserDb.hash(user));
    }

    private static string fname(int h)
    {
      return string.Format("{0}/u_{1}.dat", (object) UserDb.root, (object) h);
    }

    private static int hash(string user)
    {
      int num = user.GetHashCode() % UserDb.maxhash;
      if (num < 0)
        num = -num;
      return num;
    }

    public static void init(string path)
    {
      UserDb.root = path;
      UserDb.uidfile = UserDb.root + "/uiduser.dat";
      UserDb.uid_load();
    }

    public static bool delete(string user)
    {
      bool flag = false;
      user = user.ToLower();
      string str = UserDb.fname(user) + ".new";
      lock (UserDb.mylock)
      {
        StreamReader streamReader;
        StreamWriter streamWriter;
        try
        {
          streamReader = new StreamReader(UserDb.fname(user));
          streamWriter = new StreamWriter(str);
        }
        catch
        {
          return false;
        }
        while (true)
        {
          string line;
          User user1;
          do
          {
            line = streamReader.ReadLine();
            if (line != null)
              user1 = UserDb.read_line(line);
            else
              goto label_9;
          }
          while (user1 == null);
          if (user1.user == user)
            flag = true;
          else
            streamWriter.WriteLine(line);
        }
label_9:
        streamReader.Close();
        streamWriter.Close();
        File.Delete(UserDb.fname(user));
        File.Move(str, UserDb.fname(user));
      }
      return flag;
    }

    public static string uid_find_user(int uid)
    {
      string str;
      return UserDb.uid_to_user.TryGetValue(uid, out str) ? str : (string) null;
    }

    public static int uid_get(string user)
    {
      int uid;
      if (!UserDb.user_to_uid.TryGetValue(user, out uid))
      {
        uid = new Random().Next(1000000000);
        UserDb.uid_add(user, uid, true);
      }
      return uid;
    }

    public static void uid_add(string user, int uid, bool todisk)
    {
      try
      {
        UserDb.uid_to_user.Add(uid, user);
        UserDb.user_to_uid.Add(user, uid);
        if (!todisk)
          return;
        File.AppendAllText(UserDb.uidfile, string.Format("{0} {1}\n", (object) uid, (object) user));
      }
      catch (Exception ex)
      {
        clib.imsg("uid_add: failed {0}", (object) ex.Message);
      }
    }

    public static void uid_load()
    {
      int num = 0;
      clib.imsg("uid_load: {0}", (object) UserDb.uidfile);
      try
      {
        StreamReader streamReader = new StreamReader(UserDb.uidfile);
        string x;
        while ((x = streamReader.ReadLine()) != null)
        {
          string[] strArray = x.string_words();
          if (((IEnumerable<string>) strArray).Count<string>() >= 2)
          {
            UserDb.uid_add(strArray[1], clib.atoi(strArray[0]), false);
            ++num;
          }
        }
      }
      catch (Exception ex)
      {
        clib.imsg("read uidfile failed {0}", (object) ex.Message);
      }
      clib.imsg("uid_load: read {0} records", (object) num);
    }

    public static string digest_passwd(string user, string passwd)
    {
      string sessionkey;
      AuthDigest.digest_ha1(user, MyMain.realm(), passwd, out sessionkey);
      return "{dig}" + sessionkey;
    }

    public static string encode_passwd(string user, string passwd)
    {
      byte[] numArray = new byte[4];
      new Random().NextBytes(numArray);
      string hex = clib.byte_to_hex(numArray, 4);
      return "{enc}" + hex + EncDec.Encrypt(passwd, hex + UserDb.dbkey);
    }

    public static string decode_passwd(string user, string encoded)
    {
      if (!encoded.StartsWith("{enc}"))
        return encoded;
      string str = encoded.Substring(5, 8);
      return EncDec.Decrypt(encoded.Substring(13), str + UserDb.dbkey);
    }

    public static bool add(
      string user,
      string passwd,
      NameValueCollection info,
      string oldpassword,
      out string reason)
    {
      user = user.ToLower();
      bool flag = false;
      reason = "none";
      UserDb.uid_get(user);
      if (passwd.Length == 0)
        flag = true;
      passwd = !flag ? UserDb.encode_passwd(user, passwd) : oldpassword;
      lock (UserDb.mylock)
      {
        StreamWriter streamWriter;
        try
        {
          streamWriter = new StreamWriter(UserDb.fname(user), true);
        }
        catch (Exception ex)
        {
          clib.imsg("userdb: add write {0}", (object) ex.ToString());
          Directory.CreateDirectory(UserDb.root);
          try
          {
            streamWriter = new StreamWriter(UserDb.fname(user));
          }
          catch
          {
            reason = "Write to file failed " + ex.Message;
            clib.imsg("userdb: add write {0}", (object) ex.ToString());
            return false;
          }
        }
        User user1 = new User(user, passwd, info);
        streamWriter.WriteLine(user1.ToString());
        streamWriter.Close();
        return true;
      }
    }

    public static void search(string user, UserDb.search_cb bob, object obj)
    {
      int h = 0;
      while (h < UserDb.maxhash && UserDb.search_one(UserDb.fname(h), user, bob, obj))
        ++h;
    }

    private static User read_line(string line)
    {
      string[] strArray1 = line.Split("~\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
      ((IEnumerable<string>) strArray1).Count<string>();
      string[] strArray2 = strArray1[0].Split("=".ToCharArray());
      if (((IEnumerable<string>) strArray2).Count<string>() < 2)
        return (User) null;
      string user1 = clib.simple_decode(strArray2[1]);
      if (user1 == null)
        return (User) null;
      User user2 = new User(user1);
      foreach (string str in strArray1)
      {
        string[] strArray3 = str.Split("=".ToCharArray());
        if (((IEnumerable<string>) strArray3).Count<string>() >= 2 && !(strArray3[0] == "u"))
        {
          if (strArray3[0] == "p")
            user2.passwd = clib.simple_decode(strArray3[1]);
          else
            user2.info.Add(strArray3[0], clib.simple_decode(strArray3[1]));
        }
      }
      return user2;
    }

    public static bool search_one(string fn, string user, UserDb.search_cb bob, object obj)
    {
      StreamReader streamReader;
      try
      {
        streamReader = new StreamReader(fn);
      }
      catch
      {
        return true;
      }
      clib.imsg("search_one: reading file {0} and search for [{0}]", (object) fn, (object) user);
      User info;
      do
      {
        do
        {
          string line = streamReader.ReadLine();
          if (line != null)
            info = UserDb.read_line(line);
          else
            goto label_8;
        }
        while (info == null || user != "" && !info.user.Contains(user));
        clib.imsg("showing user: {0}", (object) info.user);
      }
      while (bob(obj, info));
      return false;
label_8:
      streamReader.Close();
      return true;
    }

    public static User lookup(string user)
    {
      User user1 = (User) null;
      user = user.ToLower();
      StreamReader streamReader;
      try
      {
        streamReader = new StreamReader(UserDb.fname(user));
      }
      catch
      {
        return (User) null;
      }
      while (true)
      {
        User user2;
        do
        {
          string line = streamReader.ReadLine();
          if (line != null)
            user2 = UserDb.read_line(line);
          else
            goto label_6;
        }
        while (user2 == null || user2.user != user);
        user1 = user2;
      }
label_6:
      streamReader.Close();
      return user1;
    }

    public static bool check(string user, string passwd)
    {
      user = user.ToLower();
      User user1 = UserDb.lookup(user);
      return user1 != null && (user1.passwd == passwd || user == "anonymous" || (SimpleHash.test_hash(passwd, user1.passwd) || UserDb.digest_passwd(user, passwd) == user1.passwd) || UserDb.decode_passwd(user, user1.passwd) == passwd);
    }

    private static bool rewrite(int hash)
    {
      return true;
    }

    public static bool unit_test()
    {
      NameValueCollection nameValueCollection = new NameValueCollection()
      {
        {
          "Full Name",
          "Bob Jones"
        },
        {
          "Created",
          "23 January 1966"
        }
      };
      User user = UserDb.lookup("bob3@here.com");
      if (user != null)
        clib.imsg("lookup found user {0}", (object) user.ToString());
      return true;
    }

    public delegate bool search_cb(object obj, User info);
  }
}
