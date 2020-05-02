// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.CipherSuiteCollection
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Collections;
using System.Globalization;

namespace Mono.Security.Protocol.Tls
{
  internal sealed class CipherSuiteCollection : IEnumerable, IList, ICollection
  {
    private ArrayList cipherSuites;
    private SecurityProtocolType protocol;

    public CipherSuiteCollection(SecurityProtocolType protocol)
    {
      this.protocol = protocol;
      this.cipherSuites = new ArrayList();
    }

    object IList.this[int index]
    {
      get
      {
        return (object) this[index];
      }
      set
      {
        this[index] = (CipherSuite) value;
      }
    }

    bool ICollection.IsSynchronized
    {
      get
      {
        return this.cipherSuites.IsSynchronized;
      }
    }

    object ICollection.SyncRoot
    {
      get
      {
        return this.cipherSuites.SyncRoot;
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.cipherSuites.GetEnumerator();
    }

    bool IList.Contains(object value)
    {
      return this.cipherSuites.Contains((object) (value as CipherSuite));
    }

    int IList.IndexOf(object value)
    {
      return this.cipherSuites.IndexOf((object) (value as CipherSuite));
    }

    void IList.Insert(int index, object value)
    {
      this.cipherSuites.Insert(index, (object) (value as CipherSuite));
    }

    void IList.Remove(object value)
    {
      this.cipherSuites.Remove((object) (value as CipherSuite));
    }

    void IList.RemoveAt(int index)
    {
      this.cipherSuites.RemoveAt(index);
    }

    int IList.Add(object value)
    {
      return this.cipherSuites.Add((object) (value as CipherSuite));
    }

    public CipherSuite this[string name]
    {
      get
      {
        return (CipherSuite) this.cipherSuites[this.IndexOf(name)];
      }
      set
      {
        this.cipherSuites[this.IndexOf(name)] = (object) value;
      }
    }

    public CipherSuite this[int index]
    {
      get
      {
        return (CipherSuite) this.cipherSuites[index];
      }
      set
      {
        this.cipherSuites[index] = (object) value;
      }
    }

    public CipherSuite this[short code]
    {
      get
      {
        return (CipherSuite) this.cipherSuites[this.IndexOf(code)];
      }
      set
      {
        this.cipherSuites[this.IndexOf(code)] = (object) value;
      }
    }

    public int Count
    {
      get
      {
        return this.cipherSuites.Count;
      }
    }

    public bool IsFixedSize
    {
      get
      {
        return this.cipherSuites.IsFixedSize;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return this.cipherSuites.IsReadOnly;
      }
    }

    public void CopyTo(Array array, int index)
    {
      this.cipherSuites.CopyTo(array, index);
    }

    public void Clear()
    {
      this.cipherSuites.Clear();
    }

    public int IndexOf(string name)
    {
      int num = 0;
      foreach (CipherSuite cipherSuite in this.cipherSuites)
      {
        if (this.cultureAwareCompare(cipherSuite.Name, name))
          return num;
        ++num;
      }
      return -1;
    }

    public int IndexOf(short code)
    {
      int num = 0;
      foreach (CipherSuite cipherSuite in this.cipherSuites)
      {
        if ((int) cipherSuite.Code == (int) code)
          return num;
        ++num;
      }
      return -1;
    }

    public CipherSuite Add(
      short code,
      string name,
      CipherAlgorithmType cipherType,
      HashAlgorithmType hashType,
      ExchangeAlgorithmType exchangeType,
      bool exportable,
      bool blockMode,
      byte keyMaterialSize,
      byte expandedKeyMaterialSize,
      short effectiveKeyBytes,
      byte ivSize,
      byte blockSize)
    {
      switch (this.protocol)
      {
        case SecurityProtocolType.Default:
        case SecurityProtocolType.Tls:
          return (CipherSuite) this.add(new TlsCipherSuite(code, name, cipherType, hashType, exchangeType, exportable, blockMode, keyMaterialSize, expandedKeyMaterialSize, effectiveKeyBytes, ivSize, blockSize));
        case SecurityProtocolType.Ssl3:
          return (CipherSuite) this.add(new SslCipherSuite(code, name, cipherType, hashType, exchangeType, exportable, blockMode, keyMaterialSize, expandedKeyMaterialSize, effectiveKeyBytes, ivSize, blockSize));
        default:
          throw new NotSupportedException("Unsupported security protocol type.");
      }
    }

    private TlsCipherSuite add(TlsCipherSuite cipherSuite)
    {
      this.cipherSuites.Add((object) cipherSuite);
      return cipherSuite;
    }

    private SslCipherSuite add(SslCipherSuite cipherSuite)
    {
      this.cipherSuites.Add((object) cipherSuite);
      return cipherSuite;
    }

    private bool cultureAwareCompare(string strA, string strB)
    {
      return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth) == 0;
    }
  }
}
