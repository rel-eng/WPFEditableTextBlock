/* WPFEditableTextBlock automation peer implementation.

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
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace Custom.Controls
{
    public class EditableTextBoxAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
    {
        public EditableTextBoxAutomationPeer(EditableTextBlock control) : base(control)
        {
        }

        protected override string GetClassNameCore()
        {
            return "EditableTextBlock";
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Edit;
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value)
            {
                return this;
            }
            return base.GetPattern(patternInterface);
        }

        #region IValueProvider

        void IValueProvider.SetValue(string value)
        {
            if (!base.IsEnabled())
            {
                throw new ElementNotEnabledException();
            }
            EditableTextBlock owner = base.Owner as EditableTextBlock;
            if (owner.IsReadOnly)
            {
                throw new ElementNotEnabledException();
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            owner.Text = value;
        }

        bool IValueProvider.IsReadOnly
        {
            get
            {
                EditableTextBlock owner = base.Owner as EditableTextBlock;
                return owner.IsReadOnly;
            }
        }

        string IValueProvider.Value
        {
            get
            {
                EditableTextBlock owner = base.Owner as EditableTextBlock;
                return owner.Text;
            }
        }

        #endregion IValueProvider
    }
}
