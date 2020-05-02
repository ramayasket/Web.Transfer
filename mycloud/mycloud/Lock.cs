// Decompiled with JetBrains decompiler
// Type: mycloud.Lock
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace mycloud
{
  public class Lock
  {
    private static List<Lock> all = new List<Lock>();
    private static int upto;
    public string path;
    public string owner;
    public bool deeper;
    public DateTime timeout;
    public int timeout_sec;
    public string opaque;
    public bool exclusive;

    private Lock(string path, string owner, bool deeper, bool exclusive)
    {
      this.path = path;
      this.owner = owner;
      this.exclusive = exclusive;
      this.deeper = deeper;
      this.opaque = string.Format("opaquelocktoken:{0}.{1}", (object) clib.time(), (object) Lock.upto++);
      this.timeout = DateTime.Now.AddSeconds((double) this.timeout_sec);
    }

    public static Lock get_lock(
      string path,
      string owner,
      bool deeper,
      bool exclusive,
      List<Lock> heldlist,
      List<Fail> faillist,
      int timeout_sec)
    {
      clib.imsg("get_lock for {0} timeout {1}", (object) path, (object) timeout_sec);
      Lock.prune_locks();
      heldlist.Clear();
      lock (Lock.all)
      {
        if (!Lock.can_lock(path, deeper, exclusive, heldlist, faillist))
          return (Lock) null;
        Lock @lock = new Lock(path, owner, deeper, exclusive);
        Lock.all.Add(@lock);
        @lock.timeout_sec = timeout_sec;
        return @lock;
      }
    }

    public static Lock find_lock(string opaque)
    {
      lock (Lock.all)
      {
        foreach (Lock @lock in Lock.all)
        {
          if (@lock.opaque == opaque)
            return @lock;
        }
      }
      return (Lock) null;
    }

    public static bool free_lock(string opaque)
    {
      lock (Lock.all)
      {
        foreach (Lock @lock in Lock.all)
        {
          if (@lock.opaque == opaque)
          {
            Lock.all.Remove(@lock);
            return true;
          }
        }
      }
      return false;
    }

    public static bool unlock_all(string path, bool deeper)
    {
      lock (Lock.all)
      {
        List<Lock> lockList = new List<Lock>();
        foreach (Lock @lock in Lock.all)
        {
          if (deeper && @lock.path.StartsWith(path) || path == @lock.path)
          {
            clib.imsg("Unlock_all: releasing lock {0} {1} ", (object) @lock.path, (object) path);
            lockList.Add(@lock);
          }
        }
        foreach (Lock @lock in lockList)
          Lock.all.Remove(@lock);
      }
      return true;
    }

    public static bool isok(string path, List<string> opaque, bool shared_ok, out string reason)
    {
      bool flag = true;
      reason = "none";
      Lock.prune_locks();
      lock (Lock.all)
      {
        foreach (Lock l in Lock.all)
        {
          if (Lock.path_match(l, path, false))
          {
            if (opaque != null)
            {
              foreach (string str1 in opaque)
              {
                string str2 = str1.Trim("<>".ToCharArray());
                if (l.opaque == str2)
                {
                  l.timeout = DateTime.Now.AddSeconds((double) l.timeout_sec);
                  return true;
                }
              }
            }
            if (!shared_ok)
            {
              reason = "Shared lock not ok and didn't match the lock opaque=" + (object) opaque.Count<string>();
              flag = false;
            }
            else if (l.exclusive)
            {
              flag = false;
              reason = "Exclusive lock, and didn't match";
            }
          }
        }
      }
      return flag;
    }

    private static void prune_locks()
    {
      lock (Lock.all)
      {
        foreach (Lock @lock in Lock.all)
        {
          if (@lock.timeout.Subtract(DateTime.Now).Minutes < 0)
          {
            Lock.all.Remove(@lock);
            break;
          }
        }
      }
    }

    public static bool is_locked(string opaque)
    {
      Lock.prune_locks();
      opaque = opaque.Trim("<>".ToCharArray());
      lock (Lock.all)
      {
        foreach (Lock @lock in Lock.all)
        {
          if (@lock.opaque == opaque)
          {
            @lock.timeout = DateTime.Now.AddSeconds((double) @lock.timeout_sec);
            clib.imsg("is_locked: found matching lock yay");
            return true;
          }
        }
      }
      clib.imsg("is_locked: did not find a matching lock");
      return false;
    }

    private static bool path_match(Lock l, string path, bool deeper)
    {
      return l.timeout.Subtract(DateTime.Now).Minutes >= 0 && (path == l.path || l.deeper && path.StartsWith(l.path) || deeper && l.path.StartsWith(path));
    }

    private static bool can_lock(
      string path,
      bool deeper,
      bool exclusive,
      List<Lock> heldlist,
      List<Fail> faillist)
    {
      bool flag1 = true;
      Lock.prune_locks();
      foreach (Lock l in Lock.all)
      {
        if (Lock.path_match(l, path, deeper))
        {
          bool flag2 = true;
          if (exclusive)
            flag2 = false;
          else if (l.exclusive)
            flag2 = false;
          if (!flag2)
          {
            faillist.Add(new Fail(l.path, "423 Already locked"));
            heldlist.Add(l);
            flag1 = false;
          }
        }
      }
      return flag1;
    }

    public static List<Lock> load_lock(string path)
    {
      List<Lock> lockList = new List<Lock>();
      foreach (Lock l in Lock.all)
      {
        if (Lock.path_match(l, path, false))
          lockList.Add(l);
      }
      return lockList;
    }
  }
}
