// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.SafeBag
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

namespace Mono.Security.X509
{
  internal class SafeBag
  {
    private string _bagOID;
    private ASN1 _asn1;

    public SafeBag(string bagOID, ASN1 asn1)
    {
      this._bagOID = bagOID;
      this._asn1 = asn1;
    }

    public string BagOID
    {
      get
      {
        return this._bagOID;
      }
    }

    public ASN1 ASN1
    {
      get
      {
        return this._asn1;
      }
    }
  }
}
