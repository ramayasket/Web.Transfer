// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.Extensions.SubjectKeyIdentifierExtension
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Globalization;
using System.Text;

namespace Mono.Security.X509.Extensions
{
  public class SubjectKeyIdentifierExtension : X509Extension
  {
    private byte[] ski;

    public SubjectKeyIdentifierExtension()
    {
      this.extnOid = "2.5.29.14";
    }

    public SubjectKeyIdentifierExtension(ASN1 asn1)
      : base(asn1)
    {
    }

    public SubjectKeyIdentifierExtension(X509Extension extension)
      : base(extension)
    {
    }

    protected override void Decode()
    {
      ASN1 asN1 = new ASN1(this.extnValue.Value);
      if (asN1.Tag != (byte) 4)
        throw new ArgumentException("Invalid SubjectKeyIdentifier extension");
      this.ski = asN1.Value;
    }

    public override string Name
    {
      get
      {
        return "Subject Key Identifier";
      }
    }

    public byte[] Identifier
    {
      get
      {
        return this.ski == null ? (byte[]) null : (byte[]) this.ski.Clone();
      }
    }

    public override string ToString()
    {
      if (this.ski == null)
        return (string) null;
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < this.ski.Length; ++index)
      {
        stringBuilder.Append(this.ski[index].ToString("X2", (IFormatProvider) CultureInfo.InvariantCulture));
        if (index % 2 == 1)
          stringBuilder.Append(" ");
      }
      return stringBuilder.ToString();
    }
  }
}
