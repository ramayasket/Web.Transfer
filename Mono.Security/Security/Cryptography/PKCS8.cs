// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.PKCS8
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Collections;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography
{
  public sealed class PKCS8
  {
    private PKCS8()
    {
    }

    public static PKCS8.KeyInfo GetType(byte[] data)
    {
      if (data == null)
        throw new ArgumentNullException(nameof (data));
      PKCS8.KeyInfo keyInfo = PKCS8.KeyInfo.Unknown;
      try
      {
        ASN1 asN1 = new ASN1(data);
        if (asN1.Tag == (byte) 48)
        {
          if (asN1.Count > 0)
          {
            switch (asN1[0].Tag)
            {
              case 2:
                keyInfo = PKCS8.KeyInfo.PrivateKey;
                break;
              case 48:
                keyInfo = PKCS8.KeyInfo.EncryptedPrivateKey;
                break;
            }
          }
        }
      }
      catch
      {
        throw new CryptographicException("invalid ASN.1 data");
      }
      return keyInfo;
    }

    public enum KeyInfo
    {
      PrivateKey,
      EncryptedPrivateKey,
      Unknown,
    }

    public class PrivateKeyInfo
    {
      private int _version;
      private string _algorithm;
      private byte[] _key;
      private ArrayList _list;

      public PrivateKeyInfo()
      {
        this._version = 0;
        this._list = new ArrayList();
      }

      public PrivateKeyInfo(byte[] data)
        : this()
      {
        this.Decode(data);
      }

      public string Algorithm
      {
        get
        {
          return this._algorithm;
        }
        set
        {
          this._algorithm = value;
        }
      }

      public ArrayList Attributes
      {
        get
        {
          return this._list;
        }
      }

      public byte[] PrivateKey
      {
        get
        {
          return this._key == null ? (byte[]) null : (byte[]) this._key.Clone();
        }
        set
        {
          if (value == null)
            throw new ArgumentNullException(nameof (PrivateKey));
          this._key = (byte[]) value.Clone();
        }
      }

      public int Version
      {
        get
        {
          return this._version;
        }
        set
        {
          if (value < 0)
            throw new ArgumentOutOfRangeException("negative version");
          this._version = value;
        }
      }

      private void Decode(byte[] data)
      {
        ASN1 asN1_1 = new ASN1(data);
        if (asN1_1.Tag != (byte) 48)
          throw new CryptographicException("invalid PrivateKeyInfo");
        ASN1 asN1_2 = asN1_1[0];
        if (asN1_2.Tag != (byte) 2)
          throw new CryptographicException("invalid version");
        this._version = (int) asN1_2.Value[0];
        ASN1 asN1_3 = asN1_1[1];
        if (asN1_3.Tag != (byte) 48)
          throw new CryptographicException("invalid algorithm");
        ASN1 asn1 = asN1_3[0];
        if (asn1.Tag != (byte) 6)
          throw new CryptographicException("missing algorithm OID");
        this._algorithm = ASN1Convert.ToOid(asn1);
        this._key = asN1_1[2].Value;
        if (asN1_1.Count <= 3)
          return;
        ASN1 asN1_4 = asN1_1[3];
        for (int index = 0; index < asN1_4.Count; ++index)
          this._list.Add((object) asN1_4[index]);
      }

      public byte[] GetBytes()
      {
        ASN1 asn1_1 = new ASN1((byte) 48);
        asn1_1.Add(ASN1Convert.FromOid(this._algorithm));
        asn1_1.Add(new ASN1((byte) 5));
        ASN1 asN1 = new ASN1((byte) 48);
        asN1.Add(new ASN1((byte) 2, new byte[1]
        {
          (byte) this._version
        }));
        asN1.Add(asn1_1);
        asN1.Add(new ASN1((byte) 4, this._key));
        if (this._list.Count > 0)
        {
          ASN1 asn1_2 = new ASN1((byte) 160);
          foreach (ASN1 asn1_3 in this._list)
            asn1_2.Add(asn1_3);
          asN1.Add(asn1_2);
        }
        return asN1.GetBytes();
      }

      private static byte[] RemoveLeadingZero(byte[] bigInt)
      {
        int srcOffset = 0;
        int length = bigInt.Length;
        if (bigInt[0] == (byte) 0)
        {
          srcOffset = 1;
          --length;
        }
        byte[] numArray = new byte[length];
        Buffer.BlockCopy((Array) bigInt, srcOffset, (Array) numArray, 0, length);
        return numArray;
      }

      private static byte[] Normalize(byte[] bigInt, int length)
      {
        if (bigInt.Length == length)
          return bigInt;
        if (bigInt.Length > length)
          return PKCS8.PrivateKeyInfo.RemoveLeadingZero(bigInt);
        byte[] numArray = new byte[length];
        Buffer.BlockCopy((Array) bigInt, 0, (Array) numArray, length - bigInt.Length, bigInt.Length);
        return numArray;
      }

      public static RSA DecodeRSA(byte[] keypair)
      {
        ASN1 asN1 = new ASN1(keypair);
        if (asN1.Tag != (byte) 48)
          throw new CryptographicException("invalid private key format");
        if (asN1[0].Tag != (byte) 2)
          throw new CryptographicException("missing version");
        if (asN1.Count < 9)
          throw new CryptographicException("not enough key parameters");
        RSAParameters parameters = new RSAParameters();
        parameters.Modulus = PKCS8.PrivateKeyInfo.RemoveLeadingZero(asN1[1].Value);
        int length1 = parameters.Modulus.Length;
        int length2 = length1 >> 1;
        parameters.D = PKCS8.PrivateKeyInfo.Normalize(asN1[3].Value, length1);
        parameters.DP = PKCS8.PrivateKeyInfo.Normalize(asN1[6].Value, length2);
        parameters.DQ = PKCS8.PrivateKeyInfo.Normalize(asN1[7].Value, length2);
        parameters.Exponent = PKCS8.PrivateKeyInfo.RemoveLeadingZero(asN1[2].Value);
        parameters.InverseQ = PKCS8.PrivateKeyInfo.Normalize(asN1[8].Value, length2);
        parameters.P = PKCS8.PrivateKeyInfo.Normalize(asN1[4].Value, length2);
        parameters.Q = PKCS8.PrivateKeyInfo.Normalize(asN1[5].Value, length2);
        RSA rsa;
        try
        {
          rsa = RSA.Create();
          rsa.ImportParameters(parameters);
        }
        catch (CryptographicException)
        {
          rsa = (RSA) new RSACryptoServiceProvider(new CspParameters()
          {
            Flags = CspProviderFlags.UseMachineKeyStore
          });
          rsa.ImportParameters(parameters);
        }
        return rsa;
      }

      public static byte[] Encode(RSA rsa)
      {
        RSAParameters rsaParameters = rsa.ExportParameters(true);
        ASN1 asN1 = new ASN1((byte) 48);
        asN1.Add(new ASN1((byte) 2, new byte[1]));
        asN1.Add(ASN1Convert.FromUnsignedBigInteger(rsaParameters.Modulus));
        asN1.Add(ASN1Convert.FromUnsignedBigInteger(rsaParameters.Exponent));
        asN1.Add(ASN1Convert.FromUnsignedBigInteger(rsaParameters.D));
        asN1.Add(ASN1Convert.FromUnsignedBigInteger(rsaParameters.P));
        asN1.Add(ASN1Convert.FromUnsignedBigInteger(rsaParameters.Q));
        asN1.Add(ASN1Convert.FromUnsignedBigInteger(rsaParameters.DP));
        asN1.Add(ASN1Convert.FromUnsignedBigInteger(rsaParameters.DQ));
        asN1.Add(ASN1Convert.FromUnsignedBigInteger(rsaParameters.InverseQ));
        return asN1.GetBytes();
      }

      public static DSA DecodeDSA(byte[] privateKey, DSAParameters dsaParameters)
      {
        ASN1 asN1 = new ASN1(privateKey);
        if (asN1.Tag != (byte) 2)
          throw new CryptographicException("invalid private key format");
        dsaParameters.X = PKCS8.PrivateKeyInfo.Normalize(asN1.Value, 20);
        DSA dsa = DSA.Create();
        dsa.ImportParameters(dsaParameters);
        return dsa;
      }

      public static byte[] Encode(DSA dsa)
      {
        return ASN1Convert.FromUnsignedBigInteger(dsa.ExportParameters(true).X).GetBytes();
      }

      public static byte[] Encode(AsymmetricAlgorithm aa)
      {
        switch (aa)
        {
          case RSA _:
            return PKCS8.PrivateKeyInfo.Encode((RSA) aa);
          case DSA _:
            return PKCS8.PrivateKeyInfo.Encode((DSA) aa);
          default:
            throw new CryptographicException("Unknown asymmetric algorithm {0}", aa.ToString());
        }
      }
    }

    public class EncryptedPrivateKeyInfo
    {
      private string _algorithm;
      private byte[] _salt;
      private int _iterations;
      private byte[] _data;

      public EncryptedPrivateKeyInfo()
      {
      }

      public EncryptedPrivateKeyInfo(byte[] data)
        : this()
      {
        this.Decode(data);
      }

      public string Algorithm
      {
        get
        {
          return this._algorithm;
        }
        set
        {
          this._algorithm = value;
        }
      }

      public byte[] EncryptedData
      {
        get
        {
          return this._data == null ? (byte[]) null : (byte[]) this._data.Clone();
        }
        set
        {
          this._data = value != null ? (byte[]) value.Clone() : (byte[]) null;
        }
      }

      public byte[] Salt
      {
        get
        {
          if (this._salt == null)
          {
            RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
            this._salt = new byte[8];
            randomNumberGenerator.GetBytes(this._salt);
          }
          return (byte[]) this._salt.Clone();
        }
        set
        {
          this._salt = (byte[]) value.Clone();
        }
      }

      public int IterationCount
      {
        get
        {
          return this._iterations;
        }
        set
        {
          if (value < 0)
            throw new ArgumentOutOfRangeException(nameof (IterationCount), "Negative");
          this._iterations = value;
        }
      }

      private void Decode(byte[] data)
      {
        ASN1 asN1_1 = new ASN1(data);
        if (asN1_1.Tag != (byte) 48)
          throw new CryptographicException("invalid EncryptedPrivateKeyInfo");
        ASN1 asN1_2 = asN1_1[0];
        if (asN1_2.Tag != (byte) 48)
          throw new CryptographicException("invalid encryptionAlgorithm");
        ASN1 asn1_1 = asN1_2[0];
        if (asn1_1.Tag != (byte) 6)
          throw new CryptographicException("invalid algorithm");
        this._algorithm = ASN1Convert.ToOid(asn1_1);
        if (asN1_2.Count > 1)
        {
          ASN1 asN1_3 = asN1_2[1];
          if (asN1_3.Tag != (byte) 48)
            throw new CryptographicException("invalid parameters");
          ASN1 asN1_4 = asN1_3[0];
          if (asN1_4.Tag != (byte) 4)
            throw new CryptographicException("invalid salt");
          this._salt = asN1_4.Value;
          ASN1 asn1_2 = asN1_3[1];
          if (asn1_2.Tag != (byte) 2)
            throw new CryptographicException("invalid iterationCount");
          this._iterations = ASN1Convert.ToInt32(asn1_2);
        }
        ASN1 asN1_5 = asN1_1[1];
        if (asN1_5.Tag != (byte) 4)
          throw new CryptographicException("invalid EncryptedData");
        this._data = asN1_5.Value;
      }

      public byte[] GetBytes()
      {
        if (this._algorithm == null)
          throw new CryptographicException("No algorithm OID specified");
        ASN1 asn1_1 = new ASN1((byte) 48);
        asn1_1.Add(ASN1Convert.FromOid(this._algorithm));
        if (this._iterations > 0 || this._salt != null)
        {
          ASN1 asn1_2 = new ASN1((byte) 4, this._salt);
          ASN1 asn1_3 = ASN1Convert.FromInt32(this._iterations);
          ASN1 asn1_4 = new ASN1((byte) 48);
          asn1_4.Add(asn1_2);
          asn1_4.Add(asn1_3);
          asn1_1.Add(asn1_4);
        }
        ASN1 asn1_5 = new ASN1((byte) 4, this._data);
        ASN1 asN1 = new ASN1((byte) 48);
        asN1.Add(asn1_1);
        asN1.Add(asn1_5);
        return asN1.GetBytes();
      }
    }
  }
}
