// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Ntlm.MessageBase
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security.Protocol.Ntlm
{
  public abstract class MessageBase
  {
    private static byte[] header = new byte[8]
    {
      (byte) 78,
      (byte) 84,
      (byte) 76,
      (byte) 77,
      (byte) 83,
      (byte) 83,
      (byte) 80,
      (byte) 0
    };
    private int _type;
    private NtlmFlags _flags;

    protected MessageBase(int messageType)
    {
      this._type = messageType;
    }

    public NtlmFlags Flags
    {
      get
      {
        return this._flags;
      }
      set
      {
        this._flags = value;
      }
    }

    public int Type
    {
      get
      {
        return this._type;
      }
    }

    protected byte[] PrepareMessage(int messageSize)
    {
      byte[] numArray = new byte[messageSize];
      Buffer.BlockCopy((Array) MessageBase.header, 0, (Array) numArray, 0, 8);
      numArray[8] = (byte) this._type;
      numArray[9] = (byte) (this._type >> 8);
      numArray[10] = (byte) (this._type >> 16);
      numArray[11] = (byte) (this._type >> 24);
      return numArray;
    }

    protected virtual void Decode(byte[] message)
    {
      if (message == null)
        throw new ArgumentNullException(nameof (message));
      if (message.Length < 12)
      {
        string text = Locale.GetText("Minimum message length is 12 bytes.");
        throw new ArgumentOutOfRangeException(nameof (message), (object) message.Length, text);
      }
      if (!this.CheckHeader(message))
        throw new ArgumentException(string.Format(Locale.GetText("Invalid Type{0} message."), (object) this._type), nameof (message));
    }

    protected bool CheckHeader(byte[] message)
    {
      for (int index = 0; index < MessageBase.header.Length; ++index)
      {
        if ((int) message[index] != (int) MessageBase.header[index])
          return false;
      }
      return (long) BitConverterLE.ToUInt32(message, 8) == (long) this._type;
    }

    public abstract byte[] GetBytes();
  }
}
