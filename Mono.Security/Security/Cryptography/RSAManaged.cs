// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.RSAManaged
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Math;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Mono.Security.Cryptography
{
  public class RSAManaged : RSA
  {
    private bool keyBlinding = true;
    //private const int defaultKeySize = 1024;
    private bool isCRTpossible;
    private bool keypairGenerated;
    private bool m_disposed;
    private BigInteger d;
    private BigInteger p;
    private BigInteger q;
    private BigInteger dp;
    private BigInteger dq;
    private BigInteger qInv;
    private BigInteger n;
    private BigInteger e;
    private RSAManaged.KeyGeneratedEventHandler _KeyGenerated;

    public RSAManaged()
      : this(1024)
    {
    }

    public RSAManaged(int keySize)
    {
      this.LegalKeySizesValue = new KeySizes[1];
      this.LegalKeySizesValue[0] = new KeySizes(384, 16384, 8);
      this.KeySize = keySize;
    }

    public event RSAManaged.KeyGeneratedEventHandler KeyGenerated
    {
      add
      {
        RSAManaged.KeyGeneratedEventHandler comparand = this._KeyGenerated;
        RSAManaged.KeyGeneratedEventHandler generatedEventHandler;
        do
        {
          generatedEventHandler = comparand;
          comparand = Interlocked.CompareExchange<RSAManaged.KeyGeneratedEventHandler>(ref this._KeyGenerated, generatedEventHandler + value, comparand);
        }
        while (comparand != generatedEventHandler);
      }
      remove
      {
        RSAManaged.KeyGeneratedEventHandler comparand = this._KeyGenerated;
        RSAManaged.KeyGeneratedEventHandler generatedEventHandler;
        do
        {
          generatedEventHandler = comparand;
          comparand = Interlocked.CompareExchange<RSAManaged.KeyGeneratedEventHandler>(ref this._KeyGenerated, generatedEventHandler - value, comparand);
        }
        while (comparand != generatedEventHandler);
      }
    }

    ~RSAManaged()
    {
      this.Dispose(false);
    }

    private void GenerateKeyPair()
    {
      int bits1 = this.KeySize + 1 >> 1;
      int bits2 = this.KeySize - bits1;
      this.e = (BigInteger) 17U;
      do
      {
        this.p = BigInteger.GeneratePseudoPrime(bits1);
      }
      while (this.p % 17U == 1U);
      while (true)
      {
        do
        {
          do
          {
            this.q = BigInteger.GeneratePseudoPrime(bits2);
          }
          while (this.q % 17U == 1U || !(this.p != this.q));
          this.n = this.p * this.q;
          if (this.n.BitCount() == this.KeySize)
            goto label_7;
        }
        while (!(this.p < this.q));
        this.p = this.q;
      }
label_7:
      BigInteger bigInteger1 = this.p - (BigInteger) 1;
      BigInteger bigInteger2 = this.q - (BigInteger) 1;
      this.d = this.e.ModInverse(bigInteger1 * bigInteger2);
      this.dp = this.d % bigInteger1;
      this.dq = this.d % bigInteger2;
      this.qInv = this.q.ModInverse(this.p);
      this.keypairGenerated = true;
      this.isCRTpossible = true;
      if (this._KeyGenerated == null)
        return;
      this._KeyGenerated((object) this, (EventArgs) null);
    }

    public override int KeySize
    {
      get
      {
        if (!this.keypairGenerated)
          return base.KeySize;
        int num = this.n.BitCount();
        if ((num & 7) != 0)
          num += 8 - (num & 7);
        return num;
      }
    }

    public override string KeyExchangeAlgorithm
    {
      get
      {
        return "RSA-PKCS1-KeyEx";
      }
    }

    public bool PublicOnly
    {
      get
      {
        if (!this.keypairGenerated)
          return false;
        return this.d == (BigInteger) null || this.n == (BigInteger) null;
      }
    }

    public override string SignatureAlgorithm
    {
      get
      {
        return "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
      }
    }

    public override byte[] DecryptValue(byte[] rgb)
    {
      if (this.m_disposed)
        throw new ObjectDisposedException("private key");
      if (!this.keypairGenerated)
        this.GenerateKeyPair();
      BigInteger bigInteger1 = new BigInteger(rgb);
      BigInteger bigInteger2 = (BigInteger) null;
      if (this.keyBlinding)
      {
        bigInteger2 = BigInteger.GenerateRandom(this.n.BitCount());
        bigInteger1 = bigInteger2.ModPow(this.e, this.n) * bigInteger1 % this.n;
      }
      BigInteger bigInteger3;
      if (this.isCRTpossible)
      {
        BigInteger bigInteger4 = bigInteger1.ModPow(this.dp, this.p);
        BigInteger bigInteger5 = bigInteger1.ModPow(this.dq, this.q);
        if (bigInteger5 > bigInteger4)
        {
          BigInteger bigInteger6 = this.p - (bigInteger5 - bigInteger4) * this.qInv % this.p;
          bigInteger3 = bigInteger5 + this.q * bigInteger6;
        }
        else
        {
          BigInteger bigInteger6 = (bigInteger4 - bigInteger5) * this.qInv % this.p;
          bigInteger3 = bigInteger5 + this.q * bigInteger6;
        }
      }
      else
      {
        if (this.PublicOnly)
          throw new CryptographicException(Locale.GetText("Missing private key to decrypt value."));
        bigInteger3 = bigInteger1.ModPow(this.d, this.n);
      }
      if (this.keyBlinding)
      {
        bigInteger3 = bigInteger3 * bigInteger2.ModInverse(this.n) % this.n;
        bigInteger2.Clear();
      }
      byte[] paddedValue = this.GetPaddedValue(bigInteger3, this.KeySize >> 3);
      bigInteger1.Clear();
      bigInteger3.Clear();
      return paddedValue;
    }

    public override byte[] EncryptValue(byte[] rgb)
    {
      if (this.m_disposed)
        throw new ObjectDisposedException("public key");
      if (!this.keypairGenerated)
        this.GenerateKeyPair();
      BigInteger bigInteger1 = new BigInteger(rgb);
      BigInteger bigInteger2 = bigInteger1.ModPow(this.e, this.n);
      byte[] paddedValue = this.GetPaddedValue(bigInteger2, this.KeySize >> 3);
      bigInteger1.Clear();
      bigInteger2.Clear();
      return paddedValue;
    }

    public override RSAParameters ExportParameters(bool includePrivateParameters)
    {
      if (this.m_disposed)
        throw new ObjectDisposedException(Locale.GetText("Keypair was disposed"));
      if (!this.keypairGenerated)
        this.GenerateKeyPair();
      RSAParameters rsaParameters = new RSAParameters { Exponent = this.e.GetBytes(), Modulus = this.n.GetBytes() };
      if (includePrivateParameters)
      {
        if (this.d == (BigInteger) null)
          throw new CryptographicException("Missing private key");
        rsaParameters.D = this.d.GetBytes();
        if (rsaParameters.D.Length != rsaParameters.Modulus.Length)
        {
          byte[] numArray = new byte[rsaParameters.Modulus.Length];
          Buffer.BlockCopy((Array) rsaParameters.D, 0, (Array) numArray, numArray.Length - rsaParameters.D.Length, rsaParameters.D.Length);
          rsaParameters.D = numArray;
        }
        if (this.p != (BigInteger) null && this.q != (BigInteger) null && (this.dp != (BigInteger) null && this.dq != (BigInteger) null) && this.qInv != (BigInteger) null)
        {
          int length = this.KeySize >> 4;
          rsaParameters.P = this.GetPaddedValue(this.p, length);
          rsaParameters.Q = this.GetPaddedValue(this.q, length);
          rsaParameters.DP = this.GetPaddedValue(this.dp, length);
          rsaParameters.DQ = this.GetPaddedValue(this.dq, length);
          rsaParameters.InverseQ = this.GetPaddedValue(this.qInv, length);
        }
      }
      return rsaParameters;
    }

    public override void ImportParameters(RSAParameters parameters)
    {
      if (this.m_disposed)
        throw new ObjectDisposedException(Locale.GetText("Keypair was disposed"));
      if (parameters.Exponent == null)
        throw new CryptographicException(Locale.GetText("Missing Exponent"));
      if (parameters.Modulus == null)
        throw new CryptographicException(Locale.GetText("Missing Modulus"));
      this.e = new BigInteger(parameters.Exponent);
      this.n = new BigInteger(parameters.Modulus);
      if (parameters.D != null)
        this.d = new BigInteger(parameters.D);
      if (parameters.DP != null)
        this.dp = new BigInteger(parameters.DP);
      if (parameters.DQ != null)
        this.dq = new BigInteger(parameters.DQ);
      if (parameters.InverseQ != null)
        this.qInv = new BigInteger(parameters.InverseQ);
      if (parameters.P != null)
        this.p = new BigInteger(parameters.P);
      if (parameters.Q != null)
        this.q = new BigInteger(parameters.Q);
      this.keypairGenerated = true;
      bool flag1 = this.p != (BigInteger) null && this.q != (BigInteger) null && this.dp != (BigInteger) null;
      this.isCRTpossible = flag1 && this.dq != (BigInteger) null && this.qInv != (BigInteger) null;
      if (!flag1)
        return;
      bool flag2 = this.n == this.p * this.q;
      if (flag2)
      {
        BigInteger bigInteger1 = this.p - (BigInteger) 1;
        BigInteger bigInteger2 = this.q - (BigInteger) 1;
        BigInteger bigInteger3 = this.e.ModInverse(bigInteger1 * bigInteger2);
        flag2 = this.d == bigInteger3;
        if (!flag2 && this.isCRTpossible)
          flag2 = this.dp == bigInteger3 % bigInteger1 && this.dq == bigInteger3 % bigInteger2 && this.qInv == this.q.ModInverse(this.p);
      }
      if (!flag2)
        throw new CryptographicException(Locale.GetText("Private/public key mismatch"));
    }

    protected override void Dispose(bool disposing)
    {
      if (!this.m_disposed)
      {
        if (this.d != (BigInteger) null)
        {
          this.d.Clear();
          this.d = (BigInteger) null;
        }
        if (this.p != (BigInteger) null)
        {
          this.p.Clear();
          this.p = (BigInteger) null;
        }
        if (this.q != (BigInteger) null)
        {
          this.q.Clear();
          this.q = (BigInteger) null;
        }
        if (this.dp != (BigInteger) null)
        {
          this.dp.Clear();
          this.dp = (BigInteger) null;
        }
        if (this.dq != (BigInteger) null)
        {
          this.dq.Clear();
          this.dq = (BigInteger) null;
        }
        if (this.qInv != (BigInteger) null)
        {
          this.qInv.Clear();
          this.qInv = (BigInteger) null;
        }
        if (disposing)
        {
          if (this.e != (BigInteger) null)
          {
            this.e.Clear();
            this.e = (BigInteger) null;
          }
          if (this.n != (BigInteger) null)
          {
            this.n.Clear();
            this.n = (BigInteger) null;
          }
        }
      }
      this.m_disposed = true;
    }

    public override string ToXmlString(bool includePrivateParameters)
    {
      StringBuilder stringBuilder = new StringBuilder();
      RSAParameters rsaParameters = this.ExportParameters(includePrivateParameters);
      try
      {
        stringBuilder.Append("<RSAKeyValue>");
        stringBuilder.Append("<Modulus>");
        stringBuilder.Append(Convert.ToBase64String(rsaParameters.Modulus));
        stringBuilder.Append("</Modulus>");
        stringBuilder.Append("<Exponent>");
        stringBuilder.Append(Convert.ToBase64String(rsaParameters.Exponent));
        stringBuilder.Append("</Exponent>");
        if (includePrivateParameters)
        {
          if (rsaParameters.P != null)
          {
            stringBuilder.Append("<P>");
            stringBuilder.Append(Convert.ToBase64String(rsaParameters.P));
            stringBuilder.Append("</P>");
          }
          if (rsaParameters.Q != null)
          {
            stringBuilder.Append("<Q>");
            stringBuilder.Append(Convert.ToBase64String(rsaParameters.Q));
            stringBuilder.Append("</Q>");
          }
          if (rsaParameters.DP != null)
          {
            stringBuilder.Append("<DP>");
            stringBuilder.Append(Convert.ToBase64String(rsaParameters.DP));
            stringBuilder.Append("</DP>");
          }
          if (rsaParameters.DQ != null)
          {
            stringBuilder.Append("<DQ>");
            stringBuilder.Append(Convert.ToBase64String(rsaParameters.DQ));
            stringBuilder.Append("</DQ>");
          }
          if (rsaParameters.InverseQ != null)
          {
            stringBuilder.Append("<InverseQ>");
            stringBuilder.Append(Convert.ToBase64String(rsaParameters.InverseQ));
            stringBuilder.Append("</InverseQ>");
          }
          stringBuilder.Append("<D>");
          stringBuilder.Append(Convert.ToBase64String(rsaParameters.D));
          stringBuilder.Append("</D>");
        }
        stringBuilder.Append("</RSAKeyValue>");
      }
      catch
      {
        if (rsaParameters.P != null)
          Array.Clear((Array) rsaParameters.P, 0, rsaParameters.P.Length);
        if (rsaParameters.Q != null)
          Array.Clear((Array) rsaParameters.Q, 0, rsaParameters.Q.Length);
        if (rsaParameters.DP != null)
          Array.Clear((Array) rsaParameters.DP, 0, rsaParameters.DP.Length);
        if (rsaParameters.DQ != null)
          Array.Clear((Array) rsaParameters.DQ, 0, rsaParameters.DQ.Length);
        if (rsaParameters.InverseQ != null)
          Array.Clear((Array) rsaParameters.InverseQ, 0, rsaParameters.InverseQ.Length);
        if (rsaParameters.D != null)
          Array.Clear((Array) rsaParameters.D, 0, rsaParameters.D.Length);
        throw;
      }
      return stringBuilder.ToString();
    }

    public bool UseKeyBlinding
    {
      get
      {
        return this.keyBlinding;
      }
      set
      {
        this.keyBlinding = value;
      }
    }

    public bool IsCrtPossible
    {
      get
      {
        return !this.keypairGenerated || this.isCRTpossible;
      }
    }

    private byte[] GetPaddedValue(BigInteger value, int length)
    {
      byte[] bytes = value.GetBytes();
      if (bytes.Length >= length)
        return bytes;
      byte[] numArray = new byte[length];
      Buffer.BlockCopy((Array) bytes, 0, (Array) numArray, length - bytes.Length, bytes.Length);
      Array.Clear((Array) bytes, 0, bytes.Length);
      return numArray;
    }

    public delegate void KeyGeneratedEventHandler(object sender, EventArgs e);
  }
}
