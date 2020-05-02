// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.SecurityProtocolType
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security.Protocol.Tls
{
  [Flags]
  [Serializable]
  public enum SecurityProtocolType
  {
    Default = -1073741824, // 0xC0000000
    Ssl2 = 12, // 0x0000000C
    Ssl3 = 48, // 0x00000030
    Tls = 192, // 0x000000C0
  }
}
