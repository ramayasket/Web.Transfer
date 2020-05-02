// Decompiled with JetBrains decompiler
// Type: mycloud.SecureTcpServer
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace mycloud
{
  public class SecureTcpServer : IDisposable
  {
    private X509Certificate serverCert;
    private RemoteCertificateValidationCallback certValidationCallback;
    private SecureConnectionResultsCallback connectionCallback;
    private AsyncCallback onAcceptConnection;
    private AsyncCallback onAuthenticateAsServer;
    private bool started;
    private int listenPort;
    private TcpListener listenerV4;
    private TcpListener listenerV6;
    private int disposed;
    private bool clientCertificateRequired;
    private bool checkCertifcateRevocation;
    private SslProtocols sslProtocols;

    public SecureTcpServer(
      int listenPort,
      X509Certificate serverCertificate,
      SecureConnectionResultsCallback callback)
      : this(listenPort, serverCertificate, callback, (RemoteCertificateValidationCallback) null)
    {
    }

    public SecureTcpServer(
      int listenPort,
      X509Certificate serverCertificate,
      SecureConnectionResultsCallback callback,
      RemoteCertificateValidationCallback certValidationCallback)
    {
      if (listenPort < 0 || listenPort > (int) ushort.MaxValue)
        throw new ArgumentOutOfRangeException(nameof (listenPort));
      if (serverCertificate == null)
        throw new ArgumentNullException(nameof (serverCertificate));
      if (callback == null)
        throw new ArgumentNullException(nameof (callback));
      this.onAcceptConnection = new AsyncCallback(this.OnAcceptConnection);
      this.onAuthenticateAsServer = new AsyncCallback(this.OnAuthenticateAsServer);
      this.serverCert = serverCertificate;
      this.certValidationCallback = certValidationCallback;
      this.connectionCallback = callback;
      this.listenPort = listenPort;
      this.disposed = 0;
      this.checkCertifcateRevocation = false;
      this.clientCertificateRequired = false;
      this.sslProtocols = SslProtocols.Default;
    }

    ~SecureTcpServer()
    {
      this.Dispose();
    }

    public SslProtocols SslProtocols
    {
      get
      {
        return this.sslProtocols;
      }
      set
      {
        this.sslProtocols = value;
      }
    }

    public bool CheckCertifcateRevocation
    {
      get
      {
        return this.checkCertifcateRevocation;
      }
      set
      {
        this.checkCertifcateRevocation = value;
      }
    }

    public bool ClientCertificateRequired
    {
      get
      {
        return this.clientCertificateRequired;
      }
      set
      {
        this.clientCertificateRequired = value;
      }
    }

    public void StartListening()
    {
      if (this.started)
        throw new InvalidOperationException("Already started...");
      if (Socket.SupportsIPv4 && this.listenerV4 == null)
      {
        IPEndPoint localEP = new IPEndPoint(IPAddress.Any, this.listenPort);
        Console.WriteLine("SecureTcpServer: Started listening on {0}", (object) localEP);
        this.listenerV4 = new TcpListener(localEP);
      }
      if (Socket.OSSupportsIPv6 && this.listenerV6 == null)
      {
        IPEndPoint localEP = new IPEndPoint(IPAddress.IPv6Any, this.listenPort);
        Console.WriteLine("SecureTcpServer: Started listening on {0}", (object) localEP);
        this.listenerV6 = new TcpListener(localEP);
      }
      if (this.listenerV4 != null)
      {
        this.listenerV4.Start();
        this.listenerV4.BeginAcceptTcpClient(this.onAcceptConnection, (object) this.listenerV4);
      }
      if (this.listenerV6 != null)
      {
        this.listenerV6.Start();
        this.listenerV6.BeginAcceptTcpClient(this.onAcceptConnection, (object) this.listenerV6);
      }
      this.started = true;
    }

    public void StopListening()
    {
      if (!this.started)
        return;
      this.started = false;
      if (this.listenerV4 != null)
        this.listenerV4.Stop();
      if (this.listenerV6 == null)
        return;
      this.listenerV6.Stop();
    }

    private void OnAcceptConnection(IAsyncResult result)
    {
      TcpListener asyncState = result.AsyncState as TcpListener;
      SslStreamM sslStreamM = (SslStreamM) null;
      try
      {
        if (!this.started)
          return;
        asyncState.BeginAcceptTcpClient(this.onAcceptConnection, (object) asyncState);
        TcpClient tcpClient = asyncState.EndAcceptTcpClient(result);
        bool leaveInnerStreamOpen = false;
        sslStreamM = this.certValidationCallback == null ? new SslStreamM((Stream) tcpClient.GetStream(), leaveInnerStreamOpen) : new SslStreamM((Stream) tcpClient.GetStream(), leaveInnerStreamOpen, this.certValidationCallback);
        sslStreamM.BeginAuthenticateAsServer(this.serverCert, false, this.sslProtocols, this.checkCertifcateRevocation, this.onAuthenticateAsServer, (object) sslStreamM);
      }
      catch (Exception ex)
      {
        sslStreamM?.Dispose();
        this.connectionCallback((object) this, new SecureConnectionResults(ex));
      }
    }

    private void OnAuthenticateAsServer(IAsyncResult result)
    {
      SslStreamM sslStream = (SslStreamM) null;
      try
      {
        sslStream = result.AsyncState as SslStreamM;
        sslStream.EndAuthenticateAsServer(result);
        this.connectionCallback((object) this, new SecureConnectionResults(sslStream));
      }
      catch (Exception ex)
      {
        sslStream?.Dispose();
        this.connectionCallback((object) this, new SecureConnectionResults(ex));
      }
    }

    public void Dispose()
    {
      if (Interlocked.Increment(ref this.disposed) != 1)
        return;
      if (this.listenerV4 != null)
        this.listenerV4.Stop();
      if (this.listenerV6 != null)
        this.listenerV6.Stop();
      this.listenerV4 = (TcpListener) null;
      this.listenerV6 = (TcpListener) null;
      GC.SuppressFinalize((object) this);
    }

    public void create_certificate()
    {
    }
  }
}
