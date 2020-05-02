// Decompiled with JetBrains decompiler
// Type: Mylib.WebGetRequest
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.IO;
using System.Net;

namespace Mylib
{
  internal class WebGetRequest
  {
    private WebRequest theRequest;
    private HttpWebResponse theResponse;

    public WebGetRequest(string url, string user, string pass)
    {
      this.theRequest = WebRequest.Create(url);
      this.theRequest.Credentials = (ICredentials) new NetworkCredential(user, pass);
      this.theRequest.Timeout = 20000;
    }

    public string GetResponse()
    {
      try
      {
        this.theResponse = (HttpWebResponse) this.theRequest.GetResponse();
        return new StreamReader(this.theResponse.GetResponseStream()).ReadToEnd();
      }
      catch (Exception ex)
      {
        stat.imsg("could not talk to server {0}", (object) ex.ToString());
        return "";
      }
    }
  }
}
