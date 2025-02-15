using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.BBQClient
{
    public class ResultObject
    {
        public int Code { get; set; }
        public string ErrorMessage { get; set; }
        public QueryResponse Result { get; set; }
    }
}
