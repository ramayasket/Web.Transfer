// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.Extensions.BasicConstraintsExtension
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Globalization;
using System.Text;

namespace Mono.Security.X509.Extensions
{
  public class BasicConstraintsExtension : X509Extension
  {
    public const int NoPathLengthConstraint = -1;
    private bool cA;
    private int pathLenConstraint;

    public BasicConstraintsExtension()
    {
      this.extnOid = "2.5.29.19";
      this.pathLenConstraint = -1;
    }

    public BasicConstraintsExtension(ASN1 asn1)
      : base(asn1)
    {
    }

    public BasicConstraintsExtension(X509Extension extension)
      : base(extension)
    {
    }

    protected override void Decode()
    {
      this.cA = false;
      this.pathLenConstraint = -1;
      ASN1 asN1_1 = new ASN1(this.extnValue.Value);
      if (asN1_1.Tag != (byte) 48)
        throw new ArgumentException("Invalid BasicConstraints extension");
      int num1 = 0;
      ASN1 asN1_2 = asN1_1;
      int index1 = num1;
      int num2 = index1 + 1;
      ASN1 asn1 = asN1_2[index1];
      if (asn1 != null && asn1.Tag == (byte) 1)
      {
        this.cA = asn1.Value[0] == byte.MaxValue;
        ASN1 asN1_3 = asN1_1;
        int index2 = num2;
        int num3 = index2 + 1;
        asn1 = asN1_3[index2];
      }
      if (asn1 == null || asn1.Tag != (byte) 2)
        return;
      this.pathLenConstraint = ASN1Convert.ToInt32(asn1);
    }

    protected override void Encode()
    {
      ASN1 asn1 = new ASN1((byte) 48);
      if (this.cA)
        asn1.Add(new ASN1((byte) 1, new byte[1]
        {
          byte.MaxValue
        }));
      if (this.cA && this.pathLenConstraint >= 0)
        asn1.Add(ASN1Convert.FromInt32(this.pathLenConstraint));
      this.extnValue = new ASN1((byte) 4);
      this.extnValue.Add(asn1);
    }

    public bool CertificateAuthority
    {
      get
      {
        return this.cA;
      }
      set
      {
        this.cA = value;
      }
    }

    public override string Name
    {
      get
      {
        return "Basic Constraints";
      }
    }

    public int PathLenConstraint
    {
      get
      {
        return this.pathLenConstraint;
      }
      set
      {
        if (value < -1)
          throw new ArgumentOutOfRangeException(Locale.GetText("PathLenConstraint must be positive or -1 for none ({0}).", (object) value));
        this.pathLenConstraint = value;
      }
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("Subject Type=");
      stringBuilder.Append(!this.cA ? "End Entity" : "CA");
      stringBuilder.Append(Environment.NewLine);
      stringBuilder.Append("Path Length Constraint=");
      if (this.pathLenConstraint == -1)
        stringBuilder.Append("None");
      else
        stringBuilder.Append(this.pathLenConstraint.ToString((IFormatProvider) CultureInfo.InvariantCulture));
      stringBuilder.Append(Environment.NewLine);
      return stringBuilder.ToString();
    }
  }
}
