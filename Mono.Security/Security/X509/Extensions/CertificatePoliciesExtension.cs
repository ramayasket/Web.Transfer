// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.Extensions.CertificatePoliciesExtension
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Collections;
using System.Text;

namespace Mono.Security.X509.Extensions
{
  public class CertificatePoliciesExtension : X509Extension
  {
    private Hashtable policies;

    public CertificatePoliciesExtension()
    {
      this.extnOid = "2.5.29.32";
      this.policies = new Hashtable();
    }

    public CertificatePoliciesExtension(ASN1 asn1)
      : base(asn1)
    {
    }

    public CertificatePoliciesExtension(X509Extension extension)
      : base(extension)
    {
    }

    protected override void Decode()
    {
      this.policies = new Hashtable();
      ASN1 asN1 = new ASN1(this.extnValue.Value);
      if (asN1.Tag != (byte) 48)
        throw new ArgumentException("Invalid CertificatePolicies extension");
      for (int index = 0; index < asN1.Count; ++index)
        this.policies.Add((object) ASN1Convert.ToOid(asN1[index][0]), (object) null);
    }

    public override string Name
    {
      get
      {
        return "Certificate Policies";
      }
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      int num = 1;
      foreach (DictionaryEntry policy in this.policies)
      {
        stringBuilder.Append("[");
        stringBuilder.Append(num++);
        stringBuilder.Append("]Certificate Policy:");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("\tPolicyIdentifier=");
        stringBuilder.Append((string) policy.Key);
        stringBuilder.Append(Environment.NewLine);
      }
      return stringBuilder.ToString();
    }
  }
}
