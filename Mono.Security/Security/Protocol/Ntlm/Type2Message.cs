// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Ntlm.Type2Message
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Ntlm
{
  public class Type2Message : MessageBase
  {
    private byte[] _nonce;

    public Type2Message()
      : base(2)
    {
      this._nonce = new byte[8];
      RandomNumberGenerator.Create().GetBytes(this._nonce);
      this.Flags = NtlmFlags.NegotiateUnicode | NtlmFlags.NegotiateNtlm | NtlmFlags.NegotiateAlwaysSign;
    }

    public Type2Message(byte[] message)
      : base(2)
    {
      this._nonce = new byte[8];
      this.Decode(message);
    }

    ~Type2Message()
    {
      if (this._nonce == null)
        return;
      Array.Clear((Array) this._nonce, 0, this._nonce.Length);
    }

    public byte[] Nonce
    {
      get
      {
        return (byte[]) this._nonce.Clone();
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException(nameof (Nonce));
        if (value.Length != 8)
          throw new ArgumentException(Locale.GetText("Invalid Nonce Length (should be 8 bytes)."), nameof (Nonce));
        this._nonce = (byte[]) value.Clone();
      }
    }

    protected override void Decode(byte[] message)
    {
      base.Decode(message);
      this.Flags = (NtlmFlags) BitConverterLE.ToUInt32(message, 20);
      Buffer.BlockCopy((Array) message, 24, (Array) this._nonce, 0, 8);
    }

    public override byte[] GetBytes()
    {
      byte[] numArray = this.PrepareMessage(40);
      short length = (short) numArray.Length;
      numArray[16] = (byte) length;
      numArray[17] = (byte) ((uint) length >> 8);
      numArray[20] = (byte) this.Flags;
      numArray[21] = (byte) ((uint) this.Flags >> 8);
      numArray[22] = (byte) ((uint) this.Flags >> 16);
      numArray[23] = (byte) ((uint) this.Flags >> 24);
      Buffer.BlockCopy((Array) this._nonce, 0, (Array) numArray, 24, this._nonce.Length);
      return numArray;
    }
  }
}
