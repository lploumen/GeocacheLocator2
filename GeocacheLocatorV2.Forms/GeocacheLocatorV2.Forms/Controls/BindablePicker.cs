using System;
using System.Collections;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace GeocacheLocatorV2.PCL.Controls
{
    public class ExtendedPicker : Picker
    {
        public ExtendedPicker()
        {
            SelectedIndexChanged += OnSelectedIndexChanged;
        }

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create("SelectedItem", typeof(object), typeof(ExtendedPicker), null, BindingMode.TwoWay, null, OnSelectedItemChanged);

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(ExtendedPicker), null, BindingMode.OneWay, null, OnItemsSourceChanged);

        public static readonly BindableProperty ToStringFunctionProperty = BindableProperty.Create(
            propertyName: nameof(ToStringFunction),
            returnType: typeof(Func<object, string>),
            declaringType: typeof(ExtendedPicker),
            defaultValue: default(Func<object, string>),
            propertyChanged: OnToStringFunctionPropertyChanged
        );

        public Func<object, string> ToStringFunction
        {
            get { return (Func<object, string>)GetValue(ToStringFunctionProperty); }
            set { SetValue(ToStringFunctionProperty, value); }
        }

        public static readonly BindableProperty DisplayPropertyProperty = BindableProperty.Create("DisplayProperty", typeof(string), typeof(ExtendedPicker), null, BindingMode.OneWay, null, OnToStringFunctionPropertyChanged);

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set
            {
                if (SelectedItem == value)
                    return;
                SetValue(SelectedItemProperty, value);
                if (ItemsSource.Contains(SelectedItem))
                {

                    var newSelectedIndex = ItemsSource.IndexOf(SelectedItem);
                    if (newSelectedIndex != SelectedIndex)
                        SelectedIndex = newSelectedIndex;
                }
                else
                    SelectedIndex = -1;
            }
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedItem = SelectedIndex == -1
                ? null
                : ItemsSource[SelectedIndex];
        }

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ExtendedPicker)bindable;
            picker.SelectedItem = newValue;
            if (picker.ItemsSource != null && picker.SelectedItem != null)
            {
                var count = 0;
                foreach (var obj in picker.ItemsSource)
                {
                    if (obj == picker.SelectedItem || (obj is string && obj.Equals(picker.SelectedItem)))
                    {
                        picker.SelectedIndex = count;
                        break;
                    }
                    count++;
                }
            }
        }

        private static void OnToStringFunctionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            LoadItemsAndSetSelected(bindable);
        }

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ExtendedPicker)bindable;
            picker.ItemsSource = (IList)newValue;

            var oc = newValue as INotifyCollectionChanged;

            if (oc != null)
            {
                oc.CollectionChanged += (a, b) => {
                    LoadItemsAndSetSelected(bindable);
                };
            }

            LoadItemsAndSetSelected(bindable);
        }

        private static void LoadItemsAndSetSelected(BindableObject bindable)
        {
            var picker = (ExtendedPicker)bindable;

            if (picker.ItemsSource == null)
                return;

            var count = 0;

            foreach (var obj in picker.ItemsSource)
            {
                var value = picker.ToStringFunction != null
                    ? picker.ToStringFunction.Invoke(obj)
                    : obj.ToString();

                if (!picker.Items.Contains(value))
                    picker.Items.Add(value);

                if (picker.SelectedItem != null && picker.SelectedItem == obj)
                    picker.SelectedIndex = count;

                count++;
            }

            if (picker.ItemsSource.Count == picker.Items.Count - 1)
                picker.SelectedIndex++;
        }
    }
}
