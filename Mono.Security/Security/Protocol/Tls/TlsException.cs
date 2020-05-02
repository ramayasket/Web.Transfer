// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.TlsException
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Runtime.Serialization;

namespace Mono.Security.Protocol.Tls
{
  [Serializable]
  internal sealed class TlsException : Exception
  {
    private Alert alert;

    internal TlsException(string message)
      : base(message)
    {
    }

    internal TlsException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    internal TlsException(string message, Exception ex)
      : base(message, ex)
    {
    }

    internal TlsException(AlertLevel level, AlertDescription description)
      : this(level, description, Alert.GetAlertMessage(description))
    {
    }

    internal TlsException(AlertLevel level, AlertDescription description, string message)
      : base(message)
    {
      this.alert = new Alert(level, description);
    }

    internal TlsException(AlertDescription description)
      : this(description, Alert.GetAlertMessage(description))
    {
    }

    internal TlsException(AlertDescription description, string message)
      : base(message)
    {
      this.alert = new Alert(description);
    }

    public Alert Alert
    {
      get
      {
        return this.alert;
      }
    }
  }
}
