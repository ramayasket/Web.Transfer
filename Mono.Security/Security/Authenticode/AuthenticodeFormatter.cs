// Decompiled with JetBrains decompiler
// Type: Mono.Security.Authenticode.AuthenticodeFormatter
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.X509;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security.Authenticode
{
  using Mono.Security;
  using Mono.Security.X509;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Globalization;
  using System.IO;
  using System.Net;
  using System.Runtime.CompilerServices;
  using System.Security.Cryptography;
  using System.Text;

  public class AuthenticodeFormatter : AuthenticodeBase
  {
    private const string signedData = "1.2.840.113549.1.7.2";
    private const string countersignature = "1.2.840.113549.1.9.6";
    private const string spcStatementType = "1.3.6.1.4.1.311.2.1.11";
    private const string spcSpOpusInfo = "1.3.6.1.4.1.311.2.1.12";
    private const string spcPelmageData = "1.3.6.1.4.1.311.2.1.15";
    private const string commercialCodeSigning = "1.3.6.1.4.1.311.2.1.22";
    private const string timestampCountersignature = "1.3.6.1.4.1.311.3.2.1";
    private Mono.Security.Authenticode.Authority authority = Mono.Security.Authenticode.Authority.Maximum;
    private X509CertificateCollection certs = new X509CertificateCollection();
    private ArrayList crls = new ArrayList();
    private string hash;
    private System.Security.Cryptography.RSA rsa;
    private Uri timestamp;
    private ASN1 authenticode;
    private PKCS7.SignedData pkcs7 = new PKCS7.SignedData();
    private string description;
    private Uri url;
    private static byte[] obsolete = new byte[] {
            3, 1, 0, 160, 0x20, 0xa2, 30, 0x80, 0x1c, 0, 60, 0, 60, 0, 60, 0,
            0x4f, 0, 0x62, 0, 0x73, 0, 0x6f, 0, 0x6c, 0, 0x65, 0, 0x74, 0, 0x65, 0,
            0x3e, 0, 0x3e, 0, 0x3e, 0, 0, 0
        };
    [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_map4;

    private ASN1 AlgorithmIdentifier(string oid)
    {
      ASN1 asn = new ASN1(0x30);
      asn.Add(ASN1Convert.FromOid(oid));
      asn.Add(new ASN1(5));
      return asn;
    }

    private ASN1 Attribute(string oid, ASN1 value)
    {
      ASN1 asn = new ASN1(0x30);
      asn.Add(ASN1Convert.FromOid(oid));
      asn.Add(new ASN1(0x31)).Add(value);
      return asn;
    }

    private byte[] Header(byte[] fileHash, string hashAlgorithm)
    {
      string oid = CryptoConfig.MapNameToOID(hashAlgorithm);
      ASN1 asn = new ASN1(0x30);
      ASN1 asn2 = asn.Add(new ASN1(0x30));
      asn2.Add(ASN1Convert.FromOid("1.3.6.1.4.1.311.2.1.15"));
      asn2.Add(new ASN1(0x30, obsolete));
      ASN1 asn3 = asn.Add(new ASN1(0x30));
      asn3.Add(this.AlgorithmIdentifier(oid));
      asn3.Add(new ASN1(4, fileHash));
      this.pkcs7.HashName = hashAlgorithm;
      this.pkcs7.Certificates.AddRange(this.certs);
      this.pkcs7.ContentInfo.ContentType = "1.3.6.1.4.1.311.2.1.4";
      this.pkcs7.ContentInfo.Content.Add(asn);
      this.pkcs7.SignerInfo.Certificate = this.certs[0];
      this.pkcs7.SignerInfo.Key = this.rsa;
      ASN1 asn4 = null;
      if (this.url == null)
      {
        asn4 = this.Attribute("1.3.6.1.4.1.311.2.1.12", this.Opus(this.description, null));
      }
      else
      {
        asn4 = this.Attribute("1.3.6.1.4.1.311.2.1.12", this.Opus(this.description, this.url.ToString()));
      }
      this.pkcs7.SignerInfo.AuthenticatedAttributes.Add(asn4);
      this.pkcs7.GetASN1();
      return this.pkcs7.SignerInfo.Signature;
    }

    private ASN1 Opus(string description, string url)
    {
      ASN1 asn = new ASN1(0x30);
      if (description != null)
      {
        asn.Add(new ASN1(160)).Add(new ASN1(0x80, Encoding.BigEndianUnicode.GetBytes(description)));
      }
      if (url != null)
      {
        asn.Add(new ASN1(0xa1)).Add(new ASN1(0x80, Encoding.ASCII.GetBytes(url)));
      }
      return asn;
    }

    public void ProcessTimestamp(byte[] response)
    {
      ASN1 asn = new ASN1(Convert.FromBase64String(Encoding.ASCII.GetString(response)));
      for (int i = 0; i < asn[1][0][3].Count; i++)
      {
        this.pkcs7.Certificates.Add(new X509Certificate(asn[1][0][3][i].GetBytes()));
      }
      this.pkcs7.SignerInfo.UnauthenticatedAttributes.Add(this.Attribute("1.2.840.113549.1.9.6", asn[1][0][4][0]));
    }

    private bool Save(string fileName, byte[] asn)
    {
      System.IO.File.Copy(fileName, fileName + ".bak", true);
      using (FileStream stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.ReadWrite))
      {
        int securityOffset;
        if (base.SecurityOffset > 0)
        {
          securityOffset = base.SecurityOffset;
        }
        else if (base.CoffSymbolTableOffset > 0)
        {
          stream.Seek((long)(base.PEOffset + 12), SeekOrigin.Begin);
          for (int i = 0; i < 8; i++)
          {
            stream.WriteByte(0);
          }
          securityOffset = base.CoffSymbolTableOffset;
        }
        else
        {
          securityOffset = (int)stream.Length;
        }
        int num3 = securityOffset & 7;
        if (num3 > 0)
        {
          num3 = 8 - num3;
        }
        byte[] bytes = BitConverterLE.GetBytes((int)(securityOffset + num3));
        stream.Seek((long)(base.PEOffset + 0x98), SeekOrigin.Begin);
        stream.Write(bytes, 0, 4);
        int num4 = asn.Length + 8;
        int num5 = num4 & 7;
        if (num5 > 0)
        {
          num5 = 8 - num5;
        }
        bytes = BitConverterLE.GetBytes((int)(num4 + num5));
        stream.Seek((long)(base.PEOffset + 0x9c), SeekOrigin.Begin);
        stream.Write(bytes, 0, 4);
        stream.Seek((long)securityOffset, SeekOrigin.Begin);
        if (num3 > 0)
        {
          byte[] buffer = new byte[num3];
          stream.Write(buffer, 0, buffer.Length);
        }
        stream.Write(bytes, 0, bytes.Length);
        bytes = BitConverterLE.GetBytes(0x20200);
        stream.Write(bytes, 0, bytes.Length);
        stream.Write(asn, 0, asn.Length);
        if (num5 > 0)
        {
          byte[] buffer = new byte[num5];
          stream.Write(buffer, 0, buffer.Length);
        }
        stream.Close();
      }
      return true;
    }

    public bool Sign(string fileName)
    {
      try
      {
        base.Open(fileName);
        HashAlgorithm hash = HashAlgorithm.Create(this.Hash);
        byte[] fileHash = base.GetHash(hash);
        byte[] signature = this.Header(fileHash, this.Hash);
        if (this.timestamp != null)
        {
          byte[] response = this.Timestamp(signature);
          this.ProcessTimestamp(response);
        }
        PKCS7.ContentInfo info = new PKCS7.ContentInfo("1.2.840.113549.1.7.2");
        info.Content.Add(this.pkcs7.ASN1);
        this.authenticode = info.ASN1;
        base.Close();
        return this.Save(fileName, this.authenticode.GetBytes());
      }
      catch (Exception exception)
      {
        Console.WriteLine(exception);
      }
      return false;
    }

    private byte[] Timestamp(byte[] signature)
    {
      ASN1 asn = this.TimestampRequest(signature);
      WebClient client = new WebClient();
      client.Headers.Add("Content-Type", "application/octet-stream");
      client.Headers.Add("Accept", "application/octet-stream");
      byte[] bytes = Encoding.ASCII.GetBytes(Convert.ToBase64String(asn.GetBytes()));
      return client.UploadData(this.timestamp.ToString(), bytes);
    }

    public bool Timestamp(string fileName)
    {
      try
      {
        AuthenticodeDeformatter deformatter = new AuthenticodeDeformatter(fileName);
        byte[] signature = deformatter.Signature;
        if (signature != null)
        {
          base.Open(fileName);
          PKCS7.ContentInfo info = new PKCS7.ContentInfo(signature);
          this.pkcs7 = new PKCS7.SignedData(info.Content);
          byte[] bytes = this.Timestamp(this.pkcs7.SignerInfo.Signature);
          ASN1 asn = new ASN1(Convert.FromBase64String(Encoding.ASCII.GetString(bytes)));
          ASN1 asn2 = new ASN1(signature);
          ASN1 asn3 = asn2.Element(1, 160);
          if (asn3 == null)
          {
            return false;
          }
          ASN1 asn4 = asn3.Element(0, 0x30);
          if (asn4 == null)
          {
            return false;
          }
          ASN1 asn5 = asn4.Element(3, 160);
          if (asn5 == null)
          {
            asn5 = new ASN1(160);
            asn4.Add(asn5);
          }
          for (int i = 0; i < asn[1][0][3].Count; i++)
          {
            asn5.Add(asn[1][0][3][i]);
          }
          ASN1 asn6 = asn4[asn4.Count - 1];
          ASN1 asn7 = asn6[0];
          ASN1 asn8 = asn7[asn7.Count - 1];
          if (asn8.Tag != 0xa1)
          {
            asn8 = new ASN1(0xa1);
            asn7.Add(asn8);
          }
          asn8.Add(this.Attribute("1.2.840.113549.1.9.6", asn[1][0][4][0]));
          return this.Save(fileName, asn2.GetBytes());
        }
      }
      catch (Exception exception)
      {
        Console.WriteLine(exception);
      }
      return false;
    }

    public ASN1 TimestampRequest(byte[] signature)
    {
      PKCS7.ContentInfo info = new PKCS7.ContentInfo("1.2.840.113549.1.7.1");
      info.Content.Add(new ASN1(4, signature));
      return PKCS7.AlgorithmIdentifier("1.3.6.1.4.1.311.3.2.1", info.ASN1);
    }

    public Mono.Security.Authenticode.Authority Authority
    {
      get =>
          this.authority;
      set => this.authority = value;
    }

    public X509CertificateCollection Certificates =>
        this.certs;

    public ArrayList Crl =>
        this.crls;

    public string Hash
    {
      get
      {
        if (this.hash == null)
        {
          this.hash = "MD5";
        }
        return this.hash;
      }
      set
      {
        if (value == null)
        {
          throw new ArgumentNullException("Hash");
        }
        string str = value.ToUpper(CultureInfo.InvariantCulture);
        string key = str;
        if (key != null)
        {
          if (__f__switch_map4 == null)
                    {
            Dictionary<string, int> dictionary = new Dictionary<string, int>(2) {
                            {
                                "MD5",
                                0
                            },
                            {
                                "SHA1",
                                0
                            }
                        };
                        __f__switch_map4 = dictionary;
          }
          if (__f__switch_map4.TryGetValue(key, out int num) && (num == 0))
                    {
            this.hash = str;
            return;
          }
        }
        throw new ArgumentException("Invalid Authenticode hash algorithm");
      }
    }

    public System.Security.Cryptography.RSA RSA
    {
      get =>
          this.rsa;
      set => this.rsa = value;
    }

    public Uri TimestampUrl
    {
      get =>
          this.timestamp;
      set => this.timestamp = value;
    }

    public string Description
    {
      get =>
          this.description;
      set => this.description = value;
    }

    public Uri Url
    {
      get =>
          this.url;
      set => this.url = value;
    }
  }
}
