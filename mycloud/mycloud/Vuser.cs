// Decompiled with JetBrains decompiler
// Type: mycloud.Vuser
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using Mylib;
using System;
using System.Collections.Specialized;
using System.IO;

namespace mycloud
{
  internal class Vuser
  {
    private static int total;

    public static string[] fields
    {
      get
      {
        return UserDb.fields;
      }
    }

    public static void search(string user, UserDb.search_cb cb, object obj)
    {
      UserDb.search(user, cb, obj);
    }

    public static void init(string root)
    {
      UserDb.init(root);
      try
      {
        Vuser.total = clib.atoi(File.ReadAllText(clib.work("user_count.dat")));
      }
      catch (Exception ex)
      {
        clib.imsg("cannot read user_count.dat {0}", (object) ex.Message);
      }
    }

    public static string uid_find_user(int uid)
    {
      return UserDb.uid_find_user(uid);
    }

    public static int uid_get(string user)
    {
      return UserDb.uid_get(user);
    }

    public static User lookup(string user)
    {
      return UserDb.lookup(user);
    }

    public static string add_domain(string user)
    {
      return user.IndexOf("@") < 0 ? user + "@" + Ini.default_domain() : user;
    }

    public static bool add(
      string user,
      string passwd,
      NameValueCollection info,
      out string reason,
      string oldpass)
    {
      if (Vuser.total > 5 && Vuser.total > MyKey.ulimit_max())
      {
        reason = "Sorry license user limit reached";
        return false;
      }
      if (!Vuser.valid_user(user, out reason))
        return false;
      User user1 = UserDb.lookup(user);
      bool flag = UserDb.add(user, passwd, info, user1 == null ? "" : user1.passwd, out reason);
      if (flag && user1 == null)
        Vuser.total_add(1);
      return flag;
    }

    public static void total_add(int x)
    {
      Vuser.total += x;
      Vuser.total_write();
    }

    public static void total_write()
    {
      File.WriteAllText(clib.work("user_count.dat"), string.Format("{0}\n", (object) Vuser.total));
    }

    public static int user_count()
    {
      return Vuser.total;
    }

    public static bool check_basic_digest(
      string method_name,
      string hdr,
      out string auth_user,
      out string reason)
    {
      auth_user = "";
      reason = "";
      if (hdr == null || hdr.Length == 0)
        return false;
      if (hdr.StartsWith("digest ", StringComparison.OrdinalIgnoreCase))
      {
        auth_user = hdr.get_param("username");
        string user = Vuser.add_domain(auth_user);
        string pass = Vuser.get_pass(user);
        if (pass.Length == 0)
        {
          reason = "No account or no password set";
          return false;
        }
        string sessionkey;
        AuthDigest.digest_ha1(hdr.get_param("username"), MyMain.realm(), pass, out sessionkey);
        string response;
        AuthDigest.digest_response(sessionkey, hdr.get_param("nonce"), hdr.get_param("nc"), hdr.get_param("cnonce"), hdr.get_param("qop"), method_name, hdr.get_param("uri"), "", out response);
        if (response == hdr.get_param("response"))
        {
          auth_user = user;
          return true;
        }
        clib.imsg("Authorization failed {0} {1} {2}", (object) hdr.get_param("username"), (object) response, (object) hdr.get_param("response"));
        reason = "digest didn't match";
        return false;
      }
      int num = hdr.IndexOf("basic ", StringComparison.OrdinalIgnoreCase);
      if (num >= 0)
      {
        string str = clib.decode_base64(hdr.Substring(num + 6));
        int length = str.IndexOf(":");
        if (length < 0)
          return false;
        auth_user = str.Substring(0, length);
        string pass = str.Substring(length + 1);
        auth_user = Vuser.add_domain(auth_user);
        return Vuser.check(auth_user, pass, out reason);
      }
      clib.imsg("NO AUTHENTICATION HEADER");
      return false;
    }

    public static string get_pass(string user)
    {
      User user1 = UserDb.lookup(user);
      return user1 == null ? "" : UserDb.decode_passwd(user, user1.passwd);
    }

    public static bool check(string user, string pass, out string reason)
    {
      bool flag1 = false;
      reason = "user doesn't exist or invalid password";
      bool flag2 = UserDb.check(user, pass);
      if (flag2)
      {
        User user1 = UserDb.lookup(user);
        flag1 = user1.isadmin();
        switch (user1.info["status"])
        {
          case "pending":
            reason = "Sorry you must activate using the token from your email first";
            return false;
          default:
            string stuff = user1.info["cached"];
            if (stuff != null && stuff.Length > 0)
            {
              clib.imsg("found cached life of {0}", (object) stuff);
              if (clib.time() > clib.atoi(stuff))
              {
                clib.imsg("EXPIRED, MAKE HIM CHECK AGAIN age {0} {1}", (object) (clib.time() - clib.atoi(stuff)), (object) stuff);
                flag2 = false;
              }
            }
            break;
        }
      }
      if (!flag1 && !Vuser.valid_user(user, out reason))
        return false;
      if (!flag2)
      {
        string dest = Ini.getstring(En.auth_imap);
        if (dest.Length > 0)
        {
          Imap imap = new Imap();
          clib.imsg("auth_imap {0} {1}", (object) user, (object) dest);
          string result;
          if (imap.login(dest, 143, "nossl", user, pass, out result))
          {
            NameValueCollection info = new NameValueCollection();
            User user1 = UserDb.lookup(user);
            if (user1 != null && user1.info != null)
              info = user1.info;
            info.Set("cached", clib.int_to_string(clib.time() + 604800));
            clib.imsg("Imap: login worked for that user/pass {0}", (object) user);
            string reason1;
            if (!UserDb.add(user, pass, info, "", out reason1))
              clib.imsg("cacheadd: {0}", (object) reason1);
            imap.netclose();
            flag2 = true;
          }
          else
            clib.imsg("imap: login failed on remost host {0}", (object) result);
        }
      }
      return flag2;
    }

    public static bool valid_user(string user, out string reason)
    {
      bool flag = true;
      reason = "Invalid user or password";
      if (!user.Contains("@"))
      {
        reason = "username must be a valid email address, e.g. user@domain.name";
        flag = false;
      }
      if (user.IndexOfAny(",?:;'\"!~`=)(*&^%$#<>/{}[]|".ToCharArray()) >= 0)
      {
        reason = "username cannot contain punctuation sorry";
        flag = false;
      }
      return flag;
    }

    public static bool delete(string user)
    {
      bool flag = UserDb.delete(user);
      if (flag)
        Vuser.total_add(-1);
      return flag;
    }
  }
}
