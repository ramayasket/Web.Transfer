// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Client.TlsServerHello
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
  internal class TlsServerHello : HandshakeMessage
  {
    private SecurityCompressionType compressionMethod;
    private byte[] random;
    private byte[] sessionId;
    private CipherSuite cipherSuite;

    public TlsServerHello(Context context, byte[] buffer)
      : base(context, HandshakeType.ServerHello, buffer)
    {
    }

    public override void Update()
    {
      base.Update();
      this.Context.SessionId = this.sessionId;
      this.Context.ServerRandom = this.random;
      this.Context.Negotiating.Cipher = this.cipherSuite;
      this.Context.CompressionMethod = this.compressionMethod;
      this.Context.ProtocolNegotiated = true;
      int length1 = this.Context.ClientRandom.Length;
      int length2 = this.Context.ServerRandom.Length;
      int length3 = length1 + length2;
      byte[] numArray1 = new byte[length3];
      Buffer.BlockCopy((Array) this.Context.ClientRandom, 0, (Array) numArray1, 0, length1);
      Buffer.BlockCopy((Array) this.Context.ServerRandom, 0, (Array) numArray1, length1, length2);
      this.Context.RandomCS = numArray1;
      byte[] numArray2 = new byte[length3];
      Buffer.BlockCopy((Array) this.Context.ServerRandom, 0, (Array) numArray2, 0, length2);
      Buffer.BlockCopy((Array) this.Context.ClientRandom, 0, (Array) numArray2, length2, length1);
      this.Context.RandomSC = numArray2;
    }

    protected override void ProcessAsSsl3()
    {
      this.ProcessAsTls1();
    }

    protected override void ProcessAsTls1()
    {
      this.processProtocol(this.ReadInt16());
      this.random = this.ReadBytes(32);
      int count = (int) this.ReadByte();
      if (count > 0)
      {
        this.sessionId = this.ReadBytes(count);
        ClientSessionCache.Add(this.Context.ClientSettings.TargetHost, this.sessionId);
        this.Context.AbbreviatedHandshake = HandshakeMessage.Compare(this.sessionId, this.Context.SessionId);
      }
      else
        this.Context.AbbreviatedHandshake = false;
      short code = this.ReadInt16();
      if (this.Context.SupportedCiphers.IndexOf(code) == -1)
        throw new TlsException(AlertDescription.InsuficientSecurity, "Invalid cipher suite received from server");
      this.cipherSuite = this.Context.SupportedCiphers[code];
      this.compressionMethod = (SecurityCompressionType) this.ReadByte();
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
  }
}
