// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.DiffieHellman
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Xml;
using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security.Cryptography
{
  public abstract class DiffieHellman : AsymmetricAlgorithm
  {
    public new static DiffieHellman Create()
    {
      return DiffieHellman.Create("Mono.Security.Cryptography.DiffieHellman");
    }

    public new static DiffieHellman Create(string algName)
    {
      return (DiffieHellman) CryptoConfig.CreateFromName(algName);
    }

    public abstract byte[] CreateKeyExchange();

    public abstract byte[] DecryptKeyExchange(byte[] keyex);

    public abstract DHParameters ExportParameters(bool includePrivate);

    public abstract void ImportParameters(DHParameters parameters);

    private byte[] GetNamedParam(SecurityElement se, string param)
    {
      SecurityElement securityElement = se.SearchForChildByTag(param);
      return securityElement == null ? (byte[]) null : Convert.FromBase64String(securityElement.Text);
    }

    public override void FromXmlString(string xmlString)
    {
      if (xmlString == null)
        throw new ArgumentNullException(nameof (xmlString));
      DHParameters parameters = new DHParameters();
      try
      {
        SecurityParser securityParser = new SecurityParser();
        securityParser.LoadXml(xmlString);
        SecurityElement xml = securityParser.ToXml();
        if (xml.Tag != "DHKeyValue")
          throw new CryptographicException();
        parameters.P = this.GetNamedParam(xml, "P");
        parameters.G = this.GetNamedParam(xml, "G");
        parameters.X = this.GetNamedParam(xml, "X");
        this.ImportParameters(parameters);
      }
      finally
      {
        if (parameters.P != null)
          Array.Clear((Array) parameters.P, 0, parameters.P.Length);
        if (parameters.G != null)
          Array.Clear((Array) parameters.G, 0, parameters.G.Length);
        if (parameters.X != null)
          Array.Clear((Array) parameters.X, 0, parameters.X.Length);
      }
    }

    public override string ToXmlString(bool includePrivateParameters)
    {
      StringBuilder stringBuilder = new StringBuilder();
      DHParameters dhParameters = this.ExportParameters(includePrivateParameters);
      try
      {
        stringBuilder.Append("<DHKeyValue>");
        stringBuilder.Append("<P>");
        stringBuilder.Append(Convert.ToBase64String(dhParameters.P));
        stringBuilder.Append("</P>");
        stringBuilder.Append("<G>");
        stringBuilder.Append(Convert.ToBase64String(dhParameters.G));
        stringBuilder.Append("</G>");
        if (includePrivateParameters)
        {
          stringBuilder.Append("<X>");
          stringBuilder.Append(Convert.ToBase64String(dhParameters.X));
          stringBuilder.Append("</X>");
        }
        stringBuilder.Append("</DHKeyValue>");
      }
      finally
      {
        Array.Clear((Array) dhParameters.P, 0, dhParameters.P.Length);
        Array.Clear((Array) dhParameters.G, 0, dhParameters.G.Length);
        if (dhParameters.X != null)
          Array.Clear((Array) dhParameters.X, 0, dhParameters.X.Length);
      }
      return stringBuilder.ToString();
    }
  }
}
