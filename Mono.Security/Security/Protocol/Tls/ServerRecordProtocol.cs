// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.ServerRecordProtocol
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Protocol.Tls.Handshake;
using Mono.Security.Protocol.Tls.Handshake.Server;
using System;
using System.Globalization;
using System.IO;

namespace Mono.Security.Protocol.Tls
{
  internal class ServerRecordProtocol : RecordProtocol
  {
    public ServerRecordProtocol(Stream innerStream, ServerContext context)
      : base(innerStream, (Context) context)
    {
    }

    public override HandshakeMessage GetMessage(HandshakeType type)
    {
      return this.createServerHandshakeMessage(type);
    }

    protected override void ProcessHandshakeMessage(TlsStream handMsg)
    {
      HandshakeType type = (HandshakeType) handMsg.ReadByte();
      int count = handMsg.ReadInt24();
      byte[] buffer = new byte[count];
      handMsg.Read(buffer, 0, count);
      HandshakeMessage handshakeMessage = this.createClientHandshakeMessage(type, buffer);
      handshakeMessage.Process();
      this.Context.LastHandshakeMsg = type;
      if (handshakeMessage == null)
        return;
      handshakeMessage.Update();
      this.Context.HandshakeMessages.WriteByte((byte) type);
      this.Context.HandshakeMessages.WriteInt24(count);
      this.Context.HandshakeMessages.Write(buffer, 0, buffer.Length);
    }

    private HandshakeMessage createClientHandshakeMessage(
      HandshakeType type,
      byte[] buffer)
    {
      HandshakeType handshakeType = type;
      switch (handshakeType)
      {
        case HandshakeType.CertificateVerify:
          return (HandshakeMessage) new TlsClientCertificateVerify(this.context, buffer);
        case HandshakeType.ClientKeyExchange:
          return (HandshakeMessage) new TlsClientKeyExchange(this.context, buffer);
        case HandshakeType.Finished:
          return (HandshakeMessage) new TlsClientFinished(this.context, buffer);
        default:
          if (handshakeType == HandshakeType.ClientHello)
            return (HandshakeMessage) new TlsClientHello(this.context, buffer);
          if (handshakeType == HandshakeType.Certificate)
            return (HandshakeMessage) new TlsClientCertificate(this.context, buffer);
          throw new TlsException(AlertDescription.UnexpectedMessage, string.Format((IFormatProvider) CultureInfo.CurrentUICulture, "Unknown server handshake message received ({0})", (object) type.ToString()));
      }
    }

    private HandshakeMessage createServerHandshakeMessage(HandshakeType type)
    {
      HandshakeType handshakeType = type;
      switch (handshakeType)
      {
        case HandshakeType.Certificate:
          return (HandshakeMessage) new TlsServerCertificate(this.context);
        case HandshakeType.ServerKeyExchange:
          return (HandshakeMessage) new TlsServerKeyExchange(this.context);
        case HandshakeType.CertificateRequest:
          return (HandshakeMessage) new TlsServerCertificateRequest(this.context);
        case HandshakeType.ServerHelloDone:
          return (HandshakeMessage) new TlsServerHelloDone(this.context);
        case HandshakeType.Finished:
          return (HandshakeMessage) new TlsServerFinished(this.context);
        default:
          switch (handshakeType)
          {
            case HandshakeType.HelloRequest:
              this.SendRecord(HandshakeType.ClientHello);
              return (HandshakeMessage) null;
            case HandshakeType.ServerHello:
              return (HandshakeMessage) new TlsServerHello(this.context);
            default:
              throw new InvalidOperationException("Unknown server handshake message type: " + type.ToString());
          }
      }
    }
  }
}
