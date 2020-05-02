// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Server.TlsClientHello
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
  internal class TlsClientHello : HandshakeMessage
  {
    private byte[] random;
    private byte[] sessionId;
    private short[] cipherSuites;
    private byte[] compressionMethods;

    public TlsClientHello(Context context, byte[] buffer)
      : base(context, HandshakeType.ClientHello, buffer)
    {
    }

    public override void Update()
    {
      base.Update();
      this.selectCipherSuite();
      this.selectCompressionMethod();
      this.Context.SessionId = this.sessionId;
      this.Context.ClientRandom = this.random;
      this.Context.ProtocolNegotiated = true;
    }

    protected override void ProcessAsSsl3()
    {
      this.ProcessAsTls1();
    }

    protected override void ProcessAsTls1()
    {
      this.processProtocol(this.ReadInt16());
      this.random = this.ReadBytes(32);
      this.sessionId = this.ReadBytes((int) this.ReadByte());
      this.cipherSuites = new short[(int) this.ReadInt16() / 2];
      for (int index = 0; index < this.cipherSuites.Length; ++index)
        this.cipherSuites[index] = this.ReadInt16();
      this.compressionMethods = new byte[(int) this.ReadByte()];
      for (int index = 0; index < this.compressionMethods.Length; ++index)
        this.compressionMethods[index] = this.ReadByte();
    }

    private void processProtocol(short protocol)
    {
      SecurityProtocolType protocol1 = this.Context.DecodeProtocolCode(protocol);
      if ((protocol1 & this.Context.SecurityProtocolFlags) != protocol1 && (this.Context.SecurityProtocolFlags & SecurityProtocolType.Default) != SecurityProtocolType.Default)
        throw new TlsException(AlertDescription.ProtocolVersion, "Incorrect protocol version received from server");
      this.Context.SecurityProtocol = protocol1;
      this.Context.SupportedCiphers.Clear();
      this.Context.SupportedCiphers = (CipherSuiteCollection) null;
      this.Context.SupportedCiphers = CipherSuiteFactory.GetSupportedCiphers(protocol1);
    }

    private void selectCipherSuite()
    {
      for (int index1 = 0; index1 < this.cipherSuites.Length; ++index1)
      {
        int index2;
        if ((index2 = this.Context.SupportedCiphers.IndexOf(this.cipherSuites[index1])) != -1)
        {
          this.Context.Negotiating.Cipher = this.Context.SupportedCiphers[index2];
          break;
        }
      }
      if (this.Context.Negotiating.Cipher == null)
        throw new TlsException(AlertDescription.InsuficientSecurity, "Insuficient Security");
    }

    private void selectCompressionMethod()
    {
      this.Context.CompressionMethod = SecurityCompressionType.None;
    }
  }
}
