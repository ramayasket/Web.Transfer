// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.BlockProcessor
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography
{
  public class BlockProcessor
  {
    private ICryptoTransform transform;
    private byte[] block;
    private int blockSize;
    private int blockCount;

    public BlockProcessor(ICryptoTransform transform)
      : this(transform, transform.InputBlockSize)
    {
    }

    public BlockProcessor(ICryptoTransform transform, int blockSize)
    {
      this.transform = transform;
      this.blockSize = blockSize;
      this.block = new byte[blockSize];
    }

    ~BlockProcessor()
    {
      Array.Clear((Array) this.block, 0, this.blockSize);
    }

    public void Initialize()
    {
      Array.Clear((Array) this.block, 0, this.blockSize);
      this.blockCount = 0;
    }

    public void Core(byte[] rgb)
    {
      this.Core(rgb, 0, rgb.Length);
    }

    public void Core(byte[] rgb, int ib, int cb)
    {
      int count = System.Math.Min(this.blockSize - this.blockCount, cb);
      Buffer.BlockCopy((Array) rgb, ib, (Array) this.block, this.blockCount, count);
      this.blockCount += count;
      if (this.blockCount != this.blockSize)
        return;
      this.transform.TransformBlock(this.block, 0, this.blockSize, this.block, 0);
      int num = (cb - count) / this.blockSize;
      for (int index = 0; index < num; ++index)
      {
        this.transform.TransformBlock(rgb, count + ib, this.blockSize, this.block, 0);
        count += this.blockSize;
      }
      this.blockCount = cb - count;
      if (this.blockCount <= 0)
        return;
      Buffer.BlockCopy((Array) rgb, count + ib, (Array) this.block, 0, this.blockCount);
    }

    public byte[] Final()
    {
      return this.transform.TransformFinalBlock(this.block, 0, this.blockCount);
    }
  }
}
