// Decompiled with JetBrains decompiler
// Type: Mono.Security.Authenticode.AuthenticodeDeformatter
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using Mono.Security.X509;
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;

namespace Mono.Security.Authenticode
{
  using Mono.Security;
  //using Mono.Security.Cryptography;
  //using Mono.Security.X509;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Runtime.CompilerServices;
  using System.Security;
  using System.Security.Cryptography;

  public class AuthenticodeDeformatter : AuthenticodeBase
  {
    private string filename;
    private byte[] hash;
    private X509CertificateCollection coll;
    private ASN1 signedHash;
    private DateTime timestamp;
    private X509Certificate signingCertificate;
    private int reason;
    private bool trustedRoot;
    private bool trustedTimestampRoot;
    private byte[] entry;
    private X509Chain signerChain;
    private X509Chain timestampChain;

    [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_map1;

    [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_map2;

    [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_map3;

    public AuthenticodeDeformatter()
    {
      this.reason = -1;
      this.signerChain = new X509Chain();
      this.timestampChain = new X509Chain();
    }

    public AuthenticodeDeformatter(string fileName) : this()
    {
      this.FileName = fileName;
    }

    private bool CheckSignature(string fileName)
    {
      this.filename = fileName;
      base.Open(this.filename);
      this.entry = base.GetSecurityEntry();
      if (this.entry == null)
      {
        this.reason = 1;
        base.Close();
        return false;
      }

      PKCS7.ContentInfo info = new PKCS7.ContentInfo(this.entry);
      if (info.ContentType != "1.2.840.113549.1.7.2")
      {
        base.Close();
        return false;
      }

      PKCS7.SignedData sd = new PKCS7.SignedData(info.Content);
      if (sd.ContentInfo.ContentType != "1.3.6.1.4.1.311.2.1.4")
      {
        base.Close();
        return false;
      }

      this.coll = sd.Certificates;
      ASN1 content = sd.ContentInfo.Content;
      this.signedHash = content[0][1][1];
      HashAlgorithm hash = null;
      switch (this.signedHash.Length)
      {
        case 0x10:
          hash = HashAlgorithm.Create("MD5");
          this.hash = base.GetHash(hash);
          break;

        case 20:
          hash = HashAlgorithm.Create("SHA1");
          this.hash = base.GetHash(hash);
          break;

        default:
          this.reason = 5;
          base.Close();
          return false;
      }

      base.Close();
      if (!this.signedHash.CompareValue(this.hash))
      {
        this.reason = 2;
      }

      byte[] buffer = content[0].Value;
      hash.Initialize();
      byte[] calculatedMessageDigest = hash.ComputeHash(buffer);
      return (this.VerifySignature(sd, calculatedMessageDigest, hash) && (this.reason == 0));
    }

    private bool CompareIssuerSerial(string issuer, byte[] serial, X509Certificate x509)
    {
      if (issuer != x509.IssuerName)
      {
        return false;
      }

      if (serial.Length != x509.SerialNumber.Length)
      {
        return false;
      }

      int length = serial.Length;
      for (int i = 0; i < serial.Length; i++)
      {
        if (serial[i] != x509.SerialNumber[--length])
        {
          return false;
        }
      }

      return true;
    }

    public bool IsTrusted()
    {
      if (this.entry == null)
      {
        this.reason = 1;
        return false;
      }

      if (this.signingCertificate == null)
      {
        this.reason = 7;
        return false;
      }

      if ((this.signerChain.Root == null) || !this.trustedRoot)
      {
        this.reason = 6;
        return false;
      }

      if (this.timestamp != DateTime.MinValue)
      {
        if ((this.timestampChain.Root == null) || !this.trustedTimestampRoot)
        {
          this.reason = 6;
          return false;
        }

        if (!this.signingCertificate.WasCurrent(this.Timestamp))
        {
          this.reason = 4;
          return false;
        }
      }
      else if (!this.signingCertificate.IsCurrent)
      {
        this.reason = 8;
        return false;
      }

      if (this.reason == -1)
      {
        this.reason = 0;
      }

      return true;
    }

    private void Reset()
    {
      this.filename = null;
      this.entry = null;
      this.hash = null;
      this.signedHash = null;
      this.signingCertificate = null;
      this.reason = -1;
      this.trustedRoot = false;
      this.trustedTimestampRoot = false;
      this.signerChain.Reset();
      this.timestampChain.Reset();
      this.timestamp = DateTime.MinValue;
    }

    private bool VerifyCounterSignature(PKCS7.SignerInfo cs, byte[] signature)
    {
      if (cs.Version == 1)
      {
        string str = null;
        ASN1 asn = null;
        for (int i = 0; i < cs.AuthenticatedAttributes.Count; i++)
        {
          ASN1 asn2 = (ASN1)cs.AuthenticatedAttributes[i];
          string key = ASN1Convert.ToOid(asn2[0]);
          if (key != null)
          {
            if (__f__switch_map3 == null)
            {
              Dictionary<string, int> dictionary = new Dictionary<string, int>(3)
                  {
                    {
                      "1.2.840.113549.1.9.3",
                      0
                    },
                    {
                      "1.2.840.113549.1.9.4",
                      1
                    },
                    {
                      "1.2.840.113549.1.9.5",
                      2
                    }
                  };
              __f__switch_map3 = dictionary;
            }
            if (__f__switch_map3.TryGetValue(key, out int num2))
            {
              switch (num2)
              {
                case 0:
                  str = ASN1Convert.ToOid(asn2[1][0]);
                  break;

                case 1:
                  asn = asn2[1][0];
                  break;

                case 2:
                  this.timestamp = ASN1Convert.ToDateTime(asn2[1][0]);
                  break;
              }
            }
          }
        }

        if (str != "1.2.840.113549.1.7.1")
        {
          return false;
        }

        if (asn == null)
        {
          return false;
        }

        string hashName = null;
        switch (asn.Length)
        {
          case 0x10:
            hashName = "MD5";
            break;

          case 20:
            hashName = "SHA1";
            break;
        }

        HashAlgorithm hash = HashAlgorithm.Create(hashName);
        if (asn.CompareValue(hash.ComputeHash(signature)))
        {
          byte[] buffer = cs.Signature;
          ASN1 asn3 = new ASN1(0x31);
          IEnumerator enumerator = cs.AuthenticatedAttributes.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              ASN1 current = (ASN1)enumerator.Current;
              asn3.Add(current);
            }
          }
          finally
          {
            if (enumerator is IDisposable disposable)
            {
              disposable.Dispose();
            }
          }

          byte[] hashValue = hash.ComputeHash(asn3.GetBytes());
          string issuerName = cs.IssuerName;
          byte[] serialNumber = cs.SerialNumber;
          X509CertificateCollection.X509CertificateEnumerator enumerator2 = this.coll.GetEnumerator();
          try
          {
            while (enumerator2.MoveNext())
            {
              X509Certificate current = enumerator2.Current;
              if (this.CompareIssuerSerial(issuerName, serialNumber, current) &&
                  (current.PublicKey.Length > buffer.Length))
              {
                RSACryptoServiceProvider rSA = (RSACryptoServiceProvider)current.RSA;
                RSAManaged rsa = new RSAManaged();
                rsa.ImportParameters(rSA.ExportParameters(false));
                if (PKCS1.Verify_v15(rsa, hash, hashValue, buffer, true))
                {
                  this.timestampChain.LoadCertificates(this.coll);
                  return this.timestampChain.Build(current);
                }
              }
            }
          }
          finally
          {
            if (enumerator2 is IDisposable disposable2)
            {
              disposable2.Dispose();
            }
          }
        }
      }

      return false;
    }

    private bool VerifySignature(PKCS7.SignedData sd, byte[] calculatedMessageDigest, HashAlgorithm ha)
    {
      string str = null;
      ASN1 asn = null;
      string str3;
      Dictionary<string, int> dictionary;
      for (int i = 0; i < sd.SignerInfo.AuthenticatedAttributes.Count; i++)
      {
        ASN1 asn2 = (ASN1)sd.SignerInfo.AuthenticatedAttributes[i];
        str3 = ASN1Convert.ToOid(asn2[0]);
        if (str3 != null)
        {
          if (__f__switch_map1 == null)
          {
            dictionary = new Dictionary<string, int>(4)
                {
                  {
                    "1.2.840.113549.1.9.3",
                    0
                  },
                  {
                    "1.2.840.113549.1.9.4",
                    1
                  },
                  {
                    "1.3.6.1.4.1.311.2.1.11",
                    2
                  },
                  {
                    "1.3.6.1.4.1.311.2.1.12",
                    3
                  }
                };
            __f__switch_map1 = dictionary;
          }
          if (__f__switch_map1.TryGetValue(str3, out int num2))
          {
            switch (num2)
            {
              case 0:
                str = ASN1Convert.ToOid(asn2[1][0]);
                break;

              case 1:
                asn = asn2[1][0];
                break;
            }
          }
        }
      }

      if (str != "1.3.6.1.4.1.311.2.1.4")
      {
        return false;
      }

      if (asn == null)
      {
        return false;
      }

      if (!asn.CompareValue(calculatedMessageDigest))
      {
        return false;
      }

      string str4 = CryptoConfig.MapNameToOID(ha.ToString());
      ASN1 asn3 = new ASN1(0x31);
      IEnumerator enumerator = sd.SignerInfo.AuthenticatedAttributes.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          ASN1 current = (ASN1)enumerator.Current;
          asn3.Add(current);
        }
      }
      finally
      {
        if (enumerator is IDisposable disposable)
        {
          disposable.Dispose();
        }
      }

      ha.Initialize();
      byte[] rgbHash = ha.ComputeHash(asn3.GetBytes());
      byte[] signature = sd.SignerInfo.Signature;
      string issuerName = sd.SignerInfo.IssuerName;
      byte[] serialNumber = sd.SignerInfo.SerialNumber;
      X509CertificateCollection.X509CertificateEnumerator enumerator2 = this.coll.GetEnumerator();
      try
      {
        while (enumerator2.MoveNext())
        {
          X509Certificate current = enumerator2.Current;
          if (this.CompareIssuerSerial(issuerName, serialNumber, current) &&
              (current.PublicKey.Length > (signature.Length >> 3)))
          {
            this.signingCertificate = current;
            RSACryptoServiceProvider rSA = (RSACryptoServiceProvider)current.RSA;
            if (rSA.VerifyHash(rgbHash, str4, signature))
            {
              this.signerChain.LoadCertificates(this.coll);
              this.trustedRoot = this.signerChain.Build(current);
              goto Label_0295;
            }
          }
        }
      }
      finally
      {
        if (enumerator2 is IDisposable disposable2)
        {
          disposable2.Dispose();
        }
      }

      Label_0295:
      if (sd.SignerInfo.UnauthenticatedAttributes.Count == 0)
      {
        this.trustedTimestampRoot = true;
      }
      else
      {
        for (int j = 0; j < sd.SignerInfo.UnauthenticatedAttributes.Count; j++)
        {
          ASN1 asn5 = (ASN1)sd.SignerInfo.UnauthenticatedAttributes[j];
          str3 = ASN1Convert.ToOid(asn5[0]);
          if (str3 != null)
          {
            if (__f__switch_map2 == null)
            {
              dictionary = new Dictionary<string, int>(1)
                  {
                    {
                      "1.2.840.113549.1.9.6",
                      0
                    }
                  };
              __f__switch_map2 = dictionary;
            }
            if (__f__switch_map2.TryGetValue(str3, out int num2) && (num2 == 0))
            {
              PKCS7.SignerInfo cs = new PKCS7.SignerInfo(asn5[1]);
              this.trustedTimestampRoot = this.VerifyCounterSignature(cs, signature);
            }
          }
        }
      }

      return (this.trustedRoot && this.trustedTimestampRoot);
    }

    public string FileName
    {
      get =>
        this.filename;
      set
      {
        this.Reset();
        try
        {
          this.CheckSignature(value);
        }
        catch (SecurityException)
        {
          throw;
        }
        catch (Exception)
        {
          this.reason = 1;
        }
      }
    }

    public byte[] Hash
    {
      get
      {
        if (this.signedHash == null)
        {
          return null;
        }

        return (byte[])this.signedHash.Value.Clone();
      }
    }

    public int Reason
    {
      get
      {
        if (this.reason == -1)
        {
          this.IsTrusted();
        }

        return this.reason;
      }
    }

    public byte[] Signature
    {
      get
      {
        if (this.entry == null)
        {
          return null;
        }

        return (byte[])this.entry.Clone();
      }
    }

    public DateTime Timestamp =>
      this.timestamp;

    public X509CertificateCollection Certificates =>
      this.coll;

    public X509Certificate SigningCertificate =>
      this.signingCertificate;
  }
}
