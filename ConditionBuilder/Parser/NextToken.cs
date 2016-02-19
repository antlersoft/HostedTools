using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public interface NextToken
    {
        bool next(String toParse, ref int currentIndex, List<Token> tokens);
    }

    public class SingleChar : NextToken
    {
        char _match;
        Token _symbol;

        public SingleChar(char match, Token symbol)
        {
            _match = match;
            _symbol = symbol;
        }

        public bool next(string toParse, ref int currentIndex, List<Token> tokens)
        {
            if (toParse[currentIndex] == _match)
            {
                ++currentIndex;
                tokens.Add(_symbol);
                return true;
            }
            return false;
        }
    }

    public class StringMatch : NextToken
    {
        string _match;
        Token _symbol;
        int _len;
        public StringMatch(string match, Token symbol)
        {
            _match = match;
            _symbol = symbol;
            _len = match.Length;
        }

        public bool next(string toParse, ref int currentIndex, List<Token> terms)
        {
            if (currentIndex + _len > toParse.Length)
                return false;
            char firstChar = toParse[currentIndex];
            if (toParse.IndexOf(_match, currentIndex, _len) == currentIndex && EndOfWord(firstChar, toParse, currentIndex + _len))
            {
                currentIndex += _len;
                terms.Add(_symbol);
                return true;
            }
            return false;
        }

        private static bool EndOfWord(char firstChar, string toParse, int index)
        {
            if (index >= toParse.Length)
            {
                return true;
            }
            char c = toParse[index];
            if (Char.IsLetter(firstChar) && (c == '_' || Char.IsLetterOrDigit(c)))
            {
                return false;
            }
            return true;
        }
    }

    public class RegexMatch : NextToken
    {
        Regex _match;
        Func<string, Token> _symbolGenerator;
        public RegexMatch(string regex, Func<string, Token> symbolGenerator)
        {
            _match = new Regex(regex);
            _symbolGenerator = symbolGenerator;
        }

        public RegexMatch(Regex regex, Func<string, Token> symbolGenerator)
        {
            _match = regex;
            _symbolGenerator = symbolGenerator;
        }

        public bool next(string toParse, ref int currentIndex, List<Token> terms)
        {
            var m = _match.Match(toParse, currentIndex);
            if (m.Success && m.Index == currentIndex)
            {
                currentIndex += m.Length;
                terms.Add(_symbolGenerator(m.Value));
                return true;
            }
            return false;
        }
    }
}
