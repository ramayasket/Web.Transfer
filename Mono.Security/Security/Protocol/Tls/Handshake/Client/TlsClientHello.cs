// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Client.TlsClientHello
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
  internal class TlsClientHello : HandshakeMessage
  {
    private byte[] random;

    public TlsClientHello(Context context)
      : base(context, HandshakeType.ClientHello)
    {
    }

    public override void Update()
    {
      ClientContext context = (ClientContext) this.Context;
      base.Update();
      context.ClientRandom = this.random;
      context.ClientHelloProtocol = this.Context.Protocol;
      this.random = (byte[]) null;
    }

    protected override void ProcessAsSsl3()
    {
      this.ProcessAsTls1();
    }

    protected override void ProcessAsTls1()
    {
      this.Write(this.Context.Protocol);
      TlsStream tlsStream = new TlsStream();
      tlsStream.Write(this.Context.GetUnixTime());
      tlsStream.Write(this.Context.GetSecureRandomBytes(28));
      this.random = tlsStream.ToArray();
      tlsStream.Reset();
      this.Write(this.random);
      this.Context.SessionId = ClientSessionCache.FromHost(this.Context.ClientSettings.TargetHost);
      if (this.Context.SessionId != null)
      {
        this.Write((byte) this.Context.SessionId.Length);
        if (this.Context.SessionId.Length > 0)
          this.Write(this.Context.SessionId);
      }
      else
        this.Write((byte) 0);
      this.Write((short) (this.Context.SupportedCiphers.Count * 2));
      for (int index = 0; index < this.Context.SupportedCiphers.Count; ++index)
        this.Write(this.Context.SupportedCiphers[index].Code);
      this.Write((byte) 1);
      this.Write((byte) this.Context.CompressionMethod);
    }
  }
}
