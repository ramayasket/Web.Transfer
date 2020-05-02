// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Client.TlsServerKeyExchange
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
  internal class TlsServerKeyExchange : HandshakeMessage
  {
    private RSAParameters rsaParams;
    private byte[] signedParams;

    public TlsServerKeyExchange(Context context, byte[] buffer)
      : base(context, HandshakeType.ServerKeyExchange, buffer)
    {
      this.verifySignature();
    }

    public override void Update()
    {
      base.Update();
      this.Context.ServerSettings.ServerKeyExchange = true;
      this.Context.ServerSettings.RsaParameters = this.rsaParams;
      this.Context.ServerSettings.SignedParams = this.signedParams;
    }

    protected override void ProcessAsSsl3()
    {
      this.ProcessAsTls1();
    }

    protected override void ProcessAsTls1()
    {
      this.rsaParams = new RSAParameters();
      this.rsaParams.Modulus = this.ReadBytes((int) this.ReadInt16());
      this.rsaParams.Exponent = this.ReadBytes((int) this.ReadInt16());
      this.signedParams = this.ReadBytes((int) this.ReadInt16());
    }

    private void verifySignature()
    {
      MD5SHA1 md5ShA1 = new MD5SHA1();
      int count = this.rsaParams.Modulus.Length + this.rsaParams.Exponent.Length + 4;
      TlsStream tlsStream = new TlsStream();
      tlsStream.Write(this.Context.RandomCS);
      tlsStream.Write(this.ToArray(), 0, count);
      md5ShA1.ComputeHash(tlsStream.ToArray());
      tlsStream.Reset();
      if (!md5ShA1.VerifySignature(this.Context.ServerSettings.CertificateRSA, this.signedParams))
        throw new TlsException(AlertDescription.DecodeError, "Data was not signed with the server certificate.");
    }
  }
}
