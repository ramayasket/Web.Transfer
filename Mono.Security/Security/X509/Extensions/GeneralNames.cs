// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.Extensions.GeneralNames
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Collections;
using System.Text;

namespace Mono.Security.X509.Extensions
{
  internal class GeneralNames
  {
    private ArrayList rfc822Name;
    private ArrayList dnsName;
    private ArrayList directoryNames;
    private ArrayList uris;
    private ArrayList ipAddr;
    private ASN1 asn;

    public GeneralNames()
    {
    }

    public GeneralNames(string[] rfc822s, string[] dnsNames, string[] ipAddresses, string[] uris)
    {
      this.asn = new ASN1((byte) 48);
      if (rfc822s != null)
      {
        this.rfc822Name = new ArrayList();
        foreach (string rfc822 in rfc822s)
        {
          this.asn.Add(new ASN1((byte) 129, Encoding.ASCII.GetBytes(rfc822)));
          this.rfc822Name.Add((object) rfc822s);
        }
      }
      if (dnsNames != null)
      {
        this.dnsName = new ArrayList();
        foreach (string dnsName in dnsNames)
        {
          this.asn.Add(new ASN1((byte) 130, Encoding.ASCII.GetBytes(dnsName)));
          this.dnsName.Add((object) dnsName);
        }
      }
      if (ipAddresses != null)
      {
        this.ipAddr = new ArrayList();
        foreach (string ipAddress in ipAddresses)
        {
          string[] strArray = ipAddress.Split('.', ':');
          byte[] data = new byte[strArray.Length];
          for (int index = 0; index < strArray.Length; ++index)
            data[index] = byte.Parse(strArray[index]);
          this.asn.Add(new ASN1((byte) 135, data));
          this.ipAddr.Add((object) ipAddress);
        }
      }
      if (uris == null)
        return;
      this.uris = new ArrayList();
      foreach (string uri in uris)
      {
        this.asn.Add(new ASN1((byte) 134, Encoding.ASCII.GetBytes(uri)));
        this.uris.Add((object) uri);
      }
    }

    public GeneralNames(ASN1 sequence)
    {
      for (int index1 = 0; index1 < sequence.Count; ++index1)
      {
        byte tag = sequence[index1].Tag;
        switch (tag)
        {
          case 129:
            if (this.rfc822Name == null)
              this.rfc822Name = new ArrayList();
            this.rfc822Name.Add((object) Encoding.ASCII.GetString(sequence[index1].Value));
            break;
          case 130:
            if (this.dnsName == null)
              this.dnsName = new ArrayList();
            this.dnsName.Add((object) Encoding.ASCII.GetString(sequence[index1].Value));
            break;
          case 132:
            if (this.directoryNames == null)
              this.directoryNames = new ArrayList();
            this.directoryNames.Add((object) X501.ToString(sequence[index1][0]));
            break;
          case 134:
            if (this.uris == null)
              this.uris = new ArrayList();
            this.uris.Add((object) Encoding.ASCII.GetString(sequence[index1].Value));
            break;
          case 135:
            if (this.ipAddr == null)
              this.ipAddr = new ArrayList();
            byte[] numArray = sequence[index1].Value;
            string str = numArray.Length != 4 ? ":" : ".";
            StringBuilder stringBuilder = new StringBuilder();
            for (int index2 = 0; index2 < numArray.Length; ++index2)
            {
              stringBuilder.Append(numArray[index2].ToString());
              if (index2 < numArray.Length - 1)
                stringBuilder.Append(str);
            }
            this.ipAddr.Add((object) stringBuilder.ToString());
            if (this.ipAddr == null)
            {
              this.ipAddr = new ArrayList();
              break;
            }
            break;
          default:
            if (tag == (byte) 164)
              goto case 132;
            else
              break;
        }
      }
    }

    public string[] RFC822
    {
      get
      {
        return this.rfc822Name == null ? new string[0] : (string[]) this.rfc822Name.ToArray(typeof (string));
      }
    }

    public string[] DirectoryNames
    {
      get
      {
        return this.directoryNames == null ? new string[0] : (string[]) this.directoryNames.ToArray(typeof (string));
      }
    }

    public string[] DNSNames
    {
      get
      {
        return this.dnsName == null ? new string[0] : (string[]) this.dnsName.ToArray(typeof (string));
      }
    }

    public string[] UniformResourceIdentifiers
    {
      get
      {
        return this.uris == null ? new string[0] : (string[]) this.uris.ToArray(typeof (string));
      }
    }

    public string[] IPAddresses
    {
      get
      {
        return this.ipAddr == null ? new string[0] : (string[]) this.ipAddr.ToArray(typeof (string));
      }
    }

    public byte[] GetBytes()
    {
      return this.asn.GetBytes();
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      if (this.rfc822Name != null)
      {
        foreach (string str in this.rfc822Name)
        {
          stringBuilder.Append("RFC822 Name=");
          stringBuilder.Append(str);
          stringBuilder.Append(Environment.NewLine);
        }
      }
      if (this.dnsName != null)
      {
        foreach (string str in this.dnsName)
        {
          stringBuilder.Append("DNS Name=");
          stringBuilder.Append(str);
          stringBuilder.Append(Environment.NewLine);
        }
      }
      if (this.directoryNames != null)
      {
        foreach (string directoryName in this.directoryNames)
        {
          stringBuilder.Append("Directory Address: ");
          stringBuilder.Append(directoryName);
          stringBuilder.Append(Environment.NewLine);
        }
      }
      if (this.uris != null)
      {
        foreach (string uri in this.uris)
        {
          stringBuilder.Append("URL=");
          stringBuilder.Append(uri);
          stringBuilder.Append(Environment.NewLine);
        }
      }
      if (this.ipAddr != null)
      {
        foreach (string str in this.ipAddr)
        {
          stringBuilder.Append("IP Address=");
          stringBuilder.Append(str);
          stringBuilder.Append(Environment.NewLine);
        }
      }
      return stringBuilder.ToString();
    }
  }
}
