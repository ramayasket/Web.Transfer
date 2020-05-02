// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.Extensions.AuthorityKeyIdentifierExtension
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Globalization;
using System.Text;

namespace Mono.Security.X509.Extensions
{
  public class AuthorityKeyIdentifierExtension : X509Extension
  {
    private byte[] aki;

    public AuthorityKeyIdentifierExtension()
    {
      this.extnOid = "2.5.29.35";
    }

    public AuthorityKeyIdentifierExtension(ASN1 asn1)
      : base(asn1)
    {
    }

    public AuthorityKeyIdentifierExtension(X509Extension extension)
      : base(extension)
    {
    }

    protected override void Decode()
    {
      ASN1 asN1_1 = new ASN1(this.extnValue.Value);
      if (asN1_1.Tag != (byte) 48)
        throw new ArgumentException("Invalid AuthorityKeyIdentifier extension");
      for (int index = 0; index < asN1_1.Count; ++index)
      {
        ASN1 asN1_2 = asN1_1[index];
        if (asN1_2.Tag == (byte) 128)
          this.aki = asN1_2.Value;
      }
    }

    public override string Name
    {
      get
      {
        return "Authority Key Identifier";
      }
    }

    public byte[] Identifier
    {
      get
      {
        return this.aki == null ? (byte[]) null : (byte[]) this.aki.Clone();
      }
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      if (this.aki != null)
      {
        int index = 0;
        stringBuilder.Append("KeyID=");
        for (; index < this.aki.Length; ++index)
        {
          stringBuilder.Append(this.aki[index].ToString("X2", (IFormatProvider) CultureInfo.InvariantCulture));
          if (index % 2 == 1)
            stringBuilder.Append(" ");
        }
      }
      return stringBuilder.ToString();
    }
  }
}
