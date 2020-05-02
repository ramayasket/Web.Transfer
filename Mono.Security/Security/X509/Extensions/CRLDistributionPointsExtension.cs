// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.Extensions.CRLDistributionPointsExtension
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Collections;
using System.Text;

namespace Mono.Security.X509.Extensions
{
  public class CRLDistributionPointsExtension : X509Extension
  {
    private ArrayList dps;

    public CRLDistributionPointsExtension()
    {
      this.extnOid = "2.5.29.31";
      this.dps = new ArrayList();
    }

    public CRLDistributionPointsExtension(ASN1 asn1)
      : base(asn1)
    {
    }

    public CRLDistributionPointsExtension(X509Extension extension)
      : base(extension)
    {
    }

    protected override void Decode()
    {
      this.dps = new ArrayList();
      ASN1 asN1 = new ASN1(this.extnValue.Value);
      if (asN1.Tag != (byte) 48)
        throw new ArgumentException("Invalid CRLDistributionPoints extension");
      for (int index = 0; index < asN1.Count; ++index)
        this.dps.Add((object) new CRLDistributionPointsExtension.DP(asN1[index]));
    }

    public override string Name
    {
      get
      {
        return "CRL Distribution Points";
      }
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      int num = 1;
      foreach (CRLDistributionPointsExtension.DP dp in this.dps)
      {
        stringBuilder.Append("[");
        stringBuilder.Append(num++);
        stringBuilder.Append("]CRL Distribution Point");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("\tDistribution Point Name:");
        stringBuilder.Append("\t\tFull Name:");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("\t\t\t");
        stringBuilder.Append(dp.DistributionPoint);
        stringBuilder.Append(Environment.NewLine);
      }
      return stringBuilder.ToString();
    }

    internal class DP
    {
      public string DistributionPoint;
      public CRLDistributionPointsExtension.ReasonFlags Reasons;
      public string CRLIssuer;

      public DP(string dp, CRLDistributionPointsExtension.ReasonFlags reasons, string issuer)
      {
        this.DistributionPoint = dp;
        this.Reasons = reasons;
        this.CRLIssuer = issuer;
      }

      public DP(ASN1 dp)
      {
        for (int index1 = 0; index1 < dp.Count; ++index1)
        {
          ASN1 asN1 = dp[index1];
          switch (asN1.Tag)
          {
            case 160:
              for (int index2 = 0; index2 < asN1.Count; ++index2)
              {
                ASN1 sequence = asN1[index2];
                if (sequence.Tag == (byte) 160)
                  this.DistributionPoint = new GeneralNames(sequence).ToString();
              }
              break;
          }
        }
      }
    }

    [Flags]
    public enum ReasonFlags
    {
      Unused = 0,
      KeyCompromise = 1,
      CACompromise = 2,
      AffiliationChanged = CACompromise | KeyCompromise, // 0x00000003
      Superseded = 4,
      CessationOfOperation = Superseded | KeyCompromise, // 0x00000005
      CertificateHold = Superseded | CACompromise, // 0x00000006
      PrivilegeWithdrawn = CertificateHold | KeyCompromise, // 0x00000007
      AACompromise = 8,
    }
  }
}
