using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using Action = System.Action;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    class LambdaDispatch
    {
        internal static void Invoke(Action lambda)
        {
            EventHandler act = delegate { lambda.Invoke(); };
            Application.Invoke(act);
        }

        private Action _toRun;

        private LambdaDispatch(Action a)
        {
            _toRun = a;
        }

        private void RunWithObject(object o)
        {
            _toRun.Invoke();
        }

        internal static void Run(Action lambda)
        {
            ThreadPool.QueueUserWorkItem(new LambdaDispatch(lambda).RunWithObject);
        }

        internal static Task RunAsync(Action lambda, Action postLambda)
        {
            /*
                IAsyncResult result = lambda.BeginInvoke(null, null);
                return new TaskFactory().FromAsync(result, (r) => postLambda());
                */
            return Task.Run(() => {
                lambda.Invoke();
                postLambda.Invoke();
            }
                
                );
        }
    }
}
