using System;

namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public class Symbol
    {
        public Symbol(string name)
        {
            Name = String.Intern(name);
        }
        public string Name { get; private set; }
        public override bool Equals(object obj)
        {
            Symbol sym = obj as Symbol;
            if (null != (object)sym)
            {
                return ReferenceEquals(sym.Name, Name);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        public override string ToString()
        {
            return Name;
        }
        public static bool operator ==(Symbol a, Symbol b)
        {
            return ReferenceEquals(a.Name, b.Name);
        }
        public static bool operator !=(Symbol a, Symbol b)
        {
            return ReferenceEquals(a.Name, b.Name);
        }
    }
}
