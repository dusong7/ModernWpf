﻿using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public class PasswordBoxHelper : DependencyObject
    {
        private const string ButtonStatesGroup = "ButtonStates";
        private const string ButtonVisibleState = "ButtonVisible";
        private const string ButtonCollapsedState = "ButtonCollapsed";

        private readonly PasswordBox _passwordBox;

        private bool _hideRevealButton;

        public PasswordBoxHelper(PasswordBox passwordBox)
        {
            _passwordBox = passwordBox;
        }

        #region PasswordRevealMode

        /// <summary>
        /// Gets a value that specifies whether the password is always, never, or
        /// optionally obscured.
        /// </summary>
        /// <param name="passwordBox">The element from which to read the property value.</param>
        /// <returns>
        /// A value of the enumeration that specifies whether the password is always, never,
        /// or optionally obscured. The default is **Peek**.
        /// </returns>
        public static PasswordRevealMode GetPasswordRevealMode(PasswordBox passwordBox)
        {
            return (PasswordRevealMode)passwordBox.GetValue(PasswordRevealModeProperty);
        }

        /// <summary>
        /// Sets a value that specifies whether the password is always, never, or
        /// optionally obscured.
        /// </summary>
        /// <param name="passwordBox">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetPasswordRevealMode(PasswordBox passwordBox, PasswordRevealMode value)
        {
            passwordBox.SetValue(PasswordRevealModeProperty, value);
        }

        /// <summary>
        /// Identifies the PasswordRevealMode dependency property.
        /// </summary>
        public static readonly DependencyProperty PasswordRevealModeProperty =
            DependencyProperty.RegisterAttached(
                "PasswordRevealMode",
                typeof(PasswordRevealMode),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(PasswordRevealMode.Peek, OnPasswordRevealModeChanged));

        private static void OnPasswordRevealModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var helper = GetHelperInstance((PasswordBox)d);
            helper?.UpdateVisualState();
        }

        #endregion

        #region IsEnabled

        public static bool GetIsEnabled(PasswordBox passwordBox)
        {
            return (bool)passwordBox.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(PasswordBox passwordBox, bool value)
        {
            passwordBox.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = (PasswordBox)d;
            if ((bool)e.NewValue)
            {
                SetHelperInstance(passwordBox, new PasswordBoxHelper(passwordBox));
            }
            else
            {
                passwordBox.ClearValue(HelperInstanceProperty);
            }
        }

        #endregion

        #region PlaceholderTextVisibility

        public static Visibility GetPlaceholderTextVisibility(Control control)
        {
            return (Visibility)control.GetValue(PlaceholderTextVisibilityProperty);
        }

        private static void SetPlaceholderTextVisibility(Control control, Visibility value)
        {
            control.SetValue(PlaceholderTextVisibilityPropertyKey, value);
        }

        private static readonly DependencyPropertyKey PlaceholderTextVisibilityPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "PlaceholderTextVisibility",
                typeof(Visibility),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty PlaceholderTextVisibilityProperty = PlaceholderTextVisibilityPropertyKey.DependencyProperty;

        #endregion

        #region HelperInstance

        private static PasswordBoxHelper GetHelperInstance(PasswordBox passwordBox)
        {
            return (PasswordBoxHelper)passwordBox.GetValue(HelperInstanceProperty);
        }

        private static void SetHelperInstance(PasswordBox passwordBox, PasswordBoxHelper value)
        {
            passwordBox.SetValue(HelperInstanceProperty, value);
        }

        private static readonly DependencyProperty HelperInstanceProperty =
            DependencyProperty.RegisterAttached(
                "HelperInstance",
                typeof(PasswordBoxHelper),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(OnHelperInstanceChanged));

        private static void OnHelperInstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is PasswordBoxHelper oldHelper)
            {
                oldHelper.Detach();
            }

            if (e.NewValue is PasswordBoxHelper newHelper)
            {
                newHelper.Attach();
            }
        }

        #endregion

        private TextBox TextBox { get; set; }

        private PasswordRevealMode PasswordRevealMode => GetPasswordRevealMode(_passwordBox);

        private void Attach()
        {
            _passwordBox.PasswordChanged += OnPasswordChanged;
            _passwordBox.GotFocus += OnGotFocus;
            _passwordBox.LostFocus += OnLostFocus;

            if (_passwordBox.IsLoaded)
            {
                OnApplyTemplate();
            }
            else
            {
                _passwordBox.Loaded += OnLoaded;
            }
        }

        private void Detach()
        {
            _passwordBox.PasswordChanged -= OnPasswordChanged;
            _passwordBox.GotFocus -= OnGotFocus;
            _passwordBox.LostFocus -= OnLostFocus;
            _passwordBox.Loaded -= OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _passwordBox.Loaded -= OnLoaded;
            OnApplyTemplate();
        }

        private void OnApplyTemplate()
        {
            _passwordBox.ApplyTemplate();

            var template = _passwordBox.Template;
            if (template != null)
            {
                TextBox = GetTemplateChild(nameof(TextBox)) as TextBox;

                if (TextBox != null)
                {
                    TextBox.TextChanged += OnTextChanged;
                }

                UpdateTextBox();
                UpdateVisualState();
            }
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_passwordBox.Password))
            {
                _hideRevealButton = true;
            }

            UpdateVisualState();
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            UpdateVisualState();
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            bool hasPassword = !string.IsNullOrEmpty(_passwordBox.Password);

            if (!hasPassword)
            {
                _hideRevealButton = false;
            }

            SetPlaceholderTextVisibility(_passwordBox, hasPassword ? Visibility.Collapsed : Visibility.Visible);
            UpdateTextBox();
            UpdateVisualState();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (PasswordRevealMode == PasswordRevealMode.Visible)
            {
                _passwordBox.Password = ((TextBox)sender).Text;
            }
        }

        private void UpdateTextBox()
        {
            if (TextBox != null && PasswordRevealMode != PasswordRevealMode.Visible)
            {
                TextBox.Text = _passwordBox.Password;
            }
        }

        private void UpdateVisualState()
        {
            bool buttonVisible = false;
            if (_passwordBox.IsFocused)
            {
                switch (PasswordRevealMode)
                {
                    case PasswordRevealMode.Peek:
                        buttonVisible = !_hideRevealButton && !string.IsNullOrEmpty(_passwordBox.Password);
                        break;
                    case PasswordRevealMode.Hidden:
                    case PasswordRevealMode.Visible:
                        buttonVisible = false;
                        break;
                }
            }

            VisualStateManager.GoToState(_passwordBox, buttonVisible ? ButtonVisibleState : ButtonCollapsedState, true);
        }

        private DependencyObject GetTemplateChild(string childName)
        {
            return _passwordBox.Template?.FindName(childName, _passwordBox) as DependencyObject;
        }
    }
}
