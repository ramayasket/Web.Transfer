// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Alert
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

namespace Mono.Security.Protocol.Tls
{
  internal class Alert
  {
    private AlertLevel level;
    private AlertDescription description;

    public Alert(AlertDescription description)
    {
      this.inferAlertLevel();
      this.description = description;
    }

    public Alert(AlertLevel level, AlertDescription description)
    {
      this.level = level;
      this.description = description;
    }

    public AlertLevel Level
    {
      get
      {
        return this.level;
      }
    }

    public AlertDescription Description
    {
      get
      {
        return this.description;
      }
    }

    public string Message
    {
      get
      {
        return Alert.GetAlertMessage(this.description);
      }
    }

    public bool IsWarning
    {
      get
      {
        return this.level == AlertLevel.Warning;
      }
    }

    public bool IsCloseNotify
    {
      get
      {
        return this.IsWarning && this.description == AlertDescription.CloseNotify;
      }
    }

    private void inferAlertLevel()
    {
      AlertDescription description = this.description;
      switch (description)
      {
        case AlertDescription.HandshakeFailiure:
        case AlertDescription.BadCertificate:
        case AlertDescription.UnsupportedCertificate:
        case AlertDescription.CertificateRevoked:
        case AlertDescription.CertificateExpired:
        case AlertDescription.CertificateUnknown:
        case AlertDescription.IlegalParameter:
        case AlertDescription.UnknownCA:
        case AlertDescription.AccessDenied:
        case AlertDescription.DecodeError:
        case AlertDescription.DecryptError:
        case AlertDescription.ExportRestriction:
label_4:
          this.level = AlertLevel.Fatal;
          break;
        default:
          switch (description)
          {
            case AlertDescription.BadRecordMAC:
            case AlertDescription.DecryptionFailed:
            case AlertDescription.RecordOverflow:
              goto label_4;
            default:
              if (description != AlertDescription.ProtocolVersion && description != AlertDescription.InsuficientSecurity && (description == AlertDescription.CloseNotify || description != AlertDescription.UnexpectedMessage && description != AlertDescription.DecompressionFailiure && description != AlertDescription.InternalError && (description == AlertDescription.UserCancelled || description == AlertDescription.NoRenegotiation)))
              {
                this.level = AlertLevel.Warning;
                return;
              }
              goto label_4;
          }
      }
    }

    public static string GetAlertMessage(AlertDescription description)
    {
      return "The authentication or decryption has failed.";
    }
  }
}
