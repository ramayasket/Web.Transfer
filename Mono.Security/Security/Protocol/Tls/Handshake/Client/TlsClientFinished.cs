// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Client.TlsClientFinished
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
  internal class TlsClientFinished : HandshakeMessage
  {
    private static byte[] Ssl3Marker = new byte[4]
    {
      (byte) 67,
      (byte) 76,
      (byte) 78,
      (byte) 84
    };

    public TlsClientFinished(Context context)
      : base(context, HandshakeType.Finished)
    {
    }

    public override void Update()
    {
      base.Update();
      this.Reset();
    }

    protected override void ProcessAsSsl3()
    {
      HashAlgorithm hashAlgorithm = (HashAlgorithm) new SslHandshakeHash(this.Context.MasterSecret);
      byte[] array = this.Context.HandshakeMessages.ToArray();
      hashAlgorithm.TransformBlock(array, 0, array.Length, array, 0);
      hashAlgorithm.TransformBlock(TlsClientFinished.Ssl3Marker, 0, TlsClientFinished.Ssl3Marker.Length, TlsClientFinished.Ssl3Marker, 0);
      hashAlgorithm.TransformFinalBlock(CipherSuite.EmptyArray, 0, 0);
      this.Write(hashAlgorithm.Hash);
    }

    protected override void ProcessAsTls1()
    {
      HashAlgorithm hashAlgorithm = (HashAlgorithm) new MD5SHA1();
      byte[] array = this.Context.HandshakeMessages.ToArray();
      this.Write(this.Context.Write.Cipher.PRF(this.Context.MasterSecret, "client finished", hashAlgorithm.ComputeHash(array, 0, array.Length), 12));
    }
  }
}
