// Decompiled with JetBrains decompiler
// Type: mycloud.ProjectInstaller
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace mycloud
{
  [RunInstaller(true)]
  public class ProjectInstaller : Installer
  {
    private IContainer components = (IContainer) null;
    private ServiceProcessInstaller serviceProcessInstaller1;
    private ServiceInstaller serviceInstaller1;

    public ProjectInstaller()
    {
      this.InitializeComponent();
    }

    private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
    {
      Directory.CreateDirectory("\\ftpusers");
      Directory.CreateDirectory("\\ftpdav");
      ServiceController serviceController = new ServiceController("FtpDav");
      try
      {
        serviceController.Start();
      }
      catch
      {
      }
      string str = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\manager.htm";
      try
      {
        Process.Start("cmd", "/c start http://localhost:6080/cgi/admin.cgi");
      }
      catch (Exception ex)
      {
        File.WriteAllText("install.log", "proces.start failed " + ex.Message);
      }
    }

    public override void Commit(IDictionary savedState)
    {
      base.Commit(savedState);
    }

    private void serviceProcessInstaller1_AfterInstall(object sender, InstallEventArgs e)
    {
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.serviceProcessInstaller1 = new ServiceProcessInstaller();
      this.serviceInstaller1 = new ServiceInstaller();
      this.serviceProcessInstaller1.Account = ServiceAccount.LocalService;
      this.serviceProcessInstaller1.Password = (string) null;
      this.serviceProcessInstaller1.Username = (string) null;
      this.serviceProcessInstaller1.AfterInstall += new InstallEventHandler(this.serviceProcessInstaller1_AfterInstall);
      this.serviceInstaller1.Description = "Personal FTP/WEBDAV server";
      this.serviceInstaller1.DisplayName = "FtpDav";
      this.serviceInstaller1.ServiceName = "FtpDav";
      this.serviceInstaller1.StartType = ServiceStartMode.Automatic;
      this.serviceInstaller1.AfterInstall += new InstallEventHandler(this.serviceInstaller1_AfterInstall);
      this.Installers.AddRange(new Installer[2]
      {
        (Installer) this.serviceProcessInstaller1,
        (Installer) this.serviceInstaller1
      });
    }
  }
}
