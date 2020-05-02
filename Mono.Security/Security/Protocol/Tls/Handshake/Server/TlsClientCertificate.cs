// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Server.TlsClientCertificate
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.X509.Extensions;
using System;
using System.Collections;

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
  internal class TlsClientCertificate : HandshakeMessage
  {
    private Mono.Security.X509.X509CertificateCollection clientCertificates;

    public TlsClientCertificate(Context context, byte[] buffer)
      : base(context, HandshakeType.Certificate, buffer)
    {
    }

    public override void Update()
    {
      foreach (Mono.Security.X509.X509Certificate clientCertificate in this.clientCertificates)
        this.Context.ClientSettings.Certificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate(clientCertificate.RawData));
    }

    protected override void ProcessAsSsl3()
    {
      this.ProcessAsTls1();
    }

    protected override void ProcessAsTls1()
    {
      int num1 = 0;
      int num2 = this.ReadInt24();
      this.clientCertificates = new Mono.Security.X509.X509CertificateCollection();
      while (num2 > num1)
      {
        int count = this.ReadInt24();
        num1 += count + 3;
        this.clientCertificates.Add(new Mono.Security.X509.X509Certificate(this.ReadBytes(count)));
      }
      if (this.clientCertificates.Count > 0)
        this.validateCertificates(this.clientCertificates);
      else if ((this.Context as ServerContext).ClientCertificateRequired)
        throw new TlsException(AlertDescription.NoCertificate);
    }

    private bool checkCertificateUsage(Mono.Security.X509.X509Certificate cert)
    {
      ServerContext context = (ServerContext) this.Context;
      if (cert.Version < 3)
        return true;
      KeyUsages usage = KeyUsages.none;
      switch (context.Negotiating.Cipher.ExchangeAlgorithmType)
      {
        case ExchangeAlgorithmType.DiffieHellman:
          usage = KeyUsages.keyAgreement;
          break;
        case ExchangeAlgorithmType.Fortezza:
          return false;
        case ExchangeAlgorithmType.RsaKeyX:
        case ExchangeAlgorithmType.RsaSign:
          usage = KeyUsages.digitalSignature;
          break;
      }
      KeyUsageExtension keyUsageExtension1 = (KeyUsageExtension) null;
      ExtendedKeyUsageExtension keyUsageExtension2 = (ExtendedKeyUsageExtension) null;
      Mono.Security.X509.X509Extension extension1 = cert.Extensions["2.5.29.15"];
      if (extension1 != null)
        keyUsageExtension1 = new KeyUsageExtension(extension1);
      Mono.Security.X509.X509Extension extension2 = cert.Extensions["2.5.29.37"];
      if (extension2 != null)
        keyUsageExtension2 = new ExtendedKeyUsageExtension(extension2);
      if (keyUsageExtension1 != null && keyUsageExtension2 != null)
        return keyUsageExtension1.Support(usage) && keyUsageExtension2.KeyPurpose.Contains((object) "1.3.6.1.5.5.7.3.2");
      if (keyUsageExtension1 != null)
        return keyUsageExtension1.Support(usage);
      if (keyUsageExtension2 != null)
        return keyUsageExtension2.KeyPurpose.Contains((object) "1.3.6.1.5.5.7.3.2");
      Mono.Security.X509.X509Extension extension3 = cert.Extensions["2.16.840.1.113730.1.1"];
      return extension3 != null && new NetscapeCertTypeExtension(extension3).Support(NetscapeCertTypeExtension.CertTypes.SslClient);
    }

    private void validateCertificates(Mono.Security.X509.X509CertificateCollection certificates)
    {
      ServerContext context = (ServerContext) this.Context;
      AlertDescription description = AlertDescription.BadCertificate;
      System.Security.Cryptography.X509Certificates.X509Certificate certificate1 = (System.Security.Cryptography.X509Certificates.X509Certificate) null;
      int[] certificateErrors;
      if (certificates.Count > 0)
      {
        Mono.Security.X509.X509Certificate certificate2 = certificates[0];
        ArrayList arrayList = new ArrayList();
        if (!this.checkCertificateUsage(certificate2))
          arrayList.Add((object) -2146762490);
        Mono.Security.X509.X509Chain x509Chain;
        if (certificates.Count > 1)
        {
          Mono.Security.X509.X509CertificateCollection chain = new Mono.Security.X509.X509CertificateCollection(certificates);
          chain.Remove(certificate2);
          x509Chain = new Mono.Security.X509.X509Chain(chain);
        }
        else
          x509Chain = new Mono.Security.X509.X509Chain();
        bool flag;
        try
        {
          flag = x509Chain.Build(certificate2);
        }
        catch (Exception)
        {
          flag = false;
        }
        if (!flag)
        {
          switch (x509Chain.Status)
          {
            case Mono.Security.X509.X509ChainStatusFlags.NotTimeValid:
              description = AlertDescription.CertificateExpired;
              arrayList.Add((object) -2146762495);
              break;
            case Mono.Security.X509.X509ChainStatusFlags.NotTimeNested:
              arrayList.Add((object) -2146762494);
              break;
            case Mono.Security.X509.X509ChainStatusFlags.NotSignatureValid:
              arrayList.Add((object) -2146869232);
              break;
            case Mono.Security.X509.X509ChainStatusFlags.UntrustedRoot:
              description = AlertDescription.UnknownCA;
              arrayList.Add((object) -2146762487);
              break;
            case Mono.Security.X509.X509ChainStatusFlags.InvalidBasicConstraints:
              arrayList.Add((object) -2146869223);
              break;
            case Mono.Security.X509.X509ChainStatusFlags.PartialChain:
              description = AlertDescription.UnknownCA;
              arrayList.Add((object) -2146762486);
              break;
            default:
              description = AlertDescription.CertificateUnknown;
              arrayList.Add((object) (int) x509Chain.Status);
              break;
          }
        }
        certificate1 = new System.Security.Cryptography.X509Certificates.X509Certificate(certificate2.RawData);
        certificateErrors = (int[]) arrayList.ToArray(typeof (int));
      }
      else
        certificateErrors = new int[0];
      System.Security.Cryptography.X509Certificates.X509CertificateCollection certificateCollection = new System.Security.Cryptography.X509Certificates.X509CertificateCollection();
      foreach (Mono.Security.X509.X509Certificate certificate2 in certificates)
        certificateCollection.Add(new System.Security.Cryptography.X509Certificates.X509Certificate(certificate2.RawData));
      if (!context.SslStream.RaiseClientCertificateValidation(certificate1, certificateErrors))
        throw new TlsException(description, "Invalid certificate received from client.");
      this.Context.ClientSettings.ClientCertificate = certificate1;
    }
  }
}
