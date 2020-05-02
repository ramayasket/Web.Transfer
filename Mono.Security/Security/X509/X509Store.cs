// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.X509Store
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using Mono.Security.X509.Extensions;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security.X509
{
  public class X509Store
  {
    private string _storePath;
    private X509CertificateCollection _certificates;
    private ArrayList _crls;
    private bool _crl;
    private string _name;

    internal X509Store(string path, bool crl)
    {
      this._storePath = path;
      this._crl = crl;
    }

    public X509CertificateCollection Certificates
    {
      get
      {
        if (this._certificates == null)
          this._certificates = this.BuildCertificatesCollection(this._storePath);
        return this._certificates;
      }
    }

    public ArrayList Crls
    {
      get
      {
        if (!this._crl)
          this._crls = new ArrayList();
        if (this._crls == null)
          this._crls = this.BuildCrlsCollection(this._storePath);
        return this._crls;
      }
    }

    public string Name
    {
      get
      {
        if (this._name == null)
          this._name = this._storePath.Substring(this._storePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        return this._name;
      }
    }

    public void Clear()
    {
      if (this._certificates != null)
        this._certificates.Clear();
      this._certificates = (X509CertificateCollection) null;
      if (this._crls != null)
        this._crls.Clear();
      this._crls = (ArrayList) null;
    }

    public void Import(X509Certificate certificate)
    {
      this.CheckStore(this._storePath, true);
      string path = Path.Combine(this._storePath, this.GetUniqueName(certificate));
      if (!File.Exists(path))
      {
        using (FileStream fileStream = File.Create(path))
        {
          byte[] rawData = certificate.RawData;
          fileStream.Write(rawData, 0, rawData.Length);
          fileStream.Close();
        }
      }
      CspParameters cspParams = new CspParameters();
      cspParams.KeyContainerName = CryptoConvert.ToHex(certificate.Hash);
      if (this._storePath.StartsWith(X509StoreManager.LocalMachinePath))
        cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
      this.ImportPrivateKey(certificate, cspParams);
    }

    public void Import(X509Crl crl)
    {
      this.CheckStore(this._storePath, true);
      string path = Path.Combine(this._storePath, this.GetUniqueName(crl));
      if (File.Exists(path))
        return;
      using (FileStream fileStream = File.Create(path))
      {
        byte[] rawData = crl.RawData;
        fileStream.Write(rawData, 0, rawData.Length);
      }
    }

    public void Remove(X509Certificate certificate)
    {
      string path = Path.Combine(this._storePath, this.GetUniqueName(certificate));
      if (!File.Exists(path))
        return;
      File.Delete(path);
    }

    public void Remove(X509Crl crl)
    {
      string path = Path.Combine(this._storePath, this.GetUniqueName(crl));
      if (!File.Exists(path))
        return;
      File.Delete(path);
    }

    private string GetUniqueName(X509Certificate certificate)
    {
      byte[] name = this.GetUniqueName(certificate.Extensions);
      string method;
      if (name == null)
      {
        method = "tbp";
        name = certificate.Hash;
      }
      else
        method = "ski";
      return this.GetUniqueName(method, name, ".cer");
    }

    private string GetUniqueName(X509Crl crl)
    {
      byte[] name = this.GetUniqueName(crl.Extensions);
      string method;
      if (name == null)
      {
        method = "tbp";
        name = crl.Hash;
      }
      else
        method = "ski";
      return this.GetUniqueName(method, name, ".crl");
    }

    private byte[] GetUniqueName(X509ExtensionCollection extensions)
    {
      X509Extension extension = extensions["2.5.29.14"];
      return extension == null ? (byte[]) null : new SubjectKeyIdentifierExtension(extension).Identifier;
    }

    private string GetUniqueName(string method, byte[] name, string fileExtension)
    {
      StringBuilder stringBuilder = new StringBuilder(method);
      stringBuilder.Append("-");
      foreach (byte num in name)
        stringBuilder.Append(num.ToString("X2", (IFormatProvider) CultureInfo.InvariantCulture));
      stringBuilder.Append(fileExtension);
      return stringBuilder.ToString();
    }

    private byte[] Load(string filename)
    {
      byte[] buffer = (byte[]) null;
      using (FileStream fileStream = File.OpenRead(filename))
      {
        buffer = new byte[fileStream.Length];
        fileStream.Read(buffer, 0, buffer.Length);
        fileStream.Close();
      }
      return buffer;
    }

    private X509Certificate LoadCertificate(string filename)
    {
      X509Certificate x509Certificate = new X509Certificate(this.Load(filename));
      CspParameters parameters = new CspParameters();
      parameters.KeyContainerName = CryptoConvert.ToHex(x509Certificate.Hash);
      if (this._storePath.StartsWith(X509StoreManager.LocalMachinePath))
        parameters.Flags = CspProviderFlags.UseMachineKeyStore;
      if (!new KeyPairPersistence(parameters).Load())
        return x509Certificate;
      if (x509Certificate.RSA != null)
        x509Certificate.RSA = (RSA) new RSACryptoServiceProvider(parameters);
      else if (x509Certificate.DSA != null)
        x509Certificate.DSA = (DSA) new DSACryptoServiceProvider(parameters);
      return x509Certificate;
    }

    private X509Crl LoadCrl(string filename)
    {
      return new X509Crl(this.Load(filename));
    }

    private bool CheckStore(string path, bool throwException)
    {
      try
      {
        if (Directory.Exists(path))
          return true;
        Directory.CreateDirectory(path);
        return Directory.Exists(path);
      }
      catch
      {
        if (!throwException)
          return false;
        throw;
      }
    }

    private X509CertificateCollection BuildCertificatesCollection(
      string storeName)
    {
      X509CertificateCollection certificateCollection = new X509CertificateCollection();
      string path = Path.Combine(this._storePath, storeName);
      if (!this.CheckStore(path, false))
        return certificateCollection;
      string[] files = Directory.GetFiles(path, "*.cer");
      if (files != null && files.Length > 0)
      {
        foreach (string filename in files)
        {
          try
          {
            X509Certificate x509Certificate = this.LoadCertificate(filename);
            certificateCollection.Add(x509Certificate);
          }
          catch
          {
          }
        }
      }
      return certificateCollection;
    }

    private ArrayList BuildCrlsCollection(string storeName)
    {
      ArrayList arrayList = new ArrayList();
      string path = Path.Combine(this._storePath, storeName);
      if (!this.CheckStore(path, false))
        return arrayList;
      string[] files = Directory.GetFiles(path, "*.crl");
      if (files != null && files.Length > 0)
      {
        foreach (string filename in files)
        {
          try
          {
            X509Crl x509Crl = this.LoadCrl(filename);
            arrayList.Add((object) x509Crl);
          }
          catch
          {
          }
        }
      }
      return arrayList;
    }

    private void ImportPrivateKey(X509Certificate certificate, CspParameters cspParams)
    {
      if (certificate.RSA is RSACryptoServiceProvider rsa)
      {
        if (rsa.PublicOnly)
          return;
        RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider(cspParams);
        cryptoServiceProvider.ImportParameters(rsa.ExportParameters(true));
        cryptoServiceProvider.PersistKeyInCsp = true;
      }
      else if (certificate.RSA is RSAManaged rsa1)
      {
        if (rsa1.PublicOnly)
          return;
        RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider(cspParams);
        cryptoServiceProvider.ImportParameters(rsa1.ExportParameters(true));
        cryptoServiceProvider.PersistKeyInCsp = true;
      }
      else
      {
        if (!(certificate.DSA is DSACryptoServiceProvider dsa) || dsa.PublicOnly)
          return;
        DSACryptoServiceProvider cryptoServiceProvider = new DSACryptoServiceProvider(cspParams);
        cryptoServiceProvider.ImportParameters(dsa.ExportParameters(true));
        cryptoServiceProvider.PersistKeyInCsp = true;
      }
    }
  }
}
