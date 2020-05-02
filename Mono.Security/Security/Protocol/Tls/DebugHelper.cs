// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.DebugHelper
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Diagnostics;

namespace Mono.Security.Protocol.Tls
{
  internal class DebugHelper
  {
    private static bool isInitialized;

    [Conditional("DEBUG")]
    public static void Initialize()
    {
      if (DebugHelper.isInitialized)
        return;
      Debug.Listeners.Add((TraceListener) new TextWriterTraceListener(Console.Out));
      Debug.AutoFlush = true;
      DebugHelper.isInitialized = true;
    }

    [Conditional("DEBUG")]
    public static void WriteLine(string format, params object[] args)
    {
    }

    [Conditional("DEBUG")]
    public static void WriteLine(string message)
    {
    }

    [Conditional("DEBUG")]
    public static void WriteLine(string message, byte[] buffer)
    {
    }

    [Conditional("DEBUG")]
    public static void WriteBuffer(byte[] buffer)
    {
    }

    [Conditional("DEBUG")]
    public static void WriteBuffer(byte[] buffer, int index, int length)
    {
      for (int index1 = index; index1 < length; index1 += 16)
      {
        int num = length - index1 < 16 ? length - index1 : 16;
        string str = string.Empty;
        for (int index2 = 0; index2 < num; ++index2)
          str = str + buffer[index1 + index2].ToString("x2") + " ";
      }
    }
  }
}
