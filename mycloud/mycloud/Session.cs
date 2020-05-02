// Decompiled with JetBrains decompiler
// Type: mycloud.Session
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using System;
using System.Collections.Generic;

namespace mycloud
{
  public class Session
  {
    private static List<Session> all = new List<Session>();
    private static Random r = new Random();
    public string cwd = "/";
    private string id;
    public User urec;

    public Session(User u)
    {
      this.urec = u;
    }

    public bool isadmin()
    {
      return this.urec != null && this.urec.isadmin();
    }

    public static string create(User u)
    {
      Session s = new Session(u);
label_1:
      lock (Session.all)
      {
        s.id = string.Format("{0}", (object) Session.r.NextDouble());
        if (!Session.all.Exists((Predicate<Session>) (x => x.id == s.id)))
        {
          Session.all.Add(s);
          return s.id;
        }
        goto label_1;
      }
    }

    public static Session find(string id)
    {
      lock (Session.all)
      {
        foreach (Session session in Session.all)
        {
          if (session.id == id)
            return session;
        }
        return (Session) null;
      }
    }

    public static void logout(string id)
    {
      lock (Session.all)
      {
        foreach (Session session in Session.all)
        {
          if (session.id == id)
          {
            Session.all.Remove(session);
            break;
          }
        }
      }
    }
  }
}
