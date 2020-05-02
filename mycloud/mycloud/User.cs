// Decompiled with JetBrains decompiler
// Type: mycloud.User
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System.Collections.Specialized;
using System.Text;

namespace mycloud
{
  public class User
  {
    public NameValueCollection info = new NameValueCollection();
    public string user;
    public string passwd;

    public User(string user)
    {
      this.user = user;
    }

    public bool save(out string reason)
    {
      return Vuser.add(this.user, "", this.info, out reason, "");
    }

    public bool is_upgraded()
    {
      return clib.atol(this.get_safe("paid_date")) + 31536000L >= (long) clib.time();
    }

    public bool is_upgraded_soon()
    {
      return clib.atol(this.get_safe("paid_date")) + 28944000L >= (long) clib.time();
    }

    public bool isadmin()
    {
      if (this.info == null)
        return false;
      string str = this.info.Get("groups");
      return str != null && str.Contains("admin");
    }

    public long quota()
    {
      string stuff = this.info.Get(nameof (quota));
      if (stuff == null)
      {
        if (this.is_upgraded())
          stuff = Ini.getstring(En.upgrade_quota);
        if (stuff == null)
          return 0;
      }
      return clib.nice_atol(stuff);
    }

    public string groups()
    {
      return this.info.Get(nameof (groups));
    }

    public string get_email()
    {
      string safe = this.info.get_safe("email");
      return safe.Length == 0 ? this.user : safe;
    }

    public string get_safe(string x)
    {
      return this.info.get_safe(x);
    }

    public User(string user, string passwd, NameValueCollection info)
    {
      this.user = user;
      this.passwd = passwd;
      this.info = info;
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("u=" + clib.simple_encode(this.user) + "~p=" + clib.simple_encode(this.passwd) + "~");
      if (this.info != null)
      {
        foreach (string allKey in this.info.AllKeys)
          stringBuilder.Append(clib.simple_encode(allKey) + "=" + clib.simple_encode(this.info[allKey]) + "~");
      }
      return stringBuilder.ToString();
    }
  }
}
