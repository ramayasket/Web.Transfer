// Decompiled with JetBrains decompiler
// Type: Mono.Math.Prime.Generator.SequentialSearchPrimeGeneratorBase
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

namespace Mono.Math.Prime.Generator
{
  public class SequentialSearchPrimeGeneratorBase : PrimeGeneratorBase
  {
    protected virtual BigInteger GenerateSearchBase(int bits, object context)
    {
      BigInteger random = BigInteger.GenerateRandom(bits);
      random.SetBit(0U);
      return random;
    }

    public override BigInteger GenerateNewPrime(int bits)
    {
      return this.GenerateNewPrime(bits, (object) null);
    }

    public virtual BigInteger GenerateNewPrime(int bits, object context)
    {
      BigInteger searchBase = this.GenerateSearchBase(bits, context);
      uint num = searchBase % 3234846615U;
      int trialDivisionBounds = this.TrialDivisionBounds;
      uint[] smallPrimes = BigInteger.smallPrimes;
      while (true)
      {
        if (num % 3U != 0U && num % 5U != 0U && (num % 7U != 0U && num % 11U != 0U) && (num % 13U != 0U && num % 17U != 0U && (num % 19U != 0U && num % 23U != 0U)) && num % 29U != 0U)
        {
          for (int index = 10; index < smallPrimes.Length && (long) smallPrimes[index] <= (long) trialDivisionBounds; ++index)
          {
            if (searchBase % smallPrimes[index] == 0U)
              goto label_8;
          }
          if (this.IsPrimeAcceptable(searchBase, context) && this.PrimalityTest(searchBase, this.Confidence))
            break;
        }
label_8:
        num += 2U;
        if (num >= 3234846615U)
          num -= 3234846615U;
        searchBase.Incr2();
      }
      return searchBase;
    }

    protected virtual bool IsPrimeAcceptable(BigInteger bi, object context)
    {
      return true;
    }
  }
}
