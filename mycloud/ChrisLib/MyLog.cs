// Decompiled with JetBrains decompiler
// Type: ChrisLib.MyLog
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.IO;
using System.Text;
using System.Threading;

namespace ChrisLib
{
  public class MyLog
  {
    private string mylock = nameof (mylock);
    private FileStream fs;
    public string fname;
    private bool done_log;
    private bool doconsole;

    public void set_console(bool b)
    {
      this.doconsole = b;
    }

    public MyLog(string fname, bool doconsole)
    {
      this.fname = fname;
      this.doconsole = doconsole;
    }

    public void log(string info)
    {
      Encoding utF8 = Encoding.UTF8;
      if (this.fs == null)
        this.open();
      info = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss ") + string.Format("[{0}] ", (object) Thread.CurrentThread.ManagedThreadId) + info;
      byte[] bytes1 = utF8.GetBytes(info);
      byte[] bytes2 = utF8.GetBytes("\r\n");
      clib.replace_eol(bytes1);
      lock (this.mylock)
      {
        if (this.doconsole)
          Console.WriteLine(info);
        if (this.fs == null)
          return;
        this.fs.Write(bytes1, 0, bytes1.Length);
        this.fs.Write(bytes2, 0, bytes2.Length);
      }
    }

    public void flush()
    {
      lock (this.mylock)
      {
        if (this.fs == null)
          return;
        this.fs.Flush();
      }
    }

    public void idle()
    {
      try
      {
        lock (this.mylock)
        {
          this.close();
          try
          {
            if (new FileInfo(this.fname).Length > 900000L)
            {
              string str = this.fname + "2";
              if (File.Exists(str))
                File.Delete(str);
              File.Move(this.fname, str);
            }
          }
          catch
          {
          }
          this.open();
        }
      }
      catch (Exception ex)
      {
        if (this.done_log)
          return;
        this.done_log = true;
        Console.WriteLine("Logging failed {0}", (object) ex.ToString());
      }
    }

    public void close()
    {
      lock (this.mylock)
      {
        if (this.fs == null)
          return;
        this.fs.Close();
        this.fs = (FileStream) null;
      }
    }

    public string read_last(int nbytes)
    {
      byte[] numArray = new byte[nbytes + 1];
      string fname = this.fname;
      try
      {
        lock (this.mylock)
        {
          this.close();
          FileStream fileStream = new FileStream(this.fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
          try
          {
            fileStream.Seek((long) -nbytes, SeekOrigin.End);
          }
          catch
          {
            fileStream.Seek(0L, SeekOrigin.Begin);
          }
          if (fileStream.Read(numArray, 0, nbytes) <= 0)
            return "";
        }
      }
      catch (Exception ex)
      {
        clib.imsg("read_last: failed {0} {1} ", (object) ex.Message, (object) ex.ToString());
      }
      return Encoding.UTF8.GetString(numArray);
    }

    public void open()
    {
      lock (this.mylock)
      {
        if (this.fs != null)
          this.fs.Close();
        try
        {
          Directory.CreateDirectory(clib.log_file(""));
        }
        catch
        {
        }
        this.fs = new FileStream(this.fname, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
      }
    }

    public void rotate()
    {
      string fname = this.fname;
      string str = this.fname + "_2";
      this.close();
      try
      {
        File.Delete(str);
        File.Move(fname, str);
      }
      catch
      {
      }
    }
  }
}
