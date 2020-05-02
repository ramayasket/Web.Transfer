// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.X509Crl
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.X509.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Mono.Security.X509
{
  using Mono.Security;
  using Mono.Security.X509.Extensions;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Security.Cryptography;

  public class X509Crl
  {
    private string issuer;
    private byte version;
    private DateTime thisUpdate;
    private DateTime nextUpdate;
    private ArrayList entries;
    private string signatureOID;
    private byte[] signature;
    private X509ExtensionCollection extensions;
    private byte[] encoded;
    private byte[] hash_value;
    [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_map12;
        [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_map13;

        public X509Crl(byte[] crl)
    {
      if (crl == null)
      {
        throw new ArgumentNullException("crl");
      }
      this.encoded = (byte[])crl.Clone();
      this.Parse(this.encoded);
    }

    private bool Compare(byte[] array1, byte[] array2)
    {
      if ((array1 != null) || (array2 != null))
      {
        if ((array1 == null) || (array2 == null))
        {
          return false;
        }
        if (array1.Length != array2.Length)
        {
          return false;
        }
        for (int i = 0; i < array1.Length; i++)
        {
          if (array1[i] != array2[i])
          {
            return false;
          }
        }
      }
      return true;
    }

    public static X509Crl CreateFromFile(string filename)
    {
      byte[] buffer = null;
      using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        stream.Close();
      }
      return new X509Crl(buffer);
    }

    public byte[] GetBytes() =>
        ((byte[])this.encoded.Clone());

    public X509CrlEntry GetCrlEntry(X509Certificate x509)
    {
      if (x509 == null)
      {
        throw new ArgumentNullException("x509");
      }
      return this.GetCrlEntry(x509.SerialNumber);
    }

    public X509CrlEntry GetCrlEntry(byte[] serialNumber)
    {
      if (serialNumber == null)
      {
        throw new ArgumentNullException("serialNumber");
      }
      for (int i = 0; i < this.entries.Count; i++)
      {
        X509CrlEntry entry = (X509CrlEntry)this.entries[i];
        if (this.Compare(serialNumber, entry.SerialNumber))
        {
          return entry;
        }
      }
      return null;
    }

    private string GetHashName()
    {
      string signatureOID = this.signatureOID;
      if (signatureOID != null)
      {
        if (__f__switch_map13 == null)
                {
          Dictionary<string, int> dictionary = new Dictionary<string, int>(4) {
                        {
                            "1.2.840.113549.1.1.2",
                            0
                        },
                        {
                            "1.2.840.113549.1.1.4",
                            1
                        },
                        {
                            "1.2.840.10040.4.3",
                            2
                        },
                        {
                            "1.2.840.113549.1.1.5",
                            2
                        }
                    };
                    __f__switch_map13 = dictionary;
        }
        if (__f__switch_map13.TryGetValue(signatureOID, out int num))
                {
          switch (num)
          {
            case 0:
              return "MD2";

            case 1:
              return "MD5";

            case 2:
              return "SHA1";
          }
        }
      }
      throw new CryptographicException("Unsupported hash algorithm: " + this.signatureOID);
    }

    private void Parse(byte[] crl)
    {
      string message = "Input data cannot be coded as a valid CRL.";
      try
      {
        ASN1 asn = new ASN1(this.encoded);
        if ((asn.Tag != 0x30) || (asn.Count != 3))
        {
          throw new CryptographicException(message);
        }
        ASN1 asn2 = asn[0];
        if ((asn2.Tag != 0x30) || (asn2.Count < 3))
        {
          throw new CryptographicException(message);
        }
        int num = 0;
        if (asn2[num].Tag == 2)
        {
          this.version = (byte)(asn2[num++].Value[0] + 1);
        }
        else
        {
          this.version = 1;
        }
        this.signatureOID = ASN1Convert.ToOid(asn2[num++][0]);
        this.issuer = X501.ToString(asn2[num++]);
        this.thisUpdate = ASN1Convert.ToDateTime(asn2[num++]);
        ASN1 time = asn2[num++];
        if ((time.Tag == 0x17) || (time.Tag == 0x18))
        {
          this.nextUpdate = ASN1Convert.ToDateTime(time);
          time = asn2[num++];
        }
        this.entries = new ArrayList();
        if ((time != null) && (time.Tag == 0x30))
        {
          ASN1 asn4 = time;
          for (int i = 0; i < asn4.Count; i++)
          {
            this.entries.Add(new X509CrlEntry(asn4[i]));
          }
        }
        else
        {
          num--;
        }
        ASN1 asn5 = asn2[num];
        if (((asn5 != null) && (asn5.Tag == 160)) && (asn5.Count == 1))
        {
          this.extensions = new X509ExtensionCollection(asn5[0]);
        }
        else
        {
          this.extensions = new X509ExtensionCollection(null);
        }
        string str2 = ASN1Convert.ToOid(asn[1][0]);
        if (this.signatureOID != str2)
        {
          throw new CryptographicException(message + " [Non-matching signature algorithms in CRL]");
        }
        byte[] src = asn[2].Value;
        this.signature = new byte[src.Length - 1];
        Buffer.BlockCopy(src, 1, this.signature, 0, this.signature.Length);
      }
      catch
      {
        throw new CryptographicException(message);
      }
    }

    public bool VerifySignature(X509Certificate x509)
    {
      if (x509 == null)
      {
        throw new ArgumentNullException("x509");
      }
      if (x509.Version >= 3)
      {
        X509Extension extension = x509.Extensions["2.5.29.15"];
        if (extension != null)
        {
          KeyUsageExtension extension2 = new KeyUsageExtension(extension);
          if (!extension2.Support(KeyUsages.cRLSign))
          {
            return false;
          }
        }
        extension = x509.Extensions["2.5.29.19"];
        if (extension != null)
        {
          BasicConstraintsExtension extension3 = new BasicConstraintsExtension(extension);
          if (!extension3.CertificateAuthority)
          {
            return false;
          }
        }
      }
      if (this.issuer != x509.SubjectName)
      {
        return false;
      }
      string signatureOID = this.signatureOID;
      if (signatureOID != null)
      {
        if (__f__switch_map12 == null)
                {
          Dictionary<string, int> dictionary = new Dictionary<string, int>(1) {
                        {
                            "1.2.840.10040.4.3",
                            0
                        }
                    };
                    __f__switch_map12 = dictionary;
        }
        if (__f__switch_map12.TryGetValue(signatureOID, out int num) && (num == 0))
                {
          return this.VerifySignature(x509.DSA);
        }
      }
      return this.VerifySignature(x509.RSA);
    }

    public bool VerifySignature(AsymmetricAlgorithm aa)
    {
      if (aa == null)
      {
        throw new ArgumentNullException("aa");
      }
      if (aa is RSA)
      {
        return this.VerifySignature(aa as RSA);
      }
      if (!(aa is DSA))
      {
        throw new NotSupportedException("Unknown Asymmetric Algorithm " + aa.ToString());
      }
      return this.VerifySignature(aa as DSA);
    }

    internal bool VerifySignature(DSA dsa)
    {
      if (this.signatureOID != "1.2.840.10040.4.3")
      {
        throw new CryptographicException("Unsupported hash algorithm: " + this.signatureOID);
      }
      DSASignatureDeformatter deformatter = new DSASignatureDeformatter(dsa);
      deformatter.SetHashAlgorithm("SHA1");
      ASN1 asn = new ASN1(this.signature);
      if ((asn == null) || (asn.Count != 2))
      {
        return false;
      }
      byte[] src = asn[0].Value;
      byte[] buffer2 = asn[1].Value;
      byte[] dst = new byte[40];
      int srcOffset = Math.Max(0, src.Length - 20);
      int dstOffset = Math.Max(0, 20 - src.Length);
      Buffer.BlockCopy(src, srcOffset, dst, dstOffset, src.Length - srcOffset);
      int num3 = Math.Max(0, buffer2.Length - 20);
      int num4 = Math.Max(20, 40 - buffer2.Length);
      Buffer.BlockCopy(buffer2, num3, dst, num4, buffer2.Length - num3);
      return deformatter.VerifySignature(this.Hash, dst);
    }

    internal bool VerifySignature(RSA rsa)
    {
      RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(rsa);
      deformatter.SetHashAlgorithm(this.GetHashName());
      return deformatter.VerifySignature(this.Hash, this.signature);
    }

    public bool WasCurrent(DateTime instant)
    {
      if (this.nextUpdate == DateTime.MinValue)
      {
        return (instant >= this.thisUpdate);
      }
      return ((instant >= this.thisUpdate) && (instant <= this.nextUpdate));
    }

    public ArrayList Entries =>
        ArrayList.ReadOnly(this.entries);

    public X509CrlEntry this[int index] =>
        ((X509CrlEntry)this.entries[index]);

    public X509CrlEntry this[byte[] serialNumber] =>
        this.GetCrlEntry(serialNumber);

    public X509ExtensionCollection Extensions =>
        this.extensions;

    public byte[] Hash
    {
      get
      {
        if (this.hash_value == null)
        {
          ASN1 asn = new ASN1(this.encoded);
          byte[] bytes = asn[0].GetBytes();
          this.hash_value = HashAlgorithm.Create(this.GetHashName()).ComputeHash(bytes);
        }
        return this.hash_value;
      }
    }

    public string IssuerName =>
        this.issuer;

    public DateTime NextUpdate =>
        this.nextUpdate;

    public DateTime ThisUpdate =>
        this.thisUpdate;

    public string SignatureAlgorithm =>
        this.signatureOID;

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
    }

    public byte[] RawData =>
        ((byte[])this.encoded.Clone());

    public byte Version =>
        this.version;

    public bool IsCurrent =>
        this.WasCurrent(DateTime.Now);

    public class X509CrlEntry
    {
      private byte[] sn;
      private DateTime revocationDate;
      private X509ExtensionCollection extensions;

      internal X509CrlEntry(ASN1 entry)
      {
        this.sn = entry[0].Value;
        Array.Reverse(this.sn);
        this.revocationDate = ASN1Convert.ToDateTime(entry[1]);
        this.extensions = new X509ExtensionCollection(entry[2]);
      }

      internal X509CrlEntry(byte[] serialNumber, DateTime revocationDate, X509ExtensionCollection extensions)
      {
        this.sn = serialNumber;
        this.revocationDate = revocationDate;
        if (extensions == null)
        {
          this.extensions = new X509ExtensionCollection();
        }
        else
        {
          this.extensions = extensions;
        }
      }

      public byte[] GetBytes()
      {
        ASN1 asn = new ASN1(0x30);
        asn.Add(new ASN1(2, this.sn));
        asn.Add(ASN1Convert.FromDateTime(this.revocationDate));
        if (this.extensions.Count > 0)
        {
          asn.Add(new ASN1(this.extensions.GetBytes()));
        }
        return asn.GetBytes();
      }

      public byte[] SerialNumber =>
          ((byte[])this.sn.Clone());

      public DateTime RevocationDate =>
          this.revocationDate;

      public X509ExtensionCollection Extensions =>
          this.extensions;
    }
  }
}
