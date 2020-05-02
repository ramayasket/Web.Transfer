// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.ClientRecordProtocol
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Protocol.Tls.Handshake;
using Mono.Security.Protocol.Tls.Handshake.Client;
using System;
using System.Globalization;
using System.IO;

namespace Mono.Security.Protocol.Tls
{
  internal class ClientRecordProtocol : RecordProtocol
  {
    public ClientRecordProtocol(Stream innerStream, ClientContext context)
      : base(innerStream, (Context) context)
    {
    }

    public override HandshakeMessage GetMessage(HandshakeType type)
    {
      return this.createClientHandshakeMessage(type);
    }

    protected override void ProcessHandshakeMessage(TlsStream handMsg)
    {
      HandshakeType type = (HandshakeType) handMsg.ReadByte();
      int count = handMsg.ReadInt24();
      byte[] buffer = (byte[]) null;
      if (count > 0)
      {
        buffer = new byte[count];
        handMsg.Read(buffer, 0, count);
      }
      HandshakeMessage handshakeMessage = this.createServerHandshakeMessage(type, buffer);
      handshakeMessage?.Process();
      this.Context.LastHandshakeMsg = type;
      if (handshakeMessage == null)
        return;
      handshakeMessage.Update();
      this.Context.HandshakeMessages.WriteByte((byte) type);
      this.Context.HandshakeMessages.WriteInt24(count);
      if (count <= 0)
        return;
      this.Context.HandshakeMessages.Write(buffer, 0, buffer.Length);
    }

    private HandshakeMessage createClientHandshakeMessage(HandshakeType type)
    {
      HandshakeType handshakeType = type;
      switch (handshakeType)
      {
        case HandshakeType.CertificateVerify:
          return (HandshakeMessage) new TlsClientCertificateVerify(this.context);
        case HandshakeType.ClientKeyExchange:
          return (HandshakeMessage) new TlsClientKeyExchange(this.context);
        case HandshakeType.Finished:
          return (HandshakeMessage) new TlsClientFinished(this.context);
        default:
          if (handshakeType == HandshakeType.ClientHello)
            return (HandshakeMessage) new TlsClientHello(this.context);
          if (handshakeType == HandshakeType.Certificate)
            return (HandshakeMessage) new TlsClientCertificate(this.context);
          throw new InvalidOperationException("Unknown client handshake message type: " + type.ToString());
      }
    }

    private HandshakeMessage createServerHandshakeMessage(
      HandshakeType type,
      byte[] buffer)
    {
      ClientContext context = (ClientContext) this.context;
      HandshakeType handshakeType = type;
      switch (handshakeType)
      {
        case HandshakeType.Certificate:
          return (HandshakeMessage) new TlsServerCertificate(this.context, buffer);
        case HandshakeType.ServerKeyExchange:
          return (HandshakeMessage) new TlsServerKeyExchange(this.context, buffer);
        case HandshakeType.CertificateRequest:
          return (HandshakeMessage) new TlsServerCertificateRequest(this.context, buffer);
        case HandshakeType.ServerHelloDone:
          return (HandshakeMessage) new TlsServerHelloDone(this.context, buffer);
        case HandshakeType.Finished:
          return (HandshakeMessage) new TlsServerFinished(this.context, buffer);
        default:
          switch (handshakeType)
          {
            case HandshakeType.HelloRequest:
              if (context.HandshakeState != HandshakeState.Started)
                context.HandshakeState = HandshakeState.None;
              else
                this.SendAlert(AlertLevel.Warning, AlertDescription.NoRenegotiation);
              return (HandshakeMessage) null;
            case HandshakeType.ServerHello:
              return (HandshakeMessage) new TlsServerHello(this.context, buffer);
            default:
              throw new TlsException(AlertDescription.UnexpectedMessage, string.Format((IFormatProvider) CultureInfo.CurrentUICulture, "Unknown server handshake message received ({0})", (object) type.ToString()));
          }
      }
    }
  }
}
