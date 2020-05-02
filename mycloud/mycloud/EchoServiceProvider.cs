// Decompiled with JetBrains decompiler
// Type: mycloud.EchoServiceProvider
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System.Text;
using TcpLib;

namespace mycloud
{
  public class EchoServiceProvider : TcpServiceProvider
  {
    private string _receivedStr;

    public override object Clone()
    {
      return (object) new EchoServiceProvider();
    }

    public override int get_timeout()
    {
      return 10;
    }

    public override void OnAcceptConnection(ConnectionState state)
    {
      this._receivedStr = "";
      if (state.Write(Encoding.UTF8.GetBytes("Hello World!\r\n"), 0, 14))
        return;
      state.EndConnection();
    }

    public override void OnReceiveData(ConnectionState state)
    {
      byte[] numArray = new byte[1024];
      clib.imsg("Echo server, got data");
      while (state.AvailableData > 0)
      {
        int count = state.Read(numArray, 0, 1024);
        if (count > 0)
        {
          this._receivedStr += Encoding.UTF8.GetString(numArray, 0, count);
          if (this._receivedStr.IndexOf("\n") >= 0)
          {
            state.Write(Encoding.UTF8.GetBytes(this._receivedStr), 0, this._receivedStr.Length);
            this._receivedStr = "";
          }
        }
        else
          state.EndConnection();
      }
    }

    public override void OnDropConnection(ConnectionState state)
    {
    }
  }
}
