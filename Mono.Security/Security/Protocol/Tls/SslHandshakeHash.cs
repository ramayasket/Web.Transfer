// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.SslHandshakeHash
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls
{
  internal class SslHandshakeHash : HashAlgorithm
  {
    private HashAlgorithm md5;
    private HashAlgorithm sha;
    private bool hashing;
    private byte[] secret;
    private byte[] innerPadMD5;
    private byte[] outerPadMD5;
    private byte[] innerPadSHA;
    private byte[] outerPadSHA;

    public SslHandshakeHash(byte[] secret)
    {
      this.md5 = HashAlgorithm.Create("MD5");
      this.sha = HashAlgorithm.Create("SHA1");
      this.HashSizeValue = this.md5.HashSize + this.sha.HashSize;
      this.secret = secret;
      this.Initialize();
    }

    public override void Initialize()
    {
      this.md5.Initialize();
      this.sha.Initialize();
      this.initializePad();
      this.hashing = false;
    }

    protected override byte[] HashFinal()
    {
      if (!this.hashing)
        this.hashing = true;
      this.md5.TransformBlock(this.secret, 0, this.secret.Length, this.secret, 0);
      this.md5.TransformFinalBlock(this.innerPadMD5, 0, this.innerPadMD5.Length);
      byte[] hash1 = this.md5.Hash;
      this.md5.Initialize();
      this.md5.TransformBlock(this.secret, 0, this.secret.Length, this.secret, 0);
      this.md5.TransformBlock(this.outerPadMD5, 0, this.outerPadMD5.Length, this.outerPadMD5, 0);
      this.md5.TransformFinalBlock(hash1, 0, hash1.Length);
      this.sha.TransformBlock(this.secret, 0, this.secret.Length, this.secret, 0);
      this.sha.TransformFinalBlock(this.innerPadSHA, 0, this.innerPadSHA.Length);
      byte[] hash2 = this.sha.Hash;
      this.sha.Initialize();
      this.sha.TransformBlock(this.secret, 0, this.secret.Length, this.secret, 0);
      this.sha.TransformBlock(this.outerPadSHA, 0, this.outerPadSHA.Length, this.outerPadSHA, 0);
      this.sha.TransformFinalBlock(hash2, 0, hash2.Length);
      this.Initialize();
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
      signatureFormatter.SetHashAlgorithm("MD5SHA1");
      return signatureFormatter.CreateSignature(this.Hash);
    }

    public bool VerifySignature(RSA rsa, byte[] rgbSignature)
    {
      if (rsa == null)
        throw new CryptographicUnexpectedOperationException("missing key");
      if (rgbSignature == null)
        throw new ArgumentNullException(nameof (rgbSignature));
      RSASslSignatureDeformatter signatureDeformatter = new RSASslSignatureDeformatter((AsymmetricAlgorithm) rsa);
      signatureDeformatter.SetHashAlgorithm("MD5SHA1");
      return signatureDeformatter.VerifySignature(this.Hash, rgbSignature);
    }

    private void initializePad()
    {
      this.innerPadMD5 = new byte[48];
      this.outerPadMD5 = new byte[48];
      for (int index = 0; index < 48; ++index)
      {
        this.innerPadMD5[index] = (byte) 54;
        this.outerPadMD5[index] = (byte) 92;
      }
      this.innerPadSHA = new byte[40];
      this.outerPadSHA = new byte[40];
      for (int index = 0; index < 40; ++index)
      {
        this.innerPadSHA[index] = (byte) 54;
        this.outerPadSHA[index] = (byte) 92;
      }
    }
  }
}
