// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Server.TlsServerHello
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
  internal class TlsServerHello : HandshakeMessage
  {
    private int unixTime;
    private byte[] random;

    public TlsServerHello(Context context)
      : base(context, HandshakeType.ServerHello)
    {
    }

    public override void Update()
    {
      base.Update();
      TlsStream tlsStream = new TlsStream();
      tlsStream.Write(this.unixTime);
      tlsStream.Write(this.random);
      this.Context.ServerRandom = tlsStream.ToArray();
      tlsStream.Reset();
      tlsStream.Write(this.Context.ClientRandom);
      tlsStream.Write(this.Context.ServerRandom);
      this.Context.RandomCS = tlsStream.ToArray();
      tlsStream.Reset();
      tlsStream.Write(this.Context.ServerRandom);
      tlsStream.Write(this.Context.ClientRandom);
      this.Context.RandomSC = tlsStream.ToArray();
      tlsStream.Reset();
    }

    protected override void ProcessAsSsl3()
    {
      this.ProcessAsTls1();
    }

    protected override void ProcessAsTls1()
    {
      this.Write(this.Context.Protocol);
      this.unixTime = this.Context.GetUnixTime();
      this.Write(this.unixTime);
      this.random = this.Context.GetSecureRandomBytes(28);
      this.Write(this.random);
      if (this.Context.SessionId == null)
      {
        this.WriteByte((byte) 0);
      }
      else
      {
        this.WriteByte((byte) this.Context.SessionId.Length);
        this.Write(this.Context.SessionId);
      }
      this.Write(this.Context.Negotiating.Cipher.Code);
      this.WriteByte((byte) this.Context.CompressionMethod);
    }
  }
}
