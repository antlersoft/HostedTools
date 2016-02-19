using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Model.Plugin
{
    public class NotifiedTextWriter : TextWriter
    {
        private readonly StringBuilder _builder = new StringBuilder();

        public delegate void Flushed(string str);

        public event Flushed OnFlush;

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }

        public override void Flush()
        {
            base.Flush();
            lock (_builder)
            {
                if (_builder.Length > 0)
                {
                    OnFlush(_builder.ToString());
                    _builder.Clear();
                }
            }
        }

        public override void Write(char c)
        {
            lock (_builder)
            {
                _builder.Append(c);
            }
            if (c == '\n')
            {
                Flush();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_builder.Length > 0)
            {
                Flush();
            }
            base.Dispose(disposing);
        }
    }
}
