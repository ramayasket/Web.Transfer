// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.ServerContext
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Protocol.Tls.Handshake;
using Mono.Security.X509;

namespace Mono.Security.Protocol.Tls
{
  internal class ServerContext : Context
  {
    private SslServerStream sslStream;
    private bool request_client_certificate;
    private bool clientCertificateRequired;

    public ServerContext(
      SslServerStream stream,
      SecurityProtocolType securityProtocolType,
      System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate,
      bool clientCertificateRequired,
      bool requestClientCertificate)
      : base(securityProtocolType)
    {
      this.sslStream = stream;
      this.clientCertificateRequired = clientCertificateRequired;
      this.request_client_certificate = requestClientCertificate;
      Mono.Security.X509.X509Certificate x509Certificate1 = new Mono.Security.X509.X509Certificate(serverCertificate.GetRawCertData());
      this.ServerSettings.Certificates = new Mono.Security.X509.X509CertificateCollection();
      this.ServerSettings.Certificates.Add(x509Certificate1);
      this.ServerSettings.UpdateCertificateRSA();
      this.ServerSettings.CertificateTypes = new ClientCertificateType[1];
      this.ServerSettings.CertificateTypes[0] = ClientCertificateType.RSA;
      Mono.Security.X509.X509CertificateCollection rootCertificates = X509StoreManager.TrustedRootCertificates;
      string[] strArray = new string[rootCertificates.Count];
      int num = 0;
      foreach (Mono.Security.X509.X509Certificate x509Certificate2 in rootCertificates)
        strArray[num++] = x509Certificate2.IssuerName;
      this.ServerSettings.DistinguisedNames = strArray;
    }

    public SslServerStream SslStream
    {
      get
      {
        return this.sslStream;
      }
    }

    public bool ClientCertificateRequired
    {
      get
      {
        return this.clientCertificateRequired;
      }
    }

    public bool RequestClientCertificate
    {
      get
      {
        return this.request_client_certificate;
      }
    }
  }
}
