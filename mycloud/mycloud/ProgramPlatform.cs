// Decompiled with JetBrains decompiler
// Type: mycloud.ProgramPlatform
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System.ServiceProcess;

namespace mycloud
{
  internal class ProgramPlatform
  {
    public static void svc_main()
    {
      ServiceBase.Run(new ServiceBase[1]
      {
        (ServiceBase) new Service1()
      });
    }
  }
}
