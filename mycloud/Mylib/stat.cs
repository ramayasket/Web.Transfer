// Decompiled with JetBrains decompiler
// Type: Mylib.stat
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mylib
{
  internal class stat
  {
    public static string[] split_quoted(string commandLine)
    {
      char[] charArray = commandLine.ToCharArray();
      bool flag = false;
      for (int index = 0; index < charArray.Length; ++index)
      {
        if (charArray[index] == '"')
          flag = !flag;
        if (!flag && charArray[index] == ' ')
          charArray[index] = '\n';
      }
      return new string(charArray).Split('\n');
    }

    public static string url_encode(string x)
    {
      return clib.url_encode(x);
    }

    public static string trim_quotes(string x)
    {
      return x.Length < 2 || x[0] != '"' ? x : x.Substring(1, x.Length - 3);
    }

    public static string wash(string x, string toremove)
    {
      char[] charArray = x.ToCharArray();
      for (int index = 0; index < charArray.Length; ++index)
      {
        if (toremove.IndexOf(charArray[index]) >= 0)
          charArray[index] = ' ';
      }
      return new string(charArray);
    }

    public static string trim_eol(string x)
    {
      return stat.wash(x, "\r\n\t");
    }

    public static void imsg(string format, params object[] args)
    {
      clib.imsg("{0}", (object) string.Format(format, args));
    }

    public static void emsg(string format, params object[] args)
    {
      clib.imsg("{0}", (object) string.Format(format, args));
    }

    public static string app(string fname)
    {
      return "./" + fname;
    }

    public static int atoi(string x)
    {
      return clib.atoi(x);
    }

    public static int time()
    {
      return Convert.ToInt32(new TimeSpan(DateTime.Now.ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks).TotalSeconds);
    }

    public static string encode_base64(string xin)
    {
      int num = 0;
      byte[] inArray = new byte[xin.Length];
      foreach (char ch in xin)
        inArray[num++] = (byte) ch;
      return Convert.ToBase64String(inArray);
    }

    public static string email_only(string email)
    {
      string[] strArray = email.Split(", <>\"".ToCharArray());
      for (int index = 0; index < ((IEnumerable<string>) strArray).Count<string>(); ++index)
      {
        string str = strArray[index];
        if (str.IndexOf("@") >= 0)
          return str.Trim();
      }
      return email.Trim();
    }

    public static string[] split_emails(string xto)
    {
      List<string> source = new List<string>();
      string[] strArray = xto.Split(", <>\"".ToCharArray());
      for (int index = 0; index < ((IEnumerable<string>) strArray).Count<string>(); ++index)
      {
        if (strArray[index].Contains("@") && !source.Contains<string>(strArray[index], (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))
          source.Add(strArray[index]);
      }
      return source.ToArray();
    }
  }
}
