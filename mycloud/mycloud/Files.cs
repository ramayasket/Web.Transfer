// Decompiled with JetBrains decompiler
// Type: mycloud.Files
// Assembly: mycloud, Version=1.0.4700.21522, Culture=neutral, PublicKeyToken=null
// MVID: 0787DFF3-C1F2-4B9F-8B58-42463D42EB31
// Assembly location: C:\Program Files (x86)\FtpDav\mycloud.exe

using ChrisLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace mycloud
{
  public class Files
  {
    private static string pext = Property.pext;
    private Profile profile = (Profile) null;
    private string current_user = "";
    public long quota_permitted;
    private Property prop;

    public NameValueCollection property_list()
    {
      return this.prop.all;
    }

    public string property_get(string key)
    {
      return this.prop.all[key];
    }

    public string get_run()
    {
      return this.profile == null || this.profile.run == null ? "" : this.profile.run;
    }

    public bool property_load(string path)
    {
      path = this.tild_encode(path);
      string path1 = this.apply_profile(path);
      this.prop = new Property();
      this.prop.load(path1);
      Fileinfo infoRaw = this.get_info_raw(path);
      clib.imsg("Property load {0} {1} ", (object) path1, (object) File.Exists(path1));
      this.prop.set("DAV::supportedlock", "<D:lockentry><D:lockscope><D:exclusive/></D:lockscope><D:locktype><D:write/></D:locktype><D:lockscope><D:shared/></D:lockscope><D:locktype><D:write/></D:locktype></D:lockentry>");
      this.load_lock(path);
      if (infoRaw == null)
        return false;
      if (infoRaw != null)
      {
        this.prop.set("DAV::getcontentlength", clib.long_to_string(infoRaw.size));
        this.prop.set("DAV::creationdate", infoRaw.created.ToString("s") + "-00:00");
        this.prop.set("DAV::getlastmodified", infoRaw.modified.ToHttpDate());
        this.prop.set("DAV::getcontenttype", clib.content_type(infoRaw.name));
        this.prop.set("DAV::displayname", clib.xml_encode(clib.fileonly(infoRaw.name)));
        this.prop.set("DAV::getetag", infoRaw.etag());
        if (infoRaw.isdir)
          this.prop.set("DAV::resourcetype", "<D:collection/>");
        if (infoRaw.isdir)
        {
          if (this.quota_permitted > 0L)
            this.prop.set("DAV::quota-available-bytes", clib.long_to_string(this.quota_permitted));
          this.prop.set("DAV::quota-used-bytes", clib.long_to_string(Quota.get(this.current_user)));
        }
      }
      return true;
    }

    public bool property_save(string path)
    {
      path = this.tild_encode(path);
      bool isreadonly;
      string path1 = this.apply_profile(path, out isreadonly);
      return !isreadonly && this.prop.save(path1);
    }

    public void property_delete(string var)
    {
      this.prop.del(var);
    }

    public void property_set(string var, string value)
    {
      this.prop.set(var, value);
    }

    public void show_aliases()
    {
      foreach (VPath path in this.profile.paths)
        clib.imsg("Alias {0} to directory {1} --> {2}", (object) path.alias, (object) path.path, (object) this.real_translate(path.path));
    }

    public SimpleStream open(
      string fname,
      bool isread,
      bool isappend,
      out string reason)
    {
      fname = this.tild_encode(fname);
      clib.imsg("ssltream open {0}", (object) fname);
      return this.open_raw(fname, isread, isappend, out reason);
    }

    public SimpleStream open_raw(
      string fname,
      bool isread,
      bool isappend,
      out string reason)
    {
      reason = "";
      bool isreadonly;
      string fname1 = this.apply_profile(fname, out isreadonly);
      if (isreadonly && !isread)
      {
        reason = "Readonly directory";
        return (SimpleStream) null;
      }
      SimpleStream simpleStream = new SimpleStream();
      clib.imsg("ss.open {0} ", (object) fname1);
      if (!simpleStream.open(fname1, isread, isappend, this.current_user, out reason))
        return (SimpleStream) null;
      clib.imsg("ss.open WORKED {0} ", (object) fname1);
      return simpleStream;
    }

    private void load_lock(string fname)
    {
      StringBuilder stringBuilder = new StringBuilder();
      fname = this.tild_encode(fname);
      foreach (Lock @lock in Lock.load_lock(this.apply_profile(fname)))
      {
        stringBuilder.Append("<D:activelock>");
        stringBuilder.Append(string.Format("<D:lockscope>{0}</D:lockscope>", @lock.exclusive ? (object) "<D:exclusive/>" : (object) "<D:shared/>"));
        stringBuilder.Append("<D:locktype><D:write/></D:locktype>");
        stringBuilder.Append(string.Format("<D:depth>{0}</D:depth>", @lock.deeper ? (object) "Infinity" : (object) "0"));
        stringBuilder.Append(string.Format("<D:owner>{0}</D:owner>", (object) @lock.owner));
        stringBuilder.Append(string.Format("<D:timeout>Second-3600</D:timeout>"));
        stringBuilder.Append(string.Format("<D:locktoken><D:href>{0}</D:href></D:locktoken>", (object) @lock.opaque));
        stringBuilder.Append("</D:activelock>");
      }
      this.prop.set("DAV::lockdiscovery", stringBuilder.ToString());
    }

    public Lock get_lock(
      string fname,
      string owner,
      bool deeper,
      bool exclusive,
      List<Lock> heldlist,
      List<Fail> faillist,
      int timeout)
    {
      fname = this.tild_encode(fname);
      string path = this.apply_profile(fname);
      clib.imsg("get_lock {0} {1}", (object) path, (object) File.Exists(path));
      Lock @lock = Lock.get_lock(this.apply_profile(fname), owner, deeper, exclusive, heldlist, faillist, timeout);
      if (@lock == null)
      {
        foreach (Fail fail in faillist)
          fail.url = this.torel(this.apply_profile(fname), fail.url, fname);
      }
      clib.imsg("get_lock_done {0} {1}", (object) path, (object) File.Exists(path));
      return @lock;
    }

    public bool free_lock(string lockid)
    {
      return Lock.free_lock(lockid);
    }

    private bool quota_exceeded(out string reason)
    {
      reason = "";
      if (this.quota_get() > this.quota_permitted && this.quota_permitted > 0L)
      {
        reason = string.Format("500 Quota Exceeded {0} of {1}", (object) clib.nice_int(this.quota_get()), (object) this.quota_permitted);
        clib.imsg("quota_Test: {0}", (object) reason);
        return true;
      }
      clib.imsg("Quota is good :-)");
      return false;
    }

    public bool rename(string from, string to, out string reason)
    {
      from = this.tild_encode(from);
      to = this.tild_encode(to);
      clib.imsg("files.rename {0} {1} ", (object) from, (object) to);
      if (!this.write_check(from, out reason) || !this.write_check(to, out reason))
        return false;
      bool isreadonly;
      string str1 = this.apply_profile(from, out isreadonly);
      if (isreadonly)
      {
        reason = "500 rename: source is readonly";
        return false;
      }
      string str2 = this.apply_profile(to, out isreadonly);
      if (isreadonly)
      {
        reason = "500 rename: dest is readonly";
        return false;
      }
      if (!File.Exists(str1) && !Directory.Exists(str1))
      {
        reason = "412 rename: source file doesn't exist " + this.apply_profile(from);
        return false;
      }
      if (File.Exists(str2) || Directory.Exists(str2))
      {
        reason = "412 rename: dest file exists already " + this.apply_profile(to);
        return false;
      }
      reason = "201 rename worked";
      try
      {
        clib.imsg("rename: from   {0} to {1}", (object) str1, (object) str2);
        File.Move(str1, str2);
        clib.imsg("rename: WORKED {0} to {1}", (object) str1, (object) str2);
      }
      catch (Exception ex)
      {
        reason = "412 rename: failed " + ex.Message;
        return false;
      }
      clib.imsg("and also rename the proeprty file {0} {1} ", (object) (this.apply_profile(from) + Property.pext), (object) (this.apply_profile(to) + Property.pext));
      try
      {
        File.Move(this.apply_profile(from) + Property.pext, this.apply_profile(to) + Property.pext);
      }
      catch (Exception ex)
      {
        clib.imsg("rename property failed {0} ", (object) ex.Message);
      }
      return true;
    }

    public bool rename_dir(string from, string to, out string reason)
    {
      from = this.tild_encode(from);
      to = this.tild_encode(to);
      if (!this.write_check(from, out reason) || !this.write_check(to, out reason))
        return false;
      reason = "200 rename dir worked";
      try
      {
        Directory.Move(this.apply_profile(from), this.apply_profile(to));
      }
      catch (Exception ex)
      {
        reason = "412 rename_dir:  " + ex.Message;
        return false;
      }
      return true;
    }

    public bool unlock_all(string fname, bool deeper)
    {
      fname = this.tild_encode(fname);
      return Lock.unlock_all(this.apply_profile(fname), deeper);
    }

    public bool write_check(string fname, out string reason)
    {
      return this.write_check(fname, "write", out reason);
    }

    public bool write_check(string fname, string want, out string reason)
    {
      reason = "200 ok";
      if (!clib.path_valid(fname))
      {
        reason = "500 Bad chars in filename " + fname;
        return false;
      }
      bool isreadonly;
      string access;
      this.apply_profile(fname, out isreadonly, out access);
      if (want == "write" && isreadonly)
      {
        reason = "500 File is readonly " + fname;
        return false;
      }
      if (!(want == "delete") || !(access != "full"))
        return true;
      reason = "500 File cannot be deleted sorry, access rule applied " + fname;
      return false;
    }

    public bool delete_dir_or_file(string fname, out string reason)
    {
      return this.is_dir(fname) ? this.delete_dir(fname, out reason) : this.delete(fname, out reason);
    }

    public bool delete_dir(string fname, out string reason)
    {
      fname = this.tild_encode(fname);
      if (!this.write_check(fname, out reason))
        return false;
      reason = "204 no content - delete worked";
      if (!Directory.Exists(this.apply_profile(fname)))
      {
        reason = "404 delete failed as does not exist";
        return false;
      }
      try
      {
        this.quota_delete_dir(fname);
        clib.imsg("Deleting Directory {0} {1}", (object) fname, (object) this.apply_profile(fname));
        Directory.Delete(this.apply_profile(fname), true);
      }
      catch (Exception ex)
      {
        reason = "404 delete failure: " + ex.Message;
        return false;
      }
      return true;
    }

    public string tild_encode(string input)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (char ch in input)
      {
        if (ch == '^' || ch == '~')
        {
          string str = BitConverter.ToString(new byte[1]
          {
            (byte) ch
          });
          stringBuilder.AppendFormat("^{0}", (object) str);
        }
        else
          stringBuilder.Append(ch);
      }
      clib.imsg("encode to {0}", (object) stringBuilder.ToString());
      return stringBuilder.ToString();
    }

    public static string tild_decode(string input)
    {
      StringBuilder stringBuilder1 = new StringBuilder();
      StringBuilder stringBuilder2 = new StringBuilder();
      bool flag = false;
      foreach (char ch in input)
      {
        if (flag)
        {
          stringBuilder2.Append(ch);
          if (stringBuilder2.Length == 2)
          {
            char int32 = (char) Convert.ToInt32(stringBuilder2.ToString(), 16);
            stringBuilder2.Length = 0;
            flag = false;
            stringBuilder1.Append(int32);
          }
        }
        else if (ch == '^')
          flag = true;
        else
          stringBuilder1.Append(ch);
      }
      clib.imsg("decoded to {0}", (object) stringBuilder1.ToString());
      return stringBuilder1.ToString();
    }

    public bool delete(string fname, out string reason)
    {
      return this.delete_raw(this.tild_encode(fname), out reason);
    }

    public bool delete_raw(string fname, out string reason)
    {
      if (!this.write_check(fname, "delete", out reason))
        return false;
      clib.imsg("delete: {0} --> {1}", (object) fname, (object) this.apply_profile(fname));
      reason = "204 no content - delete worked";
      if (!File.Exists(this.apply_profile(fname)))
      {
        reason = "500 delete failed as does not exist";
        return false;
      }
      try
      {
        this.quota_delete_file(fname);
        File.Delete(this.apply_profile(fname));
      }
      catch (Exception ex)
      {
        reason = "500 delete failure: " + ex.Message;
        return false;
      }
      return true;
    }

    public bool access_upload(string path)
    {
      path = this.tild_encode(path);
      path = path.TrimEnd('/');
      return this.exists_raw(path + "/.~access_upload");
    }

    public bool access_public(string path)
    {
      path = this.tild_encode(path);
      path = path.TrimEnd('/');
      return this.exists_raw(path + "/.~access_public");
    }

    public bool exists(string fname)
    {
      return this.exists_raw(this.tild_encode(fname));
    }

    public bool exists_raw(string fname)
    {
      string path = this.apply_profile(fname);
      bool flag = File.Exists(path);
      if (!flag)
        flag = Directory.Exists(path);
      return flag;
    }

    public bool set_modified(string fname, DateTime t, out string reason)
    {
      reason = "worked";
      fname = this.tild_encode(fname);
      string path = this.apply_profile(fname);
      if (File.Exists(path))
      {
        Directory.SetLastWriteTime(path, t);
        return true;
      }
      reason = "no such file";
      return false;
    }

    public bool rmdir(string fname, out string reason)
    {
      reason = "worked";
      try
      {
        this.quota_delete_dir(fname);
        Directory.Delete(this.apply_profile(fname), false);
      }
      catch (Exception ex)
      {
        reason = "delete: " + ex.Message;
        return false;
      }
      return true;
    }

    private string remove_prefix(string fname, string prefix)
    {
      return fname.Substring(prefix.Length);
    }

    public bool copy_dir(string src, string dest, out string reason, bool docreate)
    {
      bool flag = true;
      reason = "";
      clib.imsg("copy_dir {0} {1}", (object) src, (object) dest);
      if (!this.is_dir(src))
        return this.copy(src, dest, out reason, docreate);
      clib.imsg("copy_dir {0} {1} get tree", (object) src, (object) dest);
      Index tree = this.get_tree(src, true);
      clib.imsg("copy_dir collection {0}", (object) tree.count());
      dest = dest.TrimEnd("/".ToCharArray());
      foreach (Fileinfo fileinfo in tree)
      {
        string dest1 = dest + this.remove_prefix(fileinfo.name, src);
        clib.imsg("copy_dir {0} to {1}", (object) fileinfo.name, (object) dest1);
        if (!fileinfo.isdir && !this.copy(fileinfo.name, dest1, out reason, true))
        {
          flag = false;
          break;
        }
      }
      return flag;
    }

    public bool copy(string src, string dest, out string reason, bool docreate)
    {
      src = this.tild_encode(src);
      dest = this.tild_encode(dest);
      if (this.quota_exceeded(out reason) || !this.write_check(dest, out reason))
        return false;
      string full = this.apply_profile(dest);
      if (docreate)
      {
        string path = clib.pathonly(full);
        if (!Directory.Exists(path))
        {
          Directory.CreateDirectory(path);
          clib.imsg("Created {0}", (object) path);
        }
        else
          clib.imsg("Already existed {0}", (object) path);
      }
      if (File.Exists(this.apply_profile(dest)))
      {
        reason = "412 precondition failed, file exists";
        return false;
      }
      try
      {
        File.Copy(this.apply_profile(src), this.apply_profile(dest));
        this.quota_add_file(dest);
        reason = "201 Created";
      }
      catch (Exception ex)
      {
        reason = "409 copy failed " + ex.Message;
        return false;
      }
      try
      {
        File.Copy(this.apply_profile(src) + Property.pext, this.apply_profile(dest) + Property.pext);
      }
      catch
      {
      }
      return true;
    }

    public bool is_dir(string fname)
    {
      return this.is_dir_raw(this.tild_encode(fname));
    }

    public bool is_dir_raw(string fname)
    {
      Fileinfo infoRaw = this.get_info_raw(fname);
      return infoRaw != null && infoRaw.isdir;
    }

    public bool mkdir(string fname, out string reason)
    {
      fname = this.tild_encode(fname);
      if (!this.write_check(fname, out reason))
        return false;
      reason = "204 no content, mkdir worked";
      try
      {
        Directory.CreateDirectory(this.apply_profile(fname));
      }
      catch (Exception ex)
      {
        reason = "409 mkdir failed " + ex.Message;
        return false;
      }
      return true;
    }

    public bool set_profile(string username, string groups, long quota)
    {
      this.quota_permitted = quota;
      if (this.quota_permitted == 0L)
        this.quota_permitted = clib.nice_atol(Ini.getstring(En.quota_default));
      if (this.quota_permitted == 0L)
        this.quota_permitted = 1000000000L;
      if (username == "anonymous")
        groups = "anonymous";
      for (int i = 0; i < Profile.profile_n(); ++i)
      {
        Profile profile = Profile.profile_get(i);
        if (profile.matches(username, groups))
          this.profile = profile;
      }
      if (this.profile == null)
        clib.imsg("NO PROFILE FOUND FOR THIS USER");
      this.current_user = username;
      return this.profile != null;
    }

    public bool upload(string realpath, string vpath, out string reason)
    {
      vpath = this.tild_encode(vpath);
      if (!this.quota_exceeded(out reason) && this.write_check(vpath, out reason))
      {
        string str = this.apply_profile(vpath);
        clib.imsg("Copying {0} it to {1}", (object) realpath, (object) str);
        if (File.Exists(str))
        {
          this.quota_delete_file(vpath);
          File.Delete(str);
        }
        File.Copy(realpath, str);
        this.quota_add_file(vpath);
        return true;
      }
      File.Delete(realpath);
      return false;
    }

    private string real_translate(string real)
    {
      return real.Replace("$user$", this.current_user);
    }

    public string apply_profile(string path)
    {
      return this.apply_profile(path, out bool _);
    }

    public string apply_profile(string path, out bool isreadonly)
    {
      return this.apply_profile(path, out isreadonly, out string _);
    }

    public string apply_profile(string path, out bool isreadonly, out string access)
    {
      return Files.fname_encode(clib.to_native_slash(this.apply_profile_naked(path, out isreadonly, out access)));
    }

    public static string fname_decode(string text)
    {
      return !clib.IsLinux ? text : clib.utf8_to_string(text);
    }

    public static string fname_encode(string text)
    {
      return !clib.IsLinux ? text : clib.string_to_utf8(text);
    }

    private string fname_encode_old(string text)
    {
      if (text == null)
        return "";
      StringBuilder stringBuilder = new StringBuilder();
      foreach (char ch in text)
      {
        if (ch > '\x007F' || ch < ' ' || (ch == '~' || ch == ';') || ch == '#' || ch == '=')
          stringBuilder.Append("#" + (object) (int) ch + ";");
        else
          stringBuilder.Append(ch);
      }
      return stringBuilder.ToString();
    }

    public string apply_profile_naked(string path, out bool isreadonly, out string access)
    {
      VPath vpath = (VPath) null;
      access = "";
      if (this.profile == null)
      {
        isreadonly = true;
        access = "none";
        return "NOPROFILE";
      }
      if (this.profile.paths != null)
      {
        foreach (VPath path1 in this.profile.paths)
        {
          if (path.StartsWith(path1.alias))
            vpath = path1;
        }
      }
      if (vpath != null)
      {
        string str = this.real_translate(vpath.path);
        int length = vpath.alias.Length;
        isreadonly = vpath.isreadonly();
        access = vpath.access;
        return str + "/" + path.Substring(length);
      }
      isreadonly = true;
      return "/$NOPROFILE$";
    }

    private string torel(string realpath, string x, string relpath)
    {
      x = clib.to_unix_slash(x);
      return relpath + x.Substring(realpath.Length);
    }

    private string fixname(string realpath, string x, string relpath)
    {
      x = clib.to_unix_slash(x);
      string str = relpath + x;
      if (!relpath.EndsWith("/"))
        str = relpath + "/" + x;
      return str;
    }

    public bool lock_ok(string path, List<string> lockid, bool shared_ok, out string reason)
    {
      path = this.tild_encode(path);
      return Lock.isok(this.apply_profile(path), lockid, shared_ok, out reason);
    }

    public bool is_locked(string path, string lockid)
    {
      return Lock.is_locked(lockid);
    }

    public string get_etag(string path)
    {
      Fileinfo info = this.get_info(path);
      return info == null ? "noetagyet" : info.etag();
    }

    public Fileinfo get_info(string path)
    {
      return this.get_info_raw(this.tild_encode(path));
    }

    public Fileinfo get_info_raw(string path)
    {
      string str = this.apply_profile(path);
      Fileinfo fileinfo1;
      try
      {
        FileInfo fileInfo = new FileInfo(str);
        fileinfo1 = new Fileinfo(path, fileInfo.Length, fileInfo.CreationTime, fileInfo.LastWriteTime, (fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory);
      }
      catch (Exception ex)
      {
        Fileinfo fileinfo2;
        try
        {
          DirectoryInfo directoryInfo = new DirectoryInfo(str);
          if (directoryInfo == null || !directoryInfo.Exists)
            return (Fileinfo) null;
          fileinfo2 = new Fileinfo(path, 0L, directoryInfo.CreationTime, directoryInfo.LastWriteTime, true);
        }
        catch
        {
          clib.imsg("get_info: failed {0} {1}", (object) str, (object) ex.Message);
          return (Fileinfo) null;
        }
        return fileinfo2;
      }
      return fileinfo1;
    }

    public Index get_index_fast(string path)
    {
      return this.get_index(path);
    }

    public Index get_tree(string path, bool deeper)
    {
      Index addto = new Index();
      Fileinfo info = this.get_info(path);
      if (info == null)
        return addto;
      if (!deeper)
      {
        addto.add(info);
        return addto;
      }
      clib.imsg("get_tree, adding dir and contents");
      addto.add(info);
      if (info.isdir)
        this.get_index_deeper(addto, path);
      return addto;
    }

    public void get_index_deeper(Index addto, string path)
    {
      Index index = this.get_index(path);
      if (path == null)
        return;
      if (!this.is_dir(path))
      {
        Fileinfo info = this.get_info(path);
        if (info == null)
          return;
        addto.add(info);
      }
      else
      {
        foreach (Fileinfo f in index)
        {
          addto.add(f);
          if (f.isdir)
            this.get_index_deeper(addto, f.name);
        }
      }
    }

    public Index get_index(string path)
    {
      Index index = new Index();
      string searchPattern = (string) null;
      path = this.tild_encode(path);
      string str = this.apply_profile(path);
      if (str.EndsWith("\\"))
      {
        str = str.Substring(0, str.Length - 1);
      }
      else
      {
        try
        {
          if (!Directory.Exists(str))
          {
            if (File.Exists(str))
            {
              FileInfo ff = new FileInfo(str);
              index.add(new Fileinfo(path, ff));
              return index;
            }
          }
          else
            goto label_7;
        }
        catch
        {
        }
        searchPattern = clib.fileonly(str);
        str = clib.pathonly(str);
        path = clib.pathonly(path);
      }
label_7:
      FileInfo[] files;
      DirectoryInfo[] directories;
      try
      {
        DirectoryInfo directoryInfo = new DirectoryInfo(str);
        if (searchPattern == null)
        {
          files = directoryInfo.GetFiles();
          directories = directoryInfo.GetDirectories();
        }
        else
        {
          files = directoryInfo.GetFiles(searchPattern);
          directories = directoryInfo.GetDirectories(searchPattern);
        }
      }
      catch (Exception ex)
      {
        clib.imsg("Caught error {0}", (object) ex.Message);
        return index;
      }
      if (path == "/")
      {
        foreach (VPath path1 in this.profile.paths)
        {
          if (!(path1.alias == "/"))
          {
            Fileinfo f = new Fileinfo(path1.alias, 0L, DateTime.Now, DateTime.Now, true);
            index.add(f);
          }
        }
      }
      foreach (DirectoryInfo directoryInfo in directories)
      {
        if (clib.path_valid(directoryInfo.Name))
          index.add(new Fileinfo(this.fixname(str, directoryInfo.Name, path), 0L, directoryInfo.CreationTime, directoryInfo.LastWriteTime, true));
      }
      foreach (FileInfo ff in files)
      {
        if (!ff.Name.EndsWith(Files.pext) && !ff.Name.Contains(".~") && clib.path_valid(ff.Name))
        {
          string name = this.fixname(str, ff.Name, path);
          clib.imsg("Index: file name is {0}  ", (object) name);
          index.add(new Fileinfo(name, ff));
        }
      }
      return index;
    }

    private void quota_delete_dir(string path)
    {
      long num = 0;
      foreach (Fileinfo fileinfo in this.get_tree(path, true))
        num += fileinfo.size;
      this.quota_add(-num);
    }

    private void quota_delete_file(string path)
    {
      this.quota_add(-this.get_info_raw(path).size);
    }

    private void quota_add_file(string path)
    {
      this.quota_add(this.get_info_raw(path).size);
    }

    private void quota_add(long bytes)
    {
      Quota.add(this.current_user, bytes);
    }

    private long quota_get()
    {
      return Quota.get(this.current_user);
    }

    public static void unit_test()
    {
      Files files = new Files();
      files.set_profile("bob", "staff", 0L);
      foreach (Fileinfo fileinfo in files.get_index("/"))
        clib.imsg("File {0} ", (object) fileinfo.name);
    }
  }
}
