// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.HandshakeType
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security.Protocol.Tls.Handshake
{
  [Serializable]
  internal enum HandshakeType : byte
  {
    HelloRequest = 0,
    ClientHello = 1,
    ServerHello = 2,
    Certificate = 11, // 0x0B
    ServerKeyExchange = 12, // 0x0C
    CertificateRequest = 13, // 0x0D
    ServerHelloDone = 14, // 0x0E
    CertificateVerify = 15, // 0x0F
    ClientKeyExchange = 16, // 0x10
    Finished = 20, // 0x14
    None = 255, // 0xFF
  }
}
