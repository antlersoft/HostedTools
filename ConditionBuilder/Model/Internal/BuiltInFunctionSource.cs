using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal {

    [Export(typeof(IFunctionSource))]
    internal class BuiltInFunctionSource : HostedObjectBase, IFunctionSource
    {
        /// <summary>
        /// Start of UnixTime epoch; DateTimeKind is Utc
        /// </summary>
        public static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        private static Func<IList<IHtExpression>, IGroupExpression> MinMaxFunctor(string n)
        {
            return expressionList =>
            {
                if (expressionList.Count > 1)
                {
                    throw new ArgumentException($"{n}() group method can only have one argument");
                }
                var expression = expressionList.Count == 1 ? expressionList[0] : null;
                return new MinMax(n == "min", expression);
            };
        }

        private static AddedGroupFunctionHolder[] _groupFunctions = new[]
        {
            new AddedGroupFunctionHolder(
                argsList => {
                    if (argsList.Count > 1)
                    {
                        throw new ArgumentException($"count() group method can only have one argument");
                    }
                    if (argsList.Count == 0) {
                        return new GroupCount();
                    }
                    return new GroupCountDistinct(argsList[0]);
                }, "count", null, true, false, "Count of group items"),
            new AddedGroupFunctionHolder(argsList => new GroupCountDistinct(argsList[0]), "countdistinct", null, true, false, "Count distinct versions of the argument in the group"),
            new AddedGroupFunctionHolder(argsList => new GroupSum(argsList[0]), "sum", null, true, false, "Sum of expression over elements in the group"),
            new AddedGroupFunctionHolder(argsList => new IfThenElseExpression(argsList), "ifthenelse", null, false, true, "if (arg1) then arg2 else arg3"),
            new AddedGroupFunctionHolder(MinMaxFunctor("min"), "min", null, true, false, "Minimum of argument over group, or if no argument, minimum value in group"),
            new AddedGroupFunctionHolder(MinMaxFunctor("max"), "max", null, true, false, "Maximum of argument over group, or if no argument, maximum value in group"),
            new AddedGroupFunctionHolder(argsList => new ArraySortExpression(argsList), "arraysort", null, true, true, "Sort array that is first argument, using any subsequent arguments as sort key within first argument"),
            new AddedGroupFunctionHolder(argsList => new RowIndex(), "rowindex", null, true, true, "Index of row in group, or in overall collection"),
            new AddedGroupFunctionHolder(argsList => new SameExpression(), "_", null, false, true, "input of the expression"),
            new AddedGroupFunctionHolder(argsList => new PreviousRowExpression(argsList), "previous", null, true, true, "The argument as evaluated for the previous row in the group, or the previous row in the group if no argument")
        };
        private static AddedFunctionHolder[] _functions = new[]
        {
            new AddedFunctionHolder(arg =>
                {
                    var result = new JsonHtValue();
                    int offset = 0;
                    foreach (string s in arg[0].AsString.Split(arg[1].AsString[0]))
                    {
                        result[offset++] = new JsonHtValue(s);
                    }
                    return result;
                }, "split", null, "split the string first argument into an array of substrings, using the second argument as a delimiter"),
            new AddedFunctionHolder(arg => JsonConvert.DeserializeObject<IHtValue>(arg[0].AsString,new JsonFactory().GetSettings()), "deserialize"),
            new AddedFunctionHolder(arg => new JsonHtValue(JsonConvert.SerializeObject(arg[0],new JsonFactory().GetSettings())), "serialize"),
            new AddedFunctionHolder(arg => new JsonHtValue(Math.Abs(arg[0].AsDouble)), "abs"),
            new AddedFunctionHolder(arg => new JsonHtValue(Math.Floor(arg[0].AsDouble)), "floor"),
            new AddedFunctionHolder(arg => new JsonHtValue(Math.Round(arg[0].AsDouble)), "round"),
            new AddedFunctionHolder(arg => new JsonHtValue(Math.Ceiling(arg[0].AsDouble)), "ceiling"),
            new AddedFunctionHolder(arg => new JsonHtValue(Math.Truncate(arg[0].AsDouble)), "truncate"),
            new AddedFunctionHolder(arg => new JsonHtValue((DateTime.UtcNow - Epoch).TotalSeconds), "datetime")
        };

        internal static IFunctionNamespace[] _result = { new BuiltInNamespace() };

        public IEnumerable<IFunctionNamespace> AvailableNamespaces => _result;

        class BuiltInNamespace : HostedObjectBase, IFunctionNamespace
        {
            internal BuiltInNamespace()
            {
                AddedGroupFunctions = new Dictionary<string, IAddedGroupFunction>();
                foreach (var f in _groupFunctions)
                {
                    AddedGroupFunctions[f.Name] = f;
                }
                AddedFunctions = new Dictionary<string, IAddedFunction>();
                foreach (var f in _functions)
                {
                    AddedFunctions[f.Name] = f;
                }
            }
            public string Name => String.Empty;
            public IDictionary<string, IAddedGroupFunction> AddedGroupFunctions { get; private set; }
            public IDictionary<string, IAddedFunction> AddedFunctions { get; private set; }
        }
    }
}