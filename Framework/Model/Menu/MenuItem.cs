using com.antlersoft.HostedTools.Framework.Interface.Menu;

namespace com.antlersoft.HostedTools.Framework.Model.Menu
{
    public class MenuItem : HostedObjectBase, IMenuItem
    {
        public MenuItem(string key, string prompt, string actionId = null, string parentKey = null)
        {
            Key = key;
            Prompt = prompt;
            ActionId = actionId;
            ParentKey = parentKey;
        }

        public string Key { get; private set; }

        public string Prompt { get; private set; }

        public string ParentKey { get; private set; }

        public string ActionId { get; set; }
    }
}
