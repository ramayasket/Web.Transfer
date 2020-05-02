// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.X509ChainStatusFlags
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security.X509
{
  [Flags]
  [Serializable]
  public enum X509ChainStatusFlags
  {
    InvalidBasicConstraints = 1024, // 0x00000400
    NoError = 0,
    NotSignatureValid = 8,
    NotTimeNested = 2,
    NotTimeValid = 1,
    PartialChain = 65536, // 0x00010000
    UntrustedRoot = 32, // 0x00000020
  }
}
