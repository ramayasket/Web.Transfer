// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.HMAC
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography
{
  internal class HMAC : KeyedHashAlgorithm
  {
    private HashAlgorithm hash;
    private bool hashing;
    private byte[] innerPad;
    private byte[] outerPad;

    public HMAC()
    {
      this.hash = (HashAlgorithm) MD5.Create();
      this.HashSizeValue = this.hash.HashSize;
      byte[] data = new byte[64];
      new RNGCryptoServiceProvider().GetNonZeroBytes(data);
      this.KeyValue = (byte[]) data.Clone();
      this.Initialize();
    }

    public HMAC(string hashName, byte[] rgbKey)
    {
      switch (hashName)
      {
        case "":
        case null:
          hashName = "MD5";
          break;
      }
      this.hash = HashAlgorithm.Create(hashName);
      this.HashSizeValue = this.hash.HashSize;
      if (rgbKey.Length > 64)
        this.KeyValue = this.hash.ComputeHash(rgbKey);
      else
        this.KeyValue = (byte[]) rgbKey.Clone();
      this.Initialize();
    }

    public override byte[] Key
    {
      get
      {
        return (byte[]) this.KeyValue.Clone();
      }
      set
      {
        if (this.hashing)
          throw new Exception("Cannot change key during hash operation.");
        if (value.Length > 64)
          this.KeyValue = this.hash.ComputeHash(value);
        else
          this.KeyValue = (byte[]) value.Clone();
        this.initializePad();
      }
    }

    public override void Initialize()
    {
      this.hash.Initialize();
      this.initializePad();
      this.hashing = false;
    }

    protected override byte[] HashFinal()
    {
      if (!this.hashing)
      {
        this.hash.TransformBlock(this.innerPad, 0, this.innerPad.Length, this.innerPad, 0);
        this.hashing = true;
      }
      this.hash.TransformFinalBlock(new byte[0], 0, 0);
      byte[] hash = this.hash.Hash;
      this.hash.Initialize();
      this.hash.TransformBlock(this.outerPad, 0, this.outerPad.Length, this.outerPad, 0);
      this.hash.TransformFinalBlock(hash, 0, hash.Length);
      this.Initialize();
      return this.hash.Hash;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
      if (!this.hashing)
      {
        this.hash.TransformBlock(this.innerPad, 0, this.innerPad.Length, this.innerPad, 0);
        this.hashing = true;
      }
      this.hash.TransformBlock(array, ibStart, cbSize, array, ibStart);
    }

    private void initializePad()
    {
      this.innerPad = new byte[64];
      this.outerPad = new byte[64];
      for (int index = 0; index < this.KeyValue.Length; ++index)
      {
        this.innerPad[index] = (byte) ((uint) this.KeyValue[index] ^ 54U);
        this.outerPad[index] = (byte) ((uint) this.KeyValue[index] ^ 92U);
      }
      for (int length = this.KeyValue.Length; length < 64; ++length)
      {
        this.innerPad[length] = (byte) 54;
        this.outerPad[length] = (byte) 92;
      }
    }
  }
}
