// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.MD4Managed
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security.Cryptography
{
  public class MD4Managed : MD4
  {
    private const int S11 = 3;
    private const int S12 = 7;
    private const int S13 = 11;
    private const int S14 = 19;
    private const int S21 = 3;
    private const int S22 = 5;
    private const int S23 = 9;
    private const int S24 = 13;
    private const int S31 = 3;
    private const int S32 = 9;
    private const int S33 = 11;
    private const int S34 = 15;
    private uint[] state;
    private byte[] buffer;
    private uint[] count;
    private uint[] x;
    private byte[] digest;

    public MD4Managed()
    {
      this.state = new uint[4];
      this.count = new uint[2];
      this.buffer = new byte[64];
      this.digest = new byte[16];
      this.x = new uint[16];
      this.Initialize();
    }

    public override void Initialize()
    {
      this.count[0] = 0U;
      this.count[1] = 0U;
      this.state[0] = 1732584193U;
      this.state[1] = 4023233417U;
      this.state[2] = 2562383102U;
      this.state[3] = 271733878U;
      Array.Clear((Array) this.buffer, 0, 64);
      Array.Clear((Array) this.x, 0, 16);
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
      int dstOffset = (int) (this.count[0] >> 3) & 63;
      this.count[0] += (uint) (cbSize << 3);
      if ((long) this.count[0] < (long) (cbSize << 3))
        ++this.count[1];
      this.count[1] += (uint) (cbSize >> 29);
      int count = 64 - dstOffset;
      int num = 0;
      if (cbSize >= count)
      {
        Buffer.BlockCopy((Array) array, ibStart, (Array) this.buffer, dstOffset, count);
        this.MD4Transform(this.state, this.buffer, 0);
        for (num = count; num + 63 < cbSize; num += 64)
          this.MD4Transform(this.state, array, ibStart + num);
        dstOffset = 0;
      }
      Buffer.BlockCopy((Array) array, ibStart + num, (Array) this.buffer, dstOffset, cbSize - num);
    }

    protected override byte[] HashFinal()
    {
      byte[] numArray = new byte[8];
      this.Encode(numArray, this.count);
      uint num1 = this.count[0] >> 3 & 63U;
      int num2 = num1 >= 56U ? 120 - (int) num1 : 56 - (int) num1;
      this.HashCore(this.Padding(num2), 0, num2);
      this.HashCore(numArray, 0, 8);
      this.Encode(this.digest, this.state);
      this.Initialize();
      return this.digest;
    }

    private byte[] Padding(int nLength)
    {
      if (nLength <= 0)
        return (byte[]) null;
      byte[] numArray = new byte[nLength];
      numArray[0] = (byte) 128;
      return numArray;
    }

    private uint F(uint x, uint y, uint z)
    {
      return (uint) ((int) x & (int) y | ~(int) x & (int) z);
    }

    private uint G(uint x, uint y, uint z)
    {
      return (uint) ((int) x & (int) y | (int) x & (int) z | (int) y & (int) z);
    }

    private uint H(uint x, uint y, uint z)
    {
      return x ^ y ^ z;
    }

    private uint ROL(uint x, byte n)
    {
      return x << (int) n | x >> 32 - (int) n;
    }

    private void FF(ref uint a, uint b, uint c, uint d, uint x, byte s)
    {
      a += this.F(b, c, d) + x;
      a = this.ROL(a, s);
    }

    private void GG(ref uint a, uint b, uint c, uint d, uint x, byte s)
    {
      a += (uint) ((int) this.G(b, c, d) + (int) x + 1518500249);
      a = this.ROL(a, s);
    }

    private void HH(ref uint a, uint b, uint c, uint d, uint x, byte s)
    {
      a += (uint) ((int) this.H(b, c, d) + (int) x + 1859775393);
      a = this.ROL(a, s);
    }

    private void Encode(byte[] output, uint[] input)
    {
      int index1 = 0;
      for (int index2 = 0; index2 < output.Length; index2 += 4)
      {
        output[index2] = (byte) input[index1];
        output[index2 + 1] = (byte) (input[index1] >> 8);
        output[index2 + 2] = (byte) (input[index1] >> 16);
        output[index2 + 3] = (byte) (input[index1] >> 24);
        ++index1;
      }
    }

    private void Decode(uint[] output, byte[] input, int index)
    {
      int index1 = 0;
      int index2 = index;
      while (index1 < output.Length)
      {
        output[index1] = (uint) ((int) input[index2] | (int) input[index2 + 1] << 8 | (int) input[index2 + 2] << 16 | (int) input[index2 + 3] << 24);
        ++index1;
        index2 += 4;
      }
    }

    private void MD4Transform(uint[] state, byte[] block, int index)
    {
      uint a1 = state[0];
      uint a2 = state[1];
      uint a3 = state[2];
      uint a4 = state[3];
      this.Decode(this.x, block, index);
      this.FF(ref a1, a2, a3, a4, this.x[0], (byte) 3);
      this.FF(ref a4, a1, a2, a3, this.x[1], (byte) 7);
      this.FF(ref a3, a4, a1, a2, this.x[2], (byte) 11);
      this.FF(ref a2, a3, a4, a1, this.x[3], (byte) 19);
      this.FF(ref a1, a2, a3, a4, this.x[4], (byte) 3);
      this.FF(ref a4, a1, a2, a3, this.x[5], (byte) 7);
      this.FF(ref a3, a4, a1, a2, this.x[6], (byte) 11);
      this.FF(ref a2, a3, a4, a1, this.x[7], (byte) 19);
      this.FF(ref a1, a2, a3, a4, this.x[8], (byte) 3);
      this.FF(ref a4, a1, a2, a3, this.x[9], (byte) 7);
      this.FF(ref a3, a4, a1, a2, this.x[10], (byte) 11);
      this.FF(ref a2, a3, a4, a1, this.x[11], (byte) 19);
      this.FF(ref a1, a2, a3, a4, this.x[12], (byte) 3);
      this.FF(ref a4, a1, a2, a3, this.x[13], (byte) 7);
      this.FF(ref a3, a4, a1, a2, this.x[14], (byte) 11);
      this.FF(ref a2, a3, a4, a1, this.x[15], (byte) 19);
      this.GG(ref a1, a2, a3, a4, this.x[0], (byte) 3);
      this.GG(ref a4, a1, a2, a3, this.x[4], (byte) 5);
      this.GG(ref a3, a4, a1, a2, this.x[8], (byte) 9);
      this.GG(ref a2, a3, a4, a1, this.x[12], (byte) 13);
      this.GG(ref a1, a2, a3, a4, this.x[1], (byte) 3);
      this.GG(ref a4, a1, a2, a3, this.x[5], (byte) 5);
      this.GG(ref a3, a4, a1, a2, this.x[9], (byte) 9);
      this.GG(ref a2, a3, a4, a1, this.x[13], (byte) 13);
      this.GG(ref a1, a2, a3, a4, this.x[2], (byte) 3);
      this.GG(ref a4, a1, a2, a3, this.x[6], (byte) 5);
      this.GG(ref a3, a4, a1, a2, this.x[10], (byte) 9);
      this.GG(ref a2, a3, a4, a1, this.x[14], (byte) 13);
      this.GG(ref a1, a2, a3, a4, this.x[3], (byte) 3);
      this.GG(ref a4, a1, a2, a3, this.x[7], (byte) 5);
      this.GG(ref a3, a4, a1, a2, this.x[11], (byte) 9);
      this.GG(ref a2, a3, a4, a1, this.x[15], (byte) 13);
      this.HH(ref a1, a2, a3, a4, this.x[0], (byte) 3);
      this.HH(ref a4, a1, a2, a3, this.x[8], (byte) 9);
      this.HH(ref a3, a4, a1, a2, this.x[4], (byte) 11);
      this.HH(ref a2, a3, a4, a1, this.x[12], (byte) 15);
      this.HH(ref a1, a2, a3, a4, this.x[2], (byte) 3);
      this.HH(ref a4, a1, a2, a3, this.x[10], (byte) 9);
      this.HH(ref a3, a4, a1, a2, this.x[6], (byte) 11);
      this.HH(ref a2, a3, a4, a1, this.x[14], (byte) 15);
      this.HH(ref a1, a2, a3, a4, this.x[1], (byte) 3);
      this.HH(ref a4, a1, a2, a3, this.x[9], (byte) 9);
      this.HH(ref a3, a4, a1, a2, this.x[5], (byte) 11);
      this.HH(ref a2, a3, a4, a1, this.x[13], (byte) 15);
      this.HH(ref a1, a2, a3, a4, this.x[3], (byte) 3);
      this.HH(ref a4, a1, a2, a3, this.x[11], (byte) 9);
      this.HH(ref a3, a4, a1, a2, this.x[7], (byte) 11);
      this.HH(ref a2, a3, a4, a1, this.x[15], (byte) 15);
      state[0] += a1;
      state[1] += a2;
      state[2] += a3;
      state[3] += a4;
    }
  }
}
