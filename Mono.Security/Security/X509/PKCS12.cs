// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.PKCS12
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security.X509
{
  using Mono.Security;
  using Mono.Security.Cryptography;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Runtime.CompilerServices;
  using System.Runtime.InteropServices;
  using System.Security.Cryptography;
  using System.Text;

  public class PKCS12 : ICloneable
  {
    public const string pbeWithSHAAnd128BitRC4 = "1.2.840.113549.1.12.1.1";
    public const string pbeWithSHAAnd40BitRC4 = "1.2.840.113549.1.12.1.2";
    public const string pbeWithSHAAnd3KeyTripleDESCBC = "1.2.840.113549.1.12.1.3";
    public const string pbeWithSHAAnd2KeyTripleDESCBC = "1.2.840.113549.1.12.1.4";
    public const string pbeWithSHAAnd128BitRC2CBC = "1.2.840.113549.1.12.1.5";
    public const string pbeWithSHAAnd40BitRC2CBC = "1.2.840.113549.1.12.1.6";
    public const string keyBag = "1.2.840.113549.1.12.10.1.1";
    public const string pkcs8ShroudedKeyBag = "1.2.840.113549.1.12.10.1.2";
    public const string certBag = "1.2.840.113549.1.12.10.1.3";
    public const string crlBag = "1.2.840.113549.1.12.10.1.4";
    public const string secretBag = "1.2.840.113549.1.12.10.1.5";
    public const string safeContentsBag = "1.2.840.113549.1.12.10.1.6";
    public const string x509Certificate = "1.2.840.113549.1.9.22.1";
    public const string sdsiCertificate = "1.2.840.113549.1.9.22.2";
    public const string x509Crl = "1.2.840.113549.1.9.23.1";
    public const int CryptoApiPasswordLimit = 0x20;
    private static int recommendedIterationCount = 0x7d0;
    private byte[] _password;
    private ArrayList _keyBags;
    private ArrayList _secretBags;
    private X509CertificateCollection _certs;
    private bool _keyBagsChanged;
    private bool _secretBagsChanged;
    private bool _certsChanged;
    private int _iterations;
    private ArrayList _safeBags;
    private RandomNumberGenerator _rng;
    private static int password_max_length = 0x7fffffff;
    [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_map5;
        [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_map6;
        [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_map7;
        [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_map8;
        [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_map9;
        [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_mapA;
        [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_mapB;
        [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_mapC;

        public PKCS12()
    {
      this._iterations = recommendedIterationCount;
      this._keyBags = new ArrayList();
      this._secretBags = new ArrayList();
      this._certs = new X509CertificateCollection();
      this._keyBagsChanged = false;
      this._secretBagsChanged = false;
      this._certsChanged = false;
      this._safeBags = new ArrayList();
    }

    public PKCS12(byte[] data) : this()
    {
      this.Password = null;
      this.Decode(data);
    }

    public PKCS12(byte[] data, string password) : this()
    {
      this.Password = password;
      this.Decode(data);
    }

    public PKCS12(byte[] data, byte[] password) : this()
    {
      this._password = password;
      this.Decode(data);
    }

    public void AddCertificate(X509Certificate cert)
    {
      this.AddCertificate(cert, null);
    }

    public void AddCertificate(X509Certificate cert, IDictionary attributes)
    {
      bool flag = false;
      for (int i = 0; !flag && (i < this._safeBags.Count); i++)
      {
        SafeBag bag = (SafeBag)this._safeBags[i];
        if (bag.BagOID.Equals("1.2.840.113549.1.12.10.1.3"))
        {
          ASN1 asn2 = bag.ASN1[1];
          PKCS7.ContentInfo info = new PKCS7.ContentInfo(asn2.Value);
          X509Certificate certificate = new X509Certificate(info.Content[0].Value);
          if (this.Compare(cert.RawData, certificate.RawData))
          {
            flag = true;
          }
        }
      }
      if (!flag)
      {
        this._safeBags.Add(new SafeBag("1.2.840.113549.1.12.10.1.3", this.CertificateSafeBag(cert, attributes)));
        this._certsChanged = true;
      }
    }

    public void AddKeyBag(AsymmetricAlgorithm aa)
    {
      this.AddKeyBag(aa, null);
    }

    public void AddKeyBag(AsymmetricAlgorithm aa, IDictionary attributes)
    {
      bool flag = false;
      for (int i = 0; !flag && (i < this._safeBags.Count); i++)
      {
        SafeBag bag = (SafeBag)this._safeBags[i];
        if (bag.BagOID.Equals("1.2.840.113549.1.12.10.1.1"))
        {
          ASN1 asn = bag.ASN1[1];
          PKCS8.PrivateKeyInfo info = new PKCS8.PrivateKeyInfo(asn.Value);
          byte[] privateKey = info.PrivateKey;
          AsymmetricAlgorithm algorithm = null;
          switch (privateKey[0])
          {
            case 2:
              {
                DSAParameters dsaParameters = new DSAParameters();
                algorithm = PKCS8.PrivateKeyInfo.DecodeDSA(privateKey, dsaParameters);
                break;
              }
            case 0x30:
              algorithm = PKCS8.PrivateKeyInfo.DecodeRSA(privateKey);
              break;

            default:
              Array.Clear(privateKey, 0, privateKey.Length);
              throw new CryptographicException("Unknown private key format");
          }
          Array.Clear(privateKey, 0, privateKey.Length);
          if (this.CompareAsymmetricAlgorithm(aa, algorithm))
          {
            flag = true;
          }
        }
      }
      if (!flag)
      {
        this._safeBags.Add(new SafeBag("1.2.840.113549.1.12.10.1.1", this.KeyBagSafeBag(aa, attributes)));
        this._keyBagsChanged = true;
      }
    }

    public void AddPkcs8ShroudedKeyBag(AsymmetricAlgorithm aa)
    {
      this.AddPkcs8ShroudedKeyBag(aa, null);
    }

    public void AddPkcs8ShroudedKeyBag(AsymmetricAlgorithm aa, IDictionary attributes)
    {
      bool flag = false;
      for (int i = 0; !flag && (i < this._safeBags.Count); i++)
      {
        SafeBag bag = (SafeBag)this._safeBags[i];
        if (bag.BagOID.Equals("1.2.840.113549.1.12.10.1.2"))
        {
          ASN1 asn = bag.ASN1[1];
          PKCS8.EncryptedPrivateKeyInfo info = new PKCS8.EncryptedPrivateKeyInfo(asn.Value);
          byte[] data = this.Decrypt(info.Algorithm, info.Salt, info.IterationCount, info.EncryptedData);
          PKCS8.PrivateKeyInfo info2 = new PKCS8.PrivateKeyInfo(data);
          byte[] privateKey = info2.PrivateKey;
          AsymmetricAlgorithm algorithm = null;
          switch (privateKey[0])
          {
            case 2:
              {
                DSAParameters dsaParameters = new DSAParameters();
                algorithm = PKCS8.PrivateKeyInfo.DecodeDSA(privateKey, dsaParameters);
                break;
              }
            case 0x30:
              algorithm = PKCS8.PrivateKeyInfo.DecodeRSA(privateKey);
              break;

            default:
              Array.Clear(data, 0, data.Length);
              Array.Clear(privateKey, 0, privateKey.Length);
              throw new CryptographicException("Unknown private key format");
          }
          Array.Clear(data, 0, data.Length);
          Array.Clear(privateKey, 0, privateKey.Length);
          if (this.CompareAsymmetricAlgorithm(aa, algorithm))
          {
            flag = true;
          }
        }
      }
      if (!flag)
      {
        this._safeBags.Add(new SafeBag("1.2.840.113549.1.12.10.1.2", this.Pkcs8ShroudedKeyBagSafeBag(aa, attributes)));
        this._keyBagsChanged = true;
      }
    }

    private void AddPrivateKey(PKCS8.PrivateKeyInfo pki)
    {
      byte[] privateKey = pki.PrivateKey;
      switch (privateKey[0])
      {
        case 2:
          {
            DSAParameters existingParameters = this.GetExistingParameters(out bool flag);
            if (flag)
            {
              this._keyBags.Add(PKCS8.PrivateKeyInfo.DecodeDSA(privateKey, existingParameters));
            }
            break;
          }
        case 0x30:
          this._keyBags.Add(PKCS8.PrivateKeyInfo.DecodeRSA(privateKey));
          break;

        default:
          Array.Clear(privateKey, 0, privateKey.Length);
          throw new CryptographicException("Unknown private key format");
      }
      Array.Clear(privateKey, 0, privateKey.Length);
    }

    public void AddSecretBag(byte[] secret)
    {
      this.AddSecretBag(secret, null);
    }

    public void AddSecretBag(byte[] secret, IDictionary attributes)
    {
      bool flag = false;
      for (int i = 0; !flag && (i < this._safeBags.Count); i++)
      {
        SafeBag bag = (SafeBag)this._safeBags[i];
        if (bag.BagOID.Equals("1.2.840.113549.1.12.10.1.5"))
        {
          ASN1 asn = bag.ASN1[1];
          byte[] actual = asn.Value;
          if (this.Compare(secret, actual))
          {
            flag = true;
          }
        }
      }
      if (!flag)
      {
        this._safeBags.Add(new SafeBag("1.2.840.113549.1.12.10.1.5", this.SecretBagSafeBag(secret, attributes)));
        this._secretBagsChanged = true;
      }
    }

    private ASN1 CertificateSafeBag(X509Certificate x509, IDictionary attributes)
    {
      ASN1 asn = new ASN1(4, x509.RawData);
      PKCS7.ContentInfo info = new PKCS7.ContentInfo
      {
        ContentType = "1.2.840.113549.1.9.22.1"
      };
      info.Content.Add(asn);
      ASN1 asn2 = new ASN1(160);
      asn2.Add(info.ASN1);
      ASN1 asn3 = new ASN1(0x30);
      asn3.Add(ASN1Convert.FromOid("1.2.840.113549.1.12.10.1.3"));
      asn3.Add(asn2);
      if (attributes != null)
      {
        ASN1 asn4 = new ASN1(0x31);
        IDictionaryEnumerator enumerator = attributes.GetEnumerator();
        while (enumerator.MoveNext())
        {
          ArrayList list2;
          string str2 = (string)enumerator.Key;
          if (str2 != null)
          {
            if (__f__switch_mapC == null)
                        {
              Dictionary<string, int> dictionary = new Dictionary<string, int>(2) {
                                {
                                    "1.2.840.113549.1.9.20",
                                    0
                                },
                                {
                                    "1.2.840.113549.1.9.21",
                                    1
                                }
                            };
                            __f__switch_mapC = dictionary;
            }
            if (__f__switch_mapC.TryGetValue(str2, out int num))
                        {
              if (num != 0)
              {
                if (num == 1)
                {
                  goto Label_01AB;
                }
                continue;
              }
              ArrayList list = (ArrayList)enumerator.Value;
              if (list.Count > 0)
              {
                ASN1 asn5 = new ASN1(0x30);
                asn5.Add(ASN1Convert.FromOid("1.2.840.113549.1.9.20"));
                ASN1 asn6 = new ASN1(0x31);
                IEnumerator enumerator2 = list.GetEnumerator();
                try
                {
                  while (enumerator2.MoveNext())
                  {
                    byte[] current = (byte[])enumerator2.Current;
                    ASN1 asn7 = new ASN1(30)
                    {
                      Value = current
                    };
                    asn6.Add(asn7);
                  }
                }
                finally
                {
                  if (enumerator2 is IDisposable disposable)
                  {
                    disposable.Dispose();
                  }
                }
                asn5.Add(asn6);
                asn4.Add(asn5);
              }
            }
          }
          continue;
          Label_01AB:
          list2 = (ArrayList)enumerator.Value;
          if (list2.Count > 0)
          {
            ASN1 asn8 = new ASN1(0x30);
            asn8.Add(ASN1Convert.FromOid("1.2.840.113549.1.9.21"));
            ASN1 asn9 = new ASN1(0x31);
            IEnumerator enumerator3 = list2.GetEnumerator();
            try
            {
              while (enumerator3.MoveNext())
              {
                byte[] current = (byte[])enumerator3.Current;
                ASN1 asn10 = new ASN1(4)
                {
                  Value = current
                };
                asn9.Add(asn10);
              }
            }
            finally
            {
              if (enumerator3 is IDisposable disposable2)
              {
                disposable2.Dispose();
              }
            }
            asn8.Add(asn9);
            asn4.Add(asn8);
          }
        }
        if (asn4.Count > 0)
        {
          asn3.Add(asn4);
        }
      }
      return asn3;
    }

    public object Clone()
    {
      PKCS12 pkcs = null;
      if (this._password != null)
      {
        pkcs = new PKCS12(this.GetBytes(), Encoding.BigEndianUnicode.GetString(this._password));
      }
      else
      {
        pkcs = new PKCS12(this.GetBytes());
      }
      pkcs.IterationCount = this.IterationCount;
      return pkcs;
    }

    private bool Compare(byte[] expected, byte[] actual)
    {
      bool flag = false;
      if (expected.Length != actual.Length)
      {
        return flag;
      }
      for (int i = 0; i < expected.Length; i++)
      {
        if (expected[i] != actual[i])
        {
          return false;
        }
      }
      return true;
    }

    private bool CompareAsymmetricAlgorithm(AsymmetricAlgorithm a1, AsymmetricAlgorithm a2)
    {
      if (a1.KeySize != a2.KeySize)
      {
        return false;
      }
      return (a1.ToXmlString(false) == a2.ToXmlString(false));
    }

    private void Decode(byte[] data)
    {
      ASN1 asn = new ASN1(data);
      if (asn.Tag != 0x30)
      {
        throw new ArgumentException("invalid data");
      }
      ASN1 asn2 = asn[0];
      if (asn2.Tag != 2)
      {
        throw new ArgumentException("invalid PFX version");
      }
      PKCS7.ContentInfo info = new PKCS7.ContentInfo(asn[1]);
      if (info.ContentType != "1.2.840.113549.1.7.1")
      {
        throw new ArgumentException("invalid authenticated safe");
      }
      if (asn.Count > 2)
      {
        ASN1 asn3 = asn[2];
        if (asn3.Tag != 0x30)
        {
          throw new ArgumentException("invalid MAC");
        }
        ASN1 asn4 = asn3[0];
        if (asn4.Tag != 0x30)
        {
          throw new ArgumentException("invalid MAC");
        }
        ASN1 asn5 = asn4[0];
        if (ASN1Convert.ToOid(asn5[0]) != "1.3.14.3.2.26")
        {
          throw new ArgumentException("unsupported HMAC");
        }
        byte[] expected = asn4[1].Value;
        ASN1 asn6 = asn3[1];
        if (asn6.Tag != 4)
        {
          throw new ArgumentException("missing MAC salt");
        }
        this._iterations = 1;
        if (asn3.Count > 2)
        {
          ASN1 asn7 = asn3[2];
          if (asn7.Tag != 2)
          {
            throw new ArgumentException("invalid MAC iteration");
          }
          this._iterations = ASN1Convert.ToInt32(asn7);
        }
        byte[] buffer2 = info.Content[0].Value;
        byte[] actual = this.MAC(this._password, asn6.Value, this._iterations, buffer2);
        if (!this.Compare(expected, actual))
        {
          throw new CryptographicException("Invalid MAC - file may have been tampered!");
        }
      }
      ASN1 asn8 = new ASN1(info.Content[0].Value);
      for (int i = 0; i < asn8.Count; i++)
      {
        ASN1 asn9;
        int num3;
        ASN1 asn10;
        ASN1 asn11;
        int num4;
        ASN1 asn12;
        PKCS7.ContentInfo info2 = new PKCS7.ContentInfo(asn8[i]);
        string contentType = info2.ContentType;
        if (contentType != null)
        {
          if (__f__switch_map5 == null)
                    {
        Dictionary<string, int> dictionary = new Dictionary<string, int>(3) {
                            {
                                "1.2.840.113549.1.7.1",
                                0
                            },
                            {
                                "1.2.840.113549.1.7.6",
                                1
                            },
                            {
                                "1.2.840.113549.1.7.3",
                                2
                            }
                        };
                        __f__switch_map5 = dictionary;
      }
      if (__f__switch_map5.TryGetValue(contentType, out int num2))
                    {
        switch (num2)
        {
          case 0:
            asn9 = new ASN1(info2.Content[0].Value);
            num3 = 0;
            goto Label_028E;

          case 1:
            {
              PKCS7.EncryptedData ed = new PKCS7.EncryptedData(info2.Content[0]);
              asn11 = new ASN1(this.Decrypt(ed));
              num4 = 0;
              goto Label_02E5;
            }
          case 2:
            throw new NotImplementedException("public key encrypted");
        }
      }
    }
                throw new ArgumentException("unknown authenticatedSafe");
    Label_0275:
                asn10 = asn9[num3];
                this.ReadSafeBag(asn10);
    num3++;
            Label_028E:
                if (num3<asn9.Count)
                {
                    goto Label_0275;
                }
                continue;
            Label_02CC:
                asn12 = asn11[num4];
                this.ReadSafeBag(asn12);
                num4++;
            Label_02E5:
                if (num4<asn11.Count)
                {
                    goto Label_02CC;
                }
}
        }

        public byte[] Decrypt(PKCS7.EncryptedData ed) =>
            this.Decrypt(ed.EncryptionAlgorithm.ContentType, ed.EncryptionAlgorithm.Content[0].Value, ASN1Convert.ToInt32(ed.EncryptionAlgorithm.Content[1]), ed.EncryptedContent);

public byte[] Decrypt(string algorithmOid, byte[] salt, int iterationCount, byte[] encryptedData)
{
  SymmetricAlgorithm algorithm = null;
  byte[] buffer = null;
  try
  {
    algorithm = this.GetSymmetricAlgorithm(algorithmOid, salt, iterationCount);
    buffer = algorithm.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);
  }
  finally
  {
    if (algorithm != null)
    {
      algorithm.Clear();
    }
  }
  return buffer;
}

public byte[] Encrypt(string algorithmOid, byte[] salt, int iterationCount, byte[] data)
{
  using (SymmetricAlgorithm algorithm = this.GetSymmetricAlgorithm(algorithmOid, salt, iterationCount))
  {
    return algorithm.CreateEncryptor().TransformFinalBlock(data, 0, data.Length);
  }
}

private PKCS7.ContentInfo EncryptedContentInfo(ASN1 safeBags, string algorithmOid)
{
  byte[] data = new byte[8];
  this.RNG.GetBytes(data);
  ASN1 asn = new ASN1(0x30);
  asn.Add(new ASN1(4, data));
  asn.Add(ASN1Convert.FromInt32(this._iterations));
  ASN1 asn2 = new ASN1(0x30);
  asn2.Add(ASN1Convert.FromOid(algorithmOid));
  asn2.Add(asn);
  byte[] buffer2 = this.Encrypt(algorithmOid, data, this._iterations, safeBags.GetBytes());
  ASN1 asn3 = new ASN1(0x80, buffer2);
  ASN1 asn4 = new ASN1(0x30);
  asn4.Add(ASN1Convert.FromOid("1.2.840.113549.1.7.1"));
  asn4.Add(asn2);
  asn4.Add(asn3);
  ASN1 asn5 = new ASN1(2, new byte[1]);
  ASN1 asn6 = new ASN1(0x30);
  asn6.Add(asn5);
  asn6.Add(asn4);
  ASN1 asn7 = new ASN1(160);
  asn7.Add(asn6);
  return new PKCS7.ContentInfo("1.2.840.113549.1.7.6") { Content = asn7 };
}

~PKCS12()
{
  if (this._password != null)
  {
    Array.Clear(this._password, 0, this._password.Length);
  }
  this._password = null;
}

public AsymmetricAlgorithm GetAsymmetricAlgorithm(IDictionary attrs)
{
  IEnumerator enumerator = this._safeBags.GetEnumerator();
  try
  {
    while (enumerator.MoveNext())
    {
      SafeBag current = (SafeBag)enumerator.Current;
      if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.1") || current.BagOID.Equals("1.2.840.113549.1.12.10.1.2"))
      {
        ASN1 asn = current.ASN1;
        if (asn.Count == 3)
        {
          ASN1 asn2 = asn[2];
          int num = 0;
          for (int i = 0; i < asn2.Count; i++)
          {
            ASN1 asn3 = asn2[i];
            ASN1 asn4 = asn3[0];
            string str = ASN1Convert.ToOid(asn4);
            ArrayList list = (ArrayList)attrs[str];
            if (list != null)
            {
              ASN1 asn5 = asn3[1];
              if (list.Count == asn5.Count)
              {
                int num3 = 0;
                for (int j = 0; j < asn5.Count; j++)
                {
                  ASN1 asn6 = asn5[j];
                  byte[] expected = (byte[])list[j];
                  if (this.Compare(expected, asn6.Value))
                  {
                    num3++;
                  }
                }
                if (num3 == asn5.Count)
                {
                  num++;
                }
              }
            }
          }
          if (num == asn2.Count)
          {
            ASN1 asn7 = asn[1];
            AsymmetricAlgorithm algorithm = null;
            if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.1"))
            {
              PKCS8.PrivateKeyInfo info = new PKCS8.PrivateKeyInfo(asn7.Value);
              byte[] privateKey = info.PrivateKey;
              switch (privateKey[0])
              {
                case 2:
                  {
                    DSAParameters dsaParameters = new DSAParameters();
                    algorithm = PKCS8.PrivateKeyInfo.DecodeDSA(privateKey, dsaParameters);
                    break;
                  }
                case 0x30:
                  algorithm = PKCS8.PrivateKeyInfo.DecodeRSA(privateKey);
                  break;
              }
              Array.Clear(privateKey, 0, privateKey.Length);
              return algorithm;
            }
            if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.2"))
            {
              PKCS8.EncryptedPrivateKeyInfo info2 = new PKCS8.EncryptedPrivateKeyInfo(asn7.Value);
              byte[] data = this.Decrypt(info2.Algorithm, info2.Salt, info2.IterationCount, info2.EncryptedData);
              PKCS8.PrivateKeyInfo info3 = new PKCS8.PrivateKeyInfo(data);
              byte[] privateKey = info3.PrivateKey;
              switch (privateKey[0])
              {
                case 2:
                  {
                    DSAParameters dsaParameters = new DSAParameters();
                    algorithm = PKCS8.PrivateKeyInfo.DecodeDSA(privateKey, dsaParameters);
                    break;
                  }
                case 0x30:
                  algorithm = PKCS8.PrivateKeyInfo.DecodeRSA(privateKey);
                  break;
              }
              Array.Clear(privateKey, 0, privateKey.Length);
              Array.Clear(data, 0, data.Length);
            }
            return algorithm;
          }
        }
      }
    }
  }
  finally
  {
    if (enumerator is IDisposable disposable)
    {
      disposable.Dispose();
    }
  }
  return null;
}

public IDictionary GetAttributes(X509Certificate cert)
{
  IDictionary dictionary = new Hashtable();
  IEnumerator enumerator = this._safeBags.GetEnumerator();
  try
  {
    while (enumerator.MoveNext())
    {
      SafeBag current = (SafeBag)enumerator.Current;
      if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.3"))
      {
        ASN1 asn = current.ASN1;
        ASN1 asn2 = asn[1];
        PKCS7.ContentInfo info = new PKCS7.ContentInfo(asn2.Value);
        X509Certificate certificate = new X509Certificate(info.Content[0].Value);
        if (this.Compare(cert.RawData, certificate.RawData) && (asn.Count == 3))
        {
          ASN1 asn3 = asn[2];
          for (int i = 0; i < asn3.Count; i++)
          {
            ASN1 asn4 = asn3[i];
            ASN1 asn5 = asn4[0];
            string key = ASN1Convert.ToOid(asn5);
            ArrayList list = new ArrayList();
            ASN1 asn6 = asn4[1];
            for (int j = 0; j < asn6.Count; j++)
            {
              ASN1 asn7 = asn6[j];
              list.Add(asn7.Value);
            }
            dictionary.Add(key, list);
          }
        }
      }
    }
  }
  finally
  {
    if (enumerator is IDisposable disposable)
    {
      disposable.Dispose();
    }
  }
  return dictionary;
}

public IDictionary GetAttributes(AsymmetricAlgorithm aa)
{
  IDictionary dictionary = new Hashtable();
  IEnumerator enumerator = this._safeBags.GetEnumerator();
  try
  {
    while (enumerator.MoveNext())
    {
      SafeBag current = (SafeBag)enumerator.Current;
      if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.1") || current.BagOID.Equals("1.2.840.113549.1.12.10.1.2"))
      {
        ASN1 asn = current.ASN1;
        ASN1 asn2 = asn[1];
        AsymmetricAlgorithm algorithm = null;
        if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.1"))
        {
          PKCS8.PrivateKeyInfo info = new PKCS8.PrivateKeyInfo(asn2.Value);
          byte[] privateKey = info.PrivateKey;
          switch (privateKey[0])
          {
            case 2:
              {
                DSAParameters dsaParameters = new DSAParameters();
                algorithm = PKCS8.PrivateKeyInfo.DecodeDSA(privateKey, dsaParameters);
                break;
              }
            case 0x30:
              algorithm = PKCS8.PrivateKeyInfo.DecodeRSA(privateKey);
              break;
          }
          Array.Clear(privateKey, 0, privateKey.Length);
        }
        else if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.2"))
        {
          PKCS8.EncryptedPrivateKeyInfo info2 = new PKCS8.EncryptedPrivateKeyInfo(asn2.Value);
          byte[] data = this.Decrypt(info2.Algorithm, info2.Salt, info2.IterationCount, info2.EncryptedData);
          PKCS8.PrivateKeyInfo info3 = new PKCS8.PrivateKeyInfo(data);
          byte[] privateKey = info3.PrivateKey;
          switch (privateKey[0])
          {
            case 2:
              {
                DSAParameters dsaParameters = new DSAParameters();
                algorithm = PKCS8.PrivateKeyInfo.DecodeDSA(privateKey, dsaParameters);
                break;
              }
            case 0x30:
              algorithm = PKCS8.PrivateKeyInfo.DecodeRSA(privateKey);
              break;
          }
          Array.Clear(privateKey, 0, privateKey.Length);
          Array.Clear(data, 0, data.Length);
        }
        if (((algorithm != null) && this.CompareAsymmetricAlgorithm(algorithm, aa)) && (asn.Count == 3))
        {
          ASN1 asn3 = asn[2];
          for (int i = 0; i < asn3.Count; i++)
          {
            ASN1 asn4 = asn3[i];
            ASN1 asn5 = asn4[0];
            string key = ASN1Convert.ToOid(asn5);
            ArrayList list = new ArrayList();
            ASN1 asn6 = asn4[1];
            for (int j = 0; j < asn6.Count; j++)
            {
              ASN1 asn7 = asn6[j];
              list.Add(asn7.Value);
            }
            dictionary.Add(key, list);
          }
        }
      }
    }
  }
  finally
  {
    if (enumerator is IDisposable disposable)
    {
      disposable.Dispose();
    }
  }
  return dictionary;
}

public byte[] GetBytes()
{
  ASN1 asn = new ASN1(0x30);
  ArrayList list = new ArrayList();
  IEnumerator enumerator = this._safeBags.GetEnumerator();
  try
  {
    while (enumerator.MoveNext())
    {
      SafeBag current = (SafeBag)enumerator.Current;
      if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.3"))
      {
        ASN1 asn3 = current.ASN1[1];
        PKCS7.ContentInfo info = new PKCS7.ContentInfo(asn3.Value);
        list.Add(new X509Certificate(info.Content[0].Value));
      }
    }
  }
  finally
  {
    if (enumerator is IDisposable disposable)
    {
      disposable.Dispose();
    }
  }
  ArrayList list2 = new ArrayList();
  ArrayList list3 = new ArrayList();
  X509CertificateCollection.X509CertificateEnumerator enumerator2 = this.Certificates.GetEnumerator();
  try
  {
    while (enumerator2.MoveNext())
    {
      X509Certificate current = enumerator2.Current;
      bool flag = false;
      IEnumerator enumerator3 = list.GetEnumerator();
      try
      {
        while (enumerator3.MoveNext())
        {
          X509Certificate certificate2 = (X509Certificate)enumerator3.Current;
          if (this.Compare(current.RawData, certificate2.RawData))
          {
            flag = true;
          }
        }
      }
      finally
      {
        if (enumerator3 is IDisposable disposable2)
        {
          disposable2.Dispose();
        }
      }
      if (!flag)
      {
        list2.Add(current);
      }
    }
  }
  finally
  {
    if (enumerator2 is IDisposable disposable3)
    {
      disposable3.Dispose();
    }
  }
  IEnumerator enumerator4 = list.GetEnumerator();
  try
  {
    while (enumerator4.MoveNext())
    {
      X509Certificate current = (X509Certificate)enumerator4.Current;
      bool flag2 = false;
      X509CertificateCollection.X509CertificateEnumerator enumerator5 = this.Certificates.GetEnumerator();
      try
      {
        while (enumerator5.MoveNext())
        {
          X509Certificate certificate4 = enumerator5.Current;
          if (this.Compare(current.RawData, certificate4.RawData))
          {
            flag2 = true;
          }
        }
      }
      finally
      {
        if (enumerator5 is IDisposable disposable4)
        {
          disposable4.Dispose();
        }
      }
      if (!flag2)
      {
        list3.Add(current);
      }
    }
  }
  finally
  {
    if (enumerator4 is IDisposable disposable5)
    {
      disposable5.Dispose();
    }
  }
  IEnumerator enumerator6 = list3.GetEnumerator();
  try
  {
    while (enumerator6.MoveNext())
    {
      X509Certificate current = (X509Certificate)enumerator6.Current;
      this.RemoveCertificate(current);
    }
  }
  finally
  {
    if (enumerator6 is IDisposable disposable6)
    {
      disposable6.Dispose();
    }
  }
  IEnumerator enumerator7 = list2.GetEnumerator();
  try
  {
    while (enumerator7.MoveNext())
    {
      X509Certificate current = (X509Certificate)enumerator7.Current;
      this.AddCertificate(current);
    }
  }
  finally
  {
    if (enumerator7 is IDisposable disposable7)
    {
      disposable7.Dispose();
    }
  }
  if (this._safeBags.Count > 0)
  {
    ASN1 safeBags = new ASN1(0x30);
    IEnumerator enumerator8 = this._safeBags.GetEnumerator();
    try
    {
      while (enumerator8.MoveNext())
      {
        SafeBag current = (SafeBag)enumerator8.Current;
        if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.3"))
        {
          safeBags.Add(current.ASN1);
        }
      }
    }
    finally
    {
      if (enumerator8 is IDisposable disposable8)
      {
        disposable8.Dispose();
      }
    }
    if (safeBags.Count > 0)
    {
      PKCS7.ContentInfo info2 = this.EncryptedContentInfo(safeBags, "1.2.840.113549.1.12.1.3");
      asn.Add(info2.ASN1);
    }
  }
  if (this._safeBags.Count > 0)
  {
    ASN1 asn5 = new ASN1(0x30);
    IEnumerator enumerator9 = this._safeBags.GetEnumerator();
    try
    {
      while (enumerator9.MoveNext())
      {
        SafeBag current = (SafeBag)enumerator9.Current;
        if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.1") || current.BagOID.Equals("1.2.840.113549.1.12.10.1.2"))
        {
          asn5.Add(current.ASN1);
        }
      }
    }
    finally
    {
      if (enumerator9 is IDisposable disposable9)
      {
        disposable9.Dispose();
      }
    }
    if (asn5.Count > 0)
    {
      ASN1 asn6 = new ASN1(160);
      asn6.Add(new ASN1(4, asn5.GetBytes()));
      PKCS7.ContentInfo info3 = new PKCS7.ContentInfo("1.2.840.113549.1.7.1")
      {
        Content = asn6
      };
      asn.Add(info3.ASN1);
    }
  }
  if (this._safeBags.Count > 0)
  {
    ASN1 safeBags = new ASN1(0x30);
    IEnumerator enumerator10 = this._safeBags.GetEnumerator();
    try
    {
      while (enumerator10.MoveNext())
      {
        SafeBag current = (SafeBag)enumerator10.Current;
        if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.5"))
        {
          safeBags.Add(current.ASN1);
        }
      }
    }
    finally
    {
      if (enumerator10 is IDisposable disposable10)
      {
        disposable10.Dispose();
      }
    }
    if (safeBags.Count > 0)
    {
      PKCS7.ContentInfo info4 = this.EncryptedContentInfo(safeBags, "1.2.840.113549.1.12.1.3");
      asn.Add(info4.ASN1);
    }
  }
  ASN1 asn8 = new ASN1(4, asn.GetBytes());
  ASN1 asn9 = new ASN1(160);
  asn9.Add(asn8);
  PKCS7.ContentInfo info5 = new PKCS7.ContentInfo("1.2.840.113549.1.7.1")
  {
    Content = asn9
  };
  ASN1 asn10 = new ASN1(0x30);
  if (this._password != null)
  {
    byte[] buffer = new byte[20];
    this.RNG.GetBytes(buffer);
    byte[] buffer2 = this.MAC(this._password, buffer, this._iterations, info5.Content[0].Value);
    ASN1 asn11 = new ASN1(0x30);
    asn11.Add(ASN1Convert.FromOid("1.3.14.3.2.26"));
    asn11.Add(new ASN1(5));
    ASN1 asn12 = new ASN1(0x30);
    asn12.Add(asn11);
    asn12.Add(new ASN1(4, buffer2));
    asn10.Add(asn12);
    asn10.Add(new ASN1(4, buffer));
    asn10.Add(ASN1Convert.FromInt32(this._iterations));
  }
  byte[] data = new byte[] { 3 };
  ASN1 asn13 = new ASN1(2, data);
  ASN1 asn14 = new ASN1(0x30);
  asn14.Add(asn13);
  asn14.Add(info5.ASN1);
  if (asn10.Count > 0)
  {
    asn14.Add(asn10);
  }
  return asn14.GetBytes();
}

public X509Certificate GetCertificate(IDictionary attrs)
{
  IEnumerator enumerator = this._safeBags.GetEnumerator();
  try
  {
    while (enumerator.MoveNext())
    {
      SafeBag current = (SafeBag)enumerator.Current;
      if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.3"))
      {
        ASN1 asn = current.ASN1;
        if (asn.Count == 3)
        {
          ASN1 asn2 = asn[2];
          int num = 0;
          for (int i = 0; i < asn2.Count; i++)
          {
            ASN1 asn3 = asn2[i];
            ASN1 asn4 = asn3[0];
            string str = ASN1Convert.ToOid(asn4);
            ArrayList list = (ArrayList)attrs[str];
            if (list != null)
            {
              ASN1 asn5 = asn3[1];
              if (list.Count == asn5.Count)
              {
                int num3 = 0;
                for (int j = 0; j < asn5.Count; j++)
                {
                  ASN1 asn6 = asn5[j];
                  byte[] expected = (byte[])list[j];
                  if (this.Compare(expected, asn6.Value))
                  {
                    num3++;
                  }
                }
                if (num3 == asn5.Count)
                {
                  num++;
                }
              }
            }
          }
          if (num == asn2.Count)
          {
            ASN1 asn7 = asn[1];
            PKCS7.ContentInfo info = new PKCS7.ContentInfo(asn7.Value);
            return new X509Certificate(info.Content[0].Value);
          }
        }
      }
    }
  }
  finally
  {
    if (enumerator is IDisposable disposable)
    {
      disposable.Dispose();
    }
  }
  return null;
}

private DSAParameters GetExistingParameters(out bool found)
{
  X509CertificateCollection.X509CertificateEnumerator enumerator = this.Certificates.GetEnumerator();
  try
  {
    while (enumerator.MoveNext())
    {
      X509Certificate current = enumerator.Current;
      if (current.KeyAlgorithmParameters != null)
      {
        DSA dSA = current.DSA;
        if (dSA != null)
        {
          found = true;
          return dSA.ExportParameters(false);
        }
      }
    }
  }
  finally
  {
    if (enumerator is IDisposable disposable)
    {
      disposable.Dispose();
    }
  }
  found = false;
  return new DSAParameters();
}

public byte[] GetSecret(IDictionary attrs)
{
  IEnumerator enumerator = this._safeBags.GetEnumerator();
  try
  {
    while (enumerator.MoveNext())
    {
      SafeBag current = (SafeBag)enumerator.Current;
      if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.5"))
      {
        ASN1 asn = current.ASN1;
        if (asn.Count == 3)
        {
          ASN1 asn2 = asn[2];
          int num = 0;
          for (int i = 0; i < asn2.Count; i++)
          {
            ASN1 asn3 = asn2[i];
            ASN1 asn4 = asn3[0];
            string str = ASN1Convert.ToOid(asn4);
            ArrayList list = (ArrayList)attrs[str];
            if (list != null)
            {
              ASN1 asn5 = asn3[1];
              if (list.Count == asn5.Count)
              {
                int num3 = 0;
                for (int j = 0; j < asn5.Count; j++)
                {
                  ASN1 asn6 = asn5[j];
                  byte[] expected = (byte[])list[j];
                  if (this.Compare(expected, asn6.Value))
                  {
                    num3++;
                  }
                }
                if (num3 == asn5.Count)
                {
                  num++;
                }
              }
            }
          }
          if (num == asn2.Count)
          {
            ASN1 asn7 = asn[1];
            return asn7.Value;
          }
        }
      }
    }
  }
  finally
  {
    if (enumerator is IDisposable disposable)
    {
      disposable.Dispose();
    }
  }
  return null;
}

private SymmetricAlgorithm GetSymmetricAlgorithm(string algorithmOid, byte[] salt, int iterationCount)
{
  string algName = null;
  SymmetricAlgorithm algorithm;
  int size = 8;
  int num2 = 8;
  DeriveBytes bytes = new DeriveBytes
  {
    Password = this._password,
    Salt = salt,
    IterationCount = iterationCount
  };
  string key = algorithmOid;
  if (key != null)
  {
    if (__f__switch_map6 == null)
                {
      Dictionary<string, int> dictionary = new Dictionary<string, int>(12) {
                        {
                            "1.2.840.113549.1.5.1",
                            0
                        },
                        {
                            "1.2.840.113549.1.5.3",
                            1
                        },
                        {
                            "1.2.840.113549.1.5.4",
                            2
                        },
                        {
                            "1.2.840.113549.1.5.6",
                            3
                        },
                        {
                            "1.2.840.113549.1.5.10",
                            4
                        },
                        {
                            "1.2.840.113549.1.5.11",
                            5
                        },
                        {
                            "1.2.840.113549.1.12.1.1",
                            6
                        },
                        {
                            "1.2.840.113549.1.12.1.2",
                            7
                        },
                        {
                            "1.2.840.113549.1.12.1.3",
                            8
                        },
                        {
                            "1.2.840.113549.1.12.1.4",
                            9
                        },
                        {
                            "1.2.840.113549.1.12.1.5",
                            10
                        },
                        {
                            "1.2.840.113549.1.12.1.6",
                            11
                        }
                    };
                    __f__switch_map6 = dictionary;
    }
    if (__f__switch_map6.TryGetValue(key, out int num3))
                {
      switch (num3)
      {
        case 0:
          bytes.HashName = "MD2";
          algName = "DES";
          goto Label_026B;

        case 1:
          bytes.HashName = "MD5";
          algName = "DES";
          goto Label_026B;

        case 2:
          bytes.HashName = "MD2";
          algName = "RC2";
          size = 4;
          goto Label_026B;

        case 3:
          bytes.HashName = "MD5";
          algName = "RC2";
          size = 4;
          goto Label_026B;

        case 4:
          bytes.HashName = "SHA1";
          algName = "DES";
          goto Label_026B;

        case 5:
          bytes.HashName = "SHA1";
          algName = "RC2";
          size = 4;
          goto Label_026B;

        case 6:
          bytes.HashName = "SHA1";
          algName = "RC4";
          size = 0x10;
          num2 = 0;
          goto Label_026B;

        case 7:
          bytes.HashName = "SHA1";
          algName = "RC4";
          size = 5;
          num2 = 0;
          goto Label_026B;

        case 8:
          bytes.HashName = "SHA1";
          algName = "TripleDES";
          size = 0x18;
          goto Label_026B;

        case 9:
          bytes.HashName = "SHA1";
          algName = "TripleDES";
          size = 0x10;
          goto Label_026B;

        case 10:
          bytes.HashName = "SHA1";
          algName = "RC2";
          size = 0x10;
          goto Label_026B;

        case 11:
          bytes.HashName = "SHA1";
          algName = "RC2";
          size = 5;
          goto Label_026B;
      }
    }
  }
  throw new NotSupportedException("unknown oid " + algName);
  Label_026B:
  algorithm = SymmetricAlgorithm.Create(algName);
  algorithm.Key = bytes.DeriveKey(size);
  if (num2 > 0)
  {
    algorithm.IV = bytes.DeriveIV(num2);
    algorithm.Mode = CipherMode.CBC;
  }
  return algorithm;
}

private ASN1 KeyBagSafeBag(AsymmetricAlgorithm aa, IDictionary attributes)
{
  PKCS8.PrivateKeyInfo info = new PKCS8.PrivateKeyInfo();
  if (aa is RSA)
  {
    info.Algorithm = "1.2.840.113549.1.1.1";
    info.PrivateKey = PKCS8.PrivateKeyInfo.Encode((RSA)aa);
  }
  else
  {
    if (!(aa is DSA))
    {
      throw new CryptographicException("Unknown asymmetric algorithm {0}", aa.ToString());
    }
    info.Algorithm = null;
    info.PrivateKey = PKCS8.PrivateKeyInfo.Encode((DSA)aa);
  }
  ASN1 asn = new ASN1(0x30);
  asn.Add(ASN1Convert.FromOid("1.2.840.113549.1.12.10.1.1"));
  ASN1 asn2 = new ASN1(160);
  asn2.Add(new ASN1(info.GetBytes()));
  asn.Add(asn2);
  if (attributes != null)
  {
    ASN1 asn3 = new ASN1(0x31);
    IDictionaryEnumerator enumerator = attributes.GetEnumerator();
    while (enumerator.MoveNext())
    {
      ArrayList list2;
      string str2 = (string)enumerator.Key;
      if (str2 != null)
      {
        if (__f__switch_mapA == null)
                        {
          Dictionary<string, int> dictionary = new Dictionary<string, int>(2) {
                                {
                                    "1.2.840.113549.1.9.20",
                                    0
                                },
                                {
                                    "1.2.840.113549.1.9.21",
                                    1
                                }
                            };
                            __f__switch_mapA = dictionary;
        }
        if (__f__switch_mapA.TryGetValue(str2, out int num))
                        {
          if (num != 0)
          {
            if (num == 1)
            {
              goto Label_01EE;
            }
            continue;
          }
          ArrayList list = (ArrayList)enumerator.Value;
          if (list.Count > 0)
          {
            ASN1 asn4 = new ASN1(0x30);
            asn4.Add(ASN1Convert.FromOid("1.2.840.113549.1.9.20"));
            ASN1 asn5 = new ASN1(0x31);
            IEnumerator enumerator2 = list.GetEnumerator();
            try
            {
              while (enumerator2.MoveNext())
              {
                byte[] current = (byte[])enumerator2.Current;
                ASN1 asn6 = new ASN1(30)
                {
                  Value = current
                };
                asn5.Add(asn6);
              }
            }
            finally
            {
              if (enumerator2 is IDisposable disposable)
              {
                disposable.Dispose();
              }
            }
            asn4.Add(asn5);
            asn3.Add(asn4);
          }
        }
      }
      continue;
      Label_01EE:
      list2 = (ArrayList)enumerator.Value;
      if (list2.Count > 0)
      {
        ASN1 asn7 = new ASN1(0x30);
        asn7.Add(ASN1Convert.FromOid("1.2.840.113549.1.9.21"));
        ASN1 asn8 = new ASN1(0x31);
        IEnumerator enumerator3 = list2.GetEnumerator();
        try
        {
          while (enumerator3.MoveNext())
          {
            byte[] current = (byte[])enumerator3.Current;
            ASN1 asn9 = new ASN1(4)
            {
              Value = current
            };
            asn8.Add(asn9);
          }
        }
        finally
        {
          if (enumerator3 is IDisposable disposable2)
          {
            disposable2.Dispose();
          }
        }
        asn7.Add(asn8);
        asn3.Add(asn7);
      }
    }
    if (asn3.Count > 0)
    {
      asn.Add(asn3);
    }
  }
  return asn;
}

private static byte[] LoadFile(string filename)
{
  byte[] buffer = null;
  using (FileStream stream = File.OpenRead(filename))
  {
    buffer = new byte[stream.Length];
    stream.Read(buffer, 0, buffer.Length);
    stream.Close();
  }
  return buffer;
}

public static PKCS12 LoadFromFile(string filename)
{
  if (filename == null)
  {
    throw new ArgumentNullException("filename");
  }
  return new PKCS12(LoadFile(filename));
}

public static PKCS12 LoadFromFile(string filename, string password)
{
  if (filename == null)
  {
    throw new ArgumentNullException("filename");
  }
  return new PKCS12(LoadFile(filename), password);
}

private byte[] MAC(byte[] password, byte[] salt, int iterations, byte[] data)
{
  DeriveBytes bytes = new DeriveBytes
  {
    HashName = "SHA1",
    Password = password,
    Salt = salt,
    IterationCount = iterations
  };
  HMACSHA1 hmacsha = (HMACSHA1)System.Security.Cryptography.HMAC.Create();
  hmacsha.Key = bytes.DeriveMAC(20);
  return hmacsha.ComputeHash(data, 0, data.Length);
}

private ASN1 Pkcs8ShroudedKeyBagSafeBag(AsymmetricAlgorithm aa, IDictionary attributes)
{
  PKCS8.PrivateKeyInfo info = new PKCS8.PrivateKeyInfo();
  if (aa is RSA)
  {
    info.Algorithm = "1.2.840.113549.1.1.1";
    info.PrivateKey = PKCS8.PrivateKeyInfo.Encode((RSA)aa);
  }
  else
  {
    if (!(aa is DSA))
    {
      throw new CryptographicException("Unknown asymmetric algorithm {0}", aa.ToString());
    }
    info.Algorithm = null;
    info.PrivateKey = PKCS8.PrivateKeyInfo.Encode((DSA)aa);
  }
  PKCS8.EncryptedPrivateKeyInfo info2 = new PKCS8.EncryptedPrivateKeyInfo
  {
    Algorithm = "1.2.840.113549.1.12.1.3",
    IterationCount = this._iterations
  };
  info2.EncryptedData = this.Encrypt("1.2.840.113549.1.12.1.3", info2.Salt, this._iterations, info.GetBytes());
  ASN1 asn = new ASN1(0x30);
  asn.Add(ASN1Convert.FromOid("1.2.840.113549.1.12.10.1.2"));
  ASN1 asn2 = new ASN1(160);
  asn2.Add(new ASN1(info2.GetBytes()));
  asn.Add(asn2);
  if (attributes != null)
  {
    ASN1 asn3 = new ASN1(0x31);
    IDictionaryEnumerator enumerator = attributes.GetEnumerator();
    while (enumerator.MoveNext())
    {
      ArrayList list2;
      string str2 = (string)enumerator.Key;
      if (str2 != null)
      {
        if (__f__switch_map9 == null)
                        {
          Dictionary<string, int> dictionary = new Dictionary<string, int>(2) {
                                {
                                    "1.2.840.113549.1.9.20",
                                    0
                                },
                                {
                                    "1.2.840.113549.1.9.21",
                                    1
                                }
                            };
                            __f__switch_map9 = dictionary;
        }
        if (__f__switch_map9.TryGetValue(str2, out int num))
                        {
          if (num != 0)
          {
            if (num == 1)
            {
              goto Label_0230;
            }
            continue;
          }
          ArrayList list = (ArrayList)enumerator.Value;
          if (list.Count > 0)
          {
            ASN1 asn4 = new ASN1(0x30);
            asn4.Add(ASN1Convert.FromOid("1.2.840.113549.1.9.20"));
            ASN1 asn5 = new ASN1(0x31);
            IEnumerator enumerator2 = list.GetEnumerator();
            try
            {
              while (enumerator2.MoveNext())
              {
                byte[] current = (byte[])enumerator2.Current;
                ASN1 asn6 = new ASN1(30)
                {
                  Value = current
                };
                asn5.Add(asn6);
              }
            }
            finally
            {
              if (enumerator2 is IDisposable disposable)
              {
                disposable.Dispose();
              }
            }
            asn4.Add(asn5);
            asn3.Add(asn4);
          }
        }
      }
      continue;
      Label_0230:
      list2 = (ArrayList)enumerator.Value;
      if (list2.Count > 0)
      {
        ASN1 asn7 = new ASN1(0x30);
        asn7.Add(ASN1Convert.FromOid("1.2.840.113549.1.9.21"));
        ASN1 asn8 = new ASN1(0x31);
        IEnumerator enumerator3 = list2.GetEnumerator();
        try
        {
          while (enumerator3.MoveNext())
          {
            byte[] current = (byte[])enumerator3.Current;
            ASN1 asn9 = new ASN1(4)
            {
              Value = current
            };
            asn8.Add(asn9);
          }
        }
        finally
        {
          if (enumerator3 is IDisposable disposable2)
          {
            disposable2.Dispose();
          }
        }
        asn7.Add(asn8);
        asn3.Add(asn7);
      }
    }
    if (asn3.Count > 0)
    {
      asn.Add(asn3);
    }
  }
  return asn;
}

private void ReadSafeBag(ASN1 safeBag)
{
  Dictionary<string, int> dictionary;
  if (safeBag.Tag != 0x30)
  {
    throw new ArgumentException("invalid safeBag");
  }
  ASN1 asn = safeBag[0];
  if (asn.Tag != 6)
  {
    throw new ArgumentException("invalid safeBag id");
  }
  ASN1 asn2 = safeBag[1];
  string bagOID = ASN1Convert.ToOid(asn);
  string key = bagOID;
  if (key != null)
  {
    if (__f__switch_map7 == null)
                {
      dictionary = new Dictionary<string, int>(6) {
                        {
                            "1.2.840.113549.1.12.10.1.1",
                            0
                        },
                        {
                            "1.2.840.113549.1.12.10.1.2",
                            1
                        },
                        {
                            "1.2.840.113549.1.12.10.1.3",
                            2
                        },
                        {
                            "1.2.840.113549.1.12.10.1.4",
                            3
                        },
                        {
                            "1.2.840.113549.1.12.10.1.5",
                            4
                        },
                        {
                            "1.2.840.113549.1.12.10.1.6",
                            5
                        }
                    };
                    __f__switch_map7 = dictionary;
    }
    if (__f__switch_map7.TryGetValue(key, out int num))
                {
      switch (num)
      {
        case 0:
          this.AddPrivateKey(new PKCS8.PrivateKeyInfo(asn2.Value));
          goto Label_01DA;

        case 1:
          {
            PKCS8.EncryptedPrivateKeyInfo info = new PKCS8.EncryptedPrivateKeyInfo(asn2.Value);
            byte[] data = this.Decrypt(info.Algorithm, info.Salt, info.IterationCount, info.EncryptedData);
            this.AddPrivateKey(new PKCS8.PrivateKeyInfo(data));
            Array.Clear(data, 0, data.Length);
            goto Label_01DA;
          }
        case 2:
          {
            PKCS7.ContentInfo info2 = new PKCS7.ContentInfo(asn2.Value);
            if (info2.ContentType != "1.2.840.113549.1.9.22.1")
            {
              throw new NotSupportedException("unsupport certificate type");
            }
            X509Certificate certificate = new X509Certificate(info2.Content[0].Value);
            this._certs.Add(certificate);
            goto Label_01DA;
          }
        case 3:
        case 5:
          goto Label_01DA;

        case 4:
          {
            byte[] buffer2 = asn2.Value;
            this._secretBags.Add(buffer2);
            goto Label_01DA;
          }
      }
    }
  }
  throw new ArgumentException("unknown safeBag oid");
  Label_01DA:
  if (safeBag.Count > 2)
  {
    ASN1 asn3 = safeBag[2];
    if (asn3.Tag != 0x31)
    {
      throw new ArgumentException("invalid safeBag attributes id");
    }
    for (int i = 0; i < asn3.Count; i++)
    {
      ASN1 asn4 = asn3[i];
      if (asn4.Tag != 0x30)
      {
        throw new ArgumentException("invalid PKCS12 attributes id");
      }
      ASN1 asn5 = asn4[0];
      if (asn5.Tag != 6)
      {
        throw new ArgumentException("invalid attribute id");
      }
      string str3 = ASN1Convert.ToOid(asn5);
      ASN1 asn6 = asn4[1];
      for (int j = 0; j < asn6.Count; j++)
      {
        ASN1 asn7 = asn6[j];
        key = str3;
        if (key != null)
        {
          if (__f__switch_map8 == null)
                            {
      dictionary = new Dictionary<string, int>(2) {
                                    {
                                        "1.2.840.113549.1.9.20",
                                        0
                                    },
                                    {
                                        "1.2.840.113549.1.9.21",
                                        1
                                    }
                                };
                                __f__switch_map8 = dictionary;
    }
    if (__f__switch_map8.TryGetValue(key, out int num))
                            {
      if (num == 0)
      {
        if (asn7.Tag != 30)
        {
          throw new ArgumentException("invalid attribute value id");
        }
      }
      else if ((num == 1) && (asn7.Tag != 4))
      {
        throw new ArgumentException("invalid attribute value id");
      }
    }
  }
}
                }
            }
            this._safeBags.Add(new SafeBag(bagOID, safeBag));
        }

        public void RemoveCertificate(X509Certificate cert)
{
  this.RemoveCertificate(cert, null);
}

public void RemoveCertificate(X509Certificate cert, IDictionary attrs)
{
  int index = -1;
  for (int i = 0; (index == -1) && (i < this._safeBags.Count); i++)
  {
    SafeBag bag = (SafeBag)this._safeBags[i];
    if (bag.BagOID.Equals("1.2.840.113549.1.12.10.1.3"))
    {
      ASN1 asn = bag.ASN1;
      ASN1 asn2 = asn[1];
      PKCS7.ContentInfo info = new PKCS7.ContentInfo(asn2.Value);
      X509Certificate certificate = new X509Certificate(info.Content[0].Value);
      if (this.Compare(cert.RawData, certificate.RawData))
      {
        if (attrs != null)
        {
          if (asn.Count == 3)
          {
            ASN1 asn3 = asn[2];
            int num3 = 0;
            for (int j = 0; j < asn3.Count; j++)
            {
              ASN1 asn4 = asn3[j];
              ASN1 asn5 = asn4[0];
              string str = ASN1Convert.ToOid(asn5);
              ArrayList list = (ArrayList)attrs[str];
              if (list != null)
              {
                ASN1 asn6 = asn4[1];
                if (list.Count == asn6.Count)
                {
                  int num5 = 0;
                  for (int k = 0; k < asn6.Count; k++)
                  {
                    ASN1 asn7 = asn6[k];
                    byte[] expected = (byte[])list[k];
                    if (this.Compare(expected, asn7.Value))
                    {
                      num5++;
                    }
                  }
                  if (num5 == asn6.Count)
                  {
                    num3++;
                  }
                }
              }
            }
            if (num3 == asn3.Count)
            {
              index = i;
            }
          }
        }
        else
        {
          index = i;
        }
      }
    }
  }
  if (index != -1)
  {
    this._safeBags.RemoveAt(index);
    this._certsChanged = true;
  }
}

public void RemoveKeyBag(AsymmetricAlgorithm aa)
{
  int index = -1;
  for (int i = 0; (index == -1) && (i < this._safeBags.Count); i++)
  {
    SafeBag bag = (SafeBag)this._safeBags[i];
    if (bag.BagOID.Equals("1.2.840.113549.1.12.10.1.1"))
    {
      ASN1 asn = bag.ASN1[1];
      PKCS8.PrivateKeyInfo info = new PKCS8.PrivateKeyInfo(asn.Value);
      byte[] privateKey = info.PrivateKey;
      AsymmetricAlgorithm algorithm = null;
      switch (privateKey[0])
      {
        case 2:
          {
            DSAParameters dsaParameters = new DSAParameters();
            algorithm = PKCS8.PrivateKeyInfo.DecodeDSA(privateKey, dsaParameters);
            break;
          }
        case 0x30:
          algorithm = PKCS8.PrivateKeyInfo.DecodeRSA(privateKey);
          break;

        default:
          Array.Clear(privateKey, 0, privateKey.Length);
          throw new CryptographicException("Unknown private key format");
      }
      Array.Clear(privateKey, 0, privateKey.Length);
      if (this.CompareAsymmetricAlgorithm(aa, algorithm))
      {
        index = i;
      }
    }
  }
  if (index != -1)
  {
    this._safeBags.RemoveAt(index);
    this._keyBagsChanged = true;
  }
}

public void RemovePkcs8ShroudedKeyBag(AsymmetricAlgorithm aa)
{
  int index = -1;
  for (int i = 0; (index == -1) && (i < this._safeBags.Count); i++)
  {
    SafeBag bag = (SafeBag)this._safeBags[i];
    if (bag.BagOID.Equals("1.2.840.113549.1.12.10.1.2"))
    {
      ASN1 asn = bag.ASN1[1];
      PKCS8.EncryptedPrivateKeyInfo info = new PKCS8.EncryptedPrivateKeyInfo(asn.Value);
      byte[] data = this.Decrypt(info.Algorithm, info.Salt, info.IterationCount, info.EncryptedData);
      PKCS8.PrivateKeyInfo info2 = new PKCS8.PrivateKeyInfo(data);
      byte[] privateKey = info2.PrivateKey;
      AsymmetricAlgorithm algorithm = null;
      switch (privateKey[0])
      {
        case 2:
          {
            DSAParameters dsaParameters = new DSAParameters();
            algorithm = PKCS8.PrivateKeyInfo.DecodeDSA(privateKey, dsaParameters);
            break;
          }
        case 0x30:
          algorithm = PKCS8.PrivateKeyInfo.DecodeRSA(privateKey);
          break;

        default:
          Array.Clear(data, 0, data.Length);
          Array.Clear(privateKey, 0, privateKey.Length);
          throw new CryptographicException("Unknown private key format");
      }
      Array.Clear(data, 0, data.Length);
      Array.Clear(privateKey, 0, privateKey.Length);
      if (this.CompareAsymmetricAlgorithm(aa, algorithm))
      {
        index = i;
      }
    }
  }
  if (index != -1)
  {
    this._safeBags.RemoveAt(index);
    this._keyBagsChanged = true;
  }
}

public void RemoveSecretBag(byte[] secret)
{
  int index = -1;
  for (int i = 0; (index == -1) && (i < this._safeBags.Count); i++)
  {
    SafeBag bag = (SafeBag)this._safeBags[i];
    if (bag.BagOID.Equals("1.2.840.113549.1.12.10.1.5"))
    {
      ASN1 asn = bag.ASN1[1];
      byte[] actual = asn.Value;
      if (this.Compare(secret, actual))
      {
        index = i;
      }
    }
  }
  if (index != -1)
  {
    this._safeBags.RemoveAt(index);
    this._secretBagsChanged = true;
  }
}

public void SaveToFile(string filename)
{
  if (filename == null)
  {
    throw new ArgumentNullException("filename");
  }
  using (FileStream stream = File.Create(filename))
  {
    byte[] bytes = this.GetBytes();
    stream.Write(bytes, 0, bytes.Length);
  }
}

private ASN1 SecretBagSafeBag(byte[] secret, IDictionary attributes)
{
  ASN1 asn = new ASN1(0x30);
  asn.Add(ASN1Convert.FromOid("1.2.840.113549.1.12.10.1.5"));
  ASN1 asn2 = new ASN1(0x80, secret);
  asn.Add(asn2);
  if (attributes != null)
  {
    ASN1 asn3 = new ASN1(0x31);
    IDictionaryEnumerator enumerator = attributes.GetEnumerator();
    while (enumerator.MoveNext())
    {
      ArrayList list2;
      string str2 = (string)enumerator.Key;
      if (str2 != null)
      {
        if (__f__switch_mapB == null)
                        {
          Dictionary<string, int> dictionary = new Dictionary<string, int>(2) {
                                {
                                    "1.2.840.113549.1.9.20",
                                    0
                                },
                                {
                                    "1.2.840.113549.1.9.21",
                                    1
                                }
                            };
                            __f__switch_mapB = dictionary;
        }
        if (__f__switch_mapB.TryGetValue(str2, out int num))
                        {
          if (num != 0)
          {
            if (num == 1)
            {
              goto Label_016F;
            }
            continue;
          }
          ArrayList list = (ArrayList)enumerator.Value;
          if (list.Count > 0)
          {
            ASN1 asn4 = new ASN1(0x30);
            asn4.Add(ASN1Convert.FromOid("1.2.840.113549.1.9.20"));
            ASN1 asn5 = new ASN1(0x31);
            IEnumerator enumerator2 = list.GetEnumerator();
            try
            {
              while (enumerator2.MoveNext())
              {
                byte[] current = (byte[])enumerator2.Current;
                ASN1 asn6 = new ASN1(30)
                {
                  Value = current
                };
                asn5.Add(asn6);
              }
            }
            finally
            {
              if (enumerator2 is IDisposable disposable)
              {
                disposable.Dispose();
              }
            }
            asn4.Add(asn5);
            asn3.Add(asn4);
          }
        }
      }
      continue;
      Label_016F:
      list2 = (ArrayList)enumerator.Value;
      if (list2.Count > 0)
      {
        ASN1 asn7 = new ASN1(0x30);
        asn7.Add(ASN1Convert.FromOid("1.2.840.113549.1.9.21"));
        ASN1 asn8 = new ASN1(0x31);
        IEnumerator enumerator3 = list2.GetEnumerator();
        try
        {
          while (enumerator3.MoveNext())
          {
            byte[] current = (byte[])enumerator3.Current;
            ASN1 asn9 = new ASN1(4)
            {
              Value = current
            };
            asn8.Add(asn9);
          }
        }
        finally
        {
          if (enumerator3 is IDisposable disposable2)
          {
            disposable2.Dispose();
          }
        }
        asn7.Add(asn8);
        asn3.Add(asn7);
      }
    }
    if (asn3.Count > 0)
    {
      asn.Add(asn3);
    }
  }
  return asn;
}

public string Password
{
  set
  {
    if (value != null)
    {
      if (value.Length > 0)
      {
        int length = value.Length;
        int num2 = 0;
        if (length < MaximumPasswordLength)
        {
          if (value[length - 1] != '\0')
          {
            num2 = 1;
          }
        }
        else
        {
          length = MaximumPasswordLength;
        }
        this._password = new byte[(length + num2) << 1];
        Encoding.BigEndianUnicode.GetBytes(value, 0, length, this._password, 0);
      }
      else
      {
        this._password = new byte[2];
      }
    }
    else
    {
      this._password = null;
    }
  }
}

public int IterationCount
{
  get =>
      this._iterations;
  set => this._iterations = value;
}

public ArrayList Keys
{
  get
  {
    if (this._keyBagsChanged)
    {
      this._keyBags.Clear();
      IEnumerator enumerator = this._safeBags.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          SafeBag current = (SafeBag)enumerator.Current;
          if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.1"))
          {
            ASN1 asn2 = current.ASN1[1];
            PKCS8.PrivateKeyInfo info = new PKCS8.PrivateKeyInfo(asn2.Value);
            byte[] privateKey = info.PrivateKey;
            switch (privateKey[0])
            {
              case 2:
                {
                  DSAParameters dsaParameters = new DSAParameters();
                  this._keyBags.Add(PKCS8.PrivateKeyInfo.DecodeDSA(privateKey, dsaParameters));
                  break;
                }
              case 0x30:
                this._keyBags.Add(PKCS8.PrivateKeyInfo.DecodeRSA(privateKey));
                break;
            }
            Array.Clear(privateKey, 0, privateKey.Length);
            continue;
          }
          if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.2"))
          {
            ASN1 asn4 = current.ASN1[1];
            PKCS8.EncryptedPrivateKeyInfo info2 = new PKCS8.EncryptedPrivateKeyInfo(asn4.Value);
            byte[] data = this.Decrypt(info2.Algorithm, info2.Salt, info2.IterationCount, info2.EncryptedData);
            PKCS8.PrivateKeyInfo info3 = new PKCS8.PrivateKeyInfo(data);
            byte[] privateKey = info3.PrivateKey;
            switch (privateKey[0])
            {
              case 2:
                {
                  DSAParameters dsaParameters = new DSAParameters();
                  this._keyBags.Add(PKCS8.PrivateKeyInfo.DecodeDSA(privateKey, dsaParameters));
                  break;
                }
              case 0x30:
                this._keyBags.Add(PKCS8.PrivateKeyInfo.DecodeRSA(privateKey));
                break;
            }
            Array.Clear(privateKey, 0, privateKey.Length);
            Array.Clear(data, 0, data.Length);
          }
        }
      }
      finally
      {
        if (enumerator is IDisposable disposable)
        {
          disposable.Dispose();
        }
      }
      this._keyBagsChanged = false;
    }
    return ArrayList.ReadOnly(this._keyBags);
  }
}

public ArrayList Secrets
{
  get
  {
    if (this._secretBagsChanged)
    {
      this._secretBags.Clear();
      IEnumerator enumerator = this._safeBags.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          SafeBag current = (SafeBag)enumerator.Current;
          if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.5"))
          {
            ASN1 asn2 = current.ASN1[1];
            byte[] buffer = asn2.Value;
            this._secretBags.Add(buffer);
          }
        }
      }
      finally
      {
        if (enumerator is IDisposable disposable)
        {
          disposable.Dispose();
        }
      }
      this._secretBagsChanged = false;
    }
    return ArrayList.ReadOnly(this._secretBags);
  }
}

public X509CertificateCollection Certificates
{
  get
  {
    if (this._certsChanged)
    {
      this._certs.Clear();
      IEnumerator enumerator = this._safeBags.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          SafeBag current = (SafeBag)enumerator.Current;
          if (current.BagOID.Equals("1.2.840.113549.1.12.10.1.3"))
          {
            ASN1 asn2 = current.ASN1[1];
            PKCS7.ContentInfo info = new PKCS7.ContentInfo(asn2.Value);
            this._certs.Add(new X509Certificate(info.Content[0].Value));
          }
        }
      }
      finally
      {
        if (enumerator is IDisposable disposable)
        {
          disposable.Dispose();
        }
      }
      this._certsChanged = false;
    }
    return this._certs;
  }
}

internal RandomNumberGenerator RNG
{
  get
  {
    if (this._rng == null)
    {
      this._rng = RandomNumberGenerator.Create();
    }
    return this._rng;
  }
}

public static int MaximumPasswordLength
{
  get =>
      password_max_length;
  set
  {
    if (value < 0x20)
    {
      object[] args = new object[] { 0x20 };
      throw new ArgumentOutOfRangeException(Locale.GetText("Maximum password length cannot be less than {0}.", args));
    }
    password_max_length = value;
  }
}

public class DeriveBytes
{
  private static byte[] keyDiversifier = new byte[] {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            };
  private static byte[] ivDiversifier = new byte[] {
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            };
  private static byte[] macDiversifier = new byte[] {
                3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
                3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
                3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
                3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3
            };
  private string _hashName;
  private int _iterations;
  private byte[] _password;
  private byte[] _salt;

  private void Adjust(byte[] a, int aOff, byte[] b)
  {
    int num = ((b[b.Length - 1] & 0xff) + (a[(aOff + b.Length) - 1] & 0xff)) + 1;
    a[(aOff + b.Length) - 1] = (byte)num;
    num = num >> 8;
    for (int i = b.Length - 2; i >= 0; i--)
    {
      num += (b[i] & 0xff) + (a[aOff + i] & 0xff);
      a[aOff + i] = (byte)num;
      num = num >> 8;
    }
  }

  private byte[] Derive(byte[] diversifier, int n)
  {
    byte[] buffer2;
    byte[] buffer3;
    HashAlgorithm algorithm = HashAlgorithm.Create(this._hashName);
    int num = algorithm.HashSize >> 3;
    int num2 = 0x40;
    byte[] dst = new byte[n];
    if ((this._salt != null) && (this._salt.Length != 0))
    {
      buffer2 = new byte[num2 * (((this._salt.Length + num2) - 1) / num2)];
      for (int j = 0; j != buffer2.Length; j++)
      {
        buffer2[j] = this._salt[j % this._salt.Length];
      }
    }
    else
    {
      buffer2 = new byte[0];
    }
    if ((this._password != null) && (this._password.Length != 0))
    {
      buffer3 = new byte[num2 * (((this._password.Length + num2) - 1) / num2)];
      for (int j = 0; j != buffer3.Length; j++)
      {
        buffer3[j] = this._password[j % this._password.Length];
      }
    }
    else
    {
      buffer3 = new byte[0];
    }
    byte[] buffer4 = new byte[buffer2.Length + buffer3.Length];
    Buffer.BlockCopy(buffer2, 0, buffer4, 0, buffer2.Length);
    Buffer.BlockCopy(buffer3, 0, buffer4, buffer2.Length, buffer3.Length);
    byte[] b = new byte[num2];
    int num5 = ((n + num) - 1) / num;
    for (int i = 1; i <= num5; i++)
    {
      algorithm.TransformBlock(diversifier, 0, diversifier.Length, diversifier, 0);
      algorithm.TransformFinalBlock(buffer4, 0, buffer4.Length);
      byte[] hash = algorithm.Hash;
      algorithm.Initialize();
      for (int j = 1; j != this._iterations; j++)
      {
        hash = algorithm.ComputeHash(hash, 0, hash.Length);
      }
      for (int k = 0; k != b.Length; k++)
      {
        b[k] = hash[k % hash.Length];
      }
      for (int m = 0; m != (buffer4.Length / num2); m++)
      {
        this.Adjust(buffer4, m * num2, b);
      }
      if (i == num5)
      {
        Buffer.BlockCopy(hash, 0, dst, (i - 1) * num, dst.Length - ((i - 1) * num));
      }
      else
      {
        Buffer.BlockCopy(hash, 0, dst, (i - 1) * num, hash.Length);
      }
    }
    return dst;
  }

  public byte[] DeriveIV(int size) =>
      this.Derive(ivDiversifier, size);

  public byte[] DeriveKey(int size) =>
      this.Derive(keyDiversifier, size);

  public byte[] DeriveMAC(int size) =>
      this.Derive(macDiversifier, size);

  public string HashName
  {
    get =>
        this._hashName;
    set => this._hashName = value;
  }

  public int IterationCount
  {
    get =>
        this._iterations;
    set => this._iterations = value;
  }

  public byte[] Password
  {
    get =>
        ((byte[])this._password.Clone());
    set
    {
      if (value == null)
      {
        this._password = new byte[0];
      }
      else
      {
        this._password = (byte[])value.Clone();
      }
    }
  }

  public byte[] Salt
  {
    get =>
        ((byte[])this._salt.Clone());
    set
    {
      if (value != null)
      {
        this._salt = (byte[])value.Clone();
      }
      else
      {
        this._salt = null;
      }
    }
  }

  public enum Purpose
  {
    Key,
    IV,
    MAC
  }
}
    }
}
