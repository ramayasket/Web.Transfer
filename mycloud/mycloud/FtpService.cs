// Decompiled with JetBrains decompiler
// Type: mycloud.FtpService
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using Mylib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using TcpLib;

namespace mycloud
{
  public class FtpService : TcpServiceProvider
  {
    private bool doutf8 = true;
    private List<FtpService.CMDS> cmds = new List<FtpService.CMDS>();
    private MyBuffer bf = new MyBuffer();
    private string cwd = "/";
    private bool ssl_data;
    private bool ssl_con;
    public static int nrequests;
    public Files files;
    public ConnectionState chan;
    private string rename_from;
    private string user;
    private bool doexit;
    private long restart;
    private bool isloggedin;
    private bool isbin;
    private string port_ip;
    private int port_port;
    private TcpListener listen;

    public FtpService()
    {
      this.cmds.Add(new FtpService.CMDS("rein", new FtpService.del_cmd(this.cmd_rein), FtpService.wflag.W_LOGIN));
      this.cmds.Add(new FtpService.CMDS("exit", new FtpService.del_cmd(this.cmd_exit), FtpService.wflag.W_LOGIN));
      this.cmds.Add(new FtpService.CMDS("quit", new FtpService.del_cmd(this.cmd_exit), FtpService.wflag.W_LOGIN));
      this.cmds.Add(new FtpService.CMDS(nameof (user), new FtpService.del_cmd(this.cmd_user), FtpService.wflag.W_LOGIN));
      this.cmds.Add(new FtpService.CMDS("pass", new FtpService.del_cmd(this.cmd_pass), FtpService.wflag.W_LOGIN));
      this.cmds.Add(new FtpService.CMDS("auth", new FtpService.del_cmd(this.cmd_auth), FtpService.wflag.W_LOGIN));
      this.cmds.Add(new FtpService.CMDS("pbsz", new FtpService.del_cmd(this.cmd_pbsz), FtpService.wflag.W_LOGIN));
      this.cmds.Add(new FtpService.CMDS("prot", new FtpService.del_cmd(this.cmd_prot), FtpService.wflag.W_LOGIN));
      this.cmds.Add(new FtpService.CMDS("port", new FtpService.del_cmd(this.cmd_port), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("eprt", new FtpService.del_cmd(this.cmd_eprt), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("list", new FtpService.del_cmd(this.cmd_list), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("nlst", new FtpService.del_cmd(this.cmd_nlst), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("retr", new FtpService.del_cmd(this.cmd_retr), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("stor", new FtpService.del_cmd(this.cmd_stor), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS(nameof (cwd), new FtpService.del_cmd(this.cmd_cwd), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("cdup", new FtpService.del_cmd(this.cmd_cdup), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("pasv", new FtpService.del_cmd(this.cmd_pasv), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("epsv", new FtpService.del_cmd(this.cmd_epsv), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("type", new FtpService.del_cmd(this.cmd_type), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("stru", new FtpService.del_cmd(this.cmd_stru), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("syst", new FtpService.del_cmd(this.cmd_syst), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("mode", new FtpService.del_cmd(this.cmd_mode), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("appe", new FtpService.del_cmd(this.cmd_appe), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("rest", new FtpService.del_cmd(this.cmd_rest), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("rnfr", new FtpService.del_cmd(this.cmd_rnfr), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("rnto", new FtpService.del_cmd(this.cmd_rnto), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("dele", new FtpService.del_cmd(this.cmd_dele), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("rmd", new FtpService.del_cmd(this.cmd_rmd), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("xrmd", new FtpService.del_cmd(this.cmd_rmd), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("mkd", new FtpService.del_cmd(this.cmd_mkd), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("xmkd", new FtpService.del_cmd(this.cmd_xmkd), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("pwd", new FtpService.del_cmd(this.cmd_pwd), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("xpwd", new FtpService.del_cmd(this.cmd_pwd), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("abor", new FtpService.del_cmd(this.cmd_abor), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("site", new FtpService.del_cmd(this.cmd_site), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("help", new FtpService.del_cmd(this.cmd_help), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("noop", new FtpService.del_cmd(this.cmd_noop), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("stat", new FtpService.del_cmd(this.cmd_stat), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("size", new FtpService.del_cmd(this.cmd_size), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("mlsd", new FtpService.del_cmd(this.cmd_mlsd), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("mlst", new FtpService.del_cmd(this.cmd_mlst), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("mfmt", new FtpService.del_cmd(this.cmd_mfmt), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("feat", new FtpService.del_cmd(this.cmd_feat), FtpService.wflag.W_NORM));
      this.cmds.Add(new FtpService.CMDS("opts", new FtpService.del_cmd(this.cmd_opts), FtpService.wflag.W_NORM));
      Link.nocheck = true;
    }

    public override int get_timeout()
    {
      return 600;
    }

    public bool send(string format, params object[] args)
    {
      string x = string.Format(format, args);
      this.imsg("--> {0}", (object) x);
      return this.chan.write(x);
    }

    public void imsg(string format, params object[] args)
    {
      clib.fmsg("{0}", (object) string.Format(format, args));
    }

    public override object Clone()
    {
      return (object) new FtpService();
    }

    public override void OnAcceptConnection(ConnectionState state)
    {
      this.chan = state;
      this.imsg("Accepting FTP Connection from {0}", (object) state.RemoteEndPoint.ToString());
      this.send("220 Welcome to {0} FTP server\r\n", (object) clib.product_name());
      this.chan.set_mynodelay(true);
    }

    public override void OnReceiveData(ConnectionState state)
    {
      this.chan = state;
      int count = 2000;
      byte[] numArray = new byte[count];
      if (!state.Connected)
      {
        this.OnDropConnection(state);
      }
      else
      {
        while (state.AvailableData > 0)
        {
          int len = state.Read(numArray, 0, count);
          if (len > 0)
          {
            this.bf.add(numArray, len);
            this.process_input();
            if (this.doexit)
              state.EndConnection();
          }
          else
            state.EndConnection();
        }
      }
    }

    private void process_input()
    {
      /*
       * N.B.! this is dotPeek's version which causes warning,
       * we'll replace it with more compiler-friendly version below
      do
        ;
      while (this.process_input_one());
      */

      while (true)
        if (!this.process_input_one())
          return;
    }

    private bool process_input_one()
    {
      byte[] bytes = this.bf.getline();
      if (bytes == null)
        return false;
      this.do_cmd(Encoding.UTF8.GetString(bytes, 0, ((IEnumerable<byte>) bytes).Count<byte>()));
      return !this.doexit;
    }

    private void do_cmd(string cmdline)
    {
      string str1 = cmdline;
      string str2 = "";
      int length = cmdline.IndexOf(" ");
      if (length > 0)
      {
        str1 = cmdline.Substring(0, length);
        str2 = cmdline.Substring(length + 1);
        if (this.doutf8)
          str2 = clib.utf8_to_string(str2);
      }
      string lower = str1.ToLower();
      ++FtpService.nrequests;
      if (lower == "pass")
        this.imsg("--< " + lower + " ******");
      else
        this.imsg("--< " + lower + " " + str2);
      foreach (FtpService.CMDS cmd in this.cmds)
      {
        if (cmd.cmd == lower)
        {
          if (!this.isloggedin && cmd.myflags == FtpService.wflag.W_NORM)
          {
            this.send("550 sorry you must login first\r\n");
            return;
          }
          cmd.myfn(lower, str2);
          return;
        }
      }
      this.send("500 Unknown command ?? {0}\r\n", (object) cmdline);
      this.imsg("500 Unknown command ?? {0}", (object) cmdline);
    }

    private void cmd_rein(string cmd, string p1)
    {
      this.user = "";
      this.isloggedin = false;
      this.send("250 user logged off\r\n");
    }

    private void cmd_exit(string cmd, string p1)
    {
      this.send("200 exiting\r\n");
      this.doexit = true;
    }

    public override void OnDropConnection(ConnectionState state)
    {
      this.imsg("ondropconnection");
    }

    private void cmd_user(string cmd, string p1)
    {
      this.user = Vuser.add_domain(p1);
      this.isloggedin = false;
      this.send("331 username ok, need password\r\n");
    }

    private void cmd_pass(string cmd, string p1)
    {
      string reason;
      if (!Vuser.check(this.user, p1, out reason))
      {
        this.send("530 {0}\r\n", (object) reason);
      }
      else
      {
        this.send("230 User logged in\r\n");
        this.isloggedin = true;
        this.files = new Files();
        this.files.set_profile(this.user, "", 0L);
        this.files.show_aliases();
      }
    }

    private void cmd_opts(string cmd, string p1)
    {
      p1 = p1.ToLower();
      if (p1 == "utf8")
      {
        this.send("200 utf8 enabled\r\n");
        this.doutf8 = true;
      }
      else
        this.send("500 sorry unkown option\r\n");
    }

    private void cmd_feat(string cmd, string p1)
    {
      this.send("211- Feature response\r\n");
      this.send(" SIZE\r\n");
      this.send(" REST\r\n");
      this.send(" MLST\r\n");
      this.send(" MLSD\r\n");
      this.send(" STLS\r\n");
      this.send(" PROT\r\n");
      this.send(" PBSZ\r\n");
      this.send(" UTF8\r\n");
      this.send("211 END\r\n");
    }

    private void cmd_auth(string cmd, string p1)
    {
      p1 = p1.ToLower();
      if (p1 == "ssl")
      {
        this.ssl_data = true;
        this.ssl_con = false;
      }
      else if (p1 == "tls-p")
      {
        this.ssl_data = true;
        this.ssl_con = true;
      }
      else if (p1 == "tls-c")
      {
        this.ssl_data = false;
        this.ssl_con = true;
      }
      else if (p1 == "tls")
      {
        this.ssl_data = false;
        this.ssl_con = true;
      }
      else
      {
        this.send("504 Unknown auth mode\r\n");
        return;
      }
      if (this.ssl_con)
      {
        this.send("234 enabling SSL\r\n");
        this.takeover_ssl(this.chan, true);
      }
      else
        this.send("200 ok mode set\r\n");
    }

    public bool takeover_ssl(ConnectionState st, bool isserver)
    {
      Stream innerStream = (Stream) new NetworkStream(st._conn);
      st.ssl = new SslStreamM(innerStream, false, new RemoteCertificateValidationCallback(TcpServer.cert_validate));
      X509Certificate serverCertificate = Link.load_cert();
      if (serverCertificate == null)
      {
        st.ssl = (SslStreamM) null;
        return false;
      }
      try
      {
        st.ssl.AuthenticateAsServer(serverCertificate, false, SslProtocols.Default, false);
      }
      catch (Exception ex)
      {
        clib.imsg("authenticate failed, drat {0}", (object) ex.Message);
        return false;
      }
      return true;
    }

    private void cmd_prot(string cmd, string p1)
    {
      p1 = p1.ToLower();
      if (p1 == "p")
        this.ssl_data = true;
      else if (p1 == "c")
      {
        this.ssl_data = false;
      }
      else
      {
        this.send("504 Unknown prot option\r\n");
        return;
      }
      this.send("200 data channel will be {0}\r\n", this.ssl_data ? (object) "Protected" : (object) "Clear");
    }

    private void cmd_pbsz(string cmd, string p1)
    {
      this.send("200 Great whatever you say\r\n");
    }

    private void cmd_port(string cmd, string p1)
    {
      string str = this.chan.RemoteEndPoint.ToString();
      int length = str.IndexOf(":");
      if (length > 0)
        str = str.Substring(0, length);
      string[] strArray = p1.Split(",".ToCharArray(), StringSplitOptions.None);
      if (((IEnumerable<string>) strArray).Count<string>() < 6)
      {
        this.send("500 Invalid port commad\r\n");
      }
      else
      {
        this.port_ip = string.Format("{0}.{1}.{2}.{3}", (object) strArray[0], (object) strArray[1], (object) strArray[2], (object) strArray[3]);
        if (this.port_ip != str)
        {
          this.send("500 Wrong IP use passive mode  {0} {1}\r\n", (object) this.port_ip, (object) str);
        }
        else
        {
          this.port_port = clib.atoi(strArray[4]) * 256 + clib.atoi(strArray[5]);
          this.send("200 Port command ok {0} {1}\r\n", (object) this.port_ip, (object) this.port_port);
        }
      }
    }

    private void cmd_eprt(string cmd, string p1)
    {
      string[] strArray = p1.Split("|".ToCharArray(), StringSplitOptions.None);
      if (((IEnumerable<string>) strArray).Count<string>() < 4)
      {
        this.send("500 Invalid EPRT commad\r\n");
      }
      else
      {
        this.port_ip = strArray[2];
        this.port_port = clib.atoi(strArray[3]);
        this.send("200 Port command ok {0} {1}\r\n", (object) this.port_ip, (object) this.port_port);
      }
    }

    private string apply_path(string p1)
    {
      if (p1 == "." || p1 == "./")
        return this.cwd;
      if (p1 == "..")
      {
        clib.imsg("before apply up [{0}]", (object) this.cwd);
        string str = clib.pathonly(this.cwd);
        clib.imsg("before apply up [{0}] up={1}", (object) this.cwd, (object) str);
        if (str.Length == 0)
          str = "/";
        return str;
      }
      if (p1.StartsWith("/"))
        return p1;
      string cwd = this.cwd;
      if (!cwd.EndsWith("/"))
        cwd += "/";
      return cwd + p1;
    }

    private string list_date(DateTime n)
    {
      return string.Format("{0,3} {1,2} {2}", (object) n.ToString("MMM"), (object) n.Day, (object) n.Year);
    }

    private void cmd_size(string cmd, string p1)
    {
      Fileinfo info = this.files.get_info(this.apply_path(p1));
      if (info == null)
        this.send("550 could not get file size informatin \r\n");
      else
        this.send("213 {0}\r\n", (object) info.size);
    }

    private void cmd_mlsd(string cmd, string p1)
    {
      string path = this.apply_path(p1);
      Ftplink dataLink = this.get_data_link();
      if (dataLink == null)
        return;
      foreach (Fileinfo f in this.files.get_index(path))
        dataLink.netprintf("{0}\r\n", (object) this.fact_get(f));
      dataLink.netclose();
      this.send("226 Transfer complete for path {0}\r\n", (object) path);
    }

    private void cmd_mlst(string cmd, string p1)
    {
      if (p1 == "")
        p1 = ".";
      string path = this.apply_path(p1);
      this.send("250- Listing {0}\r\n", (object) path);
      this.send(" {0}\r\n", (object) this.files.get_info(path));
      this.send("250 End\r\n");
    }

    private string fact_get(Fileinfo f)
    {
      if (f == null)
        return string.Format("null fileinfo");
      return f.isdir ? string.Format("type=dir;size={0}; {1}", (object) f.size, (object) clib.string_to_utf8(f.name.file_only())) : string.Format("type=file;size={0};modify={1}; {2}", (object) f.size, (object) clib.date_to_ftp(f.modified), (object) clib.string_to_utf8(f.name.file_only()));
    }

    private void cmd_list(string cmd, string p1)
    {
      string path = this.apply_path(p1);
      Ftplink dataLink = this.get_data_link();
      if (dataLink == null)
        return;
      foreach (Fileinfo fileinfo in this.files.get_index(path))
        dataLink.send(string.Format("{0}rwxrwxrwx   1 user     user     {1,-10} {2} {3}\r\n", fileinfo.isdir ? (object) "d" : (object) "-", (object) fileinfo.size, (object) this.list_date(fileinfo.modified), (object) clib.string_to_utf8(clib.fileonly(fileinfo.name))));
      dataLink.netclose();
      this.send("226 Transfer complete for path {0}\r\n", (object) path);
    }

    private void cmd_nlst(string cmd, string p1)
    {
      Ftplink dataLink = this.get_data_link();
      if (dataLink == null)
        return;
      string path = this.apply_path(p1);
      foreach (Fileinfo fileinfo in this.files.get_index_fast(path))
        dataLink.netprintf("{0}\r\n", (object) clib.string_to_utf8(clib.fileonly(fileinfo.name, this.cwd)));
      dataLink.netclose();
      this.send("226 Transfer complete for path {0}\r\n", (object) path);
    }

    private void cmd_appe(string cmd, string p1)
    {
      this.cmd_stor_real(cmd, p1, true);
    }

    private void cmd_stor(string cmd, string p1)
    {
      this.cmd_stor_real(cmd, p1, false);
    }

    private void cmd_stor_real(string cmd, string p1, bool isappend)
    {
      bool flag = false;
      int max = 10000;
      byte[] bf = new byte[max];
      if (p1.Length == 0)
      {
        this.send("500 file name not specified\r\n");
      }
      else
      {
        string str = this.apply_path(p1);
        string reason;
        SimpleStream simpleStream = this.files.open(str, false, isappend, out reason);
        if (simpleStream == null)
        {
          this.send("550 store: open failed {0} {1} mode={2}\r\n", (object) str, (object) reason, isappend ? (object) "Append" : (object) "Write");
        }
        else
        {
          Ftplink dataLink = this.get_data_link();
          if (dataLink == null)
            return;
          if (this.restart != 0L)
            simpleStream.seek(this.restart);
          this.restart = 0L;
          int sz;
          do
          {
            sz = dataLink.read_bytes(bf, max, 30000, out bool _, out reason);
            if (sz <= 0)
              goto label_11;
          }
          while (simpleStream.write(bf, 0, sz) == sz);
          flag = true;
label_11:
          dataLink.netclose();
          simpleStream.close();
          if (flag)
          {
            this.send("550 Failed writing data\r\n");
          }
          else
          {
            this.send("226 Transfer complete {0} stored {1}\r\n", (object) str, isappend ? (object) "Append" : (object) "Write");
            if (this.files.get_run().Length <= 0)
              return;
            MyMain.spawn(this.files.get_run(), this.files.apply_profile(str));
          }
        }
      }
    }

    private void cmd_cwd(string cmd, string p1)
    {
      string fname = this.apply_path(p1);
      if (!this.files.is_dir(fname))
      {
        this.send("550 {0} is not a directory or is an alias but hasn't been created\r\n", (object) fname);
      }
      else
      {
        this.cwd = fname;
        this.send("250 CWD new path is {0}\r\n", (object) this.cwd);
      }
    }

    private void cmd_pwd(string cmd, string p1)
    {
      this.send("257 \"{0}\" is current directory.\r\n", (object) this.cwd);
    }

    private void cmd_cdup(string cmd, string p1)
    {
      if (this.cwd != "/")
        this.cwd = this.apply_path("..");
      this.send("250 CDUP new path is {0}\r\n", (object) this.cwd);
    }

    private Ftplink get_data_link()
    {
      Ftplink ftplink = new Ftplink();
      this.imsg("get_data_link: ssl_data={0}", (object) this.ssl_data);
      if (this.listen != null)
      {
        this.send("150 listening for {0} connection {1} (passive)\r\n", this.isbin ? (object) "BINARY" : (object) "ASCII", this.ssl_data ? (object) "SSL" : (object) "NoSSL");
        ftplink.accept(this.listen, 30);
        this.listen = (TcpListener) null;
        if (this.ssl_data)
          ftplink.ssl_enable(this.port_ip, true);
      }
      else
      {
        this.send("150 Opening {0} mode connection {1} {2} (active)\r\n", this.isbin ? (object) "BINARY" : (object) "ASCII", (object) this.port_ip, (object) this.port_port);
        string reason;
        if (!ftplink.open(this.port_ip, this.port_port, out reason))
        {
          clib.imsg("fl.open failed to connect {0}", (object) reason);
          this.send("500 Cannot connect to {0} {1}\r\n", (object) this.port_ip, (object) this.port_port);
          return (Ftplink) null;
        }
        clib.imsg("fl.open worked, connected ok ");
        if (this.ssl_data)
          ftplink.ssl_enable(this.port_ip, true);
      }
      return ftplink;
    }

    private void cmd_rnfr(string cmd, string p1)
    {
      this.rename_from = this.apply_path(p1);
      if (this.files.exists(this.rename_from))
        this.send("350 File exists ready for destination\r\n");
      else
        this.send("550 Source file not found {0}\r\n", (object) this.rename_from);
    }

    private void cmd_rnto(string cmd, string p1)
    {
      string reason;
      if (!this.files.rename(this.rename_from, this.apply_path(p1), out reason))
        this.send("550 Rename failed {0}\r\n", (object) reason);
      else
        this.send("250 RNTO command successful\r\n");
    }

    private void cmd_dele(string cmd, string p1)
    {
      string reason;
      if (!this.files.delete(this.apply_path(p1), out reason))
        this.send("550 Delete failed {0}\r\n", (object) reason);
      else
        this.send("250 DELE command successful\r\n");
    }

    private void cmd_rmd(string cmd, string p1)
    {
      string reason;
      if (!this.files.rmdir(this.apply_path(p1), out reason))
        this.send("550 RMD failed {0}\r\n", (object) reason);
      else
        this.send("250 RMD command successful\r\n");
    }

    private void cmd_mkd(string cmd, string p1)
    {
      string reason;
      if (!this.files.mkdir(this.apply_path(p1), out reason))
        this.send("550 MKD failed {0}\r\n", (object) reason);
      else
        this.send("250 MKD command successful - directory created\r\n");
    }

    private void cmd_xmkd(string cmd, string p1)
    {
      string reason;
      if (!this.files.mkdir(this.apply_path(p1), out reason))
        this.send("550 MKD failed {0}\r\n", (object) reason);
      else
        this.send("257 \"{0}\" MKD command successful - directory created\r\n", (object) this.apply_path(p1));
    }

    private void cmd_rest(string cmd, string p1)
    {
      this.restart = clib.atol(p1);
      this.send("350 Restarting at {0}.\r\n", (object) this.restart);
    }

    private void cmd_retr(string cmd, string p1)
    {
      bool flag = false;
      int sz = 10000;
      byte[] bf = new byte[sz];
      if (p1.Length == 0)
      {
        this.send("500 file name not specified\r\n");
      }
      else
      {
        string fname = this.apply_path(p1);
        string reason;
        SimpleStream simpleStream = this.files.open(fname, true, false, out reason);
        if (simpleStream == null)
        {
          this.send("550 Could not open  {0} {1}\r\n", (object) fname, (object) reason);
        }
        else
        {
          Ftplink dataLink = this.get_data_link();
          if (dataLink == null)
            return;
          if (this.restart != 0L)
            simpleStream.seek(this.restart);
          this.restart = 0L;
          int len;
          do
          {
            len = simpleStream.read(bf, 0, sz);
            if (len <= 0)
              goto label_11;
          }
          while (dataLink.write(bf, len));
          this.imsg("Write failed sending data");
          flag = true;
label_11:
          dataLink.netclose();
          simpleStream.close();
          if (flag)
            this.send("550 Failed writing data {0}\r\n", (object) fname);
          else
            this.send("226 Transfer complete {0}\r\n", (object) fname);
        }
      }
    }

    private string to_passive(string ip, int port)
    {
      char[] charArray = ip.ToCharArray();
      for (int index = 0; index < ((IEnumerable<char>) charArray).Count<char>(); ++index)
      {
        if (charArray[index] == '.')
          charArray[index] = ',';
      }
      return string.Format("{0},{1},{2}", (object) new string(charArray), (object) (port / 256), (object) (port % 256));
    }

    private void cmd_type(string cmd, string p1)
    {
      p1 = p1.ToLower();
      if (p1[0] == 'a')
        this.isbin = false;
      else if (p1[0] == 'i')
      {
        this.isbin = true;
      }
      else
      {
        this.send("500 Type setting not understood ({0})\r\n", (object) p1);
        return;
      }
      if (this.isbin)
        this.send("200 Type set to I\r\n");
      else
        this.send("200 Type set to A\r\n");
    }

    private void cmd_stou(string cmd, string p1)
    {
      string str = "";
      for (int index = 0; index < 1000; ++index)
      {
        str = string.Format("unique_{0}{1}.dat", (object) stat.time(), (object) index);
        if (!System.IO.File.Exists(str))
          break;
      }
      this.cmd_stor(cmd, str);
    }

    private void cmd_stru(string cmd, string p1)
    {
      p1 = p1.ToLower();
      if (p1[0] == 'f')
        this.send("200 STRU {0} ok.\r\n", (object) p1);
      else
        this.send("504 Unimplemented STRU type\r\n");
    }

    private void cmd_abor(string cmd, string p1)
    {
      this.send("250 Transfers aborted\r\n");
    }

    private void cmd_mode(string cmd, string p1)
    {
      p1 = p1.ToLower();
      if (p1[0] == 's')
        this.send("200 MODE {0} ok.\r\n", (object) p1);
      else
        this.send("504 Unimplemented MODE type\r\n");
    }

    private void cmd_syst(string cmd, string p1)
    {
      this.send("215 Windows\r\n");
    }

    private void cmd_site(string cmd, string p1)
    {
      this.send("502 SITE {0} not implemented\r\n", (object) p1);
    }

    private void cmd_help(string cmd, string p1)
    {
      this.send("250 Sorry no help output\r\n", (object) p1);
    }

    private void cmd_noop(string cmd, string p1)
    {
      this.send("200 NOOP command successful\r\n");
    }

    private void cmd_stat(string cmd, string p1)
    {
      // N.B.! probably this empty 'if' statement is residual from debugging.
      // if (p1.Length <= 0)
      // {
      // }

      this.send("211 STAT all is well at this end\r\n");
    }

    private void cmd_mfmt(string cmd, string p1)
    {
      int length = p1.IndexOf(" ");
      if (length < 0)
      {
        this.send("500 mfmt syntax error\r\n");
      }
      else
      {
        string sample = p1.Substring(0, length);
        string p1_1 = p1.Substring(length + 1);
        DateTime date = clib.ftp_to_date(sample);
        clib.imsg("tstr {0}, resulting date {1}", (object) sample, (object) date);
        string fname = this.apply_path(p1_1);
        string reason;
        if (!this.files.set_modified(fname, date, out reason))
          this.send("500 mfmt: failed to set modified time {0} ({1})\r\n", (object) reason, (object) fname);
        else
          this.send("213 Modify={0}; {1}\r\n", (object) clib.date_to_ftp(date), (object) fname);
      }
    }

    private void cmd_pasv(string cmd, string p1)
    {
      string ip = this.chan.localendpoint_ip();
      int port = 999;
      if (!this.do_listen(ip, out port))
        this.send("500 could not listen on port\r\n");
      else if (p1 == "test3")
      {
        this.send("500 test3\r\n");
      }
      else
      {
        this.imsg("227 Entering Passive Mode ({0}).", (object) this.to_passive(ip, port));
        this.send("227 Entering Passive Mode ({0}).\r\n", (object) this.to_passive(ip, port));
      }
    }

    private void cmd_epsv(string cmd, string p1)
    {
      if (p1.ToLower() == "all")
      {
        this.send("200 EPSV command successful.\r\n");
      }
      else
      {
        string ip = this.chan.localendpoint_ip();
        int port = 999;
        if (!this.do_listen(ip, out port))
        {
          this.send("500 could not listen on port\r\n");
        }
        else
        {
          this.send("229 Entering Extended Passive Mode (|||{0}|)\r\n", (object) port);
          this.imsg("229 Entering Extended Passive Mode (|||{0}|)\r\n", (object) port);
        }
      }
    }

    private bool do_listen(string ip, out int port)
    {
      this.listen = new TcpListener(IPAddress.Any, 0);
      this.listen.Start();
      port = ConnectionState.localendpoint_port(this.listen.LocalEndpoint);
      return true;
    }

    private delegate void del_cmd(string cmd, string p1);

    private enum wflag
    {
      W_NORM,
      W_LOGIN,
    }

    private struct CMDS
    {
      public string cmd;
      public FtpService.del_cmd myfn;
      public FtpService.wflag myflags;

      public CMDS(string a, FtpService.del_cmd b, FtpService.wflag c)
      {
        this.cmd = a;
        this.myfn = b;
        this.myflags = c;
      }
    }
  }
}
