// Decompiled with JetBrains decompiler
// Type: Mylib.Attachments
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System.Collections.Generic;
using System.Linq;

namespace Mylib
{
  public class Attachments
  {
    private List<Attachment> all = new List<Attachment>();

    public int Count()
    {
      return this.all.Count<Attachment>();
    }

    public void Add(string n, string p, string t)
    {
      this.all.Add(new Attachment(n, p, t));
    }

    public string find(string n)
    {
      foreach (Attachment attachment in this.all)
      {
        if (attachment.name == n)
          return attachment.path;
      }
      return "";
    }

    public Attachment get(int i)
    {
      return this.all[i];
    }
  }
}
