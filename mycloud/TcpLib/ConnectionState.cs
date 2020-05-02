// Decompiled with JetBrains decompiler
// Type: TcpLib.ConnectionState
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpLib
{
  public class ConnectionState
  {
    public DateTime last_read = DateTime.Now;
    internal Socket _conn;
    internal TcpServer _server;
    internal TcpServiceProvider _provider;
    internal byte[] _buffer;
    internal int bufflen;
    public SslStreamM ssl;
    public bool wantssl;

    public EndPoint RemoteEndPoint
    {
      get
      {
        return this._conn.RemoteEndPoint;
      }
    }

    public EndPoint LocalEndPoint
    {
      get
      {
        return this._conn.LocalEndPoint;
      }
    }

    public void set_mynodelay(bool x)
    {
      this._conn.NoDelay = x;
    }

    public string localendpoint_ip()
    {
      return ((IPEndPoint) this.LocalEndPoint).Address.ToString();
    }

    public static int localendpoint_port(EndPoint ep)
    {
      return ((IPEndPoint) ep).Port;
    }

    public int localendpoint_port()
    {
      return ((IPEndPoint) this.LocalEndPoint).Port;
    }

    public int AvailableData
    {
      get
      {
        try
        {
          if (this.ssl != null)
            return this.bufflen;
          return this._conn == null || !this._conn.Connected ? 0 : this._conn.Available;
        }
        catch (Exception ex1)
        {
          clib.imsg("Error getting available data, probably channel is closed {0}", (object) ex1.Message);
          try
          {
            if (this._conn.Connected)
              this.JustClose();
            this._conn = (Socket) null;
          }
          catch (Exception ex2)
          {
            clib.imsg("availabledata: {0}", (object) ex2.Message);
          }
          return 0;
        }
      }
    }

    public bool Connected
    {
      get
      {
        return this._conn.Connected;
      }
    }

    private int ssl_read(byte[] buffer, int offset, int count)
    {
      if (count > this.bufflen)
        count = this.bufflen;
      Array.Copy((Array) this._buffer, (Array) buffer, count);
      this.bufflen -= count;
      Array.Copy((Array) this._buffer, count, (Array) this._buffer, 0, this.bufflen);
      return count;
    }

    public int Read(byte[] buffer, int offset, int count)
    {
      if (this.ssl != null)
        return this.ssl_read(buffer, offset, count);
      try
      {
        return this._conn.Available > 0 ? this._conn.Receive(buffer, offset, count, SocketFlags.None) : 0;
      }
      catch (Exception ex)
      {
        clib.imsg("onreceivedata crashed {0}", (object) ex.Message);
        return 0;
      }
    }

    public bool Write(byte[] buffer, int offset, int count)
    {
      if (this.ssl != null)
      {
        try
        {
          this.ssl.Write(buffer, offset, count);
          return true;
        }
        catch
        {
          clib.imsg("Write to socket failed");
          this.ssl.Close();
          this.ssl = (SslStreamM) null;
          return false;
        }
      }
      else
      {
        try
        {
          this.send_blocks(this._conn, buffer, offset, count);
          return true;
        }
        catch (Exception ex)
        {
          clib.imsg("write failed {0}", (object) ex.Message);
          return false;
        }
      }
    }

    private int send_blocks(Socket con, byte[] buffer, int offset, int count)
    {
      int offset1 = offset;
      while (offset1 < offset + count)
      {
        int size = 32000;
        int num1 = offset + count - offset1;
        if (num1 < size)
          size = num1;
        if (size != 0)
        {
          int num2 = con.Send(buffer, offset1, size, SocketFlags.None);
          if (num2 < 0)
          {
            clib.imsg("Send failed badly");
            con.Close();
            break;
          }
          if (num2 < size)
          {
            clib.imsg("Send failed only sent {0} of {1}, will retry", (object) num2, (object) size);
            Thread.Sleep(1000);
          }
          if (num2 > 0)
            offset1 += num2;
        }
        else
          break;
      }
      return count;
    }

    public bool write(string x)
    {
      if (!this.Write(Encoding.UTF8.GetBytes(x), 0, x.Length))
      {
        clib.imsg("Write failed so closing link");
        this.EndConnection();
      }
      return true;
    }

    public void EndConnection()
    {
      this._provider.OnDropConnection(this);
      this.JustClose();
      this._server.DropConnection(this);
    }

    public void JustClose()
    {
      if (this.ssl != null)
      {
        this.ssl.Close();
        this.ssl = (SslStreamM) null;
      }
      if (this._conn == null || !this._conn.Connected)
        return;
      this._conn.Shutdown(SocketShutdown.Both);
      this._conn.Close();
      this._conn = (Socket) null;
    }
  }
}
