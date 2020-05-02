// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.RecordProtocol
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using Mono.Security.Protocol.Tls.Handshake;
using System;
using System.IO;
using System.Threading;

namespace Mono.Security.Protocol.Tls
{
  internal abstract class RecordProtocol
  {
    private static ManualResetEvent record_processing = new ManualResetEvent(true);
    protected Stream innerStream;
    protected Context context;

    public RecordProtocol(Stream innerStream, Context context)
    {
      this.innerStream = innerStream;
      this.context = context;
      this.context.RecordProtocol = this;
    }

    public Context Context
    {
      get
      {
        return this.context;
      }
      set
      {
        this.context = value;
      }
    }

    public virtual void SendRecord(HandshakeType type)
    {
      this.EndSendRecord(this.BeginSendRecord(type, (AsyncCallback) null, (object) null));
    }

    protected abstract void ProcessHandshakeMessage(TlsStream handMsg);

    protected virtual void ProcessChangeCipherSpec()
    {
      Context context = this.Context;
      context.ReadSequenceNumber = 0UL;
      if (context is ClientContext)
        context.EndSwitchingSecurityParameters(true);
      else
        context.StartSwitchingSecurityParameters(false);
    }

    public virtual HandshakeMessage GetMessage(HandshakeType type)
    {
      throw new NotSupportedException();
    }

    public IAsyncResult BeginReceiveRecord(
      Stream record,
      AsyncCallback callback,
      object state)
    {
      if (this.context.ReceivedConnectionEnd)
        throw new TlsException(AlertDescription.InternalError, "The session is finished and it's no longer valid.");
      RecordProtocol.record_processing.Reset();
      byte[] initialBuffer = new byte[1];
      RecordProtocol.ReceiveRecordAsyncResult recordAsyncResult = new RecordProtocol.ReceiveRecordAsyncResult(callback, state, initialBuffer, record);
      record.BeginRead(recordAsyncResult.InitialBuffer, 0, recordAsyncResult.InitialBuffer.Length, new AsyncCallback(this.InternalReceiveRecordCallback), (object) recordAsyncResult);
      return (IAsyncResult) recordAsyncResult;
    }

    private void InternalReceiveRecordCallback(IAsyncResult asyncResult)
    {
      RecordProtocol.ReceiveRecordAsyncResult asyncState = asyncResult.AsyncState as RecordProtocol.ReceiveRecordAsyncResult;
      Stream record = asyncState.Record;
      try
      {
        if (asyncState.Record.EndRead(asyncResult) == 0)
        {
          asyncState.SetComplete((byte[]) null);
        }
        else
        {
          int contentType1 = (int) asyncState.InitialBuffer[0];
          this.context.LastHandshakeMsg = HandshakeType.ClientHello;
          ContentType contentType2 = (ContentType) contentType1;
          byte[] numArray = this.ReadRecordBuffer(contentType1, record);
          if (numArray == null)
          {
            asyncState.SetComplete((byte[]) null);
          }
          else
          {
            if ((contentType2 != ContentType.Alert || numArray.Length != 2) && (this.Context.Read != null && this.Context.Read.Cipher != null))
              numArray = this.decryptRecordFragment(contentType2, numArray);
            switch (contentType2)
            {
              case ContentType.ChangeCipherSpec:
                this.ProcessChangeCipherSpec();
                goto case ContentType.ApplicationData;
              case ContentType.Alert:
                this.ProcessAlert((AlertLevel) numArray[0], (AlertDescription) numArray[1]);
                if (record.CanSeek)
                  record.SetLength(0L);
                numArray = (byte[]) null;
                goto case ContentType.ApplicationData;
              case ContentType.Handshake:
                TlsStream handMsg = new TlsStream(numArray);
                while (!handMsg.EOF)
                  this.ProcessHandshakeMessage(handMsg);
                goto case ContentType.ApplicationData;
              case ContentType.ApplicationData:
                asyncState.SetComplete(numArray);
                break;
              case (ContentType) 128:
                this.context.HandshakeMessages.Write(numArray);
                goto case ContentType.ApplicationData;
              default:
                throw new TlsException(AlertDescription.UnexpectedMessage, "Unknown record received from server.");
            }
          }
        }
      }
      catch (Exception ex)
      {
        asyncState.SetComplete(ex);
      }
    }

    public byte[] EndReceiveRecord(IAsyncResult asyncResult)
    {
      if (!(asyncResult is RecordProtocol.ReceiveRecordAsyncResult recordAsyncResult))
        throw new ArgumentException("Either the provided async result is null or was not created by this RecordProtocol.");
      if (!recordAsyncResult.IsCompleted)
        recordAsyncResult.AsyncWaitHandle.WaitOne();
      if (recordAsyncResult.CompletedWithError)
        throw recordAsyncResult.AsyncException;
      byte[] resultingBuffer = recordAsyncResult.ResultingBuffer;
      RecordProtocol.record_processing.Set();
      return resultingBuffer;
    }

    public byte[] ReceiveRecord(Stream record)
    {
      return this.EndReceiveRecord(this.BeginReceiveRecord(record, (AsyncCallback) null, (object) null));
    }

    private byte[] ReadRecordBuffer(int contentType, Stream record)
    {
      if (contentType == 128)
        return this.ReadClientHelloV2(record);
      if (!Enum.IsDefined(typeof (ContentType), (object) (ContentType) contentType))
        throw new TlsException(AlertDescription.DecodeError);
      return this.ReadStandardRecordBuffer(record);
    }

    private byte[] ReadClientHelloV2(Stream record)
    {
      int count1 = record.ReadByte();
      if (record.CanSeek && (long) (count1 + 1) > record.Length)
        return (byte[]) null;
      byte[] buffer1 = new byte[count1];
      record.Read(buffer1, 0, count1);
      if (buffer1[0] != (byte) 1)
        throw new TlsException(AlertDescription.DecodeError);
      int num = (int) buffer1[1] << 8 | (int) buffer1[2];
      int count2 = (int) buffer1[3] << 8 | (int) buffer1[4];
      int count3 = (int) buffer1[5] << 8 | (int) buffer1[6];
      int count4 = (int) buffer1[7] << 8 | (int) buffer1[8];
      int count5 = count4 <= 32 ? count4 : 32;
      byte[] buffer2 = new byte[count2];
      Buffer.BlockCopy((Array) buffer1, 9, (Array) buffer2, 0, count2);
      byte[] numArray1 = new byte[count3];
      Buffer.BlockCopy((Array) buffer1, 9 + count2, (Array) numArray1, 0, count3);
      byte[] numArray2 = new byte[count4];
      Buffer.BlockCopy((Array) buffer1, 9 + count2 + count3, (Array) numArray2, 0, count4);
      if (count4 < 16 || count2 == 0 || count2 % 3 != 0)
        throw new TlsException(AlertDescription.DecodeError);
      if (numArray1.Length > 0)
        this.context.SessionId = numArray1;
      this.Context.ChangeProtocol((short) num);
      this.ProcessCipherSpecV2Buffer(this.Context.SecurityProtocol, buffer2);
      this.context.ClientRandom = new byte[32];
      Buffer.BlockCopy((Array) numArray2, numArray2.Length - count5, (Array) this.context.ClientRandom, 32 - count5, count5);
      this.context.LastHandshakeMsg = HandshakeType.ClientHello;
      this.context.ProtocolNegotiated = true;
      return buffer1;
    }

    private byte[] ReadStandardRecordBuffer(Stream record)
    {
      byte[] buffer1 = new byte[4];
      if (record.Read(buffer1, 0, 4) != 4)
        throw new TlsException("buffer underrun");
      short num1 = (short) ((int) buffer1[0] << 8 | (int) buffer1[1]);
      short num2 = (short) ((int) buffer1[2] << 8 | (int) buffer1[3]);
      if (record.CanSeek && (long) ((int) num2 + 5) > record.Length)
        return (byte[]) null;
      int offset = 0;
      byte[] buffer2 = new byte[(int) num2];
      int num3;
      for (; offset != (int) num2; offset += num3)
      {
        num3 = record.Read(buffer2, offset, buffer2.Length - offset);
        if (num3 == 0)
          throw new TlsException(AlertDescription.CloseNotify, "Received 0 bytes from stream. It must be closed.");
      }
      if ((int) num1 != (int) this.context.Protocol && this.context.ProtocolNegotiated)
        throw new TlsException(AlertDescription.ProtocolVersion, "Invalid protocol version on message received");
      return buffer2;
    }

    private void ProcessAlert(AlertLevel alertLevel, AlertDescription alertDesc)
    {
      switch (alertLevel)
      {
        case AlertLevel.Fatal:
          throw new TlsException(alertLevel, alertDesc);
        default:
          if (alertDesc != AlertDescription.CloseNotify)
            break;
          this.context.ReceivedConnectionEnd = true;
          break;
      }
    }

    public void SendAlert(AlertDescription description)
    {
      this.SendAlert(new Alert(description));
    }

    public void SendAlert(AlertLevel level, AlertDescription description)
    {
      this.SendAlert(new Alert(level, description));
    }

    public void SendAlert(Alert alert)
    {
      AlertLevel alertLevel;
      AlertDescription alertDescription;
      bool flag;
      if (alert == null)
      {
        alertLevel = AlertLevel.Fatal;
        alertDescription = AlertDescription.InternalError;
        flag = true;
      }
      else
      {
        alertLevel = alert.Level;
        alertDescription = alert.Description;
        flag = alert.IsCloseNotify;
      }
      this.SendRecord(ContentType.Alert, new byte[2]
      {
        (byte) alertLevel,
        (byte) alertDescription
      });
      if (!flag)
        return;
      this.context.SentConnectionEnd = true;
    }

    public void SendChangeCipherSpec()
    {
      this.SendRecord(ContentType.ChangeCipherSpec, new byte[1]
      {
        (byte) 1
      });
      Context context = this.context;
      context.WriteSequenceNumber = 0UL;
      if (context is ClientContext)
        context.StartSwitchingSecurityParameters(true);
      else
        context.EndSwitchingSecurityParameters(false);
    }

    public IAsyncResult BeginSendRecord(
      HandshakeType handshakeType,
      AsyncCallback callback,
      object state)
    {
      HandshakeMessage message = this.GetMessage(handshakeType);
      message.Process();
      RecordProtocol.SendRecordAsyncResult recordAsyncResult = new RecordProtocol.SendRecordAsyncResult(callback, state, message);
      this.BeginSendRecord(message.ContentType, message.EncodeMessage(), new AsyncCallback(this.InternalSendRecordCallback), (object) recordAsyncResult);
      return (IAsyncResult) recordAsyncResult;
    }

    private void InternalSendRecordCallback(IAsyncResult ar)
    {
      RecordProtocol.SendRecordAsyncResult asyncState = ar.AsyncState as RecordProtocol.SendRecordAsyncResult;
      try
      {
        this.EndSendRecord(ar);
        asyncState.Message.Update();
        asyncState.Message.Reset();
        asyncState.SetComplete();
      }
      catch (Exception ex)
      {
        asyncState.SetComplete(ex);
      }
    }

    public IAsyncResult BeginSendRecord(
      ContentType contentType,
      byte[] recordData,
      AsyncCallback callback,
      object state)
    {
      if (this.context.SentConnectionEnd)
        throw new TlsException(AlertDescription.InternalError, "The session is finished and it's no longer valid.");
      byte[] buffer = this.EncodeRecord(contentType, recordData);
      return this.innerStream.BeginWrite(buffer, 0, buffer.Length, callback, state);
    }

    public void EndSendRecord(IAsyncResult asyncResult)
    {
      if (asyncResult is RecordProtocol.SendRecordAsyncResult)
      {
        RecordProtocol.SendRecordAsyncResult recordAsyncResult = asyncResult as RecordProtocol.SendRecordAsyncResult;
        if (!recordAsyncResult.IsCompleted)
          recordAsyncResult.AsyncWaitHandle.WaitOne();
        if (recordAsyncResult.CompletedWithError)
          throw recordAsyncResult.AsyncException;
      }
      else
        this.innerStream.EndWrite(asyncResult);
    }

    public void SendRecord(ContentType contentType, byte[] recordData)
    {
      this.EndSendRecord(this.BeginSendRecord(contentType, recordData, (AsyncCallback) null, (object) null));
    }

    public byte[] EncodeRecord(ContentType contentType, byte[] recordData)
    {
      return this.EncodeRecord(contentType, recordData, 0, recordData.Length);
    }

    public byte[] EncodeRecord(ContentType contentType, byte[] recordData, int offset, int count)
    {
      if (this.context.SentConnectionEnd)
        throw new TlsException(AlertDescription.InternalError, "The session is finished and it's no longer valid.");
      TlsStream tlsStream = new TlsStream();
      short num;
      for (int srcOffset = offset; srcOffset < offset + count; srcOffset += (int) num)
      {
        num = count + offset - srcOffset <= 16384 ? (short) (count + offset - srcOffset) : (short) 16384;
        byte[] numArray = new byte[(int) num];
        Buffer.BlockCopy((Array) recordData, srcOffset, (Array) numArray, 0, (int) num);
        if (this.Context.Write != null && this.Context.Write.Cipher != null)
          numArray = this.encryptRecordFragment(contentType, numArray);
        tlsStream.Write((byte) contentType);
        tlsStream.Write(this.context.Protocol);
        tlsStream.Write((short) numArray.Length);
        tlsStream.Write(numArray);
      }
      return tlsStream.ToArray();
    }

    private byte[] encryptRecordFragment(ContentType contentType, byte[] fragment)
    {
      byte[] mac = !(this.Context is ClientContext) ? this.context.Write.Cipher.ComputeServerRecordMAC(contentType, fragment) : this.context.Write.Cipher.ComputeClientRecordMAC(contentType, fragment);
      byte[] numArray = this.context.Write.Cipher.EncryptRecord(fragment, mac);
      ++this.context.WriteSequenceNumber;
      return numArray;
    }

    private byte[] decryptRecordFragment(ContentType contentType, byte[] fragment)
    {
      byte[] dcrFragment = (byte[]) null;
      byte[] dcrMAC = (byte[]) null;
      try
      {
        this.context.Read.Cipher.DecryptRecord(fragment, out dcrFragment, out dcrMAC);
      }
      catch
      {
        if (this.context is ServerContext)
          this.Context.RecordProtocol.SendAlert(AlertDescription.DecryptionFailed);
        throw;
      }
      if (!this.Compare(!(this.Context is ClientContext) ? this.context.Read.Cipher.ComputeClientRecordMAC(contentType, dcrFragment) : this.context.Read.Cipher.ComputeServerRecordMAC(contentType, dcrFragment), dcrMAC))
        throw new TlsException(AlertDescription.BadRecordMAC, "Bad record MAC");
      ++this.context.ReadSequenceNumber;
      return dcrFragment;
    }

    private bool Compare(byte[] array1, byte[] array2)
    {
      if (array1 == null)
        return array2 == null;
      if (array2 == null || array1.Length != array2.Length)
        return false;
      for (int index = 0; index < array1.Length; ++index)
      {
        if ((int) array1[index] != (int) array2[index])
          return false;
      }
      return true;
    }

    private void ProcessCipherSpecV2Buffer(SecurityProtocolType protocol, byte[] buffer)
    {
      TlsStream tlsStream = new TlsStream(buffer);
      string prefix = protocol != SecurityProtocolType.Ssl3 ? "TLS_" : "SSL_";
      while (tlsStream.Position < tlsStream.Length)
      {
        byte num = tlsStream.ReadByte();
        if (num == (byte) 0)
        {
          int index = this.Context.SupportedCiphers.IndexOf(tlsStream.ReadInt16());
          if (index != -1)
          {
            this.Context.Negotiating.Cipher = this.Context.SupportedCiphers[index];
            break;
          }
        }
        else
        {
          byte[] buffer1 = new byte[2];
          tlsStream.Read(buffer1, 0, buffer1.Length);
          int code = ((int) num & (int) byte.MaxValue) << 16 | ((int) buffer1[0] & (int) byte.MaxValue) << 8 | (int) buffer1[1] & (int) byte.MaxValue;
          CipherSuite cipherSuite = this.MapV2CipherCode(prefix, code);
          if (cipherSuite != null)
          {
            this.Context.Negotiating.Cipher = cipherSuite;
            break;
          }
        }
      }
      if (this.Context.Negotiating == null)
        throw new TlsException(AlertDescription.InsuficientSecurity, "Insuficient Security");
    }

    private CipherSuite MapV2CipherCode(string prefix, int code)
    {
      try
      {
        switch (code)
        {
          case 65664:
            return this.Context.SupportedCiphers[prefix + "RSA_WITH_RC4_128_MD5"];
          case 131200:
            return this.Context.SupportedCiphers[prefix + "RSA_EXPORT_WITH_RC4_40_MD5"];
          case 196736:
            return this.Context.SupportedCiphers[prefix + "RSA_EXPORT_WITH_RC2_CBC_40_MD5"];
          case 262272:
            return this.Context.SupportedCiphers[prefix + "RSA_EXPORT_WITH_RC2_CBC_40_MD5"];
          case 327808:
            return (CipherSuite) null;
          case 393280:
            return (CipherSuite) null;
          case 458944:
            return (CipherSuite) null;
          default:
            return (CipherSuite) null;
        }
      }
      catch
      {
        return (CipherSuite) null;
      }
    }

    private class ReceiveRecordAsyncResult : IAsyncResult
    {
      private object locker = new object();
      private AsyncCallback _userCallback;
      private object _userState;
      private Exception _asyncException;
      private ManualResetEvent handle;
      private byte[] _resultingBuffer;
      private Stream _record;
      private bool completed;
      private byte[] _initialBuffer;

      public ReceiveRecordAsyncResult(
        AsyncCallback userCallback,
        object userState,
        byte[] initialBuffer,
        Stream record)
      {
        this._userCallback = userCallback;
        this._userState = userState;
        this._initialBuffer = initialBuffer;
        this._record = record;
      }

      public Stream Record
      {
        get
        {
          return this._record;
        }
      }

      public byte[] ResultingBuffer
      {
        get
        {
          return this._resultingBuffer;
        }
      }

      public byte[] InitialBuffer
      {
        get
        {
          return this._initialBuffer;
        }
      }

      public object AsyncState
      {
        get
        {
          return this._userState;
        }
      }

      public Exception AsyncException
      {
        get
        {
          return this._asyncException;
        }
      }

      public bool CompletedWithError
      {
        get
        {
          return this.IsCompleted && null != this._asyncException;
        }
      }

      public WaitHandle AsyncWaitHandle
      {
        get
        {
          lock (this.locker)
          {
            if (this.handle == null)
              this.handle = new ManualResetEvent(this.completed);
          }
          return (WaitHandle) this.handle;
        }
      }

      public bool CompletedSynchronously
      {
        get
        {
          return false;
        }
      }

      public bool IsCompleted
      {
        get
        {
          lock (this.locker)
            return this.completed;
        }
      }

      private void SetComplete(Exception ex, byte[] resultingBuffer)
      {
        lock (this.locker)
        {
          if (this.completed)
            return;
          this.completed = true;
          this._asyncException = ex;
          this._resultingBuffer = resultingBuffer;
          if (this.handle != null)
            this.handle.Set();
          if (this._userCallback == null)
            return;
          this._userCallback.BeginInvoke((IAsyncResult) this, (AsyncCallback) null, (object) null);
        }
      }

      public void SetComplete(Exception ex)
      {
        this.SetComplete(ex, (byte[]) null);
      }

      public void SetComplete(byte[] resultingBuffer)
      {
        this.SetComplete((Exception) null, resultingBuffer);
      }

      public void SetComplete()
      {
        this.SetComplete((Exception) null, (byte[]) null);
      }
    }

    private class SendRecordAsyncResult : IAsyncResult
    {
      private object locker = new object();
      private AsyncCallback _userCallback;
      private object _userState;
      private Exception _asyncException;
      private ManualResetEvent handle;
      private HandshakeMessage _message;
      private bool completed;

      public SendRecordAsyncResult(
        AsyncCallback userCallback,
        object userState,
        HandshakeMessage message)
      {
        this._userCallback = userCallback;
        this._userState = userState;
        this._message = message;
      }

      public HandshakeMessage Message
      {
        get
        {
          return this._message;
        }
      }

      public object AsyncState
      {
        get
        {
          return this._userState;
        }
      }

      public Exception AsyncException
      {
        get
        {
          return this._asyncException;
        }
      }

      public bool CompletedWithError
      {
        get
        {
          return this.IsCompleted && null != this._asyncException;
        }
      }

      public WaitHandle AsyncWaitHandle
      {
        get
        {
          lock (this.locker)
          {
            if (this.handle == null)
              this.handle = new ManualResetEvent(this.completed);
          }
          return (WaitHandle) this.handle;
        }
      }

      public bool CompletedSynchronously
      {
        get
        {
          return false;
        }
      }

      public bool IsCompleted
      {
        get
        {
          lock (this.locker)
            return this.completed;
        }
      }

      public void SetComplete(Exception ex)
      {
        lock (this.locker)
        {
          if (this.completed)
            return;
          this.completed = true;
          if (this.handle != null)
            this.handle.Set();
          if (this._userCallback != null)
            this._userCallback.BeginInvoke((IAsyncResult) this, (AsyncCallback) null, (object) null);
          this._asyncException = ex;
        }
      }

      public void SetComplete()
      {
        this.SetComplete((Exception) null);
      }
    }
  }
}
