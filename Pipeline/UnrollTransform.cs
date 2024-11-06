using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using com.antlersoft.HostedTools.Framework.Model;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IStemNode))]
    public class UnrollTransform : AbstractPipelineNode, ISettingDefinitionSource, IHtValueStem
    {
        public static ISettingDefinition UnrollExpression = new SimpleSettingDefinition("UnrollExpression", "UnrollTransform", "Unroll Expression", "Expression that returns an array to unroll to multiple rows");
        public static ISettingDefinition UseAB = new SimpleSettingDefinition("UseAB", "UnrollTransform", "Output as A B", "If checked, value returned is dictionary with the original value as A and the unrolled value as B", typeof(bool), "false");

        [Import]
        public IConditionBuilder ConditionBuilder;

        public UnrollTransform()
            : base(new MenuItem("DevTools.Pipeline.Transform.Unroll", "Unroll transform", typeof(UnrollTransform).FullName, "DevTools.Pipeline.Transform"), new [] {UnrollExpression.FullKey(), UseAB.FullKey()} )
        { }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new[] {UnrollExpression, UseAB}; }
        }

        class Transform : HostedObjectBase, IHtValueTransform {
            private readonly string _unrollExpression;
            private readonly bool _useAB;
            private readonly IConditionBuilder _conditionBuilder;

            internal Transform(string unrollExpression, bool useAB, IConditionBuilder conditionBuilder) {
                _unrollExpression = unrollExpression;
                _useAB = useAB;
                _conditionBuilder = conditionBuilder;
            }
            public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
            {
                IHtExpression unrollExpression = null;
                if (_unrollExpression.Length > 0)
                {
                    unrollExpression = _conditionBuilder.ParseCondition(_unrollExpression);
                }
                var cancelable = monitor.Cast<ICancelableMonitor>();
                foreach (var row in input)
                {
                    if (cancelable!=null && cancelable.IsCanceled)
                    {
                        break;
                    }
                    IHtValue array;
                    if (unrollExpression != null)
                    {
                        array = unrollExpression.Evaluate(row);
                    }
                    else
                    {
                        array = row;
                    }
                    if (array.IsArray)
                    {
                        foreach (var element in array.AsArrayElements)
                        {
                            yield return GetOutput(element, row, _useAB);
                        }
                    }
                    else
                    {
                        yield return GetOutput(array, row, _useAB);
                    }
                }
            }

            private IHtValue GetOutput(IHtValue element, IHtValue originalRow, bool useAB)
            {
                if (! useAB)
                {
                    return element;
                }
                var result = new JsonHtValue();
                result["A"] = originalRow;
                result["B"] = element;
                return result;
            }

        }


        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            return new Transform(
                state.SettingValues[UnrollExpression.FullKey()],
                (bool)Convert.ChangeType(state.SettingValues[UseAB.FullKey()], typeof(bool)),
                ConditionBuilder
            );
        }

        public override string NodeDescription
        {
            get { return "Unroll "+UnrollExpression.Value<string>(SettingManager)+(UseAB.Value<bool>(SettingManager) ? " as A B" : String.Empty); }
        }
    }
}
