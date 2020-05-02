// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.Extensions.NetscapeCertTypeExtension
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Globalization;
using System.Text;

namespace Mono.Security.X509.Extensions
{
  public class NetscapeCertTypeExtension : X509Extension
  {
    private int ctbits;

    public NetscapeCertTypeExtension()
    {
      this.extnOid = "2.16.840.1.113730.1.1";
    }

    public NetscapeCertTypeExtension(ASN1 asn1)
      : base(asn1)
    {
    }

    public NetscapeCertTypeExtension(X509Extension extension)
      : base(extension)
    {
    }

    protected override void Decode()
    {
      ASN1 asN1 = new ASN1(this.extnValue.Value);
      if (asN1.Tag != (byte) 3)
        throw new ArgumentException("Invalid NetscapeCertType extension");
      int num = 1;
      while (num < asN1.Value.Length)
        this.ctbits = (this.ctbits << 8) + (int) asN1.Value[num++];
    }

    public override string Name
    {
      get
      {
        return "NetscapeCertType";
      }
    }

    public bool Support(NetscapeCertTypeExtension.CertTypes usage)
    {
      int int32 = Convert.ToInt32((object) usage, (IFormatProvider) CultureInfo.InvariantCulture);
      return (int32 & this.ctbits) == int32;
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      if (this.Support(NetscapeCertTypeExtension.CertTypes.SslClient))
        stringBuilder.Append("SSL Client Authentication");
      if (this.Support(NetscapeCertTypeExtension.CertTypes.SslServer))
      {
        if (stringBuilder.Length > 0)
          stringBuilder.Append(" , ");
        stringBuilder.Append("SSL Server Authentication");
      }
      if (this.Support(NetscapeCertTypeExtension.CertTypes.Smime))
      {
        if (stringBuilder.Length > 0)
          stringBuilder.Append(" , ");
        stringBuilder.Append("SMIME");
      }
      if (this.Support(NetscapeCertTypeExtension.CertTypes.ObjectSigning))
      {
        if (stringBuilder.Length > 0)
          stringBuilder.Append(" , ");
        stringBuilder.Append("Object Signing");
      }
      if (this.Support(NetscapeCertTypeExtension.CertTypes.SslCA))
      {
        if (stringBuilder.Length > 0)
          stringBuilder.Append(" , ");
        stringBuilder.Append("SSL CA");
      }
      if (this.Support(NetscapeCertTypeExtension.CertTypes.SmimeCA))
      {
        if (stringBuilder.Length > 0)
          stringBuilder.Append(" , ");
        stringBuilder.Append("SMIME CA");
      }
      if (this.Support(NetscapeCertTypeExtension.CertTypes.ObjectSigningCA))
      {
        if (stringBuilder.Length > 0)
          stringBuilder.Append(" , ");
        stringBuilder.Append("Object Signing CA");
      }
      stringBuilder.Append("(");
      stringBuilder.Append(this.ctbits.ToString("X2", (IFormatProvider) CultureInfo.InvariantCulture));
      stringBuilder.Append(")");
      stringBuilder.Append(Environment.NewLine);
      return stringBuilder.ToString();
    }

    [Flags]
    public enum CertTypes
    {
      SslClient = 128, // 0x00000080
      SslServer = 64, // 0x00000040
      Smime = 32, // 0x00000020
      ObjectSigning = 16, // 0x00000010
      SslCA = 4,
      SmimeCA = 2,
      ObjectSigningCA = 1,
    }
  }
}
