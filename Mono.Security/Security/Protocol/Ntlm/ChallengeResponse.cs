// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Ntlm.ChallengeResponse
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Cryptography;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security.Protocol.Ntlm
{
  public class ChallengeResponse : IDisposable
  {
    private static byte[] magic = new byte[8]
    {
      (byte) 75,
      (byte) 71,
      (byte) 83,
      (byte) 33,
      (byte) 64,
      (byte) 35,
      (byte) 36,
      (byte) 37
    };
    private static byte[] nullEncMagic = new byte[8]
    {
      (byte) 170,
      (byte) 211,
      (byte) 180,
      (byte) 53,
      (byte) 181,
      (byte) 20,
      (byte) 4,
      (byte) 238
    };
    private bool _disposed;
    private byte[] _challenge;
    private byte[] _lmpwd;
    private byte[] _ntpwd;

    public ChallengeResponse()
    {
      this._disposed = false;
      this._lmpwd = new byte[21];
      this._ntpwd = new byte[21];
    }

    public ChallengeResponse(string password, byte[] challenge)
      : this()
    {
      this.Password = password;
      this.Challenge = challenge;
    }

    ~ChallengeResponse()
    {
      if (this._disposed)
        return;
      this.Dispose();
    }

    public string Password
    {
      get
      {
        return (string) null;
      }
      set
      {
        if (this._disposed)
          throw new ObjectDisposedException("too late");
        DES des = DES.Create();
        des.Mode = CipherMode.ECB;
        if (value == null || value.Length < 1)
        {
          Buffer.BlockCopy((Array) ChallengeResponse.nullEncMagic, 0, (Array) this._lmpwd, 0, 8);
        }
        else
        {
          des.Key = this.PasswordToKey(value, 0);
          des.CreateEncryptor().TransformBlock(ChallengeResponse.magic, 0, 8, this._lmpwd, 0);
        }
        if (value == null || value.Length < 8)
        {
          Buffer.BlockCopy((Array) ChallengeResponse.nullEncMagic, 0, (Array) this._lmpwd, 8, 8);
        }
        else
        {
          des.Key = this.PasswordToKey(value, 7);
          des.CreateEncryptor().TransformBlock(ChallengeResponse.magic, 0, 8, this._lmpwd, 8);
        }
        MD4 md4 = MD4.Create();
        byte[] buffer = value != null ? Encoding.Unicode.GetBytes(value) : new byte[0];
        byte[] hash = md4.ComputeHash(buffer);
        Buffer.BlockCopy((Array) hash, 0, (Array) this._ntpwd, 0, 16);
        Array.Clear((Array) buffer, 0, buffer.Length);
        Array.Clear((Array) hash, 0, hash.Length);
        des.Clear();
      }
    }

    public byte[] Challenge
    {
      get
      {
        return (byte[]) null;
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException(nameof (Challenge));
        if (this._disposed)
          throw new ObjectDisposedException("too late");
        this._challenge = (byte[]) value.Clone();
      }
    }

    public byte[] LM
    {
      get
      {
        if (this._disposed)
          throw new ObjectDisposedException("too late");
        return this.GetResponse(this._lmpwd);
      }
    }

    public byte[] NT
    {
      get
      {
        if (this._disposed)
          throw new ObjectDisposedException("too late");
        return this.GetResponse(this._ntpwd);
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (this._disposed)
        return;
      Array.Clear((Array) this._lmpwd, 0, this._lmpwd.Length);
      Array.Clear((Array) this._ntpwd, 0, this._ntpwd.Length);
      if (this._challenge != null)
        Array.Clear((Array) this._challenge, 0, this._challenge.Length);
      this._disposed = true;
    }

    private byte[] GetResponse(byte[] pwd)
    {
      byte[] outputBuffer = new byte[24];
      DES des = DES.Create();
      des.Mode = CipherMode.ECB;
      des.Key = this.PrepareDESKey(pwd, 0);
      des.CreateEncryptor().TransformBlock(this._challenge, 0, 8, outputBuffer, 0);
      des.Key = this.PrepareDESKey(pwd, 7);
      des.CreateEncryptor().TransformBlock(this._challenge, 0, 8, outputBuffer, 8);
      des.Key = this.PrepareDESKey(pwd, 14);
      des.CreateEncryptor().TransformBlock(this._challenge, 0, 8, outputBuffer, 16);
      return outputBuffer;
    }

    private byte[] PrepareDESKey(byte[] key56bits, int position)
    {
      return new byte[8]
      {
        key56bits[position],
        (byte) ((int) key56bits[position] << 7 | (int) key56bits[position + 1] >> 1),
        (byte) ((int) key56bits[position + 1] << 6 | (int) key56bits[position + 2] >> 2),
        (byte) ((int) key56bits[position + 2] << 5 | (int) key56bits[position + 3] >> 3),
        (byte) ((int) key56bits[position + 3] << 4 | (int) key56bits[position + 4] >> 4),
        (byte) ((int) key56bits[position + 4] << 3 | (int) key56bits[position + 5] >> 5),
        (byte) ((int) key56bits[position + 5] << 2 | (int) key56bits[position + 6] >> 6),
        (byte) ((uint) key56bits[position + 6] << 1)
      };
    }

    private byte[] PasswordToKey(string password, int position)
    {
      byte[] numArray1 = new byte[7];
      int charCount = System.Math.Min(password.Length - position, 7);
      Encoding.ASCII.GetBytes(password.ToUpper(CultureInfo.CurrentCulture), position, charCount, numArray1, 0);
      byte[] numArray2 = this.PrepareDESKey(numArray1, 0);
      Array.Clear((Array) numArray1, 0, numArray1.Length);
      return numArray2;
    }
  }
}
