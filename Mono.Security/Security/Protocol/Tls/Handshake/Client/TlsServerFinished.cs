// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Client.TlsServerFinished
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
  internal class TlsServerFinished : HandshakeMessage
  {
    private static byte[] Ssl3Marker = new byte[4]
    {
      (byte) 83,
      (byte) 82,
      (byte) 86,
      (byte) 82
    };

    public TlsServerFinished(Context context, byte[] buffer)
      : base(context, HandshakeType.Finished, buffer)
    {
    }

    public override void Update()
    {
      base.Update();
      this.Context.HandshakeState = HandshakeState.Finished;
    }

    protected override void ProcessAsSsl3()
    {
      HashAlgorithm hashAlgorithm = (HashAlgorithm) new SslHandshakeHash(this.Context.MasterSecret);
      byte[] array = this.Context.HandshakeMessages.ToArray();
      hashAlgorithm.TransformBlock(array, 0, array.Length, array, 0);
      hashAlgorithm.TransformBlock(TlsServerFinished.Ssl3Marker, 0, TlsServerFinished.Ssl3Marker.Length, TlsServerFinished.Ssl3Marker, 0);
      hashAlgorithm.TransformFinalBlock(CipherSuite.EmptyArray, 0, 0);
      byte[] buffer2 = this.ReadBytes((int) this.Length);
      if (!HandshakeMessage.Compare(hashAlgorithm.Hash, buffer2))
        throw new TlsException(AlertDescription.InsuficientSecurity, "Invalid ServerFinished message received.");
    }

    protected override void ProcessAsTls1()
    {
      byte[] buffer2 = this.ReadBytes((int) this.Length);
      HashAlgorithm hashAlgorithm = (HashAlgorithm) new MD5SHA1();
      byte[] array = this.Context.HandshakeMessages.ToArray();
      if (!HandshakeMessage.Compare(this.Context.Current.Cipher.PRF(this.Context.MasterSecret, "server finished", hashAlgorithm.ComputeHash(array, 0, array.Length), 12), buffer2))
        throw new TlsException("Invalid ServerFinished message received.");
    }
  }
}
