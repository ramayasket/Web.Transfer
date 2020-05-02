// Decompiled with JetBrains decompiler
// Type: Mylib.Attachment
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

namespace Mylib
{
  public class Attachment
  {
    public string name;
    public string path;
    public string type;

    public Attachment(string n, string p, string t)
    {
      this.name = n;
      this.path = p;
      this.type = t;
    }
  }
}
