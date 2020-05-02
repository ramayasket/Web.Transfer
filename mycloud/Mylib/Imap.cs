// Decompiled with JetBrains decompiler
// Type: Mylib.Imap
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mylib
{
  public class Imap : Link
  {
    private static List<Imap> all = new List<Imap>();
    private int wasexists = 0;
    private int handle = 0;
    public int nnew = 0;
    public string cur_folder = "none.....";
    private int id = 0;
    private bool first_time = true;
    private List<string> lsub_list = new List<string>();
    private List<Header> headers = new List<Header>();
    public int exists = 0;
    private List<int> search_uids = new List<int>();
    public int recent = 0;
    public int uidnext = 0;
    private Imap.r run_state = Imap.r.R_START;
    public static bool do_exit;
    private static int global_handle;
    private bool flags_seen;
    private bool flags_deleted;
    private bool flags_forwarded;
    private bool flags_answered;
    public string host;
    public string user;
    public string logname;
    private static string last_error;
    private int last_idle;
    private int last_used;
    private Imap.cb_log cblog;
    private Imap.cb_fetch cbfetch;
    private int nfetched;
    private Imap.cb_del cb;

    public void set_log(Imap.cb_log x)
    {
      this.cblog = x;
    }

    public static void all_store(Imap im)
    {
      lock (Imap.all)
        Imap.all.Add(im);
    }

    public static void all_check()
    {
      lock (Imap.all)
      {
        foreach (Imap imap in Imap.all)
        {
          if (!imap.netisopen())
          {
            Imap.all.Remove(imap);
            break;
          }
        }
      }
    }

    public static int all_count(string host, string user)
    {
      int num = 0;
      lock (Imap.all)
      {
        foreach (Imap imap in Imap.all)
        {
          if (imap.netisopen() && (imap.host == host && imap.user == user))
            ++num;
        }
      }
      return num;
    }

    public static List<string> all_info()
    {
      List<string> stringList = new List<string>();
      lock (Imap.all)
      {
        foreach (Imap imap in Imap.all)
          stringList.Add(string.Format("imap_active: {0} {1} {2} {3} {4} ", (object) imap.user, (object) imap.logname, (object) imap.age(), (object) imap.cur_folder, (object) imap.netisopen()));
      }
      return stringList;
    }

    public static bool imap_ok(out string status)
    {
      status = "";
      if (Imap.last_error == null)
        return true;
      status = Imap.last_error;
      return false;
    }

    public int age()
    {
      return stat.time() - this.last_used;
    }

    public bool login(
      string dest,
      int port,
      string sslmode,
      string xuser,
      string pass,
      out string result)
    {
      bool flag1 = false;
      if (port == 0)
        port = 143;
      this.host = dest;
      this.user = xuser;
      this.last_used = stat.time();
      this.handle = Imap.global_handle++;
      if (this.cblog != null)
        this.cblog(string.Format("info: Open host {0} port {1} {2}", (object) dest, (object) port, (object) sslmode));
      string reason;
      if (!this.open(dest, port, out reason))
      {
        result = string.Format("Failed to open {0} {1} {2}", (object) dest, (object) port, (object) reason);
        if (this.cblog != null)
          this.cblog(string.Format("info: {0}", (object) result));
        Imap.last_error = "Open " + dest + " failed " + result;
        return false;
      }
      Imap.all_store(this);
      if (sslmode == "Implicit SSL")
      {
        if (this.ssl_enable(dest, false))
        {
          stat.imsg("ssl: implicit ssl started ok");
          flag1 = true;
        }
        else
          stat.imsg("ssl: implicit ssl could not start");
      }
      string emsg;
      if (!this.readline(out result, "Login", 300000, out bool _, out emsg))
      {
        this.netclose();
        Imap.last_error = "Open/readline " + dest + " failed " + result + " " + emsg;
        if (this.cblog != null)
          this.cblog(string.Format("failed: {0}", (object) Imap.last_error));
        return false;
      }
      this.first_time = true;
      this.cur_folder = "";
      if (sslmode == "Start SSL")
      {
        this.netprintf("starttls\r\n");
        bool flag2 = this.response_wait(out result);
        if (flag2)
          flag2 = this.ssl_enable(dest, false);
        if (flag2)
        {
          stat.imsg("ssl: SSL successfully enabled");
          flag1 = true;
        }
        else
          stat.imsg("ssl: SSL: Unable to startssl for this connection");
        if (flag2 && this.cblog != null)
          this.cblog(string.Format("info: SSL started successfully"));
      }
      this.netprintf("login {0} {1}\r\n", (object) this.user, (object) pass);
      bool flag3 = this.response_wait(out result);
      result = stat.trim_eol(result);
      if (flag1)
        result += " +SSL";
      if (!flag3)
        stat.emsg("imap: Login failed {0}", (object) result);
      Imap.last_error = flag3 ? (string) null : "Open " + dest + " failed " + result;
      if (!flag3)
      {
        if (this.cblog != null)
          this.cblog(string.Format("failed: {0}", (object) Imap.last_error));
      }
      else if (this.cblog != null)
        this.cblog(string.Format("worked: {0}", (object) result));
      return flag3;
    }

    public void reset()
    {
      this.run_state = Imap.r.R_START;
      this.cur_folder = "";
    }

    public bool select(string folder)
    {
      return this.select(folder, 0);
    }

    public bool select(string folder, int uid)
    {
      if (!this.netisopen())
        return false;
      if (!(this.cur_folder == folder) || uid >= this.uidnext)
        return this.select_real(folder);
      stat.imsg("IMAP: Auto selected folder {0}", (object) folder);
      return true;
    }

    public bool select_real(string folder)
    {
      if (!this.netisopen())
        return false;
      this.last_used = stat.time();
      stat.imsg("imap: Select \"{0}\"", (object) folder);
      if (!this.netprintf("select \"{0}\"\r\n", (object) folder))
        return false;
      string result;
      bool flag = this.response(out result, false, 30000);
      stat.imsg("imap: select response: {0}", (object) result);
      if (flag)
        this.cur_folder = folder;
      return flag;
    }

    public bool rename(string folder, string newname)
    {
      if (!this.netprintf("rename \"{0}\" \"{1}\"\r\n", (object) folder, (object) newname))
        return false;
      string result;
      bool flag = this.response(out result, false, 30000);
      stat.imsg("imap: rename response: {0}", (object) result);
      return flag;
    }

    public List<string> lsub(string folder)
    {
      this.lsub_list.Clear();
      this.netprintf("lsub \"\" \"{0}\"\r\n", (object) folder);
      this.response(out string _, false, 30000);
      return this.lsub_list;
    }

    public List<string> list(string folder)
    {
      this.lsub_list.Clear();
      this.netprintf("list \"\" \"{0}\"\r\n", (object) folder);
      this.response(out string _, false, 30000);
      return this.lsub_list;
    }

    public List<int> search(string request)
    {
      int[] numArray = new int[10];
      this.netprintf("uid search {0}\r\n", (object) request);
      this.response_wait(out string _);
      return this.search_uids;
    }

    public void imsg(string format, params object[] args)
    {
      stat.imsg("{0} {1}", (object) this.logname, (object) string.Format(format, args));
    }

    public bool fetch_unseen(out List<int> bob)
    {
      bob = new List<int>();
      this.netprintf("uid search 0:* unseen\r\n");
      string result;
      if (!this.response_wait(out result))
      {
        this.imsg("Search failed {0}", (object) result);
        return false;
      }
      this.imsg("Search worked I think {0} {1}", (object) this.search_uids.Count<int>(), (object) result);
      bob = this.search_uids;
      return true;
    }

    public bool fetch_recent(out List<int> bob)
    {
      // N.B.! probably this empty 'if' statement is residual from debugging.
      // if (this.uidnext - 100 < 0)
      // {
      // }

      bob = new List<int>();
      this.netprintf("uid search {0}:*\r\n", (object) this.uidnext);
      string result;
      if (!this.response_wait(out result))
      {
        this.imsg("Search failed {0}", (object) result);
        return false;
      }
      this.imsg("Search worked I think {0} {1}", (object) this.search_uids.Count<int>(), (object) result);
      bob = this.search_uids;
      return true;
    }

    public List<Header> fetch_headers(int from, string request)
    {
      return this.fetch_headers(from, request, false);
    }

    public List<Header> fetch_headers(int from, string request, bool notuid)
    {
      this.headers.Clear();
      if (from == 0)
        from = 1;
      if (notuid)
        this.netprintf("fetch {0}:* (UID RFC822.SIZE FLAGS body.peek[header.fields ({1})])\r\n", (object) from, (object) request);
      else
        this.netprintf("uid fetch {0}:* (UID RFC822.SIZE FLAGS body.peek[header.fields ({1})])\r\n", (object) from, (object) request);
      return !this.response_wait(out string _) ? (List<Header>) null : this.list_clone(this.headers);
    }

    public List<Header> fetch_list(string listuids, string request)
    {
      this.headers.Clear();
      this.netprintf("uid fetch {0} ({1})\r\n", (object) listuids, (object) request);
      this.response_wait(out string _);
      return this.list_clone(this.headers);
    }

    public List<Header> fetch_headers(List<int> uids, string request)
    {
      this.headers.Clear();
      StringBuilder stringBuilder = new StringBuilder();
      stat.imsg("fetch_headers uidlist count {0}", (object) uids.Count<int>());
      foreach (int uid in uids)
      {
        if (stringBuilder.Length != 0)
          stringBuilder.AppendFormat(",");
        stringBuilder.AppendFormat("{0}", (object) uid);
      }
      stat.imsg("SENDING---> uid fetch {0} (UID RFC822.SIZE FLAGS body.peek[header.fields ({1})])\r\n", (object) stringBuilder, (object) request);
      this.netprintf("uid fetch {0} (UID RFC822.SIZE FLAGS body.peek[header.fields ({1})])\r\n", (object) stringBuilder, (object) request);
      this.response_wait(out string _);
      return this.list_clone(this.headers);
    }

    public List<Header> list_clone(List<Header> all)
    {
      List<Header> headerList = new List<Header>();
      foreach (Header header in all)
        headerList.Add(header);
      return headerList;
    }

    public List<Header> fetch_heads(int from)
    {
      this.headers.Clear();
      this.netprintf("uid fetch {0}:* body.peek[header]\r\n", (object) from);
      this.response_wait(out string _);
      return this.list_clone(this.headers);
    }

    public Header getmsg(string folder, int uid, int maxsize, out bool delete)
    {
      this.imsg("IMAP: getmsg Uid called {0}", (object) uid);
      delete = false;
      if (!this.netisopen() || (this.cur_folder != folder || uid >= this.uidnext) && !this.select_real(folder))
        return (Header) null;
      this.headers.Clear();
      if (maxsize == 0)
        this.netprintf("uid fetch {0} body.peek[]\r\n", (object) uid, (object) maxsize);
      else
        this.netprintf("uid fetch {0} body.peek[]<0.{1}>\r\n", (object) uid, (object) maxsize);
      this.response_wait(out string _);
      stat.imsg("Headers count {0}", (object) this.headers.Count<Header>());
      if (this.headers.Count != 0)
        return this.get_new_message();
      stat.imsg("Tried to get message but failed BADLY");
      delete = true;
      return (Header) null;
    }

    public Header get_new_message()
    {
      if (this.headers.Count == 0)
        return (Header) null;
      Header header = this.headers[0];
      this.headers.RemoveAt(0);
      return header;
    }

    public new bool netprintf(string format, params object[] args)
    {
      string str1 = string.Format(format, args);
      string str2 = str1;
      this.last_used = stat.time();
      ++this.id;
      this.imsg("IMAP{0}:--> {1} {2} ", (object) this.handle, (object) this.id, (object) str2);
      if (this.cblog != null)
        this.cblog(string.Format("--> {0}", (object) str2));
      if (this.send(string.Format("{0} {1}", (object) this.id, (object) str1)))
        return true;
      this.imsg("IMAP:--> Send FAILED");
      this.netclose();
      return false;
    }

    public bool response_star(string[] words, string line)
    {
      int num1 = stat.atoi(words[1]);
      if (words[2] == "EXISTS")
      {
        if (this.cb != null)
          this.cb("exists", num1, this.flags_seen, this.flags_deleted, this.flags_answered, this.flags_forwarded);
        this.exists = stat.atoi(words[1]);
        if (this.first_time)
        {
          stat.imsg("idle: exists set first time {0} {1}", (object) this.wasexists, (object) this.exists);
          this.wasexists = this.exists;
          this.first_time = false;
        }
        if (this.exists > this.wasexists)
        {
          this.nnew += this.exists - this.wasexists;
          this.wasexists = this.exists;
          stat.imsg("idle: nnew is now {0}", (object) this.nnew);
        }
        this.imsg("idle: Exists found: exists {0} wasexists {1} nnew {2}", (object) this.exists, (object) this.wasexists, (object) this.nnew);
      }
      if (words[2] == "EXPUNGE")
      {
        if (this.cb != null)
          this.cb("expunge", num1, this.flags_seen, this.flags_deleted, this.flags_answered, this.flags_forwarded);
        --this.wasexists;
        this.imsg("idle: Expunge found: exists {0} wasexists {1} nnew {2}", (object) this.exists, (object) this.wasexists, (object) this.nnew);
      }
      if (words[2] == "RECENT")
        this.recent = stat.atoi(words[1]);
      for (int index = 0; index < ((IEnumerable<string>) words).Count<string>() - 1; ++index)
      {
        if (words[index] == "UIDNEXT")
        {
          this.uidnext = stat.atoi(words[index + 1]);
          this.imsg("imap: uidnext is now {0}", (object) this.uidnext);
        }
      }
      if (words[1] == "LSUB" || words[1] == "LIST")
      {
        string[] strArray = stat.split_quoted(line);
        if (((IEnumerable<string>) strArray).Count<string>() >= 4)
          this.lsub_list.Add(stat.trim_quotes(strArray[4]));
      }
      if (words[1] == "SEARCH")
      {
        this.imsg("Found 'search' keyword");
        this.search_uids.Clear();
        for (int index = 2; index < ((IEnumerable<string>) words).Count<string>(); ++index)
        {
          this.imsg("Adding [{0}]={1}", (object) index, (object) words[index]);
          int num2 = stat.atoi(words[index]);
          if (num2 > 0)
            this.search_uids.Add(num2);
        }
      }
      if (words[2] == "FETCH")
      {
        int xuid = 0;
        int xsize = 0;
        if (this.cbfetch != null)
          this.cbfetch(string.Format("Got {0}", (object) ++this.nfetched));
        for (int index = 0; index < ((IEnumerable<string>) words).Count<string>(); ++index)
        {
          if (words[index] == "UID")
          {
            xuid = stat.atoi(words[index + 1]);
            break;
          }
        }
        for (int index = 0; index < ((IEnumerable<string>) words).Count<string>(); ++index)
        {
          if (words[index] == "RFC822.SIZE")
          {
            xsize = stat.atoi(words[index + 1]);
            break;
          }
        }
        this.flags_deleted = this.flags_forwarded = this.flags_answered = this.flags_seen = false;
        for (int index1 = 0; index1 < ((IEnumerable<string>) words).Count<string>(); ++index1)
        {
          if (words[index1] == "FLAGS")
          {
            for (int index2 = index1 + 1; index2 < ((IEnumerable<string>) words).Count<string>(); ++index2)
            {
              string word = words[index2];
              if (word == "\\Seen")
                this.flags_seen = true;
              if (word == "\\Delted")
                this.flags_deleted = true;
              if (word == "\\Answered")
                this.flags_answered = true;
              if (word == "$Forwarded")
                this.flags_forwarded = true;
            }
            if (this.cb != null)
              this.cb("flags", num1, this.flags_seen, this.flags_deleted, this.flags_answered, this.flags_forwarded);
          }
        }
        for (int index = 0; index < ((IEnumerable<string>) words).Count<string>(); ++index)
        {
          if (words[index].StartsWith("{"))
          {
            this.imsg("IMAP: Found length ({0}) ({1})", (object) words[index], (object) words[index].Substring(1, words[index].Length - 2));
            int want = stat.atoi(words[index].Substring(1, words[index].Length - 2));
            string stuff;
            this.read_exact(out stuff, want);
            this.headers.Add(new Header(xuid, stuff, xsize, this.flags_seen, this.flags_answered, this.flags_forwarded, this.flags_deleted, num1));
            this.readline(out string _, "from fetch", 30000, out bool _, out string _);
            break;
          }
        }
      }
      return true;
    }

    public bool store(int uid, string flags)
    {
      this.netprintf("uid store {0} +FLAGS.SILENT ({1})\r\n", (object) uid, (object) flags);
      stat.imsg("uid store {0} +FLAGS.SILENT ({1})\r\n", (object) uid, (object) flags);
      this.response_wait(out string _);
      return true;
    }

    public bool move(int uid, string folder, out int newuid)
    {
      newuid = 0;
      this.netprintf("uid copy {0} \"{1}\"\r\n", (object) uid, (object) folder);
      string result;
      if (!this.response_wait(out result))
        return false;
      newuid = stat.atoi(this.response_string(result, "COPYUID", 2));
      this.store(uid, "\\Deleted");
      return true;
    }

    public bool copy(int uid, string folder, out int newuid)
    {
      newuid = 0;
      this.netprintf("uid copy {0} \"{1}\"\r\n", (object) uid, (object) folder);
      string result;
      if (!this.response_wait(out result))
        return false;
      newuid = stat.atoi(this.response_string(result, "COPYUID", 2));
      return true;
    }

    public bool delete(int uid)
    {
      return this.store(uid, "\\Deleted");
    }

    public bool create(string folder)
    {
      this.netprintf("create \"{0}\"\r\n", (object) folder);
      this.response_wait(out string _);
      return true;
    }

    public bool expunge()
    {
      this.netprintf("expunge\r\n");
      this.response_wait(out string _);
      return true;
    }

    public bool append(string msg, string folder, out string result, out int uid)
    {
      uid = 0;
      this.netprintf("APPEND \"{0}\" {{{1}}}\r\n", (object) folder, (object) msg.Length);
      if (!this.response_plus(out result, 30000))
      {
        this.imsg("IMAP: Expecting plus but got somethign else");
        return false;
      }
      this.imsg("IMAP: Sending message body {0} bytes", (object) msg.Length);
      this.send(msg);
      this.send("\r\n");
      this.imsg("IMAP: Now check for response...");
      if (!this.response_wait(out result))
        return false;
      uid = stat.atoi(this.response_string(result, "APPENDUID", 1));
      return true;
    }

    private string response_string(string result, string wanted, int offset)
    {
      char[] chArray = new char[5]
      {
        '\r',
        '\n',
        ' ',
        '[',
        ']'
      };
      string[] strArray = result.Split(chArray);
      for (int index = 0; index < ((IEnumerable<string>) strArray).Count<string>(); ++index)
      {
        if (strArray[index] == wanted)
          return strArray[index + 1 + offset];
      }
      return "";
    }

    public bool response_plus(out string result, int timeout)
    {
      return this.readline(out result, "Response", timeout, out bool _, out string _) && result.StartsWith("+");
    }

    public bool response_wait(out string result)
    {
      return this.response(out result, false, 30000);
    }

    public bool response(out string result, bool oneline, int mswait)
    {
      char[] chArray = new char[7]
      {
        '\r',
        '\n',
        ' ',
        '(',
        ')',
        '[',
        ']'
      };
      string stuff = "";
      result = "";
      int num = 0;
      if (mswait > 0 && mswait < 100)
        throw new ArgumentException("Parameter is milliseconds not seconds", "original");
      try
      {
        bool istimeout;
        string[] words;
        do
        {
          if (!Imap.do_exit)
          {
            if (this.readline(out stuff, "Response", mswait, out istimeout, out string _))
            {
              if (this.cblog != null)
                this.cblog(string.Format("<-- {0}", (object) stuff));
              ++num;
              this.imsg("IMAP{0}:<-- {1}", (object) this.handle, (object) stuff);
              words = stuff.Split(chArray);
              if (((IEnumerable<string>) words).Count<string>() > 0)
              {
                if (!(words[0] == ""))
                {
                  if (words[0] == "*")
                    this.response_star(words, stuff);
                  else
                    goto label_12;
                }
              }
            }
            else
              goto label_4;
          }
          else
            break;
        }
        while (!oneline);
        goto label_21;
label_4:
        this.imsg("readline returned FALSE - timeout or socket closed i guess {0} istimeout {1}", (object) mswait, (object) istimeout);
        if (!istimeout)
        {
          this.netclose();
          goto label_21;
        }
        else
          goto label_21;
label_12:
        if (words[0] == "+")
          return true;
        if (stat.atoi(words[0]) == this.id)
        {
          result = stuff;
          this.imsg("IMAP: response function finished");
          return words[1].ToUpper() == "OK";
        }
        this.imsg("IMAP: Bad value in response, expected {0} got {1}", (object) this.id, (object) words[0]);
label_21:
        this.imsg("IMAP: Read {0} lines in response function, oneline {1}", (object) num, (object) oneline);
        if (oneline)
          return true;
        this.imsg("Bad response {0}", (object) stuff);
        return false;
      }
      catch (FormatException)
      {
        this.imsg("IMAP: Bad number, not too serious");
        return false;
      }
    }

    public bool idle_setup(out string result)
    {
      stat.imsg("Set keepalive timer on this socket");
      this.select("Inbox");
      this.last_idle = stat.time();
      this.netprintf("idle\r\n");
      this.imsg("idle_setup: exists {0} wasexists {1}", (object) this.exists, (object) this.wasexists);
      if (!this.response_wait(out result))
        return false;
      this.imsg("IMAP: Idle setup completed");
      return true;
    }

    public void set_cb(Imap.cb_del dax)
    {
      this.cb = dax;
    }

    public void set_fetch_cb(Imap.cb_fetch dax)
    {
      this.nfetched = 0;
      this.cbfetch = dax;
    }

    public bool idle_tick()
    {
      if (this.canread())
      {
        string result;
        this.response(out result, true, 0);
        this.imsg("IMAP: idle_tick, read {0}", (object) result);
        this.imsg("IMAP: idle_tick, wasexists {0} ,exists {1}", (object) this.wasexists, (object) this.exists);
      }
      else
        this.idle_fix();
      return false;
    }

    public void idle_fix()
    {
      if (this.run_state != Imap.r.R_IDLE || stat.time() - this.last_idle <= 300 || !this.idle_exit())
        return;
      this.idle_setup(out string _);
    }

    public bool idle_exit()
    {
      if (this.run_state != Imap.r.R_IDLE)
      {
        this.imsg("IMAP: state was wrong, it was {0}", (object) this.run_state);
        return false;
      }
      this.send("done\r\n");
      string result;
      if (!this.response(out result, false, 30000))
      {
        this.imsg("IMAP: response wrong [{0}]", (object) result);
        this.netclose();
        return false;
      }
      this.imsg("Idle_exit complted {0}", (object) result);
      return true;
    }

    public bool run()
    {
      switch (this.run_state)
      {
        case Imap.r.R_START:
          this.imsg("IMAP: imap.run: idle_setup");
          this.idle_setup(out string _);
          this.imsg("IMAP: imap.run: idle_setup_DONE");
          this.run_state = Imap.r.R_IDLE;
          break;
        case Imap.r.R_IDLE:
          if (!this.netisopen())
            return false;
          this.idle_tick();
          break;
      }
      return true;
    }

    public delegate void cb_log(string info);

    public delegate void cb_fetch(string info);

    public delegate void cb_del(
      string mode,
      int b,
      bool seen,
      bool deleted,
      bool answered,
      bool forwarded);

    private enum r
    {
      R_START,
      R_IDLE,
      R_FETCH,
    }
  }
}
