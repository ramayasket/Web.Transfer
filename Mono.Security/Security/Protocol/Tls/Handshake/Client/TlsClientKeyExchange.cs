// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Client.TlsClientKeyExchange
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
  internal class TlsClientKeyExchange : HandshakeMessage
  {
    public TlsClientKeyExchange(Context context)
      : base(context, HandshakeType.ClientKeyExchange)
    {
    }

    protected override void ProcessAsSsl3()
    {
      this.ProcessCommon(false);
    }

    protected override void ProcessAsTls1()
    {
      this.ProcessCommon(true);
    }

    public void ProcessCommon(bool sendLength)
    {
      byte[] premasterSecret = this.Context.Negotiating.Cipher.CreatePremasterSecret();
      RSA rsa;
      if (this.Context.ServerSettings.ServerKeyExchange)
      {
        rsa = (RSA) new RSAManaged();
        rsa.ImportParameters(this.Context.ServerSettings.RsaParameters);
      }
      else
        rsa = this.Context.ServerSettings.CertificateRSA;
      byte[] keyExchange = new RSAPKCS1KeyExchangeFormatter((AsymmetricAlgorithm) rsa).CreateKeyExchange(premasterSecret);
      if (sendLength)
        this.Write((short) keyExchange.Length);
      this.Write(keyExchange);
      this.Context.Negotiating.Cipher.ComputeMasterSecret(premasterSecret);
      this.Context.Negotiating.Cipher.ComputeKeys();
      rsa.Clear();
    }
  }
}
