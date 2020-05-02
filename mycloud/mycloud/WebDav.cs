// Decompiled with JetBrains decompiler
// Type: mycloud.WebDav
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
using System.Xml;

namespace mycloud
{
  internal class WebDav : WebModule
  {
    private List<string> hdr_add = new List<string>();
    private string put_reason = "200 no message";
    private MemoryStream memoryStream;
    private Websvc w;
    private XmlNamespaceManager nsmgr;
    private Files files;
    private SimpleStream putstream;
    public static int nrequests;
    private string vpath;

    public override object Clone()
    {
      return (object) new WebDav();
    }

    public override bool isforme(string path, string url)
    {
      return path.StartsWith("/webdav/") || path == "/webdav" || url == "/webdav";
    }

    public override void data_in(byte[] inbf, int inlen)
    {
    }

    public void imsg(string format, params object[] args)
    {
      clib.wmsg("{0}", (object) string.Format(format, args));
    }

    public override string myname()
    {
      return nameof (WebDav);
    }

    private bool do_delete()
    {
      return this.do_copy(false, true, false);
    }

    private bool do_mkcol()
    {
      this.imsg("Make Collection vpath ({0})", (object) this.vpath);
      this.vpath = this.vpath.trimslash();
      string fname = clib.pathonly(this.vpath).trimslash();
      if (this.w.mem_body.Length > 0)
      {
        this.send_code("415 (Body found) - mkcol must not have a body");
        return true;
      }
      if (fname.Length > 0 && !this.files.exists(fname))
      {
        this.send_code(string.Format("409 (Conflict) - parent directory doesn't exist ({0})", (object) fname));
        return true;
      }
      if (this.files.exists(this.vpath))
      {
        this.send_code(string.Format("405 (Method not allowed) path already exists ({0})", (object) this.vpath));
        return true;
      }
      string reason;
      if (this.files.mkdir(this.vpath, out reason))
        this.send_code("201 (Created) - the collection was created");
      else
        this.send_code(reason);
      return true;
    }

    private bool do_unimp()
    {
      this.do_debug("Unimplemented");
      return true;
    }

    private bool do_debug(string response)
    {
      Web.any_header(this.w, "text/plain", "200 ok");
      Web.wp(this.w, "{0}", (object) response);
      this.w.body_send();
      return true;
    }

    private void add_header(string hdr)
    {
      this.hdr_add.Add(hdr);
    }

    private bool send_xml(string xml, string httpcode)
    {
      this.imsg("wd: xml response {0} ", (object) httpcode);
      Web.any_header(this.w, "text/xml", httpcode);
      Web.wh(this.w, "Date: {0}\r\n", (object) DateTime.Now.ToHttpDate());
      foreach (string str in this.hdr_add)
        Web.wh(this.w, "{0}\r\n", (object) str);
      this.hdr_add.Clear();
      this.imsg("xml: SEND_BODY {0}", (object) xml);
      Web.wp(this.w, "{0}", (object) xml);
      this.w.body_send();
      return true;
    }

    private bool send_code(string reason)
    {
      this.imsg("wd:--> {0}", (object) reason);
      if (clib.atoi(reason) == 0)
        throw new Exception("bad reason oops + reason");
      if (Ini.istrue(En.debug_http))
        this.imsg("webdav: response code {0}", (object) reason);
      Web.any_header(this.w, "text/plain", reason, 0, false);
      Web.wh(this.w, "Date: {0}\r\n", (object) DateTime.Now.ToHttpDate());
      this.w.body_send();
      return true;
    }

    private bool setup_files()
    {
      if (this.files != null)
        return true;
      string auth_user;
      string reason;
      if (!Vuser.check_basic_digest(this.w.method_name, this.w.auth_header, out auth_user, out reason))
      {
        this.imsg("LOGIN FAILED -DIGEST NOT HAPPY {0}", (object) reason);
        return false;
      }
      this.imsg("Login OK");
      this.files = new Files();
      User user = Vuser.lookup(auth_user);
      if (!this.files.set_profile(auth_user, user.groups(), user.quota()))
        return false;
      if (!this.files.exists("/"))
        this.files.mkdir("/", out reason);
      return true;
    }

    private string url_only(string p)
    {
      int startIndex = p.IndexOfAny("#?".ToCharArray());
      return startIndex >= 0 ? p.Substring(startIndex) : p;
    }

    private string url_to_path(string p)
    {
      if (p.StartsWith("https://"))
      {
        p = p.Substring(8);
        int startIndex = p.IndexOf("/");
        if (startIndex > 0)
          p = p.Substring(startIndex);
      }
      if (p.StartsWith("http://"))
      {
        p = p.Substring(7);
        int startIndex = p.IndexOf("/");
        if (startIndex > 0)
          p = p.Substring(startIndex);
      }
      p = p.trimslash();
      if (p.Length == 0)
        p = "/";
      if (this.w.iswebdav)
        return p;
      if (!p.StartsWith("/webdav"))
        return (string) null;
      return p.StartsWith("/webdav/") ? p.Substring(7) : "/";
    }

    public override bool do_headers(Websvc w)
    {
      ++WebDav.nrequests;
      this.w = w;
      if (!w.iswebdav && w.url == "/webdav")
        w.url = "/webdav/";
      this.vpath = this.url_to_path(w.url);
      w.isdav = true;
      this.imsg("wd:<--(header) {0} {1} {2}", (object) w.method, (object) this.vpath, (object) w.url);
      this.files = (Files) null;
      if (!w.auth_got)
      {
        this.imsg("NO AUTHENTICATED USER, FAILING");
        return false;
      }
      if (!this.setup_files())
      {
        this.imsg("authorization failed");
        w.auth_got = false;
        return false;
      }
      return w.method != Wmethod.PUT || this.do_put_start();
    }

    public override void dropconnection(Websvc w)
    {
      clib.imsg("webdav: dropconnection called");
      if (this.putstream == null || !this.putstream.isopen)
        return;
      clib.imsg("webdav: dropconnection called - deleting file we had open...");
      this.putstream.close();
      this.putstream.delete();
    }

    public override bool do_body_end(Websvc w)
    {
      this.w = w;
      this.imsg("webdav: do_body_end  called {0}  ", (object) w.method);
      if (w.method == Wmethod.OPTIONS)
        return this.do_options();
      if (!w.auth_got)
      {
        Web.need_auth(w);
        return false;
      }
      if (!this.setup_files())
      {
        Web.need_auth(w);
        return false;
      }
      if (!w.ifheader_done && w.ifheader.Length > 0 && !this.do_if(w.ifheader, false))
      {
        this.send_code("412 precondition failed do_body_end");
        return true;
      }
      this.imsg("wd:<--(body) {0} {1} {2}", (object) w.method, (object) this.vpath, (object) w.destination);
      if (w.method == Wmethod.PROPPATCH)
        return this.do_proppatch();
      if (w.method == Wmethod.PROPFIND)
        return this.do_propfind();
      if (w.method == Wmethod.LOCK)
        return this.do_lock();
      if (w.method == Wmethod.UNLOCK)
        return this.do_unlock();
      if (w.method == Wmethod.OPTIONS)
        return this.do_options();
      if (w.method == Wmethod.GET)
        return this.do_get(true);
      if (w.method == Wmethod.HEAD)
        return this.do_get(false);
      if (w.method == Wmethod.COPY)
        return this.do_copy(false, false, true);
      if (w.method == Wmethod.MOVE)
        return this.do_copy(true, false, false);
      if (w.method == Wmethod.DELETE)
        return this.do_delete();
      if (w.method == Wmethod.MKCOL)
        return this.do_mkcol();
      return w.method != Wmethod.PUT || this.do_put();
    }

    private XmlNode load_xml_request(out bool badxml)
    {
      XmlNode xmlNode = (XmlNode) null;
      badxml = false;
      this.imsg("mem_body length is {0}", (object) this.w.mem_body.Length);
      string str = clib.byte_to_string(this.w.mem_body.inbf, this.w.mem_body.Length);
      if (str != null)
        this.imsg("xml_request_string is {0}: \n{0}", (object) str.Length, (object) str);
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load((TextReader) new StringReader(str.Trim()));
        this.show(doc);
        xmlNode = (XmlNode) doc.DocumentElement;
        this.nsmgr = new XmlNamespaceManager(doc.NameTable);
        this.nsmgr.AddNamespace("D", "DAV:");
      }
      catch (Exception ex)
      {
        this.imsg("xml load failed {0}", (object) ex.Message);
        badxml = true;
        if (this.w.mem_body.Length <= 6)
          badxml = false;
      }
      return xmlNode;
    }

    private void response_add(List<XResponse> r, XmlNode n, string response)
    {
      this.imsg("Response_add: {0} {1}", (object) n.Name, (object) response);
      r.Add(new XResponse(n, response));
    }

    private void response_write(XmlWriter x, List<XResponse> all)
    {
      foreach (XResponse xresponse in all)
      {
        x.WriteStartElement("D", "propstat", "DAV:");
        x.WriteStartElement("D", "prop", "DAV:");
        x.WriteStartElement(xresponse.n.Prefix, xresponse.n.LocalName, xresponse.n.NamespaceURI);
        x.WriteEndElement();
        x.WriteEndElement();
        x.WriteElementString("D", "status", "DAV:", "HTTP/1.1 " + xresponse.reason);
        x.WriteEndElement();
      }
    }

    private string normal_name(XmlNode n)
    {
      return string.Format("{0}:{1}", (object) n.NamespaceURI, (object) n.LocalName);
    }

    private bool node_match(XmlNode n, string name)
    {
      this.imsg("Node match name={0} localname={1} ns={2} prefix={3} WANT {4}", (object) n.Name, (object) n.LocalName, (object) n.NamespaceURI, (object) n.Prefix, (object) name);
      return n.LocalName == name && n.NamespaceURI == "DAV:";
    }

    private bool do_proppatch()
    {
      this.imsg("do_proppatch starting");
      List<XResponse> xresponseList = new List<XResponse>();
      bool badxml;
      XmlNode xmlNode = this.load_xml_request(out badxml);
      if (badxml || xmlNode == null)
      {
        this.send_code("400 invalid xml");
        return false;
      }
      if (!this.do_if(this.w.ifheader, false))
      {
        this.send_code("423 locked");
        return false;
      }
      this.files.property_load(this.vpath);
      foreach (XmlNode childNode in xmlNode.SelectSingleNode("//D:propertyupdate", this.nsmgr).ChildNodes)
      {
        this.imsg("Node found name={0} localname={1} ns={2} prefix={3} ", (object) childNode.Name, (object) childNode.LocalName, (object) childNode.NamespaceURI, (object) childNode.Prefix);
        if (this.node_match(childNode, "set"))
        {
          XmlNode firstChild = childNode.FirstChild.FirstChild;
          this.imsg("setlist: {0} {1} NAME={2} VALUE={3}", (object) firstChild.LocalName, (object) firstChild.NamespaceURI, (object) this.normal_name(firstChild), (object) firstChild.InnerXml);
          this.files.property_set(this.normal_name(firstChild), firstChild.InnerXml);
          this.response_add(xresponseList, firstChild, "200 OK");
        }
        if (this.node_match(childNode, "remove"))
        {
          XmlNode firstChild = childNode.FirstChild.FirstChild;
          this.response_add(xresponseList, firstChild, "200 OK");
          this.imsg("removelist: NAME={0} ", (object) firstChild.Name);
          this.files.property_delete(this.normal_name(firstChild));
        }
      }
      this.files.property_save(this.vpath);
      XmlWriter x = this.rxml_start();
      x.WriteStartElement("D", "multistatus", "DAV:");
      x.WriteStartElement("D", "response", "DAV:");
      x.WriteElementString("D", "href", "DAV:", this.w.url);
      this.response_write(x, xresponseList);
      x.WriteEndElement();
      x.WriteEndElement();
      this.rxml_send(x, "207 Multistatus");
      return true;
    }

    private string nameonly(string x)
    {
      int length = x.IndexOf(">");
      if (length < 0)
        return x;
      x = x.Substring(0, length) + "/>";
      return x;
    }

    private string local_name(string x)
    {
      int num = x.LastIndexOf(':');
      return num < 0 ? x : x.Substring(num + 1);
    }

    private string uri_name(string x)
    {
      int length = x.LastIndexOf(':');
      return length < 0 ? x : x.Substring(0, length);
    }

    private void show_property(XmlWriter x, string pro)
    {
      this.imsg("show_property {0} {1}", (object) this.local_name(pro), (object) this.uri_name(pro));
      x.WriteStartElement(this.local_name(pro), this.uri_name(pro));
      x.WriteRaw(this.files.property_get(pro));
      x.WriteEndElement();
    }

    private void do_propfind_one(XmlNode rootxml, XmlWriter x, string url)
    {
      bool flag1 = false;
      bool flag2 = false;
      List<XmlNode> source = new List<XmlNode>();
      this.imsg("propfind_one: url {0}", (object) url);
      string path = this.url_to_path(url);
      this.files.property_load(path);
      this.imsg("propfind_one: path {0}", (object) path);
      x.WriteStartElement("D", "response", "DAV:");
      clib.imsg("Sending to writeelement as {0}", (object) clib.xml_space_encode(this.tofullpath(path)));
      x.WriteElementString("D", "href", "DAV:", clib.xml_space_encode(this.tofullpath(path)));
      x.WriteStartElement("D", "propstat", "DAV:");
      x.WriteStartElement("D", "prop", "DAV:");
      if (rootxml == null)
      {
        flag1 = true;
      }
      else
      {
        if (rootxml.SelectSingleNode("//D:propfind/D:allprop", this.nsmgr) != null)
          flag1 = true;
        if (rootxml.SelectSingleNode("//D:propfind/D:propname", this.nsmgr) != null)
          flag2 = true;
      }
      if (flag1 || flag2)
      {
        this.imsg("DOALL OR DONAMES");
        foreach (string property in (NameObjectCollectionBase) this.files.property_list())
        {
          if (flag2)
            x.WriteElementString(this.local_name(property), clib.url_encode(this.uri_name(property)), "");
          else
            this.show_property(x, property);
        }
      }
      else
      {
        foreach (XmlNode childNode in rootxml.SelectSingleNode("//D:propfind/D:prop", this.nsmgr).ChildNodes)
        {
          string str = this.files.property_get(this.normal_name(childNode));
          if (str == null || str.Length == 0)
          {
            source.Add(childNode);
          }
          else
          {
            this.imsg("propfind list: NAME={0} VALUE={1}", (object) this.normal_name(childNode), (object) childNode.InnerXml);
            x.WriteStartElement(childNode.Prefix, childNode.LocalName, childNode.NamespaceURI);
            x.WriteRaw(this.files.property_get(this.normal_name(childNode)));
            x.WriteEndElement();
          }
        }
      }
      x.WriteEndElement();
      x.WriteElementString("D", "status", "DAV:", "HTTP/1.1 200 OK");
      x.WriteEndElement();
      if (source.Count<XmlNode>() > 0)
      {
        x.WriteStartElement("D", "propstat", "DAV:");
        x.WriteStartElement("D", "prop", "DAV:");
        foreach (XmlNode xmlNode in source)
          x.WriteElementString(xmlNode.Prefix, xmlNode.LocalName, xmlNode.NamespaceURI, "");
        x.WriteEndElement();
        x.WriteElementString("D", "status", "DAV:", "HTTP/1.1 404 Not Found");
        x.WriteEndElement();
      }
      x.WriteEndElement();
    }

    private void propfind_deeper(XmlNode root, XmlWriter x, string url, string path, int depth)
    {
      this.do_propfind_one(root, x, url);
      this.imsg("+++++propfind_deeper called: url {0} path {1} depth {2}", (object) url, (object) path, (object) depth);
      switch (depth)
      {
        case 0:
          return;
        case 1:
          depth = 0;
          break;
      }
      if (!this.files.is_dir(path))
      {
        this.imsg("propfind_deeper: not a directory");
      }
      else
      {
        foreach (Fileinfo fileinfo in this.files.get_index(path))
        {
          this.imsg("propfind: deeper step 1 {0}", (object) fileinfo.name);
          string str1 = url.addslash() + clib.fileonly(fileinfo.name);
          string str2 = path.addslash() + clib.fileonly(fileinfo.name);
          this.imsg("propfind: deeper step 2 {0}", (object) str1);
          if (fileinfo.isdir)
          {
            str1 = clib.add_slash(str1);
            str2 = clib.add_slash(str2);
          }
          this.imsg("propfind: deeper {0} {1} {2}", (object) depth, (object) str1, (object) str2);
          if (fileinfo.isdir)
            this.propfind_deeper(root, x, str1, str2, depth);
          else
            this.do_propfind_one(root, x, str1);
        }
      }
    }

    private XmlWriter rxml_start()
    {
      this.memoryStream = new MemoryStream();
      XmlWriter xmlWriter = XmlWriter.Create((Stream) this.memoryStream, new XmlWriterSettings()
      {
        Encoding = (Encoding) new UTF8Encoding(false),
        Indent = true
      });
      xmlWriter.WriteStartDocument();
      return xmlWriter;
    }

    private void rxml_send(XmlWriter x, string httpcode)
    {
      x.WriteEndDocument();
      x.Flush();
      x.Close();
      this.send_xml(Encoding.UTF8.GetString(this.memoryStream.ToArray()), httpcode);
    }

    private void send_file(string fname)
    {
      byte[] buffer = new byte[200];
      string str1 = "respond.body";
      StreamReader streamReader = new StreamReader(fname);
      string str2;
      while ((str2 = streamReader.ReadLine()) != null)
        this.w.write(str2 + "\r\n");
      streamReader.Close();
      this.w.write(string.Format("Content-Length: {0}\r\n\r\n", (object) new FileInfo(str1).Length));
      FileStream fileStream = new FileStream(str1, FileMode.Open, FileAccess.Read);
      while (true)
      {
        int count = fileStream.Read(buffer, 0, 100);
        if (count > 0)
          this.w.chan.Write(buffer, 0, count);
        else
          break;
      }
      fileStream.Close();
    }

    private bool do_propfind()
    {
      this.imsg("DO_PROPFIND STARTS");
      bool badxml;
      XmlNode root = this.load_xml_request(out badxml);
      if (!this.files.property_load(this.vpath))
      {
        this.send_code("404 does not exist");
        return false;
      }
      if (badxml)
      {
        this.send_code("400 invalid xml");
        return false;
      }
      XmlWriter x = this.rxml_start();
      x.WriteStartElement("D", "multistatus", "DAV:");
      this.propfind_deeper(root, x, this.w.url, this.vpath, this.w.depth);
      x.WriteEndElement();
      this.rxml_send(x, "207 Multistatus");
      return true;
    }

    private bool do_put_start()
    {
      this.imsg("PUT START CREATING FILE {0}", (object) this.vpath);
      this.put_reason = "201 no content put worked";
      if (!this.do_if(this.w.ifheader, false))
      {
        this.put_reason = "423 locked";
        return true;
      }
      this.w.ifheader_done = true;
      if (this.files.exists(this.vpath))
      {
        this.imsg("put: path already exists {0}", (object) this.vpath);
        if (this.w.overwrite)
        {
          this.files.delete(this.vpath, out string _);
          this.put_reason = "204 put worked file overwritten";
        }
        else
        {
          this.put_reason = "412 destination exists";
          return true;
        }
      }
      else
        this.put_reason = "201 Created new object successfully";
      string reason;
      this.putstream = this.files.open(this.vpath, false, false, out reason);
      if (this.putstream == null)
        this.put_reason = "409 " + reason;
      this.imsg("put_reason is {0} pustreambad?{1}", (object) this.put_reason, (object) (this.putstream == null));
      return true;
    }

    public override bool do_body(Websvc w, byte[] inbf, int inlen)
    {
      this.w = w;
      if (this.putstream != null)
        this.putstream.write(inbf, 0, inlen);
      return true;
    }

    private bool do_put()
    {
      if (this.putstream == null)
      {
        this.imsg("webdav: do_put() called,  stream is null {0}", (object) this.put_reason);
        this.send_code(this.put_reason);
      }
      else
      {
        this.imsg("webdav: do_put() called,  file closed {0}", (object) this.put_reason);
        this.putstream.close();
        if (!this.w.data_ok)
        {
          this.put_reason = "500 incomplete data received based on content length or chunks";
          this.imsg("webdav: Incomplete data, deleting file");
          clib.imsg("webdav: Incomplete data, deleting file");
          this.putstream.delete();
        }
        this.send_code(this.put_reason);
      }
      return true;
    }

    private void send_multi(string path, string reason)
    {
      XmlWriter x = this.rxml_start();
      x.WriteStartElement("D", "multistatus", "DAV:");
      x.WriteStartElement("D", "response", "DAV:");
      x.WriteElementString("D", "href", "DAV:", this.tofullpath(path));
      x.WriteElementString("D", "status", "DAV:", "HTTP/1.1 " + reason);
      x.WriteEndElement();
      x.WriteEndElement();
      string httpcode = "207 Multi status";
      this.rxml_send(x, httpcode);
    }

    private bool do_copy(bool do_rename, bool delete_method, bool copy_method)
    {
      string reason1 = "201 no content - copy move or delete worked";
      string path1 = this.vpath;
      string str1 = "COPY";
      bool flag1 = true;
      bool flag2 = false;
      string fname = "no_destination";
      List<IfItem> ifdata = new List<IfItem>();
      bool deeper = this.files.is_dir(this.vpath);
      Index tree = this.files.get_tree(this.vpath, this.w.depth == 2);
      if (delete_method)
        str1 = "DELETE";
      if (do_rename)
        str1 = "MOVE";
      this.imsg("wd: {0} {1} --> {2} depth={3}", (object) str1, (object) this.vpath, (object) this.w.destination, (object) this.w.depth);
      this.imsg("List of matching files {0} iscol {1} path {2}", (object) tree.count(), (object) deeper, (object) this.vpath);
      if (tree.count() == 0)
      {
        this.imsg("do_copy/rename/delete: no source files found {0}", (object) this.vpath);
        this.send_code("404 source file doesn't exist");
        return false;
      }
      if (!delete_method)
      {
        fname = this.url_to_path(this.w.destination);
        if (!this.files.exists(fname))
          reason1 = "201 copy to new resource worked";
      }
      this.ifdata_build(ifdata, this.w.ifheader, "");
      if (this.w.destination != null)
        this.imsg("DO_COPY: TREE OF {0} FILES to {1}", (object) tree.count(), (object) this.w.destination);
      foreach (Fileinfo fileinfo in tree)
      {
        List<string> paths = new List<string>();
        List<string> stringList = new List<string>();
        string str2 = "";
        if (this.w.destination != null)
        {
          this.imsg("url_to_dest {0}", (object) this.url_to_path(this.w.destination));
          str2 = this.apply_root(this.url_to_path(this.w.destination), fileinfo.name, this.vpath);
        }
        stringList.Clear();
        this.imsg("DO_COPY: FILE {0} dest {1}", (object) fileinfo.name, (object) str2);
        paths.Clear();
        if (delete_method || do_rename)
          paths.Add(fileinfo.name);
        if (do_rename || copy_method)
          paths.Add(str2);
        if (!this.ifdata_run(ifdata, paths, stringList))
        {
          flag1 = false;
          path1 = fileinfo.name;
          reason1 = "424 failed dependency";
          goto label_70;
        }
        else
        {
          this.imsg("ifdata_run returned TRUE, locklist length is {0}", (object) stringList.Count<string>());
          foreach (string path2 in paths)
          {
            this.imsg("lock: Check locks ok for {0}", (object) path2);
            string reason2;
            if (!this.files.lock_ok(path2, stringList, false, out reason2))
            {
              this.imsg("lock: Already locked {0} {1}", (object) path2, (object) reason2);
              foreach (string str3 in stringList)
                this.imsg("lock: locklist entry {0}", (object) str3);
              flag1 = false;
              path1 = fileinfo.name;
              reason1 = "423 locked already " + path2;
              goto label_70;
            }
          }
        }
      }
      if (deeper && do_rename)
      {
        Fileinfo info = this.files.get_info(this.vpath);
        flag1 = false;
        if (info != null)
          flag1 = this.do_rename_dir(info, this.vpath, out reason1);
      }
      else
      {
        if (this.w.overwrite && !delete_method)
        {
          this.imsg("DELETING the destination {0} iscol = {1}", (object) fname, (object) deeper);
          if (this.files.exists(fname))
          {
            flag2 = true;
            this.imsg("DEst does exist {0}", (object) fname);
            if (this.files.is_dir(fname))
            {
              this.imsg("dest is a directory ");
              string reason2;
              if (!this.files.delete_dir(fname, out reason2))
                this.imsg("delete_dir_Dest failed {0}", (object) reason2);
            }
            else
            {
              this.imsg("dest is a file ");
              string reason2;
              if (!this.files.delete(fname, out reason2))
                this.imsg("delete_dest failed {0}", (object) reason2);
            }
            this.imsg("Deleted destination due to 'overwrite' flag {0} does it still exist? {1}", (object) fname, (object) this.files.exists(fname));
          }
          else
            this.imsg("Did not delete destination as it doesn't currently exist {0}", (object) fname);
        }
        foreach (Fileinfo f in tree)
        {
          if (delete_method)
          {
            this.imsg("delete_method {0} iscol {1}", (object) this.vpath, (object) deeper);
            if (deeper)
            {
              flag1 = this.files.delete_dir(this.vpath, out reason1);
              if (flag1)
              {
                this.files.unlock_all(this.vpath, deeper);
                break;
              }
              break;
            }
            if (!this.files.exists(this.vpath))
            {
              reason1 = string.Format("404 cannot delete as file doesn't exist ({0})", (object) this.vpath);
              flag1 = false;
              break;
            }
            if (!this.do_delete_one(f, this.vpath, out reason1))
            {
              path1 = f.name;
              flag1 = false;
              break;
            }
            this.files.unlock_all(this.vpath, false);
          }
          else
          {
            this.imsg("do_copy: file {0} {1} ", (object) f.name, (object) fname);
            if (!this.do_copy_one(f, this.vpath, do_rename, out reason1, false))
            {
              path1 = f.name;
              flag1 = false;
              break;
            }
          }
        }
      }
label_70:
      this.imsg("finished method, reason is {0}", (object) reason1);
      if (flag1)
      {
        if (flag2)
          reason1 = "204 copy onto existing resource worked";
        this.send_code(reason1);
      }
      else if (deeper)
      {
        this.imsg("Sending response to collection {0}", (object) reason1);
        this.send_multi(path1, reason1);
      }
      else
      {
        this.imsg("Sending response for non collection {0}", (object) reason1);
        this.send_code(reason1);
      }
      return true;
    }

    private string apply_root(string dest, string name, string root)
    {
      if (name == root || dest == null)
        return dest;
      if (!dest.EndsWith("/"))
        dest += "/";
      string str = dest + name.Substring(root.Length);
      this.imsg("fix_dest {0} {1} {2} res={3}", (object) dest, (object) name, (object) root, (object) str);
      return str;
    }

    private bool do_copy_one(
      Fileinfo f,
      string root,
      bool do_rename,
      out string reason,
      bool docreate)
    {
      string str = this.apply_root(this.url_to_path(this.w.destination), f.name, root);
      this.imsg("do_copy_one fname={0} dest={1} w.destination:{2}", (object) f.name, (object) str, (object) this.w.destination);
      reason = "204 no content";
      if (this.w.overwrite && this.files.exists(str) && !this.files.delete(str, out reason))
        return true;
      if (do_rename)
      {
        if (!this.files.rename(f.name, str, out reason))
          return false;
      }
      else
      {
        if (f.isdir)
          return this.files.mkdir(str, out reason);
        if (!this.files.copy(f.name, str, out reason, docreate))
          return false;
      }
      return true;
    }

    private bool do_delete_one(Fileinfo f, string root, out string reason)
    {
      this.imsg("do_delete_one {0}", (object) f.name);
      return this.files.delete(root, out reason);
    }

    private bool do_rename_dir(Fileinfo f, string root, out string reason)
    {
      string str = this.apply_root(this.url_to_path(this.w.destination), f.name, root);
      this.imsg("do_rename_Dir {0} {1}", (object) f.name, (object) str);
      reason = "201 no content";
      if (this.w.overwrite && this.files.exists(str) && !this.files.delete_dir_or_file(str, out reason))
        return false;
      return this.files.rename_dir(f.name, str, out reason);
    }

    private void write_thing(XmlWriter x, string thing, string value)
    {
      x.WriteStartElement("D", thing, "DAV:");
      x.WriteStartElement("D", value, "DAV:");
      x.WriteEndElement();
      x.WriteEndElement();
    }

    private string tofullurl(string x)
    {
      string str = "http://";
      if (this.w.isssl)
        str = "https://";
      return this.w.iswebdav ? str + this.w.host + x : str + this.w.host + "/webdav" + x;
    }

    private string tofullpath(string x)
    {
      return this.tofullurl(x);
    }

    private string if_get_lock(string x)
    {
      int num = x.IndexOf('<');
      if (num < 0)
        return "";
      x = x.Substring(num + 1);
      int length = x.IndexOf('>');
      if (length > 0)
        x = x.Substring(0, length);
      return x;
    }

    private bool do_lock()
    {
      string httpcode = "200 ok";
      bool badxml;
      XmlNode xmlNode1 = this.load_xml_request(out badxml);
      bool flag = false;
      bool exclusive = false;
      Lock @lock = (Lock) null;
      List<Lock> heldlist = new List<Lock>();
      List<Fail> faillist = new List<Fail>();
      this.imsg("do_lock starting, timeout is {0} \n", (object) this.w.h_timeout);
      if (badxml)
      {
        this.send_code("400 invalid xml");
        return false;
      }
      if (!this.do_if(this.w.ifheader, true))
      {
        this.send_code("423 do_if failed in lock method");
        return false;
      }
      if (xmlNode1 == null)
      {
        this.imsg("LOCK refresh {0}", (object) this.w.ifheader);
        string opaque = this.if_get_lock(this.w.ifheader);
        this.imsg("Lock {0}", (object) opaque);
        @lock = Lock.find_lock(opaque);
        if (@lock == null)
          this.send_code("400 refresh failed no such lock found");
      }
      if (@lock == null)
      {
        try
        {
          XmlNode xmlNode2 = xmlNode1.SelectSingleNode("//D:lockinfo/D:lockscope", this.nsmgr);
          this.imsg("REQUEST WANTED LOCKSCOPE {0}", (object) xmlNode2.FirstChild.LocalName.ToLower());
          if (xmlNode2.FirstChild.LocalName.ToLower() == "exclusive")
            exclusive = true;
          if (xmlNode1.SelectSingleNode("//D:lockinfo/D:locktype", this.nsmgr).FirstChild.LocalName.ToLower() == "write")
            flag = true;
        }
        catch
        {
        }
        string innerXml = xmlNode1.SelectSingleNode("//D:lockinfo/D:owner", this.nsmgr).InnerXml;
        if (exclusive && !this.do_if(this.w.ifheader, false))
        {
          this.send_code("423 do_if failed in lock method - we want exclusive lock");
          return false;
        }
        @lock = this.files.get_lock(this.vpath, innerXml, this.w.depth == 2, exclusive, heldlist, faillist, this.w.h_timeout);
      }
      XmlWriter x = this.rxml_start();
      if (@lock != null)
      {
        x.WriteStartElement("D", "prop", "DAV:");
        if (!this.files.exists(this.vpath))
          httpcode = "201 Null resource locked";
        this.files.property_load(this.vpath);
        this.show_property(x, "DAV::lockdiscovery");
        x.WriteEndElement();
      }
      else
      {
        this.imsg("WE could not get the lock");
        x.WriteStartElement("D", "multistatus", "DAV:");
        foreach (Fail fail in faillist)
        {
          if (!(fail.url == this.vpath))
          {
            x.WriteStartElement("D", "response", "DAV:");
            x.WriteElementString("D", "href", "DAV:", this.tofullurl(fail.url));
            x.WriteElementString("D", "status", "DAV:", "HTTP/1.1 " + fail.reason);
            x.WriteEndElement();
          }
        }
        x.WriteStartElement("D", "response", "DAV:");
        x.WriteElementString("D", "href", "DAV:", this.tofullpath(this.vpath));
        x.WriteStartElement("D", "propstat", "DAV:");
        x.WriteStartElement("D", "prop", "DAV:");
        this.files.property_load(this.vpath);
        this.show_property(x, "DAV::lockdiscovery");
        x.WriteEndElement();
        x.WriteEndElement();
        if (!flag)
          x.WriteElementString("D", "status", "DAV:", "HTTP/1.1 412 not a supported lock");
        else
          x.WriteElementString("D", "status", "DAV:", "HTTP/1.1 412 Conflict");
        x.WriteEndElement();
        x.WriteEndElement();
        httpcode = "207 Multi status";
      }
      if (@lock != null)
        this.add_header(string.Format("Lock-Token: <{0}>", (object) @lock.opaque));
      this.rxml_send(x, httpcode);
      return true;
    }

    private bool do_if(string header, bool shared_ok)
    {
      List<string> locklist = new List<string>();
      this.imsg("lock: do_if called header={0}", (object) header);
      if (header.Length == 0)
      {
        this.imsg("Check if {0} is locked", (object) this.vpath);
        string reason;
        if (this.files.lock_ok(this.vpath, (List<string>) null, shared_ok, out reason))
          return true;
        this.imsg("do_if file is locked and if header is blank, so failing {0}", (object) reason);
        return false;
      }
      List<string> paths = new List<string>();
      paths.Add(this.vpath);
      if (this.w.destination != null && this.w.destination.Length > 0)
        paths.Add(this.url_to_path(this.w.destination));
      List<IfItem> ifdata = new List<IfItem>();
      this.imsg("do_if: ifdata_build");
      this.ifdata_build(ifdata, header, "");
      this.imsg("do_if: ifdata_run");
      bool flag = this.ifdata_run(ifdata, paths, locklist);
      this.imsg("do_if: RETURNED {0}", (object) flag);
      return flag;
    }

    private bool ifdata_run(List<IfItem> ifdata, List<string> paths, List<string> locklist)
    {
      try
      {
        foreach (IfItem ifItem in ifdata)
        {
          this.imsg("lock: ifdata_run: path=({0}) ", (object) ifItem.path);
          bool flag = false;
          if (paths.Contains(ifItem.path) || ifItem.path == "")
          {
            string path = ifItem.path;
            if (ifItem.path == "")
              path = paths[0];
            this.imsg("ifdata_run: using path {0}", (object) path);
            foreach (Ething etag in ifItem.etags)
            {
              this.imsg("ifdata_run: ething {0} fileetag={1} {2} {3} {4}", (object) etag.etag, (object) this.files.get_etag(path), etag.isetag ? (object) "isetag" : (object) "islocktoken", (object) etag.isnot, (object) path);
              if (etag.isetag)
              {
                flag = this.files.get_etag(path) == etag.etag;
              }
              else
              {
                flag = this.files.is_locked(path, etag.etag);
                if (flag)
                {
                  this.imsg("ifdata: lock: if_run add locklist adding {0} {1}", (object) path, (object) etag.etag);
                  locklist.Add(etag.etag);
                }
                else
                  this.imsg("ifdata: not locked {0} {1}", (object) path, (object) etag.etag);
              }
              if (etag.isnot)
                flag = !flag;
              this.imsg("ifdata: r={0} isnot{1}", (object) flag, (object) etag.isnot);
              if (!flag)
              {
                this.imsg("ifdata: stop checking as we got a FALSE");
                break;
              }
            }
            if (!flag)
            {
              this.imsg("ifdata: something failed so returning false");
              return false;
            }
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        clib.imsg("Crash in if_run {0} {1}", (object) ex.Message, (object) ex.ToString());
        return false;
      }
    }

    private void ifdata_build(List<IfItem> ifdata, string header, string defaultpath)
    {
      while (true)
      {
        string word1 = defaultpath;
        header = header.Trim();
        if (header.Length != 0)
        {
          this.imsg("ifdata_build: adding {0} {1}", (object) header, (object) defaultpath);
          if (header[0] == '<')
          {
            header = header.get_word('>', out word1);
            this.imsg("ifdata_build: got URL ({0}) ({1})", (object) word1, (object) header);
            word1 = this.url_to_path(word1.Trim("<> ".ToCharArray()));
          }
          header = header.Trim();
          if (header.Length != 0)
          {
            if (header[0] == '(')
            {
              string word2;
              header = header.get_word(')', out word2);
              this.imsg("ifdata_build: got word list [{0}] [{1}]", (object) word2, (object) header);
              word2 = word2.Trim("() ".ToCharArray());
              string[] strArray = word2.Split(" ".ToCharArray());
              List<Ething> etags = new List<Ething>();
              bool isnot = false;
              foreach (string etag in strArray)
              {
                this.imsg("ifdata_build: adding words {0}", (object) etag);
                if (etag.Length != 0)
                {
                  if (etag.ToLower() == "not")
                  {
                    isnot = true;
                  }
                  else
                  {
                    if (etag[0] == '[')
                      etags.Add(new Ething(etag.Trim("[]".ToCharArray()), true, isnot));
                    else
                      etags.Add(new Ething(etag, false, isnot));
                    isnot = false;
                  }
                }
              }
              ifdata.Add(new IfItem(word1, etags));
            }
            else
              goto label_7;
          }
          else
            goto label_1;
        }
        else
          break;
      }
      return;
label_1:
      return;
label_7:
      this.imsg("ifdata_build: break at bad syntax {0}", (object) header);
    }

    private bool do_unlock()
    {
      if (this.files.free_lock(this.w.lockid))
        this.send_code("204 no content - unlock worked");
      else
        this.send_code("500 Unlock failed, no such lock");
      return true;
    }

    private bool do_get(bool andbody)
    {
      if (this.files.is_dir(this.vpath))
      {
        Web.any_header(this.w, "text/plain", "200 ok");
        Web.wp(this.w, "WEBDAV get request, we could give a directory listing of {0}", (object) this.vpath);
        this.w.body_send();
        return true;
      }
      string reason;
      SimpleStream ss = this.files.open(this.vpath, true, false, out reason);
      if (ss == null)
      {
        this.send_code("404 " + reason);
        return true;
      }
      WebFile.send_file(this.w, ss, this.vpath, false, andbody);
      ss.close();
      return true;
    }

    private bool do_options()
    {
      this.send_code("200 OK");
      return true;
    }

    private void show_children(int depth, XmlNodeList nodes)
    {
      this.imsg("Show children {0}", (object) depth);
      foreach (XmlNode node in nodes)
      {
        this.imsg("[{0}]ChildNode {1} Name {2} prefix {3} uri {4} Value {5}", (object) depth, (object) node.ToString(), (object) node.Name, (object) node.Prefix, (object) node.NamespaceURI, (object) node.Value);
        if (node.HasChildNodes)
          this.show_children(depth++, node.ChildNodes);
      }
    }

    private void show(XmlDocument doc)
    {
      foreach (XmlNode childNode in doc.ChildNodes)
      {
        this.imsg("Node {0} {1} {2} {3} ", (object) childNode.ToString(), (object) childNode.Name, (object) childNode.Prefix, (object) childNode.NamespaceURI);
        if (childNode.HasChildNodes)
          this.show_children(1, childNode.ChildNodes);
      }
    }
  }
}
