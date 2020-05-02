// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.HttpsClientStream
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Protocol.Tls
{
  internal class HttpsClientStream : SslClientStream
  {
    private HttpWebRequest _request;
    private int _status;

    public HttpsClientStream(
      Stream stream,
      X509CertificateCollection clientCertificates,
      HttpWebRequest request,
      byte[] buffer)
      : base(stream, request.Address.Host, false, (SecurityProtocolType) ServicePointManager.SecurityProtocol, clientCertificates)
    {
      this._request = request;
      this._status = 0;
      if (buffer != null)
        this.InputBuffer.Write(buffer, 0, buffer.Length);
      this.CheckCertRevocationStatus = ServicePointManager.CheckCertificateRevocationList;
      this.ClientCertSelection += (CertificateSelectionCallback) ((clientCerts, serverCertificate, targetHost, serverRequestedCertificates) => clientCerts == null || clientCerts.Count == 0 ? (X509Certificate) null : clientCerts[0]);
      this.PrivateKeySelection += (PrivateKeySelectionCallback) ((certificate, targetHost) => !(certificate is X509Certificate2 x509Certificate2) ? (AsymmetricAlgorithm) null : x509Certificate2.PrivateKey);
    }

    public bool TrustFailure
    {
      get
      {
        switch (this._status)
        {
          case -2146762487:
          case -2146762486:
            return true;
          default:
            return false;
        }
      }
    }

#pragma warning disable CS0618 // Type or member is obsolete

    internal override bool RaiseServerCertificateValidation(
      X509Certificate certificate,
      int[] certificateErrors)
    {
      bool flag = certificateErrors.Length > 0;
      this._status = !flag ? 0 : certificateErrors[0];
      if (ServicePointManager.CertificatePolicy != null)
      {
        if (!ServicePointManager.CertificatePolicy.CheckValidationResult(this._request.ServicePoint, certificate, (WebRequest) this._request, this._status))
          return false;
        flag = true;
      }
      if (this.HaveRemoteValidation2Callback)
        return flag;
      RemoteCertificateValidationCallback validationCallback = ServicePointManager.ServerCertificateValidationCallback;
      if (validationCallback == null)
        return flag;
      SslPolicyErrors sslPolicyErrors = SslPolicyErrors.None;
      foreach (int certificateError in certificateErrors)
      {
        switch (certificateError)
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
      X509Certificate2 certificate1 = new X509Certificate2(certificate.GetRawCertData());
      X509Chain chain = new X509Chain();
      if (!chain.Build(certificate1))
        sslPolicyErrors |= SslPolicyErrors.RemoteCertificateChainErrors;
      return validationCallback((object) this._request, (X509Certificate) certificate1, chain, sslPolicyErrors);
    }
  }
}
