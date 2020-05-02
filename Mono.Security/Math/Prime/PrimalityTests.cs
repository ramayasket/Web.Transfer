// Decompiled with JetBrains decompiler
// Type: Mono.Math.Prime.PrimalityTests
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;

namespace Mono.Math.Prime
{
  public sealed class PrimalityTests
  {
    private PrimalityTests()
    {
    }

    private static int GetSPPRounds(BigInteger bi, ConfidenceFactor confidence)
    {
      int num1 = bi.BitCount();
      int num2 = num1 > 100 ? (num1 > 150 ? (num1 > 200 ? (num1 > 250 ? (num1 > 300 ? (num1 > 350 ? (num1 > 400 ? (num1 > 500 ? (num1 > 600 ? (num1 > 800 ? (num1 > 1250 ? 2 : 3) : 4) : 5) : 6) : 7) : 8) : 9) : 12) : 15) : 18) : 27;
      switch (confidence)
      {
        case ConfidenceFactor.ExtraLow:
          int num3 = num2 >> 2;
          return num3 != 0 ? num3 : 1;
        case ConfidenceFactor.Low:
          int num4 = num2 >> 1;
          return num4 != 0 ? num4 : 1;
        case ConfidenceFactor.Medium:
          return num2;
        case ConfidenceFactor.High:
          return num2 << 1;
        case ConfidenceFactor.ExtraHigh:
          return num2 << 2;
        case ConfidenceFactor.Provable:
          throw new Exception("The Rabin-Miller test can not be executed in a way such that its results are provable");
        default:
          throw new ArgumentOutOfRangeException(nameof (confidence));
      }
    }

    public static bool Test(BigInteger n, ConfidenceFactor confidence)
    {
      return n.BitCount() < 33 ? PrimalityTests.SmallPrimeSppTest(n, confidence) : PrimalityTests.RabinMillerTest(n, confidence);
    }

    public static bool RabinMillerTest(BigInteger n, ConfidenceFactor confidence)
    {
      int bits = n.BitCount();
      int sppRounds = PrimalityTests.GetSPPRounds((BigInteger) bits, confidence);
      BigInteger bigInteger1 = n - (BigInteger) 1;
      int num = bigInteger1.LowestSetBit();
      BigInteger bigInteger2 = bigInteger1 >> num;
      BigInteger.ModulusRing modulusRing = new BigInteger.ModulusRing(n);
      BigInteger a = (BigInteger) null;
      if (n.BitCount() > 100)
        a = modulusRing.Pow(2U, bigInteger2);
      for (int index1 = 0; index1 < sppRounds; ++index1)
      {
        if (index1 > 0 || a == (BigInteger) null)
        {
          BigInteger random;
          do
          {
            random = BigInteger.GenerateRandom(bits);
          }
          while (random <= (BigInteger) 2 && random >= bigInteger1);
          a = modulusRing.Pow(random, bigInteger2);
        }
        if (!(a == 1U))
        {
          for (int index2 = 0; index2 < num && a != bigInteger1; ++index2)
          {
            a = modulusRing.Pow(a, (BigInteger) 2);
            if (a == 1U)
              return false;
          }
          if (a != bigInteger1)
            return false;
        }
      }
      return true;
    }

    public static bool SmallPrimeSppTest(BigInteger bi, ConfidenceFactor confidence)
    {
      int sppRounds = PrimalityTests.GetSPPRounds(bi, confidence);
      BigInteger bigInteger1 = bi - (BigInteger) 1;
      int num = bigInteger1.LowestSetBit();
      BigInteger exp = bigInteger1 >> num;
      BigInteger.ModulusRing modulusRing = new BigInteger.ModulusRing(bi);
      for (int index1 = 0; index1 < sppRounds; ++index1)
      {
        BigInteger bigInteger2 = modulusRing.Pow(BigInteger.smallPrimes[index1], exp);
        if (!(bigInteger2 == 1U))
        {
          bool flag = false;
          for (int index2 = 0; index2 < num; ++index2)
          {
            if (bigInteger2 == bigInteger1)
            {
              flag = true;
              break;
            }
            bigInteger2 = bigInteger2 * bigInteger2 % bi;
          }
          if (!flag)
            return false;
        }
      }
      return true;
    }
  }
}
