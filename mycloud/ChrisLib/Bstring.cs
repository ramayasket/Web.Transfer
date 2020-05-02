// Decompiled with JetBrains decompiler
// Type: ChrisLib.Bstring
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

namespace ChrisLib
{
  internal class Bstring
  {
    public byte[] bf = new byte[1000];
    public int len;

    public void concat(string x)
    {
      this.concat(x.tobyte());
    }

    public void concat(byte[] x)
    {
      foreach (byte num in x)
        this.bf[this.len++] = num;
    }
  }
}
