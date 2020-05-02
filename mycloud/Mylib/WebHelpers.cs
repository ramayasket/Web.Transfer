// Decompiled with JetBrains decompiler
// Type: Mylib.WebHelpers
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Mylib
{
  public static class WebHelpers
  {
    public static Encoding encoding = Encoding.UTF8;

    public static HttpWebResponse MultipartFormDataPost(
      string postUrl,
      string userAgent,
      Dictionary<string, object> postParameters,
      string user,
      string pass)
    {
      string boundary = "-----------------------------28947758029299";
      string contentType = "multipart/form-data; boundary=" + boundary;
      byte[] multipartFormData = WebHelpers.GetMultipartFormData(postParameters, boundary);
      return WebHelpers.PostForm(postUrl, userAgent, contentType, multipartFormData, user, pass);
    }

    private static HttpWebResponse PostForm(
      string postUrl,
      string userAgent,
      string contentType,
      byte[] formData,
      string user,
      string pass)
    {
      if (!(WebRequest.Create(postUrl) is HttpWebRequest httpWebRequest))
        throw new NullReferenceException("request is not a http request");
      httpWebRequest.Method = "POST";
      httpWebRequest.Timeout = 10000;
      httpWebRequest.ContentType = contentType;
      httpWebRequest.UserAgent = userAgent;
      httpWebRequest.CookieContainer = new CookieContainer();
      NetworkCredential networkCredential = new NetworkCredential(user, pass);
      httpWebRequest.Credentials = (ICredentials) networkCredential;
      httpWebRequest.ContentLength = (long) formData.Length;
      try
      {
        using (Stream requestStream = httpWebRequest.GetRequestStream())
        {
          requestStream.Write(formData, 0, formData.Length);
          requestStream.Close();
        }
        return httpWebRequest.GetResponse() as HttpWebResponse;
      }
      catch (Exception ex)
      {
        stat.imsg("Post failed {0}", (object) ex.ToString());
        return (HttpWebResponse) null;
      }
    }

    public static bool get_file(string url, string fname, out int gotbytes, out string reason)
    {
      int num = 0;
      gotbytes = 0;
      stat.imsg("get_file {0} {1}", (object) url, (object) fname);
      reason = "";
      try
      {
        if (System.IO.File.Exists(fname))
          System.IO.File.Delete(fname);
        WebResponse response = WebRequest.Create(url).GetResponse();
        byte[] buffer = new byte[32768];
        using (Stream responseStream = response.GetResponseStream())
        {
          using (FileStream fileStream = new FileStream(fname, FileMode.CreateNew))
          {
            int count;
            while ((count = responseStream.Read(buffer, 0, buffer.Length)) > 0)
            {
              fileStream.Write(buffer, 0, count);
              num += count;
              gotbytes = num;
            }
            fileStream.Close();
          }
        }
      }
      catch (Exception ex)
      {
        stat.imsg("get failed {0}", (object) ex.ToString());
        reason = "Fetching download failed " + ex.Message;
        return false;
      }
      stat.imsg("get completed ok {0} {1}", (object) fname, (object) gotbytes);
      return true;
    }

    private static byte[] GetMultipartFormData(
      Dictionary<string, object> postParameters,
      string boundary)
    {
      Stream stream = (Stream) new MemoryStream();
      foreach (KeyValuePair<string, object> postParameter in postParameters)
      {
        if (postParameter.Value is byte[])
        {
          byte[] buffer = postParameter.Value as byte[];
          string s = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: application/octet-stream\r\n\r\n", (object) boundary, (object) postParameter.Key, (object) postParameter.Key);
          stream.Write(WebHelpers.encoding.GetBytes(s), 0, s.Length);
          stream.Write(buffer, 0, buffer.Length);
        }
        else
        {
          string s = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n", (object) boundary, (object) postParameter.Key, postParameter.Value);
          stream.Write(WebHelpers.encoding.GetBytes(s), 0, s.Length);
        }
      }
      string s1 = "\r\n--" + boundary + "--\r\n";
      stream.Write(WebHelpers.encoding.GetBytes(s1), 0, s1.Length);
      stream.Position = 0L;
      byte[] buffer1 = new byte[stream.Length];
      stream.Read(buffer1, 0, buffer1.Length);
      stream.Close();
      return buffer1;
    }
  }
}
