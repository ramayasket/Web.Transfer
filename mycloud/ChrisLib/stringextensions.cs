// Decompiled with JetBrains decompiler
// Type: ChrisLib.stringextensions
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.Collections.Specialized;
using System.IO;

namespace ChrisLib
{
  public static class stringextensions
  {
    public static string url_path_only(this string bog)
    {
      int length = bog.LastIndexOf("/");
      return bog.Substring(0, length);
    }

    public static string path_only(this string bog)
    {
      return Path.GetDirectoryName(bog);
    }

    public static string file_only(this string bog)
    {
      return clib.fileonly(bog);
    }

    public static string path_add(this string bog, string toadd)
    {
      string str = "/";
      if (toadd.Length == 0)
        return bog;
      if (toadd[0] == '/')
        str = "";
      return bog == "/" ? str + toadd : bog.trimdir() + str + toadd;
    }

    public static byte[] tobyte(this string bob)
    {
      int length = bob.Length;
      byte[] numArray = new byte[length];
      for (int index = 0; index < length; ++index)
        numArray[index] = (byte) bob[index];
      return numArray;
    }

    public static string addslash(this string x)
    {
      return x.EndsWith("/") ? x : x + "/";
    }

    public static string trimslash(this string x)
    {
      return x.TrimEnd("/".ToCharArray());
    }

    public static string trimdir(this string x)
    {
      return x.TrimEnd("/".ToCharArray()).TrimEnd("\\".ToCharArray());
    }

    public static string get_param(this string x, string xname)
    {
      bool flag = false;
      string str1 = " " + xname + "=\"";
      int num = x.IndexOf(str1, StringComparison.OrdinalIgnoreCase);
      if (num < 0)
      {
        str1 = "," + xname + "=\"";
        num = x.IndexOf(str1, StringComparison.OrdinalIgnoreCase);
      }
      if (num < 0)
      {
        str1 = " " + xname + "=";
        num = x.IndexOf(str1, StringComparison.OrdinalIgnoreCase);
        if (num < 0)
        {
          str1 = "," + xname + "=";
          num = x.IndexOf(str1, StringComparison.OrdinalIgnoreCase);
        }
        flag = true;
        if (num < 0)
          return "";
      }
      int startIndex = num + str1.Length;
      string str2 = x.Substring(startIndex);
      if (flag)
      {
        int length1 = str2.IndexOf(',');
        if (length1 >= 0)
          str2 = str2.Substring(0, length1);
        int length2 = str2.IndexOf(' ');
        if (length2 >= 0)
          str2 = str2.Substring(0, length2);
      }
      else
      {
        int length = str2.IndexOf('"');
        if (length >= 0)
          str2 = str2.Substring(0, length);
      }
      return str2;
    }

    public static string[] string_lines(this string x)
    {
      return x.Split(new string[1]{ "\r\n" }, StringSplitOptions.None);
    }

    public static string[] string_words(this string x)
    {
      return x.Split("\t \r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
    }

    public static string[] string_lines_any(this string x)
    {
      return x.Split(new string[3]{ "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
    }

    public static int end_number(this string s)
    {
      int num = 0;
      foreach (char c in s)
      {
        if (char.IsDigit(c) || c == '.')
          ++num;
        else
          break;
      }
      return num;
    }

    public static string get_safe(this NameValueCollection all, string idx)
    {
      return all[idx] ?? "";
    }

    public static bool get_true(this NameValueCollection all, string idx)
    {
      return (all[idx] ?? "").ToLower() == "true";
    }

    public static string get_word(this string s, char endword, out string word)
    {
      int length = s.IndexOf(endword);
      if (length < 0)
      {
        word = s;
        return "";
      }
      word = s.Substring(0, length);
      return s.Substring(length + 1);
    }

    public static string ToHttpDate(this DateTime s)
    {
      return s.ToUniversalTime().ToString("r");
    }

    public static long to_unix_date(this DateTime now)
    {
      DateTime dateTime = new DateTime(1970, 1, 1);
      return (long) Convert.ToInt32(new TimeSpan(now.ToUniversalTime().Ticks - dateTime.Ticks).TotalSeconds);
    }
  }
}
