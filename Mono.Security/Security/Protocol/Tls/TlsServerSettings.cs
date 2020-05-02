// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.TlsServerSettings
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using Mono.Security.Protocol.Tls.Handshake;
using Mono.Security.X509;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls
{
  internal class TlsServerSettings
  {
    private X509CertificateCollection certificates;
    private RSA certificateRSA;
    private RSAParameters rsaParameters;
    private byte[] signedParams;
    private string[] distinguisedNames;
    private bool serverKeyExchange;
    private bool certificateRequest;
    private ClientCertificateType[] certificateTypes;

    public bool ServerKeyExchange
    {
      get
      {
        return this.serverKeyExchange;
      }
      set
      {
        this.serverKeyExchange = value;
      }
    }

    public X509CertificateCollection Certificates
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

    public RSA CertificateRSA
    {
      get
      {
        return this.certificateRSA;
      }
    }

    public RSAParameters RsaParameters
    {
      get
      {
        return this.rsaParameters;
      }
      set
      {
        this.rsaParameters = value;
      }
    }

    public byte[] SignedParams
    {
      get
      {
        return this.signedParams;
      }
      set
      {
        this.signedParams = value;
      }
    }

    public bool CertificateRequest
    {
      get
      {
        return this.certificateRequest;
      }
      set
      {
        this.certificateRequest = value;
      }
    }

    public ClientCertificateType[] CertificateTypes
    {
      get
      {
        return this.certificateTypes;
      }
      set
      {
        this.certificateTypes = value;
      }
    }

    public string[] DistinguisedNames
    {
      get
      {
        return this.distinguisedNames;
      }
      set
      {
        this.distinguisedNames = value;
      }
    }

    public void UpdateCertificateRSA()
    {
      if (this.certificates == null || this.certificates.Count == 0)
      {
        this.certificateRSA = (RSA) null;
      }
      else
      {
        this.certificateRSA = (RSA) new RSAManaged(this.certificates[0].RSA.KeySize);
        this.certificateRSA.ImportParameters(this.certificates[0].RSA.ExportParameters(false));
      }
    }
  }
}
