// Decompiled with JetBrains decompiler
// Type: mycloud.WebModule
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;

namespace mycloud
{
  public abstract class WebModule : ICloneable
  {
    public virtual object Clone()
    {
      throw new Exception("Derived clases must override Clone method.");
    }

    public abstract bool isforme(string pathonly, string url);

    public abstract void data_in(byte[] inbf, int inlen);

    public abstract bool do_headers(Websvc w);

    public abstract bool do_body(Websvc w, byte[] inbf, int len);

    public abstract bool do_body_end(Websvc w);

    public abstract string myname();

    public abstract void dropconnection(Websvc w);
  }
}
