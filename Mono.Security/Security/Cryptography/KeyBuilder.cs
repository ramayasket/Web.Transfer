// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.KeyBuilder
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System.Security.Cryptography;

namespace Mono.Security.Cryptography
{
  public sealed class KeyBuilder
  {
    private static RandomNumberGenerator rng;

    private KeyBuilder()
    {
    }

    private static RandomNumberGenerator Rng
    {
      get
      {
        if (KeyBuilder.rng == null)
          KeyBuilder.rng = RandomNumberGenerator.Create();
        return KeyBuilder.rng;
      }
    }

    public static byte[] Key(int size)
    {
      byte[] data = new byte[size];
      KeyBuilder.Rng.GetBytes(data);
      return data;
    }

    public static byte[] IV(int size)
    {
      byte[] data = new byte[size];
      KeyBuilder.Rng.GetBytes(data);
      return data;
    }
  }
}
