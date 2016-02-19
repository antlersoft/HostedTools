namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    /// <summary>
    /// A piece of text that has been assigned some syntactic meaning, represented by a Symbol
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Create a token and a symbol together
        /// </summary>
        /// <param name="name">String identifying the symbol</param>
        /// <param name="val">Text associated with the token; defaults to the symbol string</param>
        public Token(string name, string val = null)
        {
            Name = new Symbol(name);
            Value = val ?? name;
        }

        /// <summary>
        /// Create a token for a symbol
        /// </summary>
        /// <param name="name">Symbol associated with the token</param>
        /// <param name="val">Text associated with the token; defaults to the symbol string</param>
        public Token(Symbol name, string val = null)
        {
            Name = name;
            Value = val ?? name.Name;
        }

        /// <summary>
        /// Symbol associated with the token
        /// </summary>
        public Symbol Name { get; private set; }
        /// <summary>
        /// Text recognize as the token
        /// </summary>
        public string Value { get; private set; }
    }
}
