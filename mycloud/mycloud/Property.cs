// Decompiled with JetBrains decompiler
// Type: mycloud.Property
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace mycloud
{
  public class Property : IEnumerable
  {
    public static string pext = ".~prop";
    public NameValueCollection all = new NameValueCollection();
    private readonly string[] real = new string[10]
    {
      "DAV::getcontentlength",
      "DAV::creationdate",
      "DAV::getlastmodified",
      "DAV::getcontenttype",
      "DAV::getetag",
      "DAV::resourcetype",
      "DAV::supportedlock",
      "DAV::lockdiscovery",
      "DAV::quota-available-bytes",
      "DAV::quota-used-bytes"
    };

    private string propfname(string fname)
    {
      return fname + Property.pext;
    }

    public void set(string var, string val)
    {
      this.all.Set(var, val);
    }

    public void set_plain(string var, string val)
    {
      this.set(var, string.Format("<{0}>{1}</{0}>", (object) var, (object) val));
    }

    private void do_set(string var, string val)
    {
      this.all.Set(clib.simple_decode(var), clib.simple_decode(val));
    }

    public void del(string var)
    {
      this.all.Remove(var);
    }

    public void load(string path)
    {
      if (!File.Exists(this.propfname(path)))
        return;
      try
      {
        TextReader textReader = (TextReader) new StreamReader(this.propfname(path));
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
            this.do_set(strArray[0], strArray[1]);
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
        clib.imsg("Could not open {0} {1}", (object) path, (object) ex.ToString());
      }
    }

    public bool save(string path)
    {
      TextWriter textWriter;
      try
      {
        textWriter = (TextWriter) new StreamWriter(this.propfname(path));
      }
      catch (Exception ex)
      {
        clib.imsg("Property write failed for {0}", (object) ex.Message);
        return false;
      }
      foreach (string key in this.all.Keys)
      {
        clib.imsg("property.save {0} {1}", (object) key, (object) this.all[key]);
        if (key == "DAV::getlastmodified")
        {
          clib.imsg("property.save getlastmodified, so adjust file itself too {0}", (object) this.all[key]);

          // N.B.! not used, but present in the original assembly
          // DateTime dateTime = new DateTime();
          DateTime lastWriteTimeUtc = DateTime.Parse(this.all[key]);

          clib.imsg("getlastmodified property, read into date format is now {0}", (object) lastWriteTimeUtc.ToString());
          if (File.Exists(path))
            Directory.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
          else
            clib.imsg("getlastmodified, setting on non existent file ERROR ERRROR {0}", (object) path);
        }
        if (!((IEnumerable<string>) this.real).Contains<string>(key))
          textWriter.WriteLine(clib.simple_encode(key) + "~" + clib.simple_encode(this.all[key]));
      }
      textWriter.Close();
      return true;
    }

    public IEnumerator GetEnumerator()
    {
      foreach (string allKey in this.all.AllKeys)
        yield return (object) allKey;
    }
  }
}
