// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.SecurityParameters
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

namespace Mono.Security.Protocol.Tls
{
  internal class SecurityParameters
  {
    private CipherSuite cipher;
    private byte[] clientWriteMAC;
    private byte[] serverWriteMAC;

    public CipherSuite Cipher
    {
      get
      {
        return this.cipher;
      }
      set
      {
        this.cipher = value;
      }
    }

    public byte[] ClientWriteMAC
    {
      get
      {
        return this.clientWriteMAC;
      }
      set
      {
        this.clientWriteMAC = value;
      }
    }

    public byte[] ServerWriteMAC
    {
      get
      {
        return this.serverWriteMAC;
      }
      set
      {
        this.serverWriteMAC = value;
      }
    }

    public void Clear()
    {
      this.cipher = (CipherSuite) null;
    }
  }
}
