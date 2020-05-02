// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Ntlm.NtlmFlags
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security.Protocol.Ntlm
{
  [Flags]
  public enum NtlmFlags
  {
    NegotiateUnicode = 1,
    NegotiateOem = 2,
    RequestTarget = 4,
    NegotiateNtlm = 512, // 0x00000200
    NegotiateDomainSupplied = 4096, // 0x00001000
    NegotiateWorkstationSupplied = 8192, // 0x00002000
    NegotiateAlwaysSign = 32768, // 0x00008000
    NegotiateNtlm2Key = 524288, // 0x00080000
    Negotiate128 = 536870912, // 0x20000000
    Negotiate56 = -2147483648, // 0x80000000
  }
}
