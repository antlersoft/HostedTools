using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Interface {
    public class SavedFunction {
        // Artificial key
        public string Key { get; set; }
        public string Namespace { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Opaque reference to function definition information; interpretations is in CompileAdditions
        /// (or other) project
        /// </summary>
        public IHtValue Definition { get; set; }

        public SavedFunction() {
            Name = string.Empty;
            Key = string.Empty;
            Namespace = string.Empty;
            Definition = new JsonHtValue();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}