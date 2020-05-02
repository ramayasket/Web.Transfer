// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Client.TlsClientCertificateVerify
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
  internal class TlsClientCertificateVerify : HandshakeMessage
  {
    public TlsClientCertificateVerify(Context context)
      : base(context, HandshakeType.CertificateVerify)
    {
    }

    public override void Update()
    {
      base.Update();
      this.Reset();
    }

    protected override void ProcessAsSsl3()
    {
      ClientContext context = (ClientContext) this.Context;
      AsymmetricAlgorithm asymmetricAlgorithm = context.SslStream.RaisePrivateKeySelection(context.ClientSettings.ClientCertificate, context.ClientSettings.TargetHost);
      if (asymmetricAlgorithm == null)
        throw new TlsException(AlertDescription.UserCancelled, "Client certificate Private Key unavailable.");
      SslHandshakeHash sslHandshakeHash = new SslHandshakeHash(context.MasterSecret);
      sslHandshakeHash.TransformFinalBlock(context.HandshakeMessages.ToArray(), 0, (int) context.HandshakeMessages.Length);
      byte[] buffer = (byte[]) null;
      if (!(asymmetricAlgorithm is RSACryptoServiceProvider))
      {
        try
        {
          buffer = sslHandshakeHash.CreateSignature((RSA) asymmetricAlgorithm);
        }
        catch (NotImplementedException)
        {
        }
      }
      if (buffer == null)
      {
        RSA clientCertRsa = this.getClientCertRSA((RSA) asymmetricAlgorithm);
        buffer = sslHandshakeHash.CreateSignature(clientCertRsa);
      }
      this.Write((short) buffer.Length);
      this.Write(buffer, 0, buffer.Length);
    }

    protected override void ProcessAsTls1()
    {
      ClientContext context = (ClientContext) this.Context;
      AsymmetricAlgorithm asymmetricAlgorithm = context.SslStream.RaisePrivateKeySelection(context.ClientSettings.ClientCertificate, context.ClientSettings.TargetHost);
      if (asymmetricAlgorithm == null)
        throw new TlsException(AlertDescription.UserCancelled, "Client certificate Private Key unavailable.");
      MD5SHA1 md5ShA1 = new MD5SHA1();
      md5ShA1.ComputeHash(context.HandshakeMessages.ToArray(), 0, (int) context.HandshakeMessages.Length);
      byte[] buffer = (byte[]) null;
      if (!(asymmetricAlgorithm is RSACryptoServiceProvider))
      {
        try
        {
          buffer = md5ShA1.CreateSignature((RSA) asymmetricAlgorithm);
        }
        catch (NotImplementedException)
        {
        }
      }
      if (buffer == null)
      {
        RSA clientCertRsa = this.getClientCertRSA((RSA) asymmetricAlgorithm);
        buffer = md5ShA1.CreateSignature(clientCertRsa);
      }
      this.Write((short) buffer.Length);
      this.Write(buffer, 0, buffer.Length);
    }

    private RSA getClientCertRSA(RSA privKey)
    {
      RSAParameters parameters = new RSAParameters();
      RSAParameters rsaParameters = privKey.ExportParameters(true);
      ASN1 asN1_1 = new ASN1(this.Context.ClientSettings.Certificates[0].GetPublicKey());
      ASN1 asN1_2 = asN1_1[0];
      if (asN1_2 == null || asN1_2.Tag != (byte) 2)
        return (RSA) null;
      ASN1 asN1_3 = asN1_1[1];
      if (asN1_3.Tag != (byte) 2)
        return (RSA) null;
      parameters.Modulus = this.getUnsignedBigInteger(asN1_2.Value);
      parameters.Exponent = asN1_3.Value;
      parameters.D = rsaParameters.D;
      parameters.DP = rsaParameters.DP;
      parameters.DQ = rsaParameters.DQ;
      parameters.InverseQ = rsaParameters.InverseQ;
      parameters.P = rsaParameters.P;
      parameters.Q = rsaParameters.Q;
      RSAManaged rsaManaged = new RSAManaged(parameters.Modulus.Length << 3);
      rsaManaged.ImportParameters(parameters);
      return (RSA) rsaManaged;
    }

    private byte[] getUnsignedBigInteger(byte[] integer)
    {
      if (integer[0] != (byte) 0)
        return integer;
      int count = integer.Length - 1;
      byte[] numArray = new byte[count];
      Buffer.BlockCopy((Array) integer, 1, (Array) numArray, 0, count);
      return numArray;
    }
  }
}
