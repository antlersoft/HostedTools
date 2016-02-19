using System;
using System.Collections.Generic;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Framework.Model.Setting.Internal
{
    class Setting : ISetting
    {
        private string _rawValue;
        private readonly List<string> _previousValues; 
        private readonly ISettingDefinition _definition;
        private readonly ISettingScope _scope;
        private readonly ISettingManager _settings;
        private readonly ListenerCollection<ISetting> _listeners; 

        internal Setting(ISettingManager settings, ISettingScope scope, ISettingDefinition definition, string initialValue = null, IEnumerable<string> initialPrev = null)
        {
            _settings = settings;
            _scope = scope;
            _definition = definition;
            _rawValue = initialValue ?? definition.DefaultRaw;
            _previousValues = new List<string>(definition.NumberOfPreviousValues);
            _listeners = new ListenerCollection<ISetting>();
            if (initialPrev != null)
            {
                _previousValues.AddRange(initialPrev);
            }
        }

        #region ISetting implementation
        public ISettingDefinition Definition
        {
            get { return _definition; }
        }

        public ISettingScope Scope
        {
            get { return _scope; }
        }

        public T Get<T>()
        {
            string val = GetExpanded();
            if (typeof(Enum).IsAssignableFrom(typeof(T)))
            {
                return (T)Enum.Parse(typeof (T), val);
            }
            return (T) Convert.ChangeType(val, typeof (T));
        }

        public void SetRaw(string rawValue)
        {
            _rawValue = rawValue;
            lock (_previousValues)
            {
                for (int i = 0; i < _previousValues.Count; i++)
                {
                    if (_previousValues[i] == rawValue)
                    {
                        _previousValues.RemoveAt(i);
                        break;
                    }
                }
                if (Definition.NumberOfPreviousValues > 0 && _previousValues.Count == Definition.NumberOfPreviousValues)
                {
                    _previousValues.RemoveAt(Definition.NumberOfPreviousValues - 1);
                }
                if (Definition.NumberOfPreviousValues > 0)
                {
                    _previousValues.Insert(0, rawValue);
                }
            }
            _listeners.NotifyListeners(this);
        }

        public virtual string GetRaw()
        {
            return _rawValue;
        }

        public string GetExpanded()
        {
            string result = GetRaw();
            if (Definition.UseExpansion)
            {
                result = _settings.GetExpansion(result, Scope);
            }
            return result;
        }

        public IListenerCollection<ISetting> SettingChangedListeners
        {
            get { return _listeners; }
        }

        public List<string> PreviousValues { get { return _previousValues.ToList(); } } 

        #endregion
    }
}
