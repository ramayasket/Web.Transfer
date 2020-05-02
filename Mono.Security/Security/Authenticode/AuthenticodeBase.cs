// Decompiled with JetBrains decompiler
// Type: Mono.Security.Authenticode.AuthenticodeBase
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.IO;
using System.Security.Cryptography;

namespace Mono.Security.Authenticode
{
  using Mono.Security;
  using System;
  using System.IO;
  using System.Security.Cryptography;

  public class AuthenticodeBase
  {
    public const string spcIndirectDataContext = "1.3.6.1.4.1.311.2.1.4";
    private byte[] fileblock = new byte[0x1000];
    private FileStream fs;
    private int blockNo;
    private int blockLength;
    private int peOffset;
    private int dirSecurityOffset;
    private int dirSecuritySize;
    private int coffSymbolTableOffset;

    internal void Close()
    {
      if (this.fs != null)
      {
        this.fs.Close();
        this.fs = null;
      }
    }

    internal byte[] GetHash(HashAlgorithm hash)
    {
      long num;
      if (this.blockNo < 1)
      {
        this.ReadFirstBlock();
      }
      this.fs.Position = this.blockLength;
      int inputCount = 0;
      if (this.dirSecurityOffset > 0)
      {
        if (this.dirSecurityOffset < this.blockLength)
        {
          this.blockLength = this.dirSecurityOffset;
          num = 0L;
        }
        else
        {
          num = this.dirSecurityOffset - this.blockLength;
        }
      }
      else if (this.coffSymbolTableOffset > 0)
      {
        this.fileblock[this.PEOffset + 12] = 0;
        this.fileblock[this.PEOffset + 13] = 0;
        this.fileblock[this.PEOffset + 14] = 0;
        this.fileblock[this.PEOffset + 15] = 0;
        this.fileblock[this.PEOffset + 0x10] = 0;
        this.fileblock[this.PEOffset + 0x11] = 0;
        this.fileblock[this.PEOffset + 0x12] = 0;
        this.fileblock[this.PEOffset + 0x13] = 0;
        if (this.coffSymbolTableOffset < this.blockLength)
        {
          this.blockLength = this.coffSymbolTableOffset;
          num = 0L;
        }
        else
        {
          num = this.coffSymbolTableOffset - this.blockLength;
        }
      }
      else
      {
        inputCount = (int)(this.fs.Length & 7L);
        if (inputCount > 0)
        {
          inputCount = 8 - inputCount;
        }
        num = this.fs.Length - this.blockLength;
      }
      int num3 = this.peOffset + 0x58;
      hash.TransformBlock(this.fileblock, 0, num3, this.fileblock, 0);
      num3 += 4;
      hash.TransformBlock(this.fileblock, num3, 60, this.fileblock, num3);
      num3 += 0x44;
      if (num == 0L)
      {
        hash.TransformFinalBlock(this.fileblock, num3, this.blockLength - num3);
        goto Label_02B1;
      }
      hash.TransformBlock(this.fileblock, num3, this.blockLength - num3, this.fileblock, num3);
      long num4 = num >> 12;
      int count = (int)(num - (num4 << 12));
      if (count == 0)
      {
        num4 -= 1L;
        count = 0x1000;
      }
      Label_0245:
      num4 -= 1L;
      if (num4 > 0L)
      {
        this.fs.Read(this.fileblock, 0, this.fileblock.Length);
        hash.TransformBlock(this.fileblock, 0, this.fileblock.Length, this.fileblock, 0);
        goto Label_0245;
      }
      if (this.fs.Read(this.fileblock, 0, count) != count)
      {
        return null;
      }
      if (inputCount > 0)
      {
        hash.TransformBlock(this.fileblock, 0, count, this.fileblock, 0);
        hash.TransformFinalBlock(new byte[inputCount], 0, inputCount);
      }
      else
      {
        hash.TransformFinalBlock(this.fileblock, 0, count);
      }
      Label_02B1:
      return hash.Hash;
    }

    internal byte[] GetSecurityEntry()
    {
      if (this.blockNo < 1)
      {
        this.ReadFirstBlock();
      }
      if (this.dirSecuritySize > 8)
      {
        byte[] buffer = new byte[this.dirSecuritySize - 8];
        this.fs.Position = this.dirSecurityOffset + 8;
        this.fs.Read(buffer, 0, buffer.Length);
        return buffer;
      }
      return null;
    }

    protected byte[] HashFile(string fileName, string hashName)
    {
      try
      {
        this.Open(fileName);
        HashAlgorithm hash = HashAlgorithm.Create(hashName);
        byte[] buffer = this.GetHash(hash);
        this.Close();
        return buffer;
      }
      catch
      {
        return null;
      }
    }

    internal void Open(string filename)
    {
      if (this.fs != null)
      {
        this.Close();
      }
      this.fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
      this.blockNo = 0;
    }

    internal int ProcessFirstBlock()
    {
      if (this.fs == null)
      {
        return 1;
      }
      this.fs.Position = 0L;
      this.blockLength = this.fs.Read(this.fileblock, 0, this.fileblock.Length);
      this.blockNo = 1;
      if (this.blockLength < 0x40)
      {
        return 2;
      }
      if (BitConverterLE.ToUInt16(this.fileblock, 0) != 0x5a4d)
      {
        return 3;
      }
      this.peOffset = BitConverterLE.ToInt32(this.fileblock, 60);
      if (this.peOffset > this.fileblock.Length)
      {
        throw new NotSupportedException(string.Format(Locale.GetText("Header size too big (> {0} bytes)."), this.fileblock.Length));
      }
      if (this.peOffset > this.fs.Length)
      {
        return 4;
      }
      if (BitConverterLE.ToUInt32(this.fileblock, this.peOffset) != 0x4550)
      {
        return 5;
      }
      this.dirSecurityOffset = BitConverterLE.ToInt32(this.fileblock, this.peOffset + 0x98);
      this.dirSecuritySize = BitConverterLE.ToInt32(this.fileblock, this.peOffset + 0x9c);
      this.coffSymbolTableOffset = BitConverterLE.ToInt32(this.fileblock, this.peOffset + 12);
      return 0;
    }

    internal void ReadFirstBlock()
    {
      int num = this.ProcessFirstBlock();
      if (num != 0)
      {
        object[] args = new object[] { num };
        throw new NotSupportedException(Locale.GetText("Cannot sign non PE files, e.g. .CAB or .MSI files (error {0}).", args));
      }
    }

    internal int PEOffset
    {
      get
      {
        if (this.blockNo < 1)
        {
          this.ReadFirstBlock();
        }
        return this.peOffset;
      }
    }

    internal int CoffSymbolTableOffset
    {
      get
      {
        if (this.blockNo < 1)
        {
          this.ReadFirstBlock();
        }
        return this.coffSymbolTableOffset;
      }
    }

    internal int SecurityOffset
    {
      get
      {
        if (this.blockNo < 1)
        {
          this.ReadFirstBlock();
        }
        return this.dirSecurityOffset;
      }
    }
  }
}
