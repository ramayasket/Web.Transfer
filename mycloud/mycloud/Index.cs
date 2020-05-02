// Decompiled with JetBrains decompiler
// Type: mycloud.Index
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace mycloud
{
  public class Index : IEnumerable
  {
    private List<Fileinfo> all = new List<Fileinfo>();

    public IEnumerator GetEnumerator()
    {
      foreach (Fileinfo fileinfo in this.all)
        yield return (object) fileinfo;
    }

    public void add(Fileinfo f)
    {
      if (f == null)
        return;
      clib.imsg("Files: index_add {0}", (object) f.name);
      f.name = Files.fname_decode(f.name);
      f.name = Files.tild_decode(f.name);
      clib.imsg("Files: index_add_decoded {0}", (object) f.name);
      this.all.Add(f);
    }

    public int count()
    {
      return this.all.Count<Fileinfo>();
    }
  }
}
