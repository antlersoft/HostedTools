using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.Setting
{
    public interface ISettingSecurity
    {
        bool SaveToFile { get; }
        bool IsPassword { get; }
    }
}
