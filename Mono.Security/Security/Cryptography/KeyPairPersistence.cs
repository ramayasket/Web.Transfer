// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.KeyPairPersistence
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Xml;
using System;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security.Cryptography
{
  public class KeyPairPersistence
  {
    private static bool _userPathExists = false;
    private static bool _machinePathExists = false;
    private static object lockobj = new object();
    private static string _userPath;
    private static string _machinePath;
    private CspParameters _params;
    private string _keyvalue;
    private string _filename;
    private string _container;

    public KeyPairPersistence(CspParameters parameters)
      : this(parameters, (string) null)
    {
    }

    public KeyPairPersistence(CspParameters parameters, string keyPair)
    {
      if (parameters == null)
        throw new ArgumentNullException(nameof (parameters));
      this._params = this.Copy(parameters);
      this._keyvalue = keyPair;
    }

    public string Filename
    {
      get
      {
        if (this._filename == null)
        {
          this._filename = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "[{0}][{1}][{2}].xml", (object) this._params.ProviderType, (object) this.ContainerName, (object) this._params.KeyNumber);
          this._filename = !this.UseMachineKeyStore ? Path.Combine(KeyPairPersistence.UserPath, this._filename) : Path.Combine(KeyPairPersistence.MachinePath, this._filename);
        }
        return this._filename;
      }
    }

    public string KeyValue
    {
      get
      {
        return this._keyvalue;
      }
      set
      {
        if (!this.CanChange)
          return;
        this._keyvalue = value;
      }
    }

    public CspParameters Parameters
    {
      get
      {
        return this.Copy(this._params);
      }
    }

    public bool Load()
    {
      bool flag = File.Exists(this.Filename);
      if (flag)
      {
        using (StreamReader streamReader = File.OpenText(this.Filename))
          this.FromXml(streamReader.ReadToEnd());
      }
      return flag;
    }

    public void Save()
    {
      using (FileStream fileStream = File.Open(this.Filename, FileMode.Create))
      {
        StreamWriter streamWriter = new StreamWriter((Stream) fileStream, Encoding.UTF8);
        streamWriter.Write(this.ToXml());
        streamWriter.Close();
      }
      if (this.UseMachineKeyStore)
        KeyPairPersistence.ProtectMachine(this.Filename);
      else
        KeyPairPersistence.ProtectUser(this.Filename);
    }

    public void Remove()
    {
      File.Delete(this.Filename);
    }

    private static string UserPath
    {
      get
      {
        lock (KeyPairPersistence.lockobj)
        {
          if (KeyPairPersistence._userPath != null)
          {
            if (KeyPairPersistence._userPathExists)
              goto label_7;
          }
          KeyPairPersistence._userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".mono");
          KeyPairPersistence._userPath = Path.Combine(KeyPairPersistence._userPath, "keypairs");
          KeyPairPersistence._userPathExists = Directory.Exists(KeyPairPersistence._userPath);
          if (!KeyPairPersistence._userPathExists)
          {
            try
            {
              Directory.CreateDirectory(KeyPairPersistence._userPath);
              KeyPairPersistence.ProtectUser(KeyPairPersistence._userPath);
              KeyPairPersistence._userPathExists = true;
            }
            catch (Exception ex)
            {
              throw new CryptographicException(string.Format(Locale.GetText("Could not create user key store '{0}'."), (object) KeyPairPersistence._userPath), ex);
            }
          }
        }
label_7:
        if (!KeyPairPersistence.IsUserProtected(KeyPairPersistence._userPath))
          throw new CryptographicException(string.Format(Locale.GetText("Improperly protected user's key pairs in '{0}'."), (object) KeyPairPersistence._userPath));
        return KeyPairPersistence._userPath;
      }
    }

    private static string MachinePath
    {
      get
      {
        lock (KeyPairPersistence.lockobj)
        {
          if (KeyPairPersistence._machinePath != null)
          {
            if (KeyPairPersistence._machinePathExists)
              goto label_7;
          }
          KeyPairPersistence._machinePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ".mono");
          KeyPairPersistence._machinePath = Path.Combine(KeyPairPersistence._machinePath, "keypairs");
          KeyPairPersistence._machinePathExists = Directory.Exists(KeyPairPersistence._machinePath);
          if (!KeyPairPersistence._machinePathExists)
          {
            try
            {
              Directory.CreateDirectory(KeyPairPersistence._machinePath);
              KeyPairPersistence.ProtectMachine(KeyPairPersistence._machinePath);
              KeyPairPersistence._machinePathExists = true;
            }
            catch (Exception ex)
            {
              throw new CryptographicException(string.Format(Locale.GetText("Could not create machine key store '{0}'."), (object) KeyPairPersistence._machinePath), ex);
            }
          }
        }
label_7:
        if (!KeyPairPersistence.IsMachineProtected(KeyPairPersistence._machinePath))
          throw new CryptographicException(string.Format(Locale.GetText("Improperly protected machine's key pairs in '{0}'."), (object) KeyPairPersistence._machinePath));
        return KeyPairPersistence._machinePath;
      }
    }

    internal static bool _CanSecure(string root)
    {
      return true;
    }

    internal static bool _ProtectUser(string path)
    {
      return true;
    }

    internal static bool _ProtectMachine(string path)
    {
      return true;
    }

    internal static bool _IsUserProtected(string path)
    {
      return true;
    }

    internal static bool _IsMachineProtected(string path)
    {
      return true;
    }

    private static bool CanSecure(string path)
    {
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Unix:
        case PlatformID.MacOSX:
        case (PlatformID) 128:
          return true;
        default:
          return KeyPairPersistence._CanSecure(Path.GetPathRoot(path));
      }
    }

    private static bool ProtectUser(string path)
    {
      return !KeyPairPersistence.CanSecure(path) || KeyPairPersistence._ProtectUser(path);
    }

    private static bool ProtectMachine(string path)
    {
      return !KeyPairPersistence.CanSecure(path) || KeyPairPersistence._ProtectMachine(path);
    }

    private static bool IsUserProtected(string path)
    {
      return !KeyPairPersistence.CanSecure(path) || KeyPairPersistence._IsUserProtected(path);
    }

    private static bool IsMachineProtected(string path)
    {
      return !KeyPairPersistence.CanSecure(path) || KeyPairPersistence._IsMachineProtected(path);
    }

    private bool CanChange
    {
      get
      {
        return this._keyvalue == null;
      }
    }

    private bool UseDefaultKeyContainer
    {
      get
      {
        return (this._params.Flags & CspProviderFlags.UseDefaultKeyContainer) == CspProviderFlags.UseDefaultKeyContainer;
      }
    }

    private bool UseMachineKeyStore
    {
      get
      {
        return (this._params.Flags & CspProviderFlags.UseMachineKeyStore) == CspProviderFlags.UseMachineKeyStore;
      }
    }

    private string ContainerName
    {
      get
      {
        if (this._container == null)
          this._container = !this.UseDefaultKeyContainer ? (this._params.KeyContainerName == null || this._params.KeyContainerName.Length == 0 ? Guid.NewGuid().ToString() : new Guid(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(this._params.KeyContainerName))).ToString()) : "default";
        return this._container;
      }
    }

    private CspParameters Copy(CspParameters p)
    {
      return new CspParameters(p.ProviderType, p.ProviderName, p.KeyContainerName)
      {
        KeyNumber = p.KeyNumber,
        Flags = p.Flags
      };
    }

    private void FromXml(string xml)
    {
      SecurityParser securityParser = new SecurityParser();
      securityParser.LoadXml(xml);
      SecurityElement xml1 = securityParser.ToXml();
      if (!(xml1.Tag == "KeyPair"))
        return;
      SecurityElement securityElement = xml1.SearchForChildByTag("KeyValue");
      if (securityElement.Children.Count <= 0)
        return;
      this._keyvalue = securityElement.Children[0].ToString();
    }

    private string ToXml()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendFormat("<KeyPair>{0}\t<Properties>{0}\t\t<Provider ", (object) Environment.NewLine);
      if (this._params.ProviderName != null && this._params.ProviderName.Length != 0)
        stringBuilder.AppendFormat("Name=\"{0}\" ", (object) this._params.ProviderName);
      stringBuilder.AppendFormat("Type=\"{0}\" />{1}\t\t<Container ", (object) this._params.ProviderType, (object) Environment.NewLine);
      stringBuilder.AppendFormat("Name=\"{0}\" />{1}\t</Properties>{1}\t<KeyValue", (object) this.ContainerName, (object) Environment.NewLine);
      if (this._params.KeyNumber != -1)
        stringBuilder.AppendFormat(" Id=\"{0}\" ", (object) this._params.KeyNumber);
      stringBuilder.AppendFormat(">{1}\t\t{0}{1}\t</KeyValue>{1}</KeyPair>{1}", (object) this.KeyValue, (object) Environment.NewLine);
      return stringBuilder.ToString();
    }
  }
}
