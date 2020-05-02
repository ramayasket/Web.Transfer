// Decompiled with JetBrains decompiler
// Type: mycloud.WebFile
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System;
using System.IO;

namespace mycloud
{
  internal class WebFile : WebModule
  {
    public override object Clone()
    {
      return (object) new WebFile();
    }

    public static void imsg(string format, params object[] args)
    {
      clib.webmsg("{0}", (object) string.Format(format, args));
    }

    public override void dropconnection(Websvc w)
    {
    }

    public override bool isforme(string path, string url)
    {
      WebFile.imsg("isforme [{0}]", (object) path);
      return path.StartsWith("/img") || path.StartsWith("/web/") || (path == "/web" || url.StartsWith("/favicon"));
    }

    public override void data_in(byte[] inbf, int inlen)
    {
      WebFile.imsg("Got {0} bytes", (object) inlen);
    }

    public static byte[] readfile(string fullFilePath)
    {
      WebFile.imsg("openread");
      FileStream fileStream = (FileStream) null;
      WebFile.imsg("openread2");
      try
      {
        fileStream = File.OpenRead(fullFilePath);
        WebFile.imsg("openread3");
        byte[] buffer = new byte[fileStream.Length];
        fileStream.Read(buffer, 0, Convert.ToInt32(fileStream.Length));
        WebFile.imsg("Read worked");
        return buffer;
      }
      catch (Exception ex)
      {
        WebFile.imsg("openread4");
        WebFile.imsg("File error {0}", (object) ex.ToString());
        return (byte[]) null;
      }
      finally
      {
        if (fileStream != null)
        {
          fileStream.Close();
          fileStream.Dispose();
        }
      }
    }

    public override string myname()
    {
      return nameof (WebFile);
    }

    public override bool do_headers(Websvc w)
    {
      string fname = "." + w.url;
      if (fname == "./favicon.ico")
        fname = "img/favicon.ico";
      return WebFile.send_file(w, clib.app(fname), false);
    }

    public static bool send_file(Websvc w, string fname, bool attached)
    {
      SimpleStream ss = new SimpleStream();
      ss.open(fname, true, false, out string _);
      return WebFile.send_file(w, ss, fname, attached, true);
    }

    private static bool send_file_headers(Websvc w, SimpleStream ss)
    {
      w.chan.write(string.Format("Date: {0}\r\n", (object) ss.lastmodified().ToHttpDate()));
      w.chan.write(string.Format("ETag: {0}\r\n", (object) ss.etag()));
      w.chan.write(string.Format("Last-Modified: {0}\r\n", (object) ss.lastmodified().ToHttpDate()));
      return true;
    }

    public static bool send_file(
      Websvc w,
      SimpleStream ss,
      string fname,
      bool attached,
      bool andbody)
    {
      if (ss == null || !ss.isopen)
      {
        string x = "Send:File not found " + fname;
        w.chan.write(string.Format("HTTP/1.1 500 FILE NOT FOUND\r\nContent-Length: {0}\r\n\r\n", (object) x.Length));
        w.chan.write(x);
        return false;
      }
      if (!andbody)
      {
        Web.any_header(w, clib.content_type(fname), "200 Ok", 0, false);
        WebFile.send_file_headers(w, ss);
        w.body_send();
        return true;
      }
      if (w.ifmodified == ss.lastmodified().ToHttpDate())
      {
        Web.any_header(w, clib.content_type(fname), "304 Not modified", 0, false);
        w.body_send();
        return true;
      }
      Web.any_header(w, clib.content_type(fname), "200 Ok", -1, true);
      WebFile.send_file_headers(w, ss);
      if (attached)
        w.chan.write(string.Format("Content-Disposition: attachment; filename=\"{0}\"\r\n", (object) clib.fileonly(fname)));
      w.chan.write("\r\n");
      while (true)
      {
        int sz = 10000;
        byte[] numArray = new byte[sz];
        int count = ss.read(numArray, 0, sz);
        if (count > 0)
        {
          w.chan.write(string.Format("{0:x}\r\n", (object) count));
          w.chan.Write(numArray, 0, count);
          w.chan.write("\r\n");
        }
        else
          break;
      }
      w.chan.write("0\r\n\r\n");
      return true;
    }

    public override bool do_body(Websvc w, byte[] inbf, int inlen)
    {
      WebFile.imsg("Got body packet of {0} bytes", (object) inlen);
      return true;
    }

    public override bool do_body_end(Websvc w)
    {
      WebFile.imsg("Got END of body - webfile...");
      return true;
    }
  }
}
