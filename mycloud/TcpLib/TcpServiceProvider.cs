// Decompiled with JetBrains decompiler
// Type: TcpLib.TcpServiceProvider
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;

namespace TcpLib
{
  public abstract class TcpServiceProvider : ICloneable
  {
    public virtual object Clone()
    {
      throw new Exception("Derived clases must override Clone method.");
    }

    public abstract void OnAcceptConnection(ConnectionState state);

    public abstract int get_timeout();

    public abstract void OnReceiveData(ConnectionState state);

    public abstract void OnDropConnection(ConnectionState state);
  }
}
