// Decompiled with JetBrains decompiler
// Type: Mono.Security.BitConverterLE
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security
{
  internal sealed class BitConverterLE
  {
    private BitConverterLE()
    {
    }

    private static unsafe byte[] GetUShortBytes(byte* bytes)
    {
      return BitConverter.IsLittleEndian ? new byte[2]
      {
        *bytes,
        bytes[1]
      } : new byte[2]{ bytes[1], *bytes };
    }

    private static unsafe byte[] GetUIntBytes(byte* bytes)
    {
      return BitConverter.IsLittleEndian ? new byte[4]
      {
        *bytes,
        bytes[1],
        bytes[2],
        bytes[3]
      } : new byte[4]
      {
        bytes[3],
        bytes[2],
        bytes[1],
        *bytes
      };
    }

    private static unsafe byte[] GetULongBytes(byte* bytes)
    {
      return BitConverter.IsLittleEndian ? new byte[8]
      {
        *bytes,
        bytes[1],
        bytes[2],
        bytes[3],
        bytes[4],
        bytes[5],
        bytes[6],
        bytes[7]
      } : new byte[8]
      {
        bytes[7],
        bytes[6],
        bytes[5],
        bytes[4],
        bytes[3],
        bytes[2],
        bytes[1],
        *bytes
      };
    }

    internal static byte[] GetBytes(bool value)
    {
      return new byte[1]{ !value ? (byte) 0 : (byte) 1 };
    }

    internal static unsafe byte[] GetBytes(char value)
    {
      return BitConverterLE.GetUShortBytes((byte*) &value);
    }

    internal static unsafe byte[] GetBytes(short value)
    {
      return BitConverterLE.GetUShortBytes((byte*) &value);
    }

    internal static unsafe byte[] GetBytes(int value)
    {
      return BitConverterLE.GetUIntBytes((byte*) &value);
    }

    internal static unsafe byte[] GetBytes(long value)
    {
      return BitConverterLE.GetULongBytes((byte*) &value);
    }

    internal static unsafe byte[] GetBytes(ushort value)
    {
      return BitConverterLE.GetUShortBytes((byte*) &value);
    }

    internal static unsafe byte[] GetBytes(uint value)
    {
      return BitConverterLE.GetUIntBytes((byte*) &value);
    }

    internal static unsafe byte[] GetBytes(ulong value)
    {
      return BitConverterLE.GetULongBytes((byte*) &value);
    }

    internal static unsafe byte[] GetBytes(float value)
    {
      return BitConverterLE.GetUIntBytes((byte*) &value);
    }

    internal static unsafe byte[] GetBytes(double value)
    {
      return BitConverterLE.GetULongBytes((byte*) &value);
    }

    private static unsafe void UShortFromBytes(byte* dst, byte[] src, int startIndex)
    {
      if (BitConverter.IsLittleEndian)
      {
        *dst = src[startIndex];
        dst[1] = src[startIndex + 1];
      }
      else
      {
        *dst = src[startIndex + 1];
        dst[1] = src[startIndex];
      }
    }

    private static unsafe void UIntFromBytes(byte* dst, byte[] src, int startIndex)
    {
      if (BitConverter.IsLittleEndian)
      {
        *dst = src[startIndex];
        dst[1] = src[startIndex + 1];
        dst[2] = src[startIndex + 2];
        dst[3] = src[startIndex + 3];
      }
      else
      {
        *dst = src[startIndex + 3];
        dst[1] = src[startIndex + 2];
        dst[2] = src[startIndex + 1];
        dst[3] = src[startIndex];
      }
    }

    private static unsafe void ULongFromBytes(byte* dst, byte[] src, int startIndex)
    {
      if (BitConverter.IsLittleEndian)
      {
        for (int index = 0; index < 8; ++index)
          dst[index] = src[startIndex + index];
      }
      else
      {
        for (int index = 0; index < 8; ++index)
          dst[index] = src[startIndex + (7 - index)];
      }
    }

    internal static bool ToBoolean(byte[] value, int startIndex)
    {
      return value[startIndex] != (byte) 0;
    }

    internal static unsafe char ToChar(byte[] value, int startIndex)
    {
      char ch;
      BitConverterLE.UShortFromBytes((byte*) &ch, value, startIndex);
      return ch;
    }

    internal static unsafe short ToInt16(byte[] value, int startIndex)
    {
      short num;
      BitConverterLE.UShortFromBytes((byte*) &num, value, startIndex);
      return num;
    }

    internal static unsafe int ToInt32(byte[] value, int startIndex)
    {
      int num;
      BitConverterLE.UIntFromBytes((byte*) &num, value, startIndex);
      return num;
    }

    internal static unsafe long ToInt64(byte[] value, int startIndex)
    {
      long num;
      BitConverterLE.ULongFromBytes((byte*) &num, value, startIndex);
      return num;
    }

    internal static unsafe ushort ToUInt16(byte[] value, int startIndex)
    {
      ushort num;
      BitConverterLE.UShortFromBytes((byte*) &num, value, startIndex);
      return num;
    }

    internal static unsafe uint ToUInt32(byte[] value, int startIndex)
    {
      uint num;
      BitConverterLE.UIntFromBytes((byte*) &num, value, startIndex);
      return num;
    }

    internal static unsafe ulong ToUInt64(byte[] value, int startIndex)
    {
      ulong num;
      BitConverterLE.ULongFromBytes((byte*) &num, value, startIndex);
      return num;
    }

    internal static unsafe float ToSingle(byte[] value, int startIndex)
    {
      float num;
      BitConverterLE.UIntFromBytes((byte*) &num, value, startIndex);
      return num;
    }

    internal static unsafe double ToDouble(byte[] value, int startIndex)
    {
      double num;
      BitConverterLE.ULongFromBytes((byte*) &num, value, startIndex);
      return num;
    }
  }
}
