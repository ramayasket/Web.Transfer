// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.MD2
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System.Security.Cryptography;

namespace Mono.Security.Cryptography
{
  public abstract class MD2 : HashAlgorithm
  {
    protected MD2()
    {
      this.HashSizeValue = 128;
    }

    public new static MD2 Create()
    {
      return MD2.Create(nameof (MD2));
    }

    public new static MD2 Create(string hashName)
    {
      return (MD2) (CryptoConfig.CreateFromName(hashName) ?? (object) new MD2Managed());
    }
  }
}
