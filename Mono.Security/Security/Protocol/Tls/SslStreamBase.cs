// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.SslStreamBase
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace Mono.Security.Protocol.Tls
{
  public abstract class SslStreamBase : Stream, IDisposable
  {
    private static ManualResetEvent record_processing = new ManualResetEvent(true);
    private byte[] recbuf = new byte[16384];
    private MemoryStream recordStream = new MemoryStream();
    internal Stream innerStream;
    internal MemoryStream inputBuffer;
    internal Context context;
    internal RecordProtocol protocol;
    internal bool ownsStream;
    private volatile bool disposed;
    private bool checkCertRevocationStatus;
    private object negotiate;
    private object read;
    private object write;
    private ManualResetEvent negotiationComplete;

    protected SslStreamBase(Stream stream, bool ownsStream)
    {
      if (stream == null)
        throw new ArgumentNullException("stream is null.");
      if (!stream.CanRead || !stream.CanWrite)
        throw new ArgumentNullException("stream is not both readable and writable.");
      this.inputBuffer = new MemoryStream();
      this.innerStream = stream;
      this.ownsStream = ownsStream;
      this.negotiate = new object();
      this.read = new object();
      this.write = new object();
      this.negotiationComplete = new ManualResetEvent(false);
    }

    private void AsyncHandshakeCallback(IAsyncResult asyncResult)
    {
      SslStreamBase.InternalAsyncResult asyncState = asyncResult.AsyncState as SslStreamBase.InternalAsyncResult;
      try
      {
        try
        {
          this.OnNegotiateHandshakeCallback(asyncResult);
        }
        catch (TlsException ex)
        {
          this.protocol.SendAlert(ex.Alert);
          throw new IOException("The authentication or decryption has failed.", (Exception) ex);
        }
        catch (Exception ex)
        {
          this.protocol.SendAlert(AlertDescription.InternalError);
          throw new IOException("The authentication or decryption has failed.", ex);
        }
        if (asyncState.ProceedAfterHandshake)
        {
          if (asyncState.FromWrite)
            this.InternalBeginWrite(asyncState);
          else
            this.InternalBeginRead(asyncState);
          this.negotiationComplete.Set();
        }
        else
        {
          this.negotiationComplete.Set();
          asyncState.SetComplete();
        }
      }
      catch (Exception ex)
      {
        this.negotiationComplete.Set();
        asyncState.SetComplete(ex);
      }
    }

    internal bool MightNeedHandshake
    {
      get
      {
        if (this.context.HandshakeState == HandshakeState.Finished)
          return false;
        lock (this.negotiate)
          return this.context.HandshakeState != HandshakeState.Finished;
      }
    }

    internal void NegotiateHandshake()
    {
      if (!this.MightNeedHandshake)
        return;
      SslStreamBase.InternalAsyncResult asyncResult = new SslStreamBase.InternalAsyncResult((AsyncCallback) null, (object) null, (byte[]) null, 0, 0, false, false);
      if (!this.BeginNegotiateHandshake(asyncResult))
        this.negotiationComplete.WaitOne();
      else
        this.EndNegotiateHandshake(asyncResult);
    }

    internal abstract IAsyncResult OnBeginNegotiateHandshake(
      AsyncCallback callback,
      object state);

    internal abstract void OnNegotiateHandshakeCallback(IAsyncResult asyncResult);

    internal abstract System.Security.Cryptography.X509Certificates.X509Certificate OnLocalCertificateSelection(
      System.Security.Cryptography.X509Certificates.X509CertificateCollection clientCertificates,
      System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate,
      string targetHost,
      System.Security.Cryptography.X509Certificates.X509CertificateCollection serverRequestedCertificates);

    internal abstract bool OnRemoteCertificateValidation(System.Security.Cryptography.X509Certificates.X509Certificate certificate, int[] errors);

    internal abstract ValidationResult OnRemoteCertificateValidation2(
      Mono.Security.X509.X509CertificateCollection collection);

    internal abstract bool HaveRemoteValidation2Callback { get; }

    internal abstract AsymmetricAlgorithm OnLocalPrivateKeySelection(
      System.Security.Cryptography.X509Certificates.X509Certificate certificate,
      string targetHost);

    internal System.Security.Cryptography.X509Certificates.X509Certificate RaiseLocalCertificateSelection(
      System.Security.Cryptography.X509Certificates.X509CertificateCollection certificates,
      System.Security.Cryptography.X509Certificates.X509Certificate remoteCertificate,
      string targetHost,
      System.Security.Cryptography.X509Certificates.X509CertificateCollection requestedCertificates)
    {
      return this.OnLocalCertificateSelection(certificates, remoteCertificate, targetHost, requestedCertificates);
    }

    internal bool RaiseRemoteCertificateValidation(System.Security.Cryptography.X509Certificates.X509Certificate certificate, int[] errors)
    {
      return this.OnRemoteCertificateValidation(certificate, errors);
    }

    internal ValidationResult RaiseRemoteCertificateValidation2(
      Mono.Security.X509.X509CertificateCollection collection)
    {
      return this.OnRemoteCertificateValidation2(collection);
    }

    internal AsymmetricAlgorithm RaiseLocalPrivateKeySelection(
      System.Security.Cryptography.X509Certificates.X509Certificate certificate,
      string targetHost)
    {
      return this.OnLocalPrivateKeySelection(certificate, targetHost);
    }

    public bool CheckCertRevocationStatus
    {
      get
      {
        return this.checkCertRevocationStatus;
      }
      set
      {
        this.checkCertRevocationStatus = value;
      }
    }

    public CipherAlgorithmType CipherAlgorithm
    {
      get
      {
        return this.context.HandshakeState == HandshakeState.Finished ? this.context.Current.Cipher.CipherAlgorithmType : CipherAlgorithmType.None;
      }
    }

    public int CipherStrength
    {
      get
      {
        return this.context.HandshakeState == HandshakeState.Finished ? (int) this.context.Current.Cipher.EffectiveKeyBits : 0;
      }
    }

    public HashAlgorithmType HashAlgorithm
    {
      get
      {
        return this.context.HandshakeState == HandshakeState.Finished ? this.context.Current.Cipher.HashAlgorithmType : HashAlgorithmType.None;
      }
    }

    public int HashStrength
    {
      get
      {
        return this.context.HandshakeState == HandshakeState.Finished ? this.context.Current.Cipher.HashSize * 8 : 0;
      }
    }

    public int KeyExchangeStrength
    {
      get
      {
        return this.context.HandshakeState == HandshakeState.Finished ? this.context.ServerSettings.Certificates[0].RSA.KeySize : 0;
      }
    }

    public ExchangeAlgorithmType KeyExchangeAlgorithm
    {
      get
      {
        return this.context.HandshakeState == HandshakeState.Finished ? this.context.Current.Cipher.ExchangeAlgorithmType : ExchangeAlgorithmType.None;
      }
    }

    public SecurityProtocolType SecurityProtocol
    {
      get
      {
        return this.context.HandshakeState == HandshakeState.Finished ? this.context.SecurityProtocol : (SecurityProtocolType) 0;
      }
    }

    public System.Security.Cryptography.X509Certificates.X509Certificate ServerCertificate
    {
      get
      {
        return this.context.HandshakeState == HandshakeState.Finished && this.context.ServerSettings.Certificates != null && this.context.ServerSettings.Certificates.Count > 0 ? new System.Security.Cryptography.X509Certificates.X509Certificate(this.context.ServerSettings.Certificates[0].RawData) : (System.Security.Cryptography.X509Certificates.X509Certificate) null;
      }
    }

    internal Mono.Security.X509.X509CertificateCollection ServerCertificates
    {
      get
      {
        return this.context.ServerSettings.Certificates;
      }
    }

    private bool BeginNegotiateHandshake(SslStreamBase.InternalAsyncResult asyncResult)
    {
      try
      {
        lock (this.negotiate)
        {
          if (this.context.HandshakeState != HandshakeState.None)
            return false;
          this.OnBeginNegotiateHandshake(new AsyncCallback(this.AsyncHandshakeCallback), (object) asyncResult);
          return true;
        }
      }
      catch (TlsException ex)
      {
        this.negotiationComplete.Set();
        this.protocol.SendAlert(ex.Alert);
        throw new IOException("The authentication or decryption has failed.", (Exception) ex);
      }
      catch (Exception ex)
      {
        this.negotiationComplete.Set();
        this.protocol.SendAlert(AlertDescription.InternalError);
        throw new IOException("The authentication or decryption has failed.", ex);
      }
    }

    private void EndNegotiateHandshake(SslStreamBase.InternalAsyncResult asyncResult)
    {
      if (!asyncResult.IsCompleted)
        asyncResult.AsyncWaitHandle.WaitOne();
      if (asyncResult.CompletedWithError)
        throw asyncResult.AsyncException;
    }

    public override IAsyncResult BeginRead(
      byte[] buffer,
      int offset,
      int count,
      AsyncCallback callback,
      object state)
    {
      this.checkDisposed();
      if (buffer == null)
        throw new ArgumentNullException("buffer is a null reference.");
      if (offset < 0)
        throw new ArgumentOutOfRangeException("offset is less than 0.");
      if (offset > buffer.Length)
        throw new ArgumentOutOfRangeException("offset is greater than the length of buffer.");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count is less than 0.");
      if (count > buffer.Length - offset)
        throw new ArgumentOutOfRangeException("count is less than the length of buffer minus the value of the offset parameter.");
      SslStreamBase.InternalAsyncResult asyncResult = new SslStreamBase.InternalAsyncResult(callback, state, buffer, offset, count, false, true);
      if (this.MightNeedHandshake)
      {
        if (!this.BeginNegotiateHandshake(asyncResult))
        {
          this.negotiationComplete.WaitOne();
          this.InternalBeginRead(asyncResult);
        }
      }
      else
        this.InternalBeginRead(asyncResult);
      return (IAsyncResult) asyncResult;
    }

    private void InternalBeginRead(SslStreamBase.InternalAsyncResult asyncResult)
    {
      try
      {
        int bytesRead = 0;
        lock (this.read)
        {
          bool flag1 = this.inputBuffer.Position == this.inputBuffer.Length && this.inputBuffer.Length > 0L;
          bool flag2 = this.inputBuffer.Length > 0L && asyncResult.Count > 0;
          if (flag1)
            this.resetBuffer();
          else if (flag2)
            bytesRead = this.inputBuffer.Read(asyncResult.Buffer, asyncResult.Offset, asyncResult.Count);
        }
        if (0 < bytesRead)
          asyncResult.SetComplete(bytesRead);
        else if (!this.context.ReceivedConnectionEnd)
          this.innerStream.BeginRead(this.recbuf, 0, this.recbuf.Length, new AsyncCallback(this.InternalReadCallback), (object) new object[2]
          {
            (object) this.recbuf,
            (object) asyncResult
          });
        else
          asyncResult.SetComplete(0);
      }
      catch (TlsException ex)
      {
        this.protocol.SendAlert(ex.Alert);
        throw new IOException("The authentication or decryption has failed.", (Exception) ex);
      }
      catch (Exception ex)
      {
        throw new IOException("IO exception during read.", ex);
      }
    }

    private void InternalReadCallback(IAsyncResult result)
    {
      if (this.disposed)
        return;
      object[] asyncState = (object[]) result.AsyncState;
      byte[] buffer1 = (byte[]) asyncState[0];
      SslStreamBase.InternalAsyncResult internalAsyncResult = (SslStreamBase.InternalAsyncResult) asyncState[1];
      try
      {
        int count = this.innerStream.EndRead(result);
        if (count > 0)
        {
          this.recordStream.Write(buffer1, 0, count);
          bool flag = false;
          long num = this.recordStream.Position;
          this.recordStream.Position = 0L;
          byte[] buffer2 = (byte[]) null;
          if (this.recordStream.Length >= 5L)
            buffer2 = this.protocol.ReceiveRecord((Stream) this.recordStream);
          while (buffer2 != null)
          {
            long length = this.recordStream.Length - this.recordStream.Position;
            byte[] buffer3 = (byte[]) null;
            if (length > 0L)
            {
              buffer3 = new byte[length];
              this.recordStream.Read(buffer3, 0, buffer3.Length);
            }
            lock (this.read)
            {
              long position = this.inputBuffer.Position;
              if (buffer2.Length > 0)
              {
                this.inputBuffer.Seek(0L, SeekOrigin.End);
                this.inputBuffer.Write(buffer2, 0, buffer2.Length);
                this.inputBuffer.Seek(position, SeekOrigin.Begin);
                flag = true;
              }
            }
            this.recordStream.SetLength(0L);
            buffer2 = (byte[]) null;
            if (length > 0L)
            {
              this.recordStream.Write(buffer3, 0, buffer3.Length);
              if (this.recordStream.Length >= 5L)
              {
                this.recordStream.Position = 0L;
                buffer2 = this.protocol.ReceiveRecord((Stream) this.recordStream);
                if (buffer2 == null)
                  num = this.recordStream.Length;
              }
              else
                num = length;
            }
            else
              num = 0L;
          }
          if (!flag && count > 0)
          {
            if (this.context.ReceivedConnectionEnd)
            {
              internalAsyncResult.SetComplete(0);
            }
            else
            {
              this.recordStream.Position = this.recordStream.Length;
              this.innerStream.BeginRead(buffer1, 0, buffer1.Length, new AsyncCallback(this.InternalReadCallback), (object) asyncState);
            }
          }
          else
          {
            this.recordStream.Position = num;
            int bytesRead = 0;
            lock (this.read)
              bytesRead = this.inputBuffer.Read(internalAsyncResult.Buffer, internalAsyncResult.Offset, internalAsyncResult.Count);
            internalAsyncResult.SetComplete(bytesRead);
          }
        }
        else
          internalAsyncResult.SetComplete(0);
      }
      catch (Exception ex)
      {
        internalAsyncResult.SetComplete(ex);
      }
    }

    private void InternalBeginWrite(SslStreamBase.InternalAsyncResult asyncResult)
    {
      try
      {
        lock (this.write)
        {
          byte[] buffer = this.protocol.EncodeRecord(ContentType.ApplicationData, asyncResult.Buffer, asyncResult.Offset, asyncResult.Count);
          this.innerStream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(this.InternalWriteCallback), (object) asyncResult);
        }
      }
      catch (TlsException ex)
      {
        this.protocol.SendAlert(ex.Alert);
        this.Close();
        throw new IOException("The authentication or decryption has failed.", (Exception) ex);
      }
      catch (Exception ex)
      {
        throw new IOException("IO exception during Write.", ex);
      }
    }

    private void InternalWriteCallback(IAsyncResult ar)
    {
      if (this.disposed)
        return;
      SslStreamBase.InternalAsyncResult asyncState = (SslStreamBase.InternalAsyncResult) ar.AsyncState;
      try
      {
        this.innerStream.EndWrite(ar);
        asyncState.SetComplete();
      }
      catch (Exception ex)
      {
        asyncState.SetComplete(ex);
      }
    }

    public override IAsyncResult BeginWrite(
      byte[] buffer,
      int offset,
      int count,
      AsyncCallback callback,
      object state)
    {
      this.checkDisposed();
      if (buffer == null)
        throw new ArgumentNullException("buffer is a null reference.");
      if (offset < 0)
        throw new ArgumentOutOfRangeException("offset is less than 0.");
      if (offset > buffer.Length)
        throw new ArgumentOutOfRangeException("offset is greater than the length of buffer.");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count is less than 0.");
      if (count > buffer.Length - offset)
        throw new ArgumentOutOfRangeException("count is less than the length of buffer minus the value of the offset parameter.");
      SslStreamBase.InternalAsyncResult asyncResult = new SslStreamBase.InternalAsyncResult(callback, state, buffer, offset, count, true, true);
      if (this.MightNeedHandshake)
      {
        if (!this.BeginNegotiateHandshake(asyncResult))
        {
          this.negotiationComplete.WaitOne();
          this.InternalBeginWrite(asyncResult);
        }
      }
      else
        this.InternalBeginWrite(asyncResult);
      return (IAsyncResult) asyncResult;
    }

    public override int EndRead(IAsyncResult asyncResult)
    {
      this.checkDisposed();
      if (!(asyncResult is SslStreamBase.InternalAsyncResult internalAsyncResult))
        throw new ArgumentNullException("asyncResult is null or was not obtained by calling BeginRead.");
      if (!asyncResult.IsCompleted && !asyncResult.AsyncWaitHandle.WaitOne())
        throw new TlsException(AlertDescription.InternalError, "Couldn't complete EndRead");
      if (internalAsyncResult.CompletedWithError)
        throw internalAsyncResult.AsyncException;
      return internalAsyncResult.BytesRead;
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
      this.checkDisposed();
      if (!(asyncResult is SslStreamBase.InternalAsyncResult internalAsyncResult))
        throw new ArgumentNullException("asyncResult is null or was not obtained by calling BeginWrite.");
      if (!asyncResult.IsCompleted && !internalAsyncResult.AsyncWaitHandle.WaitOne())
        throw new TlsException(AlertDescription.InternalError, "Couldn't complete EndWrite");
      if (internalAsyncResult.CompletedWithError)
        throw internalAsyncResult.AsyncException;
    }

    public override void Close()
    {
      base.Close();
    }

    public override void Flush()
    {
      this.checkDisposed();
      this.innerStream.Flush();
    }

    public int Read(byte[] buffer)
    {
      return this.Read(buffer, 0, buffer.Length);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      this.checkDisposed();
      if (buffer == null)
        throw new ArgumentNullException(nameof (buffer));
      if (offset < 0)
        throw new ArgumentOutOfRangeException("offset is less than 0.");
      if (offset > buffer.Length)
        throw new ArgumentOutOfRangeException("offset is greater than the length of buffer.");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count is less than 0.");
      if (count > buffer.Length - offset)
        throw new ArgumentOutOfRangeException("count is less than the length of buffer minus the value of the offset parameter.");
      if (this.context.HandshakeState != HandshakeState.Finished)
        this.NegotiateHandshake();
      lock (this.read)
      {
        try
        {
          SslStreamBase.record_processing.Reset();
          if (this.inputBuffer.Position > 0L)
          {
            if (this.inputBuffer.Position == this.inputBuffer.Length)
            {
              this.inputBuffer.SetLength(0L);
            }
            else
            {
              int num = this.inputBuffer.Read(buffer, offset, count);
              if (num > 0)
              {
                SslStreamBase.record_processing.Set();
                return num;
              }
            }
          }
          bool flag1 = false;
label_19:
          if (this.recordStream.Position == 0L || flag1)
          {
            flag1 = false;
            byte[] buffer1 = new byte[16384];
            int count1 = 0;
            if (count == 1)
            {
              int num = this.innerStream.ReadByte();
              if (num >= 0)
              {
                buffer1[0] = (byte) num;
                count1 = 1;
              }
            }
            else
              count1 = this.innerStream.Read(buffer1, 0, buffer1.Length);
            if (count1 > 0)
            {
              if (this.recordStream.Length > 0L && this.recordStream.Position != this.recordStream.Length)
                this.recordStream.Seek(0L, SeekOrigin.End);
              this.recordStream.Write(buffer1, 0, count1);
            }
            else
            {
              SslStreamBase.record_processing.Set();
              return 0;
            }
          }
          bool flag2 = false;
          this.recordStream.Position = 0L;
          byte[] buffer2 = (byte[]) null;
          if (this.recordStream.Length >= 5L)
          {
            buffer2 = this.protocol.ReceiveRecord((Stream) this.recordStream);
            flag1 = buffer2 == null;
          }
          while (buffer2 != null)
          {
            long length = this.recordStream.Length - this.recordStream.Position;
            byte[] buffer1 = (byte[]) null;
            if (length > 0L)
            {
              buffer1 = new byte[length];
              this.recordStream.Read(buffer1, 0, buffer1.Length);
            }
            long position = this.inputBuffer.Position;
            if (buffer2.Length > 0)
            {
              this.inputBuffer.Seek(0L, SeekOrigin.End);
              this.inputBuffer.Write(buffer2, 0, buffer2.Length);
              this.inputBuffer.Seek(position, SeekOrigin.Begin);
              flag2 = true;
            }
            this.recordStream.SetLength(0L);
            buffer2 = (byte[]) null;
            if (length > 0L)
              this.recordStream.Write(buffer1, 0, buffer1.Length);
            if (flag2)
            {
              int num = this.inputBuffer.Read(buffer, offset, count);
              SslStreamBase.record_processing.Set();
              return num;
            }
          }
          goto label_19;
        }
        catch (TlsException ex)
        {
          throw new IOException("The authentication or decryption has failed.", (Exception) ex);
        }
        catch (Exception ex)
        {
          throw new IOException("IO exception during read.", ex);
        }
      }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
      throw new NotSupportedException();
    }

    public void Write(byte[] buffer)
    {
      this.Write(buffer, 0, buffer.Length);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      this.checkDisposed();
      if (buffer == null)
        throw new ArgumentNullException(nameof (buffer));
      if (offset < 0)
        throw new ArgumentOutOfRangeException("offset is less than 0.");
      if (offset > buffer.Length)
        throw new ArgumentOutOfRangeException("offset is greater than the length of buffer.");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count is less than 0.");
      if (count > buffer.Length - offset)
        throw new ArgumentOutOfRangeException("count is less than the length of buffer minus the value of the offset parameter.");
      if (this.context.HandshakeState != HandshakeState.Finished)
        this.NegotiateHandshake();
      lock (this.write)
      {
        try
        {
          byte[] buffer1 = this.protocol.EncodeRecord(ContentType.ApplicationData, buffer, offset, count);
          this.innerStream.Write(buffer1, 0, buffer1.Length);
        }
        catch (TlsException ex)
        {
          this.protocol.SendAlert(ex.Alert);
          this.Close();
          throw new IOException("The authentication or decryption has failed.", (Exception) ex);
        }
        catch (Exception ex)
        {
          throw new IOException("IO exception during Write.", ex);
        }
      }
    }

    public override bool CanRead
    {
      get
      {
        return this.innerStream.CanRead;
      }
    }

    public override bool CanSeek
    {
      get
      {
        return false;
      }
    }

    public override bool CanWrite
    {
      get
      {
        return this.innerStream.CanWrite;
      }
    }

    public override long Length
    {
      get
      {
        throw new NotSupportedException();
      }
    }

    public override long Position
    {
      get
      {
        throw new NotSupportedException();
      }
      set
      {
        throw new NotSupportedException();
      }
    }

    ~SslStreamBase()
    {
      this.Dispose(false);
    }

    protected override void Dispose(bool disposing)
    {
      if (this.disposed)
        return;
      if (disposing)
      {
        if (this.innerStream != null)
        {
          if (this.context.HandshakeState == HandshakeState.Finished)
          {
            if (!this.context.SentConnectionEnd)
            {
              try
              {
                this.protocol.SendAlert(AlertDescription.CloseNotify);
              }
              catch
              {
              }
            }
          }
          if (this.ownsStream)
            this.innerStream.Close();
        }
        this.ownsStream = false;
        this.innerStream = (Stream) null;
      }
      this.disposed = true;
      base.Dispose(disposing);
    }

    private void resetBuffer()
    {
      this.inputBuffer.SetLength(0L);
      this.inputBuffer.Position = 0L;
    }

    internal void checkDisposed()
    {
      if (this.disposed)
        throw new ObjectDisposedException("The Stream is closed.");
    }

    private delegate void AsyncHandshakeDelegate(
      SslStreamBase.InternalAsyncResult asyncResult,
      bool fromWrite);

    private class InternalAsyncResult : IAsyncResult
    {
      private object locker = new object();
      private AsyncCallback _userCallback;
      private object _userState;
      private Exception _asyncException;
      private ManualResetEvent handle;
      private bool completed;
      private int _bytesRead;
      private bool _fromWrite;
      private bool _proceedAfterHandshake;
      private byte[] _buffer;
      private int _offset;
      private int _count;

      public InternalAsyncResult(
        AsyncCallback userCallback,
        object userState,
        byte[] buffer,
        int offset,
        int count,
        bool fromWrite,
        bool proceedAfterHandshake)
      {
        this._userCallback = userCallback;
        this._userState = userState;
        this._buffer = buffer;
        this._offset = offset;
        this._count = count;
        this._fromWrite = fromWrite;
        this._proceedAfterHandshake = proceedAfterHandshake;
      }

      public bool ProceedAfterHandshake
      {
        get
        {
          return this._proceedAfterHandshake;
        }
      }

      public bool FromWrite
      {
        get
        {
          return this._fromWrite;
        }
      }

      public byte[] Buffer
      {
        get
        {
          return this._buffer;
        }
      }

      public int Offset
      {
        get
        {
          return this._offset;
        }
      }

      public int Count
      {
        get
        {
          return this._count;
        }
      }

      public int BytesRead
      {
        get
        {
          return this._bytesRead;
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

      private void SetComplete(Exception ex, int bytesRead)
      {
        lock (this.locker)
        {
          if (this.completed)
            return;
          this.completed = true;
          this._asyncException = ex;
          this._bytesRead = bytesRead;
          if (this.handle != null)
            this.handle.Set();
        }
        if (this._userCallback == null)
          return;
        this._userCallback.BeginInvoke((IAsyncResult) this, (AsyncCallback) null, (object) null);
      }

      public void SetComplete(Exception ex)
      {
        this.SetComplete(ex, 0);
      }

      public void SetComplete(int bytesRead)
      {
        this.SetComplete((Exception) null, bytesRead);
      }

      public void SetComplete()
      {
        this.SetComplete((Exception) null, 0);
      }
    }
  }
}
