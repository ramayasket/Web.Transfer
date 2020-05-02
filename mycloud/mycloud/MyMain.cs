// Decompiled with JetBrains decompiler
// Type: mycloud.MyMain
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using Mylib;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using TcpLib;

namespace mycloud
{
  internal class MyMain
  {
    public static DateTime start_time;
    private static bool main_debug;
    public static bool shutdown;
    private static int last;
    private static Websvc websvc;
    private static Websvc websvc2;
    private static Websvc webdavsvc;
    private static Websvc s_webdavsvc;
    private static Websvc s_websvc;
    private static FtpService ftpservice;

    public static string realm()
    {
      return clib.product_name().ToLower() + "." + Environment.MachineName.ToLower();
    }

    public static bool spawn(string cmd, string p1)
    {
      p1 = clib.to_native_slash(p1);
      clib.imsg("Running command: {0} {1}", (object) cmd, (object) p1);
      try
      {
        Process process = new Process();
        process.StartInfo.FileName = cmd;
        process.StartInfo.Arguments = p1;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.WorkingDirectory = clib.work(nameof (spawn));
        process.Start();
        string end = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        clib.imsg("Output: {0}", (object) end);
        return true;
      }
      catch (Exception ex)
      {
        clib.imsg("Could not spawn program: {0} {1} ", (object) cmd, (object) ex.Message);
        return false;
      }
    }

    public static TcpServer WebServer { get; private set; }

    public static TcpServer WebServer2 { get; private set; }

    public static TcpServer s_WebServer { get; private set; }

    public static TcpServer WebDavServer { get; private set; }

    public static TcpServer s_WebDavServer { get; private set; }

    public static TcpServer ftpServer { get; private set; }

    private static void tsecond_Elapsed(object bob)
    {
      clib.log_idle();
    }

    private static void record_crash(string reason, string stack)
    {
      clib.imsg("Serious crash occurred: {0}", (object) reason);
      clib.imsg("Serious crash occurred: STACK {0}", (object) stack);
      clib.cmsg("Serious crash occurred: {0}", (object) reason);
      clib.cmsg("Serious crash occurred: STACK {0}", (object) stack);
      clib.log_flush();
      clib.startstop("Crash event occurred {0} {1}", (object) reason, (object) stack);
    }

    private static void CurrentDomain_UnhandledException(
      object sender,
      UnhandledExceptionEventArgs e)
    {
      Exception exceptionObject = e.ExceptionObject as Exception;
      MyMain.record_crash(exceptionObject.Message, exceptionObject.ToString());
    }

    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      Exception exception = e.Exception;
      MyMain.record_crash(exception.Message, exception.ToString());
    }

    public static void startListeners()
    {
      clib.imsg("Creating tcpserver's...");
      MyMain.s_websvc = new Websvc();
      MyMain.s_WebServer = new TcpServer((TcpServiceProvider) MyMain.s_websvc, Ini.getstring(En.bind), Ini.getint(En.web_port_ssl), true);
      MyMain.s_WebServer.Start();
      MyMain.webdavsvc = new Websvc();
      MyMain.webdavsvc.iswebdav = true;
      MyMain.WebDavServer = new TcpServer((TcpServiceProvider) MyMain.webdavsvc, Ini.getstring(En.bind), Ini.getint(En.webdav_port), false);
      MyMain.WebDavServer.Start();
      MyMain.s_webdavsvc = new Websvc();
      MyMain.s_webdavsvc.iswebdav = true;
      MyMain.s_WebDavServer = new TcpServer((TcpServiceProvider) MyMain.s_webdavsvc, Ini.getstring(En.bind), Ini.getint(En.webdav_port_ssl), true);
      if (!MyMain.s_WebDavServer.Start())
        return;
      MyMain.ftpservice = new FtpService();
      MyMain.ftpServer = new TcpServer((TcpServiceProvider) MyMain.ftpservice, Ini.getstring(En.bind), Ini.getint(En.ftp_port), false);
      MyMain.ftpServer.Start();
      clib.imsg("Going to listeon on port {0} for http admin connections", (object) Ini.getint(En.web_port));
      MyMain.websvc = new Websvc();
      MyMain.WebServer = new TcpServer((TcpServiceProvider) MyMain.websvc, Ini.getstring(En.bind), Ini.getint(En.web_port), false);
      if (!MyMain.WebServer.Start())
      {
        MyMain.WebServer = new TcpServer((TcpServiceProvider) MyMain.websvc, Ini.getstring(En.bind), Ini.getint(En.web_port) + 1, false);
        if (MyMain.WebServer.Start())
        {
          clib.imsg("Listing on next port worked.  So changing setting...");
          Ini.do_set(En.web_port, clib.int_to_string(Ini.getint(En.web_port) + 1));
        }
      }
      MyMain.websvc2 = new Websvc();
      MyMain.WebServer2 = new TcpServer((TcpServiceProvider) MyMain.websvc2, Ini.getstring(En.bind), 6080, false);
      MyMain.WebServer2.Start();
    }

    public static void stopListeners()
    {
      clib.imsg("Stopping listeners");
      if (MyMain.s_WebDavServer != null)
      {
        MyMain.s_WebDavServer.Stop();
        MyMain.s_WebDavServer = (TcpServer) null;
      }
      if (MyMain.WebServer != null)
      {
        MyMain.WebServer.Stop();
        MyMain.WebServer = (TcpServer) null;
      }
      if (MyMain.WebServer2 != null)
      {
        MyMain.WebServer2.Stop();
        MyMain.WebServer2 = (TcpServer) null;
      }
      if (MyMain.s_WebServer != null)
      {
        MyMain.s_WebServer.Stop();
        MyMain.s_WebServer = (TcpServer) null;
      }
      if (MyMain.WebDavServer != null)
      {
        MyMain.WebDavServer.Stop();
        MyMain.WebDavServer = (TcpServer) null;
      }
      if (MyMain.ftpServer == null)
        return;
      MyMain.ftpServer.Stop();
      MyMain.ftpServer = (TcpServer) null;
    }

    public static void console_main()
    {
      MyMain.console_main(true);
    }

    public static void set_debug(bool x)
    {
      MyMain.main_debug = x;
    }

    public static void console_main(bool block)
    {
      MyMain.start_time = DateTime.Now;
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyMain.CurrentDomain_UnhandledException);
      Timer timer = new Timer(new TimerCallback(MyMain.tsecond_Elapsed), (object) null, 0, 1000);
      clib.init_log_files();
      clib.set_debug(true);
      clib.imsg("Logging home {0}", (object) clib.log_file("imsg.log"));
      clib.imsg("Starting FTPDAV Version {0} Build {1}", (object) clib.Version(), (object) clib.Build());
      clib.startstop("Starting FTPDAV Version {0} {1}", (object) clib.Version(), (object) clib.Build());
      Ini.init(clib.work("config.ini"));
      clib.set_debug(MyMain.main_debug);
      clib.log_idle();
      Quota.init();
      clib.set_tmp(clib.work("tmp"));
      Directory.CreateDirectory(clib.tmp());
      Directory.CreateDirectory(clib.work("userdb"));
      Directory.CreateDirectory(clib.work("spawn"));
      Directory.CreateDirectory(clib.work("log"));
      Vuser.init(clib.work("userdb"));
      Link.set_paths(clib.work(""), clib.app(""));
      Profile.load();
      SimpleHash.unit_test();
      Link.set_ssl_password(Ini.getstring(En.ssl_password));
      clib.set_debug(true);
      MyMain.startListeners();
      if (!MyMain.main_debug)
        clib.imsg("Going quiet now as no -debug switch on command line...");
      clib.set_debug(MyMain.main_debug);
      MyKey.init(clib.work("key.dat"));
      try
      {
        File.Delete(clib.work("ftpdav.exit"));
      }
      catch
      {
        clib.imsg("FAILED TO DELETE FTPDAV.EXIT");
      }
      if (!block)
        return;
      while (true)
      {
        try
        {
          File.WriteAllText(clib.work("main.running"), "running");
          if (File.Exists(clib.work("ftpdav.exit")))
          {
            clib.imsg("Exiting because ftpdav.exit found");
            try
            {
              File.Delete(clib.work("ftpdav.exit"));
              goto label_18;
            }
            catch (Exception ex)
            {
              clib.imsg("Delete failed {0}", (object) ex.Message);
              goto label_18;
            }
          }
        }
        catch
        {
        }
        Thread.Sleep(1000);
        if (!MyMain.shutdown)
        {
          if (clib.time() - MyMain.last > 60)
          {
            MyMain.last = clib.time();
            Quota.save();
          }
        }
        else
          break;
      }
      clib.imsg("Exiting because shutdown flag true");
label_18:
      File.Delete(clib.work("main.running"));
      clib.imsg("Key pressed or ftpdav.exit found ==============================");
      Quota.save();
      clib.startstop("Clean shutdown FTPDAV Version {0}", (object) clib.Version());
    }
  }
}
