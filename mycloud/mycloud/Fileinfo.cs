// Decompiled with JetBrains decompiler
// Type: mycloud.Fileinfo
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System;
using System.IO;

namespace mycloud
{
  public class Fileinfo
  {
    public long size;
    public bool isdir;
    public DateTime modified;
    public DateTime created;
    public string name;

    public string etag()
    {
      return string.Format("W/\"{0}\"", (object) this.modified.to_unix_date());
    }

    public Fileinfo(string name, FileInfo ff)
    {
      this.Init(name, ff.Length, ff.CreationTime, ff.LastWriteTime, (ff.Attributes & FileAttributes.Directory) == FileAttributes.Directory);
    }

    public Fileinfo(string name, long size, DateTime created, DateTime modified, bool isdir)
    {
      this.Init(name, size, created, modified, isdir);
    }

    private void Init(string name, long size, DateTime created, DateTime modified, bool isdir)
    {
      this.name = name;
      this.size = size;
      this.modified = modified;
      this.created = created;
      this.isdir = isdir;
    }
  }
}
