// Decompiled with JetBrains decompiler
// Type: Mylib.Ftplink
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.Collections.Generic;
using System.Linq;

namespace Mylib
{
  internal class Ftplink : Link
  {
    private Ftplink.cb_log cblog;

    public void set_log(Ftplink.cb_log x)
    {
      this.cblog = x;
    }

    public void netprintf_silent(string format, params object[] args)
    {
      this.send(string.Format("{0}", (object) string.Format(format, args)));
    }

    // N.B.! 'new' keyword was added after decompilation
    public new void netprintf(string format, params object[] args)
    {
      string str = string.Format(format, args);
      this.send(string.Format("{0}", (object) str));
      if (this.cblog == null)
        return;
      this.cblog(string.Format("--> {0}", (object) str));
    }

    public bool response(out string result, out int code)
    {
      code = 599;
      bool istimeout;
      do
      {
        if (this.readline(out result, "Response", 90000, out istimeout, out string _))
        {
          if (this.cblog != null)
            this.cblog(string.Format("<-- {0}", (object) result));
          stat.imsg("smtp: response {0}", (object) result);
        }
        else
          goto label_1;
      }
      while (result.Length >= 4 && !(result.Substring(3, 1) != "-"));
      goto label_10;
label_1:
      result = "550 No response from smtp server - connection closed maybe";
      if (istimeout)
        result = "550 No response from smtp server - timeout";
      if (this.cblog != null)
        this.cblog(string.Format("failed: {0}", (object) result));
      return false;
label_10:
      string[] strArray = result.Split(" -\r\n".ToCharArray());
      if (((IEnumerable<string>) strArray).Count<string>() < 1)
        return false;
      code = Convert.ToInt32(strArray[0]);
      return code <= 499;
    }

    public delegate void cb_log(string info);
  }
}
