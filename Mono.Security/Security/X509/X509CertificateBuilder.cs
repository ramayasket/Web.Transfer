// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.X509CertificateBuilder
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Security.Cryptography;

namespace Mono.Security.X509
{
  public class X509CertificateBuilder : X509Builder
  {
    private byte version;
    private byte[] sn;
    private string issuer;
    private DateTime notBefore;
    private DateTime notAfter;
    private string subject;
    private AsymmetricAlgorithm aa;
    private byte[] issuerUniqueID;
    private byte[] subjectUniqueID;
    private X509ExtensionCollection extensions;

    public X509CertificateBuilder()
      : this((byte) 3)
    {
    }

    public X509CertificateBuilder(byte version)
    {
      if (version > (byte) 3)
        throw new ArgumentException("Invalid certificate version");
      this.version = version;
      this.extensions = new X509ExtensionCollection();
    }

    public byte Version
    {
      get
      {
        return this.version;
      }
      set
      {
        this.version = value;
      }
    }

    public byte[] SerialNumber
    {
      get
      {
        return this.sn;
      }
      set
      {
        this.sn = value;
      }
    }

    public string IssuerName
    {
      get
      {
        return this.issuer;
      }
      set
      {
        this.issuer = value;
      }
    }

    public DateTime NotBefore
    {
      get
      {
        return this.notBefore;
      }
      set
      {
        this.notBefore = value;
      }
    }

    public DateTime NotAfter
    {
      get
      {
        return this.notAfter;
      }
      set
      {
        this.notAfter = value;
      }
    }

    public string SubjectName
    {
      get
      {
        return this.subject;
      }
      set
      {
        this.subject = value;
      }
    }

    public AsymmetricAlgorithm SubjectPublicKey
    {
      get
      {
        return this.aa;
      }
      set
      {
        this.aa = value;
      }
    }

    public byte[] IssuerUniqueId
    {
      get
      {
        return this.issuerUniqueID;
      }
      set
      {
        this.issuerUniqueID = value;
      }
    }

    public byte[] SubjectUniqueId
    {
      get
      {
        return this.subjectUniqueID;
      }
      set
      {
        this.subjectUniqueID = value;
      }
    }

    public X509ExtensionCollection Extensions
    {
      get
      {
        return this.extensions;
      }
    }

    private ASN1 SubjectPublicKeyInfo()
    {
      ASN1 asN1_1 = new ASN1((byte) 48);
      if (this.aa is RSA)
      {
        asN1_1.Add(PKCS7.AlgorithmIdentifier("1.2.840.113549.1.1.1"));
        RSAParameters rsaParameters = (this.aa as RSA).ExportParameters(false);
        ASN1 asN1_2 = new ASN1((byte) 48);
        asN1_2.Add(ASN1Convert.FromUnsignedBigInteger(rsaParameters.Modulus));
        asN1_2.Add(ASN1Convert.FromUnsignedBigInteger(rsaParameters.Exponent));
        asN1_1.Add(new ASN1(this.UniqueIdentifier(asN1_2.GetBytes())));
      }
      else
      {
        if (!(this.aa is DSA))
          throw new NotSupportedException("Unknown Asymmetric Algorithm " + this.aa.ToString());
        DSAParameters dsaParameters = (this.aa as DSA).ExportParameters(false);
        ASN1 parameters = new ASN1((byte) 48);
        parameters.Add(ASN1Convert.FromUnsignedBigInteger(dsaParameters.P));
        parameters.Add(ASN1Convert.FromUnsignedBigInteger(dsaParameters.Q));
        parameters.Add(ASN1Convert.FromUnsignedBigInteger(dsaParameters.G));
        asN1_1.Add(PKCS7.AlgorithmIdentifier("1.2.840.10040.4.1", parameters));
        asN1_1.Add(new ASN1((byte) 3)).Add(ASN1Convert.FromUnsignedBigInteger(dsaParameters.Y));
      }
      return asN1_1;
    }

    private byte[] UniqueIdentifier(byte[] id)
    {
      ASN1 asN1 = new ASN1((byte) 3);
      byte[] numArray = new byte[id.Length + 1];
      Buffer.BlockCopy((Array) id, 0, (Array) numArray, 1, id.Length);
      asN1.Value = numArray;
      return asN1.GetBytes();
    }

    protected override ASN1 ToBeSigned(string oid)
    {
      ASN1 asN1_1 = new ASN1((byte) 48);
      if (this.version > (byte) 1)
      {
        byte[] data = new byte[1]
        {
          (byte) ((uint) this.version - 1U)
        };
        asN1_1.Add(new ASN1((byte) 160)).Add(new ASN1((byte) 2, data));
      }
      asN1_1.Add(new ASN1((byte) 2, this.sn));
      asN1_1.Add(PKCS7.AlgorithmIdentifier(oid));
      asN1_1.Add(X501.FromString(this.issuer));
      ASN1 asN1_2 = asN1_1.Add(new ASN1((byte) 48));
      asN1_2.Add(ASN1Convert.FromDateTime(this.notBefore));
      asN1_2.Add(ASN1Convert.FromDateTime(this.notAfter));
      asN1_1.Add(X501.FromString(this.subject));
      asN1_1.Add(this.SubjectPublicKeyInfo());
      if (this.version > (byte) 1)
      {
        if (this.issuerUniqueID != null)
          asN1_1.Add(new ASN1((byte) 161, this.UniqueIdentifier(this.issuerUniqueID)));
        if (this.subjectUniqueID != null)
          asN1_1.Add(new ASN1((byte) 161, this.UniqueIdentifier(this.subjectUniqueID)));
        if (this.version > (byte) 2 && this.extensions.Count > 0)
          asN1_1.Add(new ASN1((byte) 163, this.extensions.GetBytes()));
      }
      return asN1_1;
    }
  }
}
