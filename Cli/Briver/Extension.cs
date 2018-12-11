using System;
using System.Collections.Generic;
using System.Text;

namespace Briver
{
    internal static partial class Extension
    {
        private enum States
        {
            Normal,
            Empty,
            QuoteStart,
            QuoteEnd,
        }

        private enum Kinds
        {
            Normal,
            Quote,
            Empty,
        }

        private static Kinds Kind(this char c)
        {
            if (c == '"')
            {
                return Kinds.Quote;
            }
            if (char.IsWhiteSpace(c))
            {
                return Kinds.Empty;
            }
            return Kinds.Normal;
        }


        public static string[] ParseCommandLineArguments(this string line)
        {
            var state = States.Normal;
            var arg = new StringBuilder();
            var list = new List<string>();

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                switch (state)
                {
                    case States.QuoteStart:
                        switch (c.Kind())
                        {
                            case Kinds.Quote:
                                list.Add(arg.ToString());
                                arg.Clear();
                                state = States.QuoteEnd;
                                break;
                            default:
                                arg.Append(c);
                                break;
                        }
                        break;
                    case States.QuoteEnd:
                        switch (c.Kind())
                        {
                            case Kinds.Empty:
                                state = States.Empty;
                                break;
                            default:
                                throw new Exception("引号后面必须为空格");
                        }
                        break;
                    case States.Normal:
                        switch (c.Kind())
                        {
                            case Kinds.Normal:
                                arg.Append(c);
                                break;
                            case Kinds.Empty:
                                list.Add(arg.ToString());
                                arg.Clear();
                                state = States.Empty;
                                break;
                            default:
                                throw new Exception("字符后面必须为有效字符或空格");
                        }
                        break;
                    case States.Empty:
                        switch (c.Kind())
                        {
                            case Kinds.Normal:
                                state = States.Normal;
                                arg.Append(c);
                                break;
                            case Kinds.Quote:
                                state = States.QuoteStart;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (state == States.QuoteStart)
            {
                throw new Exception("引号未封闭");
            }
            if (arg.Length > 0)
            {
                list.Add(arg.ToString());
            }
            return list.ToArray();
        }

    }
}
