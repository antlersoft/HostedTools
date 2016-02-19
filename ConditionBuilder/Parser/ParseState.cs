using System;
using System.IO;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public class ParseState
    {
        public ParseState(Dictionary<Symbol, IGoal> goals, List<Token> tokens, int tokenOffset, Type dataType)
            : this(goals, tokens, tokenOffset, dataType, null)
        {

        }

        public ParseState(Dictionary<Symbol, IGoal> goals, List<Token> tokens, int tokenOffset, Type dataType, TextWriter writer)
        {
            Goals = goals;
            Tokens = tokens;
            TokenOffset = tokenOffset;
            DataType = dataType;
            Writer = writer;
        }

        public ParseState(ParseState orig, int tokenOffset)
        {
            Goals = orig.Goals;
            Tokens = orig.Tokens;
            DataType = orig.DataType;
            TokenOffset = tokenOffset;
            Writer = orig.Writer;
        }

        public Dictionary<Symbol, IGoal> Goals { get; private set; }
        public List<Token> Tokens { get; private set; }
        public int TokenOffset { get; private set; }
        public Type DataType { get; private set; }
        public TextWriter Writer { get; private set; }

        internal void Write(string s)
        {
            if (Writer != null)
            {
                Writer.WriteLine(s);
            }
        }
    }
}
