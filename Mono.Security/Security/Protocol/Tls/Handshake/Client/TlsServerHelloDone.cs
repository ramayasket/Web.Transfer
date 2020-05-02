// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Client.TlsServerHelloDone
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
  internal class TlsServerHelloDone : HandshakeMessage
  {
    public TlsServerHelloDone(Context context, byte[] buffer)
      : base(context, HandshakeType.ServerHelloDone, buffer)
    {
    }

    protected override void ProcessAsSsl3()
    {
    }

    protected override void ProcessAsTls1()
    {
    }
  }
}
