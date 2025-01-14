using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Framework.Interface.UI {
    /// <summary>
    /// Represents a command that can be invoked on a particular item in a grid output UI, commonly
    /// by right-clicking on the item and selecting the command from a drop-down menu
    /// </summary>
    public interface IGridItemCommand {
        /// <summary>
        /// Prompt for the command as might be displayed in the drop-down menu
        /// </summary>
        string Prompt { get; }
        /// <summary>
        /// Indicate if this command should be available for a particular selection
        /// </summary>
        /// <param name="row">Row in grid output that is selected</param>
        /// <param name="column">Selected column in the given row.  This value might
        /// be null if column selection is unavailable</param>
        /// <returns>true if the command should be displayed as enabled
        /// when the menu is shown; false if the command should be disabled
        /// (or hidden)</returns>
        bool IsEnabled(Dictionary<string,object> row, string column);
        /// <summary>
        /// Run this command for a particular selection.  Should return immediately;
        /// a time-consuming process should be started asynchronously
        /// </summary>
        /// <param name="row">Row in grid output that is selected</param>
        /// <param name="column">Selected column in the given row.  This value might
        /// be null if column selection is unavailable</param>
        void Invoke(Dictionary<string,object> row, string column);
    }

}