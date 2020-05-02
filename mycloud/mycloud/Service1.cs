// Decompiled with JetBrains decompiler
// Type: mycloud.Service1
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System.ComponentModel;
using System.ServiceProcess;
using System.Threading;

namespace mycloud
{
  public class Service1 : ServiceBase
  {
    private IContainer components = (IContainer) null;

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.ServiceName = nameof (Service1);
    }

    public Service1()
    {
      this.InitializeComponent();
      this.ServiceName = "FtpDav";
    }

    protected override void OnStart(string[] args)
    {
      new Thread(new ThreadStart(MyMain.console_main)).Start();
    }

    protected override void OnStop()
    {
      MyMain.shutdown = true;
    }
  }
}
