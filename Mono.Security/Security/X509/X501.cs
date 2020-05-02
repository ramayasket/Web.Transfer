// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.X501
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Mono.Security.X509
{
  using Mono.Security;
  using Mono.Security.Cryptography;
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Runtime.CompilerServices;
  using System.Text;

  public sealed class X501
  {
    private static byte[] countryName = new byte[] { 0x55, 4, 6, 0 };
    private static byte[] organizationName = new byte[] { 0x55, 4, 10, 0 };
    private static byte[] organizationalUnitName = new byte[] { 0x55, 4, 11, 0 };
    private static byte[] commonName = new byte[] { 0x55, 4, 3, 0 };
    private static byte[] localityName = new byte[] { 0x55, 4, 7, 0 };
    private static byte[] stateOrProvinceName = new byte[] { 0x55, 4, 8, 0 };
    private static byte[] streetAddress = new byte[] { 0x55, 4, 9, 0 };
    private static byte[] domainComponent = new byte[] { 9, 0x92, 0x26, 0x89, 0x93, 0xf2, 0x2c, 100, 1, 0x19, 0, 0 };
    private static byte[] userid = new byte[] { 9, 0x92, 0x26, 0x89, 0x93, 0xf2, 0x2c, 100, 1, 1, 0, 0 };
    private static byte[] email = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 13, 1, 9, 1, 0, 0, 0 };
    private static byte[] dnQualifier = new byte[] { 0x55, 4, 0x2e, 0 };
    private static byte[] title = new byte[] { 0x55, 4, 12, 0 };
    private static byte[] surname = new byte[] { 0x55, 4, 4, 0 };
    private static byte[] givenName = new byte[] { 0x55, 4, 0x2a, 0 };
    private static byte[] initial = new byte[] { 0x55, 4, 0x2b, 0 };
    [CompilerGenerated]
    private static Dictionary<string, int> __f__switch_mapD;

        private X501()
    {
    }

    private static void AppendEntry(StringBuilder sb, ASN1 entry, bool quotes)
    {
      for (int i = 0; i < entry.Count; i++)
      {
        ASN1 asn = entry[i];
        ASN1 asn2 = asn[1];
        if (asn2 != null)
        {
          ASN1 asn3 = asn[0];
          if (asn3 != null)
          {
            if (asn3.CompareValue(countryName))
            {
              sb.Append("C=");
            }
            else if (asn3.CompareValue(organizationName))
            {
              sb.Append("O=");
            }
            else if (asn3.CompareValue(organizationalUnitName))
            {
              sb.Append("OU=");
            }
            else if (asn3.CompareValue(commonName))
            {
              sb.Append("CN=");
            }
            else if (asn3.CompareValue(localityName))
            {
              sb.Append("L=");
            }
            else if (asn3.CompareValue(stateOrProvinceName))
            {
              sb.Append("S=");
            }
            else if (asn3.CompareValue(streetAddress))
            {
              sb.Append("STREET=");
            }
            else if (asn3.CompareValue(domainComponent))
            {
              sb.Append("DC=");
            }
            else if (asn3.CompareValue(userid))
            {
              sb.Append("UID=");
            }
            else if (asn3.CompareValue(email))
            {
              sb.Append("E=");
            }
            else if (asn3.CompareValue(dnQualifier))
            {
              sb.Append("dnQualifier=");
            }
            else if (asn3.CompareValue(title))
            {
              sb.Append("T=");
            }
            else if (asn3.CompareValue(surname))
            {
              sb.Append("SN=");
            }
            else if (asn3.CompareValue(givenName))
            {
              sb.Append("G=");
            }
            else if (asn3.CompareValue(initial))
            {
              sb.Append("I=");
            }
            else
            {
              sb.Append("OID.");
              sb.Append(ASN1Convert.ToOid(asn3));
              sb.Append("=");
            }
            string str = null;
            if (asn2.Tag == 30)
            {
              StringBuilder builder = new StringBuilder();
              for (int j = 1; j < asn2.Value.Length; j += 2)
              {
                builder.Append((char)asn2.Value[j]);
              }
              str = builder.ToString();
            }
            else
            {
              if (asn2.Tag == 20)
              {
                str = Encoding.UTF7.GetString(asn2.Value);
              }
              else
              {
                str = Encoding.UTF8.GetString(asn2.Value);
              }
              char[] anyOf = new char[] { ',', '+', '"', '\\', '<', '>', ';', '\0' };
              if (quotes && (((str.IndexOfAny(anyOf, 0, str.Length) > 0) || str.StartsWith(" ")) || str.EndsWith(" ")))
              {
                str = "\"" + str + "\"";
              }
            }
            sb.Append(str);
            if (i < (entry.Count - 1))
            {
              sb.Append(", ");
            }
          }
        }
      }
    }

    public static ASN1 FromString(string rdn)
    {
      if (rdn == null)
      {
        throw new ArgumentNullException("rdn");
      }
      int pos = 0;
      ASN1 asn = new ASN1(0x30);
      while (pos < rdn.Length)
      {
        X520.AttributeTypeAndValue value2 = ReadAttribute(rdn, ref pos);
        value2.Value = ReadValue(rdn, ref pos);
        ASN1 asn2 = new ASN1(0x31);
        asn2.Add(value2.GetASN1());
        asn.Add(asn2);
      }
      return asn;
    }

    private static X520.AttributeTypeAndValue GetAttributeFromOid(string attributeType)
    {
      string oid = attributeType.ToUpper(CultureInfo.InvariantCulture).Trim();
      string key = oid;
      if (key != null)
      {
        if (__f__switch_mapD == null)
                {
          Dictionary<string, int> dictionary = new Dictionary<string, int>(15) {
                        {
                            "C",
                            0
                        },
                        {
                            "O",
                            1
                        },
                        {
                            "OU",
                            2
                        },
                        {
                            "CN",
                            3
                        },
                        {
                            "L",
                            4
                        },
                        {
                            "S",
                            5
                        },
                        {
                            "ST",
                            5
                        },
                        {
                            "E",
                            6
                        },
                        {
                            "DC",
                            7
                        },
                        {
                            "UID",
                            8
                        },
                        {
                            "DNQUALIFIER",
                            9
                        },
                        {
                            "T",
                            10
                        },
                        {
                            "SN",
                            11
                        },
                        {
                            "G",
                            12
                        },
                        {
                            "I",
                            13
                        }
                    };
                    __f__switch_mapD = dictionary;
        }
        if (__f__switch_mapD.TryGetValue(key, out int num))
                {
          switch (num)
          {
            case 0:
              return new X520.CountryName();

            case 1:
              return new X520.OrganizationName();

            case 2:
              return new X520.OrganizationalUnitName();

            case 3:
              return new X520.CommonName();

            case 4:
              return new X520.LocalityName();

            case 5:
              return new X520.StateOrProvinceName();

            case 6:
              return new X520.EmailAddress();

            case 7:
              return new X520.DomainComponent();

            case 8:
              return new X520.UserId();

            case 9:
              return new X520.DnQualifier();

            case 10:
              return new X520.Title();

            case 11:
              return new X520.Surname();

            case 12:
              return new X520.GivenName();

            case 13:
              return new X520.Initial();
          }
        }
      }
      if (oid.StartsWith("OID."))
      {
        return new X520.Oid(oid.Substring(4));
      }
      if (IsOid(oid))
      {
        return new X520.Oid(oid);
      }
      return null;
    }

    private static bool IsHex(char c)
    {
      if (char.IsDigit(c))
      {
        return true;
      }
      char ch = char.ToUpper(c, CultureInfo.InvariantCulture);
      return ((ch >= 'A') && (ch <= 'F'));
    }

    private static bool IsOid(string oid)
    {
      try
      {
        return (ASN1Convert.FromOid(oid).Tag == 6);
      }
      catch
      {
        return false;
      }
    }

    private static X520.AttributeTypeAndValue ReadAttribute(string value, ref int pos)
    {
      while ((value[pos] == ' ') && (pos < value.Length))
      {
        pos++;
      }
      int index = value.IndexOf('=', pos);
      if (index == -1)
      {
        throw new FormatException(Locale.GetText("No attribute found."));
      }
      string attributeType = value.Substring(pos, index - pos);
      X520.AttributeTypeAndValue attributeFromOid = GetAttributeFromOid(attributeType);
      if (attributeFromOid == null)
      {
        throw new FormatException(string.Format(Locale.GetText("Unknown attribute '{0}'."), attributeType));
      }
      pos = index + 1;
      return attributeFromOid;
    }

    private static int ReadEscaped(StringBuilder sb, string value, int pos)
    {
      switch (value[pos])
      {
        case ';':
        case '<':
        case '=':
        case '>':
        case '"':
        case '#':
        case '+':
        case ',':
        case '\\':
          sb.Append(value[pos]);
          return pos;
      }
      if (pos >= (value.Length - 2))
      {
        throw new FormatException(string.Format(Locale.GetText("Malformed escaped value '{0}'."), value.Substring(pos)));
      }
      sb.Append(ReadHex(value, ref pos));
      return pos;
    }

    private static string ReadHex(string value, ref int pos)
    {
      StringBuilder builder = new StringBuilder();
      builder.Append(value[pos++]);
      builder.Append(value[pos]);
      if (((pos < (value.Length - 4)) && (value[pos + 1] == '\\')) && IsHex(value[pos + 2]))
      {
        pos += 2;
        builder.Append(value[pos++]);
        builder.Append(value[pos]);
      }
      byte[] bytes = CryptoConvert.FromHex(builder.ToString());
      return Encoding.UTF8.GetString(bytes);
    }

    private static int ReadQuoted(StringBuilder sb, string value, int pos)
    {
      int startIndex = pos;
      while (pos <= value.Length)
      {
        switch (value[pos])
        {
          case '"':
            return pos;

          case '\\':
            return ReadEscaped(sb, value, pos);
        }
        sb.Append(value[pos]);
        pos++;
      }
      throw new FormatException(string.Format(Locale.GetText("Malformed quoted value '{0}'."), value.Substring(startIndex)));
    }

    private static string ReadValue(string value, ref int pos)
    {
      int startIndex = pos;
      StringBuilder sb = new StringBuilder();
      while (pos < value.Length)
      {
        int num2;
        switch (value[pos])
        {
          case ';':
          case '<':
          case '=':
          case '>':
            throw new FormatException(string.Format(Locale.GetText("Malformed value '{0}' contains '{1}' outside quotes."), value.Substring(startIndex), value[pos]));

          case '"':
            pos = num2 = pos + 1;
            pos = ReadQuoted(sb, value, num2);
            break;

          case '#':
          case '+':
            throw new NotImplementedException();

          case ',':
            pos++;
            return sb.ToString();

          case '\\':
            pos = num2 = pos + 1;
            pos = ReadEscaped(sb, value, num2);
            break;

          default:
            sb.Append(value[pos]);
            break;
        }
        pos++;
      }
      return sb.ToString();
    }

    public static string ToString(ASN1 seq)
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < seq.Count; i++)
      {
        ASN1 entry = seq[i];
        AppendEntry(sb, entry, true);
        if (i < (seq.Count - 1))
        {
          sb.Append(", ");
        }
      }
      return sb.ToString();
    }

    public static string ToString(ASN1 seq, bool reversed, string separator, bool quotes)
    {
      StringBuilder sb = new StringBuilder();
      if (reversed)
      {
        for (int i = seq.Count - 1; i >= 0; i--)
        {
          ASN1 entry = seq[i];
          AppendEntry(sb, entry, quotes);
          if (i > 0)
          {
            sb.Append(separator);
          }
        }
      }
      else
      {
        for (int i = 0; i < seq.Count; i++)
        {
          ASN1 entry = seq[i];
          AppendEntry(sb, entry, quotes);
          if (i < (seq.Count - 1))
          {
            sb.Append(separator);
          }
        }
      }
      return sb.ToString();
    }
  }
}
