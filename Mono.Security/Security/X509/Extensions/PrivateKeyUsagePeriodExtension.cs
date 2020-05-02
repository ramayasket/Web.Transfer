// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.Extensions.PrivateKeyUsagePeriodExtension
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Globalization;
using System.Text;

namespace Mono.Security.X509.Extensions
{
  public class PrivateKeyUsagePeriodExtension : X509Extension
  {
    private DateTime notBefore;
    private DateTime notAfter;

    public PrivateKeyUsagePeriodExtension()
    {
      this.extnOid = "2.5.29.16";
    }

    public PrivateKeyUsagePeriodExtension(ASN1 asn1)
      : base(asn1)
    {
    }

    public PrivateKeyUsagePeriodExtension(X509Extension extension)
      : base(extension)
    {
    }

    protected override void Decode()
    {
      ASN1 asN1 = new ASN1(this.extnValue.Value);
      if (asN1.Tag != (byte) 48)
        throw new ArgumentException("Invalid PrivateKeyUsagePeriod extension");
      for (int index = 0; index < asN1.Count; ++index)
      {
        switch (asN1[index].Tag)
        {
          case 128:
            this.notBefore = ASN1Convert.ToDateTime(asN1[index]);
            break;
          case 129:
            this.notAfter = ASN1Convert.ToDateTime(asN1[index]);
            break;
          default:
            throw new ArgumentException("Invalid PrivateKeyUsagePeriod extension");
        }
      }
    }

    public override string Name
    {
      get
      {
        return "Private Key Usage Period";
      }
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      if (this.notBefore != DateTime.MinValue)
      {
        stringBuilder.Append("Not Before: ");
        stringBuilder.Append(this.notBefore.ToString((IFormatProvider) CultureInfo.CurrentUICulture));
        stringBuilder.Append(Environment.NewLine);
      }
      if (this.notAfter != DateTime.MinValue)
      {
        stringBuilder.Append("Not After: ");
        stringBuilder.Append(this.notAfter.ToString((IFormatProvider) CultureInfo.CurrentUICulture));
        stringBuilder.Append(Environment.NewLine);
      }
      return stringBuilder.ToString();
    }
  }
}
