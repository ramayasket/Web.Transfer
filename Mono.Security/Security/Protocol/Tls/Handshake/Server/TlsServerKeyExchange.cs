// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Server.TlsServerKeyExchange
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
  internal class TlsServerKeyExchange : HandshakeMessage
  {
    public TlsServerKeyExchange(Context context)
      : base(context, HandshakeType.ServerKeyExchange)
    {
    }

    public override void Update()
    {
      throw new NotSupportedException();
    }

    protected override void ProcessAsSsl3()
    {
      this.ProcessAsTls1();
    }

    protected override void ProcessAsTls1()
    {
      ServerContext context = (ServerContext) this.Context;
      RSA rsa = (RSA) context.SslStream.PrivateKeyCertSelectionDelegate(new X509Certificate(context.ServerSettings.Certificates[0].RawData), (string) null);
      RSAParameters rsaParameters = rsa.ExportParameters(false);
      this.WriteInt24(rsaParameters.Modulus.Length);
      this.Write(rsaParameters.Modulus, 0, rsaParameters.Modulus.Length);
      this.WriteInt24(rsaParameters.Exponent.Length);
      this.Write(rsaParameters.Exponent, 0, rsaParameters.Exponent.Length);
      byte[] signature = this.createSignature(rsa, this.ToArray());
      this.WriteInt24(signature.Length);
      this.Write(signature);
    }

    private byte[] createSignature(RSA rsa, byte[] buffer)
    {
      MD5SHA1 md5ShA1 = new MD5SHA1();
      TlsStream tlsStream = new TlsStream();
      tlsStream.Write(this.Context.RandomCS);
      tlsStream.Write(buffer, 0, buffer.Length);
      md5ShA1.ComputeHash(tlsStream.ToArray());
      tlsStream.Reset();
      return md5ShA1.CreateSignature(rsa);
    }
  }
}
