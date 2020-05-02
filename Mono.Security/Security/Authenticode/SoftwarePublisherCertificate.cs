// Decompiled with JetBrains decompiler
// Type: Mono.Security.Authenticode.SoftwarePublisherCertificate
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.X509;
using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security.Authenticode
{
  public class SoftwarePublisherCertificate
  {
    private const string header = "-----BEGIN PKCS7-----";
    private const string footer = "-----END PKCS7-----";
    private PKCS7.SignedData pkcs7;

    public SoftwarePublisherCertificate()
    {
      this.pkcs7 = new PKCS7.SignedData();
      this.pkcs7.ContentInfo.ContentType = "1.2.840.113549.1.7.1";
    }

    public SoftwarePublisherCertificate(byte[] data)
      : this()
    {
      if (data == null)
        throw new ArgumentNullException(nameof (data));
      PKCS7.ContentInfo contentInfo = new PKCS7.ContentInfo(data);
      if (contentInfo.ContentType != "1.2.840.113549.1.7.2")
        throw new ArgumentException(Locale.GetText("Unsupported ContentType"));
      this.pkcs7 = new PKCS7.SignedData(contentInfo.Content);
    }

    public X509CertificateCollection Certificates
    {
      get
      {
        return this.pkcs7.Certificates;
      }
    }

    public ArrayList Crls
    {
      get
      {
        return this.pkcs7.Crls;
      }
    }

    public byte[] GetBytes()
    {
      PKCS7.ContentInfo contentInfo = new PKCS7.ContentInfo("1.2.840.113549.1.7.2");
      contentInfo.Content.Add(this.pkcs7.ASN1);
      return contentInfo.GetBytes();
    }

    public static SoftwarePublisherCertificate CreateFromFile(
      string filename)
    {
      if (filename == null)
        throw new ArgumentNullException(nameof (filename));
      byte[] numArray = (byte[]) null;
      using (FileStream fileStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        numArray = new byte[fileStream.Length];
        fileStream.Read(numArray, 0, numArray.Length);
        fileStream.Close();
      }
      if (numArray.Length < 2)
        return (SoftwarePublisherCertificate) null;
      if (numArray[0] != (byte) 48)
      {
        try
        {
          numArray = SoftwarePublisherCertificate.PEM(numArray);
        }
        catch (Exception ex)
        {
          throw new CryptographicException("Invalid encoding", ex);
        }
      }
      return new SoftwarePublisherCertificate(numArray);
    }

    private static byte[] PEM(byte[] data)
    {
      string str = data[1] != (byte) 0 ? Encoding.ASCII.GetString(data) : Encoding.Unicode.GetString(data);
      int startIndex = str.IndexOf("-----BEGIN PKCS7-----") + "-----BEGIN PKCS7-----".Length;
      int num = str.IndexOf("-----END PKCS7-----", startIndex);
      return Convert.FromBase64String(startIndex == -1 || num == -1 ? str : str.Substring(startIndex, num - startIndex));
    }
  }
}
