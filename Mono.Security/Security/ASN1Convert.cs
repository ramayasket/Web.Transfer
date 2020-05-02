// Decompiled with JetBrains decompiler
// Type: Mono.Security.ASN1Convert
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security
{
  public static class ASN1Convert
  {
    public static ASN1 FromDateTime(DateTime dt)
    {
      return dt.Year < 2050 ? new ASN1((byte) 23, Encoding.ASCII.GetBytes(dt.ToUniversalTime().ToString("yyMMddHHmmss", (IFormatProvider) CultureInfo.InvariantCulture) + "Z")) : new ASN1((byte) 24, Encoding.ASCII.GetBytes(dt.ToUniversalTime().ToString("yyyyMMddHHmmss", (IFormatProvider) CultureInfo.InvariantCulture) + "Z"));
    }

    public static ASN1 FromInt32(int value)
    {
      byte[] bytes = BitConverterLE.GetBytes(value);
      Array.Reverse((Array) bytes);
      int srcOffset = 0;
      while (srcOffset < bytes.Length && bytes[srcOffset] == (byte) 0)
        ++srcOffset;
      ASN1 asN1 = new ASN1((byte) 2);
      switch (srcOffset)
      {
        case 0:
          asN1.Value = bytes;
          break;
        case 4:
          asN1.Value = new byte[1];
          break;
        default:
          byte[] numArray = new byte[4 - srcOffset];
          Buffer.BlockCopy((Array) bytes, srcOffset, (Array) numArray, 0, numArray.Length);
          asN1.Value = numArray;
          break;
      }
      return asN1;
    }

    public static ASN1 FromOid(string oid)
    {
      if (oid == null)
        throw new ArgumentNullException(nameof (oid));
      return new ASN1(CryptoConfig.EncodeOID(oid));
    }

    public static ASN1 FromUnsignedBigInteger(byte[] big)
    {
      if (big == null)
        throw new ArgumentNullException(nameof (big));
      if (big[0] >= (byte) 128)
      {
        int length = big.Length + 1;
        byte[] numArray = new byte[length];
        Buffer.BlockCopy((Array) big, 0, (Array) numArray, 1, length - 1);
        big = numArray;
      }
      return new ASN1((byte) 2, big);
    }

    public static int ToInt32(ASN1 asn1)
    {
      if (asn1 == null)
        throw new ArgumentNullException(nameof (asn1));
      if (asn1.Tag != (byte) 2)
        throw new FormatException("Only integer can be converted");
      int num = 0;
      for (int index = 0; index < asn1.Value.Length; ++index)
        num = (num << 8) + (int) asn1.Value[index];
      return num;
    }

    public static string ToOid(ASN1 asn1)
    {
      if (asn1 == null)
        throw new ArgumentNullException(nameof (asn1));
      byte[] numArray = asn1.Value;
      StringBuilder stringBuilder = new StringBuilder();
      byte num1 = (byte) ((uint) numArray[0] / 40U);
      byte num2 = (byte) ((uint) numArray[0] % 40U);
      if (num1 > (byte) 2)
      {
        num2 += (byte) (((int) num1 - 2) * 40);
        num1 = (byte) 2;
      }
      stringBuilder.Append(num1.ToString((IFormatProvider) CultureInfo.InvariantCulture));
      stringBuilder.Append(".");
      stringBuilder.Append(num2.ToString((IFormatProvider) CultureInfo.InvariantCulture));
      ulong num3 = 0;
      for (num1 = (byte) 1; (int) num1 < numArray.Length; ++num1)
      {
        num3 = num3 << 7 | (ulong) (byte) ((uint) numArray[(int) num1] & (uint) sbyte.MaxValue);
        if (((int) numArray[(int) num1] & 128) != 128)
        {
          stringBuilder.Append(".");
          stringBuilder.Append(num3.ToString((IFormatProvider) CultureInfo.InvariantCulture));
          num3 = 0UL;
        }
      }
      return stringBuilder.ToString();
    }

    public static DateTime ToDateTime(ASN1 time)
    {
      if (time == null)
        throw new ArgumentNullException(nameof (time));
      string s = Encoding.ASCII.GetString(time.Value);
      string format = (string) null;
      switch (s.Length)
      {
        case 11:
          format = "yyMMddHHmmZ";
          break;
        case 13:
          s = Convert.ToInt16(s.Substring(0, 2), (IFormatProvider) CultureInfo.InvariantCulture) < (short) 50 ? "20" + s : "19" + s;
          format = "yyyyMMddHHmmssZ";
          break;
        case 15:
          format = "yyyyMMddHHmmssZ";
          break;
        case 17:
          string str = Convert.ToInt16(s.Substring(0, 2), (IFormatProvider) CultureInfo.InvariantCulture) < (short) 50 ? "20" : "19";
          char ch = s[12] != '+' ? '+' : '-';
          s = string.Format("{0}{1}{2}{3}{4}:{5}{6}", (object) str, (object) s.Substring(0, 12), (object) ch, (object) s[13], (object) s[14], (object) s[15], (object) s[16]);
          format = "yyyyMMddHHmmsszzz";
          break;
      }
      return DateTime.ParseExact(s, format, (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
    }
  }
}
