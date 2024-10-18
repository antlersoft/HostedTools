using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IHtValueSource))]
    public class JsonHttpQuery : EditOnlyPlugin, IHtValueSource, ISettingDefinitionSource
    {
        public static ISettingDefinition QueryUrl = new SimpleSettingDefinition("QueryURL", "JsonHttpQuery", "Query URL");
        public static ISettingDefinition UsePost = new SimpleSettingDefinition("UsePost", "JsonHttpQuery", "Use POST", "If checked, will do POST http query using content from Post Data", typeof(bool), "false");
        public static ISettingDefinition PostData = new MultiLineSettingDefinition("PostData", "JsonHttpQuery", 6, "Post Data");
        public static ISettingDefinition AdditionalHeaders = new MultiLineSettingDefinition("AdditionalHeaders", "JsonHttpQuery", 4, "Additional Headers");

        public JsonHttpQuery()
            : base(
                new MenuItem("DevTools.Pipeline.Input.JsonHttpQuery", "Json Http Query", typeof (JsonHttpQuery).FullName,
                    "DevTools.Pipeline.Input"), new[] {QueryUrl.FullKey(), UsePost.FullKey(), AdditionalHeaders.FullKey(), PostData.FullKey()})
        {
        }

        public IEnumerable<IHtValue> GetRows()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage message;
            var settings = new JsonFactory().GetSettings();
            if (UsePost.Value<bool>(SettingManager))
            {
                HttpRequestMessage request = new HttpRequestMessage
                {
                    RequestUri = new Uri(QueryUrl.Value<string>(SettingManager)),
                    Content = new StringContent(PostData.Value<string>(SettingManager)),
                    Method = HttpMethod.Post,
                };
                String headerText = AdditionalHeaders.Value<string>(SettingManager);
                foreach (string line in headerText.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] parts = line.Split(new[] {':'}, 2);
                    if (parts.Length == 2)
                    {
                        request.Headers.Add(parts[0], parts[1]);
                    }
                }
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                message = client.SendAsync(request).Result;
            }
            else
            {
                message = client.GetAsync(QueryUrl.Value<string>(SettingManager)).Result;
            }
            message.EnsureSuccessStatusCode();
            var convert = JsonConvert.DeserializeObject<IHtValue>(message.Content.ReadAsStringAsync().Result, settings);
            if (convert != null) {
                yield return convert;
            }
        }

        public string SourceDescription
        {
            get
            {
                return (UsePost.Value<bool>(SettingManager) ? "POST" : "GET") + " from " +
                       QueryUrl.Value<string>(SettingManager);
            }
        }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new [] {QueryUrl, PostData, AdditionalHeaders, UsePost}; }
        }
    }

    [Export(typeof(IHtValueTransform))]
    public class JsonHttpFilter : EditOnlyPlugin, IHtValueTransform, IExplanation
    {
        public JsonHttpFilter()
            : base(new MenuItem("DevTools.Pipeline.Transform.JsonHttpFilter", "Json Http Filter", typeof(JsonHttpFilter).FullName, "DevTools.Pipeline.Transform"), new String[0])
        { }

        public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage message;
            var settings = new JsonFactory().GetSettings();
            foreach (var row in input)
            {
                string url = row.AsString;
                message = client.GetAsync(url).Result;
                message.EnsureSuccessStatusCode();
                yield return JsonConvert.DeserializeObject<IHtValue>(message.Content.ReadAsStringAsync().Result, settings);
            }
        }

        public string TransformDescription
        {
            get { return Explanation; }
        }

        public string Explanation
        {
            get { return "IHtValue object from JSON returned from URL got by interpreting input as a string"; }
        }
    }
}
