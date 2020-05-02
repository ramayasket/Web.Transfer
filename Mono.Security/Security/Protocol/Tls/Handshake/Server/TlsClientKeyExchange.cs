// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Server.TlsClientKeyExchange
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
  internal class TlsClientKeyExchange : HandshakeMessage
  {
    public TlsClientKeyExchange(Context context, byte[] buffer)
      : base(context, HandshakeType.ClientKeyExchange, buffer)
    {
    }

    protected override void ProcessAsSsl3()
    {
      ServerContext context = (ServerContext) this.Context;
      AsymmetricAlgorithm key = context.SslStream.RaisePrivateKeySelection(new X509Certificate(context.ServerSettings.Certificates[0].RawData), (string) null);
      if (key == null)
        throw new TlsException(AlertDescription.UserCancelled, "Server certificate Private Key unavailable.");
      byte[] rgb = this.ReadBytes((int) this.Length);
      this.Context.Negotiating.Cipher.ComputeMasterSecret(new RSAPKCS1KeyExchangeDeformatter(key).DecryptKeyExchange(rgb));
      this.Context.Negotiating.Cipher.ComputeKeys();
      this.Context.Negotiating.Cipher.InitializeCipher();
    }

    protected override void ProcessAsTls1()
    {
      ServerContext context = (ServerContext) this.Context;
      AsymmetricAlgorithm key = context.SslStream.RaisePrivateKeySelection(new X509Certificate(context.ServerSettings.Certificates[0].RawData), (string) null);
      if (key == null)
        throw new TlsException(AlertDescription.UserCancelled, "Server certificate Private Key unavailable.");
      byte[] rgb = this.ReadBytes((int) this.ReadInt16());
      this.Context.Negotiating.Cipher.ComputeMasterSecret(new RSAPKCS1KeyExchangeDeformatter(key).DecryptKeyExchange(rgb));
      this.Context.Negotiating.Cipher.ComputeKeys();
      this.Context.Negotiating.Cipher.InitializeCipher();
    }
  }
}
