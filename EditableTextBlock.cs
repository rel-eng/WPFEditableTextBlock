/* WPFEditableTextBlock implementation.

   Copyright (C) 2010 rel-eng

   This file is part of WPFEditableTextBlock.

   This program is free software; you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation; either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>.  */

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace Custom.Controls
{
    [ContentProperty("Content")]
    public class EditableTextBlock : Control
    {

        #region Constructor

        static EditableTextBlock()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(EditableTextBlock), new FrameworkPropertyMetadata(typeof(EditableTextBlock)));
        }

        public EditableTextBlock()
            : base()
        {
            base.Focusable = true;
            base.FocusVisualStyle = null;
            if (this.IsInEditMode)
            {
                this.StartEditing();
            }
            else
            {
                this.StopEditing();
            }
            this.TextChanged += new RoutedPropertyChangedEventHandler<string>(EditableTextBlock_TextChanged);
            this.TextFormatChanged += new RoutedPropertyChangedEventHandler<string>(EditableTextBlock_TextFormatChanged);
            this.IsInEditModeChanged += new RoutedPropertyChangedEventHandler<bool>(EditableTextBlock_IsInEditModeChanged);
            this.MouseDoubleClick += new MouseButtonEventHandler(EditableTextBlock_MouseDoubleClick);
            this.AcceptsReturnChanged += new RoutedPropertyChangedEventHandler<bool>(EditableTextBlock_AcceptsReturnChanged);
            this.AcceptsTabChanged += new RoutedPropertyChangedEventHandler<bool>(EditableTextBlock_AcceptsTabChanged);
            this.TextWrappingChanged += new RoutedPropertyChangedEventHandler<System.Windows.TextWrapping>(EditableTextBlock_TextWrappingChanged);
            this.TextTrimmingChanged += new RoutedPropertyChangedEventHandler<System.Windows.TextTrimming>(EditableTextBlock_TextTrimmingChanged);
            this.VerticalScrollBarVisibilityChanged += new RoutedPropertyChangedEventHandler<ScrollBarVisibility>(EditableTextBlock_VerticalScrollBarVisibilityChanged);
            this.HorizontalScrollBarVisibilityChanged += new RoutedPropertyChangedEventHandler<ScrollBarVisibility>(EditableTextBlock_HorizontalScrollBarVisibilityChanged);
        }

        #endregion Constructor

        #region Member Variables

        private string _OldText;

        #endregion Member Variables

        #region Properties

        #region Text

        [Localizability(LocalizationCategory.Text), DefaultValue(""), Category("Common")]
        public string Text
        {
            get
            {
                return GetValue(TextProperty) as string;
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(EditableTextBlock),
                new FrameworkPropertyMetadata(String.Empty,
                    FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnTextChanged),
                    new CoerceValueCallback(CoerceText),
                    true,
                    UpdateSourceTrigger.LostFocus
                ), new ValidateValueCallback(ValidateText)
            );

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            EditableTextBlock etb = obj as EditableTextBlock;
            string oldValue = args.OldValue as string;
            string newValue = args.NewValue as string;

            #region Automation Events

            if (AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged))
            {
                EditableTextBoxAutomationPeer peer = UIElementAutomationPeer.FromElement(etb) as EditableTextBoxAutomationPeer;
                if (peer != null)
                {
                    peer.RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldValue, newValue);
                }
            }

            #endregion Automation Events

            RoutedPropertyChangedEventArgs<string> e = new RoutedPropertyChangedEventArgs<string>(
                oldValue, newValue, TextChangedEvent);
            etb.OnTextChanged(e);
        }

        protected virtual void OnTextChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            RaiseEvent(args);
        }

        private static object CoerceText(DependencyObject element, object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return value;
        }

        private static bool ValidateText(object value)
        {
            return true;
        }

        #endregion Text

        #region IsReadOnly

        [Category("Common")]
        public bool IsReadOnly
        {
            get
            {
                return (bool)GetValue(IsReadOnlyProperty);
            }
            set
            {
                SetValue(IsReadOnlyProperty, value);
            }
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                "IsReadOnly",
                typeof(bool),
                typeof(EditableTextBlock),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(OnIsReadOnlyChanged),
                    new CoerceValueCallback(CoerceIsReadOnly)
                ), new ValidateValueCallback(ValidateIsReadOnly)
            );

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            EditableTextBlock etb = obj as EditableTextBlock;

            etb.CoerceValue(EditableTextBlock.IsInEditModeProperty);

            bool oldValue = (bool)args.OldValue;
            bool newValue = (bool)args.NewValue;

            #region Automation Events

            if (AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged))
            {
                EditableTextBoxAutomationPeer peer = UIElementAutomationPeer.FromElement(etb) as EditableTextBoxAutomationPeer;
                if (peer != null)
                {
                    peer.RaisePropertyChangedEvent(ValuePatternIdentifiers.IsReadOnlyProperty, oldValue, newValue);
                }
            }

            #endregion Automation Events

            RoutedPropertyChangedEventArgs<bool> e = new RoutedPropertyChangedEventArgs<bool>(
                oldValue, newValue, IsReadOnlyChangedEvent);
            etb.OnIsReadOnlyChanged(e);
        }

        protected virtual void OnIsReadOnlyChanged(RoutedPropertyChangedEventArgs<bool> args)
        {
            RaiseEvent(args);
        }

        private static object CoerceIsReadOnly(DependencyObject element, object value)
        {
            return value;
        }

        private static bool ValidateIsReadOnly(object value)
        {
            return true;
        }

        #endregion IsReadOnly

        #region AcceptsReturn

        [Category("Common")]
        public bool AcceptsReturn
        {
            get
            {
                return (bool)GetValue(AcceptsReturnProperty);
            }
            set
            {
                SetValue(AcceptsReturnProperty, value);
            }
        }

        public static readonly DependencyProperty AcceptsReturnProperty =
            DependencyProperty.Register(
                "AcceptsReturn",
                typeof(bool),
                typeof(EditableTextBlock),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(OnAcceptsReturnChanged),
                    new CoerceValueCallback(CoerceAcceptsReturn)
                ), new ValidateValueCallback(ValidateAcceptsReturn)
            );

        private static void OnAcceptsReturnChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            EditableTextBlock etb = obj as EditableTextBlock;

            bool oldValue = (bool)args.OldValue;
            bool newValue = (bool)args.NewValue;

            RoutedPropertyChangedEventArgs<bool> e = new RoutedPropertyChangedEventArgs<bool>(
                oldValue, newValue, AcceptsReturnChangedEvent);
            etb.OnAcceptsReturnChanged(e);
        }

        protected virtual void OnAcceptsReturnChanged(RoutedPropertyChangedEventArgs<bool> args)
        {
            RaiseEvent(args);
        }

        private static object CoerceAcceptsReturn(DependencyObject element, object value)
        {
            return value;
        }

        private static bool ValidateAcceptsReturn(object value)
        {
            return true;
        }

        #endregion AcceptsReturn

        #region AcceptsTab

        [Category("Common")]
        public bool AcceptsTab
        {
            get
            {
                return (bool)GetValue(AcceptsTabProperty);
            }
            set
            {
                SetValue(AcceptsTabProperty, value);
            }
        }

        public static readonly DependencyProperty AcceptsTabProperty =
            DependencyProperty.Register(
                "AcceptsTab",
                typeof(bool),
                typeof(EditableTextBlock),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(OnAcceptsTabChanged),
                    new CoerceValueCallback(CoerceAcceptsTab)
                ), new ValidateValueCallback(ValidateAcceptsTab)
            );

        private static void OnAcceptsTabChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            EditableTextBlock etb = obj as EditableTextBlock;

            bool oldValue = (bool)args.OldValue;
            bool newValue = (bool)args.NewValue;

            RoutedPropertyChangedEventArgs<bool> e = new RoutedPropertyChangedEventArgs<bool>(
                oldValue, newValue, AcceptsTabChangedEvent);
            etb.OnAcceptsTabChanged(e);
        }

        protected virtual void OnAcceptsTabChanged(RoutedPropertyChangedEventArgs<bool> args)
        {
            RaiseEvent(args);
        }

        private static object CoerceAcceptsTab(DependencyObject element, object value)
        {
            return value;
        }

        private static bool ValidateAcceptsTab(object value)
        {
            return true;
        }

        #endregion AcceptsTab

        #region TextWrapping

        [Category("Text")]
        public TextWrapping TextWrapping
        {
            get
            {
                return (TextWrapping)GetValue(TextWrappingProperty);
            }
            set
            {
                SetValue(TextWrappingProperty, value);
            }
        }

        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register(
                "TextWrapping",
                typeof(TextWrapping),
                typeof(EditableTextBlock),
                new FrameworkPropertyMetadata(TextWrapping.NoWrap,
                    new PropertyChangedCallback(OnTextWrappingChanged),
                    new CoerceValueCallback(CoerceTextWrapping)
                ), new ValidateValueCallback(ValidateTextWrapping)
            );

        private static void OnTextWrappingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            EditableTextBlock etb = obj as EditableTextBlock;

            TextWrapping oldValue = (TextWrapping)args.OldValue;
            TextWrapping newValue = (TextWrapping)args.NewValue;

            RoutedPropertyChangedEventArgs<TextWrapping> e = new RoutedPropertyChangedEventArgs<TextWrapping>(
                oldValue, newValue, TextWrappingChangedEvent);
            etb.OnTextWrappingChanged(e);
        }

        protected virtual void OnTextWrappingChanged(RoutedPropertyChangedEventArgs<TextWrapping> args)
        {
            RaiseEvent(args);
        }

        private static object CoerceTextWrapping(DependencyObject element, object value)
        {
            return value;
        }

        private static bool ValidateTextWrapping(object value)
        {
            return true;
        }

        #endregion TextWrapping

        #region TextTrimming

        [Category("Text")]
        public TextTrimming TextTrimming
        {
            get
            {
                return (TextTrimming)GetValue(TextTrimmingProperty);
            }
            set
            {
                SetValue(TextTrimmingProperty, value);
            }
        }

        public static readonly DependencyProperty TextTrimmingProperty =
            DependencyProperty.Register(
                "TextTrimming",
                typeof(TextTrimming),
                typeof(EditableTextBlock),
                new FrameworkPropertyMetadata(TextTrimming.None,
                    new PropertyChangedCallback(OnTextTrimmingChanged),
                    new CoerceValueCallback(CoerceTextTrimming)
                ), new ValidateValueCallback(ValidateTextTrimming)
            );

        private static void OnTextTrimmingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            EditableTextBlock etb = obj as EditableTextBlock;

            TextTrimming oldValue = (TextTrimming)args.OldValue;
            TextTrimming newValue = (TextTrimming)args.NewValue;

            RoutedPropertyChangedEventArgs<TextTrimming> e = new RoutedPropertyChangedEventArgs<TextTrimming>(
                oldValue, newValue, TextTrimmingChangedEvent);
            etb.OnTextTrimmingChanged(e);
        }

        protected virtual void OnTextTrimmingChanged(RoutedPropertyChangedEventArgs<TextTrimming> args)
        {
            RaiseEvent(args);
        }

        private static object CoerceTextTrimming(DependencyObject element, object value)
        {
            return value;
        }

        private static bool ValidateTextTrimming(object value)
        {
            return true;
        }

        #endregion TextTrimming

        #region VerticalScrollBarVisibility

        [Category("Other")]
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get
            {
                return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
            }
            set
            {
                SetValue(VerticalScrollBarVisibilityProperty, value);
            }
        }

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
            DependencyProperty.Register(
                "VerticalScrollBarVisibility",
                typeof(ScrollBarVisibility),
                typeof(EditableTextBlock),
                new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden,
                    new PropertyChangedCallback(OnVerticalScrollBarVisibilityChanged),
                    new CoerceValueCallback(CoerceVerticalScrollBarVisibility)
                ), new ValidateValueCallback(ValidateVerticalScrollBarVisibility)
            );

        private static void OnVerticalScrollBarVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            EditableTextBlock etb = obj as EditableTextBlock;

            ScrollBarVisibility oldValue = (ScrollBarVisibility)args.OldValue;
            ScrollBarVisibility newValue = (ScrollBarVisibility)args.NewValue;

            RoutedPropertyChangedEventArgs<ScrollBarVisibility> e = new RoutedPropertyChangedEventArgs<ScrollBarVisibility>(
                oldValue, newValue, VerticalScrollBarVisibilityChangedEvent);
            etb.OnVerticalScrollBarVisibilityChanged(e);
        }

        protected virtual void OnVerticalScrollBarVisibilityChanged(RoutedPropertyChangedEventArgs<ScrollBarVisibility> args)
        {
            RaiseEvent(args);
        }

        private static object CoerceVerticalScrollBarVisibility(DependencyObject element, object value)
        {
            return value;
        }

        private static bool ValidateVerticalScrollBarVisibility(object value)
        {
            return true;
        }

        #endregion VerticalScrollBarVisibility

        #region HorizontalScrollBarVisibility

        [Category("Other")]
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get
            {
                return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
            }
            set
            {
                SetValue(HorizontalScrollBarVisibilityProperty, value);
            }
        }

        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
            DependencyProperty.Register(
                "HorizontalScrollBarVisibility",
                typeof(ScrollBarVisibility),
                typeof(EditableTextBlock),
                new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden,
                    new PropertyChangedCallback(OnHorizontalScrollBarVisibilityChanged),
                    new CoerceValueCallback(CoerceHorizontalScrollBarVisibility)
                ), new ValidateValueCallback(ValidateHorizontalScrollBarVisibility)
            );

        private static void OnHorizontalScrollBarVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            EditableTextBlock etb = obj as EditableTextBlock;

            ScrollBarVisibility oldValue = (ScrollBarVisibility)args.OldValue;
            ScrollBarVisibility newValue = (ScrollBarVisibility)args.NewValue;

            RoutedPropertyChangedEventArgs<ScrollBarVisibility> e = new RoutedPropertyChangedEventArgs<ScrollBarVisibility>(
                oldValue, newValue, HorizontalScrollBarVisibilityChangedEvent);
            etb.OnHorizontalScrollBarVisibilityChanged(e);
        }

        protected virtual void OnHorizontalScrollBarVisibilityChanged(RoutedPropertyChangedEventArgs<ScrollBarVisibility> args)
        {
            RaiseEvent(args);
        }

        private static object CoerceHorizontalScrollBarVisibility(DependencyObject element, object value)
        {
            return value;
        }

        private static bool ValidateHorizontalScrollBarVisibility(object value)
        {
            return true;
        }

        #endregion HorizontalScrollBarVisibility

        #region IsInEditMode

        [Category("Common")]
        public bool IsInEditMode
        {
            get
            {
                return (bool)GetValue(IsInEditModeProperty);
            }
            set
            {
                SetValue(IsInEditModeProperty, value);
            }
        }
        public static readonly DependencyProperty IsInEditModeProperty =
            DependencyProperty.Register(
                "IsInEditMode",
                typeof(bool),
                typeof(EditableTextBlock),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(OnIsInEditModeChanged),
                    new CoerceValueCallback(CoerceIsInEditMode)
                ), new ValidateValueCallback(ValidateIsInEditMode)
            );

        private static void OnIsInEditModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            EditableTextBlock etb = obj as EditableTextBlock;
            bool oldValue = (bool)args.OldValue;
            bool newValue = (bool)args.NewValue;

            RoutedPropertyChangedEventArgs<bool> e = new RoutedPropertyChangedEventArgs<bool>(
                oldValue, newValue, IsInEditModeChangedEvent);
            etb.OnIsInEditModeChanged(e);
        }

        protected virtual void OnIsInEditModeChanged(RoutedPropertyChangedEventArgs<bool> args)
        {
            RaiseEvent(args);
        }

        private static object CoerceIsInEditMode(DependencyObject element, object value)
        {
            EditableTextBlock etb = element as EditableTextBlock;
            bool mode=(bool)value;
            if (etb.IsReadOnly)
            {
                if (mode)
                {
                    return false;
                }
                else
                {
                    return value;
                }
            }
            else
            {
                return value;
            }
        }

        private static bool ValidateIsInEditMode(object value)
        {
            return true;
        }

        #endregion IsInEditMode

        #region TextFormat

        [Localizability(LocalizationCategory.Text), DefaultValue("{0}"), Category("Common")]
        public string TextFormat
        {
            get
            {
                return GetValue(TextFormatProperty) as string;
            }
            set
            {
                SetValue(TextFormatProperty, value);
            }
        }
        public static readonly DependencyProperty TextFormatProperty =
            DependencyProperty.Register(
                "TextFormat",
                typeof(string),
                typeof(EditableTextBlock),
                new FrameworkPropertyMetadata("{0}",
                    FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnTextFormatChanged),
                    new CoerceValueCallback(CoerceTextFormat),
                    true,
                    UpdateSourceTrigger.LostFocus
                ), new ValidateValueCallback(ValidateTextFormat)
            );

        private static void OnTextFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            EditableTextBlock etb = obj as EditableTextBlock;
            string oldValue = args.OldValue as string;
            string newValue = args.NewValue as string;

            RoutedPropertyChangedEventArgs<string> e = new RoutedPropertyChangedEventArgs<string>(
                oldValue, newValue, TextFormatChangedEvent);
            etb.OnTextFormatChanged(e);
        }

        protected virtual void OnTextFormatChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            RaiseEvent(args);
        }

        private static object CoerceTextFormat(DependencyObject element, object value)
        {
            if (value == null)
            {
                return "{0}";
            }
            else
            {
                if ((value as string) == "")
                {
                    return "{0}";
                }
            }
            return value;
        }

        private static bool ValidateTextFormat(object value)
        {
            return true;
        }

        #endregion TextFormat

        #region FormattedText

        private string FormattedText
        {
            get
            {
                return this.FormatText(TextFormat, Text);
            }
        }

        #endregion FormattedText

        #region Content

        [ReadOnly(true),DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),Bindable(false)]
        internal object Content
        {
            get
            {
                return GetValue(ContentProperty);
            }
            private set
            {
                SetValue(EditableTextBlock.ContentPropertyKey,value);
            }
        }

        internal static readonly DependencyPropertyKey ContentPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "Content",
                typeof(object),
                typeof(EditableTextBlock),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnContentChanged),
                    new CoerceValueCallback(CoerceContent)
                ), new ValidateValueCallback(ValidateContent)
            );

        internal static readonly DependencyProperty ContentProperty = ContentPropertyKey.DependencyProperty;

        private static void OnContentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            EditableTextBlock etb = obj as EditableTextBlock;
            object oldValue = args.OldValue;
            object newValue = args.NewValue;

            RoutedPropertyChangedEventArgs<object> e = new RoutedPropertyChangedEventArgs<object>(
                oldValue, newValue, ContentChangedEvent);
            etb.OnContentChanged(e);
        }

        protected virtual void OnContentChanged(RoutedPropertyChangedEventArgs<object> args)
        {
            RaiseEvent(args);
        }

        private static object CoerceContent(DependencyObject element, object value)
        {
            return value;
        }

        private static bool ValidateContent(object value)
        {
            return true;
        }

        #endregion Content

        #endregion Properties

        #region Events

        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent(
            "TextChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<string>), typeof(EditableTextBlock));

        public event RoutedPropertyChangedEventHandler<string> TextChanged
        {
            add { AddHandler(TextChangedEvent, value); }
            remove { RemoveHandler(TextChangedEvent, value); }
        }

        internal static readonly RoutedEvent IsReadOnlyChangedEvent = EventManager.RegisterRoutedEvent(
            "IsReadOnlyChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<bool>), typeof(EditableTextBlock));

        internal event RoutedPropertyChangedEventHandler<bool> IsReadOnlyChanged
        {
            add { AddHandler(IsReadOnlyChangedEvent, value); }
            remove { RemoveHandler(IsReadOnlyChangedEvent, value); }
        }

        internal static readonly RoutedEvent AcceptsReturnChangedEvent = EventManager.RegisterRoutedEvent(
            "AcceptsReturnChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<bool>), typeof(EditableTextBlock));

        internal event RoutedPropertyChangedEventHandler<bool> AcceptsReturnChanged
        {
            add { AddHandler(AcceptsReturnChangedEvent, value); }
            remove { RemoveHandler(AcceptsReturnChangedEvent, value); }
        }

        internal static readonly RoutedEvent AcceptsTabChangedEvent = EventManager.RegisterRoutedEvent(
            "AcceptsTabChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<bool>), typeof(EditableTextBlock));

        internal event RoutedPropertyChangedEventHandler<bool> AcceptsTabChanged
        {
            add { AddHandler(AcceptsTabChangedEvent, value); }
            remove { RemoveHandler(AcceptsTabChangedEvent, value); }
        }

        internal static readonly RoutedEvent TextWrappingChangedEvent = EventManager.RegisterRoutedEvent(
            "TextWrappingChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<TextWrapping>), typeof(EditableTextBlock));

        internal event RoutedPropertyChangedEventHandler<TextWrapping> TextWrappingChanged
        {
            add { AddHandler(TextWrappingChangedEvent, value); }
            remove { RemoveHandler(TextWrappingChangedEvent, value); }
        }

        internal static readonly RoutedEvent TextTrimmingChangedEvent = EventManager.RegisterRoutedEvent(
            "TextTrimmingChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<TextTrimming>), typeof(EditableTextBlock));

        internal event RoutedPropertyChangedEventHandler<TextTrimming> TextTrimmingChanged
        {
            add { AddHandler(TextTrimmingChangedEvent, value); }
            remove { RemoveHandler(TextTrimmingChangedEvent, value); }
        }

        internal static readonly RoutedEvent VerticalScrollBarVisibilityChangedEvent = EventManager.RegisterRoutedEvent(
            "VerticalScrollBarVisibilityChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<ScrollBarVisibility>), typeof(EditableTextBlock));

        internal event RoutedPropertyChangedEventHandler<ScrollBarVisibility> VerticalScrollBarVisibilityChanged
        {
            add { AddHandler(VerticalScrollBarVisibilityChangedEvent, value); }
            remove { RemoveHandler(VerticalScrollBarVisibilityChangedEvent, value); }
        }

        internal static readonly RoutedEvent HorizontalScrollBarVisibilityChangedEvent = EventManager.RegisterRoutedEvent(
            "HorizontalScrollBarVisibilityChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<ScrollBarVisibility>), typeof(EditableTextBlock));

        internal event RoutedPropertyChangedEventHandler<ScrollBarVisibility> HorizontalScrollBarVisibilityChanged
        {
            add { AddHandler(HorizontalScrollBarVisibilityChangedEvent, value); }
            remove { RemoveHandler(HorizontalScrollBarVisibilityChangedEvent, value); }
        }

        public static readonly RoutedEvent IsInEditModeChangedEvent = EventManager.RegisterRoutedEvent(
            "IsInEditModeChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<bool>), typeof(EditableTextBlock));

        public event RoutedPropertyChangedEventHandler<bool> IsInEditModeChanged
        {
            add { AddHandler(IsInEditModeChangedEvent, value); }
            remove { RemoveHandler(IsInEditModeChangedEvent, value); }
        }

        internal static readonly RoutedEvent TextFormatChangedEvent = EventManager.RegisterRoutedEvent(
            "TextFormatChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<string>), typeof(EditableTextBlock));

        internal event RoutedPropertyChangedEventHandler<string> TextFormatChanged
        {
            add { AddHandler(TextFormatChangedEvent, value); }
            remove { RemoveHandler(TextFormatChangedEvent, value); }
        }

        internal static readonly RoutedEvent ContentChangedEvent = EventManager.RegisterRoutedEvent(
            "ContentChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<object>), typeof(EditableTextBlock));

        internal event RoutedPropertyChangedEventHandler<object> ContentChanged
        {
            add { AddHandler(ContentChangedEvent, value); }
            remove { RemoveHandler(ContentChangedEvent, value); }
        }

        #endregion Events

        #region Event Handlers

        private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            Text = ((TextBox)sender).Text;
            this.IsInEditMode = false;
        }

        private void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Text = ((TextBox)sender).Text;
                this.IsInEditMode = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                this.IsInEditMode = false;
                Text = _OldText;
                e.Handled = true;
            }
        }

        private void EditableTextBlock_TextChanged(object sender, RoutedPropertyChangedEventArgs<string> e)
        {
            if (this.Content != null)
            {
                if (this.Content is TextBlock)
                {
                    TextBlock tb = this.Content as TextBlock;
                    tb.Text = this.FormatText(e.NewValue, this.TextFormat);
                }
                else
                {
                    if (this.Content is TextBox)
                    {
                        TextBox tb = this.Content as TextBox;
                        tb.Text = e.NewValue;
                    }
                }
            }
        }

        private void EditableTextBlock_TextFormatChanged(object sender, RoutedPropertyChangedEventArgs<string> e)
        {
            if (this.Content != null)
            {
                if (this.Content is TextBlock)
                {
                    TextBlock tb = this.Content as TextBlock;
                    tb.Text = this.FormatText(this.Text, e.NewValue);
                }
            }
        }

        private void EditableTextBlock_IsInEditModeChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if (e.NewValue)
            {
                this.StartEditing();
            }
            else
            {
                this.StopEditing();
            }
        }

        private void EditableTextBlock_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsInEditMode)
            {
                this.IsInEditMode = true;
            }
        }

        private void EditableTextBlock_AcceptsReturnChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if (this.Content is TextBox)
            {
                (this.Content as TextBox).AcceptsReturn = e.NewValue;
            }
        }

        private void EditableTextBlock_AcceptsTabChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if (this.Content is TextBox)
            {
                (this.Content as TextBox).AcceptsTab = e.NewValue;
            }
        }

        private void EditableTextBlock_TextWrappingChanged(object sender, RoutedPropertyChangedEventArgs<TextWrapping> e)
        {
            if (this.Content is TextBox)
            {
                (this.Content as TextBox).TextWrapping = e.NewValue;
            }
            else
            {
                if (this.Content is TextBlock)
                {
                    (this.Content as TextBlock).TextWrapping = e.NewValue;
                }
            }
        }

        private void EditableTextBlock_TextTrimmingChanged(object sender, RoutedPropertyChangedEventArgs<TextTrimming> e)
        {
            if (this.Content is TextBlock)
            {
                (this.Content as TextBlock).TextTrimming = e.NewValue;
            }
        }

        private void EditableTextBlock_VerticalScrollBarVisibilityChanged(object sender, RoutedPropertyChangedEventArgs<ScrollBarVisibility> e)
        {
            if (this.Content is TextBox)
            {
                (this.Content as TextBox).VerticalScrollBarVisibility = e.NewValue;
            }
        }

        private void EditableTextBlock_HorizontalScrollBarVisibilityChanged(object sender, RoutedPropertyChangedEventArgs<ScrollBarVisibility> e)
        {
            if (this.Content is TextBox)
            {
                (this.Content as TextBox).HorizontalScrollBarVisibility = e.NewValue;
            }
        }

        #endregion Event Handlers

        #region Automation

        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new EditableTextBoxAutomationPeer(this);
        }

        #endregion Automation

        #region Delegates

        private delegate void SetFocusDelegate();

        #endregion Delegates

        #region Methods

        private void SetFocusThroughDispatcher(TextBox tb)
        {
            this.Dispatcher.BeginInvoke(
                new SetFocusDelegate(delegate()
                {
                    tb.Focus();
                }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void StartEditing()
        {
            this._OldText = this.Text;
            TextBox tb = new TextBox();
            tb.VerticalAlignment = VerticalAlignment.Stretch;
            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.KeyDown += new KeyEventHandler(OnTextBoxKeyDown);
            tb.LostFocus += new RoutedEventHandler(OnTextBoxLostFocus);
            tb.GotFocus += new RoutedEventHandler(OnTextBoxGotFocus);
            tb.Text = this.Text;
            tb.AcceptsReturn = this.AcceptsReturn;
            tb.AcceptsTab = this.AcceptsTab;
            tb.TextWrapping = this.TextWrapping;
            tb.VerticalScrollBarVisibility = this.VerticalScrollBarVisibility;
            tb.HorizontalScrollBarVisibility = this.HorizontalScrollBarVisibility;
            this.Content = tb;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.SetFocusThroughDispatcher(tb);
            }
        }

        private void StopEditing()
        {
            TextBlock tb = new TextBlock();
            tb.VerticalAlignment = VerticalAlignment.Stretch;
            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.Text = this.Text;
            tb.Margin = new Thickness(5.0, 3.0, 5.0, 3.0);
            tb.TextWrapping = this.TextWrapping;
            tb.TextTrimming = this.TextTrimming;
            this.Content = tb;
        }

        private string FormatText(string text, string format)
        {
            return String.Format(format, text);
        }

        #endregion Methods
    }
}
