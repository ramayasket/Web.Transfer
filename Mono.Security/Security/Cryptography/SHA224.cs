// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.SHA224
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System.Security.Cryptography;

namespace Mono.Security.Cryptography
{
  public abstract class SHA224 : HashAlgorithm
  {
    public SHA224()
    {
      this.HashSizeValue = 224;
    }

    public new static SHA224 Create()
    {
      return SHA224.Create(nameof (SHA224));
    }

    public new static SHA224 Create(string hashName)
    {
      return (SHA224) (CryptoConfig.CreateFromName(hashName) ?? (object) new SHA224Managed());
    }
  }
}
