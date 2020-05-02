// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Server.TlsServerCertificate
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.X509;
using System;

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
  internal class TlsServerCertificate : HandshakeMessage
  {
    public TlsServerCertificate(Context context)
      : base(context, HandshakeType.Certificate)
    {
    }

    protected override void ProcessAsSsl3()
    {
      this.ProcessAsTls1();
    }

    protected override void ProcessAsTls1()
    {
      TlsStream tlsStream = new TlsStream();
      foreach (X509Certificate certificate in this.Context.ServerSettings.Certificates)
      {
        tlsStream.WriteInt24(certificate.RawData.Length);
        tlsStream.Write(certificate.RawData);
      }
      this.WriteInt24(Convert.ToInt32(tlsStream.Length));
      this.Write(tlsStream.ToArray());
      tlsStream.Close();
    }
  }
}
