using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Tournament_Master.Controls
{
    public partial class CyclicSelector : UserControl
    {
        // Властивості залежності, щоб можна було прив'язувати дані в XAML
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CyclicSelector), new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(CyclicSelector), new PropertyMetadata(0, OnSelectedIndexChanged));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        // Подія, яка спрацьовує при зміні вибору
        public event SelectionChangedEventHandler SelectionChanged;

        private int _maxIndex = 0;
        private IList _itemsList;

        public CyclicSelector()
        {
            InitializeComponent();
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CyclicSelector;
            if (control != null && e.NewValue is IEnumerable list)
            {
                control._itemsList = list as IList;
                if (control._itemsList != null)
                {
                    control._maxIndex = control._itemsList.Count - 1;
                    control.UpdateDisplay();
                }
            }
        }

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CyclicSelector;
            if (control != null)
            {
                control.UpdateDisplay();
                control.RaiseSelectionChanged((int)e.OldValue, (int)e.NewValue);
            }
        }

        private void UpdateDisplay()
        {
            if (_itemsList != null && SelectedIndex >= 0 && SelectedIndex < _itemsList.Count)
            {
                TxtValue.Text = _itemsList[SelectedIndex].ToString();
            }
            else
            {
                TxtValue.Text = "";
            }
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_maxIndex > 0)
            {
                int newIndex = SelectedIndex - 1;
                if (newIndex < 0) newIndex = _maxIndex; // Циклічний перехід
                SelectedIndex = newIndex;
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_maxIndex > 0)
            {
                int newIndex = SelectedIndex + 1;
                if (newIndex > _maxIndex) newIndex = 0; // Циклічний перехід
                SelectedIndex = newIndex;
            }
        }

        private void RaiseSelectionChanged(int oldIndex, int newIndex)
        {
            var removedItems = _itemsList != null && oldIndex >= 0 && oldIndex < _itemsList.Count ? new[] { _itemsList[oldIndex] } : new object[] { };
            var addedItems = _itemsList != null && newIndex >= 0 && newIndex < _itemsList.Count ? new[] { _itemsList[newIndex] } : new object[] { };
            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(ListBox.SelectionChangedEvent, removedItems, addedItems));
        }
    }
}