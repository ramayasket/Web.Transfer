// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.Extensions.KeyAttributesExtension
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Globalization;
using System.Text;

namespace Mono.Security.X509.Extensions
{
  public class KeyAttributesExtension : X509Extension
  {
    private byte[] keyId;
    private int kubits;
    private DateTime notBefore;
    private DateTime notAfter;

    public KeyAttributesExtension()
    {
      this.extnOid = "2.5.29.2";
    }

    public KeyAttributesExtension(ASN1 asn1)
      : base(asn1)
    {
    }

    public KeyAttributesExtension(X509Extension extension)
      : base(extension)
    {
    }

    protected override void Decode()
    {
      ASN1 asN1_1 = new ASN1(this.extnValue.Value);
      if (asN1_1.Tag != (byte) 48)
        throw new ArgumentException("Invalid KeyAttributesExtension extension");
      int index1 = 0;
      if (index1 < asN1_1.Count)
      {
        ASN1 asN1_2 = asN1_1[index1];
        if (asN1_2.Tag == (byte) 4)
        {
          ++index1;
          this.keyId = asN1_2.Value;
        }
      }
      if (index1 < asN1_1.Count)
      {
        ASN1 asN1_2 = asN1_1[index1];
        if (asN1_2.Tag == (byte) 3)
        {
          ++index1;
          int num = 1;
          while (num < asN1_2.Value.Length)
            this.kubits = (this.kubits << 8) + (int) asN1_2.Value[num++];
        }
      }
      if (index1 >= asN1_1.Count)
        return;
      ASN1 asN1_3 = asN1_1[index1];
      if (asN1_3.Tag != (byte) 48)
        return;
      int index2 = 0;
      if (index2 < asN1_3.Count)
      {
        ASN1 time = asN1_3[index2];
        if (time.Tag == (byte) 129)
        {
          ++index2;
          this.notBefore = ASN1Convert.ToDateTime(time);
        }
      }
      if (index2 >= asN1_3.Count)
        return;
      ASN1 time1 = asN1_3[index2];
      if (time1.Tag != (byte) 130)
        return;
      this.notAfter = ASN1Convert.ToDateTime(time1);
    }

    public byte[] KeyIdentifier
    {
      get
      {
        return this.keyId == null ? (byte[]) null : (byte[]) this.keyId.Clone();
      }
    }

    public override string Name
    {
      get
      {
        return "Key Attributes";
      }
    }

    public DateTime NotAfter
    {
      get
      {
        return this.notAfter;
      }
    }

    public DateTime NotBefore
    {
      get
      {
        return this.notBefore;
      }
    }

    public bool Support(KeyUsages usage)
    {
      int int32 = Convert.ToInt32((object) usage, (IFormatProvider) CultureInfo.InvariantCulture);
      return (int32 & this.kubits) == int32;
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      if (this.keyId != null)
      {
        stringBuilder.Append("KeyID=");
        for (int index = 0; index < this.keyId.Length; ++index)
        {
          stringBuilder.Append(this.keyId[index].ToString("X2", (IFormatProvider) CultureInfo.InvariantCulture));
          if (index % 2 == 1)
            stringBuilder.Append(" ");
        }
        stringBuilder.Append(Environment.NewLine);
      }
      if (this.kubits != 0)
      {
        stringBuilder.Append("Key Usage=");
        if (this.Support(KeyUsages.digitalSignature))
          stringBuilder.Append("Digital Signature");
        if (this.Support(KeyUsages.nonRepudiation))
        {
          if (stringBuilder.Length > 0)
            stringBuilder.Append(" , ");
          stringBuilder.Append("Non-Repudiation");
        }
        if (this.Support(KeyUsages.keyEncipherment))
        {
          if (stringBuilder.Length > 0)
            stringBuilder.Append(" , ");
          stringBuilder.Append("Key Encipherment");
        }
        if (this.Support(KeyUsages.dataEncipherment))
        {
          if (stringBuilder.Length > 0)
            stringBuilder.Append(" , ");
          stringBuilder.Append("Data Encipherment");
        }
        if (this.Support(KeyUsages.keyAgreement))
        {
          if (stringBuilder.Length > 0)
            stringBuilder.Append(" , ");
          stringBuilder.Append("Key Agreement");
        }
        if (this.Support(KeyUsages.keyCertSign))
        {
          if (stringBuilder.Length > 0)
            stringBuilder.Append(" , ");
          stringBuilder.Append("Certificate Signing");
        }
        if (this.Support(KeyUsages.cRLSign))
        {
          if (stringBuilder.Length > 0)
            stringBuilder.Append(" , ");
          stringBuilder.Append("CRL Signing");
        }
        if (this.Support(KeyUsages.encipherOnly))
        {
          if (stringBuilder.Length > 0)
            stringBuilder.Append(" , ");
          stringBuilder.Append("Encipher Only ");
        }
        if (this.Support(KeyUsages.decipherOnly))
        {
          if (stringBuilder.Length > 0)
            stringBuilder.Append(" , ");
          stringBuilder.Append("Decipher Only");
        }
        stringBuilder.Append("(");
        stringBuilder.Append(this.kubits.ToString("X2", (IFormatProvider) CultureInfo.InvariantCulture));
        stringBuilder.Append(")");
        stringBuilder.Append(Environment.NewLine);
      }
      if (this.notBefore != DateTime.MinValue)
      {
        stringBuilder.Append("Not Before=");
        stringBuilder.Append(this.notBefore.ToString((IFormatProvider) CultureInfo.CurrentUICulture));
        stringBuilder.Append(Environment.NewLine);
      }
      if (this.notAfter != DateTime.MinValue)
      {
        stringBuilder.Append("Not After=");
        stringBuilder.Append(this.notAfter.ToString((IFormatProvider) CultureInfo.CurrentUICulture));
        stringBuilder.Append(Environment.NewLine);
      }
      return stringBuilder.ToString();
    }
  }
}
