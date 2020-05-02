// Decompiled with JetBrains decompiler
// Type: mycloud.VPath
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;

namespace mycloud
{
  internal class VPath
  {
    public string alias;
    public string path;
    public string access;

    public bool isreadonly()
    {
      return this.access == "readonly";
    }

    public bool isupload()
    {
      return this.access == "upload";
    }

    public bool isfull()
    {
      return this.access == "full";
    }

    public VPath(string alias, string path, string access)
    {
      this.alias = alias;
      this.path = path;
      this.access = access;
    }

    public override string ToString()
    {
      return string.Format("alias={0}~path={1}~access={2}", (object) clib.simple_encode(this.alias), (object) clib.simple_encode(this.path), (object) this.access);
    }
  }
}
