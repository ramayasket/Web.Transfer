// Decompiled with JetBrains decompiler
// Type: mycloud.Websvc
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using Mylib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using TcpLib;

namespace mycloud
{
  public class Websvc : TcpServiceProvider
  {
    private static WebModule[] modlist = new WebModule[3]
    {
      (WebModule) new WebForm(),
      (WebModule) new WebDav(),
      (WebModule) new WebFile()
    };
    private static int bfsz = 8000;
    private StringBuilder bodybuild = new StringBuilder();
    public string cookie = "";
    private int insz = Websvc.bfsz;
    private byte[] rawin = new byte[Websvc.bfsz];
    private int content_len = 0;
    public Dictionary<string, string> head = new Dictionary<string, string>();
    public Session ses;
    private bool isgzip;
    public bool top_done;
    public bool do_continue;
    private bool was_content;
    public Files files;
    public static int nrequests;
    public bool isssl;
    public bool data_ok;
    public string destination;
    public string ifheader;
    public bool ifheader_done;
    public bool isie;
    private string save_request;
    public string ifmodified;
    public bool overwrite;
    public bool iswebdav;
    public int h_timeout;
    private bool in_chunked;
    private static int nopen;
    private int chunk_len;
    public bool auth_got;
    public string auth_header;
    public string host;
    public string lockid;
    public bool isdav;
    public string method_name;
    private bool inmem;
    public MyBuffer mem_body;
    public Wmethod method;
    private bool inbody;
    private bool in_trailer;
    public bool need_length;
    public int depth;
    private WebModule wmod;
    private int inat;
    public bool out_chunked;
    public bool need_blank;
    public ConnectionState chan;
    public string content_type;
    public string url;
    public string httpver;
    public string url_raw;
    public int httpversion;
    public NameValueCollection query;
    public NameValueCollection form;

    public override int get_timeout()
    {
      return 30;
    }

    public static int get_nopen()
    {
      return Websvc.nopen;
    }

    public void imsg(string format, params object[] args)
    {
      clib.webmsg("{0}", (object) string.Format(format, args));
    }

    private void grow_rawin(int sz)
    {
      if (sz <= this.insz)
        return;
      int length = sz + Websvc.bfsz;
      byte[] numArray = new byte[length];
      Array.Copy((Array) this.rawin, (Array) numArray, this.insz);
      this.rawin = numArray;
      this.insz = length;
    }

    public bool write(string x)
    {
      if (Ini.istrue(En.debug_http))
        this.imsg("http: write: {0}", (object) x);
      return this.chan.write(x);
    }

    public bool body_write(string x)
    {
      this.bodybuild.Append(x);
      return true;
    }

    public bool body_send()
    {
      byte[] bytes = Encoding.UTF8.GetBytes(this.bodybuild.ToString());
      this.imsg("BODYSEND, LENGTH IS {0}", (object) ((IEnumerable<byte>) bytes).Count<byte>());
      clib.log_lines(this.bodybuild.ToString());
      if (this.out_chunked)
      {
        if (this.need_blank)
          this.write("\r\n");
        this.need_blank = false;
        this.write(string.Format("{0:x}\r\n", (object) ((IEnumerable<byte>) bytes).Count<byte>()));
        this.chan.Write(bytes, 0, ((IEnumerable<byte>) bytes).Count<byte>());
        this.write("\r\n0\r\n\r\n");
        this.bodybuild.Length = 0;
      }
      else if (this.need_length)
      {
        this.imsg("Sending CONTENT LEN {0}", (object) ((IEnumerable<byte>) bytes).Count<byte>());
        this.need_blank = false;
        this.write(string.Format("Content-Length: {0}\r\n\r\n", (object) ((IEnumerable<byte>) bytes).Count<byte>()));
        this.chan.Write(bytes, 0, ((IEnumerable<byte>) bytes).Count<byte>());
        this.bodybuild.Length = 0;
      }
      else
      {
        if (this.need_blank)
          this.write("\r\n");
        this.need_blank = false;
        this.imsg("body_send and not chunked or need length, data bytes length is {0}", (object) ((IEnumerable<byte>) bytes).Count<byte>());
      }
      return true;
    }

    public override object Clone()
    {
      return (object) new Websvc()
      {
        iswebdav = this.iswebdav
      };
    }

    public override void OnAcceptConnection(ConnectionState state)
    {
      ++Websvc.nopen;
      this.imsg("WEBSVC Accepting Websvc Connection from {0}", (object) state.RemoteEndPoint.ToString());
      if (state.ssl == null)
        return;
      this.isssl = true;
      this.imsg("WEBSVC knows it's an SSL connection\n");
    }

    public override void OnReceiveData(ConnectionState state)
    {
      this.chan = state;
      byte[] buffer = new byte[Websvc.bfsz];
      this.imsg("WEBSVC onreceivedata {0}", (object) state.AvailableData);
      if (!state.Connected)
      {
        this.OnDropConnection(state);
      }
      else
      {
        while (state.AvailableData > 0)
        {
          int length = state.Read(buffer, 0, Websvc.bfsz);
          if (length > 0)
          {
            this.grow_rawin(this.inat + length);
            Array.Copy((Array) buffer, 0, (Array) this.rawin, this.inat, length);
            this.inat += length;
            try
            {
              this.process_input();
            }
            catch (Exception ex)
            {
              this.imsg("Exception in OnReceiveData: {0} {1}", (object) ex.Message, (object) ex.ToString());
            }
          }
          else
          {
            state.EndConnection();
            break;
          }
        }
      }
    }

    private string in_line()
    {
      int num1 = clib.IndexOf(this.rawin, "\r\n", 0, this.inat);
      if (num1 < 0)
        return (string) null;
      this.imsg("Yay, we got a line");
      int num2 = num1 + 2;
      string str = Encoding.UTF8.GetString(this.rawin, 0, num2);
      Array.Copy((Array) this.rawin, num2, (Array) this.rawin, 0, this.inat - num2);
      this.inat -= num2;
      this.imsg("this is ht line {0}", (object) str);
      return str;
    }

    public static void CopyStream(Stream input, Stream output)
    {
      byte[] buffer = new byte[32768];
      while (true)
      {
        int count = input.Read(buffer, 0, buffer.Length);
        if (count > 0)
          output.Write(buffer, 0, count);
        else
          break;
      }
    }

    private static byte[] ungzip(byte[] gzip, int len)
    {
      using (GZipStream gzipStream = new GZipStream((Stream) new MemoryStream(gzip, 0, len), CompressionMode.Decompress))
      {
        byte[] buffer = new byte[4096];
        using (MemoryStream memoryStream = new MemoryStream())
        {
          int count;
          do
          {
            count = gzipStream.Read(buffer, 0, 4096);
            if (count > 0)
              memoryStream.Write(buffer, 0, count);
          }
          while (count > 0);
          return memoryStream.ToArray();
        }
      }
    }

    private void body_packet(byte[] bf, int len)
    {
      if (this.inmem)
        this.mem_body.add(bf, len);
      else
        this.wmod.do_body(this, bf, len);
    }

    private int read_block(int want)
    {
      if (want > this.inat)
        want = this.inat;
      if (want > 0)
        this.body_packet(this.rawin, want);
      if (want == this.inat)
      {
        this.inat = 0;
      }
      else
      {
        Array.Copy((Array) this.rawin, want, (Array) this.rawin, 0, this.inat - want);
        this.inat -= want;
      }
      return want;
    }

    private void process_body()
    {
      if (this.in_chunked)
      {
        if (this.chunk_len > 0)
        {
          int num = this.read_block(this.chunk_len);
          this.chunk_len -= num;
          this.imsg("chunked: Got block of {0} we have {1} left now", (object) num, (object) this.chunk_len);
          if (this.chunk_len > 0)
            return;
        }
        string stuff = this.in_line();
        if (stuff == null || stuff.Length == 2)
          return;
        this.chunk_len = clib.hex_to_int(stuff);
        this.imsg("chunked: read chunked packet length {0} {1} {2}", (object) this.chunk_len, (object) stuff, (object) stuff.Length);
        if (this.chunk_len != 0)
          return;
        this.imsg("chunked: end of chunked data");
        this.in_chunked = false;
        this.inbody = false;
        this.in_trailer = true;
        this.data_ok = true;
        clib.imsg("data_ok: chunk_len is zero {0} {1}", (object) this.chunk_len, (object) stuff);
        this.process_input_one();
      }
      else
      {
        this.imsg("body_content, still waiting fora {0} {1}", (object) this.content_len, (object) this.save_request);
        this.content_len -= this.read_block(this.content_len);
        this.imsg("body_content, still waiting forb {0}", (object) this.content_len);
        if (this.content_len > 0)
          return;
        if (this.was_content)
        {
          this.data_ok = true;
          clib.imsg("data_ok: content_len is zero {0}", (object) this.content_len);
        }
        this.body_end("process_body_contentzero");
      }
    }

    private void body_end(string whence)
    {
      this.imsg("websvc: body_end called {0}", (object) whence);
      if (this.isgzip)
      {
        byte[] bf = Websvc.ungzip(this.mem_body.inbf, this.mem_body.Length);
        this.mem_body = new MyBuffer();
        this.mem_body.add(bf, bf.Length);
      }
      ++Websvc.nrequests;
      try
      {
        this.wmod.do_body_end(this);
      }
      catch (Exception ex)
      {
        this.imsg("crash in do_body_end() {0} {1}", (object) ex.Message, (object) ex.ToString());
        Web.simple_error(this, "crash in do_body_end() " + ex.Message);
      }
      clib.imsg("body_end setting wmod to null");
      this.wmod = (WebModule) null;
      if (this.query != null)
        this.query.Clear();
      if (this.form != null)
        this.form.Clear();
      this.inbody = false;
      this.content_len = 0;
      this.in_chunked = false;
      this.imsg("mystery body_end called {0}", (object) this.content_len);
    }

    private void process_input()
    {
      int inat;
      do
      {
        inat = this.inat;
        this.process_input_one();
      }
      while (inat != this.inat);
    }

    private void process_input_one()
    {
      if (this.inbody)
        this.process_body();
      else if (this.in_trailer)
      {
        string str = this.in_line();
        if (str == null || str.Length != 2)
          return;
        this.in_trailer = false;
        this.body_end(nameof (process_input_one));
      }
      else
      {
        int num1 = clib.IndexOf(this.rawin, "\r\n\r\n", 0, this.inat);
        if (num1 < 0)
          return;
        this.imsg("Yay, we got the full header");
        int num2 = num1 + 4;
        string header = Encoding.UTF8.GetString(this.rawin, 0, num2);
        Array.Copy((Array) this.rawin, num2, (Array) this.rawin, 0, this.inat - num2);
        this.inat -= num2;
        try
        {
          this.process_header(header);
          this.process_cmd_early();
        }
        catch (Exception ex)
        {
          Web.simple_error(this, "Crashed in proces_header code : " + ex.Message + "\n" + ex.ToString());
        }
        this.imsg("And now call process_cmd_early");
      }
    }

    private string head_get(string header)
    {
      string str = "";
      this.head.TryGetValue(header.ToLower(), out str);
      if (str == null)
        str = "";
      return str;
    }

    private bool decode_request(string rline)
    {
      string[] strArray = clib.string_words(rline);
      this.method = Wmethod.NONE;
      if (((IEnumerable<string>) strArray).Count<string>() < 3)
      {
        this.chan.write("HTTP/1.1 500 BAD REQUEST, EXPECTING AT LEAST 3 PARAM\r\n\r\n");
        return false;
      }
      this.method_name = strArray[0].ToUpper();
      if (strArray[0].Equals("GET", StringComparison.OrdinalIgnoreCase))
        this.method = Wmethod.GET;
      else if (strArray[0].Equals("POST", StringComparison.OrdinalIgnoreCase))
        this.method = Wmethod.POST;
      else if (strArray[0].Equals("COPY", StringComparison.OrdinalIgnoreCase))
        this.method = Wmethod.COPY;
      else if (strArray[0].Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
        this.method = Wmethod.OPTIONS;
      else if (strArray[0].Equals("DELETE", StringComparison.OrdinalIgnoreCase))
        this.method = Wmethod.DELETE;
      else if (strArray[0].Equals("HEAD", StringComparison.OrdinalIgnoreCase))
        this.method = Wmethod.HEAD;
      else if (strArray[0].Equals("LOCK", StringComparison.OrdinalIgnoreCase))
        this.method = Wmethod.LOCK;
      else if (strArray[0].Equals("MKCOL", StringComparison.OrdinalIgnoreCase))
        this.method = Wmethod.MKCOL;
      else if (strArray[0].Equals("MOVE", StringComparison.OrdinalIgnoreCase))
        this.method = Wmethod.MOVE;
      else if (strArray[0].Equals("PROPFIND", StringComparison.OrdinalIgnoreCase))
        this.method = Wmethod.PROPFIND;
      else if (strArray[0].Equals("PROPPATCH", StringComparison.OrdinalIgnoreCase))
        this.method = Wmethod.PROPPATCH;
      else if (strArray[0].Equals("PUT", StringComparison.OrdinalIgnoreCase))
        this.method = Wmethod.PUT;
      else if (strArray[0].Equals("UNLOCK", StringComparison.OrdinalIgnoreCase))
        this.method = Wmethod.UNLOCK;
      this.imsg("websvc: url we get {0}", (object) strArray[1]);
      this.url = clib.percent_decode_utf(strArray[1]);
      this.url_raw = strArray[1];
      this.imsg("websvc: url we decoded {0}", (object) this.url);
      if (this.url.StartsWith("http://"))
      {
        int startIndex = this.url.IndexOf("/", 7);
        if (startIndex > 0)
        {
          this.url = this.url.Substring(startIndex);
          this.imsg("patched url to {0}", (object) this.url);
        }
      }
      this.httpver = strArray[2];
      int num = this.httpver.IndexOf(".");
      this.httpversion = 0;
      if (num > 0)
        this.httpversion = clib.atoi(this.httpver.Substring(num + 1));
      return true;
    }

    private void process_header(string header)
    {
      this.head.Clear();
      this.content_type = "";
      this.content_len = 0;
      this.imsg("content_mystery, process_header started {0}", (object) this.content_len);
      this.depth = 2;
      string[] strArray = clib.string_lines(header);
      string rline = strArray[0];
      this.save_request = rline;
      if (Ini.istrue(En.debug_http))
        this.imsg("http: ===< Request: {0}", (object) rline);
      foreach (string str1 in strArray)
      {
        int length = str1.IndexOf(":");
        if (length >= 0)
        {
          string str2 = str1.Substring(0, length);
          string str3 = str1.Substring(length + 1);
          if (str3.Length > 0 && str3.StartsWith(" "))
            str3 = str3.Substring(1);
          if (Ini.istrue(En.debug_http))
            this.imsg("http: {0}: {1}", (object) str2, (object) str3);
          try
          {
            this.head.Add(str2.ToLower(), str3);
          }
          catch
          {
          }
        }
      }
      this.isgzip = this.head_get("Content-Encoding").Contains("gzip");
      if (this.isgzip)
      {
        this.inmem = true;
        this.mem_body = new MyBuffer();
      }
      this.isie = this.head_get("User-Agent").Contains("MSIE");
      this.was_content = false;
      this.content_len = clib.atoi(this.head_get("Content-Length"));
      if (this.content_len > 0)
        this.was_content = true;
      this.imsg("do_headers: mystery content_len is {0} {1}", (object) this.content_len, (object) rline);
      this.in_chunked = false;
      string str4 = this.head_get("Timeout");
      if (str4 != null)
      {
        int num = str4.IndexOf("Second-");
        this.h_timeout = num < 0 ? 3600 : clib.atoi(str4.Substring(num + "Second-".Length));
      }
      string str5 = this.head_get("Transfer-Encoding");
      if (str5 != null && str5.ToLower().Contains("chunked"))
      {
        this.in_chunked = true;
        this.chunk_len = 0;
      }
      this.do_continue = false;
      if (this.head_get("Expect").Contains("100-continue"))
        this.do_continue = true;
      if (this.do_continue)
      {
        this.imsg("Sending: HTTP/1.1 100 Continue\n");
        this.chan.write("HTTP/1.1 100 Continue\r\n\r\n");
      }
      this.destination = this.head_get("Destination");
      this.destination = clib.url_decode(this.destination);
      this.imsg("decoded dest is now {0}", (object) this.destination);
      this.overwrite = true;
      string str6 = this.head_get("Overwrite");
      if (str6 != null)
      {
        if (str6.ToLower().Contains("t"))
          this.overwrite = true;
        if (str6.ToLower().Contains("f"))
          this.overwrite = false;
      }
      string lower = (this.head_get("Depth") ?? "2").Trim().ToLower();
      if (lower.Length == 0)
        this.depth = 2;
      else if (lower == "0")
        this.depth = 0;
      else if (lower == "1")
        this.depth = 1;
      else if (lower == "infinity")
        this.depth = 2;
      this.imsg("Depth is {0}", (object) this.depth);
      this.cookie = this.head_get("Cookie");
      this.imsg("Main request: {0} {1}", (object) this.content_len, (object) rline);
      if (!this.decode_request(rline))
      {
        this.imsg("decode_request failed - close link");
        this.imsg("decode_request failed - close link");
        this.chan.EndConnection();
      }
      else
        this.imsg("decode_request worked okk");
    }

    private void auth_decode(string hdr)
    {
      this.auth_got = true;
      this.auth_header = hdr;
    }

    private void process_cmd_early()
    {
      if (this.iswebdav)
      {
        this.wmod = (WebModule) new WebDav();
        this.imsg("Using webdav module for this one");
      }
      else
      {
        foreach (WebModule webModule in Websvc.modlist)
        {
          if (webModule.isforme(clib.pathstart(this.url), this.url))
            this.wmod = (WebModule) webModule.Clone();
        }
      }
      if (this.wmod == null)
      {
        this.imsg("FATAL: isforme... No module found for url {0} {1} iswebdav = {2}", (object) this.url, (object) clib.pathstart(this.url), (object) this.iswebdav);
        string x = "Sorry invalid url";
        this.chan.write(string.Format("HTTP/1.1 500 Invalid url for this server\r\nContent-Length: {0}\r\n\r\n", (object) x.Length));
        this.chan.write(x);
      }
      else
      {
        this.imsg("wmod: Found module {0}", (object) this.wmod.myname());
        this.query = clib.ParseQueryString(this.url_raw);
        foreach (string allKey in this.query.AllKeys)
          this.imsg("QUERY:   {0,-10} {1}", (object) allKey, (object) this.query[allKey]);
        this.auth_decode(this.head_get("Authorization"));
        this.ifmodified = this.head_get("If-Modified-Since");
        if (this.ifmodified == null)
          this.ifmodified = "";
        this.ifheader = this.head_get("If");
        this.ifheader_done = false;
        this.top_done = false;
        if (this.ifheader == null)
          this.ifheader = "";
        this.host = this.head_get("Host");
        if (this.host == null)
          this.host = "http://localhost";
        this.lockid = this.head_get("Lock-Token");
        if (this.lockid == null)
          this.lockid = "";
        this.lockid = this.lockid.Trim("<> ".ToCharArray());
        this.content_type = this.head_get("Content-Type");
        this.data_ok = false;
        this.wmod.do_headers(this);
        this.inbody = true;
        if (this.method == Wmethod.POST || this.method == Wmethod.PUT)
        {
          this.inmem = false;
        }
        else
        {
          this.inmem = true;
          this.mem_body = new MyBuffer();
        }
      }
    }

    public override void OnDropConnection(ConnectionState state)
    {
      --Websvc.nopen;
      this.imsg("ondropconnection - close stuff if needed. ");
      if (this.wmod != null)
      {
        this.imsg("ondropconnection: calling wmod dropconnection");
        this.wmod.dropconnection(this);
      }
      else
        this.imsg("ondropconnection: wmod was null");
      this.wmod = (WebModule) null;
    }
  }
}
