// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.X509Chain
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.X509.Extensions;
using System.Net;
using System.Security.Permissions;

namespace Mono.Security.X509
{
  public class X509Chain
  {
    private X509CertificateCollection roots;
    private X509CertificateCollection certs;
    private X509Certificate _root;
    private X509CertificateCollection _chain;
    private X509ChainStatusFlags _status;

    public X509Chain()
    {
      this.certs = new X509CertificateCollection();
    }

    public X509Chain(X509CertificateCollection chain)
      : this()
    {
      this._chain = new X509CertificateCollection();
      this._chain.AddRange(chain);
    }

    public X509CertificateCollection Chain
    {
      get
      {
        return this._chain;
      }
    }

    public X509Certificate Root
    {
      get
      {
        return this._root;
      }
    }

    public X509ChainStatusFlags Status
    {
      get
      {
        return this._status;
      }
    }

    public X509CertificateCollection TrustAnchors
    {
      get
      {
        if (this.roots != null)
          return this.roots;
        this.roots = new X509CertificateCollection();
        this.roots.AddRange(X509StoreManager.TrustedRootCertificates);
        return this.roots;
      }
      [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPolicy)] set
      {
        this.roots = value;
      }
    }

    public void LoadCertificate(X509Certificate x509)
    {
      this.certs.Add(x509);
    }

    public void LoadCertificates(X509CertificateCollection collection)
    {
      this.certs.AddRange(collection);
    }

    public X509Certificate FindByIssuerName(string issuerName)
    {
      foreach (X509Certificate cert in this.certs)
      {
        if (cert.IssuerName == issuerName)
          return cert;
      }
      return (X509Certificate) null;
    }

    public bool Build(X509Certificate leaf)
    {
      this._status = X509ChainStatusFlags.NoError;
      if (this._chain == null)
      {
        this._chain = new X509CertificateCollection();
        X509Certificate child = leaf;
        X509Certificate potentialRoot = child;
        for (; child != null && !child.IsSelfSigned; child = this.FindCertificateParent(child))
        {
          potentialRoot = child;
          this._chain.Add(child);
        }
        this._root = this.FindCertificateRoot(potentialRoot);
      }
      else
      {
        int count = this._chain.Count;
        if (count > 0)
        {
          if (this.IsParent(leaf, this._chain[0]))
          {
            int index = 1;
            while (index < count && this.IsParent(this._chain[index - 1], this._chain[index]))
              ++index;
            if (index == count)
              this._root = this.FindCertificateRoot(this._chain[count - 1]);
          }
        }
        else
          this._root = this.FindCertificateRoot(leaf);
      }
      if (this._chain != null && this._status == X509ChainStatusFlags.NoError)
      {
        foreach (X509Certificate cert in this._chain)
        {
          if (!this.IsValid(cert))
            return false;
        }
        if (!this.IsValid(leaf))
        {
          if (this._status == X509ChainStatusFlags.NotTimeNested)
            this._status = X509ChainStatusFlags.NotTimeValid;
          return false;
        }
        if (this._root != null && !this.IsValid(this._root))
          return false;
      }
      return this._status == X509ChainStatusFlags.NoError;
    }

    public void Reset()
    {
      this._status = X509ChainStatusFlags.NoError;
      this.roots = (X509CertificateCollection) null;
      this.certs.Clear();
      if (this._chain == null)
        return;
      this._chain.Clear();
    }

    private bool IsValid(X509Certificate cert)
    {
      if (!cert.IsCurrent)
      {
        this._status = X509ChainStatusFlags.NotTimeNested;
        return false;
      }

      if (!ServicePointManager.CheckCertificateRevocationList)
      {
      }

      return true;
    }

    private X509Certificate FindCertificateParent(X509Certificate child)
    {
      foreach (X509Certificate cert in this.certs)
      {
        if (this.IsParent(child, cert))
          return cert;
      }
      return (X509Certificate) null;
    }

    private X509Certificate FindCertificateRoot(X509Certificate potentialRoot)
    {
      if (potentialRoot == null)
      {
        this._status = X509ChainStatusFlags.PartialChain;
        return (X509Certificate) null;
      }
      if (this.IsTrusted(potentialRoot))
        return potentialRoot;
      foreach (X509Certificate trustAnchor in this.TrustAnchors)
      {
        if (this.IsParent(potentialRoot, trustAnchor))
          return trustAnchor;
      }
      if (potentialRoot.IsSelfSigned)
      {
        this._status = X509ChainStatusFlags.UntrustedRoot;
        return potentialRoot;
      }
      this._status = X509ChainStatusFlags.PartialChain;
      return (X509Certificate) null;
    }

    private bool IsTrusted(X509Certificate potentialTrusted)
    {
      return this.TrustAnchors.Contains(potentialTrusted);
    }

    private bool IsParent(X509Certificate child, X509Certificate parent)
    {
      if (child.IssuerName != parent.SubjectName)
        return false;
      if (parent.Version > 2 && !this.IsTrusted(parent))
      {
        X509Extension extension = parent.Extensions["2.5.29.19"];
        if (extension != null)
        {
          if (!new BasicConstraintsExtension(extension).CertificateAuthority)
            this._status = X509ChainStatusFlags.InvalidBasicConstraints;
        }
        else
          this._status = X509ChainStatusFlags.InvalidBasicConstraints;
      }
      if (child.VerifySignature(parent.RSA))
        return true;
      this._status = X509ChainStatusFlags.NotSignatureValid;
      return false;
    }
  }
}
