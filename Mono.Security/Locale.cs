// Decompiled with JetBrains decompiler
// Type: Locale
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

internal sealed class Locale
{
  private Locale()
  {
  }

  public static string GetText(string msg)
  {
    return msg;
  }

  public static string GetText(string fmt, params object[] args)
  {
    return string.Format(fmt, args);
  }
}
