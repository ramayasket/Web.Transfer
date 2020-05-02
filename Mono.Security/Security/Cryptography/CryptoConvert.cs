// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.CryptoConvert
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security.Cryptography
{
  public sealed class CryptoConvert
  {
    private CryptoConvert()
    {
    }

    private static int ToInt32LE(byte[] bytes, int offset)
    {
      return (int) bytes[offset + 3] << 24 | (int) bytes[offset + 2] << 16 | (int) bytes[offset + 1] << 8 | (int) bytes[offset];
    }

    private static uint ToUInt32LE(byte[] bytes, int offset)
    {
      return (uint) ((int) bytes[offset + 3] << 24 | (int) bytes[offset + 2] << 16 | (int) bytes[offset + 1] << 8) | (uint) bytes[offset];
    }

    private static byte[] GetBytesLE(int val)
    {
      return new byte[4]
      {
        (byte) (val & (int) byte.MaxValue),
        (byte) (val >> 8 & (int) byte.MaxValue),
        (byte) (val >> 16 & (int) byte.MaxValue),
        (byte) (val >> 24 & (int) byte.MaxValue)
      };
    }

    private static byte[] Trim(byte[] array)
    {
      for (int srcOffset = 0; srcOffset < array.Length; ++srcOffset)
      {
        if (array[srcOffset] != (byte) 0)
        {
          byte[] numArray = new byte[array.Length - srcOffset];
          Buffer.BlockCopy((Array) array, srcOffset, (Array) numArray, 0, numArray.Length);
          return numArray;
        }
      }
      return (byte[]) null;
    }

    public static RSA FromCapiPrivateKeyBlob(byte[] blob)
    {
      return CryptoConvert.FromCapiPrivateKeyBlob(blob, 0);
    }

    public static RSA FromCapiPrivateKeyBlob(byte[] blob, int offset)
    {
      if (blob == null)
        throw new ArgumentNullException(nameof (blob));
      if (offset >= blob.Length)
        throw new ArgumentException("blob is too small.");
      RSAParameters parameters = new RSAParameters();
      try
      {
        if (blob[offset] != (byte) 7 || blob[offset + 1] != (byte) 2 || (blob[offset + 2] != (byte) 0 || blob[offset + 3] != (byte) 0) || CryptoConvert.ToUInt32LE(blob, offset + 8) != 843141970U)
          throw new CryptographicException("Invalid blob header");
        int int32Le = CryptoConvert.ToInt32LE(blob, offset + 12);
        byte[] array = new byte[4];
        Buffer.BlockCopy((Array) blob, offset + 16, (Array) array, 0, 4);
        Array.Reverse((Array) array);
        parameters.Exponent = CryptoConvert.Trim(array);
        int srcOffset1 = offset + 20;
        int count1 = int32Le >> 3;
        parameters.Modulus = new byte[count1];
        Buffer.BlockCopy((Array) blob, srcOffset1, (Array) parameters.Modulus, 0, count1);
        Array.Reverse((Array) parameters.Modulus);
        int srcOffset2 = srcOffset1 + count1;
        int count2 = count1 >> 1;
        parameters.P = new byte[count2];
        Buffer.BlockCopy((Array) blob, srcOffset2, (Array) parameters.P, 0, count2);
        Array.Reverse((Array) parameters.P);
        int srcOffset3 = srcOffset2 + count2;
        parameters.Q = new byte[count2];
        Buffer.BlockCopy((Array) blob, srcOffset3, (Array) parameters.Q, 0, count2);
        Array.Reverse((Array) parameters.Q);
        int srcOffset4 = srcOffset3 + count2;
        parameters.DP = new byte[count2];
        Buffer.BlockCopy((Array) blob, srcOffset4, (Array) parameters.DP, 0, count2);
        Array.Reverse((Array) parameters.DP);
        int srcOffset5 = srcOffset4 + count2;
        parameters.DQ = new byte[count2];
        Buffer.BlockCopy((Array) blob, srcOffset5, (Array) parameters.DQ, 0, count2);
        Array.Reverse((Array) parameters.DQ);
        int srcOffset6 = srcOffset5 + count2;
        parameters.InverseQ = new byte[count2];
        Buffer.BlockCopy((Array) blob, srcOffset6, (Array) parameters.InverseQ, 0, count2);
        Array.Reverse((Array) parameters.InverseQ);
        int srcOffset7 = srcOffset6 + count2;
        parameters.D = new byte[count1];
        if (srcOffset7 + count1 + offset <= blob.Length)
        {
          Buffer.BlockCopy((Array) blob, srcOffset7, (Array) parameters.D, 0, count1);
          Array.Reverse((Array) parameters.D);
        }
      }
      catch (Exception ex)
      {
        throw new CryptographicException("Invalid blob.", ex);
      }
      RSA rsa;
      try
      {
        rsa = RSA.Create();
        rsa.ImportParameters(parameters);
      }
      catch (CryptographicException ex)
      {
        try
        {
          rsa = (RSA) new RSACryptoServiceProvider(new CspParameters()
          {
            Flags = CspProviderFlags.UseMachineKeyStore
          });
          rsa.ImportParameters(parameters);
        }
        catch
        {
          throw ex;
        }
      }
      return rsa;
    }

    public static DSA FromCapiPrivateKeyBlobDSA(byte[] blob)
    {
      return CryptoConvert.FromCapiPrivateKeyBlobDSA(blob, 0);
    }

    public static DSA FromCapiPrivateKeyBlobDSA(byte[] blob, int offset)
    {
      if (blob == null)
        throw new ArgumentNullException(nameof (blob));
      if (offset >= blob.Length)
        throw new ArgumentException("blob is too small.");
      DSAParameters parameters = new DSAParameters();
      try
      {
        if (blob[offset] != (byte) 7 || blob[offset + 1] != (byte) 2 || (blob[offset + 2] != (byte) 0 || blob[offset + 3] != (byte) 0) || CryptoConvert.ToUInt32LE(blob, offset + 8) != 844321604U)
          throw new CryptographicException("Invalid blob header");
        int count = CryptoConvert.ToInt32LE(blob, offset + 12) >> 3;
        int srcOffset1 = offset + 16;
        parameters.P = new byte[count];
        Buffer.BlockCopy((Array) blob, srcOffset1, (Array) parameters.P, 0, count);
        Array.Reverse((Array) parameters.P);
        int srcOffset2 = srcOffset1 + count;
        parameters.Q = new byte[20];
        Buffer.BlockCopy((Array) blob, srcOffset2, (Array) parameters.Q, 0, 20);
        Array.Reverse((Array) parameters.Q);
        int srcOffset3 = srcOffset2 + 20;
        parameters.G = new byte[count];
        Buffer.BlockCopy((Array) blob, srcOffset3, (Array) parameters.G, 0, count);
        Array.Reverse((Array) parameters.G);
        int srcOffset4 = srcOffset3 + count;
        parameters.X = new byte[20];
        Buffer.BlockCopy((Array) blob, srcOffset4, (Array) parameters.X, 0, 20);
        Array.Reverse((Array) parameters.X);
        int offset1 = srcOffset4 + 20;
        parameters.Counter = CryptoConvert.ToInt32LE(blob, offset1);
        int srcOffset5 = offset1 + 4;
        parameters.Seed = new byte[20];
        Buffer.BlockCopy((Array) blob, srcOffset5, (Array) parameters.Seed, 0, 20);
        Array.Reverse((Array) parameters.Seed);
        int num = srcOffset5 + 20;
      }
      catch (Exception ex)
      {
        throw new CryptographicException("Invalid blob.", ex);
      }
      DSA dsa;
      try
      {
        dsa = DSA.Create();
        dsa.ImportParameters(parameters);
      }
      catch (CryptographicException ex)
      {
        try
        {
          dsa = (DSA) new DSACryptoServiceProvider(new CspParameters()
          {
            Flags = CspProviderFlags.UseMachineKeyStore
          });
          dsa.ImportParameters(parameters);
        }
        catch
        {
          throw ex;
        }
      }
      return dsa;
    }

    public static byte[] ToCapiPrivateKeyBlob(RSA rsa)
    {
      RSAParameters rsaParameters = rsa.ExportParameters(true);
      int length1 = rsaParameters.Modulus.Length;
      byte[] numArray = new byte[20 + (length1 << 2) + (length1 >> 1)];
      numArray[0] = (byte) 7;
      numArray[1] = (byte) 2;
      numArray[5] = (byte) 36;
      numArray[8] = (byte) 82;
      numArray[9] = (byte) 83;
      numArray[10] = (byte) 65;
      numArray[11] = (byte) 50;
      byte[] bytesLe = CryptoConvert.GetBytesLE(length1 << 3);
      numArray[12] = bytesLe[0];
      numArray[13] = bytesLe[1];
      numArray[14] = bytesLe[2];
      numArray[15] = bytesLe[3];
      int num = 16;
      int length2 = rsaParameters.Exponent.Length;
      while (length2 > 0)
        numArray[num++] = rsaParameters.Exponent[--length2];
      int dstOffset1 = 20;
      byte[] modulus = rsaParameters.Modulus;
      int length3 = modulus.Length;
      Array.Reverse((Array) modulus, 0, length3);
      Buffer.BlockCopy((Array) modulus, 0, (Array) numArray, dstOffset1, length3);
      int dstOffset2 = dstOffset1 + length3;
      byte[] p = rsaParameters.P;
      int length4 = p.Length;
      Array.Reverse((Array) p, 0, length4);
      Buffer.BlockCopy((Array) p, 0, (Array) numArray, dstOffset2, length4);
      int dstOffset3 = dstOffset2 + length4;
      byte[] q = rsaParameters.Q;
      int length5 = q.Length;
      Array.Reverse((Array) q, 0, length5);
      Buffer.BlockCopy((Array) q, 0, (Array) numArray, dstOffset3, length5);
      int dstOffset4 = dstOffset3 + length5;
      byte[] dp = rsaParameters.DP;
      int length6 = dp.Length;
      Array.Reverse((Array) dp, 0, length6);
      Buffer.BlockCopy((Array) dp, 0, (Array) numArray, dstOffset4, length6);
      int dstOffset5 = dstOffset4 + length6;
      byte[] dq = rsaParameters.DQ;
      int length7 = dq.Length;
      Array.Reverse((Array) dq, 0, length7);
      Buffer.BlockCopy((Array) dq, 0, (Array) numArray, dstOffset5, length7);
      int dstOffset6 = dstOffset5 + length7;
      byte[] inverseQ = rsaParameters.InverseQ;
      int length8 = inverseQ.Length;
      Array.Reverse((Array) inverseQ, 0, length8);
      Buffer.BlockCopy((Array) inverseQ, 0, (Array) numArray, dstOffset6, length8);
      int dstOffset7 = dstOffset6 + length8;
      byte[] d = rsaParameters.D;
      int length9 = d.Length;
      Array.Reverse((Array) d, 0, length9);
      Buffer.BlockCopy((Array) d, 0, (Array) numArray, dstOffset7, length9);
      return numArray;
    }

    public static byte[] ToCapiPrivateKeyBlob(DSA dsa)
    {
      DSAParameters dsaParameters = dsa.ExportParameters(true);
      int length = dsaParameters.P.Length;
      byte[] numArray = new byte[16 + length + 20 + length + 20 + 4 + 20];
      numArray[0] = (byte) 7;
      numArray[1] = (byte) 2;
      numArray[5] = (byte) 34;
      numArray[8] = (byte) 68;
      numArray[9] = (byte) 83;
      numArray[10] = (byte) 83;
      numArray[11] = (byte) 50;
      byte[] bytesLe = CryptoConvert.GetBytesLE(length << 3);
      numArray[12] = bytesLe[0];
      numArray[13] = bytesLe[1];
      numArray[14] = bytesLe[2];
      numArray[15] = bytesLe[3];
      int dstOffset1 = 16;
      byte[] p = dsaParameters.P;
      Array.Reverse((Array) p);
      Buffer.BlockCopy((Array) p, 0, (Array) numArray, dstOffset1, length);
      int dstOffset2 = dstOffset1 + length;
      byte[] q = dsaParameters.Q;
      Array.Reverse((Array) q);
      Buffer.BlockCopy((Array) q, 0, (Array) numArray, dstOffset2, 20);
      int dstOffset3 = dstOffset2 + 20;
      byte[] g = dsaParameters.G;
      Array.Reverse((Array) g);
      Buffer.BlockCopy((Array) g, 0, (Array) numArray, dstOffset3, length);
      int dstOffset4 = dstOffset3 + length;
      byte[] x = dsaParameters.X;
      Array.Reverse((Array) x);
      Buffer.BlockCopy((Array) x, 0, (Array) numArray, dstOffset4, 20);
      int dstOffset5 = dstOffset4 + 20;
      Buffer.BlockCopy((Array) CryptoConvert.GetBytesLE(dsaParameters.Counter), 0, (Array) numArray, dstOffset5, 4);
      int dstOffset6 = dstOffset5 + 4;
      byte[] seed = dsaParameters.Seed;
      Array.Reverse((Array) seed);
      Buffer.BlockCopy((Array) seed, 0, (Array) numArray, dstOffset6, 20);
      return numArray;
    }

    public static RSA FromCapiPublicKeyBlob(byte[] blob)
    {
      return CryptoConvert.FromCapiPublicKeyBlob(blob, 0);
    }

    public static RSA FromCapiPublicKeyBlob(byte[] blob, int offset)
    {
      if (blob == null)
        throw new ArgumentNullException(nameof (blob));
      if (offset >= blob.Length)
        throw new ArgumentException("blob is too small.");
      try
      {
        if (blob[offset] != (byte) 6 || blob[offset + 1] != (byte) 2 || (blob[offset + 2] != (byte) 0 || blob[offset + 3] != (byte) 0) || CryptoConvert.ToUInt32LE(blob, offset + 8) != 826364754U)
          throw new CryptographicException("Invalid blob header");
        int int32Le = CryptoConvert.ToInt32LE(blob, offset + 12);
        RSAParameters parameters = new RSAParameters();
        parameters.Exponent = new byte[3];
        parameters.Exponent[0] = blob[offset + 18];
        parameters.Exponent[1] = blob[offset + 17];
        parameters.Exponent[2] = blob[offset + 16];
        int srcOffset = offset + 20;
        int count = int32Le >> 3;
        parameters.Modulus = new byte[count];
        Buffer.BlockCopy((Array) blob, srcOffset, (Array) parameters.Modulus, 0, count);
        Array.Reverse((Array) parameters.Modulus);
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
      catch (Exception ex)
      {
        throw new CryptographicException("Invalid blob.", ex);
      }
    }

    public static DSA FromCapiPublicKeyBlobDSA(byte[] blob)
    {
      return CryptoConvert.FromCapiPublicKeyBlobDSA(blob, 0);
    }

    public static DSA FromCapiPublicKeyBlobDSA(byte[] blob, int offset)
    {
      if (blob == null)
        throw new ArgumentNullException(nameof (blob));
      if (offset >= blob.Length)
        throw new ArgumentException("blob is too small.");
      try
      {
        if (blob[offset] != (byte) 6 || blob[offset + 1] != (byte) 2 || (blob[offset + 2] != (byte) 0 || blob[offset + 3] != (byte) 0) || CryptoConvert.ToUInt32LE(blob, offset + 8) != 827544388U)
          throw new CryptographicException("Invalid blob header");
        int int32Le = CryptoConvert.ToInt32LE(blob, offset + 12);
        DSAParameters parameters = new DSAParameters();
        int count = int32Le >> 3;
        int srcOffset1 = offset + 16;
        parameters.P = new byte[count];
        Buffer.BlockCopy((Array) blob, srcOffset1, (Array) parameters.P, 0, count);
        Array.Reverse((Array) parameters.P);
        int srcOffset2 = srcOffset1 + count;
        parameters.Q = new byte[20];
        Buffer.BlockCopy((Array) blob, srcOffset2, (Array) parameters.Q, 0, 20);
        Array.Reverse((Array) parameters.Q);
        int srcOffset3 = srcOffset2 + 20;
        parameters.G = new byte[count];
        Buffer.BlockCopy((Array) blob, srcOffset3, (Array) parameters.G, 0, count);
        Array.Reverse((Array) parameters.G);
        int srcOffset4 = srcOffset3 + count;
        parameters.Y = new byte[count];
        Buffer.BlockCopy((Array) blob, srcOffset4, (Array) parameters.Y, 0, count);
        Array.Reverse((Array) parameters.Y);
        int offset1 = srcOffset4 + count;
        parameters.Counter = CryptoConvert.ToInt32LE(blob, offset1);
        int srcOffset5 = offset1 + 4;
        parameters.Seed = new byte[20];
        Buffer.BlockCopy((Array) blob, srcOffset5, (Array) parameters.Seed, 0, 20);
        Array.Reverse((Array) parameters.Seed);
        int num = srcOffset5 + 20;
        DSA dsa = DSA.Create();
        dsa.ImportParameters(parameters);
        return dsa;
      }
      catch (Exception ex)
      {
        throw new CryptographicException("Invalid blob.", ex);
      }
    }

    public static byte[] ToCapiPublicKeyBlob(RSA rsa)
    {
      RSAParameters rsaParameters = rsa.ExportParameters(false);
      int length1 = rsaParameters.Modulus.Length;
      byte[] numArray = new byte[20 + length1];
      numArray[0] = (byte) 6;
      numArray[1] = (byte) 2;
      numArray[5] = (byte) 36;
      numArray[8] = (byte) 82;
      numArray[9] = (byte) 83;
      numArray[10] = (byte) 65;
      numArray[11] = (byte) 49;
      byte[] bytesLe = CryptoConvert.GetBytesLE(length1 << 3);
      numArray[12] = bytesLe[0];
      numArray[13] = bytesLe[1];
      numArray[14] = bytesLe[2];
      numArray[15] = bytesLe[3];
      int num1 = 16;
      int length2 = rsaParameters.Exponent.Length;
      while (length2 > 0)
        numArray[num1++] = rsaParameters.Exponent[--length2];
      int dstOffset = 20;
      byte[] modulus = rsaParameters.Modulus;
      int length3 = modulus.Length;
      Array.Reverse((Array) modulus, 0, length3);
      Buffer.BlockCopy((Array) modulus, 0, (Array) numArray, dstOffset, length3);
      int num2 = dstOffset + length3;
      return numArray;
    }

    public static byte[] ToCapiPublicKeyBlob(DSA dsa)
    {
      DSAParameters dsaParameters = dsa.ExportParameters(false);
      int length = dsaParameters.P.Length;
      byte[] numArray = new byte[16 + length + 20 + length + length + 4 + 20];
      numArray[0] = (byte) 6;
      numArray[1] = (byte) 2;
      numArray[5] = (byte) 34;
      numArray[8] = (byte) 68;
      numArray[9] = (byte) 83;
      numArray[10] = (byte) 83;
      numArray[11] = (byte) 49;
      byte[] bytesLe = CryptoConvert.GetBytesLE(length << 3);
      numArray[12] = bytesLe[0];
      numArray[13] = bytesLe[1];
      numArray[14] = bytesLe[2];
      numArray[15] = bytesLe[3];
      int dstOffset1 = 16;
      byte[] p = dsaParameters.P;
      Array.Reverse((Array) p);
      Buffer.BlockCopy((Array) p, 0, (Array) numArray, dstOffset1, length);
      int dstOffset2 = dstOffset1 + length;
      byte[] q = dsaParameters.Q;
      Array.Reverse((Array) q);
      Buffer.BlockCopy((Array) q, 0, (Array) numArray, dstOffset2, 20);
      int dstOffset3 = dstOffset2 + 20;
      byte[] g = dsaParameters.G;
      Array.Reverse((Array) g);
      Buffer.BlockCopy((Array) g, 0, (Array) numArray, dstOffset3, length);
      int dstOffset4 = dstOffset3 + length;
      byte[] y = dsaParameters.Y;
      Array.Reverse((Array) y);
      Buffer.BlockCopy((Array) y, 0, (Array) numArray, dstOffset4, length);
      int dstOffset5 = dstOffset4 + length;
      Buffer.BlockCopy((Array) CryptoConvert.GetBytesLE(dsaParameters.Counter), 0, (Array) numArray, dstOffset5, 4);
      int dstOffset6 = dstOffset5 + 4;
      byte[] seed = dsaParameters.Seed;
      Array.Reverse((Array) seed);
      Buffer.BlockCopy((Array) seed, 0, (Array) numArray, dstOffset6, 20);
      return numArray;
    }

    public static RSA FromCapiKeyBlob(byte[] blob)
    {
      return CryptoConvert.FromCapiKeyBlob(blob, 0);
    }

    public static RSA FromCapiKeyBlob(byte[] blob, int offset)
    {
      if (blob == null)
        throw new ArgumentNullException(nameof (blob));
      if (offset >= blob.Length)
        throw new ArgumentException("blob is too small.");
      switch (blob[offset])
      {
        case 0:
          if (blob[offset + 12] == (byte) 6)
            return CryptoConvert.FromCapiPublicKeyBlob(blob, offset + 12);
          break;
        case 6:
          return CryptoConvert.FromCapiPublicKeyBlob(blob, offset);
        case 7:
          return CryptoConvert.FromCapiPrivateKeyBlob(blob, offset);
      }
      throw new CryptographicException("Unknown blob format.");
    }

    public static DSA FromCapiKeyBlobDSA(byte[] blob)
    {
      return CryptoConvert.FromCapiKeyBlobDSA(blob, 0);
    }

    public static DSA FromCapiKeyBlobDSA(byte[] blob, int offset)
    {
      if (blob == null)
        throw new ArgumentNullException(nameof (blob));
      if (offset >= blob.Length)
        throw new ArgumentException("blob is too small.");
      switch (blob[offset])
      {
        case 6:
          return CryptoConvert.FromCapiPublicKeyBlobDSA(blob, offset);
        case 7:
          return CryptoConvert.FromCapiPrivateKeyBlobDSA(blob, offset);
        default:
          throw new CryptographicException("Unknown blob format.");
      }
    }

    public static byte[] ToCapiKeyBlob(AsymmetricAlgorithm keypair, bool includePrivateKey)
    {
      switch (keypair)
      {
        case null:
          throw new ArgumentNullException(nameof (keypair));
        case RSA _:
          return CryptoConvert.ToCapiKeyBlob((RSA) keypair, includePrivateKey);
        case DSA _:
          return CryptoConvert.ToCapiKeyBlob((DSA) keypair, includePrivateKey);
        default:
          return (byte[]) null;
      }
    }

    public static byte[] ToCapiKeyBlob(RSA rsa, bool includePrivateKey)
    {
      if (rsa == null)
        throw new ArgumentNullException(nameof (rsa));
      return includePrivateKey ? CryptoConvert.ToCapiPrivateKeyBlob(rsa) : CryptoConvert.ToCapiPublicKeyBlob(rsa);
    }

    public static byte[] ToCapiKeyBlob(DSA dsa, bool includePrivateKey)
    {
      if (dsa == null)
        throw new ArgumentNullException(nameof (dsa));
      return includePrivateKey ? CryptoConvert.ToCapiPrivateKeyBlob(dsa) : CryptoConvert.ToCapiPublicKeyBlob(dsa);
    }

    public static string ToHex(byte[] input)
    {
      if (input == null)
        return (string) null;
      StringBuilder stringBuilder = new StringBuilder(input.Length * 2);
      foreach (byte num in input)
        stringBuilder.Append(num.ToString("X2", (IFormatProvider) CultureInfo.InvariantCulture));
      return stringBuilder.ToString();
    }

    private static byte FromHexChar(char c)
    {
      if (c >= 'a' && c <= 'f')
        return (byte) ((int) c - 97 + 10);
      if (c >= 'A' && c <= 'F')
        return (byte) ((int) c - 65 + 10);
      if (c >= '0' && c <= '9')
        return (byte) ((uint) c - 48U);
      throw new ArgumentException("invalid hex char");
    }

    public static byte[] FromHex(string hex)
    {
      if (hex == null)
        return (byte[]) null;
      if ((hex.Length & 1) == 1)
        throw new ArgumentException("Length must be a multiple of 2");
      byte[] numArray1 = new byte[hex.Length >> 1];
      int num1 = 0;
      int num2 = 0;
      while (num1 < numArray1.Length)
      {
        byte[] numArray2 = numArray1;
        int index1 = num1;
        string str1 = hex;
        int index2 = num2;
        int num3 = index2 + 1;
        int num4 = (int) (byte) ((uint) CryptoConvert.FromHexChar(str1[index2]) << 4);
        numArray2[index1] = (byte) num4;
        ref byte local = ref numArray1[num1++];
        int num5 = (int) local;
        string str2 = hex;
        int index3 = num3;
        num2 = index3 + 1;
        int num6 = (int) CryptoConvert.FromHexChar(str2[index3]);
        local = (byte) (num5 + num6);
      }
      return numArray1;
    }
  }
}
