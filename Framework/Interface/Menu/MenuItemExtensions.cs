using System;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Framework.Interface.Menu
{
    public static class MenuItemExtensions
    {
        public static string GetBreadCrumbString(this IMenuItem item, IMenuManager manager)
        {
            String result = string.Empty;

            while (item != null)
            {
                if (result.Length > 0)
                {
                    result = item.Prompt + " >> " + result;
                }
                else
                {
                    result = item.Prompt;
                }
                if (! String.IsNullOrEmpty(item.ParentKey))
                {
                    item = manager[item.ParentKey];
                }
                else
                {
                    item = null;
                }
            }

            return result;
        }
    }
}
