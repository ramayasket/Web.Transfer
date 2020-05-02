// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Context
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Protocol.Tls.Handshake;
using System;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls
{
  internal abstract class Context
  {
    internal const short MAX_FRAGMENT_SIZE = 16384;
    internal const short TLS1_PROTOCOL_CODE = 769;
    internal const short SSL3_PROTOCOL_CODE = 768;
    internal const long UNIX_BASE_TICKS = 621355968000000000;
    private SecurityProtocolType securityProtocol;
    private byte[] sessionId;
    private SecurityCompressionType compressionMethod;
    private TlsServerSettings serverSettings;
    private TlsClientSettings clientSettings;
    private SecurityParameters current;
    private SecurityParameters negotiating;
    private SecurityParameters read;
    private SecurityParameters write;
    private CipherSuiteCollection supportedCiphers;
    private HandshakeType lastHandshakeMsg;
    private HandshakeState handshakeState;
    private bool abbreviatedHandshake;
    private bool receivedConnectionEnd;
    private bool sentConnectionEnd;
    private bool protocolNegotiated;
    private ulong writeSequenceNumber;
    private ulong readSequenceNumber;
    private byte[] clientRandom;
    private byte[] serverRandom;
    private byte[] randomCS;
    private byte[] randomSC;
    private byte[] masterSecret;
    private byte[] clientWriteKey;
    private byte[] serverWriteKey;
    private byte[] clientWriteIV;
    private byte[] serverWriteIV;
    private TlsStream handshakeMessages;
    private readonly RandomNumberGenerator random;
    private RecordProtocol recordProtocol;

    public Context(SecurityProtocolType securityProtocolType)
    {
      this.SecurityProtocol = securityProtocolType;
      this.compressionMethod = SecurityCompressionType.None;
      this.serverSettings = new TlsServerSettings();
      this.clientSettings = new TlsClientSettings();
      this.handshakeMessages = new TlsStream();
      this.sessionId = (byte[]) null;
      this.handshakeState = HandshakeState.None;
      this.random = RandomNumberGenerator.Create();
    }

    public bool AbbreviatedHandshake
    {
      get
      {
        return this.abbreviatedHandshake;
      }
      set
      {
        this.abbreviatedHandshake = value;
      }
    }

    public bool ProtocolNegotiated
    {
      get
      {
        return this.protocolNegotiated;
      }
      set
      {
        this.protocolNegotiated = value;
      }
    }

    public SecurityProtocolType SecurityProtocol
    {
      get
      {
        if ((this.securityProtocol & SecurityProtocolType.Tls) == SecurityProtocolType.Tls || (this.securityProtocol & SecurityProtocolType.Default) == SecurityProtocolType.Default)
          return SecurityProtocolType.Tls;
        if ((this.securityProtocol & SecurityProtocolType.Ssl3) == SecurityProtocolType.Ssl3)
          return SecurityProtocolType.Ssl3;
        throw new NotSupportedException("Unsupported security protocol type");
      }
      set
      {
        this.securityProtocol = value;
      }
    }

    public SecurityProtocolType SecurityProtocolFlags
    {
      get
      {
        return this.securityProtocol;
      }
    }

    public short Protocol
    {
      get
      {
        switch (this.SecurityProtocol)
        {
          case SecurityProtocolType.Default:
          case SecurityProtocolType.Tls:
            return 769;
          case SecurityProtocolType.Ssl3:
            return 768;
          default:
            throw new NotSupportedException("Unsupported security protocol type");
        }
      }
    }

    public byte[] SessionId
    {
      get
      {
        return this.sessionId;
      }
      set
      {
        this.sessionId = value;
      }
    }

    public SecurityCompressionType CompressionMethod
    {
      get
      {
        return this.compressionMethod;
      }
      set
      {
        this.compressionMethod = value;
      }
    }

    public TlsServerSettings ServerSettings
    {
      get
      {
        return this.serverSettings;
      }
    }

    public TlsClientSettings ClientSettings
    {
      get
      {
        return this.clientSettings;
      }
    }

    public HandshakeType LastHandshakeMsg
    {
      get
      {
        return this.lastHandshakeMsg;
      }
      set
      {
        this.lastHandshakeMsg = value;
      }
    }

    public HandshakeState HandshakeState
    {
      get
      {
        return this.handshakeState;
      }
      set
      {
        this.handshakeState = value;
      }
    }

    public bool ReceivedConnectionEnd
    {
      get
      {
        return this.receivedConnectionEnd;
      }
      set
      {
        this.receivedConnectionEnd = value;
      }
    }

    public bool SentConnectionEnd
    {
      get
      {
        return this.sentConnectionEnd;
      }
      set
      {
        this.sentConnectionEnd = value;
      }
    }

    public CipherSuiteCollection SupportedCiphers
    {
      get
      {
        return this.supportedCiphers;
      }
      set
      {
        this.supportedCiphers = value;
      }
    }

    public TlsStream HandshakeMessages
    {
      get
      {
        return this.handshakeMessages;
      }
    }

    public ulong WriteSequenceNumber
    {
      get
      {
        return this.writeSequenceNumber;
      }
      set
      {
        this.writeSequenceNumber = value;
      }
    }

    public ulong ReadSequenceNumber
    {
      get
      {
        return this.readSequenceNumber;
      }
      set
      {
        this.readSequenceNumber = value;
      }
    }

    public byte[] ClientRandom
    {
      get
      {
        return this.clientRandom;
      }
      set
      {
        this.clientRandom = value;
      }
    }

    public byte[] ServerRandom
    {
      get
      {
        return this.serverRandom;
      }
      set
      {
        this.serverRandom = value;
      }
    }

    public byte[] RandomCS
    {
      get
      {
        return this.randomCS;
      }
      set
      {
        this.randomCS = value;
      }
    }

    public byte[] RandomSC
    {
      get
      {
        return this.randomSC;
      }
      set
      {
        this.randomSC = value;
      }
    }

    public byte[] MasterSecret
    {
      get
      {
        return this.masterSecret;
      }
      set
      {
        this.masterSecret = value;
      }
    }

    public byte[] ClientWriteKey
    {
      get
      {
        return this.clientWriteKey;
      }
      set
      {
        this.clientWriteKey = value;
      }
    }

    public byte[] ServerWriteKey
    {
      get
      {
        return this.serverWriteKey;
      }
      set
      {
        this.serverWriteKey = value;
      }
    }

    public byte[] ClientWriteIV
    {
      get
      {
        return this.clientWriteIV;
      }
      set
      {
        this.clientWriteIV = value;
      }
    }

    public byte[] ServerWriteIV
    {
      get
      {
        return this.serverWriteIV;
      }
      set
      {
        this.serverWriteIV = value;
      }
    }

    public RecordProtocol RecordProtocol
    {
      get
      {
        return this.recordProtocol;
      }
      set
      {
        this.recordProtocol = value;
      }
    }

    public int GetUnixTime()
    {
      return (int) ((DateTime.UtcNow.Ticks - 621355968000000000L) / 10000000L);
    }

    public byte[] GetSecureRandomBytes(int count)
    {
      byte[] data = new byte[count];
      this.random.GetNonZeroBytes(data);
      return data;
    }

    public virtual void Clear()
    {
      this.compressionMethod = SecurityCompressionType.None;
      this.serverSettings = new TlsServerSettings();
      this.clientSettings = new TlsClientSettings();
      this.handshakeMessages = new TlsStream();
      this.sessionId = (byte[]) null;
      this.handshakeState = HandshakeState.None;
      this.ClearKeyInfo();
    }

    public virtual void ClearKeyInfo()
    {
      if (this.masterSecret != null)
      {
        Array.Clear((Array) this.masterSecret, 0, this.masterSecret.Length);
        this.masterSecret = (byte[]) null;
      }
      if (this.clientRandom != null)
      {
        Array.Clear((Array) this.clientRandom, 0, this.clientRandom.Length);
        this.clientRandom = (byte[]) null;
      }
      if (this.serverRandom != null)
      {
        Array.Clear((Array) this.serverRandom, 0, this.serverRandom.Length);
        this.serverRandom = (byte[]) null;
      }
      if (this.randomCS != null)
      {
        Array.Clear((Array) this.randomCS, 0, this.randomCS.Length);
        this.randomCS = (byte[]) null;
      }
      if (this.randomSC != null)
      {
        Array.Clear((Array) this.randomSC, 0, this.randomSC.Length);
        this.randomSC = (byte[]) null;
      }
      if (this.clientWriteKey != null)
      {
        Array.Clear((Array) this.clientWriteKey, 0, this.clientWriteKey.Length);
        this.clientWriteKey = (byte[]) null;
      }
      if (this.clientWriteIV != null)
      {
        Array.Clear((Array) this.clientWriteIV, 0, this.clientWriteIV.Length);
        this.clientWriteIV = (byte[]) null;
      }
      if (this.serverWriteKey != null)
      {
        Array.Clear((Array) this.serverWriteKey, 0, this.serverWriteKey.Length);
        this.serverWriteKey = (byte[]) null;
      }
      if (this.serverWriteIV != null)
      {
        Array.Clear((Array) this.serverWriteIV, 0, this.serverWriteIV.Length);
        this.serverWriteIV = (byte[]) null;
      }
      this.handshakeMessages.Reset();
      if (this.securityProtocol == SecurityProtocolType.Ssl3)
      {
      }
    }

    public SecurityProtocolType DecodeProtocolCode(short code)
    {
      switch (code)
      {
        case 768:
          return SecurityProtocolType.Ssl3;
        case 769:
          return SecurityProtocolType.Tls;
        case 771:
          return SecurityProtocolType.Tls;
        default:
          throw new NotSupportedException("Unsupported security protocol type");
      }
    }

    public void ChangeProtocol(short protocol)
    {
      SecurityProtocolType protocol1 = this.DecodeProtocolCode(protocol);
      if ((protocol1 & this.SecurityProtocolFlags) != protocol1 && (this.SecurityProtocolFlags & SecurityProtocolType.Default) != SecurityProtocolType.Default)
        throw new TlsException(AlertDescription.ProtocolVersion, "Incorrect protocol version received from server");
      this.SecurityProtocol = protocol1;
      this.SupportedCiphers.Clear();
      this.SupportedCiphers = (CipherSuiteCollection) null;
      this.SupportedCiphers = CipherSuiteFactory.GetSupportedCiphers(protocol1);
    }

    public SecurityParameters Current
    {
      get
      {
        if (this.current == null)
          this.current = new SecurityParameters();
        if (this.current.Cipher != null)
          this.current.Cipher.Context = this;
        return this.current;
      }
    }

    public SecurityParameters Negotiating
    {
      get
      {
        if (this.negotiating == null)
          this.negotiating = new SecurityParameters();
        if (this.negotiating.Cipher != null)
          this.negotiating.Cipher.Context = this;
        return this.negotiating;
      }
    }

    public SecurityParameters Read
    {
      get
      {
        return this.read;
      }
    }

    public SecurityParameters Write
    {
      get
      {
        return this.write;
      }
    }

    public void StartSwitchingSecurityParameters(bool client)
    {
      if (client)
      {
        this.write = this.negotiating;
        this.read = this.current;
      }
      else
      {
        this.read = this.negotiating;
        this.write = this.current;
      }
      this.current = this.negotiating;
    }

    public void EndSwitchingSecurityParameters(bool client)
    {
      SecurityParameters securityParameters;
      if (client)
      {
        securityParameters = this.read;
        this.read = this.current;
      }
      else
      {
        securityParameters = this.write;
        this.write = this.current;
      }
      securityParameters?.Clear();
      this.negotiating = securityParameters;
    }
  }
}
