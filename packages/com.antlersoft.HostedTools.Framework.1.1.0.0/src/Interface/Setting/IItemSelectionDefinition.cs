using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.Setting
{
    /// <summary>
    /// A setting definition that allows you to select from a list of items.
    /// The text that the user sees and uses to select the list of items is obtained from
    /// the item.  How this is done depends on the UI implementation;
    /// see com.antlersoft.HostedTools.Framework.Model.Setting.ItemSelectionItem
    /// </summary>
    public interface IItemSelectionDefinition : ISettingDefinition
    {
        /// <summary>
        /// Find object associated with a given raw text
        /// </summary>
        /// <param name="rawText"></param>
        /// <returns></returns>
        object FindMatchingItem(string rawText);

        string GetRawTextForItem(object item);

        IEnumerable<object> GetAllItems();

        bool IncludeEditButton();

        string NavigateToOnEdit(object item);
    }
}
