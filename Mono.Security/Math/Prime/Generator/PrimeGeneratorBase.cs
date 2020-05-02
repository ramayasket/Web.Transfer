// Decompiled with JetBrains decompiler
// Type: Mono.Math.Prime.Generator.PrimeGeneratorBase
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

namespace Mono.Math.Prime.Generator
{
  public abstract class PrimeGeneratorBase
  {
    public virtual ConfidenceFactor Confidence
    {
      get
      {
        return ConfidenceFactor.Medium;
      }
    }

    public virtual PrimalityTest PrimalityTest
    {
      get
      {
        return new PrimalityTest(PrimalityTests.RabinMillerTest);
      }
    }

    public virtual int TrialDivisionBounds
    {
      get
      {
        return 4000;
      }
    }

    protected bool PostTrialDivisionTests(BigInteger bi)
    {
      return this.PrimalityTest(bi, this.Confidence);
    }

    public abstract BigInteger GenerateNewPrime(int bits);
  }
}
