// Decompiled with JetBrains decompiler
// Type: mycloud.Profile
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace mycloud
{
  internal class Profile
  {
    private static List<Profile> profiles = new List<Profile>();
    public string name;
    public string users;
    public string run;
    public string groups;
    public List<VPath> paths;

    public Profile(string name, string users, string groups, string run)
    {
      this.name = name;
      this.users = users;
      this.groups = groups;
      this.run = run;
      this.paths = new List<VPath>();
    }

    public bool matches(string user, string group)
    {
      foreach (string str in this.users.Split(", ".ToCharArray()))
      {
        if (user == str)
          return true;
      }
      foreach (string str in this.groups.Split(", ".ToCharArray()))
      {
        if (group == str)
          return true;
      }
      return !(user == "anonymous") && (this.users == "*" || this.groups == "*");
    }

    public static void profile_add(Profile padd)
    {
      for (int index = 0; index < Profile.profiles.Count<Profile>(); ++index)
      {
        Profile profile = Profile.profiles[index];
        if (padd.name == profile.name)
        {
          Profile.profiles[index] = padd;
          return;
        }
      }
      Profile.profiles.Add(padd);
    }

    public static Profile read_profile(string line)
    {
      NameValueCollection nameValueCollection = clib.simple_read(line);
      return new Profile(nameValueCollection.Get("profile"), nameValueCollection.Get("users"), nameValueCollection.Get("groups"), nameValueCollection.Get("run"));
    }

    private static bool add_alias(Profile p, string line)
    {
      NameValueCollection nameValueCollection = clib.simple_read(line);
      p.paths.Add(new VPath(nameValueCollection.Get("alias"), nameValueCollection.Get("path"), nameValueCollection.Get("access")));
      return true;
    }

    public void paths_clear()
    {
      this.paths = new List<VPath>();
    }

    public void path_add(string alias, string path, string access)
    {
      this.paths.Add(new VPath(alias, path, access));
    }

    public static bool unit_test()
    {
      Profile padd = new Profile("default", "*", "*", "");
      Profile.profile_add(padd);
      padd.paths_clear();
      padd.path_add("/", "\\" + clib.product_name(), "rw");
      Profile.save();
      return true;
    }

    public static bool create_simple_profile()
    {
      Profile padd1 = new Profile("default", "*", "", "");
      Profile.profile_add(padd1);
      string rootPath = clib.root_path;
      padd1.paths_clear();
      string fname1 = rootPath + "/shared";
      string fname2 = rootPath + "/public";
      string fname3 = rootPath + "/public/upload";
      clib.to_native_slash(fname1);
      string nativeSlash1 = clib.to_native_slash(fname2);
      string nativeSlash2 = clib.to_native_slash(fname3);
      Directory.CreateDirectory(rootPath);
      Directory.CreateDirectory(nativeSlash1);
      Directory.CreateDirectory(nativeSlash2);
      padd1.path_add("/", rootPath + "/$user$", "full");
      Profile padd2 = new Profile("anonymous", "anonymous", "anonymous", "");
      Profile.profile_add(padd2);
      padd2.paths_clear();
      padd2.path_add("/", nativeSlash1, "readonly");
      Profile.save();
      return true;
    }

    public static bool load()
    {
      StreamReader streamReader;
      try
      {
        streamReader = new StreamReader(clib.work("profiles.dat"));
      }
      catch
      {
        clib.imsg("cannot read profiles.dat - so creating simple one...");
        Profile.create_simple_profile();
        return false;
      }
      Profile profile = (Profile) null;
      while (!streamReader.EndOfStream)
      {
        string line = streamReader.ReadLine();
        if (line.StartsWith("profile="))
        {
          if (profile != null)
            Profile.profile_add(profile);
          profile = Profile.read_profile(line);
        }
        else if (line.StartsWith("alias=") && profile != null)
          Profile.add_alias(profile, line);
      }
      if (profile != null)
        Profile.profile_add(profile);
      streamReader.Close();
      return true;
    }

    public static bool save()
    {
      StreamWriter streamWriter = new StreamWriter(clib.work("profiles.dat"));
      foreach (Profile profile in Profile.profiles)
        streamWriter.WriteLine(profile.ToString());
      streamWriter.Close();
      return true;
    }

    public static int profile_n()
    {
      return Profile.profiles.Count<Profile>();
    }

    public static Profile profile_get(int i)
    {
      return i >= Profile.profiles.Count<Profile>() ? (Profile) null : Profile.profiles[i];
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendFormat("profile={0}~users={1}~groups={2}~run={2}\n", (object) clib.simple_encode(this.name), (object) clib.simple_encode(this.users), (object) clib.simple_encode(this.groups), (object) clib.simple_encode(this.run));
      foreach (VPath path in this.paths)
        stringBuilder.AppendFormat("{0}\n", (object) path.ToString());
      return stringBuilder.ToString();
    }
  }
}
