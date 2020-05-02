// Decompiled with JetBrains decompiler
// Type: Mylib.MyBuffer
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System;

namespace Mylib
{
  public class MyBuffer
  {
    private int bfsz = 1000;
    public byte[] inbf;
    private int inlen;
    private int upto;
    private int allocated;

    public int Length
    {
      get
      {
        return this.inlen - this.upto;
      }
    }

    private void shuffle()
    {
      Array.Copy((Array) this.inbf, this.upto, (Array) this.inbf, 0, this.inlen - this.upto);
      this.inlen -= this.upto;
      this.upto = 0;
    }

    private void grow(int sz)
    {
      if (sz <= this.allocated)
        return;
      if (this.upto > 0)
        this.shuffle();
      if (sz > this.allocated)
      {
        int length = sz + this.bfsz;
        byte[] numArray = new byte[length];
        if (this.inlen > 0)
          Array.Copy((Array) this.inbf, (Array) numArray, this.inlen);
        this.inbf = numArray;
        this.allocated = length;
      }
    }

    public void add(byte[] bf, int len)
    {
      if (len < 1)
        return;
      this.grow(this.inlen + len);
      Array.Copy((Array) bf, 0, (Array) this.inbf, this.inlen, len);
      this.inlen += len;
    }

    public byte[] getline()
    {
      int num = clib.IndexOf(this.inbf, "\r\n", this.upto, this.inlen);
      if (num < 0)
        return (byte[]) null;
      int length = num - this.upto;
      byte[] numArray = new byte[length];
      if (length > 0)
        Array.Copy((Array) this.inbf, this.upto, (Array) numArray, 0, length);
      this.upto += length + 2;
      return numArray;
    }
  }
}
