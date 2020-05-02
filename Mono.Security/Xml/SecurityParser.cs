// Decompiled with JetBrains decompiler
// Type: Mono.Xml.SecurityParser
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Collections;
using System.Security;

namespace Mono.Xml
{
  [CLSCompliant(false)]
  public class SecurityParser : MiniParser, MiniParser.IReader, MiniParser.IHandler
  {
    private SecurityElement root;
    private string xmldoc;
    private int pos;
    private SecurityElement current;
    private Stack stack;

    public SecurityParser()
    {
      this.stack = new Stack();
    }

    public void LoadXml(string xml)
    {
      this.root = (SecurityElement) null;
      this.xmldoc = xml;
      this.pos = 0;
      this.stack.Clear();
      this.Parse((MiniParser.IReader) this, (MiniParser.IHandler) this);
    }

    public SecurityElement ToXml()
    {
      return this.root;
    }

    public int Read()
    {
      return this.pos >= this.xmldoc.Length ? -1 : (int) this.xmldoc[this.pos++];
    }

    public void OnStartParsing(MiniParser parser)
    {
    }

    public void OnStartElement(string name, MiniParser.IAttrList attrs)
    {
      SecurityElement child = new SecurityElement(name);
      if (this.root == null)
      {
        this.root = child;
        this.current = child;
      }
      else
        ((SecurityElement) this.stack.Peek()).AddChild(child);
      this.stack.Push((object) child);
      this.current = child;
      int length = attrs.Length;
      for (int i = 0; i < length; ++i)
        this.current.AddAttribute(attrs.GetName(i), SecurityElement.Escape(attrs.GetValue(i)));
    }

    public void OnEndElement(string name)
    {
      this.current = (SecurityElement) this.stack.Pop();
    }

    public void OnChars(string ch)
    {
      this.current.Text = SecurityElement.Escape(ch);
    }

    public void OnEndParsing(MiniParser parser)
    {
    }
  }
}
