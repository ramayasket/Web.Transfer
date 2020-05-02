// Decompiled with JetBrains decompiler
// Type: Mono.Security.PKCS7
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.X509;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Mono.Security
{
  using Mono.Security.X509;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Runtime.CompilerServices;
  using System.Security.Cryptography;

  public sealed class PKCS7
  {
    private PKCS7()
    {
    }

    public static ASN1 AlgorithmIdentifier(string oid)
    {
      ASN1 asn = new ASN1(0x30);
      asn.Add(ASN1Convert.FromOid(oid));
      asn.Add(new ASN1(5));
      return asn;
    }

    public static ASN1 AlgorithmIdentifier(string oid, ASN1 parameters)
    {
      ASN1 asn = new ASN1(0x30);
      asn.Add(ASN1Convert.FromOid(oid));
      asn.Add(parameters);
      return asn;
    }

    public static ASN1 Attribute(string oid, ASN1 value)
    {
      ASN1 asn = new ASN1(0x30);
      asn.Add(ASN1Convert.FromOid(oid));
      asn.Add(new ASN1(0x31)).Add(value);
      return asn;
    }

    public static ASN1 IssuerAndSerialNumber(X509Certificate x509)
    {
      ASN1 asn = null;
      ASN1 asn2 = null;
      ASN1 asn3 = new ASN1(x509.RawData);
      int num = 0;
      bool flag = false;
      while (num < asn3[0].Count)
      {
        ASN1 asn4 = asn3[0][num++];
        if (asn4.Tag == 2)
        {
          asn2 = asn4;
        }
        else if (asn4.Tag == 0x30)
        {
          if (flag)
          {
            asn = asn4;
            break;
          }
          flag = true;
        }
      }
      ASN1 asn5 = new ASN1(0x30);
      asn5.Add(asn);
      asn5.Add(asn2);
      return asn5;
    }

    public class ContentInfo
    {
      private string contentType;
      private Mono.Security.ASN1 content;

      public ContentInfo()
      {
        this.content = new Mono.Security.ASN1(160);
      }

      public ContentInfo(string oid) : this()
      {
        this.contentType = oid;
      }

      public ContentInfo(byte[] data) : this(new Mono.Security.ASN1(data))
      {
      }

      public ContentInfo(Mono.Security.ASN1 asn1)
      {
        if ((asn1.Tag != 0x30) || ((asn1.Count < 1) && (asn1.Count > 2)))
        {
          throw new ArgumentException("Invalid ASN1");
        }
        if (asn1[0].Tag != 6)
        {
          throw new ArgumentException("Invalid contentType");
        }
        this.contentType = ASN1Convert.ToOid(asn1[0]);
        if (asn1.Count > 1)
        {
          if (asn1[1].Tag != 160)
          {
            throw new ArgumentException("Invalid content");
          }
          this.content = asn1[1];
        }
      }

      internal Mono.Security.ASN1 GetASN1()
      {
        Mono.Security.ASN1 asn = new Mono.Security.ASN1(0x30);
        asn.Add(ASN1Convert.FromOid(this.contentType));
        if ((this.content != null) && (this.content.Count > 0))
        {
          asn.Add(this.content);
        }
        return asn;
      }

      public byte[] GetBytes() =>
          this.GetASN1().GetBytes();

      public Mono.Security.ASN1 ASN1 =>
          this.GetASN1();

      public Mono.Security.ASN1 Content
      {
        get =>
            this.content;
        set => this.content = value;
      }

      public string ContentType
      {
        get =>
            this.contentType;
        set => this.contentType = value;
      }
    }

    public class EncryptedData
    {
      private byte _version;
      private Mono.Security.PKCS7.ContentInfo _content;
      private Mono.Security.PKCS7.ContentInfo _encryptionAlgorithm;
      private byte[] _encrypted;

      public EncryptedData()
      {
        this._version = 0;
      }

      public EncryptedData(byte[] data) : this(new Mono.Security.ASN1(data))
      {
      }

      public EncryptedData(Mono.Security.ASN1 asn1) : this()
      {
        if ((asn1.Tag != 0x30) || (asn1.Count < 2))
        {
          throw new ArgumentException("Invalid EncryptedData");
        }
        if (asn1[0].Tag != 2)
        {
          throw new ArgumentException("Invalid version");
        }
        this._version = asn1[0].Value[0];
        Mono.Security.ASN1 asn = asn1[1];
        if (asn.Tag != 0x30)
        {
          throw new ArgumentException("missing EncryptedContentInfo");
        }
        Mono.Security.ASN1 asn2 = asn[0];
        if (asn2.Tag != 6)
        {
          throw new ArgumentException("missing EncryptedContentInfo.ContentType");
        }
        this._content = new Mono.Security.PKCS7.ContentInfo(ASN1Convert.ToOid(asn2));
        Mono.Security.ASN1 asn3 = asn[1];
        if (asn3.Tag != 0x30)
        {
          throw new ArgumentException("missing EncryptedContentInfo.ContentEncryptionAlgorithmIdentifier");
        }
        this._encryptionAlgorithm = new Mono.Security.PKCS7.ContentInfo(ASN1Convert.ToOid(asn3[0]));
        this._encryptionAlgorithm.Content = asn3[1];
        Mono.Security.ASN1 asn4 = asn[2];
        if (asn4.Tag != 0x80)
        {
          throw new ArgumentException("missing EncryptedContentInfo.EncryptedContent");
        }
        this._encrypted = asn4.Value;
      }

      internal Mono.Security.ASN1 GetASN1() =>
          null;

      public byte[] GetBytes() =>
          this.GetASN1().GetBytes();

      public Mono.Security.ASN1 ASN1 =>
          this.GetASN1();

      public Mono.Security.PKCS7.ContentInfo ContentInfo =>
          this._content;

      public Mono.Security.PKCS7.ContentInfo EncryptionAlgorithm =>
          this._encryptionAlgorithm;

      public byte[] EncryptedContent
      {
        get
        {
          if (this._encrypted == null)
          {
            return null;
          }
          return (byte[])this._encrypted.Clone();
        }
      }

      public byte Version
      {
        get =>
            this._version;
        set => this._version = value;
      }
    }

    public class EnvelopedData
    {
      private byte _version;
      private Mono.Security.PKCS7.ContentInfo _content;
      private Mono.Security.PKCS7.ContentInfo _encryptionAlgorithm;
      private ArrayList _recipientInfos;
      private byte[] _encrypted;

      public EnvelopedData()
      {
        this._version = 0;
        this._content = new Mono.Security.PKCS7.ContentInfo();
        this._encryptionAlgorithm = new Mono.Security.PKCS7.ContentInfo();
        this._recipientInfos = new ArrayList();
      }

      public EnvelopedData(byte[] data) : this(new Mono.Security.ASN1(data))
      {
      }

      public EnvelopedData(Mono.Security.ASN1 asn1) : this()
      {
        if ((asn1[0].Tag != 0x30) || (asn1[0].Count < 3))
        {
          throw new ArgumentException("Invalid EnvelopedData");
        }
        if (asn1[0][0].Tag != 2)
        {
          throw new ArgumentException("Invalid version");
        }
        this._version = asn1[0][0].Value[0];
        Mono.Security.ASN1 asn = asn1[0][1];
        if (asn.Tag != 0x31)
        {
          throw new ArgumentException("missing RecipientInfos");
        }
        for (int i = 0; i < asn.Count; i++)
        {
          Mono.Security.ASN1 data = asn[i];
          this._recipientInfos.Add(new PKCS7.RecipientInfo(data));
        }
        Mono.Security.ASN1 asn3 = asn1[0][2];
        if (asn3.Tag != 0x30)
        {
          throw new ArgumentException("missing EncryptedContentInfo");
        }
        Mono.Security.ASN1 asn4 = asn3[0];
        if (asn4.Tag != 6)
        {
          throw new ArgumentException("missing EncryptedContentInfo.ContentType");
        }
        this._content = new Mono.Security.PKCS7.ContentInfo(ASN1Convert.ToOid(asn4));
        Mono.Security.ASN1 asn5 = asn3[1];
        if (asn5.Tag != 0x30)
        {
          throw new ArgumentException("missing EncryptedContentInfo.ContentEncryptionAlgorithmIdentifier");
        }
        this._encryptionAlgorithm = new Mono.Security.PKCS7.ContentInfo(ASN1Convert.ToOid(asn5[0]));
        this._encryptionAlgorithm.Content = asn5[1];
        Mono.Security.ASN1 asn6 = asn3[2];
        if (asn6.Tag != 0x80)
        {
          throw new ArgumentException("missing EncryptedContentInfo.EncryptedContent");
        }
        this._encrypted = asn6.Value;
      }

      internal Mono.Security.ASN1 GetASN1() =>
          new Mono.Security.ASN1(0x30);

      public byte[] GetBytes() =>
          this.GetASN1().GetBytes();

      public ArrayList RecipientInfos =>
          this._recipientInfos;

      public Mono.Security.ASN1 ASN1 =>
          this.GetASN1();

      public Mono.Security.PKCS7.ContentInfo ContentInfo =>
          this._content;

      public Mono.Security.PKCS7.ContentInfo EncryptionAlgorithm =>
          this._encryptionAlgorithm;

      public byte[] EncryptedContent
      {
        get
        {
          if (this._encrypted == null)
          {
            return null;
          }
          return (byte[])this._encrypted.Clone();
        }
      }

      public byte Version
      {
        get =>
            this._version;
        set => this._version = value;
      }
    }

    public class Oid
    {
      public const string rsaEncryption = "1.2.840.113549.1.1.1";
      public const string data = "1.2.840.113549.1.7.1";
      public const string signedData = "1.2.840.113549.1.7.2";
      public const string envelopedData = "1.2.840.113549.1.7.3";
      public const string signedAndEnvelopedData = "1.2.840.113549.1.7.4";
      public const string digestedData = "1.2.840.113549.1.7.5";
      public const string encryptedData = "1.2.840.113549.1.7.6";
      public const string contentType = "1.2.840.113549.1.9.3";
      public const string messageDigest = "1.2.840.113549.1.9.4";
      public const string signingTime = "1.2.840.113549.1.9.5";
      public const string countersignature = "1.2.840.113549.1.9.6";
    }

    public class RecipientInfo
    {
      private int _version;
      private string _oid;
      private byte[] _key;
      private byte[] _ski;
      private string _issuer;
      private byte[] _serial;

      public RecipientInfo()
      {
      }

      public RecipientInfo(ASN1 data)
      {
        if (data.Tag != 0x30)
        {
          throw new ArgumentException("Invalid RecipientInfo");
        }
        ASN1 asn = data[0];
        if (asn.Tag != 2)
        {
          throw new ArgumentException("missing Version");
        }
        this._version = asn.Value[0];
        ASN1 asn2 = data[1];
        if ((asn2.Tag == 0x80) && (this._version == 3))
        {
          this._ski = asn2.Value;
        }
        else
        {
          this._issuer = X501.ToString(asn2[0]);
          this._serial = asn2[1].Value;
        }
        ASN1 asn3 = data[2];
        this._oid = ASN1Convert.ToOid(asn3[0]);
        ASN1 asn4 = data[3];
        this._key = asn4.Value;
      }

      public string Oid =>
          this._oid;

      public byte[] Key
      {
        get
        {
          if (this._key == null)
          {
            return null;
          }
          return (byte[])this._key.Clone();
        }
      }

      public byte[] SubjectKeyIdentifier
      {
        get
        {
          if (this._ski == null)
          {
            return null;
          }
          return (byte[])this._ski.Clone();
        }
      }

      public string Issuer =>
          this._issuer;

      public byte[] Serial
      {
        get
        {
          if (this._serial == null)
          {
            return null;
          }
          return (byte[])this._serial.Clone();
        }
      }

      public int Version =>
          this._version;
    }

    public class SignedData
    {
      private byte version;
      private string hashAlgorithm;
      private Mono.Security.PKCS7.ContentInfo contentInfo;
      private X509CertificateCollection certs;
      private ArrayList crls;
      private Mono.Security.PKCS7.SignerInfo signerInfo;
      private bool mda;
      private bool signed;
      [CompilerGenerated]
      private static Dictionary<string, int> __f__switch_map0;

            public SignedData()
      {
        this.version = 1;
        this.contentInfo = new Mono.Security.PKCS7.ContentInfo();
        this.certs = new X509CertificateCollection();
        this.crls = new ArrayList();
        this.signerInfo = new Mono.Security.PKCS7.SignerInfo();
        this.mda = true;
        this.signed = false;
      }

      public SignedData(byte[] data) : this(new Mono.Security.ASN1(data))
      {
      }

      public SignedData(Mono.Security.ASN1 asn1)
      {
        if ((asn1[0].Tag != 0x30) || (asn1[0].Count < 4))
        {
          throw new ArgumentException("Invalid SignedData");
        }
        if (asn1[0][0].Tag != 2)
        {
          throw new ArgumentException("Invalid version");
        }
        this.version = asn1[0][0].Value[0];
        this.contentInfo = new Mono.Security.PKCS7.ContentInfo(asn1[0][2]);
        int num = 3;
        this.certs = new X509CertificateCollection();
        if (asn1[0][num].Tag == 160)
        {
          for (int i = 0; i < asn1[0][num].Count; i++)
          {
            this.certs.Add(new X509Certificate(asn1[0][num][i].GetBytes()));
          }
          num++;
        }
        this.crls = new ArrayList();
        if (asn1[0][num].Tag == 0xa1)
        {
          for (int i = 0; i < asn1[0][num].Count; i++)
          {
            this.crls.Add(asn1[0][num][i].GetBytes());
          }
          num++;
        }
        if (asn1[0][num].Count > 0)
        {
          this.signerInfo = new Mono.Security.PKCS7.SignerInfo(asn1[0][num]);
        }
        else
        {
          this.signerInfo = new Mono.Security.PKCS7.SignerInfo();
        }
        if (this.signerInfo.HashName != null)
        {
          this.HashName = this.OidToName(this.signerInfo.HashName);
        }
        this.mda = this.signerInfo.AuthenticatedAttributes.Count > 0;
      }

      internal Mono.Security.ASN1 GetASN1()
      {
        Mono.Security.ASN1 asn = new Mono.Security.ASN1(0x30);
        byte[] data = new byte[] { this.version };
        asn.Add(new Mono.Security.ASN1(2, data));
        Mono.Security.ASN1 asn2 = asn.Add(new Mono.Security.ASN1(0x31));
        if (this.hashAlgorithm != null)
        {
          string oid = CryptoConfig.MapNameToOID(this.hashAlgorithm);
          asn2.Add(PKCS7.AlgorithmIdentifier(oid));
        }
        Mono.Security.ASN1 asn3 = this.contentInfo.ASN1;
        asn.Add(asn3);
        if (!this.signed && (this.hashAlgorithm != null))
        {
          if (this.mda)
          {
            Mono.Security.ASN1 asn4 = PKCS7.Attribute("1.2.840.113549.1.9.3", asn3[0]);
            this.signerInfo.AuthenticatedAttributes.Add(asn4);
            byte[] buffer2 = HashAlgorithm.Create(this.hashAlgorithm).ComputeHash(asn3[1][0].Value);
            Mono.Security.ASN1 asn5 = new Mono.Security.ASN1(0x30);
            Mono.Security.ASN1 asn6 = PKCS7.Attribute("1.2.840.113549.1.9.4", asn5.Add(new Mono.Security.ASN1(4, buffer2)));
            this.signerInfo.AuthenticatedAttributes.Add(asn6);
          }
          else
          {
            RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(this.signerInfo.Key);
            formatter.SetHashAlgorithm(this.hashAlgorithm);
            byte[] rgbHash = HashAlgorithm.Create(this.hashAlgorithm).ComputeHash(asn3[1][0].Value);
            this.signerInfo.Signature = formatter.CreateSignature(rgbHash);
          }
          this.signed = true;
        }
        if (this.certs.Count > 0)
        {
          Mono.Security.ASN1 asn7 = asn.Add(new Mono.Security.ASN1(160));
          X509CertificateCollection.X509CertificateEnumerator enumerator = this.certs.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              X509Certificate current = enumerator.Current;
              asn7.Add(new Mono.Security.ASN1(current.RawData));
            }
          }
          finally
          {
            if (enumerator is IDisposable disposable)
            {
              disposable.Dispose();
            }
          }
        }
        if (this.crls.Count > 0)
        {
          Mono.Security.ASN1 asn8 = asn.Add(new Mono.Security.ASN1(0xa1));
          IEnumerator enumerator = this.crls.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              byte[] current = (byte[])enumerator.Current;
              asn8.Add(new Mono.Security.ASN1(current));
            }
          }
          finally
          {
            if (enumerator is IDisposable disposable2)
            {
              disposable2.Dispose();
            }
          }
        }
        Mono.Security.ASN1 asn9 = asn.Add(new Mono.Security.ASN1(0x31));
        if (this.signerInfo.Key != null)
        {
          asn9.Add(this.signerInfo.ASN1);
        }
        return asn;
      }

      public byte[] GetBytes() =>
          this.GetASN1().GetBytes();

      internal string OidToName(string oid)
      {
        string key = oid;
        if (key != null)
        {
          if (__f__switch_map0 == null)
                    {
            Dictionary<string, int> dictionary = new Dictionary<string, int>(6) {
                            {
                                "1.3.14.3.2.26",
                                0
                            },
                            {
                                "1.2.840.113549.2.2",
                                1
                            },
                            {
                                "1.2.840.113549.2.5",
                                2
                            },
                            {
                                "2.16.840.1.101.3.4.1",
                                3
                            },
                            {
                                "2.16.840.1.101.3.4.2",
                                4
                            },
                            {
                                "2.16.840.1.101.3.4.3",
                                5
                            }
                        };
                        __f__switch_map0 = dictionary;
          }
          if (!__f__switch_map0.TryGetValue(key, out int num))
                    {
            return oid;
          }
          switch (num)
          {
            case 0:
              return "SHA1";

            case 1:
              return "MD2";

            case 2:
              return "MD5";

            case 3:
              return "SHA256";

            case 4:
              return "SHA384";

            case 5:
              return "SHA512";
          }
        }
        return oid;
      }

      public bool VerifySignature(AsymmetricAlgorithm aa)
      {
        if (aa == null)
        {
          return false;
        }
        RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(aa);
        deformatter.SetHashAlgorithm(this.hashAlgorithm);
        HashAlgorithm algorithm = HashAlgorithm.Create(this.hashAlgorithm);
        byte[] signature = this.signerInfo.Signature;
        byte[] rgbHash = null;
        if (this.mda)
        {
          Mono.Security.ASN1 asn = new Mono.Security.ASN1(0x31);
          IEnumerator enumerator = this.signerInfo.AuthenticatedAttributes.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              Mono.Security.ASN1 current = (Mono.Security.ASN1)enumerator.Current;
              asn.Add(current);
            }
          }
          finally
          {
            if (enumerator is IDisposable disposable)
            {
              disposable.Dispose();
            }
          }
          rgbHash = algorithm.ComputeHash(asn.GetBytes());
        }
        else
        {
          rgbHash = algorithm.ComputeHash(this.contentInfo.Content[0].Value);
        }
        return (((rgbHash != null) && (signature != null)) && deformatter.VerifySignature(rgbHash, signature));
      }

      public Mono.Security.ASN1 ASN1 =>
          this.GetASN1();

      public X509CertificateCollection Certificates =>
          this.certs;

      public Mono.Security.PKCS7.ContentInfo ContentInfo =>
          this.contentInfo;

      public ArrayList Crls =>
          this.crls;

      public string HashName
      {
        get =>
            this.hashAlgorithm;
        set
        {
          this.hashAlgorithm = value;
          this.signerInfo.HashName = value;
        }
      }

      public Mono.Security.PKCS7.SignerInfo SignerInfo =>
          this.signerInfo;

      public byte Version
      {
        get =>
            this.version;
        set => this.version = value;
      }

      public bool UseAuthenticatedAttributes
      {
        get =>
            this.mda;
        set => this.mda = value;
      }
    }

    public class SignerInfo
    {
      private byte version;
      private X509Certificate x509;
      private string hashAlgorithm;
      private AsymmetricAlgorithm key;
      private ArrayList authenticatedAttributes;
      private ArrayList unauthenticatedAttributes;
      private byte[] signature;
      private string issuer;
      private byte[] serial;
      private byte[] ski;

      public SignerInfo()
      {
        this.version = 1;
        this.authenticatedAttributes = new ArrayList();
        this.unauthenticatedAttributes = new ArrayList();
      }

      public SignerInfo(byte[] data) : this(new Mono.Security.ASN1(data))
      {
      }

      public SignerInfo(Mono.Security.ASN1 asn1) : this()
      {
        if ((asn1[0].Tag != 0x30) || (asn1[0].Count < 5))
        {
          throw new ArgumentException("Invalid SignedData");
        }
        if (asn1[0][0].Tag != 2)
        {
          throw new ArgumentException("Invalid version");
        }
        this.version = asn1[0][0].Value[0];
        Mono.Security.ASN1 asn = asn1[0][1];
        if ((asn.Tag == 0x80) && (this.version == 3))
        {
          this.ski = asn.Value;
        }
        else
        {
          this.issuer = X501.ToString(asn[0]);
          this.serial = asn[1].Value;
        }
        Mono.Security.ASN1 asn2 = asn1[0][2];
        this.hashAlgorithm = ASN1Convert.ToOid(asn2[0]);
        int num = 3;
        Mono.Security.ASN1 asn3 = asn1[0][num];
        if (asn3.Tag == 160)
        {
          num++;
          for (int i = 0; i < asn3.Count; i++)
          {
            this.authenticatedAttributes.Add(asn3[i]);
          }
        }
        num++;
        Mono.Security.ASN1 asn4 = asn1[0][num++];
        if (asn4.Tag == 4)
        {
          this.signature = asn4.Value;
        }
        Mono.Security.ASN1 asn5 = asn1[0][num];
        if ((asn5 != null) && (asn5.Tag == 0xa1))
        {
          for (int i = 0; i < asn5.Count; i++)
          {
            this.unauthenticatedAttributes.Add(asn5[i]);
          }
        }
      }

      internal Mono.Security.ASN1 GetASN1()
      {
        if ((this.key == null) || (this.hashAlgorithm == null))
        {
          return null;
        }
        byte[] data = new byte[] { this.version };
        Mono.Security.ASN1 asn = new Mono.Security.ASN1(0x30);
        asn.Add(new Mono.Security.ASN1(2, data));
        asn.Add(PKCS7.IssuerAndSerialNumber(this.x509));
        string oid = CryptoConfig.MapNameToOID(this.hashAlgorithm);
        asn.Add(PKCS7.AlgorithmIdentifier(oid));
        Mono.Security.ASN1 asn2 = null;
        if (this.authenticatedAttributes.Count > 0)
        {
          asn2 = asn.Add(new Mono.Security.ASN1(160));
          this.authenticatedAttributes.Sort(new PKCS7.SortedSet());
          IEnumerator enumerator = this.authenticatedAttributes.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              Mono.Security.ASN1 current = (Mono.Security.ASN1)enumerator.Current;
              asn2.Add(current);
            }
          }
          finally
          {
            if (enumerator is IDisposable disposable)
            {
              disposable.Dispose();
            }
          }
        }
        if (this.key is RSA)
        {
          asn.Add(PKCS7.AlgorithmIdentifier("1.2.840.113549.1.1.1"));
          if (asn2 != null)
          {
            RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(this.key);
            formatter.SetHashAlgorithm(this.hashAlgorithm);
            byte[] bytes = asn2.GetBytes();
            bytes[0] = 0x31;
            byte[] rgbHash = HashAlgorithm.Create(this.hashAlgorithm).ComputeHash(bytes);
            this.signature = formatter.CreateSignature(rgbHash);
          }
        }
        else
        {
          if (this.key is DSA)
          {
            throw new NotImplementedException("not yet");
          }
          throw new CryptographicException("Unknown assymetric algorithm");
        }
        asn.Add(new Mono.Security.ASN1(4, this.signature));
        if (this.unauthenticatedAttributes.Count > 0)
        {
          Mono.Security.ASN1 asn4 = asn.Add(new Mono.Security.ASN1(0xa1));
          this.unauthenticatedAttributes.Sort(new PKCS7.SortedSet());
          IEnumerator enumerator = this.unauthenticatedAttributes.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              Mono.Security.ASN1 current = (Mono.Security.ASN1)enumerator.Current;
              asn4.Add(current);
            }
          }
          finally
          {
            if (enumerator is IDisposable disposable2)
            {
              disposable2.Dispose();
            }
          }
        }
        return asn;
      }

      public byte[] GetBytes() =>
          this.GetASN1().GetBytes();

      public string IssuerName =>
          this.issuer;

      public byte[] SerialNumber
      {
        get
        {
          if (this.serial == null)
          {
            return null;
          }
          return (byte[])this.serial.Clone();
        }
      }

      public byte[] SubjectKeyIdentifier
      {
        get
        {
          if (this.ski == null)
          {
            return null;
          }
          return (byte[])this.ski.Clone();
        }
      }

      public Mono.Security.ASN1 ASN1 =>
          this.GetASN1();

      public ArrayList AuthenticatedAttributes =>
          this.authenticatedAttributes;

      public X509Certificate Certificate
      {
        get =>
            this.x509;
        set => this.x509 = value;
      }

      public string HashName
      {
        get =>
            this.hashAlgorithm;
        set => this.hashAlgorithm = value;
      }

      public AsymmetricAlgorithm Key
      {
        get =>
            this.key;
        set => this.key = value;
      }

      public byte[] Signature
      {
        get
        {
          if (this.signature == null)
          {
            return null;
          }
          return (byte[])this.signature.Clone();
        }
        set
        {
          if (value != null)
          {
            this.signature = (byte[])value.Clone();
          }
        }
      }

      public ArrayList UnauthenticatedAttributes =>
          this.unauthenticatedAttributes;

      public byte Version
      {
        get =>
            this.version;
        set => this.version = value;
      }
    }

    internal class SortedSet : IComparer
    {
      public int Compare(object x, object y)
      {
        if (x == null)
        {
          return ((y != null) ? -1 : 0);
        }
        if (y == null)
        {
          return 1;
        }
        ASN1 asn = x as ASN1;
        ASN1 asn2 = y as ASN1;
        if ((asn == null) || (asn2 == null))
        {
          throw new ArgumentException(Locale.GetText("Invalid objects."));
        }
        byte[] bytes = asn.GetBytes();
        byte[] buffer2 = asn2.GetBytes();
        for (int i = 0; i < bytes.Length; i++)
        {
          if (i == buffer2.Length)
          {
            break;
          }
          if (bytes[i] != buffer2[i])
          {
            return ((bytes[i] >= buffer2[i]) ? 1 : -1);
          }
        }
        if (bytes.Length > buffer2.Length)
        {
          return 1;
        }
        if (bytes.Length < buffer2.Length)
        {
          return -1;
        }
        return 0;
      }
    }
  }
}
