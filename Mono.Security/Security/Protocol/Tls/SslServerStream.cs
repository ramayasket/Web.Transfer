// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.SslServerStream
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Protocol.Tls.Handshake;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace Mono.Security.Protocol.Tls
{
  public class SslServerStream : SslStreamBase
  {
    private CertificateValidationCallback _ClientCertValidation;
    private PrivateKeySelectionCallback _PrivateKeySelection;
    private CertificateValidationCallback2 _ClientCertValidation2;

    public SslServerStream(Stream stream, System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate)
      : this(stream, serverCertificate, false, false, SecurityProtocolType.Default)
    {
    }

    public SslServerStream(
      Stream stream,
      System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate,
      bool clientCertificateRequired,
      bool ownsStream)
      : this(stream, serverCertificate, clientCertificateRequired, ownsStream, SecurityProtocolType.Default)
    {
    }

    public SslServerStream(
      Stream stream,
      System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate,
      bool clientCertificateRequired,
      bool requestClientCertificate,
      bool ownsStream)
      : this(stream, serverCertificate, clientCertificateRequired, requestClientCertificate, ownsStream, SecurityProtocolType.Default)
    {
    }

    public SslServerStream(
      Stream stream,
      System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate,
      bool clientCertificateRequired,
      bool ownsStream,
      SecurityProtocolType securityProtocolType)
      : this(stream, serverCertificate, clientCertificateRequired, false, ownsStream, securityProtocolType)
    {
    }

    public SslServerStream(
      Stream stream,
      System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate,
      bool clientCertificateRequired,
      bool requestClientCertificate,
      bool ownsStream,
      SecurityProtocolType securityProtocolType)
      : base(stream, ownsStream)
    {
      this.context = (Context) new ServerContext(this, securityProtocolType, serverCertificate, clientCertificateRequired, requestClientCertificate);
      this.protocol = (RecordProtocol) new ServerRecordProtocol(this.innerStream, (ServerContext) this.context);
    }

    internal event CertificateValidationCallback ClientCertValidation
    {
      add
      {
        CertificateValidationCallback comparand = this._ClientCertValidation;
        CertificateValidationCallback validationCallback;
        do
        {
          validationCallback = comparand;
          comparand = Interlocked.CompareExchange<CertificateValidationCallback>(ref this._ClientCertValidation, validationCallback + value, comparand);
        }
        while (comparand != validationCallback);
      }
      remove
      {
        CertificateValidationCallback comparand = this._ClientCertValidation;
        CertificateValidationCallback validationCallback;
        do
        {
          validationCallback = comparand;
          comparand = Interlocked.CompareExchange<CertificateValidationCallback>(ref this._ClientCertValidation, validationCallback - value, comparand);
        }
        while (comparand != validationCallback);
      }
    }

    internal event PrivateKeySelectionCallback PrivateKeySelection
    {
      add
      {
        PrivateKeySelectionCallback comparand = this._PrivateKeySelection;
        PrivateKeySelectionCallback selectionCallback;
        do
        {
          selectionCallback = comparand;
          comparand = Interlocked.CompareExchange<PrivateKeySelectionCallback>(ref this._PrivateKeySelection, selectionCallback + value, comparand);
        }
        while (comparand != selectionCallback);
      }
      remove
      {
        PrivateKeySelectionCallback comparand = this._PrivateKeySelection;
        PrivateKeySelectionCallback selectionCallback;
        do
        {
          selectionCallback = comparand;
          comparand = Interlocked.CompareExchange<PrivateKeySelectionCallback>(ref this._PrivateKeySelection, selectionCallback - value, comparand);
        }
        while (comparand != selectionCallback);
      }
    }

    public event CertificateValidationCallback2 ClientCertValidation2
    {
      add
      {
        CertificateValidationCallback2 comparand = this._ClientCertValidation2;
        CertificateValidationCallback2 validationCallback2;
        do
        {
          validationCallback2 = comparand;
          comparand = Interlocked.CompareExchange<CertificateValidationCallback2>(ref this._ClientCertValidation2, validationCallback2 + value, comparand);
        }
        while (comparand != validationCallback2);
      }
      remove
      {
        CertificateValidationCallback2 comparand = this._ClientCertValidation2;
        CertificateValidationCallback2 validationCallback2;
        do
        {
          validationCallback2 = comparand;
          comparand = Interlocked.CompareExchange<CertificateValidationCallback2>(ref this._ClientCertValidation2, validationCallback2 - value, comparand);
        }
        while (comparand != validationCallback2);
      }
    }

    public System.Security.Cryptography.X509Certificates.X509Certificate ClientCertificate
    {
      get
      {
        return this.context.HandshakeState == HandshakeState.Finished ? this.context.ClientSettings.ClientCertificate : (System.Security.Cryptography.X509Certificates.X509Certificate) null;
      }
    }

    public CertificateValidationCallback ClientCertValidationDelegate
    {
      get
      {
        return this._ClientCertValidation;
      }
      set
      {
        this._ClientCertValidation = value;
      }
    }

    public PrivateKeySelectionCallback PrivateKeyCertSelectionDelegate
    {
      get
      {
        return this._PrivateKeySelection;
      }
      set
      {
        this._PrivateKeySelection = value;
      }
    }

    ~SslServerStream()
    {
      this.Dispose(false);
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      this._ClientCertValidation = (CertificateValidationCallback) null;
      this._PrivateKeySelection = (PrivateKeySelectionCallback) null;
    }

    internal override IAsyncResult OnBeginNegotiateHandshake(
      AsyncCallback callback,
      object state)
    {
      if (this.context.HandshakeState != HandshakeState.None)
        this.context.Clear();
      this.context.SupportedCiphers = CipherSuiteFactory.GetSupportedCiphers(this.context.SecurityProtocol);
      this.context.HandshakeState = HandshakeState.Started;
      return this.protocol.BeginReceiveRecord(this.innerStream, callback, state);
    }

    internal override void OnNegotiateHandshakeCallback(IAsyncResult asyncResult)
    {
      this.protocol.EndReceiveRecord(asyncResult);
      if (this.context.LastHandshakeMsg != HandshakeType.ClientHello)
        this.protocol.SendAlert(AlertDescription.UnexpectedMessage);
      this.protocol.SendRecord(HandshakeType.ServerHello);
      this.protocol.SendRecord(HandshakeType.Certificate);
      if (this.context.Negotiating.Cipher.IsExportable)
        this.protocol.SendRecord(HandshakeType.ServerKeyExchange);
      bool flag = false;
      if (this.context.Negotiating.Cipher.IsExportable || ((ServerContext) this.context).ClientCertificateRequired || ((ServerContext) this.context).RequestClientCertificate)
      {
        this.protocol.SendRecord(HandshakeType.CertificateRequest);
        flag = true;
      }
      this.protocol.SendRecord(HandshakeType.ServerHelloDone);
      while (this.context.LastHandshakeMsg != HandshakeType.Finished)
      {
        byte[] record = this.protocol.ReceiveRecord(this.innerStream);
        if (record == null || record.Length == 0)
          throw new TlsException(AlertDescription.HandshakeFailiure, "The client stopped the handshake.");
      }
      if (flag)
      {
        System.Security.Cryptography.X509Certificates.X509Certificate clientCertificate = this.context.ClientSettings.ClientCertificate;
        if (clientCertificate == null && ((ServerContext) this.context).ClientCertificateRequired)
          throw new TlsException(AlertDescription.BadCertificate, "No certificate received from client.");
        if (!this.RaiseClientCertificateValidation(clientCertificate, new int[0]))
          throw new TlsException(AlertDescription.BadCertificate, "Client certificate not accepted.");
      }
      this.protocol.SendChangeCipherSpec();
      this.protocol.SendRecord(HandshakeType.Finished);
      this.context.HandshakeState = HandshakeState.Finished;
      this.context.HandshakeMessages.Reset();
      this.context.ClearKeyInfo();
    }

    internal override System.Security.Cryptography.X509Certificates.X509Certificate OnLocalCertificateSelection(
      System.Security.Cryptography.X509Certificates.X509CertificateCollection clientCertificates,
      System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate,
      string targetHost,
      System.Security.Cryptography.X509Certificates.X509CertificateCollection serverRequestedCertificates)
    {
      throw new NotSupportedException();
    }

    internal override bool OnRemoteCertificateValidation(System.Security.Cryptography.X509Certificates.X509Certificate certificate, int[] errors)
    {
      if (this._ClientCertValidation != null)
        return this._ClientCertValidation(certificate, errors);
      return errors != null && errors.Length == 0;
    }

    internal override bool HaveRemoteValidation2Callback
    {
      get
      {
        return this._ClientCertValidation2 != null;
      }
    }

    internal override ValidationResult OnRemoteCertificateValidation2(
      Mono.Security.X509.X509CertificateCollection collection)
    {
      CertificateValidationCallback2 clientCertValidation2 = this._ClientCertValidation2;
      return clientCertValidation2 != null ? clientCertValidation2(collection) : (ValidationResult) null;
    }

    internal bool RaiseClientCertificateValidation(
      System.Security.Cryptography.X509Certificates.X509Certificate certificate,
      int[] certificateErrors)
    {
      return this.RaiseRemoteCertificateValidation(certificate, certificateErrors);
    }

    internal override AsymmetricAlgorithm OnLocalPrivateKeySelection(
      System.Security.Cryptography.X509Certificates.X509Certificate certificate,
      string targetHost)
    {
      return this._PrivateKeySelection != null ? this._PrivateKeySelection(certificate, targetHost) : (AsymmetricAlgorithm) null;
    }

    internal AsymmetricAlgorithm RaisePrivateKeySelection(
      System.Security.Cryptography.X509Certificates.X509Certificate certificate,
      string targetHost)
    {
      return this.RaiseLocalPrivateKeySelection(certificate, targetHost);
    }
  }
}
