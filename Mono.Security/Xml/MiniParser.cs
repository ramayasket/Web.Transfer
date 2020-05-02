// Decompiled with JetBrains decompiler
// Type: Mono.Xml.MiniParser
// Assembly: Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// MVID: 535AFC59-3048-44B3-B63E-2F16A106C177
// Assembly location: C:\Program Files (x86)\FtpDav\Mono.Security.dll

using System;
using System.Collections;
using System.Globalization;
using System.Text;

namespace Mono.Xml
{
  [CLSCompliant(false)]
  public class MiniParser
  {
    private static readonly int INPUT_RANGE = 13;
    private static readonly ushort[] tbl = new ushort[262]
    {
      (ushort) 2305,
      (ushort) 43264,
      (ushort) 63616,
      (ushort) 10368,
      (ushort) 6272,
      (ushort) 14464,
      (ushort) 18560,
      (ushort) 22656,
      (ushort) 26752,
      (ushort) 34944,
      (ushort) 39040,
      (ushort) 47232,
      (ushort) 30848,
      (ushort) 2177,
      (ushort) 10498,
      (ushort) 6277,
      (ushort) 14595,
      (ushort) 18561,
      (ushort) 22657,
      (ushort) 26753,
      (ushort) 35088,
      (ushort) 39041,
      (ushort) 43137,
      (ushort) 47233,
      (ushort) 30849,
      (ushort) 64004,
      (ushort) 4352,
      (ushort) 43266,
      (ushort) 64258,
      (ushort) 2177,
      (ushort) 10369,
      (ushort) 14465,
      (ushort) 18561,
      (ushort) 22657,
      (ushort) 26753,
      (ushort) 34945,
      (ushort) 39041,
      (ushort) 47233,
      (ushort) 30849,
      (ushort) 14597,
      (ushort) 2307,
      (ushort) 10499,
      (ushort) 6403,
      (ushort) 18691,
      (ushort) 22787,
      (ushort) 26883,
      (ushort) 35075,
      (ushort) 39171,
      (ushort) 43267,
      (ushort) 47363,
      (ushort) 30979,
      (ushort) 63747,
      (ushort) 64260,
      (ushort) 8710,
      (ushort) 4615,
      (ushort) 41480,
      (ushort) 2177,
      (ushort) 14465,
      (ushort) 18561,
      (ushort) 22657,
      (ushort) 26753,
      (ushort) 34945,
      (ushort) 39041,
      (ushort) 47233,
      (ushort) 30849,
      (ushort) 6400,
      (ushort) 2307,
      (ushort) 10499,
      (ushort) 14595,
      (ushort) 18691,
      (ushort) 22787,
      (ushort) 26883,
      (ushort) 35075,
      (ushort) 39171,
      (ushort) 43267,
      (ushort) 47363,
      (ushort) 30979,
      (ushort) 63747,
      (ushort) 6400,
      (ushort) 2177,
      (ushort) 10369,
      (ushort) 14465,
      (ushort) 18561,
      (ushort) 22657,
      (ushort) 26753,
      (ushort) 34945,
      (ushort) 39041,
      (ushort) 43137,
      (ushort) 47233,
      (ushort) 30849,
      (ushort) 63617,
      (ushort) 2561,
      (ushort) 23818,
      (ushort) 11274,
      (ushort) 7178,
      (ushort) 15370,
      (ushort) 19466,
      (ushort) 27658,
      (ushort) 35850,
      (ushort) 39946,
      (ushort) 43783,
      (ushort) 48138,
      (ushort) 31754,
      (ushort) 64522,
      (ushort) 64265,
      (ushort) 8198,
      (ushort) 4103,
      (ushort) 43272,
      (ushort) 2177,
      (ushort) 14465,
      (ushort) 18561,
      (ushort) 22657,
      (ushort) 26753,
      (ushort) 34945,
      (ushort) 39041,
      (ushort) 47233,
      (ushort) 30849,
      (ushort) 64265,
      (ushort) 17163,
      (ushort) 43276,
      (ushort) 2178,
      (ushort) 10370,
      (ushort) 6274,
      (ushort) 14466,
      (ushort) 22658,
      (ushort) 26754,
      (ushort) 34946,
      (ushort) 39042,
      (ushort) 47234,
      (ushort) 30850,
      (ushort) 2317,
      (ushort) 23818,
      (ushort) 11274,
      (ushort) 7178,
      (ushort) 15370,
      (ushort) 19466,
      (ushort) 27658,
      (ushort) 35850,
      (ushort) 39946,
      (ushort) 44042,
      (ushort) 48138,
      (ushort) 31754,
      (ushort) 64522,
      (ushort) 26894,
      (ushort) 30991,
      (ushort) 43275,
      (ushort) 2180,
      (ushort) 10372,
      (ushort) 6276,
      (ushort) 14468,
      (ushort) 18564,
      (ushort) 22660,
      (ushort) 34948,
      (ushort) 39044,
      (ushort) 47236,
      (ushort) 63620,
      (ushort) 17163,
      (ushort) 43276,
      (ushort) 2178,
      (ushort) 10370,
      (ushort) 6274,
      (ushort) 14466,
      (ushort) 22658,
      (ushort) 26754,
      (ushort) 34946,
      (ushort) 39042,
      (ushort) 47234,
      (ushort) 30850,
      (ushort) 63618,
      (ushort) 9474,
      (ushort) 35088,
      (ushort) 2182,
      (ushort) 6278,
      (ushort) 14470,
      (ushort) 18566,
      (ushort) 22662,
      (ushort) 26758,
      (ushort) 39046,
      (ushort) 43142,
      (ushort) 47238,
      (ushort) 30854,
      (ushort) 63622,
      (ushort) 25617,
      (ushort) 23822,
      (ushort) 2830,
      (ushort) 11022,
      (ushort) 6926,
      (ushort) 15118,
      (ushort) 19214,
      (ushort) 35598,
      (ushort) 39694,
      (ushort) 43790,
      (ushort) 47886,
      (ushort) 31502,
      (ushort) 64270,
      (ushort) 29713,
      (ushort) 23823,
      (ushort) 2831,
      (ushort) 11023,
      (ushort) 6927,
      (ushort) 15119,
      (ushort) 19215,
      (ushort) 27407,
      (ushort) 35599,
      (ushort) 39695,
      (ushort) 43791,
      (ushort) 47887,
      (ushort) 64271,
      (ushort) 38418,
      (ushort) 6400,
      (ushort) 1555,
      (ushort) 9747,
      (ushort) 13843,
      (ushort) 17939,
      (ushort) 22035,
      (ushort) 26131,
      (ushort) 34323,
      (ushort) 42515,
      (ushort) 46611,
      (ushort) 30227,
      (ushort) 62995,
      (ushort) 8198,
      (ushort) 4103,
      (ushort) 43281,
      (ushort) 64265,
      (ushort) 2177,
      (ushort) 14465,
      (ushort) 18561,
      (ushort) 22657,
      (ushort) 26753,
      (ushort) 34945,
      (ushort) 39041,
      (ushort) 47233,
      (ushort) 30849,
      (ushort) 46858,
      (ushort) 3090,
      (ushort) 11282,
      (ushort) 7186,
      (ushort) 15378,
      (ushort) 19474,
      (ushort) 23570,
      (ushort) 27666,
      (ushort) 35858,
      (ushort) 39954,
      (ushort) 44050,
      (ushort) 31762,
      (ushort) 64530,
      (ushort) 3091,
      (ushort) 11283,
      (ushort) 7187,
      (ushort) 15379,
      (ushort) 19475,
      (ushort) 23571,
      (ushort) 27667,
      (ushort) 35859,
      (ushort) 39955,
      (ushort) 44051,
      (ushort) 48147,
      (ushort) 31763,
      (ushort) 64531,
      ushort.MaxValue,
      ushort.MaxValue
    };
    protected static string[] errors = new string[8]
    {
      "Expected element",
      "Invalid character in tag",
      "No '='",
      "Invalid character entity",
      "Invalid attr value",
      "Empty tag",
      "No end tag",
      "Bad entity ref"
    };
    protected int line;
    protected int col;
    protected int[] twoCharBuff;
    protected bool splitCData;

    public MiniParser()
    {
      this.twoCharBuff = new int[2];
      this.splitCData = false;
      this.Reset();
    }

    public void Reset()
    {
      this.line = 0;
      this.col = 0;
    }

    protected static bool StrEquals(string str, StringBuilder sb, int sbStart, int len)
    {
      if (len != str.Length)
        return false;
      for (int index = 0; index < len; ++index)
      {
        if ((int) str[index] != (int) sb[sbStart + index])
          return false;
      }
      return true;
    }

    protected void FatalErr(string descr)
    {
      throw new MiniParser.XMLError(descr, this.line, this.col);
    }

    protected static int Xlat(int charCode, int state)
    {
      int index = state * MiniParser.INPUT_RANGE;
      int num1 = System.Math.Min(MiniParser.tbl.Length - index, MiniParser.INPUT_RANGE);
      while (--num1 >= 0)
      {
        ushort num2 = MiniParser.tbl[index];
        if (charCode == (int) num2 >> 12)
          return (int) num2 & 4095;
        ++index;
      }
      return 4095;
    }

    public void Parse(MiniParser.IReader reader, MiniParser.IHandler handler)
    {
      if (reader == null)
        throw new ArgumentNullException(nameof (reader));
      if (handler == null)
        handler = (MiniParser.IHandler) new MiniParser.HandlerAdapter();
      MiniParser.AttrListImpl attrListImpl = new MiniParser.AttrListImpl();
      string name1 = (string) null;
      Stack stack = new Stack();
      string name2 = (string) null;
      this.line = 1;
      this.col = 0;
      int state = 0;
      StringBuilder sb = new StringBuilder();
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      int num1 = 0;
      handler.OnStartParsing(this);
      while (true)
      {
        int num2;
        int num3;
        do
        {
          do
          {
            int num4;
            do
            {
              int charCode;
              do
              {
                ++this.col;
                num2 = reader.Read();
                if (num2 == -1)
                {
                  if (state != 0)
                  {
                    this.FatalErr("Unexpected EOF");
                    goto label_106;
                  }
                  else
                    goto label_106;
                }
                else
                  charCode = "<>/?=&'\"![ ]\t\r\n".IndexOf((char) num2) & 15;
              }
              while (charCode == 13);
              if (charCode == 12)
                charCode = 10;
              if (charCode == 14)
              {
                this.col = 0;
                ++this.line;
                charCode = 10;
              }
              num4 = MiniParser.Xlat(charCode, state);
              state = num4 & (int) byte.MaxValue;
            }
            while (num2 == 10 && (state == 14 || state == 15));
            num3 = num4 >> 8;
            if (state >= 128)
            {
              if (state == (int) byte.MaxValue)
                this.FatalErr("State dispatch error.");
              else
                this.FatalErr(MiniParser.errors[state ^ 128]);
            }
            switch (num3)
            {
              case 0:
                handler.OnStartElement(name2, (MiniParser.IAttrList) attrListImpl);
                if (num2 != 47)
                  stack.Push((object) name2);
                else
                  handler.OnEndElement(name2);
                attrListImpl.Clear();
                continue;
              case 1:
                name2 = sb.ToString();
                sb = new StringBuilder();
                string str1 = (string) null;
                if (stack.Count == 0 || name2 != (str1 = stack.Pop() as string))
                {
                  if (str1 == null)
                    this.FatalErr("Tag stack underflow");
                  else
                    this.FatalErr(string.Format("Expected end tag '{0}' but found '{1}'", (object) name2, (object) str1));
                }
                handler.OnEndElement(name2);
                continue;
              case 2:
                name2 = sb.ToString();
                sb = new StringBuilder();
                if (num2 == 47 || num2 == 62)
                  goto case 0;
                else
                  continue;
              case 3:
                name1 = sb.ToString();
                sb = new StringBuilder();
                continue;
              case 4:
                if (name1 == null)
                  this.FatalErr("Internal error.");
                attrListImpl.Add(name1, sb.ToString());
                sb = new StringBuilder();
                name1 = (string) null;
                continue;
              case 5:
                handler.OnChars(sb.ToString());
                sb = new StringBuilder();
                continue;
              case 6:
                string str2 = "CDATA[";
                flag2 = false;
                flag3 = false;
                switch (num2)
                {
                  case 45:
                    if (reader.Read() != 45)
                      this.FatalErr("Invalid comment");
                    ++this.col;
                    flag2 = true;
                    this.twoCharBuff[0] = -1;
                    this.twoCharBuff[1] = -1;
                    continue;
                  case 91:
                    for (int index = 0; index < str2.Length; ++index)
                    {
                      if (reader.Read() != (int) str2[index])
                      {
                        this.col += index + 1;
                        break;
                      }
                    }
                    this.col += str2.Length;
                    flag1 = true;
                    continue;
                  default:
                    flag3 = true;
                    num1 = 0;
                    continue;
                }
              case 7:
                int num5 = 0;
                int num6 = 93;
                while (true)
                {
                  switch (num6)
                  {
                    case 62:
                      goto label_52;
                    case 93:
                      num6 = reader.Read();
                      ++num5;
                      continue;
                    default:
                      goto label_48;
                  }
                }
label_48:
                for (int index = 0; index < num5; ++index)
                  sb.Append(']');
                sb.Append((char) num6);
                state = 18;
                goto label_56;
label_52:
                for (int index = 0; index < num5 - 2; ++index)
                  sb.Append(']');
                flag1 = false;
label_56:
                this.col += num5;
                continue;
              case 8:
                this.FatalErr(string.Format("Error {0}", (object) state));
                continue;
              case 9:
                continue;
              case 10:
                sb = new StringBuilder();
                continue;
              case 11:
                goto label_59;
              case 12:
                goto label_60;
              case 13:
                goto label_72;
              default:
                goto label_105;
            }
          }
          while (num2 == 60);
label_59:
          sb.Append((char) num2);
          continue;
label_60:
          if (flag2)
          {
            if (num2 == 62 && this.twoCharBuff[0] == 45 && this.twoCharBuff[1] == 45)
            {
              flag2 = false;
              state = 0;
            }
            else
            {
              this.twoCharBuff[0] = this.twoCharBuff[1];
              this.twoCharBuff[1] = num2;
            }
          }
          else if (flag3)
          {
            if (num2 == 60 || num2 == 62)
              num1 ^= 1;
          }
          else
            goto label_69;
        }
        while (num2 != 62 || num1 == 0);
        flag3 = false;
        state = 0;
        continue;
label_69:
        if (this.splitCData && sb.Length > 0 && flag1)
        {
          handler.OnChars(sb.ToString());
          sb = new StringBuilder();
        }
        flag1 = false;
        sb.Append((char) num2);
        continue;
label_72:
        int num7 = reader.Read();
        int num8 = this.col + 1;
        if (num7 == 35)
        {
          int num4 = 10;
          int num5 = 0;
          int num6 = 0;
          int num9 = reader.Read();
          ++num8;
          if (num9 == 120)
          {
            num9 = reader.Read();
            ++num8;
            num4 = 16;
          }
          NumberStyles style = num4 != 16 ? NumberStyles.Integer : NumberStyles.HexNumber;
          while (true)
          {
            int num10 = -1;
            if (!char.IsNumber((char) num9))
            {
              if ("abcdef".IndexOf(char.ToLower((char) num9)) == -1)
                goto label_80;
            }
            try
            {
              num10 = int.Parse(new string((char) num9, 1), style);
            }
            catch (FormatException)
            {
              num10 = -1;
            }
label_80:
            if (num10 != -1)
            {
              num5 = num5 * num4 + num10;
              ++num6;
              num9 = reader.Read();
              ++num8;
            }
            else
              break;
          }
          if (num9 == 59 && num6 > 0)
            sb.Append((char) num5);
          else
            this.FatalErr("Bad char ref");
        }
        else
        {
          string str1 = "aglmopqstu";
          string str2 = "&'\"><";
          int index1 = 0;
          int index2 = 15;
          int num4 = 0;
          int length = sb.Length;
          while (true)
          {
            int num5;
            do
            {
              if (index1 != 15)
                index1 = str1.IndexOf((char) num7) & 15;
              if (index1 == 15)
                this.FatalErr(MiniParser.errors[7]);
              sb.Append((char) num7);
              num5 = (int) "Ｕ㾏侏ཟｸ\xE1F4⊙\xEEFF\xEEFFｏ"[index1];
              int index3 = num5 >> 4 & 15;
              int index4 = num5 & 15;
              int num6 = num5 >> 12;
              int num9 = num5 >> 8 & 15;
              num7 = reader.Read();
              ++num8;
              index1 = 15;
              if (index3 != 15 && num7 == (int) str1[index3])
              {
                if (num6 < 14)
                  index2 = num6;
                num4 = 12;
                goto label_100;
              }
              else if (index4 != 15 && num7 == (int) str1[index4])
              {
                if (num9 < 14)
                  index2 = num9;
                num4 = 8;
                goto label_100;
              }
              else if (num7 != 59)
                goto label_100;
            }
            while (index2 == 15 || num4 == 0 || (num5 >> num4 & 15) != 14);
            break;
label_100:
            index1 = 0;
          }
          int len = num8 - this.col - 1;
          if (len > 0 && len < 5 && (MiniParser.StrEquals("amp", sb, length, len) || MiniParser.StrEquals("apos", sb, length, len) || (MiniParser.StrEquals("quot", sb, length, len) || MiniParser.StrEquals("lt", sb, length, len)) || MiniParser.StrEquals("gt", sb, length, len)))
          {
            sb.Length = length;
            sb.Append(str2[index2]);
          }
          else
            this.FatalErr(MiniParser.errors[7]);
        }
        this.col = num8;
        continue;
label_105:
        this.FatalErr(string.Format("Unexpected action code - {0}.", (object) num3));
      }
label_106:
      handler.OnEndParsing(this);
    }

    public interface IReader
    {
      int Read();
    }

    public interface IAttrList
    {
      int Length { get; }

      bool IsEmpty { get; }

      string GetName(int i);

      string GetValue(int i);

      string GetValue(string name);

      void ChangeValue(string name, string newValue);

      string[] Names { get; }

      string[] Values { get; }
    }

    public interface IMutableAttrList : MiniParser.IAttrList
    {
      void Clear();

      void Add(string name, string value);

      void CopyFrom(MiniParser.IAttrList attrs);

      void Remove(int i);

      void Remove(string name);
    }

    public interface IHandler
    {
      void OnStartParsing(MiniParser parser);

      void OnStartElement(string name, MiniParser.IAttrList attrs);

      void OnEndElement(string name);

      void OnChars(string ch);

      void OnEndParsing(MiniParser parser);
    }

    public class HandlerAdapter : MiniParser.IHandler
    {
      public void OnStartParsing(MiniParser parser)
      {
      }

      public void OnStartElement(string name, MiniParser.IAttrList attrs)
      {
      }

      public void OnEndElement(string name)
      {
      }

      public void OnChars(string ch)
      {
      }

      public void OnEndParsing(MiniParser parser)
      {
      }
    }

    private enum CharKind : byte
    {
      LEFT_BR = 0,
      RIGHT_BR = 1,
      SLASH = 2,
      PI_MARK = 3,
      EQ = 4,
      AMP = 5,
      SQUOTE = 6,
      DQUOTE = 7,
      BANG = 8,
      LEFT_SQBR = 9,
      SPACE = 10, // 0x0A
      RIGHT_SQBR = 11, // 0x0B
      TAB = 12, // 0x0C
      CR = 13, // 0x0D
      EOL = 14, // 0x0E
      CHARS = 15, // 0x0F
      UNKNOWN = 31, // 0x1F
    }

    private enum ActionCode : byte
    {
      START_ELEM = 0,
      END_ELEM = 1,
      END_NAME = 2,
      SET_ATTR_NAME = 3,
      SET_ATTR_VAL = 4,
      SEND_CHARS = 5,
      START_CDATA = 6,
      END_CDATA = 7,
      ERROR = 8,
      STATE_CHANGE = 9,
      FLUSH_CHARS_STATE_CHANGE = 10, // 0x0A
      ACC_CHARS_STATE_CHANGE = 11, // 0x0B
      ACC_CDATA = 12, // 0x0C
      PROC_CHAR_REF = 13, // 0x0D
      UNKNOWN = 15, // 0x0F
    }

    public class AttrListImpl : MiniParser.IAttrList, MiniParser.IMutableAttrList
    {
      protected ArrayList names;
      protected ArrayList values;

      public AttrListImpl()
        : this(0)
      {
      }

      public AttrListImpl(int initialCapacity)
      {
        if (initialCapacity <= 0)
        {
          this.names = new ArrayList();
          this.values = new ArrayList();
        }
        else
        {
          this.names = new ArrayList(initialCapacity);
          this.values = new ArrayList(initialCapacity);
        }
      }

      public AttrListImpl(MiniParser.IAttrList attrs)
        : this(attrs == null ? 0 : attrs.Length)
      {
        if (attrs == null)
          return;
        this.CopyFrom(attrs);
      }

      public int Length
      {
        get
        {
          return this.names.Count;
        }
      }

      public bool IsEmpty
      {
        get
        {
          return this.Length != 0;
        }
      }

      public string GetName(int i)
      {
        string str = (string) null;
        if (i >= 0 && i < this.Length)
          str = this.names[i] as string;
        return str;
      }

      public string GetValue(int i)
      {
        string str = (string) null;
        if (i >= 0 && i < this.Length)
          str = this.values[i] as string;
        return str;
      }

      public string GetValue(string name)
      {
        return this.GetValue(this.names.IndexOf((object) name));
      }

      public void ChangeValue(string name, string newValue)
      {
        int index = this.names.IndexOf((object) name);
        if (index < 0 || index >= this.Length)
          return;
        this.values[index] = (object) newValue;
      }

      public string[] Names
      {
        get
        {
          return this.names.ToArray(typeof (string)) as string[];
        }
      }

      public string[] Values
      {
        get
        {
          return this.values.ToArray(typeof (string)) as string[];
        }
      }

      public void Clear()
      {
        this.names.Clear();
        this.values.Clear();
      }

      public void Add(string name, string value)
      {
        this.names.Add((object) name);
        this.values.Add((object) value);
      }

      public void Remove(int i)
      {
        if (i < 0)
          return;
        this.names.RemoveAt(i);
        this.values.RemoveAt(i);
      }

      public void Remove(string name)
      {
        this.Remove(this.names.IndexOf((object) name));
      }

      public void CopyFrom(MiniParser.IAttrList attrs)
      {
        if (attrs == null || this != attrs)
          return;
        this.Clear();
        int length = attrs.Length;
        for (int i = 0; i < length; ++i)
          this.Add(attrs.GetName(i), attrs.GetValue(i));
      }
    }

    public class XMLError : Exception
    {
      protected string descr;
      protected int line;
      protected int column;

      public XMLError()
        : this("Unknown")
      {
      }

      public XMLError(string descr)
        : this(descr, -1, -1)
      {
      }

      public XMLError(string descr, int line, int column)
        : base(descr)
      {
        this.descr = descr;
        this.line = line;
        this.column = column;
      }

      public int Line
      {
        get
        {
          return this.line;
        }
      }

      public int Column
      {
        get
        {
          return this.column;
        }
      }

      public override string ToString()
      {
        return string.Format("{0} @ (line = {1}, col = {2})", (object) this.descr, (object) this.line, (object) this.column);
      }
    }
  }
}
