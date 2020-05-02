// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.X509ExtensionCollection
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Collections;

namespace Mono.Security.X509
{
  public sealed class X509ExtensionCollection : CollectionBase, IEnumerable
  {
    private bool readOnly;

    public X509ExtensionCollection()
    {
    }

    public X509ExtensionCollection(ASN1 asn1)
      : this()
    {
      this.readOnly = true;
      if (asn1 == null)
        return;
      if (asn1.Tag != (byte) 48)
        throw new Exception("Invalid extensions format");
      for (int index = 0; index < asn1.Count; ++index)
        this.InnerList.Add((object) new X509Extension(asn1[index]));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.InnerList.GetEnumerator();
    }

    public int Add(X509Extension extension)
    {
      if (extension == null)
        throw new ArgumentNullException(nameof (extension));
      if (this.readOnly)
        throw new NotSupportedException("Extensions are read only");
      return this.InnerList.Add((object) extension);
    }

    public void AddRange(X509Extension[] extension)
    {
      if (extension == null)
        throw new ArgumentNullException(nameof (extension));
      if (this.readOnly)
        throw new NotSupportedException("Extensions are read only");
      for (int index = 0; index < extension.Length; ++index)
        this.InnerList.Add((object) extension[index]);
    }

    public void AddRange(X509ExtensionCollection collection)
    {
      if (collection == null)
        throw new ArgumentNullException(nameof (collection));
      if (this.readOnly)
        throw new NotSupportedException("Extensions are read only");
      for (int index = 0; index < collection.InnerList.Count; ++index)
        this.InnerList.Add((object) collection[index]);
    }

    public bool Contains(X509Extension extension)
    {
      return this.IndexOf(extension) != -1;
    }

    public bool Contains(string oid)
    {
      return this.IndexOf(oid) != -1;
    }

    public void CopyTo(X509Extension[] extensions, int index)
    {
      if (extensions == null)
        throw new ArgumentNullException(nameof (extensions));
      this.InnerList.CopyTo((Array) extensions, index);
    }

    public int IndexOf(X509Extension extension)
    {
      if (extension == null)
        throw new ArgumentNullException(nameof (extension));
      for (int index = 0; index < this.InnerList.Count; ++index)
      {
        if (((X509Extension) this.InnerList[index]).Equals((object) extension))
          return index;
      }
      return -1;
    }

    public int IndexOf(string oid)
    {
      if (oid == null)
        throw new ArgumentNullException(nameof (oid));
      for (int index = 0; index < this.InnerList.Count; ++index)
      {
        if (((X509Extension) this.InnerList[index]).Oid == oid)
          return index;
      }
      return -1;
    }

    public void Insert(int index, X509Extension extension)
    {
      if (extension == null)
        throw new ArgumentNullException(nameof (extension));
      this.InnerList.Insert(index, (object) extension);
    }

    public void Remove(X509Extension extension)
    {
      if (extension == null)
        throw new ArgumentNullException(nameof (extension));
      this.InnerList.Remove((object) extension);
    }

    public void Remove(string oid)
    {
      if (oid == null)
        throw new ArgumentNullException(nameof (oid));
      int index = this.IndexOf(oid);
      if (index == -1)
        return;
      this.InnerList.RemoveAt(index);
    }

    public X509Extension this[int index]
    {
      get
      {
        return (X509Extension) this.InnerList[index];
      }
    }

    public X509Extension this[string oid]
    {
      get
      {
        int index = this.IndexOf(oid);
        return index == -1 ? (X509Extension) null : (X509Extension) this.InnerList[index];
      }
    }

    public byte[] GetBytes()
    {
      if (this.InnerList.Count < 1)
        return (byte[]) null;
      ASN1 asN1 = new ASN1((byte) 48);
      for (int index = 0; index < this.InnerList.Count; ++index)
      {
        X509Extension inner = (X509Extension) this.InnerList[index];
        asN1.Add(inner.ASN1);
      }
      return asN1.GetBytes();
    }
  }
}
