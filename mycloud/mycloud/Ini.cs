// Decompiled with JetBrains decompiler
// Type: mycloud.Ini
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mycloud
{
  public class Ini
  {
    public static Ini.IniEntry[] ilist = new Ini.IniEntry[30];
    private static string[] mydata = new string[30];
    private static string savepath;

    public static string url_browse()
    {
      return clib.make_url("http", Ini.host(), Ini.getint(En.web_port), "/");
    }

    public static string host()
    {
      string str = Ini.getstring(En.host);
      if (str == null || str.Length == 0)
        str = MyKey.get_host();
      return str;
    }

    public static bool load(string path)
    {
      Ini.savepath = path;
      try
      {
        TextReader textReader = (TextReader) new StreamReader(path);
        try
        {
          while (true)
          {
            string[] strArray;
            do
            {
              string str = textReader.ReadLine();
              if (str != null)
                strArray = str.Split("~\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
              else
                goto label_7;
            }
            while (((IEnumerable<string>) strArray).Count<string>() < 2);
            Ini.do_set(strArray[0], strArray[1]);
          }
        }
        catch (Exception ex)
        {
          clib.imsg("load failed {0}", (object) ex.ToString());
        }
label_7:
        textReader.Close();
      }
      catch (Exception ex)
      {
        clib.imsg("Could not open {0} {1} - Not serious, using defaults", (object) path, (object) ex.Message);
        return true;
      }
      return true;
    }

    public static int ini_find(string var)
    {
      foreach (Ini.IniEntry iniEntry in Ini.ilist)
      {
        if (iniEntry.name == var)
          return (int) iniEntry.idx;
      }
      return -1;
    }

    public static bool istrue(En e)
    {
      switch (Ini.mydata[(int) e])
      {
        case "":
          return false;
        case "true":
          return true;
        case "TRUE":
          return true;
        case null:
          return false;
        default:
          return false;
      }
    }

    public static void do_set(string var, string val)
    {
      int index = Ini.ini_find(var);
      if (index < 0)
        clib.imsg("do_set( {0} {1}  ) INVALID VARIABLE NOT FOUND", (object) var, (object) val);
      else
        Ini.mydata[index] = val;
    }

    public static void do_set(En e, string val)
    {
      Ini.mydata[(int) e] = val;
    }

    public static bool save()
    {
      TextWriter textWriter = (TextWriter) new StreamWriter(Ini.savepath);
      foreach (Ini.IniEntry iniEntry in Ini.ilist)
      {
        if (Ini.mydata[(int) iniEntry.idx] != null)
          textWriter.Write(iniEntry.name + "~" + Ini.mydata[(int) iniEntry.idx] + "~\r\n");
      }
      textWriter.Close();
      return true;
    }

    public static void addit(Ini.IniEntry e)
    {
      int idx = (int) e.idx;
      Ini.ilist[idx] = e;
    }

    public static string default_domain()
    {
      string str = Ini.getstring(En.default_domain);
      if (str.Length == 0)
        str = Ini.host();
      return str;
    }

    public static void init(string path)
    {
      Ini.addit(new Ini.IniEntry(En.comment, "comment", "Ports", "", Etype.COMMENT, ""));
      Ini.addit(new Ini.IniEntry(En.ftp_port, "ftp_port", "FTP port", "21", Etype.STRING, "typically 21"));
      Ini.addit(new Ini.IniEntry(En.web_port, "web_port", "Web HTTP port", "80", Etype.STRING, "typically 80"));
      Ini.addit(new Ini.IniEntry(En.web_port_ssl, "web_port_ssl", "Web HTTPS/SSL port", "443", Etype.STRING, "typically 443"));
      Ini.addit(new Ini.IniEntry(En.webdav_port, "webdav_port", "WEBDAV port", "1080", Etype.STRING, "default 1080"));
      Ini.addit(new Ini.IniEntry(En.webdav_port_ssl, "webdav_port_ssl", "WEBDAV HTTPS/SSL port", "1443", Etype.STRING, "default 1443"));
      Ini.addit(new Ini.IniEntry(En.noself_signup, "noself_signup", "No Self signup", "", Etype.BOOL, "Disables user self creation on login page"));
      Ini.addit(new Ini.IniEntry(En.comment2, "comment2", "Main settings", "", Etype.COMMENT, ""));
      Ini.addit(new Ini.IniEntry(En.host, "host", "Full DNS hostname for remote access", "", Etype.STRING, "e.g. ftpdav.yourdomain.com"));
      Ini.addit(new Ini.IniEntry(En.default_domain, "default_domain", "Default domain if not specified", "", Etype.STRING, "This defaults to the hostname"));
      Ini.addit(new Ini.IniEntry(En.quota_enable, "quota_enable", "Enable simple quota limits", "", Etype.BOOL, "only accounts for root alias/path"));
      Ini.addit(new Ini.IniEntry(En.quota_default, "quota_default", "Default quota", "", Etype.STRING, "e.g. 300mb or  3gig"));
      Ini.addit(new Ini.IniEntry(En.auth_imap, "auth_imap", "IMAP Host to authenticate users against", "", Etype.STRING, "optional, if set users can use their email password"));
      Ini.addit(new Ini.IniEntry(En.comment3, "comment3", "Obscure settings", "", Etype.COMMENT, ""));
      Ini.addit(new Ini.IniEntry(En.bind, "bind", "Bind to specific interface", "", Etype.STRING, "e.g. a local ip address 10.0.0.1"));
      Ini.addit(new Ini.IniEntry(En.user_groups, "user_groups", "Valid user groups", "", Etype.STRING, "default is admin,staff,anonymous"));
      Ini.addit(new Ini.IniEntry(En.ssl_password, "ssl_password", "Password for SSL pfx file", "", Etype.STRING, "Leave blank unless you specified a password when creating your pfx file"));
      Ini.addit(new Ini.IniEntry(En.comment4, "comment4", "Mail Server", "", Etype.COMMENT, ""));
      Ini.addit(new Ini.IniEntry(En.mail_server, "mail_server", "Mail Server", "localhost", Etype.STRING, "e.g. mail.your.domain, default is localhost"));
      Ini.addit(new Ini.IniEntry(En.mail_user, "mail_user", "Mail username", "", Etype.STRING, "username for smtp authentication (optional)"));
      Ini.addit(new Ini.IniEntry(En.mail_pass, "mail_pass", "Mail password", "", Etype.STRING, "password for smtp authentication (optional)"));
      Ini.addit(new Ini.IniEntry(En.mail_from, "mail_from", "Mail from", "", Etype.STRING, "e.g. admin@your.domain"));
      Ini.addit(new Ini.IniEntry(En.comment5, "comment5", "PayPal and upgrade options", "", Etype.COMMENT, ""));
      Ini.addit(new Ini.IniEntry(En.upgrade_price, "upgrade_price", "Upgrade Price", "", Etype.STRING, "Price in $ for users to upgrade their storage allocation (quota)"));
      Ini.addit(new Ini.IniEntry(En.upgrade_quota, "upgrade_quota", "Upgrade Quota", "", Etype.STRING, "Quota to apply to users who have paid"));
      Ini.addit(new Ini.IniEntry(En.paypal_account, "paypal_account", "PayPal Account", "", Etype.STRING, "Account for incoming payments from users"));
      Ini.addit(new Ini.IniEntry(En.paypal_token, "paypal_token", "PayPal PDT Identity Token", "", Etype.STRING, "Find this on PayPal Website, Payment Preferences/Payment Data Transfer section"));
      Ini.addit(new Ini.IniEntry(En.comment6, "comment6", "Debugging Logging", "", Etype.COMMENT, ""));
      Ini.addit(new Ini.IniEntry(En.debug_http, "debug_http", "Debug Http", "", Etype.BOOL, "Debug http"));
      Ini.addit(new Ini.IniEntry(En.admin_email, "admin_email", "Admin email and primary admin account", "admin", Etype.STRING, "Your email address this is also your admin username"));
      Ini.load(path);
    }

    public static string getstring(En idx)
    {
      int index = (int) idx;
      return Ini.mydata[index] == null || Ini.mydata[index] == "" ? Ini.ilist[index].dflt : Ini.mydata[index];
    }

    public static int getint(En idx)
    {
      return clib.atoi(Ini.getstring(idx));
    }

    public static List<string> valid_groups()
    {
      List<string> list = ((IEnumerable<string>) Ini.getstring(En.user_groups).Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)).ToList<string>();
      if (list.Count<string>() == 0)
      {
        list.Add("admin");
        list.Add("user");
        list.Add("anonymous");
      }
      return list;
    }

    public struct IniEntry
    {
      public En idx;
      public string name;
      public string help;
      public string dflt;
      public Etype etype;
      public string hint;

      public IniEntry(
        En _idx,
        string _name,
        string _help,
        string _dflt,
        Etype etype,
        string hint)
      {
        this.idx = _idx;
        this.name = _name;
        this.help = _help;
        this.etype = etype;
        this.dflt = _dflt;
        this.hint = hint;
      }
    }
  }
}
