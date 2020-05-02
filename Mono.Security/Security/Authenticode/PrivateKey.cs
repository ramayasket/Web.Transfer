// Decompiled with JetBrains decompiler
// Type: Mono.Security.Authenticode.PrivateKey
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security.Authenticode
{
  public class PrivateKey
  {
    private const uint magic = 2964713758;
    private bool encrypted;
    private RSA rsa;
    private bool weak;
    private int keyType;

    public PrivateKey()
    {
      this.keyType = 2;
    }

    public PrivateKey(byte[] data, string password)
    {
      if (data == null)
        throw new ArgumentNullException(nameof (data));
      if (!this.Decode(data, password))
        throw new CryptographicException(Locale.GetText("Invalid data and/or password"));
    }

    public bool Encrypted
    {
      get
      {
        return this.encrypted;
      }
    }

    public int KeyType
    {
      get
      {
        return this.keyType;
      }
      set
      {
        this.keyType = value;
      }
    }

    public RSA RSA
    {
      get
      {
        return this.rsa;
      }
      set
      {
        this.rsa = value;
      }
    }

    public bool Weak
    {
      get
      {
        return !this.encrypted || this.weak;
      }
      set
      {
        this.weak = value;
      }
    }

    private byte[] DeriveKey(byte[] salt, string password)
    {
      byte[] bytes = Encoding.ASCII.GetBytes(password);
      SHA1 shA1 = SHA1.Create();
      shA1.TransformBlock(salt, 0, salt.Length, salt, 0);
      shA1.TransformFinalBlock(bytes, 0, bytes.Length);
      byte[] numArray = new byte[16];
      Buffer.BlockCopy((Array) shA1.Hash, 0, (Array) numArray, 0, 16);
      shA1.Clear();
      Array.Clear((Array) bytes, 0, bytes.Length);
      return numArray;
    }

    private bool Decode(byte[] pvk, string password)
    {
      if (BitConverterLE.ToUInt32(pvk, 0) != 2964713758U || BitConverterLE.ToUInt32(pvk, 4) != 0U)
        return false;
      this.keyType = BitConverterLE.ToInt32(pvk, 8);
      this.encrypted = BitConverterLE.ToUInt32(pvk, 12) == 1U;
      int int32_1 = BitConverterLE.ToInt32(pvk, 16);
      int int32_2 = BitConverterLE.ToInt32(pvk, 20);
      byte[] numArray = new byte[int32_2];
      Buffer.BlockCopy((Array) pvk, 24 + int32_1, (Array) numArray, 0, int32_2);
      if (int32_1 > 0)
      {
        if (password == null)
          return false;
        byte[] salt = new byte[int32_1];
        Buffer.BlockCopy((Array) pvk, 24, (Array) salt, 0, int32_1);
        byte[] rgbKey = this.DeriveKey(salt, password);
        RC4.Create().CreateDecryptor(rgbKey, (byte[]) null).TransformBlock(numArray, 8, numArray.Length - 8, numArray, 8);
        try
        {
          this.rsa = CryptoConvert.FromCapiPrivateKeyBlob(numArray);
          this.weak = false;
        }
        catch (CryptographicException)
        {
          this.weak = true;
          Buffer.BlockCopy((Array) pvk, 24 + int32_1, (Array) numArray, 0, int32_2);
          Array.Clear((Array) rgbKey, 5, 11);
          RC4.Create().CreateDecryptor(rgbKey, (byte[]) null).TransformBlock(numArray, 8, numArray.Length - 8, numArray, 8);
          this.rsa = CryptoConvert.FromCapiPrivateKeyBlob(numArray);
        }
        Array.Clear((Array) rgbKey, 0, rgbKey.Length);
      }
      else
      {
        this.weak = true;
        this.rsa = CryptoConvert.FromCapiPrivateKeyBlob(numArray);
        Array.Clear((Array) numArray, 0, numArray.Length);
      }
      Array.Clear((Array) pvk, 0, pvk.Length);
      return this.rsa != null;
    }

    public void Save(string filename)
    {
      this.Save(filename, (string) null);
    }

    public void Save(string filename, string password)
    {
      if (filename == null)
        throw new ArgumentNullException(nameof (filename));
      byte[] numArray1 = (byte[]) null;
      FileStream fileStream = File.Open(filename, FileMode.Create, FileAccess.Write);
      try
      {
        byte[] buffer = new byte[4];
        byte[] bytes1 = BitConverterLE.GetBytes(2964713758U);
        fileStream.Write(bytes1, 0, 4);
        fileStream.Write(buffer, 0, 4);
        byte[] bytes2 = BitConverterLE.GetBytes(this.keyType);
        fileStream.Write(bytes2, 0, 4);
        this.encrypted = password != null;
        numArray1 = CryptoConvert.ToCapiPrivateKeyBlob(this.rsa);
        if (this.encrypted)
        {
          byte[] bytes3 = BitConverterLE.GetBytes(1);
          fileStream.Write(bytes3, 0, 4);
          byte[] bytes4 = BitConverterLE.GetBytes(16);
          fileStream.Write(bytes4, 0, 4);
          byte[] bytes5 = BitConverterLE.GetBytes(numArray1.Length);
          fileStream.Write(bytes5, 0, 4);
          byte[] numArray2 = new byte[16];
          RC4 rc4 = RC4.Create();
          byte[] rgbKey = (byte[]) null;
          try
          {
            RandomNumberGenerator.Create().GetBytes(numArray2);
            fileStream.Write(numArray2, 0, numArray2.Length);
            rgbKey = this.DeriveKey(numArray2, password);
            if (this.Weak)
              Array.Clear((Array) rgbKey, 5, 11);
            rc4.CreateEncryptor(rgbKey, (byte[]) null).TransformBlock(numArray1, 8, numArray1.Length - 8, numArray1, 8);
          }
          finally
          {
            Array.Clear((Array) numArray2, 0, numArray2.Length);
            Array.Clear((Array) rgbKey, 0, rgbKey.Length);
            rc4.Clear();
          }
        }
        else
        {
          fileStream.Write(buffer, 0, 4);
          fileStream.Write(buffer, 0, 4);
          byte[] bytes3 = BitConverterLE.GetBytes(numArray1.Length);
          fileStream.Write(bytes3, 0, 4);
        }
        fileStream.Write(numArray1, 0, numArray1.Length);
      }
      finally
      {
        Array.Clear((Array) numArray1, 0, numArray1.Length);
        fileStream.Close();
      }
    }

    public static PrivateKey CreateFromFile(string filename)
    {
      return PrivateKey.CreateFromFile(filename, (string) null);
    }

    public static PrivateKey CreateFromFile(string filename, string password)
    {
      if (filename == null)
        throw new ArgumentNullException(nameof (filename));
      byte[] numArray = (byte[]) null;
      using (FileStream fileStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        numArray = new byte[fileStream.Length];
        fileStream.Read(numArray, 0, numArray.Length);
        fileStream.Close();
      }
      return new PrivateKey(numArray, password);
    }
  }
}
