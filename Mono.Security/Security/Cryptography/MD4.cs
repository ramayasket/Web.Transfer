// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.MD4
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System.Security.Cryptography;

namespace Mono.Security.Cryptography
{
  public abstract class MD4 : HashAlgorithm
  {
    protected MD4()
    {
      this.HashSizeValue = 128;
    }

    public new static MD4 Create()
    {
      return MD4.Create(nameof (MD4));
    }

    public new static MD4 Create(string hashName)
    {
      return (MD4) (CryptoConfig.CreateFromName(hashName) ?? (object) new MD4Managed());
    }
  }
}
