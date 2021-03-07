using com.antlersoft.HostedTools.Framework.Gtk.Interface;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model;
using Gtk;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    class PaneItem
    {
        internal string Title;
        internal EOutputPaneType PaneType;
        internal Widget Control;
        internal OutputPane SubPane;
    }

    class OutputPane : HostedObjectBase, IHasOutputPanes, IHasImageOutput
    {
        private ITextOutput _outputText;
        private IGridOutput _outputGrid;
        private IImageOutput _outputImage;
        private Widget _topElement;
        private List<PaneItem> _items;
        internal OutputPane(IOutputPaneList paneList)
        {
            _items = new List<PaneItem>();
            if (paneList == null || paneList.Panes == null || paneList.Panes.Count == 0)
            {
                _outputText = new OutputTextBox();
                _topElement = _outputText as Widget;
            }
            else
            {
                // Calculate total proportion
                int proportion = 0;
                foreach (IOutputPaneSpecifier paneSpec in paneList.Panes)
                {
                    if (paneSpec.Proportion > 0)
                    {
                        proportion += paneSpec.Proportion;
                    }
                    else
                    {
                        proportion += 1;
                    }
                }
                if (proportion <= 0)
                {
                    proportion = 1;
                }
                int multiplier = 1;
                if (proportion < 100)
                {
                    multiplier = 100 / proportion + 1;
                    proportion *= multiplier;
                }

                var grid = paneList.Orientation == EPaneListOrientation.Horizontal ? new Table(1, (uint)proportion, false) : new Table((uint)proportion, 1, false);
                _topElement = grid;

                uint paneOffset = 0;
                foreach (IOutputPaneSpecifier paneSpec in paneList.Panes)
                {
                    PaneItem pane = CreatePane(paneSpec);
                    _items.Add(pane);
                    Label paneLabel = null;
                    var paneControl = pane.Control;
                    if (!String.IsNullOrEmpty(pane.Title))
                    {
                        paneLabel = new Label();
                        paneLabel.Text = pane.Title;
                        var vbox = new VBox(false, 2);
                        vbox.PackStart(paneLabel, false, false, 0);
                        vbox.PackEnd(paneControl, true, true, 0);
                        paneControl = vbox;
                    }
                    uint trailingEdge = paneOffset + (uint)((paneSpec.Proportion <= 0 ? 1 : paneSpec.Proportion) * multiplier - 1);
                    if (trailingEdge > proportion - 1)
                    {
                        trailingEdge = (uint)(proportion - 1);
                    }
                    if (paneList.Orientation == EPaneListOrientation.Horizontal)
                    {
                        grid.Attach(paneControl, paneOffset, trailingEdge+1, 0, 1, AttachOptions.Expand|AttachOptions.Fill, AttachOptions.Expand|AttachOptions.Fill, 2, 2);
                    }
                    else
                    {
                        grid.Attach(paneControl, 0, 1, paneOffset, trailingEdge+1, AttachOptions.Expand|AttachOptions.Fill, AttachOptions.Expand|AttachOptions.Fill, 2, 2);
                    }

                    paneOffset = trailingEdge + 1;
                }
            }
        }

        private PaneItem CreatePane(IOutputPaneSpecifier spec)
        {
            PaneItem result = new PaneItem { PaneType = spec.Type, Title = spec.Title };
            switch (spec.Type)
            {
                case EOutputPaneType.Grid:
                    result.Control = new GridOutput();
                    if (_outputGrid == null)
                    {
                        _outputGrid = result.Control as IGridOutput;
                    }
                    break;
                case EOutputPaneType.List:
                    result.Control = new OutputListBox();
                    if (_outputText == null)
                    {
                        _outputText = result.Control as ITextOutput;
                    }
                    break;
                case EOutputPaneType.Text:
                    result.Control = new OutputTextBox();
                    if (_outputText == null)
                    {
                        _outputText = result.Control as ITextOutput;
                    }
                    break;
                case EOutputPaneType.Image:
                    result.Control = new ImageOutput();
                    if (_outputImage == null)
                    {
                        _outputImage = result.Control as IImageOutput;
                    }
                    break;
                case EOutputPaneType.CustomControl:
                    result.Control = spec.Cast<IElementSource>()?.GetElement(_topElement);
                    if (result.Control == null)
                    {
                        // Failsafe
                        result.Control = new OutputTextBox();
                        if (_outputText == null)
                        {
                            _outputText = result.Control as ITextOutput;
                        }
                    }
                    else
                    {
                        if (_outputText == null)
                        {
                            _outputText = result.Control as ITextOutput;
                        }
                        if (_outputGrid == null)
                        {
                            _outputGrid = result.Control as IGridOutput;
                        }
                        if (_outputImage == null)
                        {
                            _outputImage = result.Control as IImageOutput;
                        }
                    }
                    break;
                case EOutputPaneType.Nested:
                    result.SubPane = new OutputPane(spec.NestedPanes);
                    result.Control = result.SubPane.Element;
                    if (_outputText == null)
                    {
                        _outputText = result.SubPane.FindTextOutput();
                    }
                    if (_outputGrid == null)
                    {
                        _outputGrid = result.SubPane.FindGridOutput();
                    }
                    break;
            }
            return result;
        }

        private int WidthSetting(int paneWidth, int totalProportion, int paneCount)
        {
            int count = 1;
            if (totalProportion != 0)
            {
                count = paneWidth;
                if (paneWidth == 0)
                {
                    count = totalProportion / paneCount;
                    if (count < 1)
                    {
                        count = 1;
                    }
                }
            }
            return count;
        }

        internal Widget Element
        {
            get { return _topElement; }
        }

        public ITextOutput FindTextOutput(string title = null)
        {
            if (title == null)
            {
                return _outputText;
            }
            foreach (var pi in _items)
            {
                if (pi.Title == title && (pi.PaneType == EOutputPaneType.List || pi.PaneType == EOutputPaneType.Text))
                {
                    return pi.Control as ITextOutput;
                }
                else if (pi.PaneType == EOutputPaneType.Nested)
                {
                    ITextOutput result = pi.SubPane.FindTextOutput(title);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        public IGridOutput FindGridOutput(string title = null)
        {
            if (title == null)
            {
                return _outputGrid;
            }
            foreach (var pi in _items)
            {
                if (pi.Title == title && pi.PaneType == EOutputPaneType.Grid)
                {
                    return pi.Control as IGridOutput;
                }
                else if (pi.PaneType == EOutputPaneType.Nested)
                {
                    IGridOutput result = pi.SubPane.FindGridOutput(title);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        public IImageOutput FindImageOutput(string title = null)
        {
            if (title == null)
            {
                return _outputImage;
            }
            foreach (var pi in _items)
            {
                if (pi.Title == title && pi.PaneType == EOutputPaneType.Grid)
                {
                    return pi.Control as IImageOutput;
                }
                else if (pi.PaneType == EOutputPaneType.Nested)
                {
                    IImageOutput result = pi.SubPane.FindImageOutput(title);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }
    }
}

