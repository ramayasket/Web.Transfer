// Decompiled with JetBrains decompiler
// Type: mycloud.IfItem
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System.Collections.Generic;

namespace mycloud
{
  internal class IfItem
  {
    public string path;
    public List<Ething> etags;

    public IfItem(string path, List<Ething> etags)
    {
      this.path = path;
      this.etags = etags;
    }
  }
}
