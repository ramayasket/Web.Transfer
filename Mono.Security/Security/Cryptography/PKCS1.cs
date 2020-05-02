// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.PKCS1
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography
{
  public sealed class PKCS1
  {
    private static byte[] emptySHA1 = new byte[20]
    {
      (byte) 218,
      (byte) 57,
      (byte) 163,
      (byte) 238,
      (byte) 94,
      (byte) 107,
      (byte) 75,
      (byte) 13,
      (byte) 50,
      (byte) 85,
      (byte) 191,
      (byte) 239,
      (byte) 149,
      (byte) 96,
      (byte) 24,
      (byte) 144,
      (byte) 175,
      (byte) 216,
      (byte) 7,
      (byte) 9
    };
    private static byte[] emptySHA256 = new byte[32]
    {
      (byte) 227,
      (byte) 176,
      (byte) 196,
      (byte) 66,
      (byte) 152,
      (byte) 252,
      (byte) 28,
      (byte) 20,
      (byte) 154,
      (byte) 251,
      (byte) 244,
      (byte) 200,
      (byte) 153,
      (byte) 111,
      (byte) 185,
      (byte) 36,
      (byte) 39,
      (byte) 174,
      (byte) 65,
      (byte) 228,
      (byte) 100,
      (byte) 155,
      (byte) 147,
      (byte) 76,
      (byte) 164,
      (byte) 149,
      (byte) 153,
      (byte) 27,
      (byte) 120,
      (byte) 82,
      (byte) 184,
      (byte) 85
    };
    private static byte[] emptySHA384 = new byte[48]
    {
      (byte) 56,
      (byte) 176,
      (byte) 96,
      (byte) 167,
      (byte) 81,
      (byte) 172,
      (byte) 150,
      (byte) 56,
      (byte) 76,
      (byte) 217,
      (byte) 50,
      (byte) 126,
      (byte) 177,
      (byte) 177,
      (byte) 227,
      (byte) 106,
      (byte) 33,
      (byte) 253,
      (byte) 183,
      (byte) 17,
      (byte) 20,
      (byte) 190,
      (byte) 7,
      (byte) 67,
      (byte) 76,
      (byte) 12,
      (byte) 199,
      (byte) 191,
      (byte) 99,
      (byte) 246,
      (byte) 225,
      (byte) 218,
      (byte) 39,
      (byte) 78,
      (byte) 222,
      (byte) 191,
      (byte) 231,
      (byte) 111,
      (byte) 101,
      (byte) 251,
      (byte) 213,
      (byte) 26,
      (byte) 210,
      (byte) 241,
      (byte) 72,
      (byte) 152,
      (byte) 185,
      (byte) 91
    };
    private static byte[] emptySHA512 = new byte[64]
    {
      (byte) 207,
      (byte) 131,
      (byte) 225,
      (byte) 53,
      (byte) 126,
      (byte) 239,
      (byte) 184,
      (byte) 189,
      (byte) 241,
      (byte) 84,
      (byte) 40,
      (byte) 80,
      (byte) 214,
      (byte) 109,
      (byte) 128,
      (byte) 7,
      (byte) 214,
      (byte) 32,
      (byte) 228,
      (byte) 5,
      (byte) 11,
      (byte) 87,
      (byte) 21,
      (byte) 220,
      (byte) 131,
      (byte) 244,
      (byte) 169,
      (byte) 33,
      (byte) 211,
      (byte) 108,
      (byte) 233,
      (byte) 206,
      (byte) 71,
      (byte) 208,
      (byte) 209,
      (byte) 60,
      (byte) 93,
      (byte) 133,
      (byte) 242,
      (byte) 176,
      byte.MaxValue,
      (byte) 131,
      (byte) 24,
      (byte) 210,
      (byte) 135,
      (byte) 126,
      (byte) 236,
      (byte) 47,
      (byte) 99,
      (byte) 185,
      (byte) 49,
      (byte) 189,
      (byte) 71,
      (byte) 65,
      (byte) 122,
      (byte) 129,
      (byte) 165,
      (byte) 56,
      (byte) 50,
      (byte) 122,
      (byte) 249,
      (byte) 39,
      (byte) 218,
      (byte) 62
    };

    private PKCS1()
    {
    }

    private static bool Compare(byte[] array1, byte[] array2)
    {
      bool flag = array1.Length == array2.Length;
      if (flag)
      {
        for (int index = 0; index < array1.Length; ++index)
        {
          if ((int) array1[index] != (int) array2[index])
            return false;
        }
      }
      return flag;
    }

    private static byte[] xor(byte[] array1, byte[] array2)
    {
      byte[] numArray = new byte[array1.Length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = (byte) ((uint) array1[index] ^ (uint) array2[index]);
      return numArray;
    }

    private static byte[] GetEmptyHash(HashAlgorithm hash)
    {
      switch (hash)
      {
        case SHA1 _:
          return PKCS1.emptySHA1;
        case SHA256 _:
          return PKCS1.emptySHA256;
        case SHA384 _:
          return PKCS1.emptySHA384;
        case SHA512 _:
          return PKCS1.emptySHA512;
        default:
          return hash.ComputeHash((byte[]) null);
      }
    }

    public static byte[] I2OSP(int x, int size)
    {
      byte[] bytes = BitConverterLE.GetBytes(x);
      Array.Reverse((Array) bytes, 0, bytes.Length);
      return PKCS1.I2OSP(bytes, size);
    }

    public static byte[] I2OSP(byte[] x, int size)
    {
      byte[] numArray = new byte[size];
      Buffer.BlockCopy((Array) x, 0, (Array) numArray, numArray.Length - x.Length, x.Length);
      return numArray;
    }

    public static byte[] OS2IP(byte[] x)
    {
      int srcOffset = 0;
      while ((x[srcOffset++] == 0) && (srcOffset < x.Length))
      {
      }
      srcOffset--;
      if (srcOffset > 0)
      {
        byte[] dst = new byte[x.Length - srcOffset];
        Buffer.BlockCopy(x, srcOffset, dst, 0, dst.Length);
        return dst;
      }
      return x;
    }

    public static byte[] RSAEP(RSA rsa, byte[] m)
    {
      return rsa.EncryptValue(m);
    }

    public static byte[] RSADP(RSA rsa, byte[] c)
    {
      return rsa.DecryptValue(c);
    }

    public static byte[] RSASP1(RSA rsa, byte[] m)
    {
      return rsa.DecryptValue(m);
    }

    public static byte[] RSAVP1(RSA rsa, byte[] s)
    {
      return rsa.EncryptValue(s);
    }

    public static byte[] Encrypt_OAEP(
      RSA rsa,
      HashAlgorithm hash,
      RandomNumberGenerator rng,
      byte[] M)
    {
      int size = rsa.KeySize / 8;
      int maskLen = hash.HashSize / 8;
      if (M.Length > size - 2 * maskLen - 2)
        throw new CryptographicException("message too long");
      byte[] emptyHash = PKCS1.GetEmptyHash(hash);
      int num = size - M.Length - 2 * maskLen - 2;
      byte[] array1 = new byte[emptyHash.Length + num + 1 + M.Length];
      Buffer.BlockCopy((Array) emptyHash, 0, (Array) array1, 0, emptyHash.Length);
      array1[emptyHash.Length + num] = (byte) 1;
      Buffer.BlockCopy((Array) M, 0, (Array) array1, array1.Length - M.Length, M.Length);
      byte[] numArray1 = new byte[maskLen];
      rng.GetBytes(numArray1);
      byte[] array2_1 = PKCS1.MGF1(hash, numArray1, size - maskLen - 1);
      byte[] mgfSeed = PKCS1.xor(array1, array2_1);
      byte[] array2_2 = PKCS1.MGF1(hash, mgfSeed, maskLen);
      byte[] numArray2 = PKCS1.xor(numArray1, array2_2);
      byte[] x = new byte[numArray2.Length + mgfSeed.Length + 1];
      Buffer.BlockCopy((Array) numArray2, 0, (Array) x, 1, numArray2.Length);
      Buffer.BlockCopy((Array) mgfSeed, 0, (Array) x, numArray2.Length + 1, mgfSeed.Length);
      byte[] m = PKCS1.OS2IP(x);
      return PKCS1.I2OSP(PKCS1.RSAEP(rsa, m), size);
    }

    public static byte[] Decrypt_OAEP(RSA rsa, HashAlgorithm hash, byte[] C)
    {
      int size = rsa.KeySize / 8;
      int maskLen = hash.HashSize / 8;
      if (size < 2 * maskLen + 2 || C.Length != size)
        throw new CryptographicException("decryption error");
      byte[] c = PKCS1.OS2IP(C);
      byte[] numArray1 = PKCS1.I2OSP(PKCS1.RSADP(rsa, c), size);
      byte[] array1 = new byte[maskLen];
      Buffer.BlockCopy((Array) numArray1, 1, (Array) array1, 0, array1.Length);
      byte[] numArray2 = new byte[size - maskLen - 1];
      Buffer.BlockCopy((Array) numArray1, numArray1.Length - numArray2.Length, (Array) numArray2, 0, numArray2.Length);
      byte[] array2_1 = PKCS1.MGF1(hash, numArray2, maskLen);
      byte[] mgfSeed = PKCS1.xor(array1, array2_1);
      byte[] array2_2 = PKCS1.MGF1(hash, mgfSeed, size - maskLen - 1);
      byte[] numArray3 = PKCS1.xor(numArray2, array2_2);
      byte[] emptyHash = PKCS1.GetEmptyHash(hash);
      byte[] array2_3 = new byte[emptyHash.Length];
      Buffer.BlockCopy((Array) numArray3, 0, (Array) array2_3, 0, array2_3.Length);
      bool flag = PKCS1.Compare(emptyHash, array2_3);
      int length = emptyHash.Length;
      while (numArray3[length] == (byte) 0)
        ++length;
      int count = numArray3.Length - length - 1;
      byte[] numArray4 = new byte[count];
      Buffer.BlockCopy((Array) numArray3, length + 1, (Array) numArray4, 0, count);
      return numArray1[0] != (byte) 0 || !flag || numArray3[length] != (byte) 1 ? (byte[]) null : numArray4;
    }

    public static byte[] Encrypt_v15(RSA rsa, RandomNumberGenerator rng, byte[] M)
    {
      int size = rsa.KeySize / 8;
      if (M.Length > size - 11)
        throw new CryptographicException("message too long");
      int count = System.Math.Max(8, size - M.Length - 3);
      byte[] data = new byte[count];
      rng.GetNonZeroBytes(data);
      byte[] x = new byte[size];
      x[1] = (byte) 2;
      Buffer.BlockCopy((Array) data, 0, (Array) x, 2, count);
      Buffer.BlockCopy((Array) M, 0, (Array) x, size - M.Length, M.Length);
      byte[] m = PKCS1.OS2IP(x);
      return PKCS1.I2OSP(PKCS1.RSAEP(rsa, m), size);
    }

    public static byte[] Decrypt_v15(RSA rsa, byte[] C)
    {
      int size = rsa.KeySize >> 3;
      if (size < 11 || C.Length > size)
        throw new CryptographicException("decryption error");
      byte[] c = PKCS1.OS2IP(C);
      byte[] numArray1 = PKCS1.I2OSP(PKCS1.RSADP(rsa, c), size);
      if (numArray1[0] != (byte) 0 || numArray1[1] != (byte) 2)
        return (byte[]) null;
      int index = 10;
      while (numArray1[index] != (byte) 0 && index < numArray1.Length)
        ++index;
      if (numArray1[index] != (byte) 0)
        return (byte[]) null;
      int srcOffset = index + 1;
      byte[] numArray2 = new byte[numArray1.Length - srcOffset];
      Buffer.BlockCopy((Array) numArray1, srcOffset, (Array) numArray2, 0, numArray2.Length);
      return numArray2;
    }

    public static byte[] Sign_v15(RSA rsa, HashAlgorithm hash, byte[] hashValue)
    {
      int num = rsa.KeySize >> 3;
      byte[] m = PKCS1.OS2IP(PKCS1.Encode_v15(hash, hashValue, num));
      return PKCS1.I2OSP(PKCS1.RSASP1(rsa, m), num);
    }

    public static bool Verify_v15(RSA rsa, HashAlgorithm hash, byte[] hashValue, byte[] signature)
    {
      return PKCS1.Verify_v15(rsa, hash, hashValue, signature, false);
    }

    public static bool Verify_v15(
      RSA rsa,
      HashAlgorithm hash,
      byte[] hashValue,
      byte[] signature,
      bool tryNonStandardEncoding)
    {
      int num = rsa.KeySize >> 3;
      byte[] s = PKCS1.OS2IP(signature);
      byte[] array2 = PKCS1.I2OSP(PKCS1.RSAVP1(rsa, s), num);
      bool flag = PKCS1.Compare(PKCS1.Encode_v15(hash, hashValue, num), array2);
      if (flag || !tryNonStandardEncoding)
        return flag;
      if (array2[0] != (byte) 0 || array2[1] != (byte) 1)
        return false;
      int index1;
      for (index1 = 2; index1 < array2.Length - hashValue.Length - 1; ++index1)
      {
        if (array2[index1] != byte.MaxValue)
          return false;
      }
      byte[] numArray = array2;
      int index2 = index1;
      int srcOffset = index2 + 1;
      if (numArray[index2] != (byte) 0)
        return false;
      byte[] array1 = new byte[hashValue.Length];
      Buffer.BlockCopy((Array) array2, srcOffset, (Array) array1, 0, array1.Length);
      return PKCS1.Compare(array1, hashValue);
    }

    public static byte[] Encode_v15(HashAlgorithm hash, byte[] hashValue, int emLength)
    {
      if (hashValue.Length != hash.HashSize >> 3)
        throw new CryptographicException("bad hash length for " + hash.ToString());
      string oid = CryptoConfig.MapNameToOID(hash.ToString());
      byte[] numArray1;
      if (oid != null)
      {
        ASN1 asn1_1 = new ASN1((byte) 48);
        asn1_1.Add(new ASN1(CryptoConfig.EncodeOID(oid)));
        asn1_1.Add(new ASN1((byte) 5));
        ASN1 asn1_2 = new ASN1((byte) 4, hashValue);
        ASN1 asN1 = new ASN1((byte) 48);
        asN1.Add(asn1_1);
        asN1.Add(asn1_2);
        numArray1 = asN1.GetBytes();
      }
      else
        numArray1 = hashValue;
      Buffer.BlockCopy((Array) hashValue, 0, (Array) numArray1, numArray1.Length - hashValue.Length, hashValue.Length);
      int num = System.Math.Max(8, emLength - numArray1.Length - 3);
      byte[] numArray2 = new byte[num + numArray1.Length + 3];
      numArray2[1] = (byte) 1;
      for (int index = 2; index < num + 2; ++index)
        numArray2[index] = byte.MaxValue;
      Buffer.BlockCopy((Array) numArray1, 0, (Array) numArray2, num + 3, numArray1.Length);
      return numArray2;
    }

    public static byte[] MGF1(HashAlgorithm hash, byte[] mgfSeed, int maskLen)
    {
      if (maskLen < 0)
        throw new OverflowException();
      int length = mgfSeed.Length;
      int count = hash.HashSize >> 3;
      int num = maskLen / count;
      if (maskLen % count != 0)
        ++num;
      byte[] numArray1 = new byte[num * count];
      byte[] buffer = new byte[length + 4];
      int dstOffset = 0;
      for (int x = 0; x < num; ++x)
      {
        byte[] numArray2 = PKCS1.I2OSP(x, 4);
        Buffer.BlockCopy((Array) mgfSeed, 0, (Array) buffer, 0, length);
        Buffer.BlockCopy((Array) numArray2, 0, (Array) buffer, length, 4);
        Buffer.BlockCopy((Array) hash.ComputeHash(buffer), 0, (Array) numArray1, dstOffset, count);
        dstOffset += length;
      }
      byte[] numArray3 = new byte[maskLen];
      Buffer.BlockCopy((Array) numArray1, 0, (Array) numArray3, 0, maskLen);
      return numArray3;
    }
  }
}
