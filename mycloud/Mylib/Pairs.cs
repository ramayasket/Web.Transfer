// Decompiled with JetBrains decompiler
// Type: Mylib.Pairs
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.Collections.Generic;
using System.Linq;

namespace Mylib
{
  [Serializable]
  public class Pairs
  {
    private Dictionary<string, string> d = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);

    public void clear()
    {
      this.d.Clear();
    }

    public bool TryGetValue(string a, out string b)
    {
      return this.d.TryGetValue(a, out b);
    }

    public void Remove(string a)
    {
      this.d.Remove(a);
    }

    public void Add(string a, string b)
    {
      this.d.Add(a, b);
    }

    public int Count()
    {
      return this.d.Count<KeyValuePair<string, string>>();
    }

    public void get(int i, out string a, out string b)
    {
      int num = 0;
      foreach (KeyValuePair<string, string> keyValuePair in this.d)
      {
        if (i == num)
        {
          a = keyValuePair.Key;
          b = keyValuePair.Value;
          return;
        }
        ++num;
      }
      a = b = "";
    }

    public string find(string a)
    {
      string b;
      this.TryGetValue(a, out b);
      return b;
    }
  }
}
