// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.SslClientStream
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
  public class SslClientStream : SslStreamBase
  {
    private CertificateValidationCallback _ServerCertValidation;
    private CertificateSelectionCallback _ClientCertSelection;
    private PrivateKeySelectionCallback _PrivateKeySelection;
    private CertificateValidationCallback2 _ServerCertValidation2;

    public SslClientStream(Stream stream, string targetHost, bool ownsStream)
      : this(stream, targetHost, ownsStream, SecurityProtocolType.Default, (System.Security.Cryptography.X509Certificates.X509CertificateCollection) null)
    {
    }

    public SslClientStream(Stream stream, string targetHost, System.Security.Cryptography.X509Certificates.X509Certificate clientCertificate)
      : this(stream, targetHost, false, SecurityProtocolType.Default, new System.Security.Cryptography.X509Certificates.X509CertificateCollection(new System.Security.Cryptography.X509Certificates.X509Certificate[1]
      {
        clientCertificate
      }))
    {
    }

    public SslClientStream(
      Stream stream,
      string targetHost,
      System.Security.Cryptography.X509Certificates.X509CertificateCollection clientCertificates)
      : this(stream, targetHost, false, SecurityProtocolType.Default, clientCertificates)
    {
    }

    public SslClientStream(
      Stream stream,
      string targetHost,
      bool ownsStream,
      SecurityProtocolType securityProtocolType)
      : this(stream, targetHost, ownsStream, securityProtocolType, new System.Security.Cryptography.X509Certificates.X509CertificateCollection())
    {
    }

    public SslClientStream(
      Stream stream,
      string targetHost,
      bool ownsStream,
      SecurityProtocolType securityProtocolType,
      System.Security.Cryptography.X509Certificates.X509CertificateCollection clientCertificates)
      : base(stream, ownsStream)
    {
      switch (targetHost)
      {
        case "":
        case null:
          throw new ArgumentNullException("targetHost is null or an empty string.");
        default:
          this.context = (Context) new ClientContext(this, securityProtocolType, targetHost, clientCertificates);
          this.protocol = (RecordProtocol) new ClientRecordProtocol(this.innerStream, (ClientContext) this.context);
          break;
      }
    }

    internal event CertificateValidationCallback ServerCertValidation
    {
      add
      {
        CertificateValidationCallback comparand = this._ServerCertValidation;
        CertificateValidationCallback validationCallback;
        do
        {
          validationCallback = comparand;
          comparand = Interlocked.CompareExchange<CertificateValidationCallback>(ref this._ServerCertValidation, validationCallback + value, comparand);
        }
        while (comparand != validationCallback);
      }
      remove
      {
        CertificateValidationCallback comparand = this._ServerCertValidation;
        CertificateValidationCallback validationCallback;
        do
        {
          validationCallback = comparand;
          comparand = Interlocked.CompareExchange<CertificateValidationCallback>(ref this._ServerCertValidation, validationCallback - value, comparand);
        }
        while (comparand != validationCallback);
      }
    }

    internal event CertificateSelectionCallback ClientCertSelection
    {
      add
      {
        CertificateSelectionCallback comparand = this._ClientCertSelection;
        CertificateSelectionCallback selectionCallback;
        do
        {
          selectionCallback = comparand;
          comparand = Interlocked.CompareExchange<CertificateSelectionCallback>(ref this._ClientCertSelection, selectionCallback + value, comparand);
        }
        while (comparand != selectionCallback);
      }
      remove
      {
        CertificateSelectionCallback comparand = this._ClientCertSelection;
        CertificateSelectionCallback selectionCallback;
        do
        {
          selectionCallback = comparand;
          comparand = Interlocked.CompareExchange<CertificateSelectionCallback>(ref this._ClientCertSelection, selectionCallback - value, comparand);
        }
        while (comparand != selectionCallback);
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

    public event CertificateValidationCallback2 ServerCertValidation2
    {
      add
      {
        CertificateValidationCallback2 comparand = this._ServerCertValidation2;
        CertificateValidationCallback2 validationCallback2;
        do
        {
          validationCallback2 = comparand;
          comparand = Interlocked.CompareExchange<CertificateValidationCallback2>(ref this._ServerCertValidation2, validationCallback2 + value, comparand);
        }
        while (comparand != validationCallback2);
      }
      remove
      {
        CertificateValidationCallback2 comparand = this._ServerCertValidation2;
        CertificateValidationCallback2 validationCallback2;
        do
        {
          validationCallback2 = comparand;
          comparand = Interlocked.CompareExchange<CertificateValidationCallback2>(ref this._ServerCertValidation2, validationCallback2 - value, comparand);
        }
        while (comparand != validationCallback2);
      }
    }

    internal Stream InputBuffer
    {
      get
      {
        return (Stream) this.inputBuffer;
      }
    }

    public System.Security.Cryptography.X509Certificates.X509CertificateCollection ClientCertificates
    {
      get
      {
        return this.context.ClientSettings.Certificates;
      }
    }

    public System.Security.Cryptography.X509Certificates.X509Certificate SelectedClientCertificate
    {
      get
      {
        return this.context.ClientSettings.ClientCertificate;
      }
    }

    public CertificateValidationCallback ServerCertValidationDelegate
    {
      get
      {
        return this._ServerCertValidation;
      }
      set
      {
        this._ServerCertValidation = value;
      }
    }

    public CertificateSelectionCallback ClientCertSelectionDelegate
    {
      get
      {
        return this._ClientCertSelection;
      }
      set
      {
        this._ClientCertSelection = value;
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

    ~SslClientStream()
    {
      base.Dispose(false);
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      this._ServerCertValidation = (CertificateValidationCallback) null;
      this._ClientCertSelection = (CertificateSelectionCallback) null;
      this._PrivateKeySelection = (PrivateKeySelectionCallback) null;
      this._ServerCertValidation2 = (CertificateValidationCallback2) null;
    }

    internal override IAsyncResult OnBeginNegotiateHandshake(
      AsyncCallback callback,
      object state)
    {
      try
      {
        if (this.context.HandshakeState != HandshakeState.None)
          this.context.Clear();
        this.context.SupportedCiphers = CipherSuiteFactory.GetSupportedCiphers(this.context.SecurityProtocol);
        this.context.HandshakeState = HandshakeState.Started;
        return this.protocol.BeginSendRecord(HandshakeType.ClientHello, callback, state);
      }
      catch (TlsException ex)
      {
        this.protocol.SendAlert(ex.Alert);
        throw new IOException("The authentication or decryption has failed.", (Exception) ex);
      }
      catch (Exception ex)
      {
        this.protocol.SendAlert(AlertDescription.InternalError);
        throw new IOException("The authentication or decryption has failed.", ex);
      }
    }

    private void SafeReceiveRecord(Stream s)
    {
      byte[] record = this.protocol.ReceiveRecord(s);
      if (record == null || record.Length == 0)
        throw new TlsException(AlertDescription.HandshakeFailiure, "The server stopped the handshake.");
    }

    internal override void OnNegotiateHandshakeCallback(IAsyncResult asyncResult)
    {
      this.protocol.EndSendRecord(asyncResult);
      while (this.context.LastHandshakeMsg != HandshakeType.ServerHelloDone)
      {
        this.SafeReceiveRecord(this.innerStream);
        if (this.context.AbbreviatedHandshake && this.context.LastHandshakeMsg == HandshakeType.ServerHello)
          break;
      }
      if (this.context.AbbreviatedHandshake)
      {
        ClientSessionCache.SetContextFromCache(this.context);
        this.context.Negotiating.Cipher.ComputeKeys();
        this.context.Negotiating.Cipher.InitializeCipher();
        this.protocol.SendChangeCipherSpec();
        while (this.context.HandshakeState != HandshakeState.Finished)
          this.SafeReceiveRecord(this.innerStream);
        this.protocol.SendRecord(HandshakeType.Finished);
      }
      else
      {
        bool flag = this.context.ServerSettings.CertificateRequest;
        if (this.context.SecurityProtocol == SecurityProtocolType.Ssl3)
          flag = this.context.ClientSettings.Certificates != null && this.context.ClientSettings.Certificates.Count > 0;
        if (flag)
          this.protocol.SendRecord(HandshakeType.Certificate);
        this.protocol.SendRecord(HandshakeType.ClientKeyExchange);
        this.context.Negotiating.Cipher.InitializeCipher();
        if (flag && this.context.ClientSettings.ClientCertificate != null)
          this.protocol.SendRecord(HandshakeType.CertificateVerify);
        this.protocol.SendChangeCipherSpec();
        this.protocol.SendRecord(HandshakeType.Finished);
        while (this.context.HandshakeState != HandshakeState.Finished)
          this.SafeReceiveRecord(this.innerStream);
      }
      this.context.HandshakeMessages.Reset();
      this.context.ClearKeyInfo();
    }

    internal override System.Security.Cryptography.X509Certificates.X509Certificate OnLocalCertificateSelection(
      System.Security.Cryptography.X509Certificates.X509CertificateCollection clientCertificates,
      System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate,
      string targetHost,
      System.Security.Cryptography.X509Certificates.X509CertificateCollection serverRequestedCertificates)
    {
      return this._ClientCertSelection != null ? this._ClientCertSelection(clientCertificates, serverCertificate, targetHost, serverRequestedCertificates) : (System.Security.Cryptography.X509Certificates.X509Certificate) null;
    }

    internal override bool HaveRemoteValidation2Callback
    {
      get
      {
        return this._ServerCertValidation2 != null;
      }
    }

    internal override ValidationResult OnRemoteCertificateValidation2(
      Mono.Security.X509.X509CertificateCollection collection)
    {
      CertificateValidationCallback2 serverCertValidation2 = this._ServerCertValidation2;
      return serverCertValidation2 != null ? serverCertValidation2(collection) : (ValidationResult) null;
    }

    internal override bool OnRemoteCertificateValidation(System.Security.Cryptography.X509Certificates.X509Certificate certificate, int[] errors)
    {
      if (this._ServerCertValidation != null)
        return this._ServerCertValidation(certificate, errors);
      return errors != null && errors.Length == 0;
    }

    internal virtual bool RaiseServerCertificateValidation(
      System.Security.Cryptography.X509Certificates.X509Certificate certificate,
      int[] certificateErrors)
    {
      return this.RaiseRemoteCertificateValidation(certificate, certificateErrors);
    }

    internal virtual ValidationResult RaiseServerCertificateValidation2(
      Mono.Security.X509.X509CertificateCollection collection)
    {
      return this.RaiseRemoteCertificateValidation2(collection);
    }

    internal System.Security.Cryptography.X509Certificates.X509Certificate RaiseClientCertificateSelection(
      System.Security.Cryptography.X509Certificates.X509CertificateCollection clientCertificates,
      System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate,
      string targetHost,
      System.Security.Cryptography.X509Certificates.X509CertificateCollection serverRequestedCertificates)
    {
      return this.RaiseLocalCertificateSelection(clientCertificates, serverCertificate, targetHost, serverRequestedCertificates);
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
