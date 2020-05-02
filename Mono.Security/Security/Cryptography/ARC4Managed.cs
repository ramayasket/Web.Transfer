// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.ARC4Managed
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography
{
  public class ARC4Managed : RC4, ICryptoTransform, IDisposable
  {
    private byte[] key;
    private byte[] state;
    private byte x;
    private byte y;
    private bool m_disposed;

    public ARC4Managed()
    {
      this.state = new byte[256];
      this.m_disposed = false;
    }

    ~ARC4Managed()
    {
      this.Dispose(true);
    }

    protected override void Dispose(bool disposing)
    {
      if (this.m_disposed)
        return;
      this.x = (byte) 0;
      this.y = (byte) 0;
      if (this.key != null)
      {
        Array.Clear((Array) this.key, 0, this.key.Length);
        this.key = (byte[]) null;
      }
      Array.Clear((Array) this.state, 0, this.state.Length);
      this.state = (byte[]) null;
      GC.SuppressFinalize((object) this);
      this.m_disposed = true;
    }

    public override byte[] Key
    {
      get
      {
        return (byte[]) this.key.Clone();
      }
      set
      {
        this.key = (byte[]) value.Clone();
        this.KeySetup(this.key);
      }
    }

    public bool CanReuseTransform
    {
      get
      {
        return false;
      }
    }

    public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgvIV)
    {
      this.Key = rgbKey;
      return (ICryptoTransform) this;
    }

    public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgvIV)
    {
      this.Key = rgbKey;
      return this.CreateEncryptor();
    }

    public override void GenerateIV()
    {
      this.IV = new byte[0];
    }

    public override void GenerateKey()
    {
      this.Key = KeyBuilder.Key(this.KeySizeValue >> 3);
    }

    public bool CanTransformMultipleBlocks
    {
      get
      {
        return true;
      }
    }

    public int InputBlockSize
    {
      get
      {
        return 1;
      }
    }

    public int OutputBlockSize
    {
      get
      {
        return 1;
      }
    }

    private void KeySetup(byte[] key)
    {
      byte num1 = 0;
      byte num2 = 0;
      for (int index = 0; index < 256; ++index)
        this.state[index] = (byte) index;
      this.x = (byte) 0;
      this.y = (byte) 0;
      for (int index = 0; index < 256; ++index)
      {
        num2 = (byte) ((uint) key[(int) num1] + (uint) this.state[index] + (uint) num2);
        byte num3 = this.state[index];
        this.state[index] = this.state[(int) num2];
        this.state[(int) num2] = num3;
        num1 = (byte) (((int) num1 + 1) % key.Length);
      }
    }

    private void CheckInput(byte[] inputBuffer, int inputOffset, int inputCount)
    {
      if (inputBuffer == null)
        throw new ArgumentNullException(nameof (inputBuffer));
      if (inputOffset < 0)
        throw new ArgumentOutOfRangeException(nameof (inputOffset), "< 0");
      if (inputCount < 0)
        throw new ArgumentOutOfRangeException(nameof (inputCount), "< 0");
      if (inputOffset > inputBuffer.Length - inputCount)
        throw new ArgumentException(nameof (inputBuffer), Locale.GetText("Overflow"));
    }

    public int TransformBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset)
    {
      this.CheckInput(inputBuffer, inputOffset, inputCount);
      if (outputBuffer == null)
        throw new ArgumentNullException(nameof (outputBuffer));
      if (outputOffset < 0)
        throw new ArgumentOutOfRangeException(nameof (outputOffset), "< 0");
      if (outputOffset > outputBuffer.Length - inputCount)
        throw new ArgumentException(nameof (outputBuffer), Locale.GetText("Overflow"));
      return this.InternalTransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
    }

    private int InternalTransformBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset)
    {
      for (int index = 0; index < inputCount; ++index)
      {
        ++this.x;
        this.y = (byte) ((uint) this.state[(int) this.x] + (uint) this.y);
        byte num1 = this.state[(int) this.x];
        this.state[(int) this.x] = this.state[(int) this.y];
        this.state[(int) this.y] = num1;
        byte num2 = (byte) ((uint) this.state[(int) this.x] + (uint) this.state[(int) this.y]);
        outputBuffer[outputOffset + index] = (byte) ((uint) inputBuffer[inputOffset + index] ^ (uint) this.state[(int) num2]);
      }
      return inputCount;
    }

    public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
    {
      this.CheckInput(inputBuffer, inputOffset, inputCount);
      byte[] outputBuffer = new byte[inputCount];
      this.InternalTransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
      return outputBuffer;
    }
  }
}
