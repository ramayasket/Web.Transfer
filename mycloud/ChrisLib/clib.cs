// Decompiled with JetBrains decompiler
// Type: ChrisLib.clib
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace ChrisLib
{
  public static class clib
  {
    private static List<MyLog> logs = new List<MyLog>();
    private static string work_path = "/clib_work_undfined";
    private static string tmp_path = "tmp";
    private static string version = (string) null;
    private static string product = "FtpDav";
    private static bool keycgiBuy = true;
    private static clib.cb_log_del da;
    private static MyLog ilog;
    private static bool isdebug;
    private static MyLog wlog;
    private static MyLog clog;
    private static MyLog weblog;
    private static MyLog flog;
    private static bool xisexit;
    private static bool xstart_shutdown;
    public static string root_path;
    private static string log_path;

    public static bool canBuy()
    {
      return clib.keycgiBuy;
    }

    public static void setCanBuy(bool yn)
    {
      clib.keycgiBuy = yn;
    }

    public static void init_log_files()
    {
      clib.ilog = new MyLog(clib.log_file("imsg.log"), clib.isdebug);
      clib.wlog = new MyLog(clib.log_file("webdav.log"), false);
      clib.clog = new MyLog(clib.log_file("crash.log"), false);
      clib.weblog = new MyLog(clib.log_file("web.log"), false);
      clib.flog = new MyLog(clib.log_file("ftp.log"), false);
    }

    public static string app(string fname)
    {
      return clib.IsLinux ? fname : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + fname;
    }

    public static DateTime last_change(FileInfo f)
    {
      DateTime lastWriteTime = f.LastWriteTime;

      // N.B.! probably this empty 'if' statement is residual from debugging.
      // if (lastWriteTime.CompareTo(f.CreationTime) >= 0)
      // {
      // }

      clib.imsg("{0} Write {1}  Create {2}  Last {3} ", (object) f.Name, (object) f.LastWriteTime, (object) f.CreationTime, (object) lastWriteTime);
      return lastWriteTime;
    }

    public static DateTime last_change_utc(FileInfo f)
    {
      DateTime lastWriteTimeUtc = f.LastWriteTimeUtc;

      // N.B.! probably this empty 'if' statement is residual from debugging.
      // if (lastWriteTimeUtc.CompareTo(f.CreationTimeUtc) >= 0)
      // {
      // }

      clib.imsg("{0} Write {1}  Create {2}  Last {3} ", (object) f.Name, (object) f.LastWriteTimeUtc, (object) f.CreationTimeUtc, (object) lastWriteTimeUtc);
      return lastWriteTimeUtc;
    }

    public static string int_to_string(int x)
    {
      return x.ToString();
    }

    public static void set_product(string name)
    {
      clib.product = name;
    }

    public static string Product_Name()
    {
      return clib.product;
    }

    public static string make_url(string protocol, string host, int port, string suffix)
    {
      string str = protocol + "://" + host + suffix;
      if (protocol == "http" && port != 80 || protocol == "https" && port != 443)
        str = protocol + "://" + host + ":" + port.ToString() + suffix;
      return str;
    }

    public static void start_shutdown()
    {
      clib.xstart_shutdown = true;
    }

    public static bool is_start_shutdown()
    {
      return clib.xstart_shutdown;
    }

    public static void set_exit()
    {
      clib.xisexit = true;
    }

    public static bool is_exit()
    {
      return clib.xisexit;
    }

    public static void set_debug(bool x)
    {
      clib.isdebug = x;
      if (clib.ilog == null)
        return;
      clib.ilog.set_console(x);
    }

    public static void set_work(string x)
    {
      clib.work_path = x;
    }

    public static void set_tmp(string x)
    {
      clib.tmp_path = x;
    }

    public static string tmp()
    {
      return clib.tmp_path;
    }

    public static void set_log(string x)
    {
      clib.log_path = x;
    }

    public static void set_root(string x)
    {
      clib.root_path = x;
    }

    public static string tmp(string fname)
    {
      return clib.tmp_path + "/" + fname;
    }

    public static string product_name()
    {
      return clib.Product_Name().ToLower();
    }

    public static string work(string fname)
    {
      return clib.work_path + "/" + fname;
    }

    public static NameValueCollection pair_load(string fname)
    {
      NameValueCollection nameValueCollection = new NameValueCollection();
      if (!File.Exists(fname))
        return nameValueCollection;
      try
      {
        TextReader textReader = (TextReader) new StreamReader(fname);
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
                goto label_8;
            }
            while (((IEnumerable<string>) strArray).Count<string>() < 2);
            nameValueCollection.Set(clib.simple_decode(strArray[0]), clib.simple_decode(strArray[1]));
          }
        }
        catch (Exception ex)
        {
          clib.imsg("load failed {0}", (object) ex.ToString());
        }
label_8:
        textReader.Close();
      }
      catch (Exception ex)
      {
        clib.imsg("Could not open {0} {1}", (object) fname, (object) ex.ToString());
      }
      return nameValueCollection;
    }

    public static bool pair_save(NameValueCollection all, string fname)
    {
      TextWriter textWriter;
      try
      {
        textWriter = (TextWriter) new StreamWriter(fname);
      }
      catch (Exception ex)
      {
        clib.imsg("pair write failed for {0}", (object) ex.Message);
        return false;
      }
      foreach (string key in all.Keys)
        textWriter.WriteLine(clib.simple_encode(key) + "~" + clib.simple_encode(all[key]));
      textWriter.Close();
      return true;
    }

    public static DateTime get_date_utc(string filename)
    {
      try
      {
        return clib.last_change_utc(new FileInfo(filename));
      }
      catch
      {
        return DateTime.MinValue;
      }
    }

    public static string nice_unix_date(long unixticks)
    {
      return clib.unix_to_date(unixticks).ToHttpDate();
    }

    public static DateTime unix_to_date(long timestamp)
    {
      return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((double) timestamp);
    }

    public static int time()
    {
      return Convert.ToInt32(new TimeSpan(DateTime.Now.ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks).TotalSeconds);
    }

    public static long mtime()
    {
      return Convert.ToInt64(new TimeSpan(DateTime.Now.ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks).TotalMilliseconds);
    }

    public static string date_to_ftp(DateTime d)
    {
      return d.ToUniversalTime().ToString("yyyyMMddHHmmss");
    }

    public static DateTime ftp_to_date(string sample)
    {
      string format = "yyyyMMddHHmmss";
      DateTime exact;
      try
      {
        exact = DateTime.ParseExact(sample, format, (IFormatProvider) null);
      }
      catch (Exception ex)
      {
        clib.imsg("Invalid date: {0} {1}", (object) sample, (object) ex.Message);
        return DateTime.MinValue;
      }
      return exact.ToLocalTime();
    }

    public static long date_to_utc(DateTime d)
    {
      DateTime dateTime = new DateTime(1970, 1, 1);
      return (long) Convert.ToInt32(new TimeSpan(d.ToUniversalTime().Ticks - dateTime.Ticks).TotalSeconds);
    }

    public static string trim_trailing(string x, string stuff)
    {
      int index;
      for (index = x.Length - 1; index > 0; --index)
      {
        char ch = x[index];
        if (!stuff.Contains<char>(ch))
          break;
      }
      return x.Substring(0, index + 1);
    }

    public static string unangle(string x)
    {
      return x[0] != '<' ? x : x.Substring(1, x.Length - 2);
    }

    public static int min(int a, int b)
    {
      return a < b ? a : b;
    }

    public static string wash_file_name(string name)
    {
      return new Regex(string.Format("[{0}]", (object) Regex.Escape(new string(Path.GetInvalidFileNameChars()))), RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant).Replace(name, "");
    }

    public static string get_simple_name(string name)
    {
      string[] strArray = name.Split("/\\".ToCharArray());
      return strArray[((IEnumerable<string>) strArray).Count<string>() - 1];
    }

    public static byte[] string_to_byte(string bob)
    {
      int length = bob.Length;
      byte[] numArray = new byte[length];
      for (int index = 0; index < length; ++index)
        numArray[index] = (byte) bob[index];
      return numArray;
    }

    public static string decode_base64(string encodedData, Encoding enc)
    {
      return clib.decode_base64(encodedData);
    }

    public static string decode_base64(string encodedData)
    {
      try
      {
        byte[] bob = Convert.FromBase64String(encodedData);
        return clib.byte_to_string(bob, ((IEnumerable<byte>) bob).Count<byte>());
      }
      catch
      {
        return "";
      }
    }

    public static void log_lines(string bf)
    {
      foreach (string stringLine in clib.string_lines(bf))
        ;
    }

    public static string hdr_decode(string input)
    {
      StringBuilder stringBuilder1 = new StringBuilder();
      StringBuilder stringBuilder2 = new StringBuilder();
      bool flag = false;
      int index = 0;
      while (index < input.Length)
      {
        char ch1 = input[index];
        switch (ch1)
        {
          case '=':
            if ((index == input.Length - 1 ? (int) ' ' : (int) input[index + 1]) == 63)
            {
              flag = true;
              break;
            }
            break;
          case '?':
            char ch2 = index == input.Length - 1 ? ' ' : input[index + 1];
            if (ch2 == '=')
            {
              flag = false;
              stringBuilder2.Append(ch1);
              stringBuilder2.Append(ch2);
              stringBuilder1.Append(clib.ParseEncodedWord(stringBuilder2.ToString()));
              stringBuilder2 = new StringBuilder();
              index += 2;
              continue;
            }
            break;
        }
        if (flag)
        {
          stringBuilder2.Append(ch1);
          ++index;
        }
        else
        {
          stringBuilder1.Append(ch1);
          ++index;
        }
      }
      return stringBuilder1.ToString();
    }

    private static string ParseEncodedWord(string input)
    {
      StringBuilder stringBuilder = new StringBuilder();
      if (!input.StartsWith("=?") || !input.EndsWith("?="))
        return input;
      string name = input.Substring(2, input.IndexOf("?", 2) - 2);
      Encoding encoding = Encoding.GetEncoding(name);
      char c = input[name.Length + 3];
      int startIndex = name.Length + 5;
      switch (char.ToLowerInvariant(c))
      {
        case 'b':
          byte[] bytes = Convert.FromBase64String(input.Substring(startIndex, input.Length - startIndex - 2));
          stringBuilder.Append(encoding.GetString(bytes));
          break;
        case 'q':
          while (startIndex < input.Length)
          {
            char ch = input[startIndex];
            char[] chArray1 = new char[2];
            switch (ch)
            {
              case '=':
                char[] chArray2;
                if (startIndex < input.Length - 2)
                  chArray2 = new char[2]
                  {
                    input[startIndex + 1],
                    input[startIndex + 2]
                  };
                else
                  chArray2 = (char[]) null;
                char[] chArray3 = chArray2;
                if (chArray3 == null)
                {
                  stringBuilder.Append(ch);
                  ++startIndex;
                  break;
                }
                string str = encoding.GetString(new byte[1]
                {
                  Convert.ToByte(new string(chArray3, 0, 2), 16)
                });
                stringBuilder.Append(str);
                startIndex += 3;
                break;
              case '?':
                if (input[startIndex + 1] == '=')
                  ++startIndex;
                ++startIndex;
                break;
              default:
                stringBuilder.Append(ch);
                ++startIndex;
                break;
            }
          }
          break;
      }
      return stringBuilder.ToString();
    }

    public static string tohex(string x)
    {
      byte[] hex = clib.string_to_byte(x);
      return clib.tohex(hex, ((IEnumerable<byte>) hex).Count<byte>());
    }

    public static string byte_to_hex(byte[] bs)
    {
      return clib.byte_to_hex(bs, ((IEnumerable<byte>) bs).Count<byte>());
    }

    public static string byte_to_hex(byte[] bs, int len)
    {
      StringBuilder stringBuilder = new StringBuilder();
      if (len > ((IEnumerable<byte>) bs).Count<byte>())
        len = ((IEnumerable<byte>) bs).Count<byte>();
      for (int index = 0; index < len; ++index)
      {
        byte b = bs[index];
        stringBuilder.Append(b.ToString("x2").ToLower());
      }
      return stringBuilder.ToString();
    }

    public static string tohex(byte[] hex, int n)
    {
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < n; ++index)
        stringBuilder.AppendFormat("{0,2:X} ", (object) hex[index]);
      return stringBuilder.ToString();
    }

    public static string decode_quoted_printable(string input, Encoding enc)
    {
      int length = 100 + input.Length;
      byte[] bob = new byte[length];
      int len = 0;
      bool flag = false;
      if (enc == null)
        enc = Encoding.UTF8;
      try
      {
        for (int index = 0; index < input.Length; ++index)
        {
          if (input[index] == '=')
          {
            if (index + 2 >= input.Length)
            {
              flag = true;
              continue;
            }
            bob[len++] = (byte) Convert.ToInt32(input.Substring(index + 1, 2), 16);
            index += 2;
          }
          else
            bob[len++] = (byte) input[index];
          if (len > length - 2)
            break;
        }
        if (len > 0 && !flag)
          bob[len++] = (byte) 10;
        return clib.byte_to_string(bob, len);
      }
      catch
      {
        clib.imsg("decode_quoted_printable: failed on {0}", (object) input);
        return "";
      }
    }

    public static string web_decode(string b)
    {
      return HttpUtility.HtmlDecode(b);
    }

    public static string web_encode(string Html)
    {
      string str = Html.ToString();
      StringBuilder stringBuilder = new StringBuilder();
      foreach (char ch in str)
      {
        int length = stringBuilder.Length;
        if (ch == '&')
          stringBuilder.Append("&amp;");
        if (ch == '<')
          stringBuilder.Append("&lt;");
        if (ch == '>')
          stringBuilder.Append("&gt;");
        if (ch == '`')
          stringBuilder.Append("'");
        if (ch > '\x007F')
          stringBuilder.Append("&#" + (object) (int) ch + ";");
        if (length == stringBuilder.Length)
          stringBuilder.Append(ch);
      }
      return stringBuilder.ToString();
    }

    public static NameValueCollection simple_read(string line)
    {
      NameValueCollection nameValueCollection = new NameValueCollection();
      string[] strArray1 = line.Split("~\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
      ((IEnumerable<string>) strArray1).Count<string>();
      foreach (string str in strArray1)
      {
        string[] strArray2 = str.Split("=".ToCharArray());
        if (((IEnumerable<string>) strArray2).Count<string>() >= 2)
          nameValueCollection.Set(strArray2[0], clib.simple_decode(strArray2[1]));
      }
      return nameValueCollection;
    }

    public static string long_to_string(long x)
    {
      return string.Format("{0}", (object) x);
    }

    public static string add_slash(string x)
    {
      return x.EndsWith("/") ? x : x + "/";
    }

    public static string simple_encode(string text)
    {
      if (text == null)
        return "";
      StringBuilder stringBuilder = new StringBuilder();
      foreach (char ch in text)
      {
        if (ch > '\x007F' || ch < ' ' || (ch == '~' || ch == ';') || ch == '#' || ch == '=')
          stringBuilder.Append("#" + (object) (int) ch + ";");
        else
          stringBuilder.Append(ch);
      }
      return stringBuilder.ToString();
    }

    public static string simple_decode(string text)
    {
      StringBuilder stringBuilder1 = new StringBuilder();
      StringBuilder stringBuilder2 = new StringBuilder();
      bool flag = false;
      foreach (char ch in text)
      {
        if (flag)
        {
          if (ch == ';')
          {
            char int32 = (char) Convert.ToInt32(stringBuilder2.ToString());
            stringBuilder2.Length = 0;
            flag = false;
            stringBuilder1.Append(int32);
          }
          else
            stringBuilder2.Append(ch);
        }
        else if (ch == '#')
          flag = true;
        else
          stringBuilder1.Append(ch);
      }
      return stringBuilder1.ToString();
    }

    public static string decode_text(string input, Encoding enc)
    {
      int length = 100 + input.Length;
      byte[] bytes = new byte[length];
      int count = 0;
      if (enc == null)
        enc = Encoding.UTF8;
      for (int index = 0; index < input.Length; ++index)
      {
        bytes[count++] = (byte) input[index];
        if (count > length - 2)
          break;
      }
      return enc.GetString(bytes, 0, count);
    }

    public static void create_path_if(string lclpath)
    {
      try
      {
        if (Directory.Exists(lclpath.path_only()))
          return;
        Directory.CreateDirectory(lclpath.path_only());
      }
      catch (Exception ex)
      {
        clib.imsg("Creating path failed {0} {1}", (object) lclpath.path_only(), (object) ex.Message);
      }
    }

    public static bool rename_folder(string path_src, string path_dst)
    {
      bool flag = false;
      try
      {
        Directory.Move(path_src, path_dst);
        flag = true;
      }
      catch (Exception ex)
      {
        clib.imsg("Directory rename failed {0} {1} {2}", (object) path_src.path_only(), (object) path_dst.path_only(), (object) ex.Message);
      }
      return flag;
    }

    public static string isyncu_temp()
    {
      return ".isyncu_temp_";
    }

    public static string path_encode(string text)
    {
      if (text == null)
        return "";
      StringBuilder stringBuilder = new StringBuilder();
      foreach (char ch in text)
      {
        if (ch > '\x007F' || ch < ' ' || (ch == '~' || ch == ';') || (ch == '#' || ch == '=' || (ch == '/' || ch == '\\')) || (ch == ':' || ch == '?' || (ch == '*' || ch == '|') || ch == '<') || ch == '>')
          stringBuilder.Append("#" + (object) (int) ch + ";");
        else
          stringBuilder.Append(ch);
      }
      return stringBuilder.ToString();
    }

    public static string nice_email(string f)
    {
      if (f == null)
        return "";
      int num1 = f.IndexOf('"');
      if (num1 >= 0)
      {
        int num2 = f.IndexOf('"', num1 + 1);
        if (num2 > 0)
          return f.Substring(num1 + 1, num2 - num1 - 1);
      }
      if (f.Contains(" "))
      {
        int length = f.LastIndexOf(" ");
        if (length > 0)
        {
          f = f.Substring(0, length);
          return f;
        }
      }
      return clib.email_only(f);
    }

    public static bool is_slash(char c)
    {
      return c == '/' || c == '\\';
    }

    public static string backintime(string fname, string rootx)
    {
      string str1 = rootx.trimslash();
      clib.imsg("backintime: fname {0} root {1} ", (object) fname, (object) str1);
      for (int length = str1.Length; length < fname.Length; ++length)
      {
        if (clib.is_slash(fname[length]))
        {
          string str2 = fname.Substring(0, length).path_add(".backintime").path_add(DateTime.Now.ToString("yyyyMMdd_hh")).path_add(fname.Substring(length));
          clib.imsg("backintime: result {0}", (object) str2);
          return str2;
        }
      }
      return (string) null;
    }

    public static string xml_space_encode(string input)
    {
      string source = " +%'`&";
      StringBuilder stringBuilder = new StringBuilder();
      foreach (char ch in input)
      {
        if (source.Contains<char>(ch) || ch < '!' && ch > char.MinValue)
        {
          string str = BitConverter.ToString(new byte[1]
          {
            (byte) ch
          });
          stringBuilder.AppendFormat("%{0}", (object) str);
        }
        else
          stringBuilder.Append(ch);
      }
      return stringBuilder.ToString();
    }

    public static string xml_encode(string unescaped)
    {
      XmlElement element = new XmlDocument().CreateElement("root");
      element.InnerText = unescaped;
      clib.imsg("xml_encoded {0} to {1}", (object) unescaped, (object) element.InnerXml);
      return element.InnerXml;
    }

    public static string url_encode(string input)
    {
      string source = "#'`,$\\\"><+ &|/;:=?@{}~[]!%)(-_*^.";
      StringBuilder stringBuilder = new StringBuilder();
      foreach (char ch in input)
      {
        if (source.Contains<char>(ch) || ch < '!' || ch > '~')
        {
          string str = BitConverter.ToString(new byte[1]
          {
            (byte) ch
          });
          stringBuilder.AppendFormat("%{0}", (object) str);
        }
        else
          stringBuilder.Append(ch);
      }
      return stringBuilder.ToString();
    }

    public static bool path_valid(string x)
    {
      string source = "?<>:*|\"";
      foreach (char ch in x)
      {
        if (source.Contains<char>(ch))
          return false;
      }
      return true;
    }

    public static string myurl_encode_nogood(string input)
    {
      return HttpUtility.UrlEncode(input);
    }

    public static string myurl_encode(string input_start)
    {
      string source = "#\\\">< ?@{}~[]~`%)(*^|";
      StringBuilder stringBuilder = new StringBuilder();
      foreach (char ch in clib.string_to_utf8(input_start))
      {
        if (source.Contains<char>(ch) || ch < '!' || ch > '~')
        {
          string str = BitConverter.ToString(new byte[1]
          {
            (byte) ch
          });
          stringBuilder.AppendFormat("%{0}", (object) str);
        }
        else
          stringBuilder.Append(ch);
      }
      clib.imsg("myurl_encode {0} -> {1}", (object) input_start, (object) stringBuilder.ToString());
      return stringBuilder.ToString();
    }

    public static string percent_decode(string input)
    {
      StringBuilder stringBuilder1 = new StringBuilder();
      StringBuilder stringBuilder2 = new StringBuilder();
      bool flag = false;
      foreach (char ch in input)
      {
        if (flag)
        {
          stringBuilder2.Append(ch);
          if (stringBuilder2.Length == 2)
          {
            char int32 = (char) Convert.ToInt32(stringBuilder2.ToString(), 16);
            stringBuilder2.Length = 0;
            flag = false;
            stringBuilder1.Append(int32);
          }
        }
        else if (ch == '%')
          flag = true;
        else
          stringBuilder1.Append(ch);
      }
      return stringBuilder1.ToString();
    }

    public static string percent_decode_utf(string input)
    {
      return clib.utf8_to_string(clib.percent_decode(input));
    }

    public static string url_decode_datax(string input)
    {
      return input == null ? (string) null : Uri.UnescapeDataString(input);
    }

    public static string url_local(string input)
    {
      int num = input.IndexOf("//");
      if (num < 0)
        return input;
      int startIndex = input.IndexOf("/", num + 2);
      return startIndex < 0 ? "/" : input.Substring(startIndex);
    }

    public static string url_decode(string input)
    {
      return input == null ? (string) null : HttpUtility.UrlDecode(input);
    }

    public static string to_native_slash(string fname)
    {
      return clib.IsLinux ? clib.to_unix_slash(fname) : clib.to_windows_slash(fname);
    }

    public static string to_windows_slash(string fname)
    {
      char[] charArray = fname.ToCharArray();
      int num = ((IEnumerable<char>) charArray).Count<char>();
      for (int index = 0; index < num; ++index)
      {
        if (charArray[index] == '/')
          charArray[index] = '\\';
      }
      return new string(charArray);
    }

    public static bool IsLinux
    {
      get
      {
        int platform = (int) Environment.OSVersion.Platform;
        int num;
        switch (platform)
        {
          case 4:
          case 6:
            num = 1;
            break;
          default:
            num = platform == 128 ? 1 : 0;
            break;
        }
        return num != 0;
      }
    }

    public static void set_Version(string s)
    {
      clib.version = s;
    }

    public static string VersionBuild()
    {
      return string.Format("{0}.{1}", (object) clib.Version(), (object) clib.Build());
    }

    public static string Version()
    {
      if (clib.version == null)
      {
        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        clib.version = string.Format("{0}.{1}", (object) versionInfo.ProductMajorPart, (object) versionInfo.ProductMinorPart);
      }
      return clib.version;
    }

    public static int Build()
    {
      return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductBuildPart;
    }

    public static string to_unix_slash(string fname)
    {
      char[] charArray = fname.ToCharArray();
      int num = ((IEnumerable<char>) charArray).Count<char>();
      for (int index = 0; index < num; ++index)
      {
        if (charArray[index] == '\\')
          charArray[index] = '/';
      }
      return new string(charArray);
    }

    public static void set_log_cb(clib.cb_log_del dax)
    {
      clib.da = dax;
    }

    public static string Capitalize(string x)
    {
      return x.Length == 0 ? x : x.ToUpper()[0].ToString() + x.Substring(1);
    }

    public static void email_split(string email, out string u1, out string d1)
    {
      string[] strArray = email.Split("@, <>\"".ToCharArray());
      u1 = d1 = "";
      if (((IEnumerable<string>) strArray).Count<string>() < 2)
        return;
      u1 = strArray[0];
      d1 = strArray[1];
    }

    public static string defang(string source)
    {
      char[] chArray = new char[source.Length + 20];
      int length = 0;
      bool flag1 = false;
      bool flag2 = false;
      for (int startIndex = 0; startIndex < source.Length; ++startIndex)
      {
        char ch = source[startIndex];
        try
        {
          if (flag1 && ch == '>')
          {
            flag2 = false;
            flag1 = false;
            continue;
          }
          if (ch == '<')
          {
            if (string.Compare(source.Substring(startIndex, 7), "<script", true) == 0)
              flag2 = true;
            if (string.Compare(source.Substring(startIndex, 9), "</script>", true) == 0)
              flag2 = false;
            if (string.Compare(source.Substring(startIndex, 8), "<jscript", true) == 0)
              flag2 = true;
            if (string.Compare(source.Substring(startIndex, 10), "</jscript>", true) == 0)
              flag2 = false;
          }
        }
        catch
        {
        }
        if (!flag2)
        {
          chArray[length] = ch;
          ++length;
        }
      }
      return new string(chArray, 0, length);
    }

    public static string strip_misc(string source, string misc)
    {
      char[] chArray = new char[source.Length];
      int length = 0;
      for (int index = 0; index < source.Length; ++index)
      {
        char ch = source[index];
        if (!misc.Contains<char>(ch))
          chArray[length++] = ch;
      }
      return new string(chArray, 0, length);
    }

    public static string email_remove(string list, string me)
    {
      StringBuilder stringBuilder = new StringBuilder();
      string[] strArray = list.Split(",".ToCharArray());
      if (((IEnumerable<string>) strArray).Count<string>() == 1)
        return list;
      foreach (string str in strArray)
      {
        string email = str.Trim();
        if (!(clib.email_only(email) == me))
        {
          if (stringBuilder.Length > 0)
            stringBuilder.Append(", ");
          stringBuilder.Append(email);
        }
      }
      return stringBuilder.ToString();
    }

    public static int email_count(string emaillist)
    {
      int num = 1;
      bool flag = false;
      if (emaillist == null)
        return 0;
      foreach (char ch in emaillist)
      {
        if (!flag && ch == ',')
          ++num;
        if (ch == '"')
          flag = !flag;
      }
      return num;
    }

    public static bool same_email(string a, string b)
    {
      return clib.email_only(a) == clib.email_only(b);
    }

    public static string log_read_last(string fname, int nbytes)
    {
      foreach (MyLog log in clib.logs)
      {
        if (log.fname == fname)
          return log.read_last(nbytes);
        log.close();
      }
      return "NO MYLOG MATCHED ";
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

    public static bool string_to_file(string x, string fname)
    {
      try
      {
        using (StreamWriter streamWriter = new StreamWriter(fname))
          streamWriter.Write(x);
      }
      catch (Exception ex)
      {
        clib.imsg("string_to_File error {0}", (object) ex.ToString());
        return false;
      }
      return true;
    }

    public static bool same_domain(string email1, string email2)
    {
      string d1_1;
      clib.email_split(email1, out string _, out d1_1);
      string d1_2;
      clib.email_split(email2, out string _, out d1_2);
      return d1_1 == d1_2;
    }

    public static int nice_atoi(string stuff)
    {
      int length = stuff.IndexOf(" ");
      if (length < 0)
      {
        clib.imsg("Found no space in string {0}", (object) stuff);
        return clib.atoi(stuff);
      }
      int num = clib.atoi(stuff.Substring(0, length));
      string lower = stuff.ToLower();
      clib.imsg("Lowercase {0}", (object) lower);
      if (lower.Contains("second"))
        return num;
      if (lower.Contains("min"))
        return num * 60;
      if (lower.Contains("hour"))
        return num * 60 * 60;
      if (lower.Contains("day"))
        return num * 60 * 60 * 24;
      if (lower.Contains("week"))
        return num * 60 * 60 * 24 * 7;
      if (lower.Contains("month"))
        return num * 60 * 60 * 24 * 7 * 4;
      clib.imsg("no string match {0} ", (object) lower);
      return num;
    }

    public static byte[] hex_to_byte(string hexString)
    {
      if (hexString.Length % 2 != 0)
        return (byte[]) null;
      byte[] numArray = new byte[hexString.Length / 2];
      for (int index = 0; index < numArray.Length; ++index)
      {
        string s = hexString.Substring(index * 2, 2);
        numArray[index] = byte.Parse(s, NumberStyles.HexNumber, (IFormatProvider) CultureInfo.InvariantCulture);
      }
      return numArray;
    }

    public static long nice_atol(string stuff)
    {
      int num1 = stuff.end_number();
      string stuff1 = stuff.Substring(0, num1);
      string str = stuff.Substring(num1);
      long num2 = clib.atol(stuff1);
      string lower = str.ToLower();
      if (lower.Contains("t"))
        return num2 * 1000L * 1000L * 1000L * 1000L;
      if (lower.Contains("g"))
        return num2 * 1000L * 1000L * 1000L;
      if (lower.Contains("m"))
        return num2 * 1000L * 1000L;
      return lower.Contains("k") ? num2 * 1000L : num2;
    }

    public static int atoi(string stuff)
    {
      int result = 0;
      if (stuff == null)
        return 0;
      int length = stuff.IndexOf(' ');
      if (length > 0)
        stuff = stuff.Substring(0, length);
      int.TryParse(stuff, out result);
      return result;
    }

    public static int hex_to_int(string stuff)
    {
      int result = 0;
      if (stuff == null)
        return 0;
      int length = stuff.IndexOf(' ');
      if (length > 0)
        stuff = stuff.Substring(0, length);
      int.TryParse(stuff, NumberStyles.HexNumber, (IFormatProvider) CultureInfo.InvariantCulture, out result);
      return result;
    }

    public static long atol(string stuff)
    {
      long result = 0;
      if (stuff == null)
        return 0;
      long.TryParse(stuff, out result);
      return result;
    }

    public static bool to_bool(string stuff)
    {
      bool result;
      bool.TryParse(stuff, out result);
      return result;
    }

    public static void imsg_rotate()
    {
      foreach (MyLog log in clib.logs)
        log.rotate();
    }

    public static string content_type(string fname)
    {
      fname = fname.ToLower();
      if (fname.Contains(".jpg") || fname.Contains(".jpg") || fname.Contains(".jpeg"))
        return "image/JPEG";
      if (fname.Contains(".gif"))
        return "image/GIF";
      if (fname.Contains(".png"))
        return "image/PNG";
      if (fname.Contains(".htm"))
        return "text/HTML;";
      if (fname.Contains(".sgml"))
        return "text/sgml";
      if (fname.Contains(".htc"))
        return "text/x-component";
      if (fname.Contains("rss.xml"))
        return "application/rss+xml";
      if (fname.Contains(".xml"))
        return "application/xml";
      if (fname.Contains(".css"))
        return "text/css";
      if (fname.Contains(".txt"))
        return "text/plain";
      if (fname.Contains(".js"))
        return "text/javascript";
      if (fname.Contains(".wml") || fname.Contains(".wap"))
        return "text/vnd.wap.wml";
      if (fname.Contains(".exe") || fname.Contains(".doc") || (fname.Contains(".xls") || fname.Contains(".com")))
        return "application/octet-stream";
      if (fname.Contains(".zip"))
        return "application/x-zip-compressed";
      if (fname.Contains(".jnlp"))
        return "application/x-java-jnlp-file";
      if (fname.Contains(".jar"))
        return "application/java-archive";
      if (fname.Contains(".torrent") || fname.Contains(".tor"))
        return "application/x-bittorrent";
      if (fname.Contains(".eml"))
        return "message/rfc822";
      if (fname.Contains(".vcf"))
        return "text/x-vcard";
      if (fname.Contains(".csv"))
        return "text/csv";
      return fname.Contains(".swf") ? "application/x-shockwave-flash" : "application/octet-stream";
    }

    public static NameValueCollection ParseQueryString(string s)
    {
      NameValueCollection nameValueCollection = new NameValueCollection();
      if (s.Contains("?"))
        s = s.Substring(s.IndexOf('?') + 1);
      foreach (string input in Regex.Split(s, "&"))
      {
        string[] strArray = Regex.Split(input, "=");
        if (strArray.Length == 2)
        {
          clib.imsg("query string {0} {1}", (object) strArray[1], (object) clib.url_decode(strArray[1]));
          nameValueCollection.Add(strArray[0], clib.url_decode(strArray[1]));
        }
        else
          nameValueCollection.Add(strArray[0], string.Empty);
      }
      return nameValueCollection;
    }

    public static string pathonly(string full)
    {
      int length = full.LastIndexOf('/');
      int num = full.LastIndexOf('\\');
      if (num > length)
        length = num;
      return length < 0 ? "" : full.Substring(0, length);
    }

    public static string fileonly(string full)
    {
      int num1 = full.LastIndexOf('/');
      int num2 = full.LastIndexOf('\\');
      if (num2 > num1)
        num1 = num2;
      return num1 < 0 ? full : full.Substring(num1 + 1);
    }

    public static string fileonly(string full, string cwd)
    {
      int num = cwd.Length - 1;
      if (!cwd.EndsWith("/") && !cwd.EndsWith("\\"))
        ++num;
      return full.Substring(num + 1);
    }

    public static string pathstart(string full)
    {
      int length = full.IndexOf('/', 1);
      if (length < 0)
        length = 0;
      return full.Substring(0, length);
    }

    public static void replace_eol(byte[] bob)
    {
      int num = ((IEnumerable<byte>) bob).Count<byte>();
      for (int index = 0; index < num; ++index)
      {
        if (bob[index] == (byte) 13)
          bob[index] = (byte) 94;
        if (bob[index] == (byte) 10)
          bob[index] = (byte) 126;
      }
    }

    public static string to_url(string fname)
    {
      return ("http://internal/" + fname).Replace("\\", "/").Replace(" ", "%20");
    }

    public static int IndexOf(byte[] arrayToSearchThrough, string tofind, int from, int inlen)
    {
      char[] charArray = tofind.ToCharArray();
      if (charArray.Length > arrayToSearchThrough.Length)
        return -1;
      for (int index1 = from; index1 < inlen - charArray.Length + 1; ++index1)
      {
        bool flag = true;
        for (int index2 = 0; index2 < charArray.Length; ++index2)
        {
          if ((int) arrayToSearchThrough[index1 + index2] != (int) (byte) charArray[index2])
          {
            flag = false;
            break;
          }
        }
        if (flag)
          return index1;
      }
      return -1;
    }

    public static string utf8_to_string(byte[] bob, int len)
    {
      return Encoding.UTF8.GetString(bob, 0, len);
    }

    public static string utf8_to_string(string x)
    {
      return clib.utf8_to_string(clib.string_to_byte(x), x.Length);
    }

    public static string string_to_utf8(string x)
    {
      byte[] bytes = Encoding.UTF8.GetBytes(x);
      return clib.byte_to_string(bytes, bytes.Length);
    }

    public static string nice_int(long x)
    {
      if (x > 3000000000L)
        return string.Format("{0}gb", (object) (x / 1000000000L));
      if (x > 3000000L)
        return string.Format("{0}mb", (object) (x / 1000000L));
      return x > 3000L ? string.Format("{0}k", (object) (x / 1000L)) : string.Format("{0}", (object) x);
    }

    public static string nice_int_1024(long x)
    {
      if (x > 3000000000L)
        return string.Format("{0}gb", (object) (x / 1073741824L));
      if (x > 3000000L)
        return string.Format("{0}Mb", (object) (x / 1048576L));
      return x > 3000L ? string.Format("{0}K", (object) (x / 1024L)) : string.Format("{0}", (object) x);
    }

    private static string WildcardToRegex(string pattern)
    {
      return "^" + pattern.Replace(".", "\\.").Replace("*", "(.*)").Replace("?", "(.{1,1})");
    }

    public static bool wildmatch(string thing, string wild)
    {
      string regex = clib.WildcardToRegex(wild);
      return Regex.IsMatch(thing, regex, RegexOptions.IgnoreCase);
    }

    public static string percent(double x)
    {
      return string.Format("{0:0.0}%", (object) (x * 100.0));
    }

    public static string nice_n(long x)
    {
      if (x > 3000000000L)
        return string.Format("{0}g", (object) (x / 1000000000L));
      if (x > 3000000L)
        return string.Format("{0}m", (object) (x / 1000000L));
      return x > 3000L ? string.Format("{0}k", (object) (x / 1000L)) : string.Format("{0}", (object) x);
    }

    public static string nice_int_nolimit(long x)
    {
      if (x == 0L)
        return "no limit";
      if (x > 3000000000L)
        return string.Format("{0}gb", (object) (x / 1000000000L));
      if (x > 3000000L)
        return string.Format("{0}mb", (object) (x / 1000000L));
      return x > 3000L ? string.Format("{0}k", (object) (x / 1000L)) : string.Format("{0}", (object) x);
    }

    public static string byte_to_string(byte[] bob, int len)
    {
      char[] chArray = new char[len];
      for (int index = 0; index < len; ++index)
        chArray[index] = (char) bob[index];
      return new string(chArray);
    }

    public static void imsg_flush()
    {
      clib.ilog.flush();
      clib.ilog.close();
    }

    public static string tail(string fname, int len)
    {
      try
      {
        FileStream fileStream = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        StreamReader streamReader = new StreamReader((Stream) fileStream);
        fileStream.Seek((long) -len, SeekOrigin.End);
        streamReader.BaseStream.Seek((long) -len, SeekOrigin.End);
        string end = streamReader.ReadToEnd();
        streamReader.Close();
        fileStream.Close();
        return end;
      }
      catch (Exception ex)
      {
        return "tail failed ************* " + ex.Message;
      }
    }

    public static string imsg_fname()
    {
      return clib.ilog.fname;
    }

    public static void imsg(string format, params object[] args)
    {
      if (clib.ilog == null)
        return;
      Encoding utF8 = Encoding.UTF8;
      string str = string.Format(format, args);
      clib.ilog.log(str);
      if (clib.da == null)
        return;
      clib.da(str);
    }

    public static void fmsg(string format, params object[] args)
    {
      Encoding utF8 = Encoding.UTF8;
      string info = string.Format(format, args);
      clib.flog.log(info);
      clib.ilog.log(info);
    }

    public static void wmsg(string format, params object[] args)
    {
      Encoding utF8 = Encoding.UTF8;
      string info = string.Format(format, args);
      clib.wlog.log(info);
    }

    public static void startstop(string format, params object[] args)
    {
      Encoding utF8 = Encoding.UTF8;
      File.AppendAllText(clib.log_file("startstop.log"), DateTime.Now.ToString("hh:mm:ss ") + string.Format(format, args) + "\n");
    }

    public static void cmsg(string format, params object[] args)
    {
      Encoding utF8 = Encoding.UTF8;
      string info = string.Format(format, args);
      clib.clog.log(info);
    }

    public static void webmsg(string format, params object[] args)
    {
      Encoding utF8 = Encoding.UTF8;
      string info = string.Format(format, args);
      clib.weblog.log(info);
    }

    public static string url_to_file(string x)
    {
      if (x.StartsWith("http://"))
        x = x.Substring(7);
      if (x.StartsWith("https://"))
        x = x.Substring(8);
      x = x.Replace(':', '_');
      x = x.Replace('.', '_');
      x = x.Replace('\\', '_');
      x = x.Replace('/', '_');
      return x;
    }

    public static string web_nbsp(string Html)
    {
      string str = Html.ToString();
      StringBuilder stringBuilder = new StringBuilder();
      foreach (char ch in str)
      {
        if (ch == ' ')
          stringBuilder.Append("&nbsp;");
        else
          stringBuilder.Append(ch);
      }
      return stringBuilder.ToString();
    }

    public static long file_size(string path)
    {
      try
      {
        return new FileInfo(path).Length;
      }
      catch
      {
        return 0;
      }
    }

    public static DateTime file_modified(string path)
    {
      try
      {
        return clib.last_change(new FileInfo(path));
      }
      catch
      {
        return DateTime.Now;
      }
    }

    public static string[] string_lines(string x)
    {
      return x.Split(new string[1]{ "\r\n" }, StringSplitOptions.None);
    }

    public static string[] string_lines_any(string x)
    {
      return x.Split(new string[3]{ "\r\n", "\r", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string[] string_words(string x)
    {
      return x.Split("\t \r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
    }

    public static string log_file(string fname)
    {
      return clib.log_path + "/" + fname;
    }

    public static void log_flush()
    {
      foreach (MyLog log in clib.logs)
        log.flush();
    }

    public static void log_close()
    {
      foreach (MyLog log in clib.logs)
        log.close();
    }

    public static void log_idle()
    {
      if (clib.logs.Count<MyLog>() == 0)
      {
        clib.logs.Add(clib.ilog);
        clib.logs.Add(clib.wlog);
        clib.logs.Add(clib.flog);
        clib.logs.Add(clib.weblog);
        clib.logs.Add(clib.clog);
      }
      foreach (MyLog log in clib.logs)
        log.idle();
    }

    public delegate void cb_log_del(string stuff);
  }
}
