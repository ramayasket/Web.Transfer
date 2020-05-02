// Decompiled with JetBrains decompiler
// Type: mycloud.XResponse
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System.Xml;

namespace mycloud
{
  internal class XResponse
  {
    public XmlNode n;
    public string reason;

    public XResponse(XmlNode n, string reason)
    {
      this.n = n;
      this.reason = reason;
    }
  }
}
