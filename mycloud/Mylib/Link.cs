// Decompiled with JetBrains decompiler
// Type: Mylib.Link
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Mylib
{
  public class Link
  {
    private static object mylock = new object();
    private static string ssl_password = "";
    private bool isopen = false;
    private SslStream ssl;
    private bool waitauth;
    public static bool nocheck;
    private TcpClient sk;
    private string inbf;
    private static string work_path;
    private static string app_path;
    private static X509Certificate saved_cert;
    public Link.del_progress_cb progress_cb;

    public static void set_paths(string work, string app)
    {
      Link.work_path = work;
      Link.app_path = app;
    }

    public void set_nodelay()
    {
      if (this.sk == null)
        return;
      this.sk.NoDelay = false;
    }

    public bool accept(TcpListener listen, int seconds)
    {
      int num = stat.time();
      stat.imsg("accept: Wait for connection");
      do
      {
        if (!listen.Pending())
          Thread.Sleep(10);
        else
          goto label_4;
      }
      while (stat.time() - num <= seconds);
      return false;
label_4:
      this.sk = listen.AcceptTcpClient();
      stat.imsg("accept: Wait for connection - got connection");
      this.isopen = true;
      return true;
    }

    public bool open(string dest, int port, out string reason)
    {
      reason = "";
      this.sk = new TcpClient();
      try
      {
        this.sk.Connect(dest, port);
      }
      catch (SocketException ex)
      {
        reason = string.Format("Open socket error {0}", (object) ex.SocketErrorCode, (object) ex.Message);
        return false;
      }
      catch
      {
        reason = string.Format("Open socket error, general exception");
        return false;
      }
      this.isopen = true;
      return true;
    }

    private static bool cb_cert(
      object sender,
      X509Certificate certificate,
      X509Chain chain,
      SslPolicyErrors policyErrors)
    {
      bool flag = policyErrors == SslPolicyErrors.None;
      if (Link.nocheck || certificate == null)
        return true;
      stat.imsg("Certificate validation test {0} {1} {2}", (object) flag, (object) certificate.Subject, (object) policyErrors.ToString());
      if (!flag)
      {
        if (Link.certs_check(certificate.Subject))
          flag = true;
        if (!flag)
        {
          stat.imsg("Bad Certificate: " + certificate.Subject + " " + policyErrors.ToString() + ", shall we accept it?");
          Link.certs_add(certificate.Subject);
          flag = true;
        }
      }
      return flag;
    }

    private static void certs_add(string x)
    {
      File.AppendAllText(stat.app("certificates.dat"), x + "\r\n");
    }

    private static bool certs_check(string x)
    {
      string path = stat.app("certificates.dat");
      try
      {
        if (File.ReadAllText(path).Contains(x))
          return true;
      }
      catch
      {
      }
      return false;
    }

    private static string mywork(string x)
    {
      return Path.Combine(Link.work_path, x);
    }

    private static string myapp(string x)
    {
      return Path.Combine(Link.app_path, x);
    }

    public static X509Certificate load_cert()
    {
      if (Link.saved_cert != null)
        return Link.saved_cert;
      X509Certificate2 cert = new X509Certificate2();
      if (!Link.load_cert_file(cert, Link.mywork("ssl/server.pfx")) && !Link.load_cert_file(cert, Link.mywork("ssl/server.pem")) && !Link.load_cert_file(cert, Link.myapp("server.pfx")))
        return (X509Certificate) null;
      Link.saved_cert = (X509Certificate) cert;
      return (X509Certificate) cert;
    }

    public static void set_ssl_password(string x)
    {
      Link.ssl_password = x;
    }

    private static bool load_cert_file(X509Certificate2 cert, string fname)
    {
      try
      {
        string password = Link.ssl_password;
        if (password.Length == 0)
          password = "secret";
        if (!File.Exists(fname))
        {
          stat.imsg("Not found {0} (this is ok)", (object) fname);
          return false;
        }
        if (fname.Contains(".pfx") && password.Length > 0)
          cert.Import(fname, password, X509KeyStorageFlags.DefaultKeySet);
        else
          cert.Import(fname);
        stat.imsg("Loaded certificate {0} ok", (object) fname);
        return true;
      }
      catch (Exception ex)
      {
        stat.imsg("Failed to load certificate {0} {1}", (object) fname, (object) ex.Message);
        return false;
      }
    }

    private void ssl_as_server(SslStream ssl, X509Certificate cert)
    {
      this.waitauth = true;
      stat.imsg("ssl_as_server: NEW CODE");
      ssl.BeginAuthenticateAsServer(cert, false, SslProtocols.Default, false, new AsyncCallback(this.auth_callback2), (object) this);
      stat.imsg("ssl_as_server: NEW CODE - now wait for flag");
      while (this.waitauth)
        Thread.Sleep(100);
      stat.imsg("ssl_as_server: NEW CODE - GOT FLAG ");
    }

    private void auth_callback2(IAsyncResult result)
    {
      if (this.ssl == null)
      {
        stat.imsg("connection state is null, that is not good");
        this.waitauth = false;
      }
      else
      {
        try
        {
          this.ssl.EndAuthenticateAsServer(result);
        }
        catch (Exception ex)
        {
          stat.imsg("sslauth: ENDAUTH {0} {1}", (object) ex.Message, (object) ex.InnerException);
          this.ssl.Dispose();
        }
        this.waitauth = false;
      }
    }

    public bool ssl_enable(string dest, bool isserver)
    {
      stat.imsg("ssl_enable: dest={0} issuserver={1}", (object) dest, (object) isserver);
      this.ssl = new SslStream((Stream) this.sk.GetStream(), false, new RemoteCertificateValidationCallback(Link.cb_cert), (LocalCertificateSelectionCallback) null);
      if (this.ssl == null)
        return false;
      try
      {
        if (isserver)
        {
          X509Certificate serverCertificate = Link.load_cert();
          if (serverCertificate == null)
            stat.imsg("certificate is NULL, PANIC");
          stat.imsg("SSL: authenticate as server");
          this.ssl.AuthenticateAsServer(serverCertificate, false, SslProtocols.Default, false);
          stat.imsg("SSL: authenticate as server-done");
        }
        else
        {
          stat.imsg("SSL: authenticate as client {0}", (object) dest);
          this.ssl.AuthenticateAsClient(dest);
          stat.imsg("SSL: authenticate as client-done");
        }
      }
      catch (AuthenticationException ex)
      {
        stat.imsg("Exception ssl auth: {0}", (object) ex.Message);
        if (ex.InnerException != null)
          stat.imsg("Inner exception: {0}", (object) ex.InnerException.Message);
        return false;
      }
      catch (Exception ex)
      {
        stat.imsg("SSL Exception - unknown: {0}", (object) ex.ToString());
        return false;
      }
      return this.ssl != null;
    }

    public bool netclose()
    {
      if (!this.isopen)
        return true;
      stat.imsg("NETCLOSE EVENT OCCURRING");
      this.sk.Close();
      if (this.ssl != null)
        this.ssl.Close();
      this.ssl = (SslStream) null;
      this.isopen = false;
      return true;
    }

    public bool netprintf(string format, params object[] args)
    {
      stat.imsg("tcp: Send> {0}", (object) string.Format(format, args));
      return this.send(string.Format(format, args));
    }

    public bool ssl_send(string stuff)
    {
      try
      {
        this.ssl.Write(Encoding.UTF8.GetBytes(stuff), 0, stuff.Length);
        this.ssl.Flush();
      }
      catch (Exception ex)
      {
        stat.imsg("send failed on socket {0} was SENDING: {1}", (object) ex.ToString(), (object) stuff);
        return false;
      }
      return true;
    }

    public bool ssl_write(byte[] bf, int len)
    {
      try
      {
        this.ssl.Write(bf, 0, len);
        this.ssl.Flush();
      }
      catch (Exception ex)
      {
        stat.imsg("send failed on socket {0} ", (object) ex.ToString());
        return false;
      }
      return true;
    }

    public void set_nodelay(bool x)
    {
      this.sk.NoDelay = x;
    }

    public bool send(string stuff)
    {
      if (this.ssl != null)
        return this.ssl_send(stuff);
      try
      {
        this.sk.GetStream();
        this.sk.GetStream().Write(Encoding.UTF8.GetBytes(stuff), 0, stuff.Length);
      }
      catch (Exception ex)
      {
        stat.imsg("send failed on socket {0} was SENDING: {1}", (object) ex.ToString(), (object) stuff);
        return false;
      }
      return true;
    }

    public bool write(byte[] bf, int len)
    {
      if (this.ssl != null)
        return this.ssl_write(bf, len);
      try
      {
        this.sk.GetStream().Write(bf, 0, len);
      }
      catch (Exception ex)
      {
        stat.imsg("write failed {0}", (object) ex.ToString());
        return false;
      }
      return true;
    }

    public bool read_exact(out string stuff, int want)
    {
      StringBuilder stringBuilder = new StringBuilder();
      int got = 0;
      int tot = want;
      string stuff1;
      while (want > 0 && this.read(out stuff1, want, 60000, out bool _, out string _))
      {
        stringBuilder.Append(stuff1);
        want -= stuff1.Length;
        got += stuff1.Length;
        if (this.progress_cb != null && !this.progress_cb(got, tot))
        {
          stat.imsg("progress_cb told us to close the link");
          this.netclose();
          stuff = "DOWNLOAD ABORTED";
          return false;
        }
      }
      stuff = stringBuilder.ToString();
      return true;
    }

    public bool read(
      out string stuff,
      int max,
      int timetowait,
      out bool istimedout,
      out string emsg)
    {
      emsg = "";
      istimedout = false;
      if (!this.isopen)
      {
        stuff = "";
        return false;
      }
      if (this.inbf == null || this.inbf.Length <= 0)
        return this.read_raw(out stuff, max, timetowait, out istimedout, out emsg);
      if (max < this.inbf.Length)
      {
        stuff = this.inbf.Substring(0, max);
        this.inbf = this.inbf.Substring(max);
        return true;
      }
      stuff = this.inbf;
      this.inbf = "";
      return true;
    }

    private bool read_raw(
      out string stuff,
      int max,
      int msec,
      out bool istimeout,
      out string emsg)
    {
      stuff = "";
      emsg = "";
      istimeout = false;
      if (!this.isopen)
      {
        emsg = "is not open";
        return false;
      }
      if (this.sk == null)
      {
        emsg = "socket is null";
        this.isopen = false;
        return false;
      }
      if (this.ssl != null)
      {
        emsg = "ssl...";
        return this.ssl_read_raw(out stuff, max, msec, out istimeout);
      }
      if (!this.sk.Connected)
      {
        stat.imsg("read_raw: Socket is not connected - SO CLOSING IT");
        emsg = "not connected";
        this.isopen = false;
        this.netclose();
        return false;
      }
      Encoding.GetEncoding("ISO-8859-1");
      NetworkStream stream;
      try
      {
        stream = this.sk.GetStream();
      }
      catch (SocketException ex)
      {
        this.isopen = false;
        emsg = string.Format("getstream: {0} {1}", (object) ex.SocketErrorCode, (object) ex.Message);
        return false;
      }
      catch (Exception ex)
      {
        this.isopen = false;
        emsg = string.Format("getstream: general exception");
        stat.imsg("{0}", (object) ex.ToString());
        return false;
      }
      StringBuilder stringBuilder = new StringBuilder();
      byte[] numArray = new byte[max];
      if (msec == 0)
        msec = 1;
      stream.ReadTimeout = msec;
      int len;
      try
      {
        len = stream.Read(numArray, 0, max);
        if (len == 0)
        {
          this.netclose();
          this.isopen = false;
          emsg = string.Format("getstream: returned zero, socket closed");
          return false;
        }
      }
      catch (SocketException ex)
      {
        if (ex.SocketErrorCode == SocketError.TimedOut)
          istimeout = true;
        stat.imsg("Raw read exception {0}", (object) ex.ToString());
        stuff = "";
        emsg = string.Format("SocketException {0} {1}", (object) ex.SocketErrorCode, (object) ex.Message);
        return false;
      }
      catch (Exception ex)
      {
        stat.imsg("exception {0}", (object) ex.ToString());
        stuff = "";
        emsg = "general exception";
        return false;
      }
      stuff = Link.byte_to_string(numArray, len);
      return true;
    }

    public int read_bytes(byte[] bf, int max, int msec, out bool istimeout, out string reason)
    {
      reason = "";
      istimeout = false;
      NetworkStream networkStream;
      try
      {
        networkStream = this.sk.GetStream();
      }
      catch
      {
        networkStream = (NetworkStream) null;
      }
      if (networkStream == null)
        return -1;
      try
      {
        try
        {
          networkStream.ReadTimeout = msec;
        }
        catch
        {
        }
        return networkStream.Read(bf, 0, max);
      }
      catch (SocketException ex)
      {
        if (ex.SocketErrorCode == SocketError.TimedOut)
          istimeout = true;
        reason = string.Format("SocketException {0} {1}", (object) ex.SocketErrorCode, (object) ex.Message);
        return -1;
      }
      catch (TimeoutException ex)
      {
        reason = string.Format("SocketReadtimeoutException {0}", (object) ex.Message);
        istimeout = true;
        return -1;
      }
      catch (Exception ex)
      {
        reason = string.Format("SocketReadException {0}", (object) ex.Message);
        return -1;
      }
    }

    private bool ssl_read_raw(out string stuff, int max, int msec, out bool istimeout)
    {
      Encoding.GetEncoding("ISO-8859-1");
      StringBuilder stringBuilder = new StringBuilder();
      stuff = "";
      istimeout = false;
      byte[] numArray = new byte[max];
      if (msec == 0)
        msec = 1;
      this.ssl.ReadTimeout = msec;
      int len;
      try
      {
        len = this.ssl.Read(numArray, 0, max);
        if (len == 0)
        {
          stat.imsg("ssl.Read returned {0}", (object) len);
          stuff = "";
          return false;
        }
      }
      catch (SocketException ex)
      {
        if (ex.SocketErrorCode == SocketError.TimedOut)
          istimeout = true;
        stat.imsg("Raw read exception {0}", (object) ex.ToString());
        stuff = "";
        return false;
      }
      catch (Exception ex)
      {
        stat.imsg("exception {0}", (object) ex.ToString());
        stuff = "";
        return false;
      }
      stuff = Link.byte_to_string(numArray, len);
      return true;
    }

    public bool readline(
      out string stuff,
      string whence,
      int msecwait,
      out bool istimeout,
      out string emsg)
    {
      string stuff1 = "";
      emsg = "";
      int num1 = -1;
      stuff = "";
      istimeout = false;
      bool flag = false;
      while (num1 < 0)
      {
        if (this.inbf != null)
        {
          num1 = this.inbf.IndexOf("\r\n");
          if (num1 < 0)
          {
            num1 = this.inbf.IndexOf("\n");
            if (num1 >= 0)
              flag = true;
          }
          if (num1 >= 0)
            break;
        }
        if (!this.read_raw(out stuff1, 1000, msecwait, out istimeout, out emsg))
        {
          if (!istimeout)
            this.netclose();
          return false;
        }
        this.inbf += stuff1;
      }
      int num2 = !flag ? num1 + 2 : num1 + 1;
      stuff = this.inbf.Substring(0, num2);
      this.inbf = this.inbf.Substring(num2);
      return true;
    }

    public bool netisopen()
    {
      if (!this.isopen)
        return false;
      if (!this.sk.Connected)
        this.netclose();
      return this.isopen;
    }

    public bool wait_read(int timeout)
    {
      int num = stat.time();
      while (!this.canread())
      {
        Thread.Sleep(10);
        if (stat.time() - num > timeout)
          break;
      }
      return this.canread();
    }

    public bool canread()
    {
      return this.inbf != null && this.inbf.Length > 0 || !this.isopen || this.canread_raw();
    }

    public bool canread_raw()
    {
      if (!this.sk.Connected)
      {
        this.netclose();
        return true;
      }
      try
      {
        if (this.sk.GetStream().DataAvailable)
          return true;
      }
      catch (Exception ex)
      {
        stat.imsg("canread: Exception on getstream {0}", (object) ex.ToString());
        this.netclose();
        return true;
      }
      return false;
    }

    public static byte[] string_to_byte(string x)
    {
      int length = x.Length;
      char[] charArray = x.ToCharArray();
      byte[] numArray = new byte[length];
      for (int index = 0; index < length; ++index)
        numArray[index] = (byte) charArray[index];
      return numArray;
    }

    public static string byte_to_string(byte[] datab, int len)
    {
      char[] chArray = new char[len];
      for (int index = 0; index < len; ++index)
        chArray[index] = (char) datab[index];
      return new string(chArray);
    }

    public delegate bool del_progress_cb(int got, int tot);
  }
}
