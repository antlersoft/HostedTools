using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model;
using Microsoft.Win32;

namespace com.antlersoft.HostedTools.WpfHostLib
{
    class GridOutput : ListView, IGridOutput, IListenerCollection<Dictionary<string,object>> 
    {
        private ObservableCollection<Dictionary<string,object>> _rows;
        private GridView _gridView;
        private bool _isDummyColumns;
        private Dictionary<string, GridViewColumn> _columnsByName;
        private ListenerCollection<Dictionary<string, object>> _listenerCollection;
        private string _rightClickSource;

        internal GridOutput()
        {
            _rows = new ObservableCollection<Dictionary<string,object>>();
            _columnsByName = new Dictionary<string, GridViewColumn>();
            _listenerCollection = new ListenerCollection<Dictionary<string, object>>();
            ItemsSource = _rows;
            SelectionChanged += (sender, args) =>
                {
                    if (args.AddedItems.Count > 0)
                    {
                        _listenerCollection.NotifyListeners((Dictionary<string,object>)args.AddedItems[0]);
                    }
                };
            Reset();
            var menu = ContextMenu;
            MouseRightButtonUp += (source, args) =>
            {
                TextBlock block = args.OriginalSource as TextBlock;
                if (block != null)
                {
                    _rightClickSource = block.Text;
                }
                else
                {
                    _rightClickSource = "(Unknown)";
                }
            };
            if (menu == null)
            {
                menu = new ContextMenu();
                ContextMenu = menu;
            }
            MenuItem item = new MenuItem();
            item.Header = "Write to tab-delimited";
            item.Click += (sender, args) => WriteToCsv();
            menu.Items.Add(item);
            item = new MenuItem {Header = "Copy cell contents"};
            item.Click += CopyCell;
            menu.Items.Add(item);
        }

        private void CopyCell(object sender, RoutedEventArgs args)
        {
            Clipboard.SetText(_rightClickSource);
        }

        private void WriteToCsv()
        {
            var dlg = new SaveFileDialog() { Title = "Write to tab-delimited" };
            var ext = "Tab-delimited text|*.txt";
            dlg.Filter = ext;
            bool? result = dlg.ShowDialog();
            string path = dlg.FileName;
            if (result?? false)
            {
                Task.Run(() =>
                {
                    var colList = _columnsByName.Keys.ToList();
                    using (StreamWriter fs = new StreamWriter(path))
                    {
                        bool first = true;
                        foreach (string h in colList)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                fs.Write('\t');
                            }
                            fs.Write(h);
                        }
                        fs.WriteLine();
                        foreach (var row in _rows.ToList())
                        {
                            first = true;
                            foreach (string h in colList)
                            {
                                if (first)
                                {
                                    first = false;
                                }
                                else
                                {
                                    fs.Write('\t');
                                }
                                object o;
                                if (row.TryGetValue(h, out o))
                                {
                                    fs.Write(o.ToString());
                                }
                            }
                            fs.WriteLine();
                        }
                    }
                });
            }
        }

        private void Reset()
        {
            _gridView = new GridView();
            View = _gridView;
            _isDummyColumns = true;
            _columnsByName.Clear();
        }

        public void AddRow(Dictionary<string, object> row)
        {
            if (row.Count == 0 || row.Keys.All(v => v == null))
            {
                return;
            }
            Dispatcher.Invoke(() =>
                {
                    if (_isDummyColumns)
                    {
                        _gridView.Columns.Clear();
                        _isDummyColumns = false;
                    }
                    GridViewColumn col;
                    foreach (var kvp in row)
                    {
                        if (! _columnsByName.TryGetValue(kvp.Key, out col))
                        {
                            GridViewColumn column = new GridViewColumn();
                            column.Header = kvp.Key;
                            string v = kvp.Value.ToString();
                            int len = Math.Max(kvp.Key.Length, v.Length);
                            int width = len*16;
                            if (width > 256)
                            {
                                width = 256;
                            }
                            column.Width = width;
                            column.DisplayMemberBinding = new Binding("["+kvp.Key+"]");
                            _columnsByName[kvp.Key] = column;
                            _gridView.Columns.Add(column);
                        }
                        else
                        {
                            int width = kvp.Value.ToString().Length*16;
                            if (width < 256 && width > col.Width)
                            {
                                col.Width = width;
                            }
                        }
                    }
                    _rows.Add(row);
                });
        }

        public void Clear()
        {
            Dispatcher.Invoke(() =>
                {
                    _rows.Clear();
                    Reset();
                });
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }

        public void AddListener(Action<Dictionary<string, object>> listener)
        {
            _listenerCollection.AddListener(listener);
        }

        public void RemoveListener(Action<Dictionary<string, object>> listener)
        {
            _listenerCollection.RemoveListener(listener);
        }
    }
}
