// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Ntlm.Type1Message
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Globalization;
using System.Text;

namespace Mono.Security.Protocol.Ntlm
{
  public class Type1Message : MessageBase
  {
    private string _host;
    private string _domain;

    public Type1Message()
      : base(1)
    {
      this._domain = Environment.UserDomainName;
      this._host = Environment.MachineName;
      this.Flags = NtlmFlags.NegotiateUnicode | NtlmFlags.NegotiateOem | NtlmFlags.NegotiateNtlm | NtlmFlags.NegotiateDomainSupplied | NtlmFlags.NegotiateWorkstationSupplied | NtlmFlags.NegotiateAlwaysSign;
    }

    public Type1Message(byte[] message)
      : base(1)
    {
      this.Decode(message);
    }

    public string Domain
    {
      get
      {
        return this._domain;
      }
      set
      {
        this._domain = value;
      }
    }

    public string Host
    {
      get
      {
        return this._host;
      }
      set
      {
        this._host = value;
      }
    }

    protected override void Decode(byte[] message)
    {
      base.Decode(message);
      this.Flags = (NtlmFlags) BitConverterLE.ToUInt32(message, 12);
      int uint16_1 = (int) BitConverterLE.ToUInt16(message, 16);
      int uint16_2 = (int) BitConverterLE.ToUInt16(message, 20);
      this._domain = Encoding.ASCII.GetString(message, uint16_2, uint16_1);
      int uint16_3 = (int) BitConverterLE.ToUInt16(message, 24);
      this._host = Encoding.ASCII.GetString(message, 32, uint16_3);
    }

    public override byte[] GetBytes()
    {
      short length1 = (short) this._domain.Length;
      short length2 = (short) this._host.Length;
      byte[] numArray = this.PrepareMessage(32 + (int) length1 + (int) length2);
      numArray[12] = (byte) this.Flags;
      numArray[13] = (byte) ((uint) this.Flags >> 8);
      numArray[14] = (byte) ((uint) this.Flags >> 16);
      numArray[15] = (byte) ((uint) this.Flags >> 24);
      short num = (short) (32 + (int) length2);
      numArray[16] = (byte) length1;
      numArray[17] = (byte) ((uint) length1 >> 8);
      numArray[18] = numArray[16];
      numArray[19] = numArray[17];
      numArray[20] = (byte) num;
      numArray[21] = (byte) ((uint) num >> 8);
      numArray[24] = (byte) length2;
      numArray[25] = (byte) ((uint) length2 >> 8);
      numArray[26] = numArray[24];
      numArray[27] = numArray[25];
      numArray[28] = (byte) 32;
      numArray[29] = (byte) 0;
      byte[] bytes1 = Encoding.ASCII.GetBytes(this._host.ToUpper(CultureInfo.InvariantCulture));
      Buffer.BlockCopy((Array) bytes1, 0, (Array) numArray, 32, bytes1.Length);
      byte[] bytes2 = Encoding.ASCII.GetBytes(this._domain.ToUpper(CultureInfo.InvariantCulture));
      Buffer.BlockCopy((Array) bytes2, 0, (Array) numArray, (int) num, bytes2.Length);
      return numArray;
    }
  }
}
