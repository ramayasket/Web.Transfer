// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Server.TlsClientCertificateVerify
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
  internal class TlsClientCertificateVerify : HandshakeMessage
  {
    public TlsClientCertificateVerify(Context context, byte[] buffer)
      : base(context, HandshakeType.CertificateVerify, buffer)
    {
    }

    protected override void ProcessAsSsl3()
    {
      ServerContext context = (ServerContext) this.Context;
      byte[] rgbSignature = this.ReadBytes((int) this.ReadInt16());
      SslHandshakeHash sslHandshakeHash = new SslHandshakeHash(context.MasterSecret);
      sslHandshakeHash.TransformFinalBlock(context.HandshakeMessages.ToArray(), 0, (int) context.HandshakeMessages.Length);
      if (!sslHandshakeHash.VerifySignature((RSA) context.ClientSettings.CertificateRSA, rgbSignature))
        throw new TlsException(AlertDescription.HandshakeFailiure, "Handshake Failure.");
    }

    protected override void ProcessAsTls1()
    {
      ServerContext context = (ServerContext) this.Context;
      byte[] rgbSignature = this.ReadBytes((int) this.ReadInt16());
      MD5SHA1 md5ShA1 = new MD5SHA1();
      md5ShA1.ComputeHash(context.HandshakeMessages.ToArray(), 0, (int) context.HandshakeMessages.Length);
      if (!md5ShA1.VerifySignature((RSA) context.ClientSettings.CertificateRSA, rgbSignature))
        throw new TlsException(AlertDescription.HandshakeFailiure, "Handshake Failure.");
    }
  }
}
