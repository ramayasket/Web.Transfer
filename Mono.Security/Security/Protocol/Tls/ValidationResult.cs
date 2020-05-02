// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.ValidationResult
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

namespace Mono.Security.Protocol.Tls
{
  public class ValidationResult
  {
    private bool trusted;
    private bool user_denied;
    private int error_code;

    public ValidationResult(bool trusted, bool user_denied, int error_code)
    {
      this.trusted = trusted;
      this.user_denied = user_denied;
      this.error_code = error_code;
    }

    public bool Trusted
    {
      get
      {
        return this.trusted;
      }
    }

    public bool UserDenied
    {
      get
      {
        return this.user_denied;
      }
    }

    public int ErrorCode
    {
      get
      {
        return this.error_code;
      }
    }
  }
}
