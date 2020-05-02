// Decompiled with JetBrains decompiler
// Type: ChrisLib.AuthDigest
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.Security.Cryptography;

namespace ChrisLib
{
  public class AuthDigest
  {
    private static void test_digest()
    {
      string nonce = "dcd98b7102dd2f0e8b11d0f600bfb0c093";
      string cnonce = "0a4f113b";
      string user = "Mufasa";
      string realm = "testrealm@host.com";
      string pass = "Circle Of Life";
      string alg = "md5";
      string ncount = "00000001";
      string method = "GET";
      string qop = "auth";
      string uri = "/dir/index.html";
      string hentity = "";
      string sessionkey;
      AuthDigest.digest_ha1(alg, user, realm, pass, nonce, cnonce, out sessionkey);
      string response;
      AuthDigest.digest_response(sessionkey, nonce, ncount, cnonce, qop, method, uri, hentity, out response);
      Console.WriteLine("Response = {0}", (object) response);
    }

    public static string byte_to_string(byte[] bob, int len)
    {
      char[] chArray = new char[len];
      for (int index = 0; index < len; ++index)
        chArray[index] = (char) bob[index];
      return new string(chArray);
    }

    private static byte[] md5_hash(byte[] bs, int len)
    {
      bs = new MD5CryptoServiceProvider().ComputeHash(bs, 0, len);
      return bs;
    }

    public static void digest_ha1(string user, string realm, string pass, out string sessionkey)
    {
      AuthDigest.digest_ha1("md5", user, realm, pass, "", "", out sessionkey);
    }

    public static void digest_ha1(
      string alg,
      string user,
      string realm,
      string pass,
      string nonce,
      string cnonce,
      out string sessionkey)
    {
      Bstring bstring = new Bstring();
      bstring.concat(user);
      bstring.concat(":".tobyte());
      bstring.concat(realm.tobyte());
      bstring.concat(":".tobyte());
      bstring.concat(pass.tobyte());
      byte[] numArray = AuthDigest.md5_hash(bstring.bf, bstring.len);
      if (alg == "md5-sess")
      {
        bstring.len = 0;
        bstring.concat(numArray);
        bstring.concat(":".tobyte());
        bstring.concat(nonce.tobyte());
        bstring.concat(":".tobyte());
        bstring.concat(cnonce.tobyte());
        numArray = AuthDigest.md5_hash(bstring.bf, bstring.len);
      }
      sessionkey = clib.byte_to_hex(numArray);
    }

    public static void digest_response(
      string ha1,
      string nonce,
      string ncount,
      string cnonce,
      string qop,
      string method,
      string uri,
      string hentity,
      out string response)
    {
      Bstring bstring = new Bstring();
      bstring.concat(method.tobyte());
      bstring.concat(":".tobyte());
      bstring.concat(uri.tobyte());
      if (qop == "auth-int")
      {
        bstring.concat(":".tobyte());
        bstring.concat(hentity.tobyte());
      }
      string hex = clib.byte_to_hex(AuthDigest.md5_hash(bstring.bf, bstring.len));
      bstring.len = 0;
      bstring.concat(ha1.tobyte());
      bstring.concat(":".tobyte());
      bstring.concat(nonce.tobyte());
      bstring.concat(":".tobyte());
      if (qop.Length > 0)
      {
        bstring.concat(ncount.tobyte());
        bstring.concat(":".tobyte());
        bstring.concat(cnonce.tobyte());
        bstring.concat(":".tobyte());
        bstring.concat(qop.tobyte());
        bstring.concat(":".tobyte());
      }
      bstring.concat(hex.tobyte());
      byte[] bs = AuthDigest.md5_hash(bstring.bf, bstring.len);
      response = clib.byte_to_hex(bs);
    }
  }
}
