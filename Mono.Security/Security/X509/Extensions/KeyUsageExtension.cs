// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.Extensions.KeyUsageExtension
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Globalization;
using System.Text;

namespace Mono.Security.X509.Extensions
{
  public class KeyUsageExtension : X509Extension
  {
    private int kubits;

    public KeyUsageExtension(ASN1 asn1)
      : base(asn1)
    {
    }

    public KeyUsageExtension(X509Extension extension)
      : base(extension)
    {
    }

    public KeyUsageExtension()
    {
      this.extnOid = "2.5.29.15";
    }

    protected override void Decode()
    {
      ASN1 asN1 = new ASN1(this.extnValue.Value);
      if (asN1.Tag != (byte) 3)
        throw new ArgumentException("Invalid KeyUsage extension");
      int num = 1;
      while (num < asN1.Value.Length)
        this.kubits = (this.kubits << 8) + (int) asN1.Value[num++];
    }

    protected override void Encode()
    {
      this.extnValue = new ASN1((byte) 4);
      ushort kubits = (ushort) this.kubits;
      if (kubits > (ushort) 0)
      {
        byte num;
        for (num = (byte) 15; num > (byte) 0 && ((int) kubits & 32768) != 32768; --num)
          kubits <<= 1;
        if (this.kubits > (int) byte.MaxValue)
          this.extnValue.Add(new ASN1((byte) 3, new byte[3]
          {
            (byte) ((uint) num - 8U),
            (byte) this.kubits,
            (byte) (this.kubits >> 8)
          }));
        else
          this.extnValue.Add(new ASN1((byte) 3, new byte[2]
          {
            num,
            (byte) this.kubits
          }));
      }
      else
        this.extnValue.Add(new ASN1((byte) 3, new byte[2]
        {
          (byte) 7,
          (byte) 0
        }));
    }

    public KeyUsages KeyUsage
    {
      get
      {
        return (KeyUsages) this.kubits;
      }
      set
      {
        this.kubits = Convert.ToInt32((object) value, (IFormatProvider) CultureInfo.InvariantCulture);
      }
    }

    public override string Name
    {
      get
      {
        return "Key Usage";
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
      return stringBuilder.ToString();
    }
  }
}
