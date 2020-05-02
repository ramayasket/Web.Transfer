// Decompiled with JetBrains decompiler
// Type: Mono.Security.StrongName
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System;
using System.Configuration.Assemblies;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;

namespace Mono.Security
{
  using Mono.Security.Cryptography;
  using System;
  using System.Configuration.Assemblies;
  using System.Globalization;
  using System.IO;
  using System.Security.Cryptography;

  public sealed class StrongName
  {
    private System.Security.Cryptography.RSA rsa;
    private byte[] publicKey;
    private byte[] keyToken;
    private string tokenAlgorithm;

    public StrongName()
    {
    }

    public StrongName(int keySize)
    {
      this.rsa = new RSAManaged(keySize);
    }

    public StrongName(byte[] data)
    {
      if (data == null)
      {
        throw new ArgumentNullException("data");
      }
      if (data.Length == 0x10)
      {
        int num = 0;
        int num2 = 0;
        while (num < data.Length)
        {
          num2 += data[num++];
        }
        if (num2 == 4)
        {
          this.publicKey = (byte[])data.Clone();
        }
      }
      else
      {
        this.RSA = CryptoConvert.FromCapiKeyBlob(data);
        if (this.rsa == null)
        {
          throw new ArgumentException("data isn't a correctly encoded RSA public key");
        }
      }
    }

    public StrongName(System.Security.Cryptography.RSA rsa)
    {
      if (rsa == null)
      {
        throw new ArgumentNullException("rsa");
      }
      this.RSA = rsa;
    }

    public byte[] GetBytes() =>
        CryptoConvert.ToCapiPrivateKeyBlob(this.RSA);

    public byte[] Hash(string fileName)
    {
      FileStream stream = File.OpenRead(fileName);
      StrongNameSignature signature = this.StrongHash(stream, StrongNameOptions.Metadata);
      stream.Close();
      return signature.Hash;
    }

    private void InvalidateCache()
    {
      this.publicKey = null;
      this.keyToken = null;
    }

    private uint RVAtoPosition(uint r, int sections, byte[] headers)
    {
      for (int i = 0; i < sections; i++)
      {
        uint num2 = BitConverterLE.ToUInt32(headers, (i * 40) + 20);
        uint num3 = BitConverterLE.ToUInt32(headers, (i * 40) + 12);
        int num4 = (int)BitConverterLE.ToUInt32(headers, (i * 40) + 8);
        if ((num3 <= r) && (r < (num3 + num4)))
        {
          return ((num2 + r) - num3);
        }
      }
      return 0;
    }

    public bool Sign(string fileName)
    {
      StrongNameSignature signature;
      using (FileStream stream = File.OpenRead(fileName))
      {
        signature = this.StrongHash(stream, StrongNameOptions.Signature);
        stream.Close();
      }
      if (signature.Hash == null)
      {
        return false;
      }
      byte[] array = null;
      try
      {
        RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(this.rsa);
        formatter.SetHashAlgorithm(this.TokenAlgorithm);
        array = formatter.CreateSignature(signature.Hash);
        Array.Reverse(array);
      }
      catch (CryptographicException)
      {
        return false;
      }
      using (FileStream stream2 = File.OpenWrite(fileName))
      {
        stream2.Position = signature.SignaturePosition;
        stream2.Write(array, 0, array.Length);
        stream2.Close();
        return true;
      }
    }

    internal StrongNameSignature StrongHash(Stream stream, StrongNameOptions options)
    {
      StrongNameSignature signature = new StrongNameSignature();
      HashAlgorithm transform = HashAlgorithm.Create(this.TokenAlgorithm);
      CryptoStream stream2 = new CryptoStream(Stream.Null, transform, CryptoStreamMode.Write);
      byte[] buffer = new byte[0x80];
      stream.Read(buffer, 0, 0x80);
      if (BitConverterLE.ToUInt16(buffer, 0) != 0x5a4d)
      {
        return null;
      }
      uint num = BitConverterLE.ToUInt32(buffer, 60);
      stream2.Write(buffer, 0, 0x80);
      if (num != 0x80)
      {
        byte[] buffer2 = new byte[num - 0x80];
        stream.Read(buffer2, 0, buffer2.Length);
        stream2.Write(buffer2, 0, buffer2.Length);
      }
      byte[] buffer3 = new byte[0xf8];
      stream.Read(buffer3, 0, 0xf8);
      if (BitConverterLE.ToUInt32(buffer3, 0) != 0x4550)
      {
        return null;
      }
      if (BitConverterLE.ToUInt16(buffer3, 4) != 0x14c)
      {
        return null;
      }
      byte[] src = new byte[8];
      Buffer.BlockCopy(src, 0, buffer3, 0x58, 4);
      Buffer.BlockCopy(src, 0, buffer3, 0x98, 8);
      stream2.Write(buffer3, 0, 0xf8);
      ushort sections = BitConverterLE.ToUInt16(buffer3, 6);
      int count = sections * 40;
      byte[] buffer5 = new byte[count];
      stream.Read(buffer5, 0, count);
      stream2.Write(buffer5, 0, count);
      uint r = BitConverterLE.ToUInt32(buffer3, 0xe8);
      uint num5 = this.RVAtoPosition(r, sections, buffer5);
      int num6 = (int)BitConverterLE.ToUInt32(buffer3, 0xec);
      byte[] buffer6 = new byte[num6];
      stream.Position = num5;
      stream.Read(buffer6, 0, num6);
      uint num7 = BitConverterLE.ToUInt32(buffer6, 0x20);
      signature.SignaturePosition = this.RVAtoPosition(num7, sections, buffer5);
      signature.SignatureLength = BitConverterLE.ToUInt32(buffer6, 0x24);
      uint num8 = BitConverterLE.ToUInt32(buffer6, 8);
      signature.MetadataPosition = this.RVAtoPosition(num8, sections, buffer5);
      signature.MetadataLength = BitConverterLE.ToUInt32(buffer6, 12);
      if (options == StrongNameOptions.Metadata)
      {
        stream2.Close();
        transform.Initialize();
        byte[] buffer7 = new byte[signature.MetadataLength];
        stream.Position = signature.MetadataPosition;
        stream.Read(buffer7, 0, buffer7.Length);
        signature.Hash = transform.ComputeHash(buffer7);
        return signature;
      }
      for (int i = 0; i < sections; i++)
      {
        uint num10 = BitConverterLE.ToUInt32(buffer5, (i * 40) + 20);
        int num11 = (int)BitConverterLE.ToUInt32(buffer5, (i * 40) + 0x10);
        byte[] buffer8 = new byte[num11];
        stream.Position = num10;
        stream.Read(buffer8, 0, num11);
        if ((num10 <= signature.SignaturePosition) && (signature.SignaturePosition < (num10 + num11)))
        {
          int num12 = (int)(signature.SignaturePosition - num10);
          if (num12 > 0)
          {
            stream2.Write(buffer8, 0, num12);
          }
          signature.Signature = new byte[signature.SignatureLength];
          Buffer.BlockCopy(buffer8, num12, signature.Signature, 0, (int)signature.SignatureLength);
          Array.Reverse(signature.Signature);
          int offset = num12 + ((int)signature.SignatureLength);
          int num14 = num11 - offset;
          if (num14 > 0)
          {
            stream2.Write(buffer8, offset, num14);
          }
        }
        else
        {
          stream2.Write(buffer8, 0, num11);
        }
      }
      stream2.Close();
      signature.Hash = transform.Hash;
      return signature;
    }

    public bool Verify(Stream stream)
    {
      StrongNameSignature signature = this.StrongHash(stream, StrongNameOptions.Signature);
      if (signature.Hash == null)
      {
        return false;
      }
      try
      {
        AssemblyHashAlgorithm algorithm = AssemblyHashAlgorithm.SHA1;
        if (this.tokenAlgorithm == "MD5")
        {
          algorithm = AssemblyHashAlgorithm.MD5;
        }
        return Verify(this.rsa, algorithm, signature.Hash, signature.Signature);
      }
      catch (CryptographicException)
      {
        return false;
      }
    }

    public bool Verify(string fileName)
    {
      bool flag = false;
      using (FileStream stream = File.OpenRead(fileName))
      {
        flag = this.Verify(stream);
        stream.Close();
      }
      return flag;
    }

    private static bool Verify(System.Security.Cryptography.RSA rsa, AssemblyHashAlgorithm algorithm, byte[] hash, byte[] signature)
    {
      RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(rsa);
      switch (algorithm)
      {
        case AssemblyHashAlgorithm.MD5:
          deformatter.SetHashAlgorithm("MD5");
          break;

        default:
          deformatter.SetHashAlgorithm("SHA1");
          break;
      }
      return deformatter.VerifySignature(hash, signature);
    }

    public bool CanSign
    {
      get
      {
        if (this.rsa == null)
        {
          return false;
        }
        if (this.RSA is RSAManaged)
        {
          return !(this.rsa as RSAManaged).PublicOnly;
        }
        try
        {
          RSAParameters parameters = this.rsa.ExportParameters(true);
          return (((parameters.D != null) && (parameters.P != null)) && (parameters.Q != null));
        }
        catch (CryptographicException)
        {
          return false;
        }
      }
    }

    public System.Security.Cryptography.RSA RSA
    {
      get
      {
        if (this.rsa == null)
        {
          this.rsa = System.Security.Cryptography.RSA.Create();
        }
        return this.rsa;
      }
      set
      {
        this.rsa = value;
        this.InvalidateCache();
      }
    }

    public byte[] PublicKey
    {
      get
      {
        if (this.publicKey == null)
        {
          byte[] src = CryptoConvert.ToCapiKeyBlob(this.rsa, false);
          this.publicKey = new byte[0x20 + (this.rsa.KeySize >> 3)];
          this.publicKey[0] = src[4];
          this.publicKey[1] = src[5];
          this.publicKey[2] = src[6];
          this.publicKey[3] = src[7];
          this.publicKey[4] = 4;
          this.publicKey[5] = 0x80;
          this.publicKey[6] = 0;
          this.publicKey[7] = 0;
          byte[] bytes = BitConverterLE.GetBytes((int)(this.publicKey.Length - 12));
          this.publicKey[8] = bytes[0];
          this.publicKey[9] = bytes[1];
          this.publicKey[10] = bytes[2];
          this.publicKey[11] = bytes[3];
          this.publicKey[12] = 6;
          Buffer.BlockCopy(src, 1, this.publicKey, 13, this.publicKey.Length - 13);
          this.publicKey[0x17] = 0x31;
        }
        return (byte[])this.publicKey.Clone();
      }
    }

    public byte[] PublicKeyToken
    {
      get
      {
        if (this.keyToken == null)
        {
          byte[] publicKey = this.PublicKey;
          if (publicKey == null)
          {
            return null;
          }
          byte[] src = HashAlgorithm.Create(this.TokenAlgorithm).ComputeHash(publicKey);
          this.keyToken = new byte[8];
          Buffer.BlockCopy(src, src.Length - 8, this.keyToken, 0, 8);
          Array.Reverse(this.keyToken, 0, 8);
        }
        return (byte[])this.keyToken.Clone();
      }
    }

    public string TokenAlgorithm
    {
      get
      {
        if (this.tokenAlgorithm == null)
        {
          this.tokenAlgorithm = "SHA1";
        }
        return this.tokenAlgorithm;
      }
      set
      {
        string str = value.ToUpper(CultureInfo.InvariantCulture);
        if ((str != "SHA1") && (str != "MD5"))
        {
          throw new ArgumentException("Unsupported hash algorithm for token");
        }
        this.tokenAlgorithm = value;
        this.InvalidateCache();
      }
    }

    internal enum StrongNameOptions
    {
      Metadata,
      Signature
    }

    internal class StrongNameSignature
    {
      private byte[] hash;
      private byte[] signature;
      private uint signaturePosition;
      private uint signatureLength;
      private uint metadataPosition;
      private uint metadataLength;
      private byte cliFlag;
      private uint cliFlagPosition;

      public byte[] Hash
      {
        get =>
            this.hash;
        set => this.hash = value;
      }

      public byte[] Signature
      {
        get =>
            this.signature;
        set => this.signature = value;
      }

      public uint MetadataPosition
      {
        get =>
            this.metadataPosition;
        set => this.metadataPosition = value;
      }

      public uint MetadataLength
      {
        get =>
            this.metadataLength;
        set => this.metadataLength = value;
      }

      public uint SignaturePosition
      {
        get =>
            this.signaturePosition;
        set => this.signaturePosition = value;
      }

      public uint SignatureLength
      {
        get =>
            this.signatureLength;
        set => this.signatureLength = value;
      }

      public byte CliFlag
      {
        get =>
            this.cliFlag;
        set => this.cliFlag = value;
      }

      public uint CliFlagPosition
      {
        get =>
            this.cliFlagPosition;
        set => this.cliFlagPosition = value;
      }
    }
  }
}
