// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.RC4
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System.Security.Cryptography;

namespace Mono.Security.Cryptography
{
  public abstract class RC4 : SymmetricAlgorithm
  {
    private static KeySizes[] s_legalBlockSizes = new KeySizes[1]
    {
      new KeySizes(64, 64, 0)
    };
    private static KeySizes[] s_legalKeySizes = new KeySizes[1]
    {
      new KeySizes(40, 2048, 8)
    };

    public RC4()
    {
      this.KeySizeValue = 128;
      this.BlockSizeValue = 64;
      this.FeedbackSizeValue = this.BlockSizeValue;
      this.LegalBlockSizesValue = RC4.s_legalBlockSizes;
      this.LegalKeySizesValue = RC4.s_legalKeySizes;
    }

    public override byte[] IV
    {
      get
      {
        return new byte[0];
      }
      set
      {
      }
    }

    public new static RC4 Create()
    {
      return RC4.Create(nameof (RC4));
    }

    public new static RC4 Create(string algName)
    {
      return (RC4) (CryptoConfig.CreateFromName(algName) ?? (object) new ARC4Managed());
    }
  }
}
