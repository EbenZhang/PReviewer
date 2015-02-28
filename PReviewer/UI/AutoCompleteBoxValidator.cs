using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using WpfCommon.Extensions;
using Xceed.Wpf.AvalonDock.Controls;

namespace PReviewer.UI
{
    public static class AutoCompleteBoxValidator
    {
        public static bool Valid(this AutoCompleteBox box)
        {
            if (!box.IsEnabled || !box.IsVisible)
            {
                return true;
            }

            return ValidText(box) && ValidSelectedItem(box);
        }

        private static bool ValidText(AutoCompleteBox tb)
        {
            var binding = BindingOperations.GetBinding(tb, AutoCompleteBox.TextProperty);
            if (binding == null) return true;

            var bingdExp = tb.GetBindingExpression(AutoCompleteBox.TextProperty);
            if (bingdExp == null) return true;

            Validation.ClearInvalid(bingdExp);
            foreach (var rule in binding.ValidationRules)
            {
                var tempRuleForClosureCapture = rule;
                var validResult = rule.Validate(tb.Text, CultureInfo.CurrentCulture);

                if (validResult.IsValid) continue;

                tb.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                {
                    tb.BringIntoView();
                    var validationError = new ValidationError(tempRuleForClosureCapture, binding, validResult.ErrorContent, null);
                    Validation.MarkInvalid(bingdExp, validationError);
                    tb.Focus();
                }));
                return false;
            }
            return true;
        }

        private static bool ValidSelectedItem(AutoCompleteBox tb)
        {
            var binding = BindingOperations.GetBinding(tb, AutoCompleteBox.TextProperty);
            if (binding == null) return true;

            var bingdExp = tb.GetBindingExpression(AutoCompleteBox.TextProperty);
            if (bingdExp == null) return true;

            Validation.ClearInvalid(bingdExp);
            foreach (var rule in binding.ValidationRules)
            {
                var tempRuleForClosureCapture = rule;
                var validResult = rule.Validate(tb.Text, CultureInfo.CurrentCulture);

                if (validResult.IsValid) continue;

                tb.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                {
                    tb.BringIntoView();
                    var validationError = new ValidationError(tempRuleForClosureCapture, binding, validResult.ErrorContent, null);
                    Validation.MarkInvalid(bingdExp, validationError);
                    tb.Focus();
                }));
                return false;
            }
            return true;
        }

        public static bool ValidateAutoCompleteBoxes(this DependencyObject container)
        {
            var tbs = container.FindVisualChildren<AutoCompleteBox>();
            return tbs.All(tb => tb.Valid());
        }
    }
}
