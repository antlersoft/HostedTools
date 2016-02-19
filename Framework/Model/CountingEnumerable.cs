using System.Collections;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Framework.Model
{
    public abstract class CountingEnumerable<T> : IEnumerable<T>
    {
        protected class CountingEnumerator : IEnumerator<T>
        {
            private readonly IEnumerator<T> _baseEnumerator;
            private readonly CountingEnumerable<T> _parent;
            private readonly int _stepSize;
            private int _count;
            internal CountingEnumerator(IEnumerator<T> baseEnumerator, CountingEnumerable<T> parent, int stepSize)
            {
                _baseEnumerator = baseEnumerator;
                _parent = parent;
                _stepSize = stepSize;
            }
            public T Current
            {
                get { return _baseEnumerator.Current; }
            }

            public void Dispose()
            {
                _baseEnumerator.Dispose();
            }

            object IEnumerator.Current
            {
                get { return ((IEnumerator)_baseEnumerator).Current; }
            }

            public bool MoveNext()
            {
                bool result = _baseEnumerator.MoveNext();
                if (result)
                {
                    if (++_count%_stepSize == 0)
                    {
                        _parent.WhenCounted(_count);
                    }
                }
                else
                {
                    _parent.Final(_count);
                }
                return result;
            }

            public void Reset()
            {
                _count = 0;
                _baseEnumerator.Reset();
            }
        }
        private readonly IEnumerable<T> _baseEnumerable;
        private readonly int _stepSize;

        public CountingEnumerable(IEnumerable<T> baseEnumerable, int stepSize)
        {
            _baseEnumerable = baseEnumerable;
            _stepSize = stepSize;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ConstructEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ConstructEnumerator();
        }

        protected virtual IEnumerator<T> ConstructEnumerator()
        {
            return new CountingEnumerator(_baseEnumerable.GetEnumerator(), this, _stepSize);
        }

        protected abstract void WhenCounted(int count);

        protected virtual void Final(int count)
        {
            WhenCounted(count);
        }
    }
}
