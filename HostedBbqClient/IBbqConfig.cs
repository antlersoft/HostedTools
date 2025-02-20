using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.BBQClient
{
    public interface IBbqConfig
    {
        bool UseLegacyService { get; }
        string WebServiceUrl { get; }

        string UserName { get; }
        string ApiKey { get; }

        List<Substitution> Substitutions { get; }
    }
}
