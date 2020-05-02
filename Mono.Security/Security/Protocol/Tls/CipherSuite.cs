// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.CipherSuite
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security.Protocol.Tls
{
  internal abstract class CipherSuite
  {
    public static byte[] EmptyArray = new byte[0];
    private short code;
    private string name;
    private CipherAlgorithmType cipherAlgorithmType;
    private HashAlgorithmType hashAlgorithmType;
    private ExchangeAlgorithmType exchangeAlgorithmType;
    private bool isExportable;
    private CipherMode cipherMode;
    private byte keyMaterialSize;
    private int keyBlockSize;
    private byte expandedKeyMaterialSize;
    private short effectiveKeyBits;
    private byte ivSize;
    private byte blockSize;
    private Context context;
    private SymmetricAlgorithm encryptionAlgorithm;
    private ICryptoTransform encryptionCipher;
    private SymmetricAlgorithm decryptionAlgorithm;
    private ICryptoTransform decryptionCipher;
    private KeyedHashAlgorithm clientHMAC;
    private KeyedHashAlgorithm serverHMAC;

    public CipherSuite(
      short code,
      string name,
      CipherAlgorithmType cipherAlgorithmType,
      HashAlgorithmType hashAlgorithmType,
      ExchangeAlgorithmType exchangeAlgorithmType,
      bool exportable,
      bool blockMode,
      byte keyMaterialSize,
      byte expandedKeyMaterialSize,
      short effectiveKeyBits,
      byte ivSize,
      byte blockSize)
    {
      this.code = code;
      this.name = name;
      this.cipherAlgorithmType = cipherAlgorithmType;
      this.hashAlgorithmType = hashAlgorithmType;
      this.exchangeAlgorithmType = exchangeAlgorithmType;
      this.isExportable = exportable;
      if (blockMode)
        this.cipherMode = CipherMode.CBC;
      this.keyMaterialSize = keyMaterialSize;
      this.expandedKeyMaterialSize = expandedKeyMaterialSize;
      this.effectiveKeyBits = effectiveKeyBits;
      this.ivSize = ivSize;
      this.blockSize = blockSize;
      this.keyBlockSize = (int) this.keyMaterialSize + this.HashSize + (int) this.ivSize << 1;
    }

    protected ICryptoTransform EncryptionCipher
    {
      get
      {
        return this.encryptionCipher;
      }
    }

    protected ICryptoTransform DecryptionCipher
    {
      get
      {
        return this.decryptionCipher;
      }
    }

    protected KeyedHashAlgorithm ClientHMAC
    {
      get
      {
        return this.clientHMAC;
      }
    }

    protected KeyedHashAlgorithm ServerHMAC
    {
      get
      {
        return this.serverHMAC;
      }
    }

    public CipherAlgorithmType CipherAlgorithmType
    {
      get
      {
        return this.cipherAlgorithmType;
      }
    }

    public string HashAlgorithmName
    {
      get
      {
        switch (this.hashAlgorithmType)
        {
          case HashAlgorithmType.Md5:
            return "MD5";
          case HashAlgorithmType.Sha1:
            return "SHA1";
          default:
            return "None";
        }
      }
    }

    public HashAlgorithmType HashAlgorithmType
    {
      get
      {
        return this.hashAlgorithmType;
      }
    }

    public int HashSize
    {
      get
      {
        switch (this.hashAlgorithmType)
        {
          case HashAlgorithmType.Md5:
            return 16;
          case HashAlgorithmType.Sha1:
            return 20;
          default:
            return 0;
        }
      }
    }

    public ExchangeAlgorithmType ExchangeAlgorithmType
    {
      get
      {
        return this.exchangeAlgorithmType;
      }
    }

    public CipherMode CipherMode
    {
      get
      {
        return this.cipherMode;
      }
    }

    public short Code
    {
      get
      {
        return this.code;
      }
    }

    public string Name
    {
      get
      {
        return this.name;
      }
    }

    public bool IsExportable
    {
      get
      {
        return this.isExportable;
      }
    }

    public byte KeyMaterialSize
    {
      get
      {
        return this.keyMaterialSize;
      }
    }

    public int KeyBlockSize
    {
      get
      {
        return this.keyBlockSize;
      }
    }

    public byte ExpandedKeyMaterialSize
    {
      get
      {
        return this.expandedKeyMaterialSize;
      }
    }

    public short EffectiveKeyBits
    {
      get
      {
        return this.effectiveKeyBits;
      }
    }

    public byte IvSize
    {
      get
      {
        return this.ivSize;
      }
    }

    public Context Context
    {
      get
      {
        return this.context;
      }
      set
      {
        this.context = value;
      }
    }

    internal void Write(byte[] array, int offset, short value)
    {
      if (offset > array.Length - 2)
        throw new ArgumentException(nameof (offset));
      array[offset] = (byte) ((uint) value >> 8);
      array[offset + 1] = (byte) value;
    }

    internal void Write(byte[] array, int offset, ulong value)
    {
      if (offset > array.Length - 8)
        throw new ArgumentException(nameof (offset));
      array[offset] = (byte) (value >> 56);
      array[offset + 1] = (byte) (value >> 48);
      array[offset + 2] = (byte) (value >> 40);
      array[offset + 3] = (byte) (value >> 32);
      array[offset + 4] = (byte) (value >> 24);
      array[offset + 5] = (byte) (value >> 16);
      array[offset + 6] = (byte) (value >> 8);
      array[offset + 7] = (byte) value;
    }

    public void InitializeCipher()
    {
      this.createEncryptionCipher();
      this.createDecryptionCipher();
    }

    public byte[] EncryptRecord(byte[] fragment, byte[] mac)
    {
      int length = fragment.Length + mac.Length;
      int num1 = 0;
      if (this.CipherMode == CipherMode.CBC)
      {
        int num2 = length + 1;
        num1 = (int) this.blockSize - num2 % (int) this.blockSize;
        if (num1 == (int) this.blockSize)
          num1 = 0;
        length = num2 + num1;
      }
      byte[] numArray = new byte[length];
      Buffer.BlockCopy((Array) fragment, 0, (Array) numArray, 0, fragment.Length);
      Buffer.BlockCopy((Array) mac, 0, (Array) numArray, fragment.Length, mac.Length);
      if (num1 > 0)
      {
        int num2 = fragment.Length + mac.Length;
        for (int index = num2; index < num2 + num1 + 1; ++index)
          numArray[index] = (byte) num1;
      }
      this.EncryptionCipher.TransformBlock(numArray, 0, numArray.Length, numArray, 0);
      return numArray;
    }

    public void DecryptRecord(byte[] fragment, out byte[] dcrFragment, out byte[] dcrMAC)
    {
      this.DecryptionCipher.TransformBlock(fragment, 0, fragment.Length, fragment, 0);
      int length;
      if (this.CipherMode == CipherMode.CBC)
      {
        int num = (int) fragment[fragment.Length - 1];
        length = fragment.Length - (num + 1) - this.HashSize;
      }
      else
        length = fragment.Length - this.HashSize;
      dcrFragment = new byte[length];
      dcrMAC = new byte[this.HashSize];
      Buffer.BlockCopy((Array) fragment, 0, (Array) dcrFragment, 0, dcrFragment.Length);
      Buffer.BlockCopy((Array) fragment, dcrFragment.Length, (Array) dcrMAC, 0, dcrMAC.Length);
    }

    public abstract byte[] ComputeClientRecordMAC(ContentType contentType, byte[] fragment);

    public abstract byte[] ComputeServerRecordMAC(ContentType contentType, byte[] fragment);

    public abstract void ComputeMasterSecret(byte[] preMasterSecret);

    public abstract void ComputeKeys();

    public byte[] CreatePremasterSecret()
    {
      ClientContext context = (ClientContext) this.context;
      byte[] secureRandomBytes = this.context.GetSecureRandomBytes(48);
      secureRandomBytes[0] = (byte) ((uint) context.ClientHelloProtocol >> 8);
      secureRandomBytes[1] = (byte) context.ClientHelloProtocol;
      return secureRandomBytes;
    }

    public byte[] PRF(byte[] secret, string label, byte[] data, int length)
    {
      int count = secret.Length >> 1;
      if ((secret.Length & 1) == 1)
        ++count;
      TlsStream tlsStream = new TlsStream();
      tlsStream.Write(Encoding.ASCII.GetBytes(label));
      tlsStream.Write(data);
      byte[] array = tlsStream.ToArray();
      tlsStream.Reset();
      byte[] secret1 = new byte[count];
      Buffer.BlockCopy((Array) secret, 0, (Array) secret1, 0, count);
      byte[] secret2 = new byte[count];
      Buffer.BlockCopy((Array) secret, secret.Length - count, (Array) secret2, 0, count);
      byte[] numArray1 = this.Expand("MD5", secret1, array, length);
      byte[] numArray2 = this.Expand("SHA1", secret2, array, length);
      byte[] numArray3 = new byte[length];
      for (int index = 0; index < numArray3.Length; ++index)
        numArray3[index] = (byte) ((uint) numArray1[index] ^ (uint) numArray2[index]);
      return numArray3;
    }

    public byte[] Expand(string hashName, byte[] secret, byte[] seed, int length)
    {
      int num1 = !(hashName == "MD5") ? 20 : 16;
      int num2 = length / num1;
      if (length % num1 > 0)
        ++num2;
      Mono.Security.Cryptography.HMAC hmac = new Mono.Security.Cryptography.HMAC(hashName, secret);
      TlsStream tlsStream1 = new TlsStream();
      byte[][] numArray1 = new byte[num2 + 1][];
      numArray1[0] = seed;
      for (int index = 1; index <= num2; ++index)
      {
        TlsStream tlsStream2 = new TlsStream();
        hmac.TransformFinalBlock(numArray1[index - 1], 0, numArray1[index - 1].Length);
        numArray1[index] = hmac.Hash;
        tlsStream2.Write(numArray1[index]);
        tlsStream2.Write(seed);
        hmac.TransformFinalBlock(tlsStream2.ToArray(), 0, (int) tlsStream2.Length);
        tlsStream1.Write(hmac.Hash);
        tlsStream2.Reset();
      }
      byte[] numArray2 = new byte[length];
      Buffer.BlockCopy((Array) tlsStream1.ToArray(), 0, (Array) numArray2, 0, numArray2.Length);
      tlsStream1.Reset();
      return numArray2;
    }

    private void createEncryptionCipher()
    {
      switch (this.cipherAlgorithmType)
      {
        case CipherAlgorithmType.Des:
          this.encryptionAlgorithm = (SymmetricAlgorithm) DES.Create();
          break;
        case CipherAlgorithmType.Rc2:
          this.encryptionAlgorithm = (SymmetricAlgorithm) RC2.Create();
          break;
        case CipherAlgorithmType.Rc4:
          this.encryptionAlgorithm = (SymmetricAlgorithm) new ARC4Managed();
          break;
        case CipherAlgorithmType.Rijndael:
          this.encryptionAlgorithm = (SymmetricAlgorithm) Rijndael.Create();
          break;
        case CipherAlgorithmType.TripleDes:
          this.encryptionAlgorithm = (SymmetricAlgorithm) TripleDES.Create();
          break;
      }
      if (this.cipherMode == CipherMode.CBC)
      {
        this.encryptionAlgorithm.Mode = this.cipherMode;
        this.encryptionAlgorithm.Padding = PaddingMode.None;
        this.encryptionAlgorithm.KeySize = (int) this.expandedKeyMaterialSize * 8;
        this.encryptionAlgorithm.BlockSize = (int) this.blockSize * 8;
      }
      if (this.context is ClientContext)
      {
        this.encryptionAlgorithm.Key = this.context.ClientWriteKey;
        this.encryptionAlgorithm.IV = this.context.ClientWriteIV;
      }
      else
      {
        this.encryptionAlgorithm.Key = this.context.ServerWriteKey;
        this.encryptionAlgorithm.IV = this.context.ServerWriteIV;
      }
      this.encryptionCipher = this.encryptionAlgorithm.CreateEncryptor();
      if (this.context is ClientContext)
        this.clientHMAC = (KeyedHashAlgorithm) new Mono.Security.Cryptography.HMAC(this.HashAlgorithmName, this.context.Negotiating.ClientWriteMAC);
      else
        this.serverHMAC = (KeyedHashAlgorithm) new Mono.Security.Cryptography.HMAC(this.HashAlgorithmName, this.context.Negotiating.ServerWriteMAC);
    }

    private void createDecryptionCipher()
    {
      switch (this.cipherAlgorithmType)
      {
        case CipherAlgorithmType.Des:
          this.decryptionAlgorithm = (SymmetricAlgorithm) DES.Create();
          break;
        case CipherAlgorithmType.Rc2:
          this.decryptionAlgorithm = (SymmetricAlgorithm) RC2.Create();
          break;
        case CipherAlgorithmType.Rc4:
          this.decryptionAlgorithm = (SymmetricAlgorithm) new ARC4Managed();
          break;
        case CipherAlgorithmType.Rijndael:
          this.decryptionAlgorithm = (SymmetricAlgorithm) Rijndael.Create();
          break;
        case CipherAlgorithmType.TripleDes:
          this.decryptionAlgorithm = (SymmetricAlgorithm) TripleDES.Create();
          break;
      }
      if (this.cipherMode == CipherMode.CBC)
      {
        this.decryptionAlgorithm.Mode = this.cipherMode;
        this.decryptionAlgorithm.Padding = PaddingMode.None;
        this.decryptionAlgorithm.KeySize = (int) this.expandedKeyMaterialSize * 8;
        this.decryptionAlgorithm.BlockSize = (int) this.blockSize * 8;
      }
      if (this.context is ClientContext)
      {
        this.decryptionAlgorithm.Key = this.context.ServerWriteKey;
        this.decryptionAlgorithm.IV = this.context.ServerWriteIV;
      }
      else
      {
        this.decryptionAlgorithm.Key = this.context.ClientWriteKey;
        this.decryptionAlgorithm.IV = this.context.ClientWriteIV;
      }
      this.decryptionCipher = this.decryptionAlgorithm.CreateDecryptor();
      if (this.context is ClientContext)
        this.serverHMAC = (KeyedHashAlgorithm) new Mono.Security.Cryptography.HMAC(this.HashAlgorithmName, this.context.Negotiating.ServerWriteMAC);
      else
        this.clientHMAC = (KeyedHashAlgorithm) new Mono.Security.Cryptography.HMAC(this.HashAlgorithmName, this.context.Negotiating.ClientWriteMAC);
    }
  }
}
