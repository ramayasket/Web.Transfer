// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.DiffieHellmanManaged
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Math;
using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography
{
  public sealed class DiffieHellmanManaged : DiffieHellman
  {
    private static byte[] m_OAKLEY768 = new byte[96]
    {
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      (byte) 201,
      (byte) 15,
      (byte) 218,
      (byte) 162,
      (byte) 33,
      (byte) 104,
      (byte) 194,
      (byte) 52,
      (byte) 196,
      (byte) 198,
      (byte) 98,
      (byte) 139,
      (byte) 128,
      (byte) 220,
      (byte) 28,
      (byte) 209,
      (byte) 41,
      (byte) 2,
      (byte) 78,
      (byte) 8,
      (byte) 138,
      (byte) 103,
      (byte) 204,
      (byte) 116,
      (byte) 2,
      (byte) 11,
      (byte) 190,
      (byte) 166,
      (byte) 59,
      (byte) 19,
      (byte) 155,
      (byte) 34,
      (byte) 81,
      (byte) 74,
      (byte) 8,
      (byte) 121,
      (byte) 142,
      (byte) 52,
      (byte) 4,
      (byte) 221,
      (byte) 239,
      (byte) 149,
      (byte) 25,
      (byte) 179,
      (byte) 205,
      (byte) 58,
      (byte) 67,
      (byte) 27,
      (byte) 48,
      (byte) 43,
      (byte) 10,
      (byte) 109,
      (byte) 242,
      (byte) 95,
      (byte) 20,
      (byte) 55,
      (byte) 79,
      (byte) 225,
      (byte) 53,
      (byte) 109,
      (byte) 109,
      (byte) 81,
      (byte) 194,
      (byte) 69,
      (byte) 228,
      (byte) 133,
      (byte) 181,
      (byte) 118,
      (byte) 98,
      (byte) 94,
      (byte) 126,
      (byte) 198,
      (byte) 244,
      (byte) 76,
      (byte) 66,
      (byte) 233,
      (byte) 166,
      (byte) 58,
      (byte) 54,
      (byte) 32,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue
    };
    private static byte[] m_OAKLEY1024 = new byte[128]
    {
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      (byte) 201,
      (byte) 15,
      (byte) 218,
      (byte) 162,
      (byte) 33,
      (byte) 104,
      (byte) 194,
      (byte) 52,
      (byte) 196,
      (byte) 198,
      (byte) 98,
      (byte) 139,
      (byte) 128,
      (byte) 220,
      (byte) 28,
      (byte) 209,
      (byte) 41,
      (byte) 2,
      (byte) 78,
      (byte) 8,
      (byte) 138,
      (byte) 103,
      (byte) 204,
      (byte) 116,
      (byte) 2,
      (byte) 11,
      (byte) 190,
      (byte) 166,
      (byte) 59,
      (byte) 19,
      (byte) 155,
      (byte) 34,
      (byte) 81,
      (byte) 74,
      (byte) 8,
      (byte) 121,
      (byte) 142,
      (byte) 52,
      (byte) 4,
      (byte) 221,
      (byte) 239,
      (byte) 149,
      (byte) 25,
      (byte) 179,
      (byte) 205,
      (byte) 58,
      (byte) 67,
      (byte) 27,
      (byte) 48,
      (byte) 43,
      (byte) 10,
      (byte) 109,
      (byte) 242,
      (byte) 95,
      (byte) 20,
      (byte) 55,
      (byte) 79,
      (byte) 225,
      (byte) 53,
      (byte) 109,
      (byte) 109,
      (byte) 81,
      (byte) 194,
      (byte) 69,
      (byte) 228,
      (byte) 133,
      (byte) 181,
      (byte) 118,
      (byte) 98,
      (byte) 94,
      (byte) 126,
      (byte) 198,
      (byte) 244,
      (byte) 76,
      (byte) 66,
      (byte) 233,
      (byte) 166,
      (byte) 55,
      (byte) 237,
      (byte) 107,
      (byte) 11,
      byte.MaxValue,
      (byte) 92,
      (byte) 182,
      (byte) 244,
      (byte) 6,
      (byte) 183,
      (byte) 237,
      (byte) 238,
      (byte) 56,
      (byte) 107,
      (byte) 251,
      (byte) 90,
      (byte) 137,
      (byte) 159,
      (byte) 165,
      (byte) 174,
      (byte) 159,
      (byte) 36,
      (byte) 17,
      (byte) 124,
      (byte) 75,
      (byte) 31,
      (byte) 230,
      (byte) 73,
      (byte) 40,
      (byte) 102,
      (byte) 81,
      (byte) 236,
      (byte) 230,
      (byte) 83,
      (byte) 129,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue
    };
    private static byte[] m_OAKLEY1536 = new byte[192]
    {
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      (byte) 201,
      (byte) 15,
      (byte) 218,
      (byte) 162,
      (byte) 33,
      (byte) 104,
      (byte) 194,
      (byte) 52,
      (byte) 196,
      (byte) 198,
      (byte) 98,
      (byte) 139,
      (byte) 128,
      (byte) 220,
      (byte) 28,
      (byte) 209,
      (byte) 41,
      (byte) 2,
      (byte) 78,
      (byte) 8,
      (byte) 138,
      (byte) 103,
      (byte) 204,
      (byte) 116,
      (byte) 2,
      (byte) 11,
      (byte) 190,
      (byte) 166,
      (byte) 59,
      (byte) 19,
      (byte) 155,
      (byte) 34,
      (byte) 81,
      (byte) 74,
      (byte) 8,
      (byte) 121,
      (byte) 142,
      (byte) 52,
      (byte) 4,
      (byte) 221,
      (byte) 239,
      (byte) 149,
      (byte) 25,
      (byte) 179,
      (byte) 205,
      (byte) 58,
      (byte) 67,
      (byte) 27,
      (byte) 48,
      (byte) 43,
      (byte) 10,
      (byte) 109,
      (byte) 242,
      (byte) 95,
      (byte) 20,
      (byte) 55,
      (byte) 79,
      (byte) 225,
      (byte) 53,
      (byte) 109,
      (byte) 109,
      (byte) 81,
      (byte) 194,
      (byte) 69,
      (byte) 228,
      (byte) 133,
      (byte) 181,
      (byte) 118,
      (byte) 98,
      (byte) 94,
      (byte) 126,
      (byte) 198,
      (byte) 244,
      (byte) 76,
      (byte) 66,
      (byte) 233,
      (byte) 166,
      (byte) 55,
      (byte) 237,
      (byte) 107,
      (byte) 11,
      byte.MaxValue,
      (byte) 92,
      (byte) 182,
      (byte) 244,
      (byte) 6,
      (byte) 183,
      (byte) 237,
      (byte) 238,
      (byte) 56,
      (byte) 107,
      (byte) 251,
      (byte) 90,
      (byte) 137,
      (byte) 159,
      (byte) 165,
      (byte) 174,
      (byte) 159,
      (byte) 36,
      (byte) 17,
      (byte) 124,
      (byte) 75,
      (byte) 31,
      (byte) 230,
      (byte) 73,
      (byte) 40,
      (byte) 102,
      (byte) 81,
      (byte) 236,
      (byte) 228,
      (byte) 91,
      (byte) 61,
      (byte) 194,
      (byte) 0,
      (byte) 124,
      (byte) 184,
      (byte) 161,
      (byte) 99,
      (byte) 191,
      (byte) 5,
      (byte) 152,
      (byte) 218,
      (byte) 72,
      (byte) 54,
      (byte) 28,
      (byte) 85,
      (byte) 211,
      (byte) 154,
      (byte) 105,
      (byte) 22,
      (byte) 63,
      (byte) 168,
      (byte) 253,
      (byte) 36,
      (byte) 207,
      (byte) 95,
      (byte) 131,
      (byte) 101,
      (byte) 93,
      (byte) 35,
      (byte) 220,
      (byte) 163,
      (byte) 173,
      (byte) 150,
      (byte) 28,
      (byte) 98,
      (byte) 243,
      (byte) 86,
      (byte) 32,
      (byte) 133,
      (byte) 82,
      (byte) 187,
      (byte) 158,
      (byte) 213,
      (byte) 41,
      (byte) 7,
      (byte) 112,
      (byte) 150,
      (byte) 150,
      (byte) 109,
      (byte) 103,
      (byte) 12,
      (byte) 53,
      (byte) 78,
      (byte) 74,
      (byte) 188,
      (byte) 152,
      (byte) 4,
      (byte) 241,
      (byte) 116,
      (byte) 108,
      (byte) 8,
      (byte) 202,
      (byte) 35,
      (byte) 115,
      (byte) 39,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue,
      byte.MaxValue
    };
    private BigInteger m_P;
    private BigInteger m_G;
    private BigInteger m_X;
    private bool m_Disposed;

    public DiffieHellmanManaged()
      : this(1024, 160, DHKeyGeneration.Static)
    {
    }

    public DiffieHellmanManaged(int bitLength, int l, DHKeyGeneration method)
    {
      if (bitLength < 256 || l < 0)
        throw new ArgumentException();
      BigInteger p;
      BigInteger g;
      this.GenerateKey(bitLength, method, out p, out g);
      this.Initialize(p, g, (BigInteger) null, l, false);
    }

    public DiffieHellmanManaged(byte[] p, byte[] g, byte[] x)
    {
      if (p == null || g == null)
        throw new ArgumentNullException();
      if (x == null)
        this.Initialize(new BigInteger(p), new BigInteger(g), (BigInteger) null, 0, true);
      else
        this.Initialize(new BigInteger(p), new BigInteger(g), new BigInteger(x), 0, true);
    }

    public DiffieHellmanManaged(byte[] p, byte[] g, int l)
    {
      if (p == null || g == null)
        throw new ArgumentNullException();
      if (l < 0)
        throw new ArgumentException();
      this.Initialize(new BigInteger(p), new BigInteger(g), (BigInteger) null, l, true);
    }

    private void Initialize(
      BigInteger p,
      BigInteger g,
      BigInteger x,
      int secretLen,
      bool checkInput)
    {
      if (checkInput && (!p.IsProbablePrime() || g <= (BigInteger) 0 || g >= p || x != (BigInteger) null && (x <= (BigInteger) 0 || x > p - (BigInteger) 2)))
        throw new CryptographicException();
      if (secretLen == 0)
        secretLen = p.BitCount();
      this.m_P = p;
      this.m_G = g;
      if (x == (BigInteger) null)
      {
        BigInteger bigInteger = this.m_P - (BigInteger) 1;
        this.m_X = BigInteger.GenerateRandom(secretLen);
        while (this.m_X >= bigInteger || this.m_X == 0U)
          this.m_X = BigInteger.GenerateRandom(secretLen);
      }
      else
        this.m_X = x;
    }

    public override byte[] CreateKeyExchange()
    {
      BigInteger bigInteger = this.m_G.ModPow(this.m_X, this.m_P);
      byte[] bytes = bigInteger.GetBytes();
      bigInteger.Clear();
      return bytes;
    }

    public override byte[] DecryptKeyExchange(byte[] keyEx)
    {
      BigInteger bigInteger = new BigInteger(keyEx).ModPow(this.m_X, this.m_P);
      byte[] bytes = bigInteger.GetBytes();
      bigInteger.Clear();
      return bytes;
    }

    public override string KeyExchangeAlgorithm
    {
      get
      {
        return "1.2.840.113549.1.3";
      }
    }

    public override string SignatureAlgorithm
    {
      get
      {
        return (string) null;
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (!this.m_Disposed)
      {
        this.m_P.Clear();
        this.m_G.Clear();
        this.m_X.Clear();
      }
      this.m_Disposed = true;
    }

    public override DHParameters ExportParameters(bool includePrivateParameters)
    {
      DHParameters dhParameters = new DHParameters();
      dhParameters.P = this.m_P.GetBytes();
      dhParameters.G = this.m_G.GetBytes();
      if (includePrivateParameters)
        dhParameters.X = this.m_X.GetBytes();
      return dhParameters;
    }

    public override void ImportParameters(DHParameters parameters)
    {
      if (parameters.P == null)
        throw new CryptographicException("Missing P value.");
      if (parameters.G == null)
        throw new CryptographicException("Missing G value.");
      BigInteger p = new BigInteger(parameters.P);
      BigInteger g = new BigInteger(parameters.G);
      BigInteger x = (BigInteger) null;
      if (parameters.X != null)
        x = new BigInteger(parameters.X);
      this.Initialize(p, g, x, 0, true);
    }

    ~DiffieHellmanManaged()
    {
      this.Dispose(false);
    }

    private void GenerateKey(
      int bitlen,
      DHKeyGeneration keygen,
      out BigInteger p,
      out BigInteger g)
    {
      if (keygen == DHKeyGeneration.Static)
      {
        switch (bitlen)
        {
          case 768:
            p = new BigInteger(DiffieHellmanManaged.m_OAKLEY768);
            break;
          case 1024:
            p = new BigInteger(DiffieHellmanManaged.m_OAKLEY1024);
            break;
          case 1536:
            p = new BigInteger(DiffieHellmanManaged.m_OAKLEY1536);
            break;
          default:
            throw new ArgumentException("Invalid bit size.");
        }
        g = new BigInteger(22U);
      }
      else
      {
        p = BigInteger.GeneratePseudoPrime(bitlen);
        g = new BigInteger(3U);
      }
    }
  }
}
