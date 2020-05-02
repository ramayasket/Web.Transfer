// Decompiled with JetBrains decompiler
// Type: Mono.Security.Cryptography.SymmetricTransform
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography
{
  internal abstract class SymmetricTransform : ICryptoTransform, IDisposable
  {
    protected SymmetricAlgorithm algo;
    protected bool encrypt;
    private int BlockSizeByte;
    private byte[] temp;
    private byte[] temp2;
    private byte[] workBuff;
    private byte[] workout;
    private int FeedBackByte;
    private int FeedBackIter;
    private bool m_disposed;
    private bool lastBlock;
    private RandomNumberGenerator _rng;

    public SymmetricTransform(SymmetricAlgorithm symmAlgo, bool encryption, byte[] rgbIV)
    {
      this.algo = symmAlgo;
      this.encrypt = encryption;
      this.BlockSizeByte = this.algo.BlockSize >> 3;
      rgbIV = rgbIV != null ? (byte[]) rgbIV.Clone() : KeyBuilder.IV(this.BlockSizeByte);
      if (rgbIV.Length < this.BlockSizeByte)
        throw new CryptographicException(Locale.GetText("IV is too small ({0} bytes), it should be {1} bytes long.", (object) rgbIV.Length, (object) this.BlockSizeByte));
      this.temp = new byte[this.BlockSizeByte];
      Buffer.BlockCopy((Array) rgbIV, 0, (Array) this.temp, 0, System.Math.Min(this.BlockSizeByte, rgbIV.Length));
      this.temp2 = new byte[this.BlockSizeByte];
      this.FeedBackByte = this.algo.FeedbackSize >> 3;
      if (this.FeedBackByte != 0)
        this.FeedBackIter = this.BlockSizeByte / this.FeedBackByte;
      this.workBuff = new byte[this.BlockSizeByte];
      this.workout = new byte[this.BlockSizeByte];
    }

    void IDisposable.Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    ~SymmetricTransform()
    {
      this.Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this.m_disposed)
        return;
      if (disposing)
      {
        Array.Clear((Array) this.temp, 0, this.BlockSizeByte);
        this.temp = (byte[]) null;
        Array.Clear((Array) this.temp2, 0, this.BlockSizeByte);
        this.temp2 = (byte[]) null;
      }
      this.m_disposed = true;
    }

    public virtual bool CanTransformMultipleBlocks
    {
      get
      {
        return true;
      }
    }

    public virtual bool CanReuseTransform
    {
      get
      {
        return false;
      }
    }

    public virtual int InputBlockSize
    {
      get
      {
        return this.BlockSizeByte;
      }
    }

    public virtual int OutputBlockSize
    {
      get
      {
        return this.BlockSizeByte;
      }
    }

    protected virtual void Transform(byte[] input, byte[] output)
    {
      switch (this.algo.Mode)
      {
        case CipherMode.CBC:
          this.CBC(input, output);
          break;
        case CipherMode.ECB:
          this.ECB(input, output);
          break;
        case CipherMode.OFB:
          this.OFB(input, output);
          break;
        case CipherMode.CFB:
          this.CFB(input, output);
          break;
        case CipherMode.CTS:
          this.CTS(input, output);
          break;
        default:
          throw new NotImplementedException("Unkown CipherMode" + this.algo.Mode.ToString());
      }
    }

    protected abstract void ECB(byte[] input, byte[] output);

    protected virtual void CBC(byte[] input, byte[] output)
    {
      if (this.encrypt)
      {
        for (int index = 0; index < this.BlockSizeByte; ++index)
          this.temp[index] ^= input[index];
        this.ECB(this.temp, output);
        Buffer.BlockCopy((Array) output, 0, (Array) this.temp, 0, this.BlockSizeByte);
      }
      else
      {
        Buffer.BlockCopy((Array) input, 0, (Array) this.temp2, 0, this.BlockSizeByte);
        this.ECB(input, output);
        for (int index = 0; index < this.BlockSizeByte; ++index)
          output[index] ^= this.temp[index];
        Buffer.BlockCopy((Array) this.temp2, 0, (Array) this.temp, 0, this.BlockSizeByte);
      }
    }

    protected virtual void CFB(byte[] input, byte[] output)
    {
      if (this.encrypt)
      {
        for (int srcOffset = 0; srcOffset < this.FeedBackIter; ++srcOffset)
        {
          this.ECB(this.temp, this.temp2);
          for (int index = 0; index < this.FeedBackByte; ++index)
            output[index + srcOffset] = (byte) ((uint) this.temp2[index] ^ (uint) input[index + srcOffset]);
          Buffer.BlockCopy((Array) this.temp, this.FeedBackByte, (Array) this.temp, 0, this.BlockSizeByte - this.FeedBackByte);
          Buffer.BlockCopy((Array) output, srcOffset, (Array) this.temp, this.BlockSizeByte - this.FeedBackByte, this.FeedBackByte);
        }
      }
      else
      {
        for (int srcOffset = 0; srcOffset < this.FeedBackIter; ++srcOffset)
        {
          this.encrypt = true;
          this.ECB(this.temp, this.temp2);
          this.encrypt = false;
          Buffer.BlockCopy((Array) this.temp, this.FeedBackByte, (Array) this.temp, 0, this.BlockSizeByte - this.FeedBackByte);
          Buffer.BlockCopy((Array) input, srcOffset, (Array) this.temp, this.BlockSizeByte - this.FeedBackByte, this.FeedBackByte);
          for (int index = 0; index < this.FeedBackByte; ++index)
            output[index + srcOffset] = (byte) ((uint) this.temp2[index] ^ (uint) input[index + srcOffset]);
        }
      }
    }

    protected virtual void OFB(byte[] input, byte[] output)
    {
      throw new CryptographicException("OFB isn't supported by the framework");
    }

    protected virtual void CTS(byte[] input, byte[] output)
    {
      throw new CryptographicException("CTS isn't supported by the framework");
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

    public virtual int TransformBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset)
    {
      if (this.m_disposed)
        throw new ObjectDisposedException("Object is disposed");
      this.CheckInput(inputBuffer, inputOffset, inputCount);
      if (outputBuffer == null)
        throw new ArgumentNullException(nameof (outputBuffer));
      if (outputOffset < 0)
        throw new ArgumentOutOfRangeException(nameof (outputOffset), "< 0");
      int num = outputBuffer.Length - inputCount - outputOffset;
      if (!this.encrypt && 0 > num && (this.algo.Padding == PaddingMode.None || this.algo.Padding == PaddingMode.Zeros))
        throw new CryptographicException(nameof (outputBuffer), Locale.GetText("Overflow"));
      if (this.KeepLastBlock)
      {
        if (0 > num + this.BlockSizeByte)
          throw new CryptographicException(nameof (outputBuffer), Locale.GetText("Overflow"));
      }
      else if (0 > num)
      {
        if (inputBuffer.Length - inputOffset - outputBuffer.Length != this.BlockSizeByte)
          throw new CryptographicException(nameof (outputBuffer), Locale.GetText("Overflow"));
        inputCount = outputBuffer.Length - outputOffset;
      }
      return this.InternalTransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
    }

    private bool KeepLastBlock
    {
      get
      {
        return !this.encrypt && this.algo.Padding != PaddingMode.None && this.algo.Padding != PaddingMode.Zeros;
      }
    }

    private int InternalTransformBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset)
    {
      int srcOffset = inputOffset;
      int num1;
      if (inputCount != this.BlockSizeByte)
      {
        if (inputCount % this.BlockSizeByte != 0)
          throw new CryptographicException("Invalid input block size.");
        num1 = inputCount / this.BlockSizeByte;
      }
      else
        num1 = 1;
      if (this.KeepLastBlock)
        --num1;
      int num2 = 0;
      if (this.lastBlock)
      {
        this.Transform(this.workBuff, this.workout);
        Buffer.BlockCopy((Array) this.workout, 0, (Array) outputBuffer, outputOffset, this.BlockSizeByte);
        outputOffset += this.BlockSizeByte;
        num2 += this.BlockSizeByte;
        this.lastBlock = false;
      }
      for (int index = 0; index < num1; ++index)
      {
        Buffer.BlockCopy((Array) inputBuffer, srcOffset, (Array) this.workBuff, 0, this.BlockSizeByte);
        this.Transform(this.workBuff, this.workout);
        Buffer.BlockCopy((Array) this.workout, 0, (Array) outputBuffer, outputOffset, this.BlockSizeByte);
        srcOffset += this.BlockSizeByte;
        outputOffset += this.BlockSizeByte;
        num2 += this.BlockSizeByte;
      }
      if (this.KeepLastBlock)
      {
        Buffer.BlockCopy((Array) inputBuffer, srcOffset, (Array) this.workBuff, 0, this.BlockSizeByte);
        this.lastBlock = true;
      }
      return num2;
    }

    private void Random(byte[] buffer, int start, int length)
    {
      if (this._rng == null)
        this._rng = RandomNumberGenerator.Create();
      byte[] data = new byte[length];
      this._rng.GetBytes(data);
      Buffer.BlockCopy((Array) data, 0, (Array) buffer, start, length);
    }

    private void ThrowBadPaddingException(PaddingMode padding, int length, int position)
    {
      string message = string.Format(Locale.GetText("Bad {0} padding."), (object) padding);
      if (length >= 0)
        message += string.Format(Locale.GetText(" Invalid length {0}."), (object) length);
      if (position >= 0)
        message += string.Format(Locale.GetText(" Error found at position {0}."), (object) position);
      throw new CryptographicException(message);
    }

    private byte[] FinalEncrypt(byte[] inputBuffer, int inputOffset, int inputCount)
    {
      int num1 = inputCount / this.BlockSizeByte * this.BlockSizeByte;
      int count = inputCount - num1;
      int length1 = num1;
      switch (this.algo.Padding)
      {
        case PaddingMode.PKCS7:
        case PaddingMode.ANSIX923:
        case PaddingMode.ISO10126:
          length1 += this.BlockSizeByte;
          break;
        default:
          if (inputCount == 0)
            return new byte[0];
          if (count != 0)
          {
            if (this.algo.Padding == PaddingMode.None)
              throw new CryptographicException("invalid block length");
            byte[] numArray = new byte[num1 + this.BlockSizeByte];
            Buffer.BlockCopy((Array) inputBuffer, inputOffset, (Array) numArray, 0, inputCount);
            inputBuffer = numArray;
            inputOffset = 0;
            inputCount = numArray.Length;
            length1 = inputCount;
            break;
          }
          break;
      }
      byte[] numArray1 = new byte[length1];
      int outputOffset = 0;
      for (; length1 > this.BlockSizeByte; length1 -= this.BlockSizeByte)
      {
        this.InternalTransformBlock(inputBuffer, inputOffset, this.BlockSizeByte, numArray1, outputOffset);
        inputOffset += this.BlockSizeByte;
        outputOffset += this.BlockSizeByte;
      }
      byte num2 = (byte) (this.BlockSizeByte - count);
      switch (this.algo.Padding)
      {
        case PaddingMode.PKCS7:
          int length2 = numArray1.Length;
          while (--length2 >= numArray1.Length - (int) num2)
            numArray1[length2] = num2;
          Buffer.BlockCopy((Array) inputBuffer, inputOffset, (Array) numArray1, num1, count);
          this.InternalTransformBlock(numArray1, num1, this.BlockSizeByte, numArray1, num1);
          break;
        case PaddingMode.ANSIX923:
          numArray1[numArray1.Length - 1] = num2;
          Buffer.BlockCopy((Array) inputBuffer, inputOffset, (Array) numArray1, num1, count);
          this.InternalTransformBlock(numArray1, num1, this.BlockSizeByte, numArray1, num1);
          break;
        case PaddingMode.ISO10126:
          this.Random(numArray1, numArray1.Length - (int) num2, (int) num2 - 1);
          numArray1[numArray1.Length - 1] = num2;
          Buffer.BlockCopy((Array) inputBuffer, inputOffset, (Array) numArray1, num1, count);
          this.InternalTransformBlock(numArray1, num1, this.BlockSizeByte, numArray1, num1);
          break;
        default:
          this.InternalTransformBlock(inputBuffer, inputOffset, this.BlockSizeByte, numArray1, outputOffset);
          break;
      }
      return numArray1;
    }

    private byte[] FinalDecrypt(byte[] inputBuffer, int inputOffset, int inputCount)
    {
      if (inputCount % this.BlockSizeByte > 0)
        throw new CryptographicException("Invalid input block size.");
      int count = inputCount;
      if (this.lastBlock)
        count += this.BlockSizeByte;
      byte[] outputBuffer = new byte[count];
      int num1 = 0;
      for (; inputCount > 0; inputCount -= this.BlockSizeByte)
      {
        int num2 = this.InternalTransformBlock(inputBuffer, inputOffset, this.BlockSizeByte, outputBuffer, num1);
        inputOffset += this.BlockSizeByte;
        num1 += num2;
      }
      if (this.lastBlock)
      {
        this.Transform(this.workBuff, this.workout);
        Buffer.BlockCopy((Array) this.workout, 0, (Array) outputBuffer, num1, this.BlockSizeByte);
        int num2 = num1 + this.BlockSizeByte;
        this.lastBlock = false;
      }
      byte num3 = count <= 0 ? (byte) 0 : outputBuffer[count - 1];
      switch (this.algo.Padding)
      {
        case PaddingMode.PKCS7:
          if (num3 == (byte) 0 || (int) num3 > this.BlockSizeByte)
            this.ThrowBadPaddingException(this.algo.Padding, (int) num3, -1);
          for (int position = (int) num3 - 1; position > 0; --position)
          {
            if ((int) outputBuffer[count - 1 - position] != (int) num3)
              this.ThrowBadPaddingException(this.algo.Padding, -1, position);
          }
          count -= (int) num3;
          break;
        case PaddingMode.ANSIX923:
          if (num3 == (byte) 0 || (int) num3 > this.BlockSizeByte)
            this.ThrowBadPaddingException(this.algo.Padding, (int) num3, -1);
          for (int position = (int) num3 - 1; position > 0; --position)
          {
            if (outputBuffer[count - 1 - position] != (byte) 0)
              this.ThrowBadPaddingException(this.algo.Padding, -1, position);
          }
          count -= (int) num3;
          break;
        case PaddingMode.ISO10126:
          if (num3 == (byte) 0 || (int) num3 > this.BlockSizeByte)
            this.ThrowBadPaddingException(this.algo.Padding, (int) num3, -1);
          count -= (int) num3;
          break;
      }
      if (count <= 0)
        return new byte[0];
      byte[] numArray = new byte[count];
      Buffer.BlockCopy((Array) outputBuffer, 0, (Array) numArray, 0, count);
      Array.Clear((Array) outputBuffer, 0, outputBuffer.Length);
      return numArray;
    }

    public virtual byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
    {
      if (this.m_disposed)
        throw new ObjectDisposedException("Object is disposed");
      this.CheckInput(inputBuffer, inputOffset, inputCount);
      return this.encrypt ? this.FinalEncrypt(inputBuffer, inputOffset, inputCount) : this.FinalDecrypt(inputBuffer, inputOffset, inputCount);
    }
  }
}
