// Decompiled with JetBrains decompiler
// Type: Mylib.Header
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

namespace Mylib
{
  public class Header
  {
    public int uid;
    public int imap_base;
    public bool flags_seen;
    public bool flags_forwarded;
    public bool flags_answered;
    public bool flags_deleted;
    public int size;
    public string header;
    private string my_subject;
    private string xfrom;
    private string xsubject;

    public string from
    {
      get
      {
        this.get_headers();
        return this.xfrom;
      }
    }

    public string subject
    {
      get
      {
        if (this.my_subject != null)
          return this.my_subject;
        this.get_headers();
        return this.xsubject;
      }
    }

    public void set_my_subject(string x)
    {
      this.my_subject = x;
    }

    private void get_headers()
    {
      Mime mime = new Mime(false);
      mime.decode(this.header);
      this.xfrom = mime.from;
      this.xsubject = mime.subject;
      stat.imsg("get_headers called, from ({0}) {1}", (object) mime.from, (object) mime.subject);
    }

    public Header(
      int xuid,
      string xheader,
      int xsize,
      bool xflags_seen,
      bool xf_answered,
      bool xf_forwarded,
      bool xf_deleted,
      int xbase)
    {
      this.imap_base = xbase;
      this.uid = xuid;
      this.header = xheader;
      this.size = xsize;
      this.flags_deleted = xf_deleted;
      this.flags_seen = xflags_seen;
      this.flags_answered = xf_answered;
      this.flags_forwarded = xf_forwarded;
    }

    public Header(int xuid, string xheader, int xsize)
      : this(xuid, xheader, xsize, false, false, false, false, 0)
    {
    }

    public Header(string fname)
    {
    }
  }
}
