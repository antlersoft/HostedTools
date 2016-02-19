using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline
{
    public abstract class PipelinePluginBase : GridWorker, IHtValueTransform
    {
        [Import]
        public IConditionBuilder ConditionBuilder;

        private readonly string _transformLayout;

        protected PipelinePluginBase(IMenuItem menuItem, IEnumerable<string> settingKeys,
            string transformLayout = "filter|transform|custom")
            : base(menuItem, GetSettingKeysInLayout(settingKeys, transformLayout))
        {
            _transformLayout = transformLayout;
        }

        private static IEnumerable<string> GetSettingKeysInLayout(IEnumerable<string> originalKeys,
            string transformLayout)
        {
            List<string> result = new List<string>(10);
            result.Add(PipelinePlugin.Source.FullKey());
            string[] layoutComponents = transformLayout == null ? new string[0] : transformLayout.Split('|');
            foreach (string s in layoutComponents.Distinct())
            {
                if (s == "filter")
                {
                    result.Add("DynamoQuery.LocalFilter");
                }
                else if (s == "transform")
                {
                    result.Add(PipelinePlugin.Transform.FullKey());
                }
                else if (s == "custom" && originalKeys != null)
                {
                    result.AddRange(originalKeys);
                }
            }
            result.Add(PipelinePlugin.GridOutput.FullKey());
            result.Add(PipelinePlugin.Sink.FullKey());
            return result;
        }

        public override void Perform(IWorkMonitor monitor)
        {
            ClearGrid(monitor);
            IHtValueSource input = ((PluginSelectionItem)PipelinePlugin.Source.FindMatchingItem(PipelinePlugin.Source.Value<string>(SettingManager))).Plugin.Cast<IHtValueSource>();
            IHtValueSink output = ((PluginSelectionItem)PipelinePlugin.Sink.FindMatchingItem(PipelinePlugin.Sink.Value<string>(SettingManager))).Plugin.Cast<IHtValueSink>();
            IHtValueTransform transform = ((PluginSelectionItem)PipelinePlugin.Transform.FindMatchingItem(PipelinePlugin.Transform.Value<string>(SettingManager))).Plugin.Cast<IHtValueTransform>();
            bool gridOutput = PipelinePlugin.GridOutput.Value<bool>(SettingManager);
            string filter = SettingManager["DynamoQuery.LocalFilter"].Get<string>();
            CanBackground(monitor, "Pipe from " + input.SourceDescription + " through " + TransformDescription + " to " + output.SinkDescription);
            IEnumerable<IHtValue> rows = input.GetRows();
            bool customSet = false;
            string[] layoutComponents = _transformLayout == null ? new string[0] : _transformLayout.Split('|');
            foreach (string s in layoutComponents.Distinct())
            {
                if (s == "filter")
                {
                    if (!String.IsNullOrWhiteSpace(filter))
                    {
                        IHtExpression expression =
                            ConditionBuilder.ParseCondition(filter);
                        if (expression != null)
                        {
                            rows = rows.Where(r => expression.Evaluate(r).AsBool);
                        }
                    }
                }
                else if (s == "transform" && ! (transform is NullTransform))
                {
                    rows = transform.GetTransformed(rows, monitor);
                }
                else if (s == "custom")
                {
                    rows = GetTransformed(rows, monitor);
                    customSet = true;
                }
            }
            if (! customSet)
            {
                rows = GetTransformed(rows, monitor);
            }

            if (gridOutput)
            {
                rows = rows.Select(r =>
                {
                    WriteRecord(monitor, r);
                    return r;
                });
            }
            output.ReceiveRows(new MonitoredEnumerable<IHtValue>(rows, monitor), monitor);
        }

        public abstract IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor);

        public abstract string TransformDescription { get; }
    }
}
