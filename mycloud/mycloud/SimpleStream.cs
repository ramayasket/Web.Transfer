// Decompiled with JetBrains decompiler
// Type: mycloud.SimpleStream
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System;
using System.IO;

namespace mycloud
{
  public class SimpleStream
  {
    private DateTime lastmod = DateTime.Now;
    private FileStream fs;
    private string quota_user;
    public bool isopen;
    private bool isread;
    private bool iswrite;
    private long quota_tot;
    private string saved_fname;

    ~SimpleStream()
    {
      if (this.isread || this.saved_fname == null || !this.isopen)
        return;
      if (this.isopen)
        this.fs.Close();
      this.isopen = false;
      clib.imsg("simplestream distructor, ondropconnection? deleting {0}", (object) this.saved_fname);
      this.delete();
    }

    public DateTime lastmodified()
    {
      return this.lastmod;
    }

    public string etag()
    {
      return string.Format("W/\"{0}\"", (object) this.lastmod.to_unix_date());
    }

    public bool open(string fname, bool isread, bool isappend, out string reason)
    {
      return this.open(fname, isread, isappend, "", out reason);
    }

    public void delete()
    {
      File.Delete(this.saved_fname);
    }

    public bool open(
      string fname,
      bool isread,
      bool isappend,
      string username,
      out string reason)
    {
      this.isread = isread;
      this.fs = (FileStream) null;
      this.quota_user = username;
      reason = "";
      this.iswrite = false;
      this.saved_fname = fname;
      if (!isread && !isappend)
      {
        this.iswrite = true;
        fname += ".ftpdav_uploading";
      }
      try
      {
        if (isread)
          this.lastmod = File.GetLastWriteTime(fname);
        if (isread)
          this.fs = File.OpenRead(fname);
        else if (isappend)
        {
          this.fs = File.Open(fname, FileMode.Append);
        }
        else
        {
          this.quota_tot = -clib.file_size(fname);
          this.fs = File.Open(fname, FileMode.Create);
          clib.imsg("simplestream: openwrite {0}", (object) fname);
        }
      }
      catch (Exception ex)
      {
        clib.imsg("ssopen {0} {1}", (object) ex.Message, (object) fname);
        reason = !isread ? "ssopen WRITE " + ex.Message : "ssopen READ " + ex.Message;
      }
      if (this.fs != null)
        this.isopen = true;
      return this.fs != null;
    }

    public bool close()
    {
      if (this.quota_user.Length > 0)
        Quota.add(this.quota_user, this.quota_tot);
      if (this.isopen)
      {
        this.fs.Close();
        this.isopen = false;
        if (this.iswrite)
        {
          try
          {
            if (File.Exists(this.saved_fname))
              File.Delete(this.saved_fname);
            File.Move(this.saved_fname + ".ftpdav_uploading", this.saved_fname);
          }
          catch (Exception ex)
          {
            clib.imsg("simplestream: close exception {0} {1} ", (object) this.saved_fname, (object) ex.Message);
            return false;
          }
        }
      }
      return true;
    }

    public int read(byte[] bf, int offset, int sz)
    {
      return this.fs.Read(bf, offset, sz);
    }

    public int write(byte[] bf, int offset, int sz)
    {
      this.quota_tot += (long) sz;
      this.fs.Write(bf, offset, sz);
      return sz;
    }

    public bool seek(long offset)
    {
      this.fs.Seek(offset, SeekOrigin.Begin);
      return true;
    }
  }
}
