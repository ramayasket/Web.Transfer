// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.X509Builder
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;

namespace Mono.Security.X509
{
  using Mono.Security;
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Runtime.CompilerServices;
  using System.Security.Cryptography;

  public abstract class X509Builder
  {
    private const string defaultHash = "SHA1";
    private string hashName = "SHA1";
    [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_mapE;

        protected X509Builder()
    {
    }

    private byte[] Build(ASN1 tbs, string hashoid, byte[] signature)
    {
      ASN1 asn = new ASN1(0x30);
      asn.Add(tbs);
      asn.Add(PKCS7.AlgorithmIdentifier(hashoid));
      byte[] dst = new byte[signature.Length + 1];
      Buffer.BlockCopy(signature, 0, dst, 1, signature.Length);
      asn.Add(new ASN1(3, dst));
      return asn.GetBytes();
    }

    protected string GetOid(string hashName)
    {
      string key = hashName.ToLower(CultureInfo.InvariantCulture);
      if (key != null)
      {
        if (__f__switch_mapE == null)
                {
          Dictionary<string, int> dictionary = new Dictionary<string, int>(7) {
                        {
                            "md2",
                            0
                        },
                        {
                            "md4",
                            1
                        },
                        {
                            "md5",
                            2
                        },
                        {
                            "sha1",
                            3
                        },
                        {
                            "sha256",
                            4
                        },
                        {
                            "sha384",
                            5
                        },
                        {
                            "sha512",
                            6
                        }
                    };
                    __f__switch_mapE = dictionary;
        }
        if (__f__switch_mapE.TryGetValue(key, out int num))
                {
          switch (num)
          {
            case 0:
              return "1.2.840.113549.1.1.2";

            case 1:
              return "1.2.840.113549.1.1.3";

            case 2:
              return "1.2.840.113549.1.1.4";

            case 3:
              return "1.2.840.113549.1.1.5";

            case 4:
              return "1.2.840.113549.1.1.11";

            case 5:
              return "1.2.840.113549.1.1.12";

            case 6:
              return "1.2.840.113549.1.1.13";
          }
        }
      }
      throw new NotSupportedException("Unknown hash algorithm " + hashName);
    }

    public virtual byte[] Sign(AsymmetricAlgorithm aa)
    {
      if (aa is RSA)
      {
        return this.Sign(aa as RSA);
      }
      if (!(aa is DSA))
      {
        throw new NotSupportedException("Unknown Asymmetric Algorithm " + aa.ToString());
      }
      return this.Sign(aa as DSA);
    }

    public virtual byte[] Sign(DSA key)
    {
      string hashName = "1.2.840.10040.4.3";
      ASN1 tbs = this.ToBeSigned(hashName);
      HashAlgorithm algorithm = HashAlgorithm.Create(this.hashName);
      if (!(algorithm is SHA1))
      {
        throw new NotSupportedException("Only SHA-1 is supported for DSA");
      }
      byte[] rgbHash = algorithm.ComputeHash(tbs.GetBytes());
      DSASignatureFormatter formatter = new DSASignatureFormatter(key);
      formatter.SetHashAlgorithm(this.hashName);
      byte[] src = formatter.CreateSignature(rgbHash);
      byte[] dst = new byte[20];
      Buffer.BlockCopy(src, 0, dst, 0, 20);
      byte[] buffer4 = new byte[20];
      Buffer.BlockCopy(src, 20, buffer4, 0, 20);
      ASN1 asn2 = new ASN1(0x30);
      asn2.Add(new ASN1(2, dst));
      asn2.Add(new ASN1(2, buffer4));
      return this.Build(tbs, hashName, asn2.GetBytes());
    }

    public virtual byte[] Sign(RSA key)
    {
      string oid = this.GetOid(this.hashName);
      ASN1 tbs = this.ToBeSigned(oid);
      byte[] rgbHash = HashAlgorithm.Create(this.hashName).ComputeHash(tbs.GetBytes());
      RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(key);
      formatter.SetHashAlgorithm(this.hashName);
      byte[] signature = formatter.CreateSignature(rgbHash);
      return this.Build(tbs, oid, signature);
    }

    protected abstract ASN1 ToBeSigned(string hashName);

    public string Hash
    {
      get =>
          this.hashName;
      set
      {
        if (this.hashName == null)
        {
          this.hashName = "SHA1";
        }
        else
        {
          this.hashName = value;
        }
      }
    }
  }
}
