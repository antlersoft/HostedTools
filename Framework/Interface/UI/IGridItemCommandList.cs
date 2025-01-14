using System;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Framework.Interface.UI {
    /// <summary>
    /// An object that implements IOutputPaneList and specifies a pane type of
    /// EOutputPaneType.Grid might want to also implement this interface to specify
    /// that it has IGridItemCommand objects to add to the output grid.
    /// </summary>
    public interface IGridItemCommandList {
        /// <summary>
        /// Returns a list of commands that can be invoked on selected items in a grid output
        /// </summary>
        /// <param name="title">Identifier of grid pane; may be null if it is the "default" grid output pan</param>
        /// <returns>List of IGridItemCommand objects for the specified grid</returns>
        IEnumerable<IGridItemCommand> GetGridItemCommands(string title);
    }
}