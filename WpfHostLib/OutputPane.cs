using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Wpf.Interface;

namespace com.antlersoft.HostedTools.WpfHostLib
{
    class PaneItem
    {
        internal string Title;
        internal EOutputPaneType PaneType;
        internal FrameworkElement Control;
        internal OutputPane SubPane;
    }

    class OutputPane : HostedObjectBase, IHasOutputPanes, IHasImageOutput
    {
        private ITextOutput _outputText;
        private IGridOutput _outputGrid;
        private IImageOutput _outputImage;
        private FrameworkElement _topElement;
        private List<PaneItem> _items; 
        internal OutputPane(IOutputPaneList paneList)
        {
            _items = new List<PaneItem>();
            if (paneList == null || paneList.Panes == null || paneList.Panes.Count == 0)
            {
                _outputText = new OutputTextBox();
                _topElement = _outputText as FrameworkElement;
            }
            else
            {
                var grid = new Grid();
                _topElement = grid;

                // Calculate total proportion
                int proportion = 0;
                foreach (IOutputPaneSpecifier paneSpec in paneList.Panes)
                {
                    if (paneSpec.Proportion > 0)
                    {
                        proportion += paneSpec.Proportion;
                    }
                }
                if (paneList.Orientation == EPaneListOrientation.Horizontal)
                {
                    grid.RowDefinitions.Clear();
                    RowDefinition labelRow = new RowDefinition();
                    labelRow.Height = new GridLength(0, GridUnitType.Auto);
                    grid.RowDefinitions.Add(labelRow);
                    RowDefinition mainRow = new RowDefinition();
                    mainRow.Height = new GridLength(1, GridUnitType.Star);
                    grid.RowDefinitions.Add(mainRow);
                }
                int paneOffset = 0;
                foreach (IOutputPaneSpecifier paneSpec in paneList.Panes)
                {
                    PaneItem pane = CreatePane(paneSpec);
                    _items.Add(pane);
                    Label paneLabel = null;
                    if (! String.IsNullOrEmpty(pane.Title))
                    {
                        paneLabel = new Label();
                        paneLabel.Content = pane.Title;
                    }
                    if (paneList.Orientation == EPaneListOrientation.Horizontal)
                    {
                        var col = new ColumnDefinition();
                        col.Width = new GridLength(WidthSetting(paneSpec.Proportion, proportion, paneList.Panes.Count), GridUnitType.Star);
                        grid.ColumnDefinitions.Add(col);
                        if (paneLabel != null)
                        {
                            Grid.SetRow(paneLabel, 0);
                            Grid.SetColumn(paneLabel, paneOffset);
                            grid.Children.Add(paneLabel);
                        }
                        Grid.SetRow(pane.Control, 1);
                        Grid.SetColumn(pane.Control, paneOffset);
                        grid.Children.Add(pane.Control);
                    }
                    else
                    {
                        if (paneLabel != null)
                        {
                            RowDefinition labelRow = new RowDefinition();
                            labelRow.Height = new GridLength(0, GridUnitType.Auto);
                            grid.RowDefinitions.Add(labelRow);
                            Grid.SetRow(paneLabel, paneOffset++);
                            Grid.SetColumn(paneLabel, 0);
                            grid.Children.Add(paneLabel);
                        }
                        var row = new RowDefinition();
                        row.Height = new GridLength(WidthSetting(paneSpec.Proportion, proportion, paneList.Panes.Count), GridUnitType.Star);
                        grid.RowDefinitions.Add(row);
                        Grid.SetRow(pane.Control, paneOffset);
                        Grid.SetColumn(pane.Control, 0);
                        grid.Children.Add(pane.Control);
                    }
                    paneOffset++;
                }
            }            
        }

        private PaneItem CreatePane(IOutputPaneSpecifier spec)
        {
            PaneItem result = new PaneItem {PaneType = spec.Type, Title = spec.Title};
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
                    count = totalProportion/paneCount;
                    if (count < 1)
                    {
                        count = 1;
                    }
                }
            }
            return count;
        }

        internal FrameworkElement Element
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
