// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.CertificateSelectionCallback
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Protocol.Tls
{
  public delegate X509Certificate CertificateSelectionCallback(
    X509CertificateCollection clientCertificates,
    X509Certificate serverCertificate,
    string targetHost,
    X509CertificateCollection serverRequestedCertificates);
}
