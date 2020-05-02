// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Client.TlsServerCertificate
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.X509.Extensions;
using System;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
  internal class TlsServerCertificate : HandshakeMessage
  {
    private Mono.Security.X509.X509CertificateCollection certificates;

    public TlsServerCertificate(Context context, byte[] buffer)
      : base(context, HandshakeType.Certificate, buffer)
    {
    }

    public override void Update()
    {
      base.Update();
      this.Context.ServerSettings.Certificates = this.certificates;
      this.Context.ServerSettings.UpdateCertificateRSA();
    }

    protected override void ProcessAsSsl3()
    {
      this.ProcessAsTls1();
    }

    protected override void ProcessAsTls1()
    {
      this.certificates = new Mono.Security.X509.X509CertificateCollection();
      int num1 = 0;
      int num2 = this.ReadInt24();
      while (num1 < num2)
      {
        int count = this.ReadInt24();
        num1 += 3;
        if (count > 0)
        {
          this.certificates.Add(new Mono.Security.X509.X509Certificate(this.ReadBytes(count)));
          num1 += count;
        }
      }
      this.validateCertificates(this.certificates);
    }

    private bool checkCertificateUsage(Mono.Security.X509.X509Certificate cert)
    {
      ClientContext context = (ClientContext) this.Context;
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
          usage = KeyUsages.keyEncipherment;
          break;
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
      {
        if (!keyUsageExtension1.Support(usage))
          return false;
        return keyUsageExtension2.KeyPurpose.Contains((object) "1.3.6.1.5.5.7.3.1") || keyUsageExtension2.KeyPurpose.Contains((object) "2.16.840.1.113730.4.1");
      }
      if (keyUsageExtension1 != null)
        return keyUsageExtension1.Support(usage);
      if (keyUsageExtension2 != null)
        return keyUsageExtension2.KeyPurpose.Contains((object) "1.3.6.1.5.5.7.3.1") || keyUsageExtension2.KeyPurpose.Contains((object) "2.16.840.1.113730.4.1");
      Mono.Security.X509.X509Extension extension3 = cert.Extensions["2.16.840.1.113730.1.1"];
      return extension3 == null || new NetscapeCertTypeExtension(extension3).Support(NetscapeCertTypeExtension.CertTypes.SslServer);
    }

    private static void VerifyOSX(Mono.Security.X509.X509CertificateCollection certificates)
    {
    }

    private void validateCertificates(Mono.Security.X509.X509CertificateCollection certificates)
    {
      ClientContext context = (ClientContext) this.Context;
      AlertDescription description1 = AlertDescription.BadCertificate;
      if (context.SslStream.HaveRemoteValidation2Callback)
      {
        ValidationResult validationResult = context.SslStream.RaiseServerCertificateValidation2(certificates);
        if (!validationResult.Trusted)
        {
          long errorCode = (long) validationResult.ErrorCode;
          AlertDescription description2;
          switch (errorCode)
          {
            case 2148204801:
              description2 = AlertDescription.CertificateExpired;
              break;
            case 2148204809:
              description2 = AlertDescription.UnknownCA;
              break;
            case 2148204810:
              description2 = AlertDescription.UnknownCA;
              break;
            default:
              description2 = AlertDescription.CertificateUnknown;
              break;
          }
          string str = string.Format("0x{0:x}", (object) errorCode);
          throw new TlsException(description2, "Invalid certificate received from server. Error code: " + str);
        }
      }
      else
      {
        Mono.Security.X509.X509Certificate certificate1 = certificates[0];
        System.Security.Cryptography.X509Certificates.X509Certificate certificate2 = new System.Security.Cryptography.X509Certificates.X509Certificate(certificate1.RawData);
        ArrayList arrayList = new ArrayList();
        if (!this.checkCertificateUsage(certificate1))
          arrayList.Add((object) -2146762490);
        if (!this.checkServerIdentity(certificate1))
          arrayList.Add((object) -2146762481);
        Mono.Security.X509.X509CertificateCollection chain = new Mono.Security.X509.X509CertificateCollection(certificates);
        chain.Remove(certificate1);
        Mono.Security.X509.X509Chain x509Chain = new Mono.Security.X509.X509Chain(chain);
        bool flag;
        try
        {
          flag = x509Chain.Build(certificate1);
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
              description1 = AlertDescription.CertificateExpired;
              arrayList.Add((object) -2146762495);
              break;
            case Mono.Security.X509.X509ChainStatusFlags.NotTimeNested:
              arrayList.Add((object) -2146762494);
              break;
            case Mono.Security.X509.X509ChainStatusFlags.NotSignatureValid:
              arrayList.Add((object) -2146869232);
              break;
            case Mono.Security.X509.X509ChainStatusFlags.UntrustedRoot:
              description1 = AlertDescription.UnknownCA;
              arrayList.Add((object) -2146762487);
              break;
            case Mono.Security.X509.X509ChainStatusFlags.InvalidBasicConstraints:
              arrayList.Add((object) -2146869223);
              break;
            case Mono.Security.X509.X509ChainStatusFlags.PartialChain:
              description1 = AlertDescription.UnknownCA;
              arrayList.Add((object) -2146762486);
              break;
            default:
              description1 = AlertDescription.CertificateUnknown;
              arrayList.Add((object) (int) x509Chain.Status);
              break;
          }
        }
        int[] array = (int[]) arrayList.ToArray(typeof (int));
        if (!context.SslStream.RaiseServerCertificateValidation(certificate2, array))
          throw new TlsException(description1, "Invalid certificate received from server.");
      }
    }

    private bool checkServerIdentity(Mono.Security.X509.X509Certificate cert)
    {
      string targetHost = this.Context.ClientSettings.TargetHost;
      Mono.Security.X509.X509Extension extension = cert.Extensions["2.5.29.17"];
      if (extension != null)
      {
        SubjectAltNameExtension altNameExtension = new SubjectAltNameExtension(extension);
        foreach (string dnsName in altNameExtension.DNSNames)
        {
          if (TlsServerCertificate.Match(targetHost, dnsName))
            return true;
        }
        foreach (string ipAddress in altNameExtension.IPAddresses)
        {
          if (ipAddress == targetHost)
            return true;
        }
      }
      return this.checkDomainName(cert.SubjectName);
    }

    private bool checkDomainName(string subjectName)
    {
      ClientContext context = (ClientContext) this.Context;
      string empty = string.Empty;
      MatchCollection matchCollection = new Regex("CN\\s*=\\s*([^,]*)").Matches(subjectName);
      if (matchCollection.Count == 1 && matchCollection[0].Success)
        empty = matchCollection[0].Groups[1].Value.ToString();
      return TlsServerCertificate.Match(context.ClientSettings.TargetHost, empty);
    }

    private static bool Match(string hostname, string pattern)
    {
      int length = pattern.IndexOf('*');
      if (length == -1)
        return string.Compare(hostname, pattern, true, CultureInfo.InvariantCulture) == 0;
      if (length != pattern.Length - 1 && pattern[length + 1] != '.' || pattern.IndexOf('*', length + 1) != -1)
        return false;
      string strB1 = pattern.Substring(length + 1);
      int indexA = hostname.Length - strB1.Length;
      if (indexA <= 0 || string.Compare(hostname, indexA, strB1, 0, strB1.Length, true, CultureInfo.InvariantCulture) != 0)
        return false;
      if (length == 0)
      {
        int num = hostname.IndexOf('.');
        return num == -1 || num >= hostname.Length - strB1.Length;
      }
      string strB2 = pattern.Substring(0, length);
      return string.Compare(hostname, 0, strB2, 0, strB2.Length, true, CultureInfo.InvariantCulture) == 0;
    }
  }
}
