// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.Extensions.ExtendedKeyUsageExtension
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mono.Security.X509.Extensions
{
  using Mono.Security;
  using Mono.Security.X509;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Runtime.CompilerServices;
  using System.Text;

  public class ExtendedKeyUsageExtension : X509Extension
  {
    private ArrayList keyPurpose;
    [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_map14;

        public ExtendedKeyUsageExtension()
    {
      base.extnOid = "2.5.29.37";
      this.keyPurpose = new ArrayList();
    }

    public ExtendedKeyUsageExtension(ASN1 asn1) : base(asn1)
    {
    }

    public ExtendedKeyUsageExtension(X509Extension extension) : base(extension)
    {
    }

    protected override void Decode()
    {
      this.keyPurpose = new ArrayList();
      ASN1 asn = new ASN1(base.extnValue.Value);
      if (asn.Tag != 0x30)
      {
        throw new ArgumentException("Invalid ExtendedKeyUsage extension");
      }
      for (int i = 0; i < asn.Count; i++)
      {
        this.keyPurpose.Add(ASN1Convert.ToOid(asn[i]));
      }
    }

    protected override void Encode()
    {
      ASN1 asn = new ASN1(0x30);
      IEnumerator enumerator = this.keyPurpose.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          string current = (string)enumerator.Current;
          asn.Add(ASN1Convert.FromOid(current));
        }
      }
      finally
      {
        if (enumerator is IDisposable disposable)
        {
          disposable.Dispose();
        }
      }
      base.extnValue = new ASN1(4);
      base.extnValue.Add(asn);
    }

    public override string ToString()
    {
      StringBuilder builder = new StringBuilder();
      IEnumerator enumerator = this.keyPurpose.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          string current = (string)enumerator.Current;
          string key = current;
          if (key != null)
          {
            if (__f__switch_map14 == null)
                        {
              Dictionary<string, int> dictionary = new Dictionary<string, int>(6) {
                                {
                                    "1.3.6.1.5.5.7.3.1",
                                    0
                                },
                                {
                                    "1.3.6.1.5.5.7.3.2",
                                    1
                                },
                                {
                                    "1.3.6.1.5.5.7.3.3",
                                    2
                                },
                                {
                                    "1.3.6.1.5.5.7.3.4",
                                    3
                                },
                                {
                                    "1.3.6.1.5.5.7.3.8",
                                    4
                                },
                                {
                                    "1.3.6.1.5.5.7.3.9",
                                    5
                                }
                            };
                            __f__switch_map14 = dictionary;
            }
            if (__f__switch_map14.TryGetValue(key, out int num))
                        {
              switch (num)
              {
                case 0:
                  builder.Append("Server Authentication");
                  goto Label_013F;

                case 1:
                  builder.Append("Client Authentication");
                  goto Label_013F;

                case 2:
                  builder.Append("Code Signing");
                  goto Label_013F;

                case 3:
                  builder.Append("Email Protection");
                  goto Label_013F;

                case 4:
                  builder.Append("Time Stamping");
                  goto Label_013F;

                case 5:
                  builder.Append("OCSP Signing");
                  goto Label_013F;
              }
            }
          }
          builder.Append("unknown");
          Label_013F:
          builder.AppendFormat(" ({0}){1}", current, Environment.NewLine);
        }
      }
      finally
      {
        if (enumerator is IDisposable disposable)
        {
          disposable.Dispose();
        }
      }
      return builder.ToString();
    }

    public ArrayList KeyPurpose =>
        this.keyPurpose;

    public override string Name =>
        "Extended Key Usage";
  }
}
