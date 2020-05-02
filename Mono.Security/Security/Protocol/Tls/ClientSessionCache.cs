// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.ClientSessionCache
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Collections;

namespace Mono.Security.Protocol.Tls
{
  internal class ClientSessionCache
  {
    private static Hashtable cache = new Hashtable();
    private static object locker = new object();

    public static void Add(string host, byte[] id)
    {
      lock (ClientSessionCache.locker)
      {
        string str = BitConverter.ToString(id);
        ClientSessionInfo clientSessionInfo = (ClientSessionInfo) ClientSessionCache.cache[(object) str];
        if (clientSessionInfo == null)
          ClientSessionCache.cache.Add((object) str, (object) new ClientSessionInfo(host, id));
        else if (clientSessionInfo.HostName == host)
        {
          clientSessionInfo.KeepAlive();
        }
        else
        {
          clientSessionInfo.Dispose();
          ClientSessionCache.cache.Remove((object) str);
          ClientSessionCache.cache.Add((object) str, (object) new ClientSessionInfo(host, id));
        }
      }
    }

    public static byte[] FromHost(string host)
    {
      lock (ClientSessionCache.locker)
      {
        foreach (ClientSessionInfo clientSessionInfo in (IEnumerable) ClientSessionCache.cache.Values)
        {
          if (clientSessionInfo.HostName == host && clientSessionInfo.Valid)
          {
            clientSessionInfo.KeepAlive();
            return clientSessionInfo.Id;
          }
        }
        return (byte[]) null;
      }
    }

    private static ClientSessionInfo FromContext(
      Context context,
      bool checkValidity)
    {
      if (context == null)
        return (ClientSessionInfo) null;
      byte[] sessionId = context.SessionId;
      if (sessionId == null || sessionId.Length == 0)
        return (ClientSessionInfo) null;
      string str = BitConverter.ToString(sessionId);
      ClientSessionInfo clientSessionInfo = (ClientSessionInfo) ClientSessionCache.cache[(object) str];
      if (clientSessionInfo == null)
        return (ClientSessionInfo) null;
      if (context.ClientSettings.TargetHost != clientSessionInfo.HostName)
        return (ClientSessionInfo) null;
      if (!checkValidity || clientSessionInfo.Valid)
        return clientSessionInfo;
      clientSessionInfo.Dispose();
      ClientSessionCache.cache.Remove((object) str);
      return (ClientSessionInfo) null;
    }

    public static bool SetContextInCache(Context context)
    {
      lock (ClientSessionCache.locker)
      {
        ClientSessionInfo clientSessionInfo = ClientSessionCache.FromContext(context, false);
        if (clientSessionInfo == null)
          return false;
        clientSessionInfo.GetContext(context);
        clientSessionInfo.KeepAlive();
        return true;
      }
    }

    public static bool SetContextFromCache(Context context)
    {
      lock (ClientSessionCache.locker)
      {
        ClientSessionInfo clientSessionInfo = ClientSessionCache.FromContext(context, true);
        if (clientSessionInfo == null)
          return false;
        clientSessionInfo.SetContext(context);
        clientSessionInfo.KeepAlive();
        return true;
      }
    }
  }
}
