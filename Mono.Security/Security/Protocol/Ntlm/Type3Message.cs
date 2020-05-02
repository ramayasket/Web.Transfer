// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Ntlm.Type3Message
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Globalization;
using System.Text;

namespace Mono.Security.Protocol.Ntlm
{
  public class Type3Message : MessageBase
  {
    private byte[] _challenge;
    private string _host;
    private string _domain;
    private string _username;
    private string _password;
    private byte[] _lm;
    private byte[] _nt;

    public Type3Message()
      : base(3)
    {
      this._domain = Environment.UserDomainName;
      this._host = Environment.MachineName;
      this._username = Environment.UserName;
      this.Flags = NtlmFlags.NegotiateUnicode | NtlmFlags.NegotiateNtlm | NtlmFlags.NegotiateAlwaysSign;
    }

    public Type3Message(byte[] message)
      : base(3)
    {
      this.Decode(message);
    }

    ~Type3Message()
    {
      if (this._challenge != null)
        Array.Clear((Array) this._challenge, 0, this._challenge.Length);
      if (this._lm != null)
        Array.Clear((Array) this._lm, 0, this._lm.Length);
      if (this._nt == null)
        return;
      Array.Clear((Array) this._nt, 0, this._nt.Length);
    }

    public byte[] Challenge
    {
      get
      {
        return this._challenge == null ? (byte[]) null : (byte[]) this._challenge.Clone();
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException(nameof (Challenge));
        if (value.Length != 8)
          throw new ArgumentException(Locale.GetText("Invalid Challenge Length (should be 8 bytes)."), nameof (Challenge));
        this._challenge = (byte[]) value.Clone();
      }
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

    public string Password
    {
      get
      {
        return this._password;
      }
      set
      {
        this._password = value;
      }
    }

    public string Username
    {
      get
      {
        return this._username;
      }
      set
      {
        this._username = value;
      }
    }

    public byte[] LM
    {
      get
      {
        return this._lm;
      }
    }

    public byte[] NT
    {
      get
      {
        return this._nt;
      }
    }

    protected override void Decode(byte[] message)
    {
      base.Decode(message);
      if ((int) BitConverterLE.ToUInt16(message, 56) != message.Length)
        throw new ArgumentException(Locale.GetText("Invalid Type3 message length."), nameof (message));
      this._password = (string) null;
      int uint16_1 = (int) BitConverterLE.ToUInt16(message, 28);
      int index = 64;
      this._domain = Encoding.Unicode.GetString(message, index, uint16_1);
      int uint16_2 = (int) BitConverterLE.ToUInt16(message, 44);
      int uint16_3 = (int) BitConverterLE.ToUInt16(message, 48);
      this._host = Encoding.Unicode.GetString(message, uint16_3, uint16_2);
      int uint16_4 = (int) BitConverterLE.ToUInt16(message, 36);
      int uint16_5 = (int) BitConverterLE.ToUInt16(message, 40);
      this._username = Encoding.Unicode.GetString(message, uint16_5, uint16_4);
      this._lm = new byte[24];
      int uint16_6 = (int) BitConverterLE.ToUInt16(message, 16);
      Buffer.BlockCopy((Array) message, uint16_6, (Array) this._lm, 0, 24);
      this._nt = new byte[24];
      int uint16_7 = (int) BitConverterLE.ToUInt16(message, 24);
      Buffer.BlockCopy((Array) message, uint16_7, (Array) this._nt, 0, 24);
      if (message.Length < 64)
        return;
      this.Flags = (NtlmFlags) BitConverterLE.ToUInt32(message, 60);
    }

    public override byte[] GetBytes()
    {
      byte[] bytes1 = Encoding.Unicode.GetBytes(this._domain.ToUpper(CultureInfo.InvariantCulture));
      byte[] bytes2 = Encoding.Unicode.GetBytes(this._username);
      byte[] bytes3 = Encoding.Unicode.GetBytes(this._host.ToUpper(CultureInfo.InvariantCulture));
      byte[] numArray = this.PrepareMessage(64 + bytes1.Length + bytes2.Length + bytes3.Length + 24 + 24);
      short num1 = (short) (64 + bytes1.Length + bytes2.Length + bytes3.Length);
      numArray[12] = (byte) 24;
      numArray[13] = (byte) 0;
      numArray[14] = (byte) 24;
      numArray[15] = (byte) 0;
      numArray[16] = (byte) num1;
      numArray[17] = (byte) ((uint) num1 >> 8);
      short num2 = (short) ((int) num1 + 24);
      numArray[20] = (byte) 24;
      numArray[21] = (byte) 0;
      numArray[22] = (byte) 24;
      numArray[23] = (byte) 0;
      numArray[24] = (byte) num2;
      numArray[25] = (byte) ((uint) num2 >> 8);
      short length1 = (short) bytes1.Length;
      short num3 = 64;
      numArray[28] = (byte) length1;
      numArray[29] = (byte) ((uint) length1 >> 8);
      numArray[30] = numArray[28];
      numArray[31] = numArray[29];
      numArray[32] = (byte) num3;
      numArray[33] = (byte) ((uint) num3 >> 8);
      short length2 = (short) bytes2.Length;
      short num4 = (short) ((int) num3 + (int) length1);
      numArray[36] = (byte) length2;
      numArray[37] = (byte) ((uint) length2 >> 8);
      numArray[38] = numArray[36];
      numArray[39] = numArray[37];
      numArray[40] = (byte) num4;
      numArray[41] = (byte) ((uint) num4 >> 8);
      short length3 = (short) bytes3.Length;
      short num5 = (short) ((int) num4 + (int) length2);
      numArray[44] = (byte) length3;
      numArray[45] = (byte) ((uint) length3 >> 8);
      numArray[46] = numArray[44];
      numArray[47] = numArray[45];
      numArray[48] = (byte) num5;
      numArray[49] = (byte) ((uint) num5 >> 8);
      short length4 = (short) numArray.Length;
      numArray[56] = (byte) length4;
      numArray[57] = (byte) ((uint) length4 >> 8);
      numArray[60] = (byte) this.Flags;
      numArray[61] = (byte) ((uint) this.Flags >> 8);
      numArray[62] = (byte) ((uint) this.Flags >> 16);
      numArray[63] = (byte) ((uint) this.Flags >> 24);
      Buffer.BlockCopy((Array) bytes1, 0, (Array) numArray, (int) num3, bytes1.Length);
      Buffer.BlockCopy((Array) bytes2, 0, (Array) numArray, (int) num4, bytes2.Length);
      Buffer.BlockCopy((Array) bytes3, 0, (Array) numArray, (int) num5, bytes3.Length);
      using (ChallengeResponse challengeResponse = new ChallengeResponse(this._password, this._challenge))
      {
        Buffer.BlockCopy((Array) challengeResponse.LM, 0, (Array) numArray, (int) num1, 24);
        Buffer.BlockCopy((Array) challengeResponse.NT, 0, (Array) numArray, (int) num2, 24);
      }
      return numArray;
    }
  }
}
