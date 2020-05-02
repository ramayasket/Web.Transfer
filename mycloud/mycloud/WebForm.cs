// Decompiled with JetBrains decompiler
// Type: mycloud.WebForm
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using Mylib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace mycloud
{
  internal class WebForm : WebModule
  {
    private List<WebForm.CMDS> cmds = new List<WebForm.CMDS>()
    {
      new WebForm.CMDS("cmd_status", new WebForm.del_cmd(WebForm.web_cmd_status), WebForm.wflag.W_ADMIN),
      new WebForm.CMDS("cmd_wizard_save", new WebForm.del_cmd(WebForm.web_cmd_wizard_save), WebForm.wflag.W_LOGIN),
      new WebForm.CMDS("cmd_login_do", new WebForm.del_cmd(WebForm.web_cmd_login_do), WebForm.wflag.W_LOGIN),
      new WebForm.CMDS("cmd_reset_password", new WebForm.del_cmd(WebForm.web_cmd_reset_password), WebForm.wflag.W_LOGIN),
      new WebForm.CMDS("cmd_login", new WebForm.del_cmd(WebForm.web_cmd_login), WebForm.wflag.W_LOGIN),
      new WebForm.CMDS("cmd_signup_do", new WebForm.del_cmd(WebForm.web_cmd_signup_do), WebForm.wflag.W_LOGIN),
      new WebForm.CMDS("activate", new WebForm.del_cmd(WebForm.web_cmd_activate), WebForm.wflag.W_LOGIN),
      new WebForm.CMDS("cmd_log", new WebForm.del_cmd(WebForm.web_cmd_log), WebForm.wflag.W_ADMIN),
      new WebForm.CMDS("cmd_exit", new WebForm.del_cmd(WebForm.web_cmd_exit), WebForm.wflag.W_ADMIN),
      new WebForm.CMDS("cmd_upload", new WebForm.del_cmd(WebForm.web_cmd_upload), WebForm.wflag.W_NORM),
      new WebForm.CMDS("cmd_upload_do", new WebForm.del_cmd(WebForm.web_cmd_upload_do), WebForm.wflag.W_NORM),
      new WebForm.CMDS("cmd_browse", new WebForm.del_cmd(WebForm.web_cmd_browse), WebForm.wflag.W_NORM),
      new WebForm.CMDS("cmd_del_sel", new WebForm.del_cmd(WebForm.web_cmd_del_sel), WebForm.wflag.W_NORM),
      new WebForm.CMDS("cmd_move_sel", new WebForm.del_cmd(WebForm.web_cmd_move_sel), WebForm.wflag.W_NORM),
      new WebForm.CMDS("cmd_copy_sel", new WebForm.del_cmd(WebForm.web_cmd_copy_sel), WebForm.wflag.W_NORM),
      new WebForm.CMDS("cmd_mkdir", new WebForm.del_cmd(WebForm.web_cmd_mkdir), WebForm.wflag.W_NORM),
      new WebForm.CMDS("cmd_set_public_true", new WebForm.del_cmd(WebForm.web_cmd_set_public_true), WebForm.wflag.W_NORM),
      new WebForm.CMDS("cmd_set_upload_true", new WebForm.del_cmd(WebForm.web_cmd_set_upload_true), WebForm.wflag.W_NORM),
      new WebForm.CMDS("cmd_set_public_false", new WebForm.del_cmd(WebForm.web_cmd_set_public_false), WebForm.wflag.W_NORM),
      new WebForm.CMDS("cmd_set_upload_false", new WebForm.del_cmd(WebForm.web_cmd_set_upload_false), WebForm.wflag.W_NORM),
      new WebForm.CMDS("cmd_quota_repair", new WebForm.del_cmd(WebForm.web_cmd_quota_repair), WebForm.wflag.W_NORM),
      new WebForm.CMDS("cmd_options", new WebForm.del_cmd(WebForm.web_cmd_options), WebForm.wflag.W_NORM),
      new WebForm.CMDS("cmd_options_save", new WebForm.del_cmd(WebForm.web_cmd_options_save), WebForm.wflag.W_NORM),
      new WebForm.CMDS("cmd_profile_list", new WebForm.del_cmd(WebForm.web_cmd_profile_list), WebForm.wflag.W_ADMIN),
      new WebForm.CMDS("cmd_profile", new WebForm.del_cmd(WebForm.web_cmd_profile), WebForm.wflag.W_ADMIN),
      new WebForm.CMDS("cmd_profile_save", new WebForm.del_cmd(WebForm.web_cmd_profile_save), WebForm.wflag.W_ADMIN),
      new WebForm.CMDS("cmd_users", new WebForm.del_cmd(WebForm.web_cmd_users), WebForm.wflag.W_ADMIN),
      new WebForm.CMDS("cmd_users_save", new WebForm.del_cmd(WebForm.web_cmd_users_save), WebForm.wflag.W_ADMIN),
      new WebForm.CMDS("cmd_users_delete", new WebForm.del_cmd(WebForm.web_cmd_users_delete), WebForm.wflag.W_ADMIN),
      new WebForm.CMDS("cmd_users_search", new WebForm.del_cmd(WebForm.web_cmd_users_search), WebForm.wflag.W_ADMIN),
      new WebForm.CMDS("cmd_users_lookup", new WebForm.del_cmd(WebForm.web_cmd_users_lookup), WebForm.wflag.W_ADMIN),
      new WebForm.CMDS("cmd_config", new WebForm.del_cmd(WebForm.web_cmd_config), WebForm.wflag.W_ADMIN),
      new WebForm.CMDS("cmd_config_save", new WebForm.del_cmd(WebForm.web_cmd_config_save), WebForm.wflag.W_ADMIN),
      new WebForm.CMDS("cmd_register", new WebForm.del_cmd(WebForm.web_cmd_register), WebForm.wflag.W_ADMIN),
      new WebForm.CMDS("cmd_register_save", new WebForm.del_cmd(WebForm.web_cmd_register_save), WebForm.wflag.W_ADMIN),
      new WebForm.CMDS("cmd_register_activate", new WebForm.del_cmd(WebForm.web_cmd_register_activate), WebForm.wflag.W_ADMIN)
    };
    private Mime mime;

    public static void imsg(string format, params object[] args)
    {
      clib.webmsg("{0}", (object) string.Format(format, args));
    }

    public override object Clone()
    {
      return (object) new WebForm();
    }

    public override bool isforme(string path, string url)
    {
      return path.StartsWith("/cgi/") || path.StartsWith("/cgi") || (path.StartsWith("/public") || path.StartsWith("/download")) || (path.StartsWith("/upload") || path == "");
    }

    public override void data_in(byte[] inbf, int inlen)
    {
      WebForm.imsg("Got {0} bytes", (object) inlen);
    }

    public override void dropconnection(Websvc w)
    {
    }

    public override string myname()
    {
      return nameof (WebForm);
    }

    public override bool do_headers(Websvc w)
    {
      WebForm.imsg("do_headers in webform {0}", (object) ("." + w.url));
      this.mime = new Mime(true);
      WebForm.imsg("mime.DECODE_START {0}", (object) w.content_type);
      this.mime.decode_start(w.content_type);
      return true;
    }

    public override bool do_body(Websvc w, byte[] inbf, int inlen)
    {
      WebForm.imsg("Got body packet of {0} bytes", (object) inlen);
      this.mime.decode_packet(inbf, inlen);
      return true;
    }

    private bool form_exists(Websvc w, string name)
    {
      if (w.form != null)
      {
        string str = w.form.Get(name);
        if (str != null && str.Length > 0)
          return true;
      }
      if (w.query != null)
      {
        string str = w.query.Get(name);
        if (str != null && str.Length > 0)
          return true;
      }
      return false;
    }

    public static string form_get(Websvc w, string name, int i)
    {
      return WebForm.form_get(w, name + "_" + (object) i);
    }

    public static string form_get(Websvc w, string name)
    {
      if (w.form != null)
      {
        string str = w.form.Get(name);
        if (str != null && str.Length > 0)
          return str;
      }
      if (w.query != null)
      {
        string str = w.query.Get(name);
        if (str != null && str.Length > 0)
          return str;
      }
      return "";
    }

    private static string fname_decode(string fname)
    {
      string str = fname;
      clib.imsg("fname_decode {0} to {1}", (object) fname, (object) str);
      return str;
    }

    private void do_cmd_open(Websvc w)
    {
      string fname = WebForm.fname_decode(WebForm.form_get(w, "path"));
      SimpleStream ss = w.files.open(fname, true, false, out string _);
      WebFile.send_file(w, ss, fname, true, true);
    }

    private void do_cmd_view(Websvc w)
    {
      string fname = WebForm.fname_decode(WebForm.form_get(w, "path"));
      Files files = w.files;
      clib.imsg("cmd_view: {0}", (object) fname);
      SimpleStream ss = files.open(fname, true, false, out string _);
      WebFile.send_file(w, ss, fname, false, true);
    }

    private static void web_page_top(Websvc w, string sid)
    {
      bool isadmin = false;
      bool flag = false;
      if (w.top_done)
        return;
      w.top_done = true;
      w.ses = Session.find(sid);
      if (w.ses != null)
        isadmin = w.ses.isadmin();
      if (w.ses != null)
        flag = true;
      Web.top(w, clib.Product_Name(), clib.VersionBuild(), w.ses == null ? "" : w.ses.urec.user);
      if (flag)
        Web.show_menu(w, isadmin);
      Web.page_body(w);
      Web.web_start_form(w, "frm1");
    }

    private bool public_split(string url, out int uid, out string filepath)
    {
      int num1 = url.IndexOf("/", 1);
      string str = url.Substring(num1 + 1);
      int num2 = str.IndexOf("/");
      uid = 0;
      filepath = "none";
      if (num2 < 0)
        return false;
      string stuff = str.Substring(0, num2);
      uid = clib.atoi(stuff);
      filepath = str.Substring(num2);
      return true;
    }

    private bool setup_files(Websvc w, string user)
    {
      User user1 = Vuser.lookup(user);
      if (user1 == null)
        return false;
      w.files = new Files();
      w.files.set_profile(user1.user, user1.groups(), user1.quota());
      return true;
    }

    private bool do_download_url(Websvc w)
    {
      int uid;
      string filepath;
      if (!this.public_split(w.url, out uid, out filepath))
      {
        WebForm.wp_err(w, "500 Invalid url {0}", (object) w.url);
      }
      else
      {
        WebForm.imsg("url {0} uid {1} filepath {2}", (object) w.url, (object) uid, (object) filepath);
        string user = Vuser.uid_find_user(uid);
        if (user == null)
        {
          WebForm.wp_err(w, "500 User doesn't exist");
        }
        else
        {
          this.setup_files(w, user);
          if (!w.files.access_public(clib.pathonly(filepath)))
          {
            WebForm.wp_err(w, "Public browse not permitted for this folder {0}", (object) clib.pathonly(filepath));
          }
          else
          {
            SimpleStream ss = w.files.open(filepath, true, false, out string _);
            WebFile.send_file(w, ss, filepath, false, true);
            this.notify_user(w, user, "notify_download", filepath);
          }
        }
      }
      return true;
    }

    private bool do_public_url(Websvc w)
    {
      Web.start(w);
      Web.body(w);
      WebForm.web_page_top(w, "-1");
      int uid;
      string filepath;
      if (!this.public_split(w.url, out uid, out filepath))
      {
        WebForm.wp_err(w, "Invalid url {0}", (object) w.url);
      }
      else
      {
        WebForm.imsg("url {0} uid {1} filepath {2}", (object) w.url, (object) uid, (object) filepath);
        string user = Vuser.uid_find_user(uid);
        if (user == null)
        {
          WebForm.wp_err(w, "User doesn't exist");
        }
        else
        {
          this.setup_files(w, user);
          if (!w.files.access_public(filepath))
            WebForm.wp_err(w, "Public browse not permitted {0}", (object) w.url);
          else
            this.public_browse(w, uid, user, filepath);
        }
      }
      Web.web_end_form(w);
      w.ses = (Session) null;
      Web.page_end(w);
      Web.end(w);
      return true;
    }

    private void notify_user(Websvc w, string user, string action, string dest)
    {
      User user1 = Vuser.lookup(user);
      Smtp smtp = new Smtp();
      if (user1 == null || !user1.info.get_true(action))
        return;
      smtp.set_server(Ini.getstring(En.mail_server), Ini.getstring(En.mail_user), Ini.getstring(En.mail_pass), "25", "");
      string from = Ini.getstring(En.mail_from);
      if (from.Length == 0)
        from = "postmaster@" + Ini.getstring(En.mail_server);
      string body = (!(action == "notify_upload") ? string.Format("File downloaded from your FTPDAV folder\r\n    " + dest) : string.Format("File uploaded to your FTPDAV folder\r\n    " + dest)) + "\r\nAccess your FTPDAV area using: " + Ini.url_browse() + "\r\n";
      string message = smtp.build_body(from, user1.get_email(), "FtpDav " + action, body);
      string result;
      if (!smtp.send_message(from, user1.get_email(), message, out result))
        WebForm.wp_err(w, "<p>notify failed: {0}", (object) result);
      else
        Web.wp(w, "<p>User notified of upload", (object) result);
    }

    private bool do_public_upload(Websvc w)
    {
      string path = WebForm.form_get(w, "path");
      string user = Vuser.uid_find_user(clib.atoi(WebForm.form_get(w, "uid")));
      Web.start(w);
      Web.body(w);
      if (user != null)
      {
        this.setup_files(w, user);
        if (!w.files.access_upload(path))
        {
          WebForm.wp_err(w, "Upload not permitted {0}", (object) w.url);
        }
        else
        {
          WebForm.web_page_top(w, "-1");
          string str = path.TrimEnd('/') + "/" + clib.fileonly(WebForm.form_get(w, "u_attach_1_original"));
          WebForm.wp_ok(w, "Storing file: {0}\n", (object) str);
          string reason;
          if (!w.files.upload(WebForm.form_get(w, "u_attach_1"), str, out reason))
          {
            WebForm.wp(w, "<p class=\"red\">Upload FAILED {0} {1}", (object) str, (object) reason);
          }
          else
          {
            WebForm.wp(w, "<p class=\"green\">Upload worked {0} {1}", (object) str, (object) reason);
            this.notify_user(w, user, "notify_upload", str);
          }
          Web.web_end_form(w);
        }
      }
      w.ses = (Session) null;
      Web.page_end(w);
      Web.end(w);
      return true;
    }

    private bool do_upload_url(Websvc w)
    {
      Web.start(w);
      Web.body(w);
      WebForm.web_page_top(w, "-1");
      int uid;
      string filepath;
      if (!this.public_split(w.url, out uid, out filepath))
      {
        WebForm.wp_err(w, "Invalid url {0}", (object) w.url);
      }
      else
      {
        WebForm.imsg("url {0} uid {1} filepath {2}", (object) w.url, (object) uid, (object) filepath);
        string user = Vuser.uid_find_user(uid);
        if (user == null)
        {
          WebForm.wp_err(w, "User doesn't exist");
        }
        else
        {
          this.setup_files(w, user);
          if (!w.files.access_upload(filepath))
          {
            WebForm.wp_err(w, "Upload not permitted {0}", (object) w.url);
          }
          else
          {
            Web.web_input_file(w, "u_attach_1", "");
            Web.web_input_hidden(w, "uid", uid.ToString());
            Web.web_input_hidden(w, "cmd", "public_upload");
            Web.web_input_hidden(w, "path", filepath);
            Web.web_cmd_button(w, "cmd_upload_do", "Upload", "Upload a file to this directory");
          }
        }
      }
      Web.web_end_form(w);
      w.ses = (Session) null;
      Web.page_end(w);
      Web.end(w);
      return true;
    }

    private void public_browse(Websvc w, int uid, string user, string path)
    {
      Files files = w.files;
      if (path == "")
        path = "/";
      WebForm.wp_top(w, "Browse files for user {0} <p>", (object) user);
      Index index = files.get_index(path);
      string str1 = clib.pathonly(path);
      Web.wp(w, "<table class=\"files\">");
      if (str1.Length == 0 && path.Length > 1)
        str1 = "/";
      Web.wp(w, "<tr><th class=\"imgcol\">");
      Web.wp(w, "<Th class=\"imgcol\"><Th>Name<Th>Date<Th>Size");
      if (str1.Length > 0)
      {
        if (str1 == "/")
          str1 = "";
        string str2 = "/public/" + uid.ToString() + "/" + str1;
        Web.wp(w, "<tr><td><td><a href=\"{0}\"><img border=\"0\" src=\"{1}\"></a><Td>\n", (object) str2, (object) "/img/back.png");
      }
      int num = 0;
      foreach (Fileinfo fileinfo in index)
      {
        if (!clib.fileonly(fileinfo.name).StartsWith(".~"))
        {
          Web.wp(w, "<tr><td>");
          if (fileinfo.isdir)
          {
            string str2 = "/public/" + uid.ToString() + fileinfo.name;
            Web.wp(w, "<td class=\"imgcol\"><a href=\"{0}\"><img border=\"0\" src=\"{1}\"></a><Td>Directory: <a href=\"{0}\">{2}</a> <td><td>\n", (object) str2, (object) "/img/folder.png", (object) clib.fileonly(fileinfo.name));
          }
          else
          {
            string str2 = "/download/" + uid.ToString() + fileinfo.name;
            Web.wp(w, "<td class=\"imgcol\"><a target=\"_new\" href=\"{0}\"> <img border=\"0\" src=\"{1}\"> </a><Td><a target=\"_new\" href=\"{0}\">{2}</a><td>{3}<td>{4}", (object) str2, (object) ("/img/" + WebForm.choose_image(fileinfo.name)), (object) clib.fileonly(fileinfo.name), (object) fileinfo.modified.ToString(), (object) clib.nice_int(fileinfo.size));
          }
          ++num;
        }
      }
      Web.wp(w, "</table>");
    }

    private static void setup_session(Websvc w, string sid)
    {
      w.ses = Session.find(sid);
      if (w.ses == null)
        return;
      User urec = w.ses.urec;
      w.files = new Files();
      w.files.set_profile(urec.user, urec.groups(), urec.quota());
      w.files.mkdir("/", out string _);
    }

    public override bool do_body_end(Websvc w)
    {
      bool flag1 = false;
      this.mime.decode_end();
      w.form = this.mime.form;
      if (w.url.StartsWith("/public/"))
        return this.do_public_url(w);
      if (w.url.StartsWith("/download/"))
        return this.do_download_url(w);
      if (w.url.StartsWith("/upload/"))
        return this.do_upload_url(w);
      foreach (string index in (NameObjectCollectionBase) this.mime.form)
        WebForm.imsg("FORM_DATA:   {0,-10} {1}", (object) index, (object) this.mime.form[index]);
      string str1 = WebForm.form_get(w, "tx");
      string str2 = WebForm.form_get(w, "cmd");
      WebForm.imsg("Got END of body webform...");
      string str3 = Web.cookie_get(w, "mycloud_login");
      WebForm.imsg("cookie from web server {0}", (object) str3);
      if (this.form_exists(w, "cmd_logout"))
        Session.logout(str3);
      if (str2 == "public_upload")
        return this.do_public_upload(w);
      WebForm.setup_session(w, str3);
      WebForm.imsg("profile is sussed");
      if (w.ses != null)
      {
        if (str2 == "cmd_open")
        {
          this.do_cmd_open(w);
          return true;
        }
        if (str2 == "cmd_view")
        {
          this.do_cmd_view(w);
          return true;
        }
      }
      WebForm.imsg("web.start()");
      Web.start(w);
      WebForm.imsg("web.body()");
      Web.body(w);
      if (!this.form_exists(w, "cmd_wizard_save") && !this.form_exists(w, "cmd_login_do"))
        WebForm.web_page_top(w, str3);
      if (str1.Length > 0)
      {
        WebForm.do_paypal_upgrade(w);
      }
      else
      {
        WebForm.imsg("web.cmd...()");
        bool flag2 = false;
        if (w.ses != null)
          flag1 = true;
        if (w.ses != null)
          flag2 = w.ses.isadmin();
        foreach (WebForm.CMDS cmd in this.cmds)
        {
          try
          {
            if (!flag2)
            {
              if (cmd.myflags == WebForm.wflag.W_ADMIN)
                continue;
            }
            if (!flag1)
            {
              if (cmd.myflags != WebForm.wflag.W_LOGIN)
                continue;
            }
            if (str2 == cmd.cmd)
            {
              WebForm.imsg("xcmd: {0}", (object) cmd.cmd);
              cmd.myfn(w);
              goto label_59;
            }
          }
          catch (Exception ex)
          {
            WebForm.imsg("Exception in function {0} {1} {2}", (object) cmd.cmd, (object) ex.Message, (object) ex.ToString());
            WebForm.wp(w, "CRASH2: {0}", (object) ex.Message);
          }
        }
        foreach (WebForm.CMDS cmd in this.cmds)
        {
          try
          {
            if (!flag2)
            {
              if (cmd.myflags == WebForm.wflag.W_ADMIN)
                continue;
            }
            if (!flag1)
            {
              if (cmd.myflags != WebForm.wflag.W_LOGIN)
                continue;
            }
            if (this.form_exists(w, cmd.cmd))
            {
              WebForm.imsg("cmd: {0}", (object) cmd.cmd);
              cmd.myfn(w);
              goto label_59;
            }
          }
          catch (Exception ex)
          {
            WebForm.imsg("Exception in function {0} {1} {2}", (object) cmd.cmd, (object) ex.Message, (object) ex.ToString());
            clib.cmsg("Exception in function {0} {1} {2}", (object) cmd.cmd, (object) ex.Message, (object) ex.ToString());
            WebForm.wp(w, "CRASH: {0}", (object) ex.Message);
          }
        }
        if (!flag1)
          WebForm.web_cmd_login(w);
        else
          WebForm.wp(w, "Unknown command {0} ", (object) str2);
      }
label_59:
      Web.web_end_form(w);
      Web.page_end(w);
      WebForm.imsg("web.end()");
      Web.end(w);
      WebForm.imsg("web.enddone()");
      return true;
    }

    private static void web_cmd_status(Websvc w)
    {
      TimeSpan timeSpan1 = DateTime.Now.Subtract(MyMain.start_time);
      WebForm.wp_top(w, "Server status, uptime etc...");
      TimeSpan timeSpan2 = new TimeSpan(timeSpan1.Ticks - timeSpan1.Ticks % 100000L);
      Web.wp(w, "<p>Uptime: {0:d } ", (object) timeSpan2.ToString());
      if (MyMain.WebServer != null)
      {
        int num1 = MyMain.WebServer.CurrentConnections + MyMain.s_WebServer.CurrentConnections;
        int num2 = MyMain.WebServer.maxCurrentConnections + MyMain.s_WebServer.maxCurrentConnections;
        Web.wp(w, "<p>Web {0}/{1} connectionss {2} requests served", (object) num1, (object) num2, (object) Websvc.nrequests);
      }
      if (MyMain.WebDavServer != null)
      {
        int num1 = MyMain.WebDavServer.CurrentConnections + MyMain.s_WebDavServer.CurrentConnections;
        int num2 = MyMain.WebDavServer.maxCurrentConnections + MyMain.s_WebDavServer.maxCurrentConnections;
        Web.wp(w, "<p>WebDav {0}/{1} cons {2} requests, {3} still open", (object) num1, (object) num2, (object) WebDav.nrequests, (object) Websvc.get_nopen());
      }
      if (MyMain.ftpServer == null)
        return;
      int currentConnections1 = MyMain.ftpServer.CurrentConnections;
      int currentConnections2 = MyMain.ftpServer.maxCurrentConnections;
      Web.wp(w, "<p>FTP {0}/{1} cons {2} requests", (object) currentConnections1, (object) currentConnections2, (object) FtpService.nrequests);
    }

    private static void web_cmd_exit(Websvc w)
    {
      Web.wp(w, "CMD_EXIT  CALLED - Shutting down now...");
      MyMain.shutdown = true;
    }

    private static void web_cmd_config_save(Websvc w)
    {
      foreach (Ini.IniEntry iniEntry in Ini.ilist)
      {
        if (iniEntry.etype != Etype.COMMENT)
        {
          WebForm.imsg("Is there a value for {0} {1}", (object) iniEntry.name, (object) WebForm.form_get(w, iniEntry.name));
          Ini.do_set(iniEntry.idx, WebForm.form_get(w, iniEntry.name));
        }
      }
      Ini.save();
      WebForm.wp_ok(w, "Changes SAVED!");
      WebForm.web_cmd_config(w);
    }

    private static void web_cmd_upload_do(Websvc w)
    {
      string vpath = WebForm.form_get(w, "path").TrimEnd('/') + "/" + clib.fileonly(WebForm.form_get(w, "u_attach_1_original"));
      WebForm.wp(w, "Storing file: {0}\n", (object) vpath);
      string reason;
      if (!w.files.upload(WebForm.form_get(w, "u_attach_1"), vpath, out reason))
        WebForm.wp(w, "<p>Upload FAILED {0} {1}", (object) vpath, (object) reason);
      else
        WebForm.wp(w, "<p>Upload worked {0} {1}", (object) vpath, (object) reason);
      WebForm.web_cmd_browse(w);
    }

    private static void web_cmd_upload(Websvc w)
    {
      Web.web_input_hidden(w, "path", WebForm.form_get(w, "path"));
      Web.web_input_file(w, "u_attach_1", "");
      Web.web_cmd_button(w, "cmd_upload_do", "Upload Now", "Upload the chosen file");
    }

    public static string choose_image(string fname)
    {
      if (fname.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || fname.EndsWith(".gz", StringComparison.OrdinalIgnoreCase) || (fname.EndsWith(".Z", StringComparison.OrdinalIgnoreCase) || fname.EndsWith(".gzip", StringComparison.OrdinalIgnoreCase)) || fname.EndsWith(".tar", StringComparison.OrdinalIgnoreCase))
        return "package-x-generic.png";
      if (fname.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        return "application-x-executable.png";
      if (fname.EndsWith(".htm", StringComparison.OrdinalIgnoreCase) || fname.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        return "text-html.png";
      if (fname.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || fname.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) || (fname.EndsWith(".doc", StringComparison.OrdinalIgnoreCase) || fname.EndsWith(".odt", StringComparison.OrdinalIgnoreCase)) || (fname.EndsWith(".log", StringComparison.OrdinalIgnoreCase) || fname.EndsWith(".xls", StringComparison.OrdinalIgnoreCase)))
        return "text-x-generic.png";
      if (fname.EndsWith(".c") || fname.EndsWith(".h") || (fname.EndsWith(".cpp", StringComparison.OrdinalIgnoreCase) || fname.EndsWith(".cs")))
        return "text-x-script.png";
      if (fname.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) || fname.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
        return "video-x-generic.png";
      if (fname.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || fname.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) || fname.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        return "image-x-generic.png";
      return fname.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || fname.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || fname.EndsWith(".m4a", StringComparison.OrdinalIgnoreCase) ? "audio-x-generic.png" : "blank.png";
    }

    private static string fname_encodew(string x)
    {
      return clib.simple_encode(clib.string_to_utf8(x));
    }

    private static string fname_decodew(string x)
    {
      return clib.utf8_to_string(clib.simple_decode(x));
    }

    private static void web_cmd_del_sel(Websvc w)
    {
      Files files = w.files;
      int num = clib.atoi(WebForm.form_get(w, "sel_n"));
      for (int i = 0; i <= num; ++i)
      {
        string str = WebForm.fname_decodew(WebForm.form_get(w, "sel", i));
        if (str.Length > 0)
        {
          WebForm.imsg("Delete Selected: Form get {0}", (object) str);
          if (!clib.path_valid(str))
          {
            WebForm.wp_err(w, "Invalid path  to delete {0}", (object) str);
          }
          else
          {
            string reason;
            if (!files.delete_dir_or_file(str, out reason))
              WebForm.wp_err(w, "Unable to delete {0} {1}", (object) str, (object) reason);
          }
        }
      }
      WebForm.web_cmd_browse(w);
    }

    private static void web_cmd_copy_sel(Websvc w)
    {
      WebForm.web_cmd_move_sel(w, true);
    }

    private static void web_cmd_move_sel(Websvc w)
    {
      WebForm.web_cmd_move_sel(w, false);
    }

    private static void web_cmd_move_sel(Websvc w, bool iscopy)
    {
      Files files = w.files;
      int num1 = 0;
      string fname = clib.to_unix_slash(WebForm.form_get(w, "destination").Trim(" ".ToCharArray()).TrimEnd("/".ToCharArray()));
      clib.imsg("Destination starts {0}", (object) fname);
      if (!fname.StartsWith("/"))
        fname = WebForm.form_get(w, "path") + "/" + fname;
      clib.imsg("Destination updated to {0}", (object) fname);
      int num2 = clib.atoi(WebForm.form_get(w, "sel_n"));
      for (int i = 0; i < num2; ++i)
      {
        if (WebForm.form_get(w, "sel", i).Length > 0)
          ++num1;
      }
      bool flag = files.is_dir(fname);
      if (num1 > 1 && !flag)
      {
        Web.wp(w, "<p>Files not moved - destination must be a directory that already exists");
        WebForm.web_cmd_browse(w);
      }
      else
      {
        for (int i = 0; i < num2; ++i)
        {
          string str1 = WebForm.fname_decodew(WebForm.form_get(w, "sel", i));
          if (str1.Length > 0)
          {
            string str2 = fname + "/" + clib.fileonly(str1);
            if (!flag && num1 == 1)
              str2 = fname;
            string reason;
            if (iscopy)
            {
              WebForm.imsg("Copy {0} to {1}", (object) str1, (object) str2);
              if (!files.copy_dir(str1, str2, out reason, false))
                Web.wp(w, "<p>Error copying {0} to {1} {2}", (object) str1, (object) str2, (object) reason);
            }
            else
            {
              WebForm.imsg("Move {0} to {1}", (object) str1, (object) str2);
              if (files.is_dir(str1))
              {
                if (!files.rename_dir(str1, str2, out reason))
                  Web.wp(w, "<p>Error moving dir {0} to {1} {2}", (object) str1, (object) str2, (object) reason);
              }
              else if (!files.rename(str1, str2, out reason))
                Web.wp(w, "<p>Error moving file {0} to {1} {2}", (object) str1, (object) str2, (object) reason);
            }
          }
        }
        WebForm.web_cmd_browse(w);
      }
    }

    private static void web_cmd_set_public_true(Websvc w)
    {
      string reason;
      SimpleStream simpleStream = w.files.open_raw(WebForm.form_get(w, "path").TrimEnd("/".ToCharArray()) + "/.~access_public", false, false, out reason);
      if (simpleStream == null)
        Web.wp(w, "<p>set_upload failed {0}", (object) reason);
      else
        simpleStream.close();
      WebForm.web_cmd_browse(w);
    }

    private static void web_cmd_set_public_false(Websvc w)
    {
      Files files = w.files;
      string fname = WebForm.form_get(w, "path").TrimEnd("/".ToCharArray()) + "/.~access_public";
      WebForm.imsg("Delete: {0}", (object) fname);
      string reason;
      if (!files.delete_raw(fname, out reason))
        Web.wp(w, "<p>set_public_false failed {0}", (object) reason);
      WebForm.web_cmd_browse(w);
    }

    private static void web_cmd_set_upload_true(Websvc w)
    {
      string reason;
      SimpleStream simpleStream = w.files.open_raw(WebForm.form_get(w, "path").TrimEnd("/".ToCharArray()) + "/.~access_upload", false, false, out reason);
      if (simpleStream == null)
        Web.wp(w, "<p>set_upload failed {0}", (object) reason);
      else
        simpleStream.close();
      WebForm.web_cmd_browse(w);
    }

    private static void web_cmd_set_upload_false(Websvc w)
    {
      string reason;
      if (!w.files.delete_raw(WebForm.form_get(w, "path").TrimEnd("/".ToCharArray()) + "/.~access_upload", out reason))
        Web.wp(w, "<p>set_public_false failed {0}", (object) reason);
      WebForm.web_cmd_browse(w);
    }

    private static void web_cmd_mkdir(Websvc w)
    {
      Files files = w.files;
      string str = clib.to_unix_slash(WebForm.form_get(w, "destination").Trim(" ".ToCharArray()).TrimEnd("/".ToCharArray()));
      clib.imsg("Destination starts {0}", (object) str);
      if (!str.StartsWith("/"))
        str = WebForm.form_get(w, "path") + "/" + str;
      clib.imsg("Destination updated to {0}", (object) str);
      string fname = str.TrimEnd("/".ToCharArray());
      string reason;
      if (!files.mkdir(fname, out reason))
        Web.wp(w, "<p>mkdir failed {0}", (object) reason);
      WebForm.web_cmd_browse(w);
    }

    private static string fname_encode(string x)
    {
      string utf8 = clib.string_to_utf8(x);
      clib.imsg("fname_encode {0} to {1}", (object) x, (object) utf8);
      return clib.url_encode(utf8);
    }

    private static string view_encode(string x)
    {
      return clib.web_encode(x);
    }

    private static void web_cmd_browse(Websvc w)
    {
      Files files = w.files;
      int num1 = 0;
      double num2 = 0.0;
      string str1 = WebForm.form_get(w, "path");
      if (str1 == "")
        str1 = "/";
      WebForm.wp_top(w, "Browse files for user {0} path {1}", (object) w.ses.urec.user, (object) str1);
      Index index = files.get_index(str1);
      string input = clib.pathonly(str1);
      Web.wp(w, "<table class=\"files\">");
      if (input.Length == 0 && str1.Length > 1)
        input = "/";
      Web.wp(w, "<tr><th class=\"imgcol\">");
      Web.wp(w, "<input type=checkbox name=\"aaa\" onClick=\"checkall(form,this.checked)\">");
      Web.wp(w, "<Th class=\"imgcol\"><Th>Name<Th>Date<Th>Size");
      if (input.Length > 0)
        Web.wp(w, "<tr><td><td><a href=\"/cgi/admin.cgi?cmd=cmd_browse&path={0}\"><img border=\"0\" src=\"{2}\"></a><Td>\n", (object) clib.url_encode(input), (object) input, (object) "/img/back.png");
      int x = 0;
      foreach (Fileinfo fileinfo in index)
      {
        string str2 = clib.fileonly(fileinfo.name);
        if (!(str2 == ".DS_Store") && !(str2 == "._.DS_Store"))
        {
          ++num1;
          num2 += (double) fileinfo.size;
          Web.wp(w, "<tr><td>");
          if (fileinfo.isdir)
          {
            Web.web_checkbox(w, "sel_" + clib.int_to_string(x), WebForm.fname_encodew(fileinfo.name), "", false);
            Web.wp(w, "<td class=\"imgcol\"><a href=\"/cgi/admin.cgi?cmd=cmd_browse&path={0}\"><img border=\"0\" src=\"{2}\"></a><Td>Directory: <a href=\"/cgi/admin.cgi?cmd=cmd_browse&path={0}\">{1}</a> <td><td>\n", (object) WebForm.fname_encode(fileinfo.name), (object) WebForm.view_encode(fileinfo.name.file_only()), (object) "/img/folder.png");
          }
          else
          {
            Web.web_checkbox(w, "sel_" + clib.int_to_string(x), WebForm.fname_encodew(fileinfo.name), "", false);
            Web.wp(w, "<td class=\"imgcol\"><a target=\"_new\" href=\"/cgi/admin.cgi/{1}?cmd=cmd_view&path={0}\"> <img border=\"0\" src=\"{3}\"> </a><Td><a target=\"_new\" href=\"/cgi/admin.cgi/{1}?cmd=cmd_view&path={0}\">{2}</a><td>{4}<td>{5}", (object) WebForm.fname_encode(fileinfo.name), (object) WebForm.fname_encode(clib.fileonly(fileinfo.name)), (object) WebForm.view_encode(clib.fileonly(fileinfo.name)), (object) ("/img/" + WebForm.choose_image(fileinfo.name)), (object) fileinfo.modified.ToString(), (object) clib.nice_int_1024(fileinfo.size));
          }
          ++x;
        }
      }
      Web.wp(w, "</table>");
      Web.wp(w, "<p>Total {0} files, {1:f1}mb</p>", (object) num1, (object) (num2 / 1000000.0));
      Web.web_cmd_button(w, "cmd_del_sel", "Del Selected", "Delete the selected files");
      Web.web_cmd_buttoni(w, "cmd_move_sel", "Move Selected", "Move/Rename the selected files", string.Format("onClick = \"prompt_dest('cmd_move_sel',this.form,'Move','{0}')\"", (object) str1));
      Web.web_cmd_buttoni(w, "cmd_copy_sel", "Copy Selected", "Copy the selected files", string.Format("onClick = \"prompt_dest('cmd_copy_sel',this.form,'Copy','{0}')\"", (object) str1));
      Web.web_cmd_buttoni(w, "cmd_mkdir", "Create Folder", "Create Folder", string.Format("onClick = \"prompt_dest('cmd_mkdir',this.form,'Create Folder','{0}')\"", (object) str1));
      Web.web_cmd_button(w, "cmd_upload", "Upload Here", "Upload Here");
      Web.wp(w, "<div id=\"dest_div\" style=\"display:none; padding-left: 10px; margin:10px;\">");
      Web.wp(w, "<div id=\"dest_div2\" style=\"width: 500px;    height:30px; border: 2px solid Black; background-color:lightgreen;\">");
      Web.wp(w, "<p>Specify Destination: ");
      Web.wp(w, "<input type=\"text\"  id=\"dest_input\" name=\"destination\">");
      Web.web_cmd_button(w, "cmd_movecopy", "Move/Copy/Rename", "Doit", "id=\"movecopy\"");
      Web.wp(w, "</div>");
      Web.wp(w, "</div>");
      Web.web_input_hidden(w, "sel_n", clib.int_to_string(x));
      Web.web_input_hidden(w, "cmd", "");
      Web.web_input_hidden(w, "path", str1);
      Web.wp(w, "<p>Quota: {0}/{1}", (object) clib.nice_int(Quota.get(w.ses.urec.user)), (object) clib.nice_int_nolimit(w.ses.urec.quota()));
      Web.wp(w, "<p>");
      if (w.files.access_upload(str1))
      {
        Web.wp(w, "<p><a target=\"_new\" href=\"{0}\">Public Upload URL</a> ", (object) ("/upload/" + (object) Vuser.uid_get(w.ses.urec.user) + str1));
        Web.web_cmd_button_small(w, "cmd_set_upload_false", "Disable public uploads", "Disable anonymous upload access to this folder");
      }
      else
        Web.web_cmd_button_small(w, "cmd_set_upload_true", "Allow public uploads", "Enables anonymous upload access to this folder");
      Web.wp(w, "<p>");
      if (w.files.access_public(str1))
      {
        Web.wp(w, "<a target=\"_new\" href=\"{0}\">Public URL for this folder</a> ", (object) ("/public/" + (object) Vuser.uid_get(w.ses.urec.user) + str1));
        Web.web_cmd_button_small(w, "cmd_set_public_false", "Make Private", "Disable anonymous access to 'read' the folder");
      }
      else
        Web.web_cmd_button_small(w, "cmd_set_public_true", "Make Folder Public", "Enables anonymous access to 'read' the folder");
    }

    private static void web_cmd_log(Websvc w)
    {
      WebForm.wp_top(w, "View log files");
      string str = WebForm.form_get(w, "logfile");
      if (str == "")
        str = "imsg.log";
      Web.web_select_text(w, "logfile", "imsg.log,ftp.log,webdav.log,web.log,crash.log,startstop.log", str);
      Web.web_cmd_button(w, "cmd_log", "Refresh", "Show main log file");
      Web.wp(w, "<p><font face=\"Verdana, Arial, Helvetica, sans-serif\" size=1>");
      string x = clib.log_read_last(clib.log_file(str), 100000);
      int startIndex = x.IndexOf("\n");
      if (startIndex > 0)
        x = x.Substring(startIndex);
      foreach (string stringLine in x.string_lines())
      {
        if (stringLine != null)
          Web.wp(w, "{0}", (object) (clib.web_encode(stringLine) + "<br>\n"));
        else
          break;
      }
      Web.wp(w, "</font>");
    }

    private static void web_cmd_users_save(Websvc w)
    {
      WebForm.imsg("cmd_users_save pressed");
      Web.wp(w, "<br><p>");
      string user1 = WebForm.form_get(w, "u_user");
      string passwd = WebForm.form_get(w, "u_pass");
      NameValueCollection info = new NameValueCollection();
      foreach (string field in Vuser.fields)
        info.Set(field, WebForm.form_get(w, "u_" + field));
      string oldpass = "";
      info.Set("groups", WebForm.web_get_groups(w));
      bool flag = true;
      User user2 = Vuser.lookup(user1);
      if (user2 != null)
      {
        oldpass = user2.passwd;
        WebForm.imsg("Old user exists, and password is {0}", (object) oldpass);
        flag = false;
      }
      else
        WebForm.imsg("Old user doesn't exist");
      string reason;
      if (!Vuser.add(user1, passwd, info, out reason, oldpass))
        WebForm.wp_err(w, "Userdb error saving {0} user to database {1}", (object) user1, (object) reason);
      else
        WebForm.wp_ok(w, "Saved {0} user to database {1}", flag ? (object) "new" : (object) "existing", (object) user1);
      WebForm.imsg("cmd_users_save pressed - finishd1");
      WebForm.web_cmd_users(w);
      WebForm.imsg("cmd_users_save pressed - finished2");
    }

    private static void web_cmd_users_delete(Websvc w)
    {
      string user = WebForm.form_get(w, "u_user");
      if (Vuser.lookup(user) == null)
      {
        Web.wp(w, "<p>User didn't exist... {0}", (object) user);
        WebForm.web_cmd_users(w);
      }
      else
      {
        Web.wp(w, "<p>Deleted user {0}", (object) user);
        Vuser.delete(user);
        WebForm.web_cmd_users(w);
      }
    }

    private static bool search_cb(object obj, User u)
    {
      Web.wp((Websvc) obj, "Match: <a href=\"/cgi/admin.cgi?cmd=cmd_users&u_user={0}\">{0}</a><br>", (object) u.user);
      return true;
    }

    private static void web_cmd_users_search(Websvc w)
    {
      Web.wp(w, "<p>");
      string user = WebForm.form_get(w, "u_user");
      Web.wp(w, "Search results for [{0}]<p>", (object) user);
      Vuser.search(user, new UserDb.search_cb(WebForm.search_cb), (object) w);
      Web.wp(w, "<hr>");
      WebForm.web_cmd_users(w);
    }

    private static void web_cmd_users_lookup(Websvc w)
    {
      Web.wp(w, "<br><p>");
      string user = WebForm.form_get(w, "u_user");
      if (Vuser.lookup(user) == null)
      {
        Web.wp(w, "<p>User didn't exist... {0}", (object) user);
        WebForm.web_cmd_users(w);
      }
      else
      {
        Web.wp(w, "lookup user {0}", (object) user);
        WebForm.web_cmd_users(w);
      }
    }

    private static void web_cmd_users(Websvc w)
    {
      WebForm.wp_top(w, "Lookup existing users, add new users, set group access for users");
      WebForm.imsg("cmd_users: Lookup user");
      User user = Vuser.lookup(Vuser.add_domain(WebForm.form_get(w, "u_user")));
      WebForm.imsg("cmd_users: show some stuff");
      Web.web_cmd_button(w, "cmd_users_delete", "Delete User", "Delete user");
      Web.web_cmd_button(w, "cmd_users_search", "Search", "Find existing user");
      Web.web_table_start(w);
      Web.web_table_start(w);
      WebForm.imsg("cmd_users: show table stuff");
      Web.wp(w, "<tr><td  align=right>User<td>");
      Web.web_input_text(w, "u_user", WebForm.form_get(w, "u_user"));
      Web.wp(w, "<td>");
      Web.web_cmd_button(w, "cmd_users_lookup", "Lookup", "Find existing user");
      Web.wp(w, "<tr><td  align=right>Pass<td>");
      Web.web_input_text(w, "u_pass", "");
      Web.wp(w, "<td>");
      Web.web_cmd_button(w, "cmd_users_save", "Save User", "Save user");
      WebForm.imsg("cmd_users: show fields");
      if (Vuser.fields != null)
      {
        foreach (string field in Vuser.fields)
        {
          if (field == "groups")
          {
            string grps = "";
            WebForm.imsg("cmd_users: groups");
            if (user != null)
              grps = user.info.Get(field);
            WebForm.web_groups(w, grps);
          }
          else if (field.StartsWith("notify_"))
          {
            bool check = false;
            WebForm.imsg("cmd_users: notify");
            if (user != null)
              check = user.info.Get(field) == "true";
            Web.wp(w, "<tr><td  align=right>{0}:<td>", (object) clib.Capitalize(field));
            Web.web_checkbox(w, "u_" + field, "true", "", check);
          }
          else
          {
            WebForm.imsg("cmd_users: xxx");
            Web.wp(w, "<tr><td  align=right>{0}:<td>", (object) clib.Capitalize(field));
            Web.web_input_text(w, "u_" + field, user == null ? "" : user.info.Get(field));
          }
        }
      }
      WebForm.imsg("cmd_users: show notify stuff");
      if (user != null && user.info != null)
      {
        foreach (string allKey in user.info.AllKeys)
        {
          if (!((IEnumerable<string>) Vuser.fields).Contains<string>(allKey))
          {
            Web.wp(w, "<tr><td  align=right>{0}:<td>", (object) clib.Capitalize(allKey));
            Web.web_input_text(w, "u_" + allKey, user == null ? "" : user.info.Get(allKey));
          }
        }
      }
      WebForm.imsg("cmd_users: finished");
      Web.wp(w, "</table>");
    }

    private static void web_cmd_register_activate(Websvc w)
    {
      string reason = "";
      string keycode;
      if (!MyKey.keylib_activate_get("ftpdav", WebForm.form_get(w, "regid"), WebForm.form_get(w, "email"), out keycode, out reason))
      {
        Web.wp(w, "<p>Activate error: {0}", (object) reason);
        Web.wp(w, "<h3>If web base activation fails send this data via email to keyrobot@netwinsite.com</h3>");
        Web.wp(w, "<pre>\r\n");
        Web.wp(w, "{0}\r\n", (object) MyKey.email_message());
        Web.wp(w, "</pre>\r\n");
      }
      else
      {
        MyKey.save();
        WebForm.imsg("activate gave us code {0} {1}", (object) keycode, (object) reason);
        if (!MyKey.decode(keycode, out reason))
          Web.wp(w, "<p>Loading that key failed: {0}", (object) reason);
        else
          MyKey.save();
        MyKey.load();
        WebForm.web_cmd_register(w);
      }
    }

    private static void web_cmd_register_save(Websvc w)
    {
      string reason = "";
      if (!MyKey.decode(WebForm.form_get(w, "code"), out reason))
        Web.wp(w, "<p>Loading that key failed: {0}", (object) reason);
      else
        MyKey.save();
      MyKey.load();
      WebForm.web_cmd_register(w);
    }

    private static void web_cmd_register(Websvc w)
    {
      WebForm.wp_top(w, "Use this page to purchase a license and load your key if you need to");
      Web.wp(w, "<h3>Current Key Status</h3>");
      Web.wp(w, "<p>Note: FtpDav is free for personal use, for the first 5 users - you only need a key if you wish to have more users.");
      Web.web_table_start(w);
      if (MyKey.istemp)
        Web.wp(w, "<tr><td>State<td>Temporary");
      if (MyKey.isexpired())
        Web.wp(w, "<tr><td>State<td>Expired!!!");
      Web.wp(w, "<tr><td>Regid<td>N{0}", (object) MyKey.regid);
      Web.wp(w, "<tr><td>EMail<td>{0}", (object) MyKey.s_email);
      Web.wp(w, "<tr><td>Code<td>{0}", (object) MyKey.s_code);
      Web.wp(w, "<tr><td>Host<td>{0}", (object) MyKey.get_host());
      Web.wp(w, "<tr><td>User Limit<td>{0}", (object) MyKey.ulimit);
      Web.wp(w, "<tr><td>Local Users<td>{0}", (object) Vuser.user_count());
      Web.wp(w, "<tr><td>Expires<td>{0}", (object) clib.nice_unix_date(MyKey.expdate));
      Web.wp(w, "</table>");
      Web.wp(w, "<h3>Purchase...</h3>");
      Web.wp(w, "<p><a href=\"{0}\">Purchase Now</a>", (object) MyKey.purchase_url());
      Web.wp(w, "<h3>Load license with registration number</h3>");
      Web.web_table_start(w);
      Web.wp(w, "<tr><td  align=right>Reginstration Number (Nxxxx)<td>");
      Web.web_input_text(w, "regid", string.Format("N{0}", (object) MyKey.regid));
      Web.wp(w, "<tr><td  align=right>Reginstration EMail (you@your.domain)<td>");
      Web.web_input_text(w, "email", MyKey.s_email);
      Web.wp(w, "</table>");
      Web.web_cmd_button(w, "cmd_register_activate", "Save key", "Save and activate key");
      Web.wp(w, "<h3>Load license code</h3>");
      Web.web_table_start(w);
      Web.wp(w, "<tr><td  align=right>License Code (e.g. a1ff9933123453...)<td>");
      Web.web_input_text_size(w, "code", MyKey.s_code, 40);
      Web.wp(w, "</table>");
      Web.web_cmd_button(w, "cmd_register_save", "Save activation code", "Save and activate key");
    }

    private static void web_cmd_config(Websvc w)
    {
      bool flag = true;
      WebForm.wp_top(w, "Main service config options, specify ports to listen on, mail server etc..");
      Web.web_table_start(w);
      foreach (Ini.IniEntry iniEntry in Ini.ilist)
      {
        if (iniEntry.etype == Etype.COMMENT)
        {
          if (!flag)
          {
            Web.wp(w, "<tr><td> <td>");
            Web.web_cmd_button(w, "cmd_config_save", "Save changes", "Save the settings you have modified");
          }
          Web.wp(w, "<tr><td  align=left colspan=2><a name=\"x{0}\" >{1}<hr></a> <td>", (object) iniEntry.name, (object) iniEntry.help);
          flag = false;
        }
        else
          Web.wp(w, "<tr><td  align=right><a name=\"x{0}\">{1}</a><td>", (object) iniEntry.name, (object) iniEntry.help);
        if (iniEntry.etype == Etype.STRING)
          Web.web_input_text(w, iniEntry.name, Ini.getstring(iniEntry.idx));
        else if (iniEntry.etype == Etype.BOOL)
          Web.web_checkbox(w, iniEntry.name, "true", "", Ini.istrue(iniEntry.idx));
        else if (iniEntry.etype == Etype.INT)
          Web.web_input_text(w, iniEntry.name, Ini.getstring(iniEntry.idx));
        Web.wp(w, "{0}", (object) iniEntry.hint);
      }
      Web.wp(w, "<tr><td> <td>");
      Web.web_cmd_button(w, "cmd_config_save", "Save changes", "Save the settings you have modified");
      Web.wp(w, "</table>");
      Web.wp(w, "<p>You must press the 'save' button on this form or changes are not stored");
    }

    public static string web_get_groups(Websvc w)
    {
      List<string> stringList = Ini.valid_groups();
      string str1 = "";
      foreach (string str2 in stringList)
      {
        if (WebForm.form_get(w, "g_" + str2).Length > 0)
          str1 = str1 + str2 + ",";
      }
      return str1.Trim(",".ToCharArray());
    }

    private static void web_cmd_profile_save(Websvc w)
    {
      int i1 = clib.atoi(WebForm.form_get(w, "id"));
      Profile padd = Profile.profile_get(i1);
      if (padd == null)
      {
        padd = new Profile("", "", "", "");
        Profile.profile_add(padd);
      }
      padd.paths_clear();
      padd.name = WebForm.form_get(w, "p_name");
      padd.users = WebForm.form_get(w, "p_users");
      padd.run = WebForm.form_get(w, "p_run");
      padd.groups = WebForm.web_get_groups(w);
      for (int i2 = 0; i2 < 100; ++i2)
      {
        string alias = WebForm.form_get(w, "alias", i2);
        if (alias != null && alias.Length != 0)
        {
          string access = WebForm.form_get(w, "access", i2);
          padd.path_add(alias, WebForm.form_get(w, "path", i2), access);
        }
      }
      Profile.save();
      WebForm.wp_ok(w, "PROFILE {0} {1} Saved! ", (object) i1, (object) padd.name);
      WebForm.web_cmd_profile(w);
    }

    private static void web_groups(Websvc w, string grps)
    {
      if (grps == null)
        grps = "";
      List<string> stringList = Ini.valid_groups();
      Web.wp(w, "<tr><td  align=right>Groups ");
      Web.wp(w, "<td>");
      foreach (string x in stringList)
        Web.wp(w, "<input {2} type=\"checkbox\" name=\"g_{0}\" value=\"checked\"/>{1}, ", (object) x, (object) clib.Capitalize(x), grps.Contains(x) ? (object) "checked" : (object) "");
    }

    private static void quota_reset(Websvc w, Files files, User u, string path)
    {
      Web.wp(w, "Quota adding folder {0}<br>", (object) path);
      foreach (Fileinfo fileinfo in files.get_index(path))
      {
        Web.wp(w, "Quota adding {0} {1} {2}<br>", (object) fileinfo.name, (object) fileinfo.size, (object) fileinfo.isdir);
        Quota.add(u.user, fileinfo.size);
        if (fileinfo.isdir)
          WebForm.quota_reset(w, files, u, path.path_add(fileinfo.name.file_only()));
      }
    }

    private static void web_cmd_quota_repair(Websvc w)
    {
      Files files = w.files;
      WebForm.imsg("cmd_quota_repair pressed");
      User urec = w.ses.urec;
      Quota.reset(urec.user);
      WebForm.quota_reset(w, files, urec, "/");
      WebForm.web_cmd_browse(w);
    }

    private static void web_cmd_options_save(Websvc w)
    {
      WebForm.imsg("cmd_options_save pressed");
      User urec = w.ses.urec;
      urec.info.Set("notify_upload", WebForm.form_get(w, "notify_upload"));
      urec.info.Set("notify_download", WebForm.form_get(w, "notify_download"));
      string pass = WebForm.form_get(w, "password_old");
      string passwd = WebForm.form_get(w, "password_new");
      string str = WebForm.form_get(w, "password_new2");
      string reason;
      if (passwd.Length > 0)
      {
        if (passwd != str)
        {
          Web.wp(w, "<p class=\"red\">Sorry new passwords didn't match </p>");
          goto label_8;
        }
        else if (!Vuser.check(urec.user, pass, out reason))
        {
          Web.wp(w, "<p class=\"red\">Sorry old password didn't match </p>");
          goto label_8;
        }
      }
      if (!Vuser.add(urec.user, passwd, urec.info, out reason, ""))
        WebForm.wp_err(w, "Userdb error saving user to database {0}", (object) reason);
      else
        WebForm.wp_ok(w, "Successfully saved changes");
label_8:
      WebForm.web_cmd_options(w);
    }

    private static void web_cmd_options(Websvc w)
    {
      User urec = w.ses.urec;
      WebForm.wp_top(w, "User Preferences");
      Web.web_table_start(w);
      Web.wp(w, "<tr><td>Send email on uploads<td>");
      Web.web_checkbox(w, "notify_upload", "true", "", urec.info.get_true("notify_upload"));
      Web.wp(w, "<tr><td>Send email on downloads<td>");
      Web.web_checkbox(w, "notify_download", "true", "", urec.info.get_true("notify_download"));
      Web.wp(w, "</table>");
      Web.web_cmd_button(w, "cmd_options_save", "Save changes", "Save the settings you have modified");
      Web.wp(w, "<hr>");
      Web.wp(w, "<p> Modify your password");
      Web.web_table_start(w);
      Web.wp(w, "<tr><td>Old Password<td>");
      Web.web_input_password(w, "password_old", "");
      Web.wp(w, "<tr><td>New Password<td>");
      Web.web_input_password(w, "password_new", "");
      Web.wp(w, "<tr><td>New Password<td>");
      Web.web_input_password(w, "password_new2", "");
      Web.wp(w, "</table>");
      Web.web_cmd_button(w, "cmd_options_save", "Save Password", "Save new password");
      Web.wp(w, "<hr>");
      Web.web_cmd_button(w, "cmd_quota_repair", "ReCheck Quota", "ReCheck Quota");
      if (Ini.getstring(En.upgrade_price).Length <= 0)
        return;
      WebForm.wp(w, "<hr>");
      if (!urec.is_upgraded_soon())
      {
        WebForm.wp(w, "<p>Account quota is currently {0}</p>\n", (object) clib.nice_int_nolimit(urec.quota()));
        WebForm.wp(w, "</form>\n");
        WebForm.wp(w, "<form name=\"_xclick\" action=\"https://www.paypal.com/cgi-bin/webscr\" method=\"post\">\n");
        WebForm.wp(w, "<p><b> To upgrade quota to {0} for ${1}/year click here: ", (object) Ini.getstring(En.upgrade_quota), (object) Ini.getstring(En.upgrade_price));
        Web.web_input_hidden(w, "cmd", "_xclick");
        Web.web_input_hidden(w, "business", Ini.getstring(En.paypal_account));
        Web.web_input_hidden(w, "currency_code", "USD");
        Web.web_input_hidden(w, "custom", urec.user);
        Web.web_input_hidden(w, "item_name", "Upgrade Quota");
        Web.web_input_hidden(w, "amount", Ini.getstring(En.upgrade_price));
        WebForm.wp(w, "<input type=\"image\" src=\"http://www.paypal.com/en_US/i/btn/btn_buynow_LG.gif\" border=\"0\" name=\"submit\" alt=\"Make payments with PayPal - it's fast, free and secure!\">\n");
        WebForm.wp(w, "</b></p>\n");
        WebForm.wp(w, "</form>\n");
      }
      else
        WebForm.wp(w, "<p>Account has been upgraded already, quota is {0}</p>\n", (object) clib.nice_int_nolimit(urec.quota()));
    }

    private static void do_paypal_upgrade(Websvc w)
    {
      string str1 = "www.paypal.com";
      string user1 = WebForm.form_get(w, "cm");
      string postUrl = string.Format("http://{0}/cgi-bin/webscr", (object) str1);
      Dictionary<string, object> postParameters = new Dictionary<string, object>();
      WebForm.imsg("Customer paid {0} via paypal for customer {1} ", (object) WebForm.form_get(w, "amt"), (object) user1);
      postParameters.Add("cmd", (object) "_notify-synch");
      postParameters.Add("tx", (object) WebForm.form_get(w, "tx"));
      postParameters.Add("at", (object) Ini.getstring(En.paypal_token));
      string end;
      try
      {
        HttpWebResponse httpWebResponse = WebHelpers.MultipartFormDataPost(postUrl, "FtpDav", postParameters, "", "");
        end = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
        httpWebResponse.Close();
      }
      catch (Exception ex)
      {
        stat.imsg("paypal confirm attempt failed {0}", (object) ex.ToString());
        WebForm.wp_err(w, "paypal confirm attempt failed {0}", (object) ex.ToString());
        return;
      }
      bool flag = false;
      string str2 = "";
      foreach (string str3 in end.string_lines_any())
      {
        WebForm.imsg("PAYPAL Response: {0}", (object) str3);
        if (str3 == "FAIL")
        {
          WebForm.wp_err(w, "paypal postback failed {0}", (object) end);
          return;
        }
        if (str3 == "SUCCESS")
          flag = true;
        string[] strArray = str3.Split("=".ToCharArray());
        if (((IEnumerable<string>) strArray).Count<string>() >= 2)
        {
          if (strArray[0] == "payment_status" && strArray[1] != "Completed")
          {
            WebForm.wp_err(w, "paypal not completed status {0}", (object) end);
            return;
          }
          if (strArray[0] == "payer_email")
            str2 = strArray[1];
        }
      }
      if (!flag)
      {
        WebForm.wp_err(w, "paypal did not get a success return status from paypal");
      }
      else
      {
        User user2 = Vuser.lookup(user1);
        if (user2 == null)
        {
          WebForm.wp_err(w, "paypal: sorry unknown account ? {0}", (object) user1);
        }
        else
        {
          user2.info.Set("paid_date", clib.time().ToString());
          string reason;
          if (!user2.save(out reason))
          {
            WebForm.wp_err(w, "paypal: sorry failed to update account informaiton, please contact the administrator {0}", (object) reason);
          }
          else
          {
            WebForm.wp_ok(w, "Account upgraded, new quota limit for {0} is {1}", (object) user1, (object) Ini.getstring(En.upgrade_quota));
            WebForm.web_cmd_login(w);
          }
        }
      }
    }

    private static void web_cmd_profile(Websvc w)
    {
      WebForm.wp_top(w, "Profiles let you specify which users/groups get mapped to which physical disk paths");
      Web.web_table_start(w);
      int i1 = 0;
      int i2 = clib.atoi(WebForm.form_get(w, "id"));
      WebForm.imsg("PROFILE {0}======================", (object) i2);
      Profile profile = Profile.profile_get(i2) ?? new Profile("", "", "", "");
      Web.web_input_hidden(w, "id", i2.ToString());
      Web.wp(w, "<tr><td>Profile Name<td>");
      Web.web_input_text(w, "p_name", profile.name);
      Web.wp(w, "<tr><td>Users<td>");
      Web.web_input_text(w, "p_users", profile.users);
      Web.wp(w, "<tr><td>Run command on upload<td>");
      Web.web_input_text(w, "p_run", profile.run);
      WebForm.web_groups(w, profile.groups);
      Web.wp(w, "</table>");
      Web.web_table_start(w);
      Web.wp(w, "<tr><th><th>Virtual Alias<th>Physical Path<th>Access (default=read+write)");
      foreach (VPath path in profile.paths)
      {
        Web.wp(w, "<tr><td>Path Mapping");
        Web.wp(w, "<td>");
        Web.web_input_text_i(w, "alias", path.alias, i1);
        Web.wp(w, "<td>");
        Web.web_input_text_i(w, "path", path.path, i1);
        Web.wp(w, "<td>");
        Web.web_radio_i(w, "access", "full", "Full", path.isfull(), i1);
        Web.web_radio_i(w, "access", "upload", "Upload", path.isupload(), i1);
        Web.web_radio_i(w, "access", "readonly", "Read Only", path.isreadonly(), i1);
        ++i1;
      }
      Web.wp(w, "<tr><td>Path Mapping");
      Web.wp(w, "<td>");
      Web.web_input_text_i(w, "alias", "", i1);
      Web.wp(w, "<td>");
      Web.web_input_text_i(w, "path", "", i1);
      Web.wp(w, "<td>");
      Web.web_radio_i(w, "access", "readonly", "Read", false, i1);
      Web.web_radio_i(w, "access", "upload", "Upload", false, i1);
      Web.web_radio_i(w, "access", "full", "Full", true, i1);
      Web.wp(w, "</table>");
      Web.web_cmd_button(w, "cmd_profile_save", "Save changes", "Save the settings you have modified");
      Web.wp(w, "<p>The real path can include $user$ which will be replaced by the login name<hr>");
      WebForm.web_cmd_profile_list(w);
    }

    private static void web_cmd_profile_list(Websvc w)
    {
      Web.web_table_start(w);
      Web.wp(w, "<h3>Other Profiles</h3>");
      Web.wp(w, "<tr><th>Profile<th>Users<th>Groups");
      int i;
      for (i = 0; i < Profile.profile_n(); ++i)
      {
        Profile profile = Profile.profile_get(i);
        Web.wp(w, "<tr><td  align=right><a href=\"/cgi/admin.cgi?cmd=cmd_profile&id={0}\">{1}</a>", (object) i, (object) profile.name);
        Web.wp(w, "<td>{0}", (object) profile.users);
        Web.wp(w, "<td>{0}", (object) profile.groups);
      }
      Web.wp(w, "<tr><td  align=right><a class=\"btnnormal\" href=\"/cgi/admin.cgi?cmd=cmd_profile&id={0}\">{1}</a>", (object) i, (object) "New Profile");
      Web.wp(w, "</table>");
    }

    public static bool wp(Websvc w, string format, params object[] args)
    {
      string str = string.Format(format, args);
      return Web.wp(w, "{0}", (object) str);
    }

    public static bool wp_ok(Websvc w, string format, params object[] args)
    {
      string str = string.Format(format, args);
      WebForm.imsg("wp_ok: {0}", (object) str);
      return Web.wp(w, "<p class=\"green\">{0}</p><hr>", (object) str);
    }

    public static bool wp_top(Websvc w, string format, params object[] args)
    {
      string str = string.Format(format, args);
      return Web.wp(w, "<h2 class=\"top\">{0}</p>", (object) str);
    }

    public static bool wp_err(Websvc w, string format, params object[] args)
    {
      string str = string.Format(format, args);
      WebForm.imsg("wp_err: {0}", (object) str);
      return Web.wp(w, "<p class=\"red\">{0}</p><hr>", (object) str);
    }

    private static void web_cmd_reset_password(Websvc w)
    {
      string user1 = WebForm.form_get(w, "u_user");
      WebForm.form_get(w, "u_pass");
      WebForm.imsg("reset_password {0}", (object) user1);
      string user2 = Vuser.add_domain(user1);
      if (Vuser.lookup(user2) == null)
      {
        if (user2.Length > 0)
          Thread.Sleep(5000);
        WebForm.wp_err(w, "Unknown account, enter your email account name before pressing the button");
      }
      else
      {
        string str = "tmp" + clib.int_to_string(new Random().Next(1000000));
        Vuser.add(user2, str, (NameValueCollection) null, out string _, "");
        WebForm.send_password(w, user2, str);
        WebForm.wp_ok(w, "Your password has been reset, read your email to find your new password");
        WebForm.imsg("YOUR PASSWORD HAS BEEN RESET,");
      }
      WebForm.web_cmd_login(w);
    }

    private static void web_cmd_login_do(Websvc w)
    {
      WebForm.imsg("cmd_login_do");
      string user1 = WebForm.form_get(w, "u_user");
      string pass = WebForm.form_get(w, "u_pass");
      string user2 = Vuser.add_domain(user1);
      string reason;
      if (!Vuser.check(user2, pass, out reason))
      {
        string str = w.chan.RemoteEndPoint.ToString();
        WebForm.imsg("cmd_login_do user_check failed");
        WebForm.web_page_top(w, "");
        WebForm.wp(w, "LOGIN FAILED, {0}<br>\n", (object) reason);
        WebForm.wp(w, "Remote connection {0}<br>\n", (object) w.chan.RemoteEndPoint.ToString());
        if (str.Contains("127.0.0") || str.Contains("192.168."))
          WebForm.wp(w, "Hint: If you have forgotten your admin password, you can recreate the admin account by deleting {0}", (object) clib.work("wizard.done"));
        WebForm.web_cmd_login(w);
      }
      else
      {
        string str = Session.create(Vuser.lookup(user2));
        Web.cookie_set(w, "mycloud_login", str);
        WebForm.web_page_top(w, str);
        if (w.ses.isadmin())
          WebForm.wp_ok(w, "Login Successful! ADMIN access granted\n");
        else
          WebForm.wp(w, "Login Successful! \n");
        clib.imsg("User {0} logged into web interface ok", (object) user2);
        WebForm.setup_session(w, str);
        WebForm.web_cmd_browse(w);
      }
    }

    public static bool configure_done()
    {
      return System.IO.File.Exists(clib.work("wizard.done"));
    }

    public static void notify_netwinsite(string email)
    {
      string postUrl = string.Format("http://{0}/cgi-bin/keycgi.exe", (object) "netwinsite.com");
      Dictionary<string, object> postParameters = new Dictionary<string, object>();
      postParameters.Add("cmd", (object) "ftpdav_install");
      postParameters.Add(nameof (email), (object) email);
      postParameters.Add("host", (object) MyKey.host);
      postParameters.Add("build", (object) clib.VersionBuild());
      postParameters.Add("dummy", (object) "test");
      try
      {
        string userAgent = "ftpdav";
        HttpWebResponse httpWebResponse = WebHelpers.MultipartFormDataPost(postUrl, userAgent, postParameters, "", "");
        string end = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
        httpWebResponse.Close();
        stat.imsg("keycgi install response cmd=ftpdav_install {0}", (object) end);
      }
      catch (Exception ex)
      {
        stat.imsg("keycgi wizard install failed {0}", (object) ex.ToString());
      }
    }

    private static void web_cmd_wizard_save(Websvc w)
    {
      WebForm.imsg("cmd_wizard_save event");
      if (WebForm.configure_done())
        return;
      string path = clib.work("wizard.done");
      string str = Vuser.add_domain(WebForm.form_get(w, "u_user"));
      string reason;
      if (!Vuser.valid_user(str, out reason))
      {
        WebForm.wp_err(w, "Sorry invalid account name, user a valid email address, e.g. admin@your.domain ");
        WebForm.do_configure(w);
      }
      else
      {
        System.IO.File.WriteAllText(path, "done");
        NameValueCollection info = new NameValueCollection();
        info.Add("groups", "admin");
        WebForm.imsg("cmd_wizard_save event - adding user ok - {0}", (object) str);
        if (!Vuser.add(str, WebForm.form_get(w, "u_pass"), info, out reason, ""))
        {
          WebForm.wp_err(w, "Creating user failed {0}", (object) reason);
          WebForm.do_configure(w);
        }
        else
        {
          WebForm.notify_netwinsite(str);
          Ini.do_set(En.admin_email, str);
          if (str.Contains("@"))
            Ini.do_set(En.default_domain, str.Split('@')[1]);
          Ini.save();
          WebForm.web_cmd_login_do(w);
        }
      }
    }

    private static void do_configure(Websvc w)
    {
      Web.web_start_form(w, "frm1");
      WebForm.wp(w, "<p>");
      WebForm.wp(w, "Configuration Wizard - First we need to create an admin account<br>\n");
      Web.web_table_start(w);
      WebForm.wp(w, "<tr><td  align=right>Admin Email:<td>");
      Web.web_input_text(w, "u_user", "");
      WebForm.wp(w, "<tr><td  align=right>Admin Password:<td>");
      Web.web_input_text(w, "u_pass", "");
      WebForm.wp(w, "</table>");
      Web.web_cmd_button(w, "cmd_wizard_save", "Create", "Create Account");
      WebForm.wp(w, "<p> Hint: To make life simple use your email address as your account name");
    }

    private static void web_cmd_login(Websvc w)
    {
      if (!WebForm.configure_done())
      {
        WebForm.do_configure(w);
      }
      else
      {
        WebForm.wp(w, "<p>");
        WebForm.wp(w, "<div align=\"center\">");
        WebForm.wp(w, "<div id=\"glow\" >");
        WebForm.wp(w, "<p>Please Login, always specify full account name, e.g. user@domain.name<br>\n");
        Web.web_table_start(w);
        WebForm.wp(w, "<tr><td width=100px align=right>Email:<td width=180px>");
        Web.web_input_text(w, "u_user", "");
        WebForm.wp(w, "<tr><td  align=right>Password:<td>");
        Web.web_input_password(w, "u_pass", "");
        WebForm.wp(w, "<td>");
        Web.web_cmd_button(w, "cmd_login_do", "Login", "Login");
        WebForm.wp(w, "</table>");
        WebForm.wp(w, "</div>");
        WebForm.wp(w, "<hr>");
        if (!Ini.istrue(En.noself_signup))
        {
          WebForm.wp(w, "or signup for an account now with one click\n");
          Web.web_table_start(w);
          WebForm.wp(w, "<tr><td  width=100px align=right>EMail:<td width=180px >");
          Web.web_input_text(w, "s_user", "");
          WebForm.wp(w, "<tr><td  align=right>Password:<td>");
          Web.web_input_password(w, "s_pass", "");
          WebForm.wp(w, "<td>");
          Web.web_cmd_button(w, "cmd_signup_do", "Signup", "Signup");
          WebForm.wp(w, "</table>");
        }
        Web.web_table_start(w);
        WebForm.wp(w, "<tr><td width=100px><td width=180px align=right>or<td> ");
        Web.web_cmd_button(w, "cmd_reset_password", "reset password", "reset password");
        WebForm.wp(w, "</table>");
      }
    }

    private static void web_cmd_signup_do(Websvc w)
    {
      WebForm.imsg("cmd_signup_do");
      string user1 = WebForm.form_get(w, "s_user");
      string passwd = WebForm.form_get(w, "s_pass");
      string user2 = Vuser.add_domain(user1);
      if (Vuser.lookup(user2) != null)
      {
        WebForm.wp_err(w, "Sorry that account already exists<br>\n");
        WebForm.web_cmd_login(w);
      }
      else
      {
        WebForm.imsg("cmd_signup_do1");
        NameValueCollection info = new NameValueCollection();
        info.Add("status", "pending");
        Random random = new Random();
        WebForm.imsg("cmd_signup_do2");
        string token = random.Next(10000000).ToString();
        info.Add("activate", token);
        WebForm.imsg("cmd_signup_do3");
        string reason;
        if (!Vuser.add(user2, passwd, info, out reason, ""))
          WebForm.wp_err(w, "Userdb error saving {0} user to database {1}", (object) user2, (object) reason);
        else
          WebForm.wp_ok(w, "Account created, you must now verify your account by reading the email we sent you and clicking on the link");
        WebForm.imsg("cmd_signup_do4");
        WebForm.send_token(w, user2, token);
        WebForm.imsg("cmd_signup_do5");
        WebForm.web_cmd_login(w);
      }
    }

    private static void send_token(Websvc w, string user, string token)
    {
      Smtp smtp = new Smtp();
      WebForm.imsg("send_token: step1");
      smtp.set_server(Ini.getstring(En.mail_server), Ini.getstring(En.mail_user), Ini.getstring(En.mail_pass), "25", "");
      WebForm.imsg("send_token: step2");
      string from = Ini.getstring(En.mail_from);
      if (from.Length == 0)
        from = "postmaster@" + Ini.getstring(En.mail_server);
      string str = Ini.url_browse() + string.Format("cgi/admin.cgi?cmd=activate&token={0}&user={1}", (object) token, (object) user);
      WebForm.imsg("send_token: step3");
      string body = string.Format("Please use this url to activate your FtpDav account\r\n    " + str + "\r\n");
      WebForm.imsg("send_token: step4");
      string message = smtp.build_body(from, user, "FtpDav Activation", body);
      WebForm.imsg("send_token: step5");
      string result;
      if (!smtp.send_message(from, user, message, out result))
        WebForm.wp_err(w, "Activation email not sent {0}", (object) result);
      else
        WebForm.wp_ok(w, "Activation email sent {0}", (object) result);
    }

    private static void send_password(Websvc w, string user, string password)
    {
      Smtp smtp = new Smtp();
      WebForm.imsg("send_token: step1");
      smtp.set_server(Ini.getstring(En.mail_server), Ini.getstring(En.mail_user), Ini.getstring(En.mail_pass), "25", "");
      WebForm.imsg("send_token: step2");
      string from = Ini.getstring(En.mail_from);
      if (from.Length == 0)
        from = "postmaster@" + Ini.getstring(En.mail_server);
      string body = string.Format("Your password has been reset to: \r\n    " + password + "\r\n");
      string message = smtp.build_body(from, user, "FtpDav Password Reset", body);
      string result;
      if (!smtp.send_message(from, user, message, out result))
        WebForm.wp_err(w, "Reset email not sent {0}", (object) result);
      else
        WebForm.wp_ok(w, "Reset email sent {0}", (object) result);
    }

    private static void web_cmd_activate(Websvc w)
    {
      string str = WebForm.form_get(w, "token");
      string user1 = WebForm.form_get(w, "user");
      User user2 = Vuser.lookup(user1);
      if (user2.get_safe("activate") != str)
      {
        Thread.Sleep(5000);
        WebForm.wp_err(w, "Sorry incorrect token");
      }
      else
      {
        user2.info.Set("status", "activated");
        user2.info.Remove("activate");
        if (!Vuser.add(user1, "", user2.info, out string _, ""))
          WebForm.wp_err(w, "Failed to modify account sorry {0}", (object) user1);
        else
          WebForm.wp_ok(w, "Activated, you can now login");
      }
      WebForm.web_cmd_login(w);
    }

    private delegate void del_cmd(Websvc w);

    private enum wflag
    {
      W_NORM,
      W_ADMIN,
      W_LOGIN,
    }

    private struct CMDS
    {
      public string cmd;
      public WebForm.del_cmd myfn;
      public WebForm.wflag myflags;

      public CMDS(string a, WebForm.del_cmd b, WebForm.wflag c)
      {
        this.cmd = a;
        this.myfn = b;
        this.myflags = c;
      }
    }
  }
}
