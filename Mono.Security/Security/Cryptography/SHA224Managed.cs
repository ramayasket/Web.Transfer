// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.SHA224Managed
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security.Cryptography
{
  public class SHA224Managed : SHA224
  {
    private const int BLOCK_SIZE_BYTES = 64;
    private uint[] _H;
    private ulong count;
    private byte[] _ProcessingBuffer;
    private int _ProcessingBufferCount;

    public SHA224Managed()
    {
      this._H = new uint[8];
      this._ProcessingBuffer = new byte[64];
      this.Initialize();
    }

    private uint Ch(uint u, uint v, uint w)
    {
      return (uint) ((int) u & (int) v ^ ~(int) u & (int) w);
    }

    private uint Maj(uint u, uint v, uint w)
    {
      return (uint) ((int) u & (int) v ^ (int) u & (int) w ^ (int) v & (int) w);
    }

    private uint Ro0(uint x)
    {
      return (uint) (((int) (x >> 7) | (int) x << 25) ^ ((int) (x >> 18) | (int) x << 14)) ^ x >> 3;
    }

    private uint Ro1(uint x)
    {
      return (uint) (((int) (x >> 17) | (int) x << 15) ^ ((int) (x >> 19) | (int) x << 13)) ^ x >> 10;
    }

    private uint Sig0(uint x)
    {
      return (uint) (((int) (x >> 2) | (int) x << 30) ^ ((int) (x >> 13) | (int) x << 19) ^ ((int) (x >> 22) | (int) x << 10));
    }

    private uint Sig1(uint x)
    {
      return (uint) (((int) (x >> 6) | (int) x << 26) ^ ((int) (x >> 11) | (int) x << 21) ^ ((int) (x >> 25) | (int) x << 7));
    }

    protected override void HashCore(byte[] rgb, int start, int size)
    {
      this.State = 1;
      if (this._ProcessingBufferCount != 0)
      {
        if (size < 64 - this._ProcessingBufferCount)
        {
          Buffer.BlockCopy((Array) rgb, start, (Array) this._ProcessingBuffer, this._ProcessingBufferCount, size);
          this._ProcessingBufferCount += size;
          return;
        }
        int count = 64 - this._ProcessingBufferCount;
        Buffer.BlockCopy((Array) rgb, start, (Array) this._ProcessingBuffer, this._ProcessingBufferCount, count);
        this.ProcessBlock(this._ProcessingBuffer, 0);
        this._ProcessingBufferCount = 0;
        start += count;
        size -= count;
      }
      for (int index = 0; index < size - size % 64; index += 64)
        this.ProcessBlock(rgb, start + index);
      if (size % 64 == 0)
        return;
      Buffer.BlockCopy((Array) rgb, size - size % 64 + start, (Array) this._ProcessingBuffer, 0, size % 64);
      this._ProcessingBufferCount = size % 64;
    }

    protected override byte[] HashFinal()
    {
      byte[] numArray = new byte[28];
      this.ProcessFinalBlock(this._ProcessingBuffer, 0, this._ProcessingBufferCount);
      for (int index1 = 0; index1 < 7; ++index1)
      {
        for (int index2 = 0; index2 < 4; ++index2)
          numArray[index1 * 4 + index2] = (byte) (this._H[index1] >> 24 - index2 * 8);
      }
      this.State = 0;
      return numArray;
    }

    public override void Initialize()
    {
      this.count = 0UL;
      this._ProcessingBufferCount = 0;
      this._H[0] = 3238371032U;
      this._H[1] = 914150663U;
      this._H[2] = 812702999U;
      this._H[3] = 4144912697U;
      this._H[4] = 4290775857U;
      this._H[5] = 1750603025U;
      this._H[6] = 1694076839U;
      this._H[7] = 3204075428U;
    }

    private void ProcessBlock(byte[] inputBuffer, int inputOffset)
    {
      this.count += 64UL;
      uint[] numArray = new uint[64];
      for (int index = 0; index < 16; ++index)
        numArray[index] = (uint) ((int) inputBuffer[inputOffset + 4 * index] << 24 | (int) inputBuffer[inputOffset + 4 * index + 1] << 16 | (int) inputBuffer[inputOffset + 4 * index + 2] << 8) | (uint) inputBuffer[inputOffset + 4 * index + 3];
      for (int index = 16; index < 64; ++index)
        numArray[index] = this.Ro1(numArray[index - 2]) + numArray[index - 7] + this.Ro0(numArray[index - 15]) + numArray[index - 16];
      uint num1 = this._H[0];
      uint v1 = this._H[1];
      uint w1 = this._H[2];
      uint num2 = this._H[3];
      uint num3 = this._H[4];
      uint v2 = this._H[5];
      uint w2 = this._H[6];
      uint num4 = this._H[7];
      for (int index = 0; index < 64; ++index)
      {
        uint num5 = num4 + this.Sig1(num3) + this.Ch(num3, v2, w2) + SHAConstants.K1[index] + numArray[index];
        uint num6 = this.Sig0(num1) + this.Maj(num1, v1, w1);
        num4 = w2;
        w2 = v2;
        v2 = num3;
        num3 = num2 + num5;
        num2 = w1;
        w1 = v1;
        v1 = num1;
        num1 = num5 + num6;
      }
      this._H[0] += num1;
      this._H[1] += v1;
      this._H[2] += w1;
      this._H[3] += num2;
      this._H[4] += num3;
      this._H[5] += v2;
      this._H[6] += w2;
      this._H[7] += num4;
    }

    private void ProcessFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
    {
      ulong num1 = this.count + (ulong) inputCount;
      int num2 = 56 - (int) (num1 % 64UL);
      if (num2 < 1)
        num2 += 64;
      byte[] numArray = new byte[inputCount + num2 + 8];
      for (int index = 0; index < inputCount; ++index)
        numArray[index] = inputBuffer[index + inputOffset];
      numArray[inputCount] = (byte) 128;
      for (int index = inputCount + 1; index < inputCount + num2; ++index)
        numArray[index] = (byte) 0;
      this.AddLength(num1 << 3, numArray, inputCount + num2);
      this.ProcessBlock(numArray, 0);
      if (inputCount + num2 + 8 != 128)
        return;
      this.ProcessBlock(numArray, 64);
    }

    internal void AddLength(ulong length, byte[] buffer, int position)
    {
      buffer[position++] = (byte) (length >> 56);
      buffer[position++] = (byte) (length >> 48);
      buffer[position++] = (byte) (length >> 40);
      buffer[position++] = (byte) (length >> 32);
      buffer[position++] = (byte) (length >> 24);
      buffer[position++] = (byte) (length >> 16);
      buffer[position++] = (byte) (length >> 8);
      buffer[position] = (byte) length;
    }
  }
}
