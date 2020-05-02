// Decompiled with JetBrains decompiler
// Type: Mono.Math.Prime.Generator.NextPrimeFinder
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Math.Prime.Generator
{
  public class NextPrimeFinder : SequentialSearchPrimeGeneratorBase
  {
    protected override BigInteger GenerateSearchBase(int bits, object Context)
    {
      if (Context == null)
        throw new ArgumentNullException(nameof (Context));
      BigInteger bigInteger = new BigInteger((BigInteger) Context);
      bigInteger.SetBit(0U);
      return bigInteger;
    }
  }
}
