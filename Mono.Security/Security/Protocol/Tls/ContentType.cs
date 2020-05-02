// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.ContentType
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security.Protocol.Tls
{
  [Serializable]
  internal enum ContentType : byte
  {
    ChangeCipherSpec = 20, // 0x14
    Alert = 21, // 0x15
    Handshake = 22, // 0x16
    ApplicationData = 23, // 0x17
  }
}
