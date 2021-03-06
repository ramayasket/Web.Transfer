// Decompiled with JetBrains decompiler
// Type: Mylib.WebPostBinary
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.Collections;
using System.IO;
using System.Net;

namespace Mylib
{
  internal class WebPostBinary
  {
    private WebRequest theRequest;
    private HttpWebResponse theResponse;
    private ArrayList theQueryData;
    private string filename;

    public WebPostBinary(string url, string user, string pass, string filenamex)
    {
      this.theRequest = WebRequest.Create(url);
      this.theRequest.Method = "POST";
      this.filename = filenamex;
      this.theQueryData = new ArrayList();
      this.theRequest.Credentials = (ICredentials) new NetworkCredential(user, pass);
    }

    public void Add(string key, string value)
    {
      this.theQueryData.Add((object) string.Format("{0}={1}", (object) key, (object) stat.url_encode(value)));
      stat.imsg("Encoded string is {0}", (object) stat.url_encode(value));
    }

    public string GetResponse()
    {
      this.theRequest.ContentType = "multipart/form-data";
      string str = string.Join("&", (string[]) this.theQueryData.ToArray(typeof (string)));
      this.theRequest.ContentLength = (long) str.Length;
      try
      {
        StreamWriter streamWriter = new StreamWriter(this.theRequest.GetRequestStream());
        streamWriter.Write(str);
        streamWriter.Close();
        this.theResponse = (HttpWebResponse) this.theRequest.GetResponse();
        return new StreamReader(this.theResponse.GetResponseStream()).ReadToEnd();
      }
      catch (Exception ex)
      {
        stat.imsg("IGnoring error talking to web server", (object) ex.ToString());
        return "-ERR failed to connect " + ex.ToString();
      }
    }
  }
}
