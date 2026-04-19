using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RecipeManager.Core.Utils;

namespace RecipeManager.WPF.Controls
{
    public partial class DynamicFilterControl : UserControl
    {
        public static readonly DependencyProperty TargetTypeProperty =
            DependencyProperty.Register("TargetType", typeof(Type), typeof(DynamicFilterControl), new PropertyMetadata(null, OnTargetTypeChanged));

        public static readonly DependencyProperty FilterPropertiesProperty =
            DependencyProperty.Register("FilterProperties", typeof(IEnumerable<string>), typeof(DynamicFilterControl), new PropertyMetadata(null, OnTargetTypeChanged));

        public static readonly DependencyProperty FilterCommandProperty =
            DependencyProperty.Register("FilterCommand", typeof(ICommand), typeof(DynamicFilterControl), new PropertyMetadata(null));

        public DynamicFilterControl()
        {
            InitializeComponent();
        }

        public Type TargetType
        {
            get => (Type)GetValue(TargetTypeProperty);
            set => SetValue(TargetTypeProperty, value);
        }

        public IEnumerable<string> FilterProperties
        {
            get => (IEnumerable<string>)GetValue(FilterPropertiesProperty);
            set => SetValue(FilterPropertiesProperty, value);
        }

        public ICommand FilterCommand
        {
            get => (ICommand)GetValue(FilterCommandProperty);
            set => SetValue(FilterCommandProperty, value);
        }

        private List<FilterItem> _filterItems = new List<FilterItem>();

        private static void OnTargetTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DynamicFilterControl control)
            {
                control.GenerateFilterUI();
            }
        }

        private void GenerateFilterUI()
        {
            if (TargetType == null || FilterProperties == null) return;

            _filterItems.Clear();
            foreach (var propName in FilterProperties)
            {
                var propInfo = TargetType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (propInfo != null)
                {
                    _filterItems.Add(new FilterItem { PropertyName = propName, PropertyType = propInfo.PropertyType });
                }
            }

            FilterPanel.ItemsSource = null;
            FilterPanel.ItemsSource = _filterItems;
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            if (TargetType == null) return;
            var values = new Dictionary<string, string>();
            foreach (var item in _filterItems)
            {
                values[item.PropertyName] = item.FilterValue ?? string.Empty;
            }

            // Using reflection to call Generic BuildExpression<T> from utils
            var method = typeof(FilterExpressionBuilder).GetMethod("BuildExpression")?.MakeGenericMethod(TargetType);
            if (method != null)
            {
                var expr = method.Invoke(null, new object[] { values });
                if (FilterCommand != null && FilterCommand.CanExecute(expr))
                {
                    FilterCommand.Execute(expr);
                }
            }
        }
    }

    public class FilterItem
    {
        public string PropertyName { get; set; }
        public Type PropertyType { get; set; }
        public string FilterValue { get; set; }
    }
}