// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.X509Extension
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Globalization;
using System.Text;

namespace Mono.Security.X509
{
  public class X509Extension
  {
    protected string extnOid;
    protected bool extnCritical;
    protected ASN1 extnValue;

    protected X509Extension()
    {
      this.extnCritical = false;
    }

    public X509Extension(ASN1 asn1)
    {
      if (asn1.Tag != (byte) 48 || asn1.Count < 2)
        throw new ArgumentException(Locale.GetText("Invalid X.509 extension."));
      if (asn1[0].Tag != (byte) 6)
        throw new ArgumentException(Locale.GetText("Invalid X.509 extension."));
      this.extnOid = ASN1Convert.ToOid(asn1[0]);
      this.extnCritical = asn1[1].Tag == (byte) 1 && asn1[1].Value[0] == byte.MaxValue;
      this.extnValue = asn1[asn1.Count - 1];
      if (this.extnValue.Tag == (byte) 4 && this.extnValue.Length > 0)
      {
        if (this.extnValue.Count == 0)
        {
          try
          {
            ASN1 asn1_1 = new ASN1(this.extnValue.Value);
            this.extnValue.Value = (byte[]) null;
            this.extnValue.Add(asn1_1);
          }
          catch
          {
          }
        }
      }
      this.Decode();
    }

    public X509Extension(X509Extension extension)
    {
      if (extension == null)
        throw new ArgumentNullException(nameof (extension));
      if (extension.Value == null || extension.Value.Tag != (byte) 4 || extension.Value.Count != 1)
        throw new ArgumentException(Locale.GetText("Invalid X.509 extension."));
      this.extnOid = extension.Oid;
      this.extnCritical = extension.Critical;
      this.extnValue = extension.Value;
      this.Decode();
    }

    protected virtual void Decode()
    {
    }

    protected virtual void Encode()
    {
    }

    public ASN1 ASN1
    {
      get
      {
        ASN1 asN1 = new ASN1((byte) 48);
        asN1.Add(ASN1Convert.FromOid(this.extnOid));
        if (this.extnCritical)
          asN1.Add(new ASN1((byte) 1, new byte[1]
          {
            byte.MaxValue
          }));
        this.Encode();
        asN1.Add(this.extnValue);
        return asN1;
      }
    }

    public string Oid
    {
      get
      {
        return this.extnOid;
      }
    }

    public bool Critical
    {
      get
      {
        return this.extnCritical;
      }
      set
      {
        this.extnCritical = value;
      }
    }

    public virtual string Name
    {
      get
      {
        return this.extnOid;
      }
    }

    public ASN1 Value
    {
      get
      {
        if (this.extnValue == null)
          this.Encode();
        return this.extnValue;
      }
    }

    public override bool Equals(object obj)
    {
      if (obj == null || !(obj is X509Extension x509Extension) || (this.extnCritical != x509Extension.extnCritical || this.extnOid != x509Extension.extnOid) || this.extnValue.Length != x509Extension.extnValue.Length)
        return false;
      for (int index = 0; index < this.extnValue.Length; ++index)
      {
        if (this.extnValue[index] != x509Extension.extnValue[index])
          return false;
      }
      return true;
    }

    public byte[] GetBytes()
    {
      return this.ASN1.GetBytes();
    }

    public override int GetHashCode()
    {
      return this.extnOid.GetHashCode();
    }

    private void WriteLine(StringBuilder sb, int n, int pos)
    {
      byte[] numArray = this.extnValue.Value;
      int num1 = pos;
      for (int index = 0; index < 8; ++index)
      {
        if (index < n)
        {
          sb.Append(numArray[num1++].ToString("X2", (IFormatProvider) CultureInfo.InvariantCulture));
          sb.Append(" ");
        }
        else
          sb.Append("   ");
      }
      sb.Append("  ");
      int num2 = pos;
      for (int index = 0; index < n; ++index)
      {
        byte num3 = numArray[num2++];
        if (num3 < (byte) 32)
          sb.Append(".");
        else
          sb.Append(Convert.ToChar(num3));
      }
      sb.Append(Environment.NewLine);
    }

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      int num = this.extnValue.Length >> 3;
      int n = this.extnValue.Length - (num << 3);
      int pos = 0;
      for (int index = 0; index < num; ++index)
      {
        this.WriteLine(sb, 8, pos);
        pos += 8;
      }
      this.WriteLine(sb, n, pos);
      return sb.ToString();
    }
  }
}
