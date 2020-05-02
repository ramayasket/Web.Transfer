// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Server.TlsClientFinished
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
  internal class TlsClientFinished : HandshakeMessage
  {
    public TlsClientFinished(Context context, byte[] buffer)
      : base(context, HandshakeType.Finished, buffer)
    {
    }

    protected override void ProcessAsSsl3()
    {
      HashAlgorithm hashAlgorithm = (HashAlgorithm) new SslHandshakeHash(this.Context.MasterSecret);
      TlsStream tlsStream = new TlsStream();
      tlsStream.Write(this.Context.HandshakeMessages.ToArray());
      tlsStream.Write(1129074260);
      hashAlgorithm.TransformFinalBlock(tlsStream.ToArray(), 0, (int) tlsStream.Length);
      tlsStream.Reset();
      if (!HandshakeMessage.Compare(this.ReadBytes((int) this.Length), hashAlgorithm.Hash))
        throw new TlsException(AlertDescription.DecryptError, "Decrypt error.");
    }

    protected override void ProcessAsTls1()
    {
      byte[] buffer1 = this.ReadBytes((int) this.Length);
      HashAlgorithm hashAlgorithm = (HashAlgorithm) new MD5SHA1();
      byte[] array = this.Context.HandshakeMessages.ToArray();
      byte[] buffer2 = this.Context.Current.Cipher.PRF(this.Context.MasterSecret, "client finished", hashAlgorithm.ComputeHash(array, 0, array.Length), 12);
      if (!HandshakeMessage.Compare(buffer1, buffer2))
        throw new TlsException(AlertDescription.DecryptError, "Decrypt error.");
    }
  }
}
