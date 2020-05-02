// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.X509Stores
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.IO;

namespace Mono.Security.X509
{
  public class X509Stores
  {
    private string _storePath;
    private X509Store _personal;
    private X509Store _other;
    private X509Store _intermediate;
    private X509Store _trusted;
    private X509Store _untrusted;

    internal X509Stores(string path)
    {
      this._storePath = path;
    }

    public X509Store Personal
    {
      get
      {
        if (this._personal == null)
          this._personal = new X509Store(Path.Combine(this._storePath, "My"), false);
        return this._personal;
      }
    }

    public X509Store OtherPeople
    {
      get
      {
        if (this._other == null)
          this._other = new X509Store(Path.Combine(this._storePath, "AddressBook"), false);
        return this._other;
      }
    }

    public X509Store IntermediateCA
    {
      get
      {
        if (this._intermediate == null)
          this._intermediate = new X509Store(Path.Combine(this._storePath, "CA"), true);
        return this._intermediate;
      }
    }

    public X509Store TrustedRoot
    {
      get
      {
        if (this._trusted == null)
          this._trusted = new X509Store(Path.Combine(this._storePath, "Trust"), true);
        return this._trusted;
      }
    }

    public X509Store Untrusted
    {
      get
      {
        if (this._untrusted == null)
          this._untrusted = new X509Store(Path.Combine(this._storePath, "Disallowed"), false);
        return this._untrusted;
      }
    }

    public void Clear()
    {
      if (this._personal != null)
        this._personal.Clear();
      this._personal = (X509Store) null;
      if (this._other != null)
        this._other.Clear();
      this._other = (X509Store) null;
      if (this._intermediate != null)
        this._intermediate.Clear();
      this._intermediate = (X509Store) null;
      if (this._trusted != null)
        this._trusted.Clear();
      this._trusted = (X509Store) null;
      if (this._untrusted != null)
        this._untrusted.Clear();
      this._untrusted = (X509Store) null;
    }

    public X509Store Open(string storeName, bool create)
    {
      if (storeName == null)
        throw new ArgumentNullException(nameof (storeName));
      string path = Path.Combine(this._storePath, storeName);
      return !create && !Directory.Exists(path) ? (X509Store) null : new X509Store(path, true);
    }

    public class Names
    {
      public const string Personal = "My";
      public const string OtherPeople = "AddressBook";
      public const string IntermediateCA = "CA";
      public const string TrustedRoot = "Trust";
      public const string Untrusted = "Disallowed";
    }
  }
}
