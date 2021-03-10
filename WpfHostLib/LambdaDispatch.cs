using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace com.antlersoft.HostedTools.WpfHostLib
{
    class LambdaDispatch
    {
        delegate void Act0();
        internal static void Invoke(Dispatcher dispatcher, Action lambda)
        {
            Act0 act = lambda.Invoke;
            dispatcher.Invoke(act);
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
