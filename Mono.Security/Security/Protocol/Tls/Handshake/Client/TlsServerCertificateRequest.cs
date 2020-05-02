// Decompiled with JetBrains decompiler
// Type: Mono.Security.Protocol.Tls.Handshake.Client.TlsServerCertificateRequest
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System.Text;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
  internal class TlsServerCertificateRequest : HandshakeMessage
  {
    private ClientCertificateType[] certificateTypes;
    private string[] distinguisedNames;

    public TlsServerCertificateRequest(Context context, byte[] buffer)
      : base(context, HandshakeType.CertificateRequest, buffer)
    {
    }

    public override void Update()
    {
      base.Update();
      this.Context.ServerSettings.CertificateTypes = this.certificateTypes;
      this.Context.ServerSettings.DistinguisedNames = this.distinguisedNames;
      this.Context.ServerSettings.CertificateRequest = true;
    }

    protected override void ProcessAsSsl3()
    {
      this.ProcessAsTls1();
    }

    protected override void ProcessAsTls1()
    {
      int length = (int) this.ReadByte();
      this.certificateTypes = new ClientCertificateType[length];
      for (int index = 0; index < length; ++index)
        this.certificateTypes[index] = (ClientCertificateType) this.ReadByte();
      if (this.ReadInt16() == (short) 0)
        return;
      ASN1 asN1_1 = new ASN1(this.ReadBytes((int) this.ReadInt16()));
      this.distinguisedNames = new string[asN1_1.Count];
      for (int index = 0; index < asN1_1.Count; ++index)
      {
        ASN1 asN1_2 = new ASN1(asN1_1[index].Value);
        this.distinguisedNames[index] = Encoding.UTF8.GetString(asN1_2[1].Value);
      }
    }
  }
}
