// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.MD2Managed
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security.Cryptography
{
  public class MD2Managed : MD2
  {
    private static readonly byte[] PI_SUBST = new byte[256]
    {
      (byte) 41,
      (byte) 46,
      (byte) 67,
      (byte) 201,
      (byte) 162,
      (byte) 216,
      (byte) 124,
      (byte) 1,
      (byte) 61,
      (byte) 54,
      (byte) 84,
      (byte) 161,
      (byte) 236,
      (byte) 240,
      (byte) 6,
      (byte) 19,
      (byte) 98,
      (byte) 167,
      (byte) 5,
      (byte) 243,
      (byte) 192,
      (byte) 199,
      (byte) 115,
      (byte) 140,
      (byte) 152,
      (byte) 147,
      (byte) 43,
      (byte) 217,
      (byte) 188,
      (byte) 76,
      (byte) 130,
      (byte) 202,
      (byte) 30,
      (byte) 155,
      (byte) 87,
      (byte) 60,
      (byte) 253,
      (byte) 212,
      (byte) 224,
      (byte) 22,
      (byte) 103,
      (byte) 66,
      (byte) 111,
      (byte) 24,
      (byte) 138,
      (byte) 23,
      (byte) 229,
      (byte) 18,
      (byte) 190,
      (byte) 78,
      (byte) 196,
      (byte) 214,
      (byte) 218,
      (byte) 158,
      (byte) 222,
      (byte) 73,
      (byte) 160,
      (byte) 251,
      (byte) 245,
      (byte) 142,
      (byte) 187,
      (byte) 47,
      (byte) 238,
      (byte) 122,
      (byte) 169,
      (byte) 104,
      (byte) 121,
      (byte) 145,
      (byte) 21,
      (byte) 178,
      (byte) 7,
      (byte) 63,
      (byte) 148,
      (byte) 194,
      (byte) 16,
      (byte) 137,
      (byte) 11,
      (byte) 34,
      (byte) 95,
      (byte) 33,
      (byte) 128,
      (byte) 127,
      (byte) 93,
      (byte) 154,
      (byte) 90,
      (byte) 144,
      (byte) 50,
      (byte) 39,
      (byte) 53,
      (byte) 62,
      (byte) 204,
      (byte) 231,
      (byte) 191,
      (byte) 247,
      (byte) 151,
      (byte) 3,
      byte.MaxValue,
      (byte) 25,
      (byte) 48,
      (byte) 179,
      (byte) 72,
      (byte) 165,
      (byte) 181,
      (byte) 209,
      (byte) 215,
      (byte) 94,
      (byte) 146,
      (byte) 42,
      (byte) 172,
      (byte) 86,
      (byte) 170,
      (byte) 198,
      (byte) 79,
      (byte) 184,
      (byte) 56,
      (byte) 210,
      (byte) 150,
      (byte) 164,
      (byte) 125,
      (byte) 182,
      (byte) 118,
      (byte) 252,
      (byte) 107,
      (byte) 226,
      (byte) 156,
      (byte) 116,
      (byte) 4,
      (byte) 241,
      (byte) 69,
      (byte) 157,
      (byte) 112,
      (byte) 89,
      (byte) 100,
      (byte) 113,
      (byte) 135,
      (byte) 32,
      (byte) 134,
      (byte) 91,
      (byte) 207,
      (byte) 101,
      (byte) 230,
      (byte) 45,
      (byte) 168,
      (byte) 2,
      (byte) 27,
      (byte) 96,
      (byte) 37,
      (byte) 173,
      (byte) 174,
      (byte) 176,
      (byte) 185,
      (byte) 246,
      (byte) 28,
      (byte) 70,
      (byte) 97,
      (byte) 105,
      (byte) 52,
      (byte) 64,
      (byte) 126,
      (byte) 15,
      (byte) 85,
      (byte) 71,
      (byte) 163,
      (byte) 35,
      (byte) 221,
      (byte) 81,
      (byte) 175,
      (byte) 58,
      (byte) 195,
      (byte) 92,
      (byte) 249,
      (byte) 206,
      (byte) 186,
      (byte) 197,
      (byte) 234,
      (byte) 38,
      (byte) 44,
      (byte) 83,
      (byte) 13,
      (byte) 110,
      (byte) 133,
      (byte) 40,
      (byte) 132,
      (byte) 9,
      (byte) 211,
      (byte) 223,
      (byte) 205,
      (byte) 244,
      (byte) 65,
      (byte) 129,
      (byte) 77,
      (byte) 82,
      (byte) 106,
      (byte) 220,
      (byte) 55,
      (byte) 200,
      (byte) 108,
      (byte) 193,
      (byte) 171,
      (byte) 250,
      (byte) 36,
      (byte) 225,
      (byte) 123,
      (byte) 8,
      (byte) 12,
      (byte) 189,
      (byte) 177,
      (byte) 74,
      (byte) 120,
      (byte) 136,
      (byte) 149,
      (byte) 139,
      (byte) 227,
      (byte) 99,
      (byte) 232,
      (byte) 109,
      (byte) 233,
      (byte) 203,
      (byte) 213,
      (byte) 254,
      (byte) 59,
      (byte) 0,
      (byte) 29,
      (byte) 57,
      (byte) 242,
      (byte) 239,
      (byte) 183,
      (byte) 14,
      (byte) 102,
      (byte) 88,
      (byte) 208,
      (byte) 228,
      (byte) 166,
      (byte) 119,
      (byte) 114,
      (byte) 248,
      (byte) 235,
      (byte) 117,
      (byte) 75,
      (byte) 10,
      (byte) 49,
      (byte) 68,
      (byte) 80,
      (byte) 180,
      (byte) 143,
      (byte) 237,
      (byte) 31,
      (byte) 26,
      (byte) 219,
      (byte) 153,
      (byte) 141,
      (byte) 51,
      (byte) 159,
      (byte) 17,
      (byte) 131,
      (byte) 20
    };
    private byte[] state;
    private byte[] checksum;
    private byte[] buffer;
    private int count;
    private byte[] x;

    public MD2Managed()
    {
      this.state = new byte[16];
      this.checksum = new byte[16];
      this.buffer = new byte[16];
      this.x = new byte[48];
      this.Initialize();
    }

    private byte[] Padding(int nLength)
    {
      if (nLength <= 0)
        return (byte[]) null;
      byte[] numArray = new byte[nLength];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = (byte) nLength;
      return numArray;
    }

    public override void Initialize()
    {
      this.count = 0;
      Array.Clear((Array) this.state, 0, 16);
      Array.Clear((Array) this.checksum, 0, 16);
      Array.Clear((Array) this.buffer, 0, 16);
      Array.Clear((Array) this.x, 0, 48);
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
      int dstOffset = this.count;
      this.count = dstOffset + cbSize & 15;
      int count = 16 - dstOffset;
      int num;
      if (cbSize >= count)
      {
        Buffer.BlockCopy((Array) array, ibStart, (Array) this.buffer, dstOffset, count);
        this.MD2Transform(this.state, this.checksum, this.buffer, 0);
        for (num = count; num + 15 < cbSize; num += 16)
          this.MD2Transform(this.state, this.checksum, array, ibStart + num);
        dstOffset = 0;
      }
      else
        num = 0;
      Buffer.BlockCopy((Array) array, ibStart + num, (Array) this.buffer, dstOffset, cbSize - num);
    }

    protected override byte[] HashFinal()
    {
      int num = 16 - this.count;
      if (num > 0)
        this.HashCore(this.Padding(num), 0, num);
      this.HashCore(this.checksum, 0, 16);
      byte[] numArray = (byte[]) this.state.Clone();
      this.Initialize();
      return numArray;
    }

    private void MD2Transform(byte[] state, byte[] checksum, byte[] block, int index)
    {
      Buffer.BlockCopy((Array) state, 0, (Array) this.x, 0, 16);
      Buffer.BlockCopy((Array) block, index, (Array) this.x, 16, 16);
      for (int index1 = 0; index1 < 16; ++index1)
        this.x[index1 + 32] = (byte) ((uint) state[index1] ^ (uint) block[index + index1]);
      int index2 = 0;
      for (int index1 = 0; index1 < 18; ++index1)
      {
        for (int index3 = 0; index3 < 48; ++index3)
          index2 = (int) (this.x[index3] ^= MD2Managed.PI_SUBST[index2]);
        index2 = index2 + index1 & (int) byte.MaxValue;
      }
      Buffer.BlockCopy((Array) this.x, 0, (Array) state, 0, 16);
      int num = (int) checksum[15];
      for (int index1 = 0; index1 < 16; ++index1)
        num = (int) (checksum[index1] ^= MD2Managed.PI_SUBST[(int) block[index + index1] ^ num]);
    }
  }
}
