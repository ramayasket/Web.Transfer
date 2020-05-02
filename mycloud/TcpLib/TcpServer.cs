// Decompiled with JetBrains decompiler
// Type: TcpLib.TcpServer
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using Mylib;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace TcpLib
{
  public class TcpServer
  {
    private int _maxConnections = 100;
    private bool isssl;
    private int _port;
    private Socket _listener;
    private TcpServiceProvider _provider;
    private ArrayList _connections;
    private string bindip;
    private AsyncCallback ConnectionReady;
    private WaitCallback AcceptConnection;
    private AsyncCallback ReceivedDataReady;

    public int maxCurrentConnections { get; private set; }

    public TcpServer(TcpServiceProvider provider, string bindip, int port, bool isssl)
    {
      this.maxCurrentConnections = 0;
      this._provider = provider;
      this._port = port;
      this.bindip = bindip;
      this.isssl = isssl;
      this._listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      this._connections = new ArrayList();
      this.ConnectionReady = new AsyncCallback(this.ConnectionReady_Handler);
      this.AcceptConnection = new WaitCallback(this.AcceptConnection_Handler);
      this.ReceivedDataReady = new AsyncCallback(this.ReceivedDataReady_Handler);
    }

    public bool Start()
    {
      try
      {
        clib.imsg("Listening for connections on port {1} : {0}", (object) this._port, (object) this.bindip);
        IPAddress address1 = (IPAddress) null;
        if (this.bindip.Length > 0)
        {
          foreach (IPAddress address2 in Dns.GetHostEntry(this.bindip).AddressList)
          {
            clib.imsg("Address is {0}", (object) address2.ToString());
            address1 = address2;
            if (address2.ToString() == this.bindip)
              break;
          }
        }
        if (address1 == null)
          address1 = IPAddress.Any;
        this._listener.Bind((EndPoint) new IPEndPoint(address1, this._port));
        Console.WriteLine("bound to port {0}", (object) this._port);
        this._listener.Listen(100);
        this._listener.BeginAccept(this.ConnectionReady, (object) null);
        return true;
      }
      catch (Exception ex)
      {
        clib.imsg("REceiveddataready error {0}", (object) ex.Message);
        return false;
      }
    }

    private void ConnectionReady_Handler(IAsyncResult ar)
    {
      lock (this)
      {
        if (this._listener == null)
          return;
        this.OnIdle();
        Socket socket;
        try
        {
          socket = this._listener.EndAccept(ar);
        }
        catch (Exception ex)
        {
          clib.wmsg("Exception on accepting connection {0}", (object) ex.Message);
          return;
        }
        if (this._connections.Count >= this._maxConnections)
        {
          clib.imsg("Max connections exceeded, server busy {0} ", (object) this._connections.Count);
          string s = "SE001: Server busy";
          socket.Send(Encoding.UTF8.GetBytes(s), 0, s.Length, SocketFlags.None);
          socket.Shutdown(SocketShutdown.Both);
          socket.Close();
        }
        else
        {
          ConnectionState connectionState = new ConnectionState();
          connectionState._conn = socket;
          connectionState._server = this;
          connectionState._provider = (TcpServiceProvider) this._provider.Clone();
          connectionState.wantssl = this.isssl;
          connectionState._buffer = new byte[4000];
          this._connections.Add((object) connectionState);
          ThreadPool.QueueUserWorkItem(this.AcceptConnection, (object) connectionState);
        }
        this._listener.BeginAccept(this.ConnectionReady, (object) null);
      }
    }

    public static bool cert_validate(
      object sender,
      X509Certificate certificate,
      X509Chain chain,
      SslPolicyErrors sslPolicyErrors)
    {
      if (sslPolicyErrors != SslPolicyErrors.None && (sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != SslPolicyErrors.None)
      {
        foreach (X509ChainStatus chainStatu in chain.ChainStatus)
          ;
      }
      return true;
    }

    public bool takeover_ssl(ConnectionState st, bool isserver)
    {
      Stream innerStream = (Stream) new NetworkStream(st._conn);
      st.ssl = new SslStreamM(innerStream, false, new RemoteCertificateValidationCallback(TcpServer.cert_validate));
      X509Certificate serverCertificate = Link.load_cert();
      if (serverCertificate == null)
      {
        st.ssl = (SslStreamM) null;
        return false;
      }
      try
      {
        st.ssl.BeginAuthenticateAsServer(serverCertificate, false, SslProtocols.Tls, false, new AsyncCallback(this.auth_callback), (object) st);
      }
      catch (Exception ex)
      {
        clib.imsg("takeover_ssl: failed: {0}", (object) ex.Message);
        return false;
      }
      return true;
    }

    private void auth_callback(IAsyncResult result)
    {
      if (!(result.AsyncState is ConnectionState asyncState))
      {
        clib.imsg("connection state is null, that is not good");
      }
      else
      {
        SslStreamM sslStreamM = asyncState.ssl;
        try
        {
          sslStreamM.EndAuthenticateAsServer(result);
        }
        catch (Exception ex)
        {
          clib.imsg("sslauth: ENDAUTH {0} {1}", (object) ex.Message, (object) ex.InnerException);
          if (sslStreamM != null)
          {
            sslStreamM.Dispose();
            sslStreamM = (SslStreamM) null;
            asyncState.ssl = (SslStreamM) null;
            asyncState.EndConnection();
          }
        }
        try
        {
          if (sslStreamM != null)
          {
            asyncState._provider.OnAcceptConnection(asyncState);
            this.begin_read(asyncState);
          }
        }
        catch (Exception ex)
        {
          clib.imsg("sslauth: accept {0}", (object) ex.Message);
          if (sslStreamM != null)
          {
            sslStreamM.Dispose();
            asyncState.ssl = (SslStreamM) null;
            asyncState.EndConnection();
          }
        }
      }
    }

    private void begin_read(ConnectionState st)
    {
      st.last_read = DateTime.Now;
      try
      {
        if (st.ssl != null)
          st.ssl.BeginRead(st._buffer, 0, st._buffer.Length, this.ReceivedDataReady, (object) st);
        else if (st._conn != null && st._conn.Connected)
          st._conn.BeginReceive(st._buffer, 0, 0, SocketFlags.None, this.ReceivedDataReady, (object) st);
      }
      catch (Exception ex)
      {
        clib.imsg("begin_read: error reading, so ending connection {0}", (object) ex.Message);
        st.EndConnection();
      }
    }

    private void AcceptConnection_Handler(object state)
    {
      ConnectionState connectionState = state as ConnectionState;
      if (connectionState.wantssl)
      {
        if (this.takeover_ssl(connectionState, true))
          return;
        connectionState.EndConnection();
      }
      else
      {
        connectionState._provider.OnAcceptConnection(connectionState);
        this.begin_read(connectionState);
        int currentConnections = this.CurrentConnections;
        if (currentConnections <= this.maxCurrentConnections)
          return;
        this.maxCurrentConnections = currentConnections;
      }
    }

    private void received_ssl(IAsyncResult ar)
    {
      ConnectionState asyncState = ar.AsyncState as ConnectionState;
      SslStreamM ssl = asyncState.ssl;
      int num = -1;
      try
      {
        num = ssl.EndRead(ar);
      }
      catch (Exception ex)
      {
        clib.imsg("ssl: endread failed {0}", (object) ex.Message);
        asyncState.EndConnection();
      }
      if (num == 0)
        asyncState.ssl = (SslStreamM) null;
      asyncState.bufflen = num;
      asyncState._provider.OnReceiveData(asyncState);
      if (asyncState.bufflen > 0)
        clib.imsg("PANIC, BUFFER WAS NOT COMPLETELY READ ========================= {0}", (object) asyncState.bufflen);
      this.begin_read(asyncState);
    }

    private void ReceivedDataReady_Handler(IAsyncResult ar)
    {
      ConnectionState asyncState = ar.AsyncState as ConnectionState;
      if (asyncState.ssl != null)
      {
        this.received_ssl(ar);
      }
      else
      {
        try
        {
          asyncState._conn.EndReceive(ar);
        }
        catch (Exception ex)
        {
          clib.imsg("REceiveddataready error {0} {1}", (object) ex.Message, (object) ex.ToString());
          asyncState.EndConnection();
          return;
        }
        if (asyncState._conn.Available == 0)
        {
          this.DropConnection(asyncState);
        }
        else
        {
          asyncState._provider.OnReceiveData(asyncState);
          try
          {
            this.begin_read(asyncState);
          }
          catch (Exception ex)
          {
            clib.imsg("Error setting up receive data, so connection must have closed {0}", (object) ex.Message);
            asyncState._provider.OnDropConnection(asyncState);
            this.DropConnection(asyncState);
          }
        }
      }
    }

    public void Stop()
    {
      lock (this)
      {
        this._listener.Close();
        this._listener = (Socket) null;
        foreach (object connection in this._connections)
        {
          ConnectionState state = connection as ConnectionState;
          try
          {
            state._provider.OnDropConnection(state);
          }
          catch (Exception ex)
          {
            clib.imsg("ondropconnection {0}", (object) ex.Message);
            throw ex;
          }
          state._conn.Shutdown(SocketShutdown.Both);
          state._conn.Close();
        }
        this._connections.Clear();
      }
    }

    public void OnIdle()
    {
      lock (this)
      {
        foreach (object connection in this._connections)
        {
          ConnectionState connectionState = connection as ConnectionState;
          try
          {
            DateTime lastRead = connectionState.last_read;
            if (connectionState.Connected)
            {
              TimeSpan timeSpan = DateTime.Now.Subtract(connectionState.last_read);
              if (timeSpan.TotalSeconds > (double) connectionState._provider.get_timeout())
              {
                clib.imsg("Onidle: Closing connection after {0} seconds", (object) timeSpan.TotalSeconds);
                connectionState._conn.Shutdown(SocketShutdown.Both);
                connectionState._conn.Close();
              }
            }
          }
          catch (Exception ex)
          {
            clib.imsg("onidle: exception {0} {1}", (object) ex.Message, (object) ex.ToString());
          }
        }
      }
    }

    internal void DropConnection(ConnectionState st)
    {
      lock (this)
      {
        try
        {
          if (st._conn != null)
            st.JustClose();
        }
        catch
        {
        }
        if (!this._connections.Contains((object) st))
          return;
        this._connections.Remove((object) st);
      }
    }

    public int MaxConnections
    {
      get
      {
        return this._maxConnections;
      }
      set
      {
        this._maxConnections = value;
      }
    }

    public int CurrentConnections
    {
      get
      {
        lock (this)
          return this._connections.Count;
      }
    }
  }
}
