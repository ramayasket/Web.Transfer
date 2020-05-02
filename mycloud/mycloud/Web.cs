// Decompiled with JetBrains decompiler
// Type: mycloud.Web
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mycloud
{
  internal class Web
  {
    public static void imsg(string format, params object[] args)
    {
      clib.webmsg("{0}", (object) string.Format(format, args));
    }

    public static bool wp(Websvc w, string format, params object[] args)
    {
      string x = string.Format(format, args);
      return w.body_write(x);
    }

    public static bool wh(Websvc w, string format, params object[] args)
    {
      string x = string.Format(format, args);
      return w.write(x);
    }

    public static string cookie_date(int x)
    {
      // N.B.! not used, but present in the original assembly
      // DateTime dateTime = new DateTime();
      DateTime now = DateTime.Now;

      now.AddSeconds((double) x);
      now.ToUniversalTime();
      return string.Format("{0:ddd, dd-MMM-yyyy HH:mm:ss} GMT", (object) now);
    }

    public static void cookie_set(Websvc w, string var, string val)
    {
      Web.wh(w, "Set-Cookie: {0}={1};\r\n", (object) var, (object) val);
    }

    public static void send_file(Websvc w, string fname)
    {
      string str = File.ReadAllText(clib.app("web/" + fname));
      Web.wp(w, "{0}", (object) str);
    }

    public static string cookie_get(Websvc w, string name)
    {
      if (w.cookie == null || w.cookie == "")
        return "";
      string[] strArray = w.cookie.Split("; =".ToArray<char>());
      for (int index = 0; index < ((IEnumerable<string>) strArray).Count<string>(); ++index)
      {
        if (strArray[index] == name)
        {
          int num;
          return strArray[num = index + 1];
        }
      }
      return "";
    }

    public static void top(Websvc w, string product, string version, string cuser)
    {
      Web.wp(w, "<div id=\"banner\">\n");
      Web.wp(w, "<center>\n");
      Web.wp(w, "<div>");
      Web.wp(w, "<span class=\"topbutton\"> {0} {1}</span>", (object) clib.Product_Name(), (object) version);
      Web.wp(w, "</div>");
      Web.wp(w, "</center>\n");
      Web.wp(w, "</div>\n");
    }

    public static void start(Websvc w)
    {
      Web.any_header(w, "text/html", "200 Ok");
      Web.wp(w, "<HTML>\n");
      Web.wp(w, "<HEAD>\n");
      Web.wp(w, "<LINK REL=\"shortcut icon\"  HREF=\"/img/favicon.ico\">");
      Web.wp(w, "<LINK REL=\"stylesheet\" TYPE=\"text/css\" HREF=\"/web/free.css\">");
      Web.wp(w, "<LINK REL=\"stylesheet\" TYPE=\"text/css\" HREF=\"/web/all.css\">");
      if (!w.isie)
        Web.wp(w, "<LINK REL=\"stylesheet\" TYPE=\"text/css\" HREF=\"/web/buttons.css\">");
      else
        Web.wp(w, "<LINK REL=\"stylesheet\" TYPE=\"text/css\" HREF=\"/web/buttons_ie.css\">");
      Web.wp(w, "<LINK REL=\"stylesheet\" TYPE=\"text/css\" HREF=\"/web/files.css\">");
      Web.wp(w, "<script language=\"javascript\" type=\"text/javascript\" src=\"/web/all.js\"></script>");
    }

    public static void simple_error(Websvc w, string msg)
    {
      Web.any_header(w, "text/plain", "200 Ok");
      Web.wp(w, "{0}\n", (object) msg);
      w.body_send();
      w.chan.EndConnection();
    }

    public static void body(Websvc w)
    {
      Web.wp(w, "</HEAD>\n");
      Web.wp(w, "<BODY onload=\"do_init();\" >\n");
      Web.wp(w, "<DIV ID=\"infodiv\" STYLE=\"position:absolute; visibility:hidden; z-index:20; top:0px; left:0px;\"></DIV>\n");
    }

    public static void page_body(Websvc w)
    {
      Web.wp(w, "<div id=\"centercontent\">\n");
    }

    public static void page_end(Websvc w)
    {
      Web.wp(w, "<hr>\n");
      if (w.ses != null)
      {
        Web.wp(w, "<p>WEBDAV:\n");
        string str1 = clib.make_url("http", Ini.host(), Ini.getint(En.webdav_port), "/");
        Web.wp(w, "<a href=\"{0}\">{0}</a>", (object) str1);
        Web.wp(w, " or ");
        string str2 = clib.make_url("https", Ini.host(), Ini.getint(En.webdav_port_ssl), "/");
        Web.wp(w, "<a href=\"{0}\">{0}</a>", (object) str2);
        Web.wp(w, "<BR>FTP:\n");
        string str3 = string.Format("ftp://{0}:{1}", (object) Ini.host(), (object) Ini.getint(En.ftp_port));
        Web.wp(w, "<a href=\"{0}\">{0}</a>", (object) str3);
        Web.wp(w, "<BR> Web Browser: ");
        string str4 = Ini.url_browse();
        Web.wp(w, "<a href=\"{0}\">{0}</a>", (object) str4);
        Web.wp(w, " or ");
        string str5 = clib.make_url("https", Ini.host(), Ini.getint(En.web_port_ssl), "/");
        Web.wp(w, "<a href=\"{0}\">{0}</a>", (object) str5);
        Web.wp(w, "<BR> <a href=\"http://netwinsite.com/ftpdav/guide.htm\">Guide to free WebDav/FTP Clients</a>");
      }
      Web.wp(w, "</div>");
    }

    public static void end(Websvc w)
    {
      Web.wp(w, "</BODY></HTML>\n");
      w.body_send();
    }

    public static void web_end_form(Websvc w)
    {
      Web.wp(w, "</form>\n");
    }

    public static void web_select_text(Websvc w, string var, string list, string cur)
    {
      Web.wp(w, "<select name=\"{0}\">", (object) var);
      foreach (string str in list.Split(",".ToCharArray()))
        Web.wp(w, "<option {0} value=\"{1}\">{2}</option>", cur == str ? (object) "Selected" : (object) "", (object) str, (object) str);
      Web.wp(w, "</select>\n");
    }

    public static void web_cmd_buttoni(
      Websvc w,
      string name,
      string value,
      string help,
      string extra)
    {
      Web.wp(w, "<input type=\"button\" name=\"{0}\" class=\"btnnormal\" {3} value=\"{1}\" title=\"{2}\">\n", (object) name, (object) value, (object) help, (object) extra);
    }

    public static void web_cmd_button(Websvc w, string name, string value, string help)
    {
      Web.web_cmd_button(w, name, value, help, "");
    }

    public static void web_cmd_button_trtd(Websvc w, string name, string value, string help)
    {
      Web.wp(w, "<tr><td>");
      Web.web_cmd_buttonw(w, name, value, help);
    }

    public static void web_cmd_buttonw(Websvc w, string name, string value, string help)
    {
      bool isdown = false;
      if (WebForm.form_get(w, name).Length > 0)
        isdown = true;
      Web.web_cmd_buttonw(w, name, value, help, "", isdown);
    }

    public static void web_cmd_button(
      Websvc w,
      string name,
      string value,
      string help,
      string extra)
    {
      Web.wp(w, "<input type=\"submit\" name=\"{0}\" class=\"btnnormal\" {3} value=\"{1}\" title=\"{2}\">\n", (object) name, (object) value, (object) help, (object) extra);
    }

    public static void web_cmd_button_small(Websvc w, string name, string value, string help)
    {
      string str = "";
      Web.wp(w, "<input type=\"submit\" name=\"{0}\" class=\"btnsmall\" {3} value=\"{1}\" title=\"{2}\">\n", (object) name, (object) value, (object) help, (object) str);
    }

    public static void web_cmd_buttonw(
      Websvc w,
      string name,
      string value,
      string help,
      string extra)
    {
      Web.web_cmd_buttonw(w, name, value, help, extra, false);
    }

    public static void web_cmd_buttonw(
      Websvc w,
      string name,
      string value,
      string help,
      string extra,
      bool isdown)
    {
      Web.wp(w, "<input type=\"submit\" name=\"{0}\" class=\"btnmenu{4}\" {3} value=\"{1}\" title=\"{2}\">\n", (object) name, (object) value, (object) help, (object) extra, isdown ? (object) "_down" : (object) "");
    }

    public static void web_href_button(Websvc w, string text, string href)
    {
      Web.wp(w, "<input type=\"button\"  class=\"btnmenu\" value=\"{0}\" title=\"{1}\" onClick=\"window.open('{2}')\">\n", (object) text, (object) text, (object) href);
    }

    public static void show_all_menu(Websvc w, bool isadmin)
    {
      Web.wp(w, "<p>");
      Web.wp(w, "<table class=\"menutable\">");
      Web.web_cmd_button_trtd(w, "cmd_browse", "Files", "Browse Files");
      Web.web_cmd_button_trtd(w, "cmd_options", "Preferences", "Preferences");
      Web.web_cmd_button_trtd(w, "cmd_logout", "Logout", "Logout");
      Web.wp(w, "</table>");
      if (isadmin)
      {
        Web.wp(w, "<hr>");
        Web.wp(w, "<p>Admin Config");
        Web.wp(w, "<table class=\"menutable\">");
        Web.web_cmd_button_trtd(w, "cmd_users", "Users", "Manage local users");
        Web.web_cmd_button_trtd(w, "cmd_profile", "Profiles", "Manager profiles (path mappings)");
        Web.web_cmd_button_trtd(w, "cmd_config", "Config", "Change configuration settings");
        if (clib.canBuy())
          Web.web_cmd_button_trtd(w, "cmd_register", "Buy/Activate", "Load license key");
        Web.wp(w, "</table>");
        Web.wp(w, "<hr>");
        Web.wp(w, "<p>Admin Manage");
        Web.wp(w, "<table class=\"menutable\">");
        Web.web_cmd_button_trtd(w, "cmd_log", "Log", "Show main log file");
        Web.web_cmd_button_trtd(w, "cmd_status", "Status", "Server Status");
        Web.wp(w, "<tr><td>");
        Web.web_href_button(w, "Help", "http://netwinsite.com/ftpdav/guide.htm");
        Web.wp(w, "</table>");
      }
      Web.wp(w, "<hr>");
      Web.wp(w, "</div>\n");
      Web.web_end_form(w);
    }

    public static void web_table_start(Websvc w)
    {
      Web.wp(w, "<table class=\"invisible\">");
    }

    public static void web_checkbox(Websvc w, string name, string value, string text, bool check)
    {
      Web.wp(w, "<input {2} type=\"checkbox\" name=\"{0}\" value=\"{3}\"/>{1} ", (object) name, (object) text, check ? (object) "checked" : (object) "", (object) value);
    }

    public static void web_checkbox_i(
      Websvc w,
      string name,
      string value,
      string text,
      bool check,
      int i)
    {
      Web.wp(w, "<input {2} type=\"checkbox\" name=\"{0}_{4}\" value=\"{3}\"/>{1} ", (object) name, (object) text, check ? (object) "checked" : (object) "", (object) value, (object) i);
    }

    public static void web_radio_i(
      Websvc w,
      string name,
      string value,
      string text,
      bool check,
      int i)
    {
      Web.wp(w, "<input {0} type=\"radio\" name=\"{1}_{2}\" value=\"{3}\"/>{4} ", check ? (object) "checked" : (object) "", (object) name, (object) i, (object) value, (object) text);
    }

    public static void web_input_text(Websvc w, string name, string value)
    {
      Web.wp(w, "<input type=\"text\" name=\"{0}\" \tvalue=\"{1}\">\n", (object) name, (object) value);
    }

    public static void web_input_text_size(Websvc w, string name, string value, int sz)
    {
      Web.wp(w, "<input type=\"text\" name=\"{0}\" size=\"{2}\"\tvalue=\"{1}\">\n", (object) name, (object) value, (object) sz);
    }

    public static void web_input_file(Websvc w, string name, string value)
    {
      Web.wp(w, "<input  type=\"file\" name=\"{0}\" \tvalue=\"{1}\">\n", (object) name, (object) value);
    }

    public static void web_input_password(Websvc w, string name, string value)
    {
      Web.wp(w, "<input type=\"password\" name=\"{0}\" \tvalue=\"{1}\">\n", (object) name, (object) value);
    }

    public static void web_input_text_i(Websvc w, string name, string value, int i)
    {
      Web.wp(w, "<input type=\"text\" name=\"{0}_{2}\" \tvalue=\"{1}\">\n", (object) name, (object) value, (object) i);
    }

    public static void web_input_hidden(Websvc w, string name, string value)
    {
      Web.wp(w, "<input type=\"hidden\" name=\"{0}\" \tvalue=\"{1}\">\n", (object) name, (object) value);
    }

    public static void web_start_form(Websvc w, string id)
    {
      Web.wp(w, "<font face=\"Verdana, Arial, Helvetica, sans-serif\" size=\"2\">\n");
      Web.wp(w, "<form ENCTYPE=\"multipart/form-data\" method=\"post\" name=\"form\" id='{0}' action=\"/cgi/admin.cgi\"><p>\n", (object) id);
    }

    public static void show_menu(Websvc w, bool isadmin)
    {
      Web.web_start_form(w, "aafrm1");
      Web.wp(w, "<div id=\"leftcontent\">\n");
      Web.show_all_menu(w, isadmin);
      Web.wp(w, "</div>\n");
    }

    public static void need_auth(Websvc w)
    {
      Random random = new Random();
      byte[] numArray = new byte[10];
      random.NextBytes(numArray);
      Web.any_header(w, "text/plain", "401 Authorization required");
      Web.wh(w, "WWW-Authenticate: Basic realm=\"{0}\"\r\n", (object) MyMain.realm());
      Web.wh(w, "WWW-Authenticate: Digest");
      Web.wh(w, " realm=\"{0}\",", (object) MyMain.realm());
      Web.wh(w, " qop=\"auth\",");
      Web.wh(w, " nonce=\"{0}\",", (object) clib.byte_to_hex(numArray, ((IEnumerable<byte>) numArray).Count<byte>()));
      Web.wh(w, " opaque=\"placeholder\"\r\n");
      Web.wp(w, "Authorization required");
      w.body_send();
      if (!Ini.istrue(En.debug_http))
        return;
      clib.imsg("http: requesting authentication");
    }

    public static void any_header(Websvc w, string content, string reason)
    {
      Web.any_header(w, content, reason, -1, false);
    }

    public static void any_header(
      Websvc w,
      string content,
      string reason,
      int content_len,
      bool ischunked)
    {
      w.out_chunked = false;
      w.need_length = false;
      if (Ini.istrue(En.debug_http))
        clib.imsg("http: response: {0}", (object) reason);
      Web.imsg("http: response: {0}", (object) reason);
      Web.wh(w, string.Format("HTTP/1.{0} {1}\r\n", (object) w.httpversion, (object) reason));
      Web.wh(w, "Server: DManager\r\n");
      Web.wh(w, "MIME-version: 1.0\r\n");
      if (w.isdav)
      {
        Web.wh(w, "DAV: 1,2\r\n");
        Web.wh(w, "Allow: GET, PUT, DELETE, MKCOL, OPTIONS, COPY, MOVE, PROPFIND, PROPPATCH, LOCK, UNLOCK\r\n");
        Web.wh(w, "Connection: keep-alive\r\n");
        Web.wh(w, "Pragma: no-cache\r\n");
        Web.wh(w, "Cache-Control: no-store, no-cache, must-revalidate, post-check=0, pre-check=0\r\n");
      }
      if (content.Contains("text/html"))
        content += "; charset=utf-8";
      Web.wh(w, "Content-Type: {0}\r\n", (object) content);
      w.need_length = true;
      w.need_blank = true;
      if (content_len >= 0)
      {
        Web.wh(w, "Content-Length: {0}\r\n", (object) content_len);
        w.need_length = false;
      }
      else if (w.httpversion == 1 && ischunked)
      {
        Web.wh(w, "Transfer-Encoding: chunked\r\n");
        w.out_chunked = true;
      }
    }
  }
}
