// Decompiled with JetBrains decompiler
// Type: Mono.Security.ASN1
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Mono.Security
{
  using System;
  using System.Collections;
  using System.IO;
  using System.Reflection;
  using System.Runtime.InteropServices;
  using System.Text;

  public class ASN1
  {
    private byte m_nTag;
    private byte[] m_aValue;
    private ArrayList elist;

    public ASN1() : this(0, null)
    {
    }

    public ASN1(byte tag) : this(tag, null)
    {
    }

    public ASN1(byte[] data)
    {
      this.m_nTag = data[0];
      int num = 0;
      int count = data[1];
      if (count > 0x80)
      {
        num = count - 0x80;
        count = 0;
        for (int i = 0; i < num; i++)
        {
          count *= 0x100;
          count += data[i + 2];
        }
      }
      else if (count == 0x80)
      {
        throw new NotSupportedException("Undefined length encoding.");
      }
      this.m_aValue = new byte[count];
      Buffer.BlockCopy(data, 2 + num, this.m_aValue, 0, count);
      if ((this.m_nTag & 0x20) == 0x20)
      {
        int anPos = 2 + num;
        this.Decode(data, ref anPos, data.Length);
      }
    }

    public ASN1(byte tag, byte[] data)
    {
      this.m_nTag = tag;
      this.m_aValue = data;
    }

    public ASN1 Add(ASN1 asn1)
    {
      if (asn1 != null)
      {
        if (this.elist == null)
        {
          this.elist = new ArrayList();
        }
        this.elist.Add(asn1);
      }
      return asn1;
    }

    private bool CompareArray(byte[] array1, byte[] array2)
    {
      bool flag = array1.Length == array2.Length;
      if (flag)
      {
        for (int i = 0; i < array1.Length; i++)
        {
          if (array1[i] != array2[i])
          {
            return false;
          }
        }
      }
      return flag;
    }

    public bool CompareValue(byte[] value) =>
        this.CompareArray(this.m_aValue, value);

    protected void Decode(byte[] asn1, ref int anPos, int anLength)
    {
      while (anPos < (anLength - 1))
      {
        this.DecodeTLV(asn1, ref anPos, out byte num, out int num2, out byte[] buffer);
        if (num != 0)
        {
          ASN1 asn = this.Add(new ASN1(num, buffer));
          if ((num & 0x20) == 0x20)
          {
            int num3 = anPos;
            asn.Decode(asn1, ref num3, num3 + num2);
          }
          anPos += num2;
        }
      }
    }

    protected void DecodeTLV(byte[] asn1, ref int pos, out byte tag, out int length, out byte[] content)
    {
      tag = asn1[pos++];
      length = asn1[pos++];
      if ((length & 0x80) == 0x80)
      {
        int num2 = length & 0x7f;
        length = 0;
        for (int i = 0; i < num2; i++)
        {
          length = (length * 0x100) + asn1[pos++];
        }
      }
      content = new byte[length];
      Buffer.BlockCopy(asn1, pos, content, 0, length);
    }

    public ASN1 Element(int index, byte anTag)
    {
      try
      {
        if ((this.elist != null) && (index < this.elist.Count))
        {
          ASN1 asn2 = (ASN1)this.elist[index];
          if (asn2.Tag == anTag)
          {
            return asn2;
          }
        }
        return null;
      }
      catch (ArgumentOutOfRangeException)
      {
        return null;
      }
    }

    public bool Equals(byte[] asn1) =>
        this.CompareArray(this.GetBytes(), asn1);

    public virtual byte[] GetBytes()
    {
      byte[] dst = null;
      byte[] buffer4;
      if (this.Count > 0)
      {
        int num = 0;
        ArrayList list = new ArrayList();
        IEnumerator enumerator = this.elist.GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
          {
            byte[] bytes = ((ASN1)enumerator.Current).GetBytes();
            list.Add(bytes);
            num += bytes.Length;
          }
        }
        finally
        {
          if (enumerator is IDisposable disposable)
          {
            disposable.Dispose();
          }
        }
        dst = new byte[num];
        int dstOffset = 0;
        for (int i = 0; i < this.elist.Count; i++)
        {
          byte[] src = (byte[])list[i];
          Buffer.BlockCopy(src, 0, dst, dstOffset, src.Length);
          dstOffset += src.Length;
        }
      }
      else if (this.m_aValue != null)
      {
        dst = this.m_aValue;
      }
      int num4 = 0;
      if (dst != null)
      {
        int length = dst.Length;
        if (length > 0x7f)
        {
          if (length <= 0xff)
          {
            buffer4 = new byte[3 + length];
            Buffer.BlockCopy(dst, 0, buffer4, 3, length);
            num4 = 0x81;
            buffer4[2] = (byte)length;
          }
          else if (length <= 0xffff)
          {
            buffer4 = new byte[4 + length];
            Buffer.BlockCopy(dst, 0, buffer4, 4, length);
            num4 = 130;
            buffer4[2] = (byte)(length >> 8);
            buffer4[3] = (byte)length;
          }
          else if (length <= 0xffffff)
          {
            buffer4 = new byte[5 + length];
            Buffer.BlockCopy(dst, 0, buffer4, 5, length);
            num4 = 0x83;
            buffer4[2] = (byte)(length >> 0x10);
            buffer4[3] = (byte)(length >> 8);
            buffer4[4] = (byte)length;
          }
          else
          {
            buffer4 = new byte[6 + length];
            Buffer.BlockCopy(dst, 0, buffer4, 6, length);
            num4 = 0x84;
            buffer4[2] = (byte)(length >> 0x18);
            buffer4[3] = (byte)(length >> 0x10);
            buffer4[4] = (byte)(length >> 8);
            buffer4[5] = (byte)length;
          }
        }
        else
        {
          buffer4 = new byte[2 + length];
          Buffer.BlockCopy(dst, 0, buffer4, 2, length);
          num4 = length;
        }
        if (this.m_aValue == null)
        {
          this.m_aValue = dst;
        }
      }
      else
      {
        buffer4 = new byte[2];
      }
      buffer4[0] = this.m_nTag;
      buffer4[1] = (byte)num4;
      return buffer4;
    }

    public void SaveToFile(string filename)
    {
      if (filename == null)
      {
        throw new ArgumentNullException("filename");
      }
      using (FileStream stream = File.Create(filename))
      {
        byte[] bytes = this.GetBytes();
        stream.Write(bytes, 0, bytes.Length);
      }
    }

    public override string ToString()
    {
      StringBuilder builder = new StringBuilder();
      builder.AppendFormat("Tag: {0} {1}", this.m_nTag.ToString("X2"), Environment.NewLine);
      builder.AppendFormat("Length: {0} {1}", this.Value.Length, Environment.NewLine);
      builder.Append("Value: ");
      builder.Append(Environment.NewLine);
      for (int i = 0; i < this.Value.Length; i++)
      {
        builder.AppendFormat("{0} ", this.Value[i].ToString("X2"));
        if (((i + 1) % 0x10) == 0)
        {
          builder.AppendFormat(Environment.NewLine, new object[0]);
        }
      }
      return builder.ToString();
    }

    public int Count => this.elist?.Count ?? 0;

    public byte Tag =>
        this.m_nTag;

    public int Length
    {
      get
      {
        if (this.m_aValue != null)
        {
          return this.m_aValue.Length;
        }
        return 0;
      }
    }

    public byte[] Value
    {
      get
      {
        if (this.m_aValue == null)
        {
          this.GetBytes();
        }
        return (byte[])this.m_aValue.Clone();
      }
      set
      {
        if (value != null)
        {
          this.m_aValue = (byte[])value.Clone();
        }
      }
    }

    public ASN1 this[int index]
    {
      get
      {
        try
        {
          if ((this.elist == null) || (index >= this.elist.Count))
          {
            return null;
          }
          return (ASN1)this.elist[index];
        }
        catch (ArgumentOutOfRangeException)
        {
          return null;
        }
      }
    }
  }
}
