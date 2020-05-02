// Decompiled with JetBrains decompiler
// Type: Mylib.Mime
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace Mylib
{
  public class Mime
  {
    public bool wasflowed = false;
    private StringBuilder form_val = new StringBuilder();
    public Attachments attachments = new Attachments();
    private Mime.encoding current_encoding = Mime.encoding.none;
    private MyBuffer linebf = new MyBuffer();
    private int xid = 0;
    private int msgi = 0;
    private List<string> boundary = new List<string>();
    public StringBuilder html = new StringBuilder();
    public StringBuilder text = new StringBuilder();
    public Pairs headers = new Pairs();
    public NameValueCollection form = new NameValueCollection();
    private string path;
    private bool isflowed;
    private Encoding textenc;
    private bool done_cleanup;
    public bool has_attachments;
    private bool gobble_indent;
    private static int na;
    private bool ismsgheader;
    private bool ismsgpart;
    private string part_subj;
    private string part_from;
    private string part_fname;
    private string form_var;
    private bool inform;
    private FileStream attach_stream;
    private string attach_fname;
    private string attach_original;
    public string from;
    public string subject;
    public string content_type;
    public string mid;
    public string to;
    private bool inmimeheader;
    private string current_cid;
    private string fname;
    private FileStream fout;
    private bool do_attach;
    private bool inhead;
    private bool intextpart;
    private bool inhtmlpart;
    private bool inattachpart;
    private string lasthead;

    private void init()
    {
      this.headers.clear();
      this.from = "";
      this.subject = "";
      this.mid = "";
      this.inhead = true;
      this.has_attachments = false;
    }

    public void cleanup()
    {
      if (!this.do_attach || this.done_cleanup)
        return;
      int num = this.attachments.Count();
      this.done_cleanup = true;
      for (int i = 0; i < num; ++i)
      {
        Attachment attachment = this.attachments.get(i);
        try
        {
          clib.imsg("Deleting file {0}", (object) attachment.path);
          File.Delete(attachment.path);
        }
        catch
        {
          clib.imsg("Need to gracefully delete this file later ****  {0}", (object) attachment.path);
        }
      }
    }

    ~Mime()
    {
      this.cleanup();
    }

    public void setpath(string x)
    {
      this.path = x;
    }

    public Mime(bool xdo_attach)
    {
      this.do_attach = xdo_attach;
      this.init();
    }

    public bool decode_start(string ctype)
    {
      this.init();
      this.gobble_indent = false;
      this.set_content(ctype);
      this.inhead = false;
      return true;
    }

    public bool decode_packet(byte[] inbf, int inlen)
    {
      this.linebf.add(inbf, inlen);
      while (true)
      {
        byte[] bob = this.linebf.getline();
        if (bob != null)
          this.decode_line(0, clib.byte_to_string(bob, ((IEnumerable<byte>) bob).Count<byte>()));
        else
          break;
      }
      return true;
    }

    public bool decode_end()
    {
      this.decode_done();
      return true;
    }

    public bool decode(string msg)
    {
      string[] separator = new string[1]{ "\r\n" };
      this.init();
      this.gobble_indent = false;
      string[] strArray = msg.Split(separator, StringSplitOptions.None);
      for (int linei = 0; linei < ((IEnumerable<string>) strArray).Count<string>(); ++linei)
        this.decode_line(linei, strArray[linei]);
      this.decode_done();
      return true;
    }

    private void decode_done()
    {
      string str1 = clib.trim_trailing(this.text.ToString(), "\r\n ") + "\r\n";
      this.text.Length = 0;
      this.text.Append(str1);
      if (this.textenc != null)
      {
        if (this.html != null && this.html.Length > 0)
        {
          byte[] bytes = clib.string_to_byte(this.html.ToString());
          if (bytes != null && bytes.Length > 0)
          {
            string str2 = this.textenc.GetString(bytes, 0, bytes.Length);
            if (str2 != null)
              this.html = new StringBuilder(str2);
          }
        }
        if (this.text != null && this.text.Length > 0)
        {
          byte[] bytes = clib.string_to_byte(this.text.ToString());
          if (bytes != null && bytes.Length > 0)
          {
            string str2 = this.textenc.GetString(bytes, 0, bytes.Length);
            if (str2 != null)
              this.text = new StringBuilder(str2);
          }
        }
      }
      this.form_done();
      this.attach_done();
    }

    private void form_done()
    {
      if (this.inform)
      {
        if (this.attach_stream != null)
        {
          this.form.Add(this.form_var, this.attach_fname);
          if (this.attach_original != null)
            this.form.Add(this.form_var + "_original", this.attach_original);
          this.attach_stream.Close();
          this.attach_stream = (FileStream) null;
        }
        else
        {
          string str = this.form_val.ToString();
          this.form.Add(this.form_var, str.Substring(0, str.Length - 2));
        }
        this.form_var = "";
      }
      this.inform = false;
    }

    private void attach_done()
    {
      if (!this.inattachpart)
        return;
      if (this.fout != null)
      {
        this.fout.Close();
        this.fout.Dispose();
        this.fout = (FileStream) null;
      }
      this.inattachpart = false;
      if (this.current_cid == null)
        this.current_cid = string.Format("fakecid_{0}", (object) this.xid++);
      this.attachments.Add(clib.unangle(this.current_cid), this.fname, this.content_type);
      this.current_cid = (string) null;
    }

    public void decode_line(int linei, string line)
    {
      // N.B.! probably this empty 'if' statement is residual from debugging.
      // if (!this.inhead)
      // {
      // }

      if (this.inhead)
        this.decode_head(linei, line);
      else
        this.decode_body(linei, line);
    }

    public string get_header(string x)
    {
      string b = "";
      this.headers.TryGetValue(x, out b);
      return b;
    }

    public string get_to_list()
    {
      string header = this.get_header("Cc");
      return header == null ? this.to : this.to + "," + header;
    }

    private void add_last(string line)
    {
      string b;
      this.headers.TryGetValue(this.lasthead, out b);
      this.headers.Remove(this.lasthead);
      this.headers.Add(this.lasthead, b + " " + line.Substring(1));
    }

    private void add_boundary(string bf)
    {
      int startIndex = bf.IndexOf("boundary=", StringComparison.OrdinalIgnoreCase);
      if (startIndex > 0)
        startIndex += 9;
      if (startIndex < 0)
      {
        int num = bf.IndexOf("boundary =", StringComparison.OrdinalIgnoreCase);
        if (num <= 0)
          return;
        startIndex = num + 10;
      }
      if (bf[startIndex] == ' ')
        ++startIndex;
      if (bf[startIndex] == '"')
        ++startIndex;
      int length1 = bf.Length - startIndex;
      int num1 = bf.IndexOf('"', startIndex);
      if (num1 > 0)
        length1 = num1 - startIndex;
      string str = bf.Substring(startIndex, length1);
      int length2 = str.IndexOf(';');
      if (length2 > 0)
        str = str.Substring(0, length2);
      this.boundary.Add(str);
    }

    private void get_content_type()
    {
      string b;
      this.headers.TryGetValue("Content-Type", out b);
      if (b != null)
        this.set_content(b);
      else
        this.intextpart = true;
      this.headers.TryGetValue("Content-Transfer-Encoding", out b);
      if (b == null)
        return;
      this.set_transfer(b);
    }

    private void set_transfer(string bf)
    {
      bf = bf.ToLower();
      this.current_encoding = Mime.encoding.none;
      if (bf.Contains("quoted"))
        this.current_encoding = Mime.encoding.quoted;
      if (!bf.Contains("base64"))
        return;
      this.current_encoding = Mime.encoding.base64;
    }

    private void set_cid(string bf)
    {
      this.current_cid = bf;
    }

    private string get_simple_name(string name)
    {
      return clib.get_simple_name(name);
    }

    private string get_name(string content)
    {
      int num1 = content.IndexOf("name=", StringComparison.OrdinalIgnoreCase);
      if (num1 < 0)
        return "none";
      int startIndex = num1 + 5;
      int num2;
      if (content[startIndex] == '"')
      {
        ++startIndex;
        num2 = content.IndexOf('"', startIndex);
      }
      else
        num2 = content.IndexOf(' ', startIndex);
      if (num2 < 0)
        num2 = content.Length;
      return content.Substring(startIndex, num2 - startIndex);
    }

    private void find_name(string bf)
    {
      if (this.fout != null || !bf.Contains("name="))
        return;
      this.has_attachments = true;
      if (!this.do_attach || this.path == null)
        return;
      this.fname = this.get_name(bf);
      this.fname = this.get_simple_name(this.fname);
      this.fname = clib.wash_file_name(this.fname);
      this.inattachpart = true;
      Directory.CreateDirectory(this.path);
      this.fname = string.Format("{0}/{1}", (object) this.path, (object) this.fname);
      try
      {
        this.part_fname = this.fname;
        this.fout = File.Open(this.fname, FileMode.Create);
      }
      catch (Exception ex)
      {
        clib.imsg("Open {0} {1}", (object) this.fname, (object) ex.ToString());
        this.fout = (FileStream) null;
      }
    }

    private void set_continue(string bf)
    {
      bf = bf.ToLower();
      if (bf.Contains("iso-8859"))
        this.textenc = Encoding.GetEncoding("iso-8859-1");
      if (!bf.Contains("utf-8"))
        return;
      this.textenc = Encoding.UTF8;
    }

    private void set_content(string bf)
    {
      if (bf == null)
        return;
      string bf1 = bf;
      this.textenc = (Encoding) null;
      bf = bf.ToLower();
      if (bf.Contains("iso-8859"))
        this.textenc = Encoding.GetEncoding("iso-8859-1");
      if (bf.Contains("utf-8"))
        this.textenc = Encoding.UTF8;
      this.isflowed = false;
      if (bf.Contains("format=flowed"))
        this.isflowed = true;
      if (this.isflowed)
        this.wasflowed = true;
      this.add_boundary(bf1);
      int length = bf.IndexOf(';');
      if (length > 0)
        bf = bf.Substring(0, length);
      this.content_type = bf.ToLower();
      if (bf.Length > 0)
      {
        this.intextpart = false;
        this.inhtmlpart = false;
      }
      if (this.content_type.Contains("text/plain"))
        this.intextpart = true;
      else if (this.content_type.Contains("text/html"))
        this.inhtmlpart = true;
    }

    private void decode_head_ended()
    {
      this.inhead = false;
      this.headers.TryGetValue("From", out this.from);
      this.headers.TryGetValue("To", out this.to);
      this.headers.TryGetValue("Message-ID", out this.mid);
      if (this.mid == null)
        this.headers.TryGetValue("Message-id", out this.mid);
      this.headers.TryGetValue("Subject", out this.subject);
      this.mid = this.mid == null ? "" : clib.email_only(this.mid);
      this.get_content_type();
    }

    private string get_param(string line, string word)
    {
      word += "=\"";
      int num = line.IndexOf(word);
      if (num <= 0)
        return "";
      string str = line.Substring(num + word.Length);
      int length = str.IndexOf("\"");
      if (length > 0)
        str = str.Substring(0, length);
      return str;
    }

    private void decode_part_header(string line)
    {
      if (line.Length == 0)
      {
        this.inmimeheader = false;
        if (this.content_type.IndexOf("rfc822", StringComparison.OrdinalIgnoreCase) <= 0)
          return;
        this.ismsgpart = true;
        this.ismsgheader = true;
      }
      else
      {
        if (line.Contains("Content-Disposition:") && line.Contains("attachment"))
        {
          this.intextpart = false;
          this.inhtmlpart = false;
        }
        if (line.Contains("Content-Disposition:") && line.Contains("form-data"))
        {
          string str1 = this.get_param(line, "name");
          string str2 = this.get_param(line, "filename");
          if (str1.Length > 0)
          {
            this.form_var = str1;
            this.form_val.Length = 0;
            this.inform = true;
            if (this.form_var.StartsWith("u_attach_"))
            {
              this.attach_fname = clib.tmp(string.Format("attached_{0}.dat", (object) Mime.na++));
              this.attach_stream = File.Open(this.attach_fname, FileMode.Create);
              this.attach_original = str2;
            }
          }
        }
        this.add_boundary(line);
        this.find_name(line);
        int length = line.IndexOf(':');
        if (line[0] == ' ' || line[0] == '\t')
          this.set_continue(line);
        if (length <= 0)
          return;
        string str = line.Substring(0, length);
        string bf = line.Substring(length + 2);
        if (str.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
          this.set_content(bf);
        if (str.Equals("Content-Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
          this.set_transfer(bf);
        if (!str.Equals("Content-ID", StringComparison.OrdinalIgnoreCase))
          return;
        this.set_cid(bf);
      }
    }

    private void decode_head(int linei, string line)
    {
      if (line.Length == 0)
      {
        this.decode_head_ended();
      }
      else
      {
        this.add_boundary(line);
        if (line[0] == ' ')
          this.add_last(clib.hdr_decode(line));
        else if (line[0] == '\t')
        {
          this.add_last(clib.hdr_decode(line));
        }
        else
        {
          int length = line.IndexOf(':');
          if (length <= 0)
            return;
          try
          {
            string a = line.Substring(0, length);
            string input = line.Substring(length + 2);
            this.lasthead = a;
            this.headers.Add(a, clib.hdr_decode(input));
          }
          catch
          {
          }
        }
      }
    }

    private bool is_boundary(string line, out bool isend)
    {
      isend = false;
      if (!line.StartsWith("--"))
        return false;
      foreach (string str in this.boundary)
      {
        if (line.Contains(str))
        {
          if (line.EndsWith("--") && line == "--" + str + "--")
            isend = true;
          return true;
        }
      }
      return false;
    }

    private string decode_charset(string line)
    {
      return clib.decode_text(line, this.textenc);
    }

    private string unencode_line(Mime.encoding enc, string line)
    {
      if (enc == Mime.encoding.base64)
        return clib.decode_base64(line, this.textenc);
      return enc == Mime.encoding.quoted ? clib.decode_quoted_printable(line, this.textenc) : line;
    }

    private void decode_body(int linei, string line)
    {
      bool isend;
      if (this.is_boundary(line, out isend))
      {
        this.attach_done();
        this.form_done();
        this.current_encoding = Mime.encoding.none;
        this.content_type = "";
        this.intextpart = false;
        this.inhtmlpart = false;
        this.inattachpart = false;
        this.ismsgheader = false;
        this.ismsgpart = false;
        this.part_subj = "";
        this.part_from = "";
        if (isend)
          return;
        this.inmimeheader = true;
      }
      else if (this.inmimeheader)
      {
        if (line.Length == 0)
        {
          if (this.content_type == "")
            this.decode_part_header("Content-Type: text/plain");
          if (!this.inattachpart && this.content_type.IndexOf("rfc822", StringComparison.OrdinalIgnoreCase) > 0)
            this.decode_part_header(string.Format(" name=test_{0}.eml", (object) this.msgi++));
        }
        this.decode_part_header(line);
      }
      else
      {
        if (this.gobble_indent && line.Length > 0)
        {
          int startIndex = 0;
          while (startIndex < line.Length && line[startIndex] == '>')
            ++startIndex;
          if (startIndex < line.Length)
          {
            if (line[startIndex] == ' ')
              ++startIndex;
          }
          try
          {
            line = line.Substring(startIndex);
          }
          catch
          {
          }
        }
        this.gobble_indent = false;
        if (this.current_encoding != Mime.encoding.none)
        {
          if (line.Length > 0)
            line = this.unencode_line(this.current_encoding, line);
        }
        else if (this.intextpart && this.isflowed)
        {
          int length = line.Length;
          if (length > 0)
          {
            if (line[length - 1] != ' ')
              line += "\r\n";
            else
              this.gobble_indent = true;
          }
          else
            line += "\r\n";
        }
        else
          line += "\r\n";
        if (this.inform)
        {
          if (this.attach_stream != null)
          {
            byte[] buffer = clib.string_to_byte(line);
            this.attach_stream.Write(buffer, 0, ((IEnumerable<byte>) buffer).Count<byte>());
          }
          else
            this.form_val.Append(line);
        }
        else if (this.intextpart)
          this.text.Append(line);
        else if (this.inhtmlpart)
          this.html.Append(line);
        if (this.ismsgheader)
        {
          if (line == "\r\n")
          {
            this.ismsgheader = false;
            if (this.ismsgpart && this.html.Length > 0)
              this.html.Append(string.Format("<hr><p><a href=\"{0}\">Click to Open Attached Email Message</a></p>", (object) clib.to_url(this.part_fname), (object) clib.web_encode(this.part_subj.Substring(0, clib.min(100, this.part_subj.Length))), (object) clib.web_encode(clib.nice_email(this.part_from))));
          }
          if (line.StartsWith("Subject: "))
            this.part_subj = line.Substring(9);
          if (line.StartsWith("From: "))
            this.part_from = line.Substring(6);
        }
        if (this.inattachpart)
        {
          byte[] buffer = clib.string_to_byte(line);
          if (this.fout != null)
            this.fout.Write(buffer, 0, ((IEnumerable<byte>) buffer).Count<byte>());
        }
      }
    }

    private enum encoding
    {
      none,
      base64,
      quoted,
    }
  }
}
