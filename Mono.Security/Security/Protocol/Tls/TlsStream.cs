// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.TlsStream
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.IO;

namespace Mono.Security.Protocol.Tls
{
  internal class TlsStream : Stream
  {
    private const int temp_size = 4;
    private bool canRead;
    private bool canWrite;
    private MemoryStream buffer;
    private byte[] temp;

    public TlsStream()
    {
      this.buffer = new MemoryStream(0);
      this.canRead = false;
      this.canWrite = true;
    }

    public TlsStream(byte[] data)
    {
      this.buffer = data == null ? new MemoryStream() : new MemoryStream(data);
      this.canRead = true;
      this.canWrite = false;
    }

    public bool EOF
    {
      get
      {
        return this.Position >= this.Length;
      }
    }

    public override bool CanWrite
    {
      get
      {
        return this.canWrite;
      }
    }

    public override bool CanRead
    {
      get
      {
        return this.canRead;
      }
    }

    public override bool CanSeek
    {
      get
      {
        return this.buffer.CanSeek;
      }
    }

    public override long Position
    {
      get
      {
        return this.buffer.Position;
      }
      set
      {
        this.buffer.Position = value;
      }
    }

    public override long Length
    {
      get
      {
        return this.buffer.Length;
      }
    }

    private byte[] ReadSmallValue(int length)
    {
      if (length > 4)
        throw new ArgumentException("8 bytes maximum");
      if (this.temp == null)
        this.temp = new byte[4];
      if (this.Read(this.temp, 0, length) != length)
        throw new TlsException(string.Format("buffer underrun"));
      return this.temp;
    }

    public new byte ReadByte()
    {
      return this.ReadSmallValue(1)[0];
    }

    public short ReadInt16()
    {
      byte[] numArray = this.ReadSmallValue(2);
      return (short) ((int) numArray[0] << 8 | (int) numArray[1]);
    }

    public int ReadInt24()
    {
      byte[] numArray = this.ReadSmallValue(3);
      return (int) numArray[0] << 16 | (int) numArray[1] << 8 | (int) numArray[2];
    }

    public int ReadInt32()
    {
      byte[] numArray = this.ReadSmallValue(4);
      return (int) numArray[0] << 24 | (int) numArray[1] << 16 | (int) numArray[2] << 8 | (int) numArray[3];
    }

    public byte[] ReadBytes(int count)
    {
      byte[] buffer = new byte[count];
      if (this.Read(buffer, 0, count) != count)
        throw new TlsException("buffer underrun");
      return buffer;
    }

    public void Write(byte value)
    {
      if (this.temp == null)
        this.temp = new byte[4];
      this.temp[0] = value;
      this.Write(this.temp, 0, 1);
    }

    public void Write(short value)
    {
      if (this.temp == null)
        this.temp = new byte[4];
      this.temp[0] = (byte) ((uint) value >> 8);
      this.temp[1] = (byte) value;
      this.Write(this.temp, 0, 2);
    }

    public void WriteInt24(int value)
    {
      if (this.temp == null)
        this.temp = new byte[4];
      this.temp[0] = (byte) (value >> 16);
      this.temp[1] = (byte) (value >> 8);
      this.temp[2] = (byte) value;
      this.Write(this.temp, 0, 3);
    }

    public void Write(int value)
    {
      if (this.temp == null)
        this.temp = new byte[4];
      this.temp[0] = (byte) (value >> 24);
      this.temp[1] = (byte) (value >> 16);
      this.temp[2] = (byte) (value >> 8);
      this.temp[3] = (byte) value;
      this.Write(this.temp, 0, 4);
    }

    public void Write(ulong value)
    {
      this.Write((int) (value >> 32));
      this.Write((int) value);
    }

    public void Write(byte[] buffer)
    {
      this.Write(buffer, 0, buffer.Length);
    }

    public void Reset()
    {
      this.buffer.SetLength(0L);
      this.buffer.Position = 0L;
    }

    public byte[] ToArray()
    {
      return this.buffer.ToArray();
    }

    public override void Flush()
    {
      this.buffer.Flush();
    }

    public override void SetLength(long length)
    {
      this.buffer.SetLength(length);
    }

    public override long Seek(long offset, SeekOrigin loc)
    {
      return this.buffer.Seek(offset, loc);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      if (this.canRead)
        return this.buffer.Read(buffer, offset, count);
      throw new InvalidOperationException("Read operations are not allowed by this stream");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      if (!this.canWrite)
        throw new InvalidOperationException("Write operations are not allowed by this stream");
      this.buffer.Write(buffer, offset, count);
    }
  }
}
