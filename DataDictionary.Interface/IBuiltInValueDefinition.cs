using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.DataDictionary.Interface
{
    public enum EBuiltInType
    {
        Bool,
        Double,
        Long,
        String
    };
    /// <summary>
    /// A data definition where a value has one of the intrinsic types supported by JSON/IHtValue
    /// </summary>
    public interface IBuiltInValueDefinition : IValueDefinition
    {
        EBuiltInType BuiltInType { get; }
    }
}
