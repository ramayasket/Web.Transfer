// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.SslCipherSuite
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security.Protocol.Tls
{
  internal class SslCipherSuite : CipherSuite
  {
    private const int MacHeaderLength = 11;
    private byte[] pad1;
    private byte[] pad2;
    private byte[] header;

    public SslCipherSuite(
      short code,
      string name,
      CipherAlgorithmType cipherAlgorithmType,
      HashAlgorithmType hashAlgorithmType,
      ExchangeAlgorithmType exchangeAlgorithmType,
      bool exportable,
      bool blockMode,
      byte keyMaterialSize,
      byte expandedKeyMaterialSize,
      short effectiveKeyBytes,
      byte ivSize,
      byte blockSize)
      : base(code, name, cipherAlgorithmType, hashAlgorithmType, exchangeAlgorithmType, exportable, blockMode, keyMaterialSize, expandedKeyMaterialSize, effectiveKeyBytes, ivSize, blockSize)
    {
      int length = hashAlgorithmType != HashAlgorithmType.Md5 ? 40 : 48;
      this.pad1 = new byte[length];
      this.pad2 = new byte[length];
      for (int index = 0; index < length; ++index)
      {
        this.pad1[index] = (byte) 54;
        this.pad2[index] = (byte) 92;
      }
    }

    public override byte[] ComputeServerRecordMAC(ContentType contentType, byte[] fragment)
    {
      HashAlgorithm hashAlgorithm = HashAlgorithm.Create(this.HashAlgorithmName);
      byte[] serverWriteMac = this.Context.Read.ServerWriteMAC;
      hashAlgorithm.TransformBlock(serverWriteMac, 0, serverWriteMac.Length, serverWriteMac, 0);
      hashAlgorithm.TransformBlock(this.pad1, 0, this.pad1.Length, this.pad1, 0);
      if (this.header == null)
        this.header = new byte[11];
      this.Write(this.header, 0, !(this.Context is ClientContext) ? this.Context.WriteSequenceNumber : this.Context.ReadSequenceNumber);
      this.header[8] = (byte) contentType;
      this.Write(this.header, 9, (short) fragment.Length);
      hashAlgorithm.TransformBlock(this.header, 0, this.header.Length, this.header, 0);
      hashAlgorithm.TransformBlock(fragment, 0, fragment.Length, fragment, 0);
      hashAlgorithm.TransformFinalBlock(CipherSuite.EmptyArray, 0, 0);
      byte[] hash = hashAlgorithm.Hash;
      hashAlgorithm.Initialize();
      hashAlgorithm.TransformBlock(serverWriteMac, 0, serverWriteMac.Length, serverWriteMac, 0);
      hashAlgorithm.TransformBlock(this.pad2, 0, this.pad2.Length, this.pad2, 0);
      hashAlgorithm.TransformBlock(hash, 0, hash.Length, hash, 0);
      hashAlgorithm.TransformFinalBlock(CipherSuite.EmptyArray, 0, 0);
      return hashAlgorithm.Hash;
    }

    public override byte[] ComputeClientRecordMAC(ContentType contentType, byte[] fragment)
    {
      HashAlgorithm hashAlgorithm = HashAlgorithm.Create(this.HashAlgorithmName);
      byte[] clientWriteMac = this.Context.Current.ClientWriteMAC;
      hashAlgorithm.TransformBlock(clientWriteMac, 0, clientWriteMac.Length, clientWriteMac, 0);
      hashAlgorithm.TransformBlock(this.pad1, 0, this.pad1.Length, this.pad1, 0);
      if (this.header == null)
        this.header = new byte[11];
      this.Write(this.header, 0, !(this.Context is ClientContext) ? this.Context.ReadSequenceNumber : this.Context.WriteSequenceNumber);
      this.header[8] = (byte) contentType;
      this.Write(this.header, 9, (short) fragment.Length);
      hashAlgorithm.TransformBlock(this.header, 0, this.header.Length, this.header, 0);
      hashAlgorithm.TransformBlock(fragment, 0, fragment.Length, fragment, 0);
      hashAlgorithm.TransformFinalBlock(CipherSuite.EmptyArray, 0, 0);
      byte[] hash = hashAlgorithm.Hash;
      hashAlgorithm.Initialize();
      hashAlgorithm.TransformBlock(clientWriteMac, 0, clientWriteMac.Length, clientWriteMac, 0);
      hashAlgorithm.TransformBlock(this.pad2, 0, this.pad2.Length, this.pad2, 0);
      hashAlgorithm.TransformBlock(hash, 0, hash.Length, hash, 0);
      hashAlgorithm.TransformFinalBlock(CipherSuite.EmptyArray, 0, 0);
      return hashAlgorithm.Hash;
    }

    public override void ComputeMasterSecret(byte[] preMasterSecret)
    {
      TlsStream tlsStream = new TlsStream();
      tlsStream.Write(this.prf(preMasterSecret, "A", this.Context.RandomCS));
      tlsStream.Write(this.prf(preMasterSecret, "BB", this.Context.RandomCS));
      tlsStream.Write(this.prf(preMasterSecret, "CCC", this.Context.RandomCS));
      this.Context.MasterSecret = tlsStream.ToArray();
    }

    public override void ComputeKeys()
    {
      TlsStream tlsStream1 = new TlsStream();
      char ch = 'A';
      int num = 1;
      while (tlsStream1.Length < (long) this.KeyBlockSize)
      {
        string empty = string.Empty;
        for (int index = 0; index < num; ++index)
          empty += ch.ToString();
        byte[] buffer = this.prf(this.Context.MasterSecret, empty.ToString(), this.Context.RandomSC);
        int count = tlsStream1.Length + (long) buffer.Length <= (long) this.KeyBlockSize ? buffer.Length : this.KeyBlockSize - (int) tlsStream1.Length;
        tlsStream1.Write(buffer, 0, count);
        ++ch;
        ++num;
      }
      TlsStream tlsStream2 = new TlsStream(tlsStream1.ToArray());
      this.Context.Negotiating.ClientWriteMAC = tlsStream2.ReadBytes(this.HashSize);
      this.Context.Negotiating.ServerWriteMAC = tlsStream2.ReadBytes(this.HashSize);
      this.Context.ClientWriteKey = tlsStream2.ReadBytes((int) this.KeyMaterialSize);
      this.Context.ServerWriteKey = tlsStream2.ReadBytes((int) this.KeyMaterialSize);
      if (!this.IsExportable)
      {
        if (this.IvSize != (byte) 0)
        {
          this.Context.ClientWriteIV = tlsStream2.ReadBytes((int) this.IvSize);
          this.Context.ServerWriteIV = tlsStream2.ReadBytes((int) this.IvSize);
        }
        else
        {
          this.Context.ClientWriteIV = CipherSuite.EmptyArray;
          this.Context.ServerWriteIV = CipherSuite.EmptyArray;
        }
      }
      else
      {
        HashAlgorithm hashAlgorithm = (HashAlgorithm) MD5.Create();
        byte[] outputBuffer = new byte[hashAlgorithm.HashSize >> 3];
        hashAlgorithm.TransformBlock(this.Context.ClientWriteKey, 0, this.Context.ClientWriteKey.Length, outputBuffer, 0);
        hashAlgorithm.TransformFinalBlock(this.Context.RandomCS, 0, this.Context.RandomCS.Length);
        byte[] numArray1 = new byte[(int) this.ExpandedKeyMaterialSize];
        Buffer.BlockCopy((Array) hashAlgorithm.Hash, 0, (Array) numArray1, 0, (int) this.ExpandedKeyMaterialSize);
        hashAlgorithm.Initialize();
        hashAlgorithm.TransformBlock(this.Context.ServerWriteKey, 0, this.Context.ServerWriteKey.Length, outputBuffer, 0);
        hashAlgorithm.TransformFinalBlock(this.Context.RandomSC, 0, this.Context.RandomSC.Length);
        byte[] numArray2 = new byte[(int) this.ExpandedKeyMaterialSize];
        Buffer.BlockCopy((Array) hashAlgorithm.Hash, 0, (Array) numArray2, 0, (int) this.ExpandedKeyMaterialSize);
        this.Context.ClientWriteKey = numArray1;
        this.Context.ServerWriteKey = numArray2;
        if (this.IvSize > (byte) 0)
        {
          hashAlgorithm.Initialize();
          byte[] hash1 = hashAlgorithm.ComputeHash(this.Context.RandomCS, 0, this.Context.RandomCS.Length);
          this.Context.ClientWriteIV = new byte[(int) this.IvSize];
          Buffer.BlockCopy((Array) hash1, 0, (Array) this.Context.ClientWriteIV, 0, (int) this.IvSize);
          hashAlgorithm.Initialize();
          byte[] hash2 = hashAlgorithm.ComputeHash(this.Context.RandomSC, 0, this.Context.RandomSC.Length);
          this.Context.ServerWriteIV = new byte[(int) this.IvSize];
          Buffer.BlockCopy((Array) hash2, 0, (Array) this.Context.ServerWriteIV, 0, (int) this.IvSize);
        }
        else
        {
          this.Context.ClientWriteIV = CipherSuite.EmptyArray;
          this.Context.ServerWriteIV = CipherSuite.EmptyArray;
        }
      }
      ClientSessionCache.SetContextInCache(this.Context);
      tlsStream2.Reset();
      tlsStream1.Reset();
    }

    private byte[] prf(byte[] secret, string label, byte[] random)
    {
      HashAlgorithm hashAlgorithm1 = (HashAlgorithm) MD5.Create();
      HashAlgorithm hashAlgorithm2 = (HashAlgorithm) SHA1.Create();
      TlsStream tlsStream = new TlsStream();
      tlsStream.Write(Encoding.ASCII.GetBytes(label));
      tlsStream.Write(secret);
      tlsStream.Write(random);
      byte[] hash1 = hashAlgorithm2.ComputeHash(tlsStream.ToArray(), 0, (int) tlsStream.Length);
      tlsStream.Reset();
      tlsStream.Write(secret);
      tlsStream.Write(hash1);
      byte[] hash2 = hashAlgorithm1.ComputeHash(tlsStream.ToArray(), 0, (int) tlsStream.Length);
      tlsStream.Reset();
      return hash2;
    }
  }
}
