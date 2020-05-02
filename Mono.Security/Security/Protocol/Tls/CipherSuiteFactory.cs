// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.CipherSuiteFactory
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Security.Protocol.Tls
{
  internal class CipherSuiteFactory
  {
    public static CipherSuiteCollection GetSupportedCiphers(
      SecurityProtocolType protocol)
    {
      switch (protocol)
      {
        case SecurityProtocolType.Default:
        case SecurityProtocolType.Tls:
          return CipherSuiteFactory.GetTls1SupportedCiphers();
        case SecurityProtocolType.Ssl3:
          return CipherSuiteFactory.GetSsl3SupportedCiphers();
        default:
          throw new NotSupportedException("Unsupported security protocol type");
      }
    }

    private static CipherSuiteCollection GetTls1SupportedCiphers()
    {
      return new CipherSuiteCollection(SecurityProtocolType.Tls)
      {
        {
          (short) 53,
          "TLS_RSA_WITH_AES_256_CBC_SHA",
          CipherAlgorithmType.Rijndael,
          HashAlgorithmType.Sha1,
          ExchangeAlgorithmType.RsaKeyX,
          false,
          true,
          (byte) 32,
          (byte) 32,
          (short) 256,
          (byte) 16,
          (byte) 16
        },
        {
          (short) 47,
          "TLS_RSA_WITH_AES_128_CBC_SHA",
          CipherAlgorithmType.Rijndael,
          HashAlgorithmType.Sha1,
          ExchangeAlgorithmType.RsaKeyX,
          false,
          true,
          (byte) 16,
          (byte) 16,
          (short) 128,
          (byte) 16,
          (byte) 16
        },
        {
          (short) 10,
          "TLS_RSA_WITH_3DES_EDE_CBC_SHA",
          CipherAlgorithmType.TripleDes,
          HashAlgorithmType.Sha1,
          ExchangeAlgorithmType.RsaKeyX,
          false,
          true,
          (byte) 24,
          (byte) 24,
          (short) 168,
          (byte) 8,
          (byte) 8
        },
        {
          (short) 5,
          "TLS_RSA_WITH_RC4_128_SHA",
          CipherAlgorithmType.Rc4,
          HashAlgorithmType.Sha1,
          ExchangeAlgorithmType.RsaKeyX,
          false,
          false,
          (byte) 16,
          (byte) 16,
          (short) 128,
          (byte) 0,
          (byte) 0
        },
        {
          (short) 4,
          "TLS_RSA_WITH_RC4_128_MD5",
          CipherAlgorithmType.Rc4,
          HashAlgorithmType.Md5,
          ExchangeAlgorithmType.RsaKeyX,
          false,
          false,
          (byte) 16,
          (byte) 16,
          (short) 128,
          (byte) 0,
          (byte) 0
        },
        {
          (short) 9,
          "TLS_RSA_WITH_DES_CBC_SHA",
          CipherAlgorithmType.Des,
          HashAlgorithmType.Sha1,
          ExchangeAlgorithmType.RsaKeyX,
          false,
          true,
          (byte) 8,
          (byte) 8,
          (short) 56,
          (byte) 8,
          (byte) 8
        },
        {
          (short) 3,
          "TLS_RSA_EXPORT_WITH_RC4_40_MD5",
          CipherAlgorithmType.Rc4,
          HashAlgorithmType.Md5,
          ExchangeAlgorithmType.RsaKeyX,
          true,
          false,
          (byte) 5,
          (byte) 16,
          (short) 40,
          (byte) 0,
          (byte) 0
        },
        {
          (short) 6,
          "TLS_RSA_EXPORT_WITH_RC2_CBC_40_MD5",
          CipherAlgorithmType.Rc2,
          HashAlgorithmType.Md5,
          ExchangeAlgorithmType.RsaKeyX,
          true,
          true,
          (byte) 5,
          (byte) 16,
          (short) 40,
          (byte) 8,
          (byte) 8
        },
        {
          (short) 8,
          "TLS_RSA_EXPORT_WITH_DES40_CBC_SHA",
          CipherAlgorithmType.Des,
          HashAlgorithmType.Sha1,
          ExchangeAlgorithmType.RsaKeyX,
          true,
          true,
          (byte) 5,
          (byte) 8,
          (short) 40,
          (byte) 8,
          (byte) 8
        },
        {
          (short) 96,
          "TLS_RSA_EXPORT_WITH_RC4_56_MD5",
          CipherAlgorithmType.Rc4,
          HashAlgorithmType.Md5,
          ExchangeAlgorithmType.RsaKeyX,
          true,
          false,
          (byte) 7,
          (byte) 16,
          (short) 56,
          (byte) 0,
          (byte) 0
        },
        {
          (short) 97,
          "TLS_RSA_EXPORT_WITH_RC2_CBC_56_MD5",
          CipherAlgorithmType.Rc2,
          HashAlgorithmType.Md5,
          ExchangeAlgorithmType.RsaKeyX,
          true,
          true,
          (byte) 7,
          (byte) 16,
          (short) 56,
          (byte) 8,
          (byte) 8
        },
        {
          (short) 98,
          "TLS_RSA_EXPORT_WITH_DES_CBC_56_SHA",
          CipherAlgorithmType.Des,
          HashAlgorithmType.Sha1,
          ExchangeAlgorithmType.RsaKeyX,
          true,
          true,
          (byte) 8,
          (byte) 8,
          (short) 64,
          (byte) 8,
          (byte) 8
        },
        {
          (short) 100,
          "TLS_RSA_EXPORT_WITH_RC4_56_SHA",
          CipherAlgorithmType.Rc4,
          HashAlgorithmType.Sha1,
          ExchangeAlgorithmType.RsaKeyX,
          true,
          false,
          (byte) 7,
          (byte) 16,
          (short) 56,
          (byte) 0,
          (byte) 0
        }
      };
    }

    private static CipherSuiteCollection GetSsl3SupportedCiphers()
    {
      return new CipherSuiteCollection(SecurityProtocolType.Ssl3)
      {
        {
          (short) 53,
          "SSL_RSA_WITH_AES_256_CBC_SHA",
          CipherAlgorithmType.Rijndael,
          HashAlgorithmType.Sha1,
          ExchangeAlgorithmType.RsaKeyX,
          false,
          true,
          (byte) 32,
          (byte) 32,
          (short) 256,
          (byte) 16,
          (byte) 16
        },
        {
          (short) 10,
          "SSL_RSA_WITH_3DES_EDE_CBC_SHA",
          CipherAlgorithmType.TripleDes,
          HashAlgorithmType.Sha1,
          ExchangeAlgorithmType.RsaKeyX,
          false,
          true,
          (byte) 24,
          (byte) 24,
          (short) 168,
          (byte) 8,
          (byte) 8
        },
        {
          (short) 5,
          "SSL_RSA_WITH_RC4_128_SHA",
          CipherAlgorithmType.Rc4,
          HashAlgorithmType.Sha1,
          ExchangeAlgorithmType.RsaKeyX,
          false,
          false,
          (byte) 16,
          (byte) 16,
          (short) 128,
          (byte) 0,
          (byte) 0
        },
        {
          (short) 4,
          "SSL_RSA_WITH_RC4_128_MD5",
          CipherAlgorithmType.Rc4,
          HashAlgorithmType.Md5,
          ExchangeAlgorithmType.RsaKeyX,
          false,
          false,
          (byte) 16,
          (byte) 16,
          (short) 128,
          (byte) 0,
          (byte) 0
        },
        {
          (short) 9,
          "SSL_RSA_WITH_DES_CBC_SHA",
          CipherAlgorithmType.Des,
          HashAlgorithmType.Sha1,
          ExchangeAlgorithmType.RsaKeyX,
          false,
          true,
          (byte) 8,
          (byte) 8,
          (short) 56,
          (byte) 8,
          (byte) 8
        },
        {
          (short) 3,
          "SSL_RSA_EXPORT_WITH_RC4_40_MD5",
          CipherAlgorithmType.Rc4,
          HashAlgorithmType.Md5,
          ExchangeAlgorithmType.RsaKeyX,
          true,
          false,
          (byte) 5,
          (byte) 16,
          (short) 40,
          (byte) 0,
          (byte) 0
        },
        {
          (short) 6,
          "SSL_RSA_EXPORT_WITH_RC2_CBC_40_MD5",
          CipherAlgorithmType.Rc2,
          HashAlgorithmType.Md5,
          ExchangeAlgorithmType.RsaKeyX,
          true,
          true,
          (byte) 5,
          (byte) 16,
          (short) 40,
          (byte) 8,
          (byte) 8
        },
        {
          (short) 8,
          "SSL_RSA_EXPORT_WITH_DES40_CBC_SHA",
          CipherAlgorithmType.Des,
          HashAlgorithmType.Sha1,
          ExchangeAlgorithmType.RsaKeyX,
          true,
          true,
          (byte) 5,
          (byte) 8,
          (short) 40,
          (byte) 8,
          (byte) 8
        },
        {
          (short) 96,
          "SSL_RSA_EXPORT_WITH_RC4_56_MD5",
          CipherAlgorithmType.Rc4,
          HashAlgorithmType.Md5,
          ExchangeAlgorithmType.RsaKeyX,
          true,
          false,
          (byte) 7,
          (byte) 16,
          (short) 56,
          (byte) 0,
          (byte) 0
        },
        {
          (short) 97,
          "SSL_RSA_EXPORT_WITH_RC2_CBC_56_MD5",
          CipherAlgorithmType.Rc2,
          HashAlgorithmType.Md5,
          ExchangeAlgorithmType.RsaKeyX,
          true,
          true,
          (byte) 7,
          (byte) 16,
          (short) 56,
          (byte) 8,
          (byte) 8
        },
        {
          (short) 98,
          "SSL_RSA_EXPORT_WITH_DES_CBC_56_SHA",
          CipherAlgorithmType.Des,
          HashAlgorithmType.Sha1,
          ExchangeAlgorithmType.RsaKeyX,
          true,
          true,
          (byte) 8,
          (byte) 8,
          (short) 64,
          (byte) 8,
          (byte) 8
        },
        {
          (short) 100,
          "SSL_RSA_EXPORT_WITH_RC4_56_SHA",
          CipherAlgorithmType.Rc4,
          HashAlgorithmType.Sha1,
          ExchangeAlgorithmType.RsaKeyX,
          true,
          false,
          (byte) 7,
          (byte) 16,
          (short) 56,
          (byte) 0,
          (byte) 0
        }
      };
    }
  }
}
