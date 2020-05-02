// Decompiled with JetBrains decompiler
// Type: ChrisLib.SimpleHash
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.Security.Cryptography;
using System.Text;

namespace ChrisLib
{
  public class SimpleHash
  {
    public static void unit_test()
    {
      clib.imsg("unit_test for simplehash {0} ", (object) SimpleHash.test_hash("test", "{ssha}ljMJ0hTP46n4jmbmep6J2PpBkY3GcFiZ"));
    }

    public static string create_hash(string password)
    {
      return string.Format("{{ssha}}{0}", (object) SimpleHash.ComputeHash(password, (byte[]) null));
    }

    public static bool test_hash(string password, string hash)
    {
      return hash.StartsWith("{ssha}") && SimpleHash.VerifyHash(password, hash.Substring(6));
    }

    public static string ComputeHash(string plainText, byte[] saltBytes)
    {
      if (saltBytes == null)
      {
        saltBytes = new byte[new Random().Next(4, 8)];
        new RNGCryptoServiceProvider().GetNonZeroBytes(saltBytes);
      }
      byte[] bytes = Encoding.UTF8.GetBytes(plainText);
      byte[] buffer = new byte[bytes.Length + saltBytes.Length];
      for (int index = 0; index < bytes.Length; ++index)
        buffer[index] = bytes[index];
      for (int index = 0; index < saltBytes.Length; ++index)
        buffer[bytes.Length + index] = saltBytes[index];
      byte[] hash = new SHA1Managed().ComputeHash(buffer);
      byte[] inArray = new byte[hash.Length + saltBytes.Length];
      for (int index = 0; index < hash.Length; ++index)
        inArray[index] = hash[index];
      for (int index = 0; index < saltBytes.Length; ++index)
        inArray[hash.Length + index] = saltBytes[index];
      return Convert.ToBase64String(inArray);
    }

    public static bool VerifyHash(string plainText, string hashValue)
    {
      byte[] numArray = Convert.FromBase64String(hashValue);
      HashAlgorithm hashAlgorithm = (HashAlgorithm) new SHA1Managed();
      int num = 160 / 8;
      if (numArray.Length < num)
        return false;
      byte[] saltBytes = new byte[numArray.Length - num];
      for (int index = 0; index < saltBytes.Length; ++index)
        saltBytes[index] = numArray[num + index];
      string hash = SimpleHash.ComputeHash(plainText, saltBytes);
      return hashValue == hash;
    }
  }
}
