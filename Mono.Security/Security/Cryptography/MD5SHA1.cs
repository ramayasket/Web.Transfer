// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.MD5SHA1
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Protocol.Tls;
using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography
{
  internal class MD5SHA1 : HashAlgorithm
  {
    private HashAlgorithm md5;
    private HashAlgorithm sha;
    private bool hashing;

    public MD5SHA1()
    {
      this.md5 = (HashAlgorithm) MD5.Create();
      this.sha = (HashAlgorithm) SHA1.Create();
      this.HashSizeValue = this.md5.HashSize + this.sha.HashSize;
    }

    public override void Initialize()
    {
      this.md5.Initialize();
      this.sha.Initialize();
      this.hashing = false;
    }

    protected override byte[] HashFinal()
    {
      if (!this.hashing)
        this.hashing = true;
      this.md5.TransformFinalBlock(new byte[0], 0, 0);
      this.sha.TransformFinalBlock(new byte[0], 0, 0);
      byte[] numArray = new byte[36];
      Buffer.BlockCopy((Array) this.md5.Hash, 0, (Array) numArray, 0, 16);
      Buffer.BlockCopy((Array) this.sha.Hash, 0, (Array) numArray, 16, 20);
      return numArray;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
      if (!this.hashing)
        this.hashing = true;
      this.md5.TransformBlock(array, ibStart, cbSize, array, ibStart);
      this.sha.TransformBlock(array, ibStart, cbSize, array, ibStart);
    }

    public byte[] CreateSignature(RSA rsa)
    {
      if (rsa == null)
        throw new CryptographicUnexpectedOperationException("missing key");
      RSASslSignatureFormatter signatureFormatter = new RSASslSignatureFormatter((AsymmetricAlgorithm) rsa);
      signatureFormatter.SetHashAlgorithm(nameof (MD5SHA1));
      return signatureFormatter.CreateSignature(this.Hash);
    }

    public bool VerifySignature(RSA rsa, byte[] rgbSignature)
    {
      if (rsa == null)
        throw new CryptographicUnexpectedOperationException("missing key");
      if (rgbSignature == null)
        throw new ArgumentNullException(nameof (rgbSignature));
      RSASslSignatureDeformatter signatureDeformatter = new RSASslSignatureDeformatter((AsymmetricAlgorithm) rsa);
      signatureDeformatter.SetHashAlgorithm(nameof (MD5SHA1));
      return signatureDeformatter.VerifySignature(this.Hash, rgbSignature);
    }
  }
}
