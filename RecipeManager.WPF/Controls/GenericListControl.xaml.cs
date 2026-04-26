using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RecipeManager.WPF.Controls
{
    public partial class GenericListControl : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(GenericListControl), new PropertyMetadata(null));

        public static readonly DependencyProperty ColumnNamesProperty =
            DependencyProperty.Register("ColumnNames", typeof(IEnumerable<string>), typeof(GenericListControl), new PropertyMetadata(null, OnColumnNamesChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(GenericListControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public GenericListControl()
        {
            InitializeComponent();
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public IEnumerable<string> ColumnNames
        {
            get => (IEnumerable<string>)GetValue(ColumnNamesProperty);
            set => SetValue(ColumnNamesProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        private static void OnColumnNamesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GenericListControl control && e.NewValue is IEnumerable<string> columns)
            {
                control.GenerateColumns(columns);
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GenericListControl control)
            {
                if (control.DynamicDataGrid.SelectedItem != e.NewValue)
                {
                    control.DynamicDataGrid.SelectedItem = e.NewValue;
                }
            }
        }

        private void GenerateColumns(IEnumerable<string> columns)
        {
            DynamicDataGrid.Columns.Clear();
            if (columns == null) return;

            foreach (var colName in columns)
            {
                var displayName = colName;
                if (colName == "CategoryName") displayName = "Category Name";
                if (colName == "CommonIngredients") displayName = "Common Ingredients";

                var column = new DataGridTextColumn
                {
                    Header = displayName,
                    Binding = new Binding(colName),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                };

                DynamicDataGrid.Columns.Add(column);
            }
        }

        private void DynamicDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItem != DynamicDataGrid.SelectedItem)
            {
                SelectedItem = DynamicDataGrid.SelectedItem;
            }
        }
    }
}