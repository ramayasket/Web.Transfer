// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Server.TlsServerCertificateRequest
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.X509;
using System;

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
  internal class TlsServerCertificateRequest : HandshakeMessage
  {
    public TlsServerCertificateRequest(Context context)
      : base(context, HandshakeType.CertificateRequest)
    {
    }

    protected override void ProcessAsSsl3()
    {
      this.ProcessAsTls1();
    }

    protected override void ProcessAsTls1()
    {
      ServerContext context = (ServerContext) this.Context;
      int length = context.ServerSettings.CertificateTypes.Length;
      this.WriteByte(Convert.ToByte(length));
      for (int index = 0; index < length; ++index)
        this.WriteByte((byte) context.ServerSettings.CertificateTypes[index]);
      if (context.ServerSettings.DistinguisedNames.Length > 0)
      {
        TlsStream tlsStream = new TlsStream();
        foreach (string distinguisedName in context.ServerSettings.DistinguisedNames)
        {
          byte[] bytes = X501.FromString(distinguisedName).GetBytes();
          tlsStream.Write((short) bytes.Length);
          tlsStream.Write(bytes);
        }
        this.Write((short) tlsStream.Length);
        this.Write(tlsStream.ToArray());
      }
      else
        this.Write((short) 0);
    }
  }
}
