// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.Extensions.KeyUsages
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security.X509.Extensions
{
  [Flags]
  public enum KeyUsages
  {
    digitalSignature = 128, // 0x00000080
    nonRepudiation = 64, // 0x00000040
    keyEncipherment = 32, // 0x00000020
    dataEncipherment = 16, // 0x00000010
    keyAgreement = 8,
    keyCertSign = 4,
    cRLSign = 2,
    encipherOnly = 1,
    decipherOnly = 2048, // 0x00000800
    none = 0,
  }
}
