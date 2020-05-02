// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.HandshakeMessage
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security.Protocol.Tls.Handshake
{
  internal abstract class HandshakeMessage : TlsStream
  {
    private Context context;
    private HandshakeType handshakeType;
    private ContentType contentType;
    private byte[] cache;

    public HandshakeMessage(Context context, HandshakeType handshakeType)
      : this(context, handshakeType, ContentType.Handshake)
    {
    }

    public HandshakeMessage(Context context, HandshakeType handshakeType, ContentType contentType)
    {
      this.context = context;
      this.handshakeType = handshakeType;
      this.contentType = contentType;
    }

    public HandshakeMessage(Context context, HandshakeType handshakeType, byte[] data)
      : base(data)
    {
      this.context = context;
      this.handshakeType = handshakeType;
    }

    public Context Context
    {
      get
      {
        return this.context;
      }
    }

    public HandshakeType HandshakeType
    {
      get
      {
        return this.handshakeType;
      }
    }

    public ContentType ContentType
    {
      get
      {
        return this.contentType;
      }
    }

    protected abstract void ProcessAsTls1();

    protected abstract void ProcessAsSsl3();

    public void Process()
    {
      switch (this.Context.SecurityProtocol)
      {
        case SecurityProtocolType.Default:
        case SecurityProtocolType.Tls:
          this.ProcessAsTls1();
          break;
        case SecurityProtocolType.Ssl3:
          this.ProcessAsSsl3();
          break;
        default:
          throw new NotSupportedException("Unsupported security protocol type");
      }
    }

    public virtual void Update()
    {
      if (!this.CanWrite)
        return;
      if (this.cache == null)
        this.cache = this.EncodeMessage();
      this.context.HandshakeMessages.Write(this.cache);
      this.Reset();
      this.cache = (byte[]) null;
    }

    public virtual byte[] EncodeMessage()
    {
      this.cache = (byte[]) null;
      if (this.CanWrite)
      {
        byte[] array = this.ToArray();
        int length = array.Length;
        this.cache = new byte[4 + length];
        this.cache[0] = (byte) this.HandshakeType;
        this.cache[1] = (byte) (length >> 16);
        this.cache[2] = (byte) (length >> 8);
        this.cache[3] = (byte) length;
        Buffer.BlockCopy((Array) array, 0, (Array) this.cache, 4, length);
      }
      return this.cache;
    }

    public static bool Compare(byte[] buffer1, byte[] buffer2)
    {
      if (buffer1 == null || buffer2 == null || buffer1.Length != buffer2.Length)
        return false;
      for (int index = 0; index < buffer1.Length; ++index)
      {
        if ((int) buffer1[index] != (int) buffer2[index])
          return false;
      }
      return true;
    }
  }
}
