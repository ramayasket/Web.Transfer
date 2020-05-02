// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.X509StoreManager
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Collections;
using System.IO;

namespace Mono.Security.X509
{
  public sealed class X509StoreManager
  {
    private static string _userPath;
    private static string _localMachinePath;
    private static X509Stores _userStore;
    private static X509Stores _machineStore;

    private X509StoreManager()
    {
    }

    internal static string CurrentUserPath
    {
      get
      {
        if (X509StoreManager._userPath == null)
        {
          X509StoreManager._userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".mono");
          X509StoreManager._userPath = Path.Combine(X509StoreManager._userPath, "certs");
        }
        return X509StoreManager._userPath;
      }
    }

    internal static string LocalMachinePath
    {
      get
      {
        if (X509StoreManager._localMachinePath == null)
        {
          X509StoreManager._localMachinePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ".mono");
          X509StoreManager._localMachinePath = Path.Combine(X509StoreManager._localMachinePath, "certs");
        }
        return X509StoreManager._localMachinePath;
      }
    }

    public static X509Stores CurrentUser
    {
      get
      {
        if (X509StoreManager._userStore == null)
          X509StoreManager._userStore = new X509Stores(X509StoreManager.CurrentUserPath);
        return X509StoreManager._userStore;
      }
    }

    public static X509Stores LocalMachine
    {
      get
      {
        if (X509StoreManager._machineStore == null)
          X509StoreManager._machineStore = new X509Stores(X509StoreManager.LocalMachinePath);
        return X509StoreManager._machineStore;
      }
    }

    public static X509CertificateCollection IntermediateCACertificates
    {
      get
      {
        X509CertificateCollection certificateCollection = new X509CertificateCollection();
        certificateCollection.AddRange(X509StoreManager.CurrentUser.IntermediateCA.Certificates);
        certificateCollection.AddRange(X509StoreManager.LocalMachine.IntermediateCA.Certificates);
        return certificateCollection;
      }
    }

    public static ArrayList IntermediateCACrls
    {
      get
      {
        ArrayList arrayList = new ArrayList();
        arrayList.AddRange((ICollection) X509StoreManager.CurrentUser.IntermediateCA.Crls);
        arrayList.AddRange((ICollection) X509StoreManager.LocalMachine.IntermediateCA.Crls);
        return arrayList;
      }
    }

    public static X509CertificateCollection TrustedRootCertificates
    {
      get
      {
        X509CertificateCollection certificateCollection = new X509CertificateCollection();
        certificateCollection.AddRange(X509StoreManager.CurrentUser.TrustedRoot.Certificates);
        certificateCollection.AddRange(X509StoreManager.LocalMachine.TrustedRoot.Certificates);
        return certificateCollection;
      }
    }

    public static ArrayList TrustedRootCACrls
    {
      get
      {
        ArrayList arrayList = new ArrayList();
        arrayList.AddRange((ICollection) X509StoreManager.CurrentUser.TrustedRoot.Crls);
        arrayList.AddRange((ICollection) X509StoreManager.LocalMachine.TrustedRoot.Crls);
        return arrayList;
      }
    }

    public static X509CertificateCollection UntrustedCertificates
    {
      get
      {
        X509CertificateCollection certificateCollection = new X509CertificateCollection();
        certificateCollection.AddRange(X509StoreManager.CurrentUser.Untrusted.Certificates);
        certificateCollection.AddRange(X509StoreManager.LocalMachine.Untrusted.Certificates);
        return certificateCollection;
      }
    }
  }
}
