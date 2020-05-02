// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.TlsCipherSuite
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls
{
  internal class TlsCipherSuite : CipherSuite
  {
    private const int MacHeaderLength = 13;
    private byte[] header;

    public TlsCipherSuite(
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
    }

    public override byte[] ComputeServerRecordMAC(ContentType contentType, byte[] fragment)
    {
      if (this.header == null)
        this.header = new byte[13];
      this.Write(this.header, 0, !(this.Context is ClientContext) ? this.Context.WriteSequenceNumber : this.Context.ReadSequenceNumber);
      this.header[8] = (byte) contentType;
      this.Write(this.header, 9, this.Context.Protocol);
      this.Write(this.header, 11, (short) fragment.Length);
      HashAlgorithm serverHmac = (HashAlgorithm) this.ServerHMAC;
      serverHmac.TransformBlock(this.header, 0, this.header.Length, this.header, 0);
      serverHmac.TransformBlock(fragment, 0, fragment.Length, fragment, 0);
      serverHmac.TransformFinalBlock(CipherSuite.EmptyArray, 0, 0);
      return serverHmac.Hash;
    }

    public override byte[] ComputeClientRecordMAC(ContentType contentType, byte[] fragment)
    {
      if (this.header == null)
        this.header = new byte[13];
      this.Write(this.header, 0, !(this.Context is ClientContext) ? this.Context.ReadSequenceNumber : this.Context.WriteSequenceNumber);
      this.header[8] = (byte) contentType;
      this.Write(this.header, 9, this.Context.Protocol);
      this.Write(this.header, 11, (short) fragment.Length);
      HashAlgorithm clientHmac = (HashAlgorithm) this.ClientHMAC;
      clientHmac.TransformBlock(this.header, 0, this.header.Length, this.header, 0);
      clientHmac.TransformBlock(fragment, 0, fragment.Length, fragment, 0);
      clientHmac.TransformFinalBlock(CipherSuite.EmptyArray, 0, 0);
      return clientHmac.Hash;
    }

    public override void ComputeMasterSecret(byte[] preMasterSecret)
    {
      this.Context.MasterSecret = new byte[preMasterSecret.Length];
      this.Context.MasterSecret = this.PRF(preMasterSecret, "master secret", this.Context.RandomCS, 48);
    }

    public override void ComputeKeys()
    {
      TlsStream tlsStream = new TlsStream(this.PRF(this.Context.MasterSecret, "key expansion", this.Context.RandomSC, this.KeyBlockSize));
      this.Context.Negotiating.ClientWriteMAC = tlsStream.ReadBytes(this.HashSize);
      this.Context.Negotiating.ServerWriteMAC = tlsStream.ReadBytes(this.HashSize);
      this.Context.ClientWriteKey = tlsStream.ReadBytes((int) this.KeyMaterialSize);
      this.Context.ServerWriteKey = tlsStream.ReadBytes((int) this.KeyMaterialSize);
      if (!this.IsExportable)
      {
        if (this.IvSize != (byte) 0)
        {
          this.Context.ClientWriteIV = tlsStream.ReadBytes((int) this.IvSize);
          this.Context.ServerWriteIV = tlsStream.ReadBytes((int) this.IvSize);
        }
        else
        {
          this.Context.ClientWriteIV = CipherSuite.EmptyArray;
          this.Context.ServerWriteIV = CipherSuite.EmptyArray;
        }
      }
      else
      {
        byte[] numArray1 = this.PRF(this.Context.ClientWriteKey, "client write key", this.Context.RandomCS, (int) this.ExpandedKeyMaterialSize);
        byte[] numArray2 = this.PRF(this.Context.ServerWriteKey, "server write key", this.Context.RandomCS, (int) this.ExpandedKeyMaterialSize);
        this.Context.ClientWriteKey = numArray1;
        this.Context.ServerWriteKey = numArray2;
        if (this.IvSize > (byte) 0)
        {
          byte[] numArray3 = this.PRF(CipherSuite.EmptyArray, "IV block", this.Context.RandomCS, (int) this.IvSize * 2);
          this.Context.ClientWriteIV = new byte[(int) this.IvSize];
          Buffer.BlockCopy((Array) numArray3, 0, (Array) this.Context.ClientWriteIV, 0, this.Context.ClientWriteIV.Length);
          this.Context.ServerWriteIV = new byte[(int) this.IvSize];
          Buffer.BlockCopy((Array) numArray3, (int) this.IvSize, (Array) this.Context.ServerWriteIV, 0, this.Context.ServerWriteIV.Length);
        }
        else
        {
          this.Context.ClientWriteIV = CipherSuite.EmptyArray;
          this.Context.ServerWriteIV = CipherSuite.EmptyArray;
        }
      }
      ClientSessionCache.SetContextInCache(this.Context);
      tlsStream.Reset();
    }
  }
}
