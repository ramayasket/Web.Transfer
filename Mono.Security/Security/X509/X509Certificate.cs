// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.X509Certificate
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;

namespace Mono.Security.X509
{
  using Mono.Security;
  using Mono.Security.Cryptography;
  using System;
  using System.Collections.Generic;
  using System.Runtime.CompilerServices;
  using System.Runtime.Serialization;
  using System.Security.Cryptography;
  using System.Security.Permissions;
  using System.Text;

  public class X509Certificate : ISerializable
  {
    private ASN1 decoder;
    private byte[] m_encodedcert;
    private DateTime m_from;
    private DateTime m_until;
    private ASN1 issuer;
    private string m_issuername;
    private string m_keyalgo;
    private byte[] m_keyalgoparams;
    private ASN1 subject;
    private string m_subject;
    private byte[] m_publickey;
    private byte[] signature;
    private string m_signaturealgo;
    private byte[] m_signaturealgoparams;
    private byte[] certhash;
    private System.Security.Cryptography.RSA _rsa;
    private System.Security.Cryptography.DSA _dsa;
    private int version;
    private byte[] serialnumber;
    private byte[] issuerUniqueID;
    private byte[] subjectUniqueID;
    private X509ExtensionCollection extensions;
    private static string encoding_error = Locale.GetText("Input data cannot be coded as a valid certificate.");
    [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_mapF;
        [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_map10;
        [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_map11;

        public X509Certificate(byte[] data)
    {
      if (data != null)
      {
        if ((data.Length > 0) && (data[0] != 0x30))
        {
          try
          {
            data = PEM("CERTIFICATE", data);
          }
          catch (Exception exception)
          {
            throw new CryptographicException(encoding_error, exception);
          }
        }
        this.Parse(data);
      }
    }

    protected X509Certificate(SerializationInfo info, StreamingContext context)
    {
      this.Parse((byte[])info.GetValue("raw", typeof(byte[])));
    }

    public bool CheckSignature(byte[] hash, string hashAlgorithm, byte[] signature)
    {
      RSACryptoServiceProvider rSA = (RSACryptoServiceProvider)this.RSA;
      return rSA.VerifyHash(hash, hashAlgorithm, signature);
    }

    public ASN1 GetIssuerName() =>
        this.issuer;

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("raw", this.m_encodedcert);
    }

    public ASN1 GetSubjectName() =>
        this.subject;

    private byte[] GetUnsignedBigInteger(byte[] integer)
    {
      if (integer[0] == 0)
      {
        int count = integer.Length - 1;
        byte[] dst = new byte[count];
        Buffer.BlockCopy(integer, 1, dst, 0, count);
        return dst;
      }
      return integer;
    }

    private void Parse(byte[] data)
    {
      try
      {
        this.decoder = new ASN1(data);
        if (this.decoder.Tag != 0x30)
        {
          throw new CryptographicException(encoding_error);
        }
        if (this.decoder[0].Tag != 0x30)
        {
          throw new CryptographicException(encoding_error);
        }
        ASN1 asn = this.decoder[0];
        int index = 0;
        ASN1 asn2 = this.decoder[0][index];
        this.version = 1;
        if ((asn2.Tag == 160) && (asn2.Count > 0))
        {
          this.version += asn2[0].Value[0];
          index++;
        }
        ASN1 asn3 = this.decoder[0][index++];
        if (asn3.Tag != 2)
        {
          throw new CryptographicException(encoding_error);
        }
        this.serialnumber = asn3.Value;
        Array.Reverse(this.serialnumber, 0, this.serialnumber.Length);
        index++;
        this.issuer = asn.Element(index++, 0x30);
        this.m_issuername = X501.ToString(this.issuer);
        ASN1 asn4 = asn.Element(index++, 0x30);
        ASN1 time = asn4[0];
        this.m_from = ASN1Convert.ToDateTime(time);
        ASN1 asn6 = asn4[1];
        this.m_until = ASN1Convert.ToDateTime(asn6);
        this.subject = asn.Element(index++, 0x30);
        this.m_subject = X501.ToString(this.subject);
        ASN1 asn7 = asn.Element(index++, 0x30);
        ASN1 asn8 = asn7.Element(0, 0x30);
        ASN1 asn9 = asn8.Element(0, 6);
        this.m_keyalgo = ASN1Convert.ToOid(asn9);
        ASN1 asn10 = asn8[1];
        this.m_keyalgoparams = (asn8.Count <= 1) ? null : asn10.GetBytes();
        ASN1 asn11 = asn7.Element(1, 3);
        int count = asn11.Length - 1;
        this.m_publickey = new byte[count];
        Buffer.BlockCopy(asn11.Value, 1, this.m_publickey, 0, count);
        byte[] src = this.decoder[2].Value;
        this.signature = new byte[src.Length - 1];
        Buffer.BlockCopy(src, 1, this.signature, 0, this.signature.Length);
        asn8 = this.decoder[1];
        asn9 = asn8.Element(0, 6);
        this.m_signaturealgo = ASN1Convert.ToOid(asn9);
        asn10 = asn8[1];
        if (asn10 != null)
        {
          this.m_signaturealgoparams = asn10.GetBytes();
        }
        else
        {
          this.m_signaturealgoparams = null;
        }
        ASN1 asn12 = asn.Element(index, 0x81);
        if (asn12 != null)
        {
          index++;
          this.issuerUniqueID = asn12.Value;
        }
        ASN1 asn13 = asn.Element(index, 130);
        if (asn13 != null)
        {
          index++;
          this.subjectUniqueID = asn13.Value;
        }
        ASN1 asn14 = asn.Element(index, 0xa3);
        if ((asn14 != null) && (asn14.Count == 1))
        {
          this.extensions = new X509ExtensionCollection(asn14[0]);
        }
        else
        {
          this.extensions = new X509ExtensionCollection(null);
        }
        this.m_encodedcert = (byte[])data.Clone();
      }
      catch (Exception exception)
      {
        throw new CryptographicException(encoding_error, exception);
      }
    }

    private static byte[] PEM(string type, byte[] data)
    {
      string str = Encoding.ASCII.GetString(data);
      string str2 = $"-----BEGIN {type}-----";
      string str3 = $"-----END {type}-----";
      int startIndex = str.IndexOf(str2) + str2.Length;
      int index = str.IndexOf(str3, startIndex);
      return Convert.FromBase64String(str.Substring(startIndex, index - startIndex));
    }

    public bool VerifySignature(AsymmetricAlgorithm aa)
    {
      if (aa == null)
      {
        throw new ArgumentNullException("aa");
      }
      if (aa is System.Security.Cryptography.RSA)
      {
        return this.VerifySignature(aa as System.Security.Cryptography.RSA);
      }
      if (!(aa is System.Security.Cryptography.DSA))
      {
        throw new NotSupportedException("Unknown Asymmetric Algorithm " + aa.ToString());
      }
      return this.VerifySignature(aa as System.Security.Cryptography.DSA);
    }

    internal bool VerifySignature(System.Security.Cryptography.DSA dsa)
    {
      DSASignatureDeformatter deformatter = new DSASignatureDeformatter(dsa);
      deformatter.SetHashAlgorithm("SHA1");
      return deformatter.VerifySignature(this.Hash, this.Signature);
    }

    internal bool VerifySignature(System.Security.Cryptography.RSA rsa)
    {
      RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(rsa);
      string signaturealgo = this.m_signaturealgo;
      if (signaturealgo != null)
      {
        if (__f__switch_map11 == null)
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
                            "1.2.840.113549.1.1.5",
                            2
                        },
                        {
                            "1.3.14.3.2.29",
                            2
                        }
                    };
                    __f__switch_map11 = dictionary;
        }
        if (__f__switch_map11.TryGetValue(signaturealgo, out int num))
                {
          switch (num)
          {
            case 0:
              deformatter.SetHashAlgorithm("MD2");
              goto Label_00CA;

            case 1:
              deformatter.SetHashAlgorithm("MD5");
              goto Label_00CA;

            case 2:
              deformatter.SetHashAlgorithm("SHA1");
              goto Label_00CA;
          }
        }
      }
      throw new CryptographicException("Unsupported hash algorithm: " + this.m_signaturealgo);
      Label_00CA:
      return deformatter.VerifySignature(this.Hash, this.Signature);
    }

    public bool WasCurrent(DateTime instant) =>
        ((instant > this.ValidFrom) && (instant <= this.ValidUntil));

    public System.Security.Cryptography.DSA DSA
    {
      get
      {
        if (this.m_keyalgoparams == null)
        {
          throw new CryptographicException("Missing key algorithm parameters.");
        }
        if (this._dsa == null)
        {
          DSAParameters parameters = new DSAParameters();
          ASN1 asn = new ASN1(this.m_publickey);
          if ((asn == null) || (asn.Tag != 2))
          {
            return null;
          }
          parameters.Y = this.GetUnsignedBigInteger(asn.Value);
          ASN1 asn2 = new ASN1(this.m_keyalgoparams);
          if (((asn2 == null) || (asn2.Tag != 0x30)) || (asn2.Count < 3))
          {
            return null;
          }
          if (((asn2[0].Tag != 2) || (asn2[1].Tag != 2)) || (asn2[2].Tag != 2))
          {
            return null;
          }
          parameters.P = this.GetUnsignedBigInteger(asn2[0].Value);
          parameters.Q = this.GetUnsignedBigInteger(asn2[1].Value);
          parameters.G = this.GetUnsignedBigInteger(asn2[2].Value);
          this._dsa = new DSACryptoServiceProvider(parameters.Y.Length << 3);
          this._dsa.ImportParameters(parameters);
        }
        return this._dsa;
      }
      set
      {
        this._dsa = value;
        if (value != null)
        {
          this._rsa = null;
        }
      }
    }

    public X509ExtensionCollection Extensions =>
        this.extensions;

    public byte[] Hash
    {
      get
      {
        if (this.certhash != null)
        {
          goto Label_00FD;
        }
        HashAlgorithm algorithm = null;
        string signaturealgo = this.m_signaturealgo;
        if (signaturealgo != null)
        {
          if (__f__switch_mapF == null)
                    {
            Dictionary<string, int> dictionary = new Dictionary<string, int>(5) {
                            {
                                "1.2.840.113549.1.1.2",
                                0
                            },
                            {
                                "1.2.840.113549.1.1.4",
                                1
                            },
                            {
                                "1.2.840.113549.1.1.5",
                                2
                            },
                            {
                                "1.3.14.3.2.29",
                                2
                            },
                            {
                                "1.2.840.10040.4.3",
                                2
                            }
                        };
                        __f__switch_mapF = dictionary;
          }
          if (__f__switch_mapF.TryGetValue(signaturealgo, out int num))
                    {
            switch (num)
            {
              case 0:
                algorithm = MD2.Create();
                goto Label_00B9;

              case 1:
                algorithm = MD5.Create();
                goto Label_00B9;

              case 2:
                algorithm = SHA1.Create();
                goto Label_00B9;
            }
          }
        }
        return null;
        Label_00B9:
        if ((this.decoder == null) || (this.decoder.Count < 1))
        {
          return null;
        }
        byte[] bytes = this.decoder[0].GetBytes();
        this.certhash = algorithm.ComputeHash(bytes, 0, bytes.Length);
        Label_00FD:
        return (byte[])this.certhash.Clone();
      }
    }

    public virtual string IssuerName =>
        this.m_issuername;

    public virtual string KeyAlgorithm =>
        this.m_keyalgo;

    public virtual byte[] KeyAlgorithmParameters
    {
      get
      {
        if (this.m_keyalgoparams == null)
        {
          return null;
        }
        return (byte[])this.m_keyalgoparams.Clone();
      }
      set => this.m_keyalgoparams = value;
    }

    public virtual byte[] PublicKey
    {
      get
      {
        if (this.m_publickey == null)
        {
          return null;
        }
        return (byte[])this.m_publickey.Clone();
      }
    }

    public virtual System.Security.Cryptography.RSA RSA
    {
      get
      {
        if (this._rsa == null)
        {
          RSAParameters parameters = new RSAParameters();
          ASN1 asn = new ASN1(this.m_publickey);
          ASN1 asn2 = asn[0];
          if ((asn2 == null) || (asn2.Tag != 2))
          {
            return null;
          }
          ASN1 asn3 = asn[1];
          if (asn3.Tag != 2)
          {
            return null;
          }
          parameters.Modulus = this.GetUnsignedBigInteger(asn2.Value);
          parameters.Exponent = asn3.Value;
          int dwKeySize = parameters.Modulus.Length << 3;
          this._rsa = new RSACryptoServiceProvider(dwKeySize);
          this._rsa.ImportParameters(parameters);
        }
        return this._rsa;
      }
      set
      {
        if (value != null)
        {
          this._dsa = null;
        }
        this._rsa = value;
      }
    }

    public virtual byte[] RawData
    {
      get
      {
        if (this.m_encodedcert == null)
        {
          return null;
        }
        return (byte[])this.m_encodedcert.Clone();
      }
    }

    public virtual byte[] SerialNumber
    {
      get
      {
        if (this.serialnumber == null)
        {
          return null;
        }
        return (byte[])this.serialnumber.Clone();
      }
    }

    public virtual byte[] Signature
    {
      get
      {
        if (this.signature == null)
        {
          return null;
        }
        string signaturealgo = this.m_signaturealgo;
        if (signaturealgo != null)
        {
          if (__f__switch_map10 == null)
                    {
            Dictionary<string, int> dictionary = new Dictionary<string, int>(5) {
                            {
                                "1.2.840.113549.1.1.2",
                                0
                            },
                            {
                                "1.2.840.113549.1.1.4",
                                0
                            },
                            {
                                "1.2.840.113549.1.1.5",
                                0
                            },
                            {
                                "1.3.14.3.2.29",
                                0
                            },
                            {
                                "1.2.840.10040.4.3",
                                1
                            }
                        };
                        __f__switch_map10 = dictionary;
          }
          if (__f__switch_map10.TryGetValue(signaturealgo, out int num))
                    {
            switch (num)
            {
              case 0:
                return (byte[])this.signature.Clone();

              case 1:
                {
                  ASN1 asn = new ASN1(this.signature);
                  if ((asn == null) || (asn.Count != 2))
                  {
                    return null;
                  }
                  byte[] src = asn[0].Value;
                  byte[] buffer2 = asn[1].Value;
                  byte[] dst = new byte[40];
                  int srcOffset = Math.Max(0, src.Length - 20);
                  int dstOffset = Math.Max(0, 20 - src.Length);
                  Buffer.BlockCopy(src, srcOffset, dst, dstOffset, src.Length - srcOffset);
                  int num4 = Math.Max(0, buffer2.Length - 20);
                  int num5 = Math.Max(20, 40 - buffer2.Length);
                  Buffer.BlockCopy(buffer2, num4, dst, num5, buffer2.Length - num4);
                  return dst;
                }
            }
          }
        }
        throw new CryptographicException("Unsupported hash algorithm: " + this.m_signaturealgo);
      }
    }

    public virtual string SignatureAlgorithm =>
        this.m_signaturealgo;

    public virtual byte[] SignatureAlgorithmParameters
    {
      get
      {
        if (this.m_signaturealgoparams == null)
        {
          return this.m_signaturealgoparams;
        }
        return (byte[])this.m_signaturealgoparams.Clone();
      }
    }

    public virtual string SubjectName =>
        this.m_subject;

    public virtual DateTime ValidFrom =>
        this.m_from;

    public virtual DateTime ValidUntil =>
        this.m_until;

    public int Version =>
        this.version;

    public bool IsCurrent =>
        this.WasCurrent(DateTime.UtcNow);

    public byte[] IssuerUniqueIdentifier
    {
      get
      {
        if (this.issuerUniqueID == null)
        {
          return null;
        }
        return (byte[])this.issuerUniqueID.Clone();
      }
    }

    public byte[] SubjectUniqueIdentifier
    {
      get
      {
        if (this.subjectUniqueID == null)
        {
          return null;
        }
        return (byte[])this.subjectUniqueID.Clone();
      }
    }

    public bool IsSelfSigned =>
        ((this.m_issuername == this.m_subject) && this.VerifySignature(this.RSA));
  }
}
