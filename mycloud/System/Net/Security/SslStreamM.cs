// Decompiled with JetBrains decompiler
// Type: System.Net.Security.SslStreamM
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using Mono.Security.Protocol.Tls;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace System.Net.Security
{
  public class SslStreamM : AuthenticatedStream
  {
    private SslStreamBase ssl_stream;
    private RemoteCertificateValidationCallback validation_callback;
    private LocalCertificateSelectionCallback selection_callback;

    public SslStreamM(Stream innerStream)
      : this(innerStream, false)
    {
    }

    public SslStreamM(Stream innerStream, bool leaveInnerStreamOpen)
      : base(innerStream, leaveInnerStreamOpen)
    {
    }

    public SslStreamM(
      Stream innerStream,
      bool leaveInnerStreamOpen,
      RemoteCertificateValidationCallback userCertificateValidationCallback)
      : this(innerStream, leaveInnerStreamOpen, userCertificateValidationCallback, (LocalCertificateSelectionCallback) null)
    {
    }

    public SslStreamM(
      Stream innerStream,
      bool leaveInnerStreamOpen,
      RemoteCertificateValidationCallback userCertificateValidationCallback,
      LocalCertificateSelectionCallback userCertificateSelectionCallback)
      : base(innerStream, leaveInnerStreamOpen)
    {
      this.validation_callback = userCertificateValidationCallback;
      this.selection_callback = userCertificateSelectionCallback;
    }

    public override bool CanRead
    {
      get
      {
        return this.InnerStream.CanRead;
      }
    }

    public override bool CanSeek
    {
      get
      {
        return this.InnerStream.CanSeek;
      }
    }

    public override bool CanTimeout
    {
      get
      {
        return this.InnerStream.CanTimeout;
      }
    }

    public override bool CanWrite
    {
      get
      {
        return this.InnerStream.CanWrite;
      }
    }

    public override long Length
    {
      get
      {
        return this.InnerStream.Length;
      }
    }

    public override long Position
    {
      get
      {
        return this.InnerStream.Position;
      }
      set
      {
        throw new NotSupportedException("This stream does not support seek operations");
      }
    }

    public override bool IsAuthenticated
    {
      get
      {
        return this.ssl_stream != null;
      }
    }

    public override bool IsEncrypted
    {
      get
      {
        return this.IsAuthenticated;
      }
    }

    public override bool IsMutuallyAuthenticated
    {
      get
      {
        return this.IsAuthenticated && (this.IsServer ? this.RemoteCertificate != null : this.LocalCertificate != null);
      }
    }

    public override bool IsServer
    {
      get
      {
        return this.ssl_stream is SslServerStream;
      }
    }

    public override bool IsSigned
    {
      get
      {
        return this.IsAuthenticated;
      }
    }

    public override int ReadTimeout
    {
      get
      {
        return this.InnerStream.ReadTimeout;
      }
      set
      {
        this.InnerStream.ReadTimeout = value;
      }
    }

    public override int WriteTimeout
    {
      get
      {
        return this.InnerStream.WriteTimeout;
      }
      set
      {
        this.InnerStream.WriteTimeout = value;
      }
    }

    public virtual bool CheckCertRevocationStatus
    {
      get
      {
        return this.IsAuthenticated && this.ssl_stream.CheckCertRevocationStatus;
      }
    }

    public virtual System.Security.Authentication.CipherAlgorithmType CipherAlgorithm
    {
      get
      {
        this.CheckConnectionAuthenticated();
        switch (this.ssl_stream.CipherAlgorithm)
        {
          case Mono.Security.Protocol.Tls.CipherAlgorithmType.Des:
            return System.Security.Authentication.CipherAlgorithmType.Des;
          case Mono.Security.Protocol.Tls.CipherAlgorithmType.None:
            return System.Security.Authentication.CipherAlgorithmType.None;
          case Mono.Security.Protocol.Tls.CipherAlgorithmType.Rc2:
            return System.Security.Authentication.CipherAlgorithmType.Rc2;
          case Mono.Security.Protocol.Tls.CipherAlgorithmType.Rc4:
            return System.Security.Authentication.CipherAlgorithmType.Rc4;
          case Mono.Security.Protocol.Tls.CipherAlgorithmType.Rijndael:
            switch (this.ssl_stream.CipherStrength)
            {
              case 128:
                return System.Security.Authentication.CipherAlgorithmType.Aes128;
              case 192:
                return System.Security.Authentication.CipherAlgorithmType.Aes192;
              case 256:
                return System.Security.Authentication.CipherAlgorithmType.Aes256;
            }
            break;
          case Mono.Security.Protocol.Tls.CipherAlgorithmType.TripleDes:
            return System.Security.Authentication.CipherAlgorithmType.TripleDes;
        }
        throw new InvalidOperationException("Not supported cipher algorithm is in use. It is likely a bug in SslStreamM.");
      }
    }

    public virtual int CipherStrength
    {
      get
      {
        this.CheckConnectionAuthenticated();
        return this.ssl_stream.CipherStrength;
      }
    }

    public virtual System.Security.Authentication.HashAlgorithmType HashAlgorithm
    {
      get
      {
        this.CheckConnectionAuthenticated();
        switch (this.ssl_stream.HashAlgorithm)
        {
          case Mono.Security.Protocol.Tls.HashAlgorithmType.Md5:
            return System.Security.Authentication.HashAlgorithmType.Md5;
          case Mono.Security.Protocol.Tls.HashAlgorithmType.None:
            return System.Security.Authentication.HashAlgorithmType.None;
          case Mono.Security.Protocol.Tls.HashAlgorithmType.Sha1:
            return System.Security.Authentication.HashAlgorithmType.Sha1;
          default:
            throw new InvalidOperationException("Not supported hash algorithm is in use. It is likely a bug in SslStreamM.");
        }
      }
    }

    public virtual int HashStrength
    {
      get
      {
        this.CheckConnectionAuthenticated();
        return this.ssl_stream.HashStrength;
      }
    }

    public virtual System.Security.Authentication.ExchangeAlgorithmType KeyExchangeAlgorithm
    {
      get
      {
        this.CheckConnectionAuthenticated();
        switch (this.ssl_stream.KeyExchangeAlgorithm)
        {
          case Mono.Security.Protocol.Tls.ExchangeAlgorithmType.DiffieHellman:
            return System.Security.Authentication.ExchangeAlgorithmType.DiffieHellman;
          case Mono.Security.Protocol.Tls.ExchangeAlgorithmType.None:
            return System.Security.Authentication.ExchangeAlgorithmType.None;
          case Mono.Security.Protocol.Tls.ExchangeAlgorithmType.RsaKeyX:
            return System.Security.Authentication.ExchangeAlgorithmType.RsaKeyX;
          case Mono.Security.Protocol.Tls.ExchangeAlgorithmType.RsaSign:
            return System.Security.Authentication.ExchangeAlgorithmType.RsaSign;
          default:
            throw new InvalidOperationException("Not supported exchange algorithm is in use. It is likely a bug in SslStreamM.");
        }
      }
    }

    public virtual int KeyExchangeStrength
    {
      get
      {
        this.CheckConnectionAuthenticated();
        return this.ssl_stream.KeyExchangeStrength;
      }
    }

    public virtual X509Certificate LocalCertificate
    {
      get
      {
        this.CheckConnectionAuthenticated();
        return this.IsServer ? this.ssl_stream.ServerCertificate : ((SslClientStream) this.ssl_stream).SelectedClientCertificate;
      }
    }

    public virtual X509Certificate RemoteCertificate
    {
      get
      {
        this.CheckConnectionAuthenticated();
        return !this.IsServer ? this.ssl_stream.ServerCertificate : ((SslServerStream) this.ssl_stream).ClientCertificate;
      }
    }

    public virtual SslProtocols SslProtocol
    {
      get
      {
        this.CheckConnectionAuthenticated();
        switch (this.ssl_stream.SecurityProtocol)
        {
          case Mono.Security.Protocol.Tls.SecurityProtocolType.Default:
            return SslProtocols.Default;
          case Mono.Security.Protocol.Tls.SecurityProtocolType.Ssl2:
            return SslProtocols.Ssl2;
          case Mono.Security.Protocol.Tls.SecurityProtocolType.Ssl3:
            return SslProtocols.Ssl3;
          case Mono.Security.Protocol.Tls.SecurityProtocolType.Tls:
            return SslProtocols.Tls;
          default:
            throw new InvalidOperationException("Not supported SSL/TLS protocol is in use. It is likely a bug in SslStreamM.");
        }
      }
    }

    private X509Certificate OnCertificateSelection(
      X509CertificateCollection clientCerts,
      X509Certificate serverCert,
      string targetHost,
      X509CertificateCollection serverRequestedCerts)
    {
      string[] acceptableIssuers = new string[serverRequestedCerts != null ? serverRequestedCerts.Count : 0];
      for (int index = 0; index < acceptableIssuers.Length; ++index)
        acceptableIssuers[index] = serverRequestedCerts[index].Issuer;
      return this.selection_callback((object) this, targetHost, clientCerts, serverCert, acceptableIssuers);
    }

    public virtual IAsyncResult BeginAuthenticateAsClient(
      string targetHost,
      AsyncCallback asyncCallback,
      object asyncState)
    {
      return this.BeginAuthenticateAsClient(targetHost, new X509CertificateCollection(), SslProtocols.Tls, false, asyncCallback, asyncState);
    }

    public virtual IAsyncResult BeginAuthenticateAsClient(
      string targetHost,
      X509CertificateCollection clientCertificates,
      SslProtocols enabledSslProtocols,
      bool checkCertificateRevocation,
      AsyncCallback asyncCallback,
      object asyncState)
    {
      if (this.IsAuthenticated)
        throw new InvalidOperationException("This SslStreamM is already authenticated");
      SslClientStream sslClientStream = new SslClientStream(this.InnerStream, targetHost, !this.LeaveInnerStreamOpen, this.GetMonoSslProtocol(enabledSslProtocols), clientCertificates);
      sslClientStream.CheckCertRevocationStatus = checkCertificateRevocation;
      sslClientStream.PrivateKeyCertSelectionDelegate = (PrivateKeySelectionCallback) ((cert, host) =>
      {
        string certHashString = cert.GetCertHashString();
        foreach (X509Certificate clientCertificate in clientCertificates)
        {
          if (!(clientCertificate.GetCertHashString() != certHashString))
          {
            if (!(clientCertificate is X509Certificate2 x509Certificate2))
              x509Certificate2 = new X509Certificate2(clientCertificate);
            return x509Certificate2.PrivateKey;
          }
        }
        return (AsymmetricAlgorithm) null;
      });
      if (this.validation_callback != null)
        sslClientStream.ServerCertValidationDelegate = (CertificateValidationCallback) ((cert, certErrors) =>
        {
          X509Chain chain = new X509Chain();
          if (!(cert is X509Certificate2 certificate))
            certificate = new X509Certificate2(cert);
          if (!ServicePointManager.CheckCertificateRevocationList)
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
          SslPolicyErrors sslPolicyErrors = SslPolicyErrors.None;
          foreach (int certError in certErrors)
          {
            switch (certError)
            {
              case -2146762490:
                sslPolicyErrors |= SslPolicyErrors.RemoteCertificateNotAvailable;
                break;
              case -2146762481:
                sslPolicyErrors |= SslPolicyErrors.RemoteCertificateNameMismatch;
                break;
              default:
                sslPolicyErrors |= SslPolicyErrors.RemoteCertificateChainErrors;
                break;
            }
          }
          chain.Build(certificate);
          foreach (X509ChainStatus chainStatu in chain.ChainStatus)
          {
            if (chainStatu.Status != X509ChainStatusFlags.NoError)
            {
              if ((chainStatu.Status & X509ChainStatusFlags.PartialChain) != X509ChainStatusFlags.NoError)
                sslPolicyErrors |= SslPolicyErrors.RemoteCertificateNotAvailable;
              else
                sslPolicyErrors |= SslPolicyErrors.RemoteCertificateChainErrors;
            }
          }
          return this.validation_callback((object) this, cert, chain, sslPolicyErrors);
        });
      if (this.selection_callback != null)
        sslClientStream.ClientCertSelectionDelegate = new CertificateSelectionCallback(this.OnCertificateSelection);
      this.ssl_stream = (SslStreamBase) sslClientStream;
      return this.BeginWrite(new byte[0], 0, 0, asyncCallback, asyncState);
    }

    public override IAsyncResult BeginRead(
      byte[] buffer,
      int offset,
      int count,
      AsyncCallback asyncCallback,
      object asyncState)
    {
      this.CheckConnectionAuthenticated();
      return this.ssl_stream.BeginRead(buffer, offset, count, asyncCallback, asyncState);
    }

    public virtual IAsyncResult BeginAuthenticateAsServer(
      X509Certificate serverCertificate,
      AsyncCallback asyncCallback,
      object asyncState)
    {
      return this.BeginAuthenticateAsServer(serverCertificate, false, SslProtocols.Tls, false, asyncCallback, asyncState);
    }

    public virtual IAsyncResult BeginAuthenticateAsServer(
      X509Certificate serverCertificate,
      bool clientCertificateRequired,
      SslProtocols enabledSslProtocols,
      bool checkCertificateRevocation,
      AsyncCallback asyncCallback,
      object asyncState)
    {
      if (this.IsAuthenticated)
        throw new InvalidOperationException("This SslStreamM is already authenticated");

      var monoSsl = this.GetMonoSslProtocol(enabledSslProtocols);

      SslServerStream sslServerStream = new SslServerStream(this.InnerStream, serverCertificate, clientCertificateRequired, !this.LeaveInnerStreamOpen, monoSsl);
      sslServerStream.CheckCertRevocationStatus = checkCertificateRevocation;
      sslServerStream.PrivateKeyCertSelectionDelegate = (PrivateKeySelectionCallback) ((cert, targetHost) =>
      {
        if (!(serverCertificate is X509Certificate2 x509Certificate2))
          x509Certificate2 = new X509Certificate2(serverCertificate);
        return x509Certificate2?.PrivateKey;
      });
      if (this.validation_callback != null)
        sslServerStream.ClientCertValidationDelegate = (CertificateValidationCallback) ((cert, certErrors) =>
        {
          X509Chain chain = (X509Chain) null;
          if (cert is X509Certificate2)
          {
            chain = new X509Chain();
            chain.Build((X509Certificate2) cert);
          }
          SslPolicyErrors sslPolicyErrors = certErrors.Length > 0 ? SslPolicyErrors.RemoteCertificateChainErrors : SslPolicyErrors.None;
          return this.validation_callback((object) this, cert, chain, sslPolicyErrors);
        });
      this.ssl_stream = (SslStreamBase) sslServerStream;
      return this.BeginWrite(new byte[0], 0, 0, asyncCallback, asyncState);
    }

    private Mono.Security.Protocol.Tls.SecurityProtocolType GetMonoSslProtocol(
      SslProtocols ms)
    {
      switch (ms)
      {
        case SslProtocols.Ssl2:
          return Mono.Security.Protocol.Tls.SecurityProtocolType.Ssl2;
        case SslProtocols.Ssl3:
          return Mono.Security.Protocol.Tls.SecurityProtocolType.Ssl3;
        case SslProtocols.Tls:
          return Mono.Security.Protocol.Tls.SecurityProtocolType.Tls;
        default:
          return Mono.Security.Protocol.Tls.SecurityProtocolType.Default;
      }
    }

    public override IAsyncResult BeginWrite(
      byte[] buffer,
      int offset,
      int count,
      AsyncCallback asyncCallback,
      object asyncState)
    {
      this.CheckConnectionAuthenticated();
      return this.ssl_stream.BeginWrite(buffer, offset, count, asyncCallback, asyncState);
    }

    public virtual void AuthenticateAsClient(string targetHost)
    {
      this.AuthenticateAsClient(targetHost, new X509CertificateCollection(), SslProtocols.Tls, false);
    }

    public virtual void AuthenticateAsClient(
      string targetHost,
      X509CertificateCollection clientCertificates,
      SslProtocols enabledSslProtocols,
      bool checkCertificateRevocation)
    {
      this.EndAuthenticateAsClient(this.BeginAuthenticateAsClient(targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation, (AsyncCallback) null, (object) null));
    }

    public virtual void AuthenticateAsServer(X509Certificate serverCertificate)
    {
      this.AuthenticateAsServer(serverCertificate, false, SslProtocols.Tls, false);
    }

    public virtual void AuthenticateAsServer(
      X509Certificate serverCertificate,
      bool clientCertificateRequired,
      SslProtocols enabledSslProtocols,
      bool checkCertificateRevocation)
    {
      this.EndAuthenticateAsServer(this.BeginAuthenticateAsServer(serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation, (AsyncCallback) null, (object) null));
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (this.ssl_stream != null)
          this.ssl_stream.Dispose();
        this.ssl_stream = (SslStreamBase) null;
      }
      base.Dispose(disposing);
    }

    public virtual void EndAuthenticateAsClient(IAsyncResult asyncResult)
    {
      this.CheckConnectionAuthenticated();
      if (this.CanRead)
        this.ssl_stream.EndRead(asyncResult);
      else
        this.ssl_stream.EndWrite(asyncResult);
    }

    public virtual void EndAuthenticateAsServer(IAsyncResult asyncResult)
    {
      this.CheckConnectionAuthenticated();
      if (this.CanRead)
        this.ssl_stream.EndRead(asyncResult);
      else
        this.ssl_stream.EndWrite(asyncResult);
    }

    public override int EndRead(IAsyncResult asyncResult)
    {
      this.CheckConnectionAuthenticated();
      return this.ssl_stream.EndRead(asyncResult);
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
      this.CheckConnectionAuthenticated();
      this.ssl_stream.EndWrite(asyncResult);
    }

    public override void Flush()
    {
      this.CheckConnectionAuthenticated();
      this.InnerStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      return this.EndRead(this.BeginRead(buffer, offset, count, (AsyncCallback) null, (object) null));
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotSupportedException("This stream does not support seek operations");
    }

    public override void SetLength(long value)
    {
      this.InnerStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      this.EndWrite(this.BeginWrite(buffer, offset, count, (AsyncCallback) null, (object) null));
    }

    public void Write(byte[] buffer)
    {
      this.Write(buffer, 0, buffer.Length);
    }

    private void CheckConnectionAuthenticated()
    {
      if (!this.IsAuthenticated)
        throw new InvalidOperationException("This operation is invalid until it is successfully authenticated");
    }
  }
}
