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

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IHtValueTransform))]
    public class UnrollTransform : EditOnlyPlugin, ISettingDefinitionSource, IHtValueTransform
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

        public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
        {
            IHtExpression unrollExpression = null;
            string unrollText = UnrollExpression.Value<string>(SettingManager);
            if (unrollText.Length > 0)
            {
                unrollExpression = ConditionBuilder.ParseCondition(unrollText);
            }
            bool useAB = UseAB.Value<bool>(SettingManager);
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
                        yield return GetOutput(element, row, useAB);
                    }
                }
                else
                {
                    yield return GetOutput(array, row, useAB);
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

        public string TransformDescription
        {
            get { return "Unroll "+UnrollExpression.Value<string>(SettingManager)+(UseAB.Value<bool>(SettingManager) ? " as A B" : String.Empty); }
        }
    }
}
