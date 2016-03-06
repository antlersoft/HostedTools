using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using Newtonsoft.Json;
using FileScope = System.Collections.Generic.Dictionary<string, com.antlersoft.HostedTools.Framework.Model.Setting.Internal.FileSetting>;
using FileDictionary = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, com.antlersoft.HostedTools.Framework.Model.Setting.Internal.FileSetting>>;

namespace com.antlersoft.HostedTools.Framework.Model.Setting.Internal
{
    [Export(typeof(ISettingManager))]
    public class SettingManager : ISettingManager, IPartImportsSatisfiedNotification, IDisposable
    {
        [ImportMany]
        public IEnumerable<ISettingDefinitionSource> Sources;

        private readonly Dictionary<string, IInternalScope> _scopes = new Dictionary<string, IInternalScope>();
        private readonly object _lock = new object();
        private FileDictionary _fileDictionary;
        private string _settingsPath;

        private static readonly Regex CalloutMatcher = new Regex(@"\{[^{}]*\}");

        #region ISettingManager Implementation
        public ISetting this[string key]
        {
            get {
                Setting setting;
                IInternalScope scope;
                lock (_lock)
                {
                    if (!TryGetValueFromKey(key, null, out scope, out setting))
                    {
                        throw new ArgumentOutOfRangeException("key", key, "Setting definition not found");
                    }
                }
                return setting;
            }
        }

        public ISettingScope Scope(string key)
        {
            lock (_lock)
            {
                return _scopes[key];
            }
        }

        public IEnumerable<ISettingScope> Scopes
        {
            get { return _scopes.Values; }
        }

        public ISetting CreateSetting(ISettingDefinition definition)
        {
            string scopeKey = definition.ScopeKey;
            if (scopeKey == EnvScope.Key)
            {
                throw new ArgumentOutOfRangeException("Can't create a setting with reserved scope "+EnvScope.Key);
            }
            Setting result = null;
            lock (_lock)
            {
                IInternalScope scope;
                if (!_scopes.TryGetValue(scopeKey, out scope))
                {
                    scope = new SettingScope(scopeKey);
                    _scopes[scopeKey] = scope;
                }
                Setting existingSetting;
                if (scope.TryGetSetting(definition.Name, out existingSetting))
                {
                    if (! existingSetting.Definition.Equals(definition))
                    {
                        throw new ArgumentOutOfRangeException("definition", existingSetting.Definition,
                                                              "Already a setting with that key");
                    }
                    return existingSetting;
                }
                FileScope fileScope;
                if (_fileDictionary.TryGetValue(scopeKey, out fileScope))
                {
                    FileSetting initialValue;
                    if (fileScope.TryGetValue(definition.Name, out initialValue))
                    {
                        result = new Setting(this, scope, definition, initialValue.rawValue, initialValue.previousValues);
                    }
                }
                if (result == null)
                {
                    result = new Setting(this, scope, definition);
                }
                scope.AddSetting(definition.Name, result);
            }
            return result;
        }

        public string GetExpansion(string unexpandedText, ISettingScope scope = null)
        {
            StringBuilder resultBuilder = new StringBuilder();
            int oldOffset = 0;
            for (Match match = CalloutMatcher.Match(unexpandedText); match.Success; match = match.NextMatch())
            {
                resultBuilder.Append(unexpandedText.Substring(oldOffset, match.Index - oldOffset));
                String keyName = match.Value.Substring(1, match.Length - 2);
                IInternalScope nextScope;
                Setting nextSetting;
                string matchText;
                lock (_lock)
                {
                    if (TryGetValueFromKey(keyName, scope as SettingScope, out nextScope, out nextSetting))
                    {
                        matchText = nextSetting.GetExpanded();
                    }
                    else
                    {
                        matchText = string.Empty;
                    }
                }
                resultBuilder.Append(matchText);
                oldOffset = match.Index + match.Length;
            }
            if (oldOffset < unexpandedText.Length)
            {
                resultBuilder.Append(unexpandedText.Substring(oldOffset, unexpandedText.Length - oldOffset));
            }
            return resultBuilder.ToString();
        }

        public virtual void Save()
        {
            using (var writer = new StreamWriter(_settingsPath))
            {
                writer.Write(SerializedPersistentSettings());
            }
        }
        #endregion

        public void Dispose()
        {
            Save();
        }

        public virtual void OnImportsSatisfied()
        {
            _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "com.antlersoft.HostedTools.Framework.Settings.json");
            lock (_lock)
            {
                _fileDictionary = ReadSettingsFile();
                _scopes[EnvScope.Key] = new EnvScope(this);
                foreach (var source in Sources)
                {
                    foreach (var def in source.Definitions)
                    {
                        CreateSetting(def);
                    }
                }
            }
        }

        protected FileDictionary FileDictionary
        {
            get { return _fileDictionary; }
            set { _fileDictionary = value; }
        }

        #region protected methods
        protected virtual string SerializedPersistentSettings()
        {
            lock (_lock)
            {
                foreach (var scope in _scopes.Values.Where(scope => scope.ScopeKey != EnvScope.Key))
                {
                    FileScope fileScope;
                    if (!_fileDictionary.TryGetValue(scope.ScopeKey, out fileScope))
                    {
                        fileScope = new FileScope();
                        _fileDictionary[scope.ScopeKey] = fileScope;
                    }
                    foreach (var setting in scope.Settings)
                    {
                        var security = setting.Definition.Cast<ISettingSecurity>();
                        if (security != null && !security.SaveToFile)
                        {
                            continue;
                        }
                        fileScope[setting.Definition.Name] = new FileSetting
                        {
                            previousValues = setting.PreviousValues,
                            rawValue = setting.GetRaw()
                        };
                    }
                }
                return JsonConvert.SerializeObject(_fileDictionary, Formatting.Indented);
            }
        }

        protected virtual FileDictionary ReadSettingsFile()
        {
            FileDictionary fileDictionary;
            if (File.Exists(_settingsPath))
            {
                using (var reader = new StreamReader(_settingsPath))
                {
                    fileDictionary = JsonConvert.DeserializeObject<FileDictionary>(reader.ReadToEnd());
                }
            }
            else
            {
                fileDictionary = new FileDictionary();
            }
            return fileDictionary;
        }
        #endregion

        #region Private methods

        private bool TryGetValueFromKey(string key, IInternalScope initialScope, out IInternalScope scope, out Setting setting)
        {
            setting = null;
            if (initialScope != null)
            {
                if (initialScope.TryGetSetting(key, out setting))
                {
                    scope = initialScope;
                    return true;
                }
            }
            string scopeKey = string.Empty;
            string settingKey;
            int scopeIndex = key.LastIndexOf('.');
            if (scopeIndex >= 0)
            {
                scopeKey = key.Substring(0, scopeIndex);
                settingKey = key.Substring(scopeIndex + 1);
            }
            else
            {
                settingKey = key;
            }
            if (! _scopes.TryGetValue(scopeKey, out scope))
            {
                return false;
            }
            return scope.TryGetSetting(settingKey, out setting);
        }
        #endregion
    }
}
