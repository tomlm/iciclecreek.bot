using System.Collections.Generic;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers
{
    public static class TokenUtils
    {
        public static List<Token> GetTokens(string value)
        {
            List<Token> tokens = new List<Token>();
#if LUCENE
            var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);
            TokenStream stream = analyzer.GetTokenStream("fullName", new StringReader(value));
            stream.Reset();
            while (stream.IncrementToken())
            {
                var term = stream.GetAttribute<ICharTermAttribute>().ToString();
                var pos = stream.GetAttribute<IOffsetAttribute>();
                tokens.Add(new Token() { Text = term, StartOffset = pos.StartOffset, EndOffset = pos.EndOffset });
            }
#else
            StringBuilder sb = new StringBuilder();
            int iStart = 0;

            for (int iCh = 0; iCh < value.Length; iCh++)
            {
                char ch = value[iCh];

                if (char.IsWhiteSpace(ch))
                {
                    if (sb.Length > 0)
                    {
                        tokens.Add(new Token() { Text = sb.ToString(), StartOffset = iStart, EndOffset = iCh });
                    }
                    sb.Clear();
                    iStart = iCh;
                }
                else if (ch != '-' && char.IsPunctuation(ch))
                {
                    // if we are building a token, add it
                    if (sb.Length > 0)
                    {
                        tokens.Add(new Token() { Text = sb.ToString(), StartOffset = iStart, EndOffset = iCh });
                    }
                    sb.Clear();

                    // add the Punctuation as a token
                    iStart = iCh;
                    tokens.Add(new Token() { Text = ch.ToString(), StartOffset = iStart, EndOffset = iCh + 1 });
                }
                else
                {
                    if (sb.Length == 0)
                    {
                        iStart = iCh;
                    }

                    sb.Append(ch);
                }
            }

            if (sb.Length > 0)
            {
                tokens.Add(new Token() { Text = sb.ToString(), StartOffset = iStart, EndOffset = value.Length });
            }
#endif
            return tokens;
        }

    }
}
