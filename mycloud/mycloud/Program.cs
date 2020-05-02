// Decompiled with JetBrains decompiler
// Type: mycloud.Program
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System;
using System.IO;

namespace mycloud
{
  public static class Program
  {
    public static int Main(string[] args)
    {
      foreach (string str in args)
      {
        if (str == "-debug")
          MyMain.set_debug(true);
      }
      string str1;
      string str2;
      if (!clib.IsLinux)
      {
        str1 = "/" + clib.Product_Name();
        clib.root_path = "\\ftpusers";
        str2 = str1 + "\\log";
      }
      else
      {
        str1 = "/var/ftpdav";
        clib.root_path = "/home/ftpusers";
        str2 = str1 + "/log";
      }
      Directory.CreateDirectory(str1);
      Directory.CreateDirectory(str2);
      if (!clib.IsLinux)
        Directory.SetCurrentDirectory(clib.app("."));
      clib.set_work(str1);
      clib.set_log(str2);
      if (clib.IsLinux)
      {
        MyMain.console_main(true);
        return 0;
      }
      if (!Environment.UserInteractive)
        ProgramPlatform.svc_main();
      else
        MyMain.console_main(true);
      return 0;
    }
  }
}
