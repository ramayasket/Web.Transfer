// Decompiled with JetBrains decompiler
// Type: mycloud.Quota
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
  internal class Quota
  {
    private static Dictionary<string, long> data = new Dictionary<string, long>();
    private static bool modified;

    public static void reset(string user)
    {
      user = user.ToLower();
      if (!Quota.data.ContainsKey(user))
        return;
      Quota.data[user] = 0L;
    }

    public static long add(string user, long x)
    {
      long num = 0;
      if (x == 0L)
        return 0;
      user = user.ToLower();
      if (Quota.data.ContainsKey(user))
      {
        num = Quota.data[user];
        Quota.data[user] = num + x;
      }
      else
        Quota.data.Add(user, num + x);
      if (Quota.data[user] < 0L)
        Quota.data[user] = 0L;
      Quota.modified = true;
      return num + x;
    }

    public static long get(string user)
    {
      long num = 0;
      user = user.ToLower();
      if (Quota.data.ContainsKey(user))
        num = Quota.data[user];
      return num;
    }

    public static void init()
    {
      int num1 = 0;
      try
      {
        using (StreamReader streamReader = new StreamReader(clib.work("quota.dat")))
        {
          string x;
          while ((x = streamReader.ReadLine()) != null)
          {
            string[] strArray = clib.string_words(x);
            if (((IEnumerable<string>) strArray).Count<string>() >= 2)
            {
              ++num1;
              long num2 = clib.atol(strArray[1]);
              if (num2 < 0L)
                num2 = 0L;
              Quota.data.Add(strArray[0], num2);
            }
          }
          streamReader.Close();
        }
      }
      catch (Exception ex)
      {
        clib.imsg("Read quota.dat failed {0}", (object) ex.Message);
      }
    }

    public static void save()
    {
      int num = 0;
      if (!Quota.modified)
        return;
      clib.imsg("Quota: writing to {0}", (object) clib.work("quota.dat"));
      Quota.modified = false;
      try
      {
        using (StreamWriter streamWriter = new StreamWriter(clib.work("quota.new")))
        {
          foreach (KeyValuePair<string, long> keyValuePair in Quota.data)
          {
            streamWriter.WriteLine("{0} {1}", (object) keyValuePair.Key, (object) keyValuePair.Value);
            ++num;
          }
          streamWriter.Close();
          File.Delete(clib.work("quota.dat"));
          File.Move(clib.work("quota.new"), clib.work("quota.dat"));
        }
      }
      catch (Exception ex)
      {
        clib.imsg("save: quota.dat failed {0}", (object) ex.Message);
      }
      clib.imsg("Quota: wrote {0} records", (object) num);
    }
  }
}
