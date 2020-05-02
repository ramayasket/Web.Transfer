// Decompiled with JetBrains decompiler
// Type: mycloud.MyKey
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using Mylib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace mycloud
{
  internal class MyKey
  {
    private static int PRE_CRC = 10;
    public static string s_product = "FtpDav";
    public static string s_email = "";
    private static string robot_email;
    public static int regid;
    public static int regdate;
    public static int host;
    public static int prodid;
    public static int flags;
    public static int ulimit;
    public static int reg_day1;
    public static int reg_day2;
    public static long fromdate;
    public static long expdate;
    public static string s_code;
    public static int s_hostid;
    private static string keyfile;
    public static bool istemp;

    public static void init(string fname)
    {
      MyKey.keyfile = fname;
      MyKey.load();
      if (MyKey.s_code.Length <= 0)
        return;
      MyKey.decode(MyKey.s_code, out string _);
    }

    public static int ulimit_max()
    {
      if (MyKey.s_code.Length < 20 || MyKey.isexpired())
        return 5;
      return MyKey.ulimit > 0 ? MyKey.ulimit : 10000000;
    }

    public static void save()
    {
      NameValueCollection all = new NameValueCollection();
      all.Set("regid", clib.int_to_string(MyKey.regid));
      all.Set("email", MyKey.s_email);
      all.Set("hostid", clib.int_to_string(MyKey.s_hostid));
      all.Set("code", MyKey.s_code);
      clib.pair_save(all, MyKey.keyfile);
    }

    public static void load()
    {
      NameValueCollection all = clib.pair_load(MyKey.keyfile);
      MyKey.s_email = all.get_safe("email");
      MyKey.regid = clib.atoi(all.get_safe("regid"));
      MyKey.s_hostid = clib.atoi(all.get_safe("hostid"));
      MyKey.s_code = all.get_safe("code");
      if (MyKey.s_hostid != 0)
        return;
      MyKey.s_hostid = new Random().Next(1000000);
      MyKey.save();
    }

    public static string purchase_url()
    {
      return MyKey.regid <= 0 ? string.Format("https://netwinsite.com/cgi-bin/keycgi.exe?cmd=buy_new&product={0}", (object) MyKey.s_product) : string.Format("https://netwinsite.com/cgi-bin/keycgi.exe?cmd=buy&email={0}&regid=n{1}", (object) MyKey.s_email, (object) MyKey.regid);
    }

    public static bool decode(string key, out string reason)
    {
      byte[] xin = clib.hex_to_byte(key);
      int index1 = 0;
      int mon = 0;
      reason = "";
      if (key.Length < 30)
      {
        reason = "tooshort: Keys are 30 char long";
        return false;
      }
      MyKey.regid = (int) xin[index1] + (int) xin[index1 + 1] * 256 + (int) xin[index1 + 2] * 65536;
      int index2 = index1 + 3;
      MyKey.regdate = (int) xin[index2] + (int) xin[index2 + 1] * 256;
      int index3 = index2 + 2;
      MyKey.host = (int) xin[index3] + (int) xin[index3 + 1] * 256;
      int num1 = index3 + 2;
      byte[] numArray1 = xin;
      int index4 = num1;
      int num2 = index4 + 1;
      MyKey.prodid = (int) numArray1[index4];
      byte[] numArray2 = xin;
      int index5 = num2;
      int num3 = index5 + 1;
      MyKey.flags = (int) numArray2[index5];
      byte[] numArray3 = xin;
      int index6 = num3;
      int index7 = index6 + 1;
      MyKey.ulimit = MyKey.keylib_ulimit_decode((int) numArray3[index6]);
      int num4 = MyKey.keylib_crc(xin, MyKey.PRE_CRC);
      int num5 = (int) xin[index7] + (int) xin[index7 + 1] * 256;
      int num6 = index7 + 2;
      clib.imsg("crc {0} {1}", (object) num4, (object) num5);
      MyKey.reg_day1 = (int) xin[12];
      MyKey.reg_day2 = (int) xin[13];
      MyKey.fromdate = MyKey.keylib_fromdate(MyKey.regdate, MyKey.reg_day1, MyKey.reg_day2, out mon);
      if (MyKey.host != 0)
      {
        string host = MyKey.get_host();
        if (MyKey.keylib_host(host) != MyKey.host && MyKey.keylib_host(MyKey.keylib_host_trim(host)) != MyKey.host)
        {
          reason = string.Format("Key for wrong host=({0}) ({1}) , try activate again", (object) host, (object) MyKey.keylib_host_trim(host));
          return false;
        }
      }
      DateTime date = clib.unix_to_date(MyKey.fromdate);
      MyKey.expdate = date.AddMonths(mon).to_unix_date();
      if (mon == 0)
      {
        date = clib.unix_to_date(MyKey.fromdate);
        MyKey.expdate = date.AddMonths(12).to_unix_date();
      }
      MyKey.istemp = true;
      if (mon == 0)
        MyKey.istemp = false;
      clib.imsg("Fromdate is {0} {1} Expires {2}", (object) MyKey.fromdate, (object) clib.nice_unix_date(MyKey.fromdate), (object) clib.nice_unix_date(MyKey.expdate));
      if (num4 == num5)
        return true;
      reason = "bad crc";
      return false;
    }

    public static bool isexpired()
    {
      return MyKey.istemp && (long) clib.time() > MyKey.expdate;
    }

    private static string keylib_host_trim(string host)
    {
      int length = host.IndexOf(".");
      return length < 0 ? host : host.Substring(0, length);
    }

    public static string show()
    {
      return string.Format("Regid={0} regdate={1} host={2} prodid={3} flags={4} ulimit={5} istemp={6} host={7} expires={8}", (object) MyKey.regid, (object) MyKey.regdate, (object) MyKey.host, (object) MyKey.prodid, (object) MyKey.flags, (object) MyKey.ulimit, (object) MyKey.istemp, (object) MyKey.keylib_host_new(MyKey.get_host()), (object) clib.nice_unix_date(MyKey.expdate));
    }

    private static int keylib_host(string host)
    {
      host = host.ToLower();
      return host == "*" ? 0 : MyKey.keylib_crc(clib.string_to_byte(host), host.Length) & (int) ushort.MaxValue;
    }

    private static int keylib_crc(byte[] xin, int len)
    {
      byte[] numArray1 = new byte[1000];
      for (int index = 0; index < 400; ++index)
        numArray1[index] = (byte) 0;
      for (int index = 0; index < len; ++index)
        numArray1[index] = xin[index];
      int num1 = 0;
      int index1;
      for (int index2 = 0; index2 < len; index2 = index1 + 1)
      {
        int num2 = num1;
        byte[] numArray2 = numArray1;
        int index3 = index2;
        index1 = index3 + 1;
        int num3 = (int) numArray2[index3] * 256;
        num1 = num2 + num3 + (int) numArray1[index1] + (int) numArray1[index1 + 1] * index1 % (int) byte.MaxValue;
      }
      return num1 & (int) ushort.MaxValue;
    }

    private static int keylib_ulimit_decode(int n)
    {
      int num = n >= 20 ? (n >= 120 ? (n - 120) * 100 : (n - 20) * 10) : n;
      if (num == 440)
        num = 20000;
      if (num == 760)
        num = 100000;
      if (num == 1600)
        num = 1000000;
      return num;
    }

    private static long keylib_fromdate(int regdate, int reg_day1, int reg_day2, out int mon)
    {
      int num1 = regdate;
      int num2 = num1 & 4095;
      long unixDate = new DateTime(2002, 1, 1).to_unix_date();
      long num3 = (long) (num2 * 86400) + unixDate;
      mon = (num1 & 61440) >> 12;
      int num4 = reg_day1 + (reg_day2 << 8);
      if (num4 > 0 && num4 < 7200)
        num3 = (long) (num4 * 86400) + unixDate;
      return num3;
    }

    private static string keylib_host_new(string host)
    {
      return host + ":$new$";
    }

    private static long keylib_build_date()
    {
      return new DateTime(2012, 1, 1).to_unix_date();
    }

    public static string get_host()
    {
      return Environment.MachineName.ToLower();
    }

    public static string email_message()
    {
      return MyKey.robot_email;
    }

    public static bool keylib_activate_get(
      string prod,
      string sregid,
      string email,
      out string keycode,
      out string reason)
    {
      string str1 = "netwinsite.com";
      string str2 = MyKey.keylib_host_new(MyKey.get_host());
      string postUrl = string.Format("http://{0}/cgi-bin/keycgi.exe", (object) str1);
      keycode = "failed";
      MyKey.robot_email = string.Format("{{host: {0}}}\r\n{{regid: {1}}}\r\n{{email: {2}}}\r\n{{product: {3}}}\r\n{{build: {4}}}\r\n{{hostid: {5}}}\r\n", (object) str2, (object) sregid, (object) email, (object) prod, (object) MyKey.keylib_build_date(), (object) MyKey.s_hostid);
      if (email.Length > 0)
        MyKey.s_email = email;
      reason = "";
      sregid = sregid.ToLower();
      if (sregid.StartsWith("n"))
        sregid = sregid.Substring(1);
      Dictionary<string, object> postParameters = new Dictionary<string, object>();
      postParameters.Add("cmd", (object) "activate");
      postParameters.Add("regid", (object) sregid);
      postParameters.Add(nameof (email), (object) email);
      postParameters.Add("host", (object) str2);
      postParameters.Add("product", (object) prod);
      postParameters.Add("build", (object) MyKey.keylib_build_date());
      postParameters.Add("hostid", (object) MyKey.s_hostid);
      string end;
      try
      {
        string userAgent = "ftpdav";
        HttpWebResponse httpWebResponse = WebHelpers.MultipartFormDataPost(postUrl, userAgent, postParameters, "", "");
        end = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
        httpWebResponse.Close();
        stat.imsg("image post response {0}", (object) end);
      }
      catch (Exception ex)
      {
        stat.imsg("image post failed to connect {0}", (object) ex.ToString());
        return false;
      }
      keycode = "";
      string str3 = "";
      foreach (string str4 in end.string_lines_any())
      {
        clib.imsg("Response: {0}", (object) str4);
        if (str4.StartsWith("key: "))
          keycode = str4.Substring(5);
        if (str4.StartsWith("key_state: "))
          str3 = str4.Substring(1);
        if (str4.StartsWith("error: "))
          reason = str4.Substring(7);
      }
      if (reason.Length > 0)
        return false;
      clib.imsg("key state {0}", (object) str3);
      MyKey.s_code = keycode;
      return true;
    }
  }
}
