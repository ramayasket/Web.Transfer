// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.AlertDescription
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security.Protocol.Tls
{
  [Serializable]
  internal enum AlertDescription : byte
  {
    CloseNotify = 0,
    UnexpectedMessage = 10, // 0x0A
    BadRecordMAC = 20, // 0x14
    DecryptionFailed = 21, // 0x15
    RecordOverflow = 22, // 0x16
    DecompressionFailiure = 30, // 0x1E
    HandshakeFailiure = 40, // 0x28
    NoCertificate = 41, // 0x29
    BadCertificate = 42, // 0x2A
    UnsupportedCertificate = 43, // 0x2B
    CertificateRevoked = 44, // 0x2C
    CertificateExpired = 45, // 0x2D
    CertificateUnknown = 46, // 0x2E
    IlegalParameter = 47, // 0x2F
    UnknownCA = 48, // 0x30
    AccessDenied = 49, // 0x31
    DecodeError = 50, // 0x32
    DecryptError = 51, // 0x33
    ExportRestriction = 60, // 0x3C
    ProtocolVersion = 70, // 0x46
    InsuficientSecurity = 71, // 0x47
    InternalError = 80, // 0x50
    UserCancelled = 90, // 0x5A
    NoRenegotiation = 100, // 0x64
  }
}
