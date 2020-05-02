// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.ClientSessionInfo
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security.Protocol.Tls
{
  internal class ClientSessionInfo : IDisposable
  {
    private const int DefaultValidityInterval = 180;
    private static readonly int ValidityInterval;
    private bool disposed;
    private DateTime validuntil;
    private string host;
    private byte[] sid;
    private byte[] masterSecret;

    public ClientSessionInfo(string hostname, byte[] id)
    {
      this.host = hostname;
      this.sid = id;
      this.KeepAlive();
    }

    ~ClientSessionInfo()
    {
      this.Dispose(false);
    }

    public string HostName
    {
      get
      {
        return this.host;
      }
    }

    public byte[] Id
    {
      get
      {
        return this.sid;
      }
    }

    public bool Valid
    {
      get
      {
        return this.masterSecret != null && this.validuntil > DateTime.UtcNow;
      }
    }

    public void GetContext(Context context)
    {
      this.CheckDisposed();
      if (context.MasterSecret == null)
        return;
      this.masterSecret = (byte[]) context.MasterSecret.Clone();
    }

    public void SetContext(Context context)
    {
      this.CheckDisposed();
      if (this.masterSecret == null)
        return;
      context.MasterSecret = (byte[]) this.masterSecret.Clone();
    }

    public void KeepAlive()
    {
      this.CheckDisposed();
      this.validuntil = DateTime.UtcNow.AddSeconds((double) ClientSessionInfo.ValidityInterval);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (!this.disposed)
      {
        this.validuntil = DateTime.MinValue;
        this.host = (string) null;
        this.sid = (byte[]) null;
        if (this.masterSecret != null)
        {
          Array.Clear((Array) this.masterSecret, 0, this.masterSecret.Length);
          this.masterSecret = (byte[]) null;
        }
      }
      this.disposed = true;
    }

    private void CheckDisposed()
    {
      if (this.disposed)
        throw new ObjectDisposedException(Locale.GetText("Cache session information were disposed."));
    }

    static ClientSessionInfo()
    {
      string environmentVariable = Environment.GetEnvironmentVariable("MONO_TLS_SESSION_CACHE_TIMEOUT");
      if (environmentVariable == null)
      {
        ClientSessionInfo.ValidityInterval = 180;
      }
      else
      {
        try
        {
          ClientSessionInfo.ValidityInterval = int.Parse(environmentVariable);
        }
        catch
        {
          ClientSessionInfo.ValidityInterval = 180;
        }
      }
    }
  }
}
