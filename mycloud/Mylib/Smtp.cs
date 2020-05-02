// Decompiled with JetBrains decompiler
// Type: Mylib.Smtp
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mylib
{
  internal class Smtp : Link
  {
    private string server = "localhost";
    private string user;
    private string pass;
    private int port;
    private string ssl;
    private Smtp.cb_log cblog;

    public void set_log(Smtp.cb_log x)
    {
      this.cblog = x;
    }

    public void set_server(string s, string u, string p, string xport, string xssl)
    {
      this.user = u;
      this.pass = p;
      this.server = s;
      this.port = stat.atoi(xport);
      if (this.port == 0)
        this.port = 25;
      this.ssl = xssl;
    }

    public bool do_auth(out string result)
    {
      string xin = string.Format("\0{0}\0{1}\0", (object) this.user, (object) this.pass);
      this.netprintf("AUTH PLAIN {0}\r\n", (object) stat.encode_base64(xin));
      stat.imsg("AUTH PLAIN {0}\r\n", (object) stat.encode_base64(xin));
      if (!this.response(out result, out int _))
      {
        stat.imsg("--> auth responseBAD (0) ", (object) result);
        return false;
      }
      stat.imsg("--> auth response (0) ", (object) result);
      return true;
    }

    public bool send_message(string from, string xto, string message, out string result)
    {
      string result1;
      string stage;
      bool flag = this.send_message(from, xto, message, out result1, out stage);
      result = stage + ": " + result1;
      return flag;
    }

    public string build_body(string from, string to, string subject, string body)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendFormat("From: {0}\r\n", (object) from);
      stringBuilder.AppendFormat("To: {0}\r\n", (object) to);
      stringBuilder.AppendFormat("Subject: {0}\r\n", (object) subject);
      stringBuilder.AppendFormat("\r\n");
      stringBuilder.AppendFormat("{0}\r\n", (object) body);
      return stringBuilder.ToString();
    }

    public bool send_message(
      string from,
      string xto,
      string message,
      out string result,
      out string stage)
    {
      string[] strArray1 = stat.split_emails(xto);
      stage = "Open";
      stat.imsg("Sending message {0} SSL MODE ({1})", (object) this.server, (object) this.ssl);
      if (this.port == 0)
        this.port = 25;
      if (this.cblog != null)
        this.cblog(string.Format("info: Open host {0} port {1}", (object) this.server, (object) this.port));
      string reason;
      if (!this.open(this.server, this.port, out reason))
      {
        result = "Open smtp port failed" + reason;
        return false;
      }
      stat.imsg("Open worked to server {0}", (object) this.server);
      if (this.ssl == "Implicit SSL" && !this.ssl_enable(this.server, false))
      {
        result = "Unable to start SSL";
        stage = "Implicitssl";
        return false;
      }
      stage = "OpenResponse";
      int code;
      if (!this.response(out result, out code))
        return false;
      stage = "Ehlo";
      this.netprintf("EHLO localhost\r\n");
      if (!this.response(out result, out code))
        return false;
      if (this.ssl == "Start SSL")
      {
        stat.imsg("SENDING: STARTTLS\r\n");
        stage = "starttls";
        this.netprintf("STARTTLS\r\n");
        if (this.response(out result, out code))
        {
          if (!this.ssl_enable(this.server, false))
          {
            result = "Unable to start vis stls command";
            return false;
          }
          stat.imsg("ssl_enable: call seemed to work");
        }
        else
          stat.imsg("Didn't get response we wanted to start ssl");
      }
      else
        stat.imsg("Not starting ssl as ssl is {0}", (object) this.ssl);
      if (this.user != null)
      {
        stage = "authent";
        if (!this.do_auth(out result))
          return false;
      }
      stage = "mailfrom";
      this.netprintf("MAIL FROM:<{0}>\r\n", (object) stat.email_only(from));
      stat.imsg("MAIL FROM:<{0}>\r\n", (object) stat.email_only(from));
      if (!this.response(out result, out code))
        return false;
      stage = "xsentstore";
      this.netprintf("XSENTSTORE \"Sent\"\r\n", (object) from);
      if (!this.response(out result, out code))
        stat.imsg("Ignoring bad xsentstore response {0}", (object) result);
      for (int index = 0; index < ((IEnumerable<string>) strArray1).Count<string>(); ++index)
      {
        if (strArray1[index].Length >= 2)
        {
          string str = stat.email_only(strArray1[index]);
          stat.imsg("RCPT TO:<{0}>\r\n", (object) str);
          this.netprintf("RCPT TO:<{0}>\r\n", (object) str);
          stage = "rcptto";
          if (!this.response(out result, out code))
            return false;
        }
      }
      stage = "data";
      stat.imsg("DATA");
      this.netprintf("DATA\r\n");
      if (!this.response(out result, out code))
        return false;
      string[] separator = new string[1]{ "\r\n" };
      string[] strArray2 = message.Split(separator, StringSplitOptions.None);
      if (this.cblog != null)
        this.cblog("info: Sending data...");
      for (int index = 0; index < ((IEnumerable<string>) strArray2).Count<string>(); ++index)
      {
        string str = strArray2[index];
        if (str.Length > 0 && str[0] == '.')
          str = "." + str;
        this.netprintf_silent("{0}\r\n", (object) str);
      }
      stage = "dataend";
      this.netprintf(".\r\n");
      if (!this.response(out result, out code))
        return false;
      this.netprintf("quit\r\n");
      this.response(out result, out code);
      this.netclose();
      return true;
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
      if (str.StartsWith("AUTH PLAIN"))
        str = "AUTH PLAIN (HIDDEN)";
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
