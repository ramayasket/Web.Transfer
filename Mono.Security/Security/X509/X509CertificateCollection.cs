// Decompiled with JetBrains decompiler
// Type: Mono.Security.X509.X509CertificateCollection
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Collections;

namespace Mono.Security.X509
{
  [Serializable]
  public class X509CertificateCollection : CollectionBase, IEnumerable
  {
    public X509CertificateCollection()
    {
    }

    public X509CertificateCollection(X509Certificate[] value)
    {
      this.AddRange(value);
    }

    public X509CertificateCollection(X509CertificateCollection value)
    {
      this.AddRange(value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.InnerList.GetEnumerator();
    }

    public X509Certificate this[int index]
    {
      get
      {
        return (X509Certificate) this.InnerList[index];
      }
      set
      {
        this.InnerList[index] = (object) value;
      }
    }

    public int Add(X509Certificate value)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      return this.InnerList.Add((object) value);
    }

    public void AddRange(X509Certificate[] value)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      for (int index = 0; index < value.Length; ++index)
        this.InnerList.Add((object) value[index]);
    }

    public void AddRange(X509CertificateCollection value)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      for (int index = 0; index < value.InnerList.Count; ++index)
        this.InnerList.Add((object) value[index]);
    }

    public bool Contains(X509Certificate value)
    {
      return this.IndexOf(value) != -1;
    }

    public void CopyTo(X509Certificate[] array, int index)
    {
      this.InnerList.CopyTo((Array) array, index);
    }

    public new X509CertificateCollection.X509CertificateEnumerator GetEnumerator()
    {
      return new X509CertificateCollection.X509CertificateEnumerator(this);
    }

    public override int GetHashCode()
    {
      return this.InnerList.GetHashCode();
    }

    public int IndexOf(X509Certificate value)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      byte[] hash = value.Hash;
      for (int index = 0; index < this.InnerList.Count; ++index)
      {
        if (this.Compare(((X509Certificate) this.InnerList[index]).Hash, hash))
          return index;
      }
      return -1;
    }

    public void Insert(int index, X509Certificate value)
    {
      this.InnerList.Insert(index, (object) value);
    }

    public void Remove(X509Certificate value)
    {
      this.InnerList.Remove((object) value);
    }

    private bool Compare(byte[] array1, byte[] array2)
    {
      if (array1 == null && array2 == null)
        return true;
      if (array1 == null || array2 == null || array1.Length != array2.Length)
        return false;
      for (int index = 0; index < array1.Length; ++index)
      {
        if ((int) array1[index] != (int) array2[index])
          return false;
      }
      return true;
    }

    public class X509CertificateEnumerator : IEnumerator
    {
      private IEnumerator enumerator;

      public X509CertificateEnumerator(X509CertificateCollection mappings)
      {
        this.enumerator = ((IEnumerable) mappings).GetEnumerator();
      }

      object IEnumerator.Current
      {
        get
        {
          return this.enumerator.Current;
        }
      }

      bool IEnumerator.MoveNext()
      {
        return this.enumerator.MoveNext();
      }

      void IEnumerator.Reset()
      {
        this.enumerator.Reset();
      }

      public X509Certificate Current
      {
        get
        {
          return (X509Certificate) this.enumerator.Current;
        }
      }

      public bool MoveNext()
      {
        return this.enumerator.MoveNext();
      }

      public void Reset()
      {
        this.enumerator.Reset();
      }
    }
  }
}
