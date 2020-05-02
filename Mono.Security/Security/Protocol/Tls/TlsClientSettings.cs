// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.TlsClientSettings
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;

namespace Mono.Security.Protocol.Tls
{
  internal sealed class TlsClientSettings
  {
    private string targetHost;
    private System.Security.Cryptography.X509Certificates.X509CertificateCollection certificates;
    private System.Security.Cryptography.X509Certificates.X509Certificate clientCertificate;
    private RSAManaged certificateRSA;

    public TlsClientSettings()
    {
      this.certificates = new System.Security.Cryptography.X509Certificates.X509CertificateCollection();
      this.targetHost = string.Empty;
    }

    public string TargetHost
    {
      get
      {
        return this.targetHost;
      }
      set
      {
        this.targetHost = value;
      }
    }

    public System.Security.Cryptography.X509Certificates.X509CertificateCollection Certificates
    {
      get
      {
        return this.certificates;
      }
      set
      {
        this.certificates = value;
      }
    }

    public System.Security.Cryptography.X509Certificates.X509Certificate ClientCertificate
    {
      get
      {
        return this.clientCertificate;
      }
      set
      {
        this.clientCertificate = value;
        this.UpdateCertificateRSA();
      }
    }

    public RSAManaged CertificateRSA
    {
      get
      {
        return this.certificateRSA;
      }
    }

    public void UpdateCertificateRSA()
    {
      if (this.clientCertificate == null)
      {
        this.certificateRSA = (RSAManaged) null;
      }
      else
      {
        Mono.Security.X509.X509Certificate x509Certificate = new Mono.Security.X509.X509Certificate(this.clientCertificate.GetRawCertData());
        this.certificateRSA = new RSAManaged(x509Certificate.RSA.KeySize);
        this.certificateRSA.ImportParameters(x509Certificate.RSA.ExportParameters(false));
      }
    }
  }
}
