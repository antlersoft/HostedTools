﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;


namespace com.antlersoft.HostedTools.CommandHost
{
    class ConsoleMonitor : HostedObjectBase, ICancelableMonitor
    {
        private CancellationTokenSource _source;
        private bool _isCanceled;

        public TextWriter Writer
        {
            get { return Console.Out; }
        }

        public bool IsCanceled
        {
            get { return _isCanceled; }
            set
            {
                if (! _isCanceled && value)
                {
                    if (_source == null)
                    {
                        _isCanceled = true;
                    }
                    else
                    {
                        _source.Cancel();
                        _isCanceled = true;
                    }
                }
            }
        }

        public Exception Thrown { get; set; }

        public CancellationToken Cancellation
        {
            get
            {
                if (_source == null)
                {
                    _source = new CancellationTokenSource();
                    if (IsCanceled)
                    {
                        _source.Cancel();
                    }
                }
                return _source.Token;
            }
        }
    }
}
