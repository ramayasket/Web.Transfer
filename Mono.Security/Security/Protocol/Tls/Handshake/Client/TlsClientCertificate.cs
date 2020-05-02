// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Client.TlsClientCertificate
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
  internal class TlsClientCertificate : HandshakeMessage
  {
    private bool clientCertSelected;
    private X509Certificate clientCert;

    public TlsClientCertificate(Context context)
      : base(context, HandshakeType.Certificate)
    {
    }

    public X509Certificate ClientCertificate
    {
      get
      {
        if (!this.clientCertSelected)
        {
          this.GetClientCertificate();
          this.clientCertSelected = true;
        }
        return this.clientCert;
      }
    }

    public override void Update()
    {
      base.Update();
      this.Reset();
    }

    private void GetClientCertificate()
    {
      ClientContext context = (ClientContext) this.Context;
      if (context.ClientSettings.Certificates != null && context.ClientSettings.Certificates.Count > 0)
        this.clientCert = context.SslStream.RaiseClientCertificateSelection(this.Context.ClientSettings.Certificates, new X509Certificate(this.Context.ServerSettings.Certificates[0].RawData), this.Context.ClientSettings.TargetHost, (X509CertificateCollection) null);
      context.ClientSettings.ClientCertificate = this.clientCert;
    }

    private void SendCertificates()
    {
      TlsStream tlsStream = new TlsStream();
      for (X509Certificate cert = this.ClientCertificate; cert != null; cert = this.FindParentCertificate(cert))
      {
        byte[] rawCertData = cert.GetRawCertData();
        tlsStream.WriteInt24(rawCertData.Length);
        tlsStream.Write(rawCertData);
      }
      this.WriteInt24((int) tlsStream.Length);
      this.Write(tlsStream.ToArray());
    }

    protected override void ProcessAsSsl3()
    {
      if (this.ClientCertificate == null)
        return;
      this.SendCertificates();
    }

    protected override void ProcessAsTls1()
    {
      if (this.ClientCertificate != null)
        this.SendCertificates();
      else
        this.WriteInt24(0);
    }

#pragma warning disable CS0618 // Type or member is obsolete

    private X509Certificate FindParentCertificate(X509Certificate cert)
    {
      if (cert.GetName() == cert.GetIssuerName())
        return (X509Certificate) null;
      foreach (X509Certificate certificate in this.Context.ClientSettings.Certificates)
      {
        if (cert.GetName() == cert.GetIssuerName())
          return certificate;
      }
      return (X509Certificate) null;
    }
  }
}
