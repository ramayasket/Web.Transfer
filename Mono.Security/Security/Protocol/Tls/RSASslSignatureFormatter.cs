// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.RSASslSignatureFormatter
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls
{
  using Mono.Security.Cryptography;
  using System;
  using System.Collections.Generic;
  using System.Runtime.CompilerServices;
  using System.Security.Cryptography;

  internal class RSASslSignatureFormatter : AsymmetricSignatureFormatter
  {
    private RSA key;
    private HashAlgorithm hash;
    [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_map16;

        public RSASslSignatureFormatter()
    {
    }

    public RSASslSignatureFormatter(AsymmetricAlgorithm key)
    {
      this.SetKey(key);
    }

    public override byte[] CreateSignature(byte[] rgbHash)
    {
      if (this.key == null)
      {
        throw new CryptographicUnexpectedOperationException("The key is a null reference");
      }
      if (this.hash == null)
      {
        throw new CryptographicUnexpectedOperationException("The hash algorithm is a null reference.");
      }
      if (rgbHash == null)
      {
        throw new ArgumentNullException("The rgbHash parameter is a null reference.");
      }
      return PKCS1.Sign_v15(this.key, this.hash, rgbHash);
    }

    public override void SetHashAlgorithm(string strName)
    {
      string key = strName;
      if (key != null)
      {
        if (__f__switch_map16 == null)
                {
          Dictionary<string, int> dictionary = new Dictionary<string, int>(1) {
                        {
                            "MD5SHA1",
                            0
                        }
                    };
                    __f__switch_map16 = dictionary;
        }
        if (__f__switch_map16.TryGetValue(key, out int num) && (num == 0))
                {
          this.hash = new MD5SHA1();
          return;
        }
      }
      this.hash = HashAlgorithm.Create(strName);
    }

    public override void SetKey(AsymmetricAlgorithm key)
    {
      if (!(key is RSA))
      {
        throw new ArgumentException("Specfied key is not an RSA key");
      }
      this.key = key as RSA;
    }
  }
}
