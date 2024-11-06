using System.Collections;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Pipeline {
    public interface IRuntimeStateSettings {
        IEnumerable<string> RuntimeSettingKeys { get;}
    }
}