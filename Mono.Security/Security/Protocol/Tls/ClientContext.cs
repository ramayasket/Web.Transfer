// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.ClientContext
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Protocol.Tls
{
  internal class ClientContext : Context
  {
    private SslClientStream sslStream;
    private short clientHelloProtocol;

    public ClientContext(
      SslClientStream stream,
      SecurityProtocolType securityProtocolType,
      string targetHost,
      X509CertificateCollection clientCertificates)
      : base(securityProtocolType)
    {
      this.sslStream = stream;
      this.ClientSettings.Certificates = clientCertificates;
      this.ClientSettings.TargetHost = targetHost;
    }

    public SslClientStream SslStream
    {
      get
      {
        return this.sslStream;
      }
    }

    public short ClientHelloProtocol
    {
      get
      {
        return this.clientHelloProtocol;
      }
      set
      {
        this.clientHelloProtocol = value;
      }
    }

    public override void Clear()
    {
      this.clientHelloProtocol = (short) 0;
      base.Clear();
    }
  }
}
