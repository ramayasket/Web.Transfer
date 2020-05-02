// Decompiled with JetBrains decompiler
// Type: mycloud.SecureConnectionResults
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.Net.Security;

namespace mycloud
{
  public class SecureConnectionResults
  {
    private SslStreamM secureStream;
    private Exception asyncException;

    internal SecureConnectionResults(SslStreamM sslStream)
    {
      this.secureStream = sslStream;
    }

    internal SecureConnectionResults(Exception exception)
    {
      this.asyncException = exception;
    }

    public Exception AsyncException
    {
      get
      {
        return this.asyncException;
      }
    }

    public SslStreamM SecureStream
    {
      get
      {
        return this.secureStream;
      }
    }
  }
}
