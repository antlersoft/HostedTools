using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IStemNode))]
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IAfterComposition))]
    public class ConcatTransform : AbstractPipelineNode, IHtValueStem, ISettingDefinitionSource, IAfterComposition
    {
        static PluginSelectionSettingDefinition SourcePlugin = new PluginSelectionSettingDefinition(PipelinePlugin.NodeFunc<IRootNode>, "SourcePlugin", "Pipeline.ConcatTransform", "Source to concatenate", "Select a source plugin for data to concatenate");

        public ConcatTransform()
        : base(new MenuItem("DevTools.Pipeline.Transform.ConcatTransform", "Concat", typeof(ConcatTransform).FullName,"DevTools.Pipeline.Transform"), new [] { SourcePlugin.FullKey()})
        {
        }

        public IEnumerable<ISettingDefinition> Definitions => new ISettingDefinition[] { SourcePlugin};

        public override string NodeDescription => "Concatenate "+((PluginSelectionItem)SourcePlugin.FindMatchingItem(SourcePlugin.Value<string>(SettingManager))).Plugin.Cast<IRootNode>().NodeDescription;

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            var source=PluginManager[state.SettingValues[SourcePlugin.FullKey()]].Cast<IHtValueRoot>().GetHtValueSource(state.NestedValues[SourcePlugin.FullKey()]);
            return new Transform(source);
        }

        public void AfterComposition()
        {
            SourcePlugin.SetPlugins(PluginManager.Plugins.Where(p => p.Cast<IRootNode>() != null).ToList(), SettingManager);
        }

        class Transform : HostedObjectBase, IHtValueTransform
        {
            IHtValueSource _source;
            internal Transform(IHtValueSource source)
            {
                _source = source;
            }
            public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
            {
                foreach (var v in input)
                {
                    yield return v;
                    if (monitor.Cast<ICancelableMonitor>()?.IsCanceled??false)
                    {
                        break;
                    }
                }
                if (monitor.Cast<ICancelableMonitor>()?.IsCanceled??false)
                {
                    yield break;
                }
                foreach (var v in _source.GetRows(monitor))
                {
                    yield return v;
                    if (monitor.Cast<ICancelableMonitor>()?.IsCanceled??false)
                    {
                        break;
                    }                    
                }
            }                
        }
    }
}