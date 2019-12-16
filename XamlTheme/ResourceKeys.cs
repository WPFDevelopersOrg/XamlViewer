using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace XamlTheme
{
    public static class ResourceKeys
    {
        #region ButtonBase
        /// <summary>
        /// Style="{DynamicResource/StaticResource {x:Static themes:ResourceKeys.NoBgButtonStyleKey}}"
        /// </summary>
        public const string NoBgButtonBaseStyle = "NoBgButtonBaseStyle";
        public static ComponentResourceKey NoBgButtonBaseStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NoBgButtonBaseStyle); }
        }

        #endregion

        #region Button

        public const string NormalButtonStyle = "NormalButtonStyle";
        public static ComponentResourceKey NormalButtonStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalButtonStyle); }
        }

        public const string TitlebarButtonStyle = "TitlebarButtonStyle";
        public static ComponentResourceKey TitlebarButtonStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), TitlebarButtonStyle); }
        }

        public const string ToolbarButtonStyle = "ToolbarButtonStyle";
        public static ComponentResourceKey ToolbarButtonStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), ToolbarButtonStyle); }
        }

        public const string TitlebarCloseBtnStyle = "TitlebarCloseBtnStyle";
        public static ComponentResourceKey TitlebarCloseBtnStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), TitlebarCloseBtnStyle); }
        }

        public const string SelectionCloseBtnStyle = "SelectionCloseBtnStyle";
        public static ComponentResourceKey SelectionCloseBtnStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), SelectionCloseBtnStyle); }
        }

        #endregion

        #region RepeatButton

        public const string NormalRepeatButtonStyle = "NormalRepeatButtonStyle";
        public static ComponentResourceKey NormalRepeatButtonStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalRepeatButtonStyle); }
        }

        public const string ScrollRepeatButtonStyle = "ScrollRepeatButtonStyle";
        public static ComponentResourceKey ScrollRepeatButtonStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), ScrollRepeatButtonStyle); }
        }

        #endregion

        #region ToggleButton

        public const string NormalToggleButtonStyle = "NormalToggleButtonStyle";
        public static ComponentResourceKey NormalToggleButtonStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalToggleButtonStyle); }
        }

        #endregion

        #region RadioButton

        public const string NormalRadioButtonStyle = "NormalRadioButtonStyle";
        public static ComponentResourceKey NormalRadioButtonStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalRadioButtonStyle); }
        }

        #endregion

        #region ToggleStatus

        public const string NoBgToggleStatusStyle = "NoBgToggleStatusStyle";
        public static ComponentResourceKey NoBgToggleStatusStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NoBgToggleStatusStyle); }
        }

        #endregion

        #region CheckBox

        public const string NormalCheckBoxStyle = "NormalCheckBoxStyle";
        public static ComponentResourceKey NormalCheckBoxStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalCheckBoxStyle); }
        }

        #endregion

        #region SpitButton

        public const string MenuItemStyle = "MenuItemStyle";
        public static ComponentResourceKey MenuItemStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), MenuItemStyle); }
        }

        public const string HorSeparatorStyle = "HorSeparatorStyle";
        public static ComponentResourceKey HorSeparatorStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), HorSeparatorStyle); }
        }

        public const string VerSeparatorStyle = "VerSeparatorStyle";
        public static ComponentResourceKey VerSeparatorStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), VerSeparatorStyle); }
        }

        public const string MenuItemSubmenuContent = "MenuItemSubmenuContent";
        public static ComponentResourceKey MenuItemSubmenuContentKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), MenuItemSubmenuContent); }
        }

        public const string MenuItemTopLevelHeaderTemplate = "MenuItemTopLevelHeaderTemplate";
        public static ComponentResourceKey MenuItemTopLevelHeaderTemplateKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), MenuItemTopLevelHeaderTemplate); }
        }

        public const string MenuItemTopLevelItemTemplate = "MenuItemTopLevelItemTemplate";
        public static ComponentResourceKey MenuItemTopLevelItemTemplateKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), MenuItemTopLevelItemTemplate); }
        }

        public const string MenuItemSubmenuHeaderTemplate = "MenuItemSubmenuHeaderTemplate";
        public static ComponentResourceKey MenuItemSubmenuHeaderTemplateKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), MenuItemSubmenuHeaderTemplate); }
        }

        public const string MenuItemSubmenuItemTemplate = "MenuItemSubmenuItemTemplate";
        public static ComponentResourceKey MenuItemSubmenuItemTemplateKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), MenuItemSubmenuItemTemplate); }
        }

        #endregion

        #region ListBox

        public const string NormalListBoxStyle = "NormalListBoxStyle";
        public static ComponentResourceKey NormalListBoxStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalListBoxStyle); }
        }

        public const string NormalListBoxItemStyle = "NormalListBoxItemStyle";
        public static ComponentResourceKey NormalListBoxItemStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalListBoxItemStyle); }
        }

        #endregion

        #region ComboBox

        public const string NormalComboBoxStyle = "NormalComboBoxStyle";
        public static ComponentResourceKey NormalComboBoxStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalComboBoxStyle); }
        }

        public const string NormalComboBoxItemStyle = "NormalComboBoxItemStyle";
        public static ComponentResourceKey NormalComboBoxItemStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalComboBoxItemStyle); }
        }

        #endregion

        #region TabControl

        public const string NormalTabControlStyle = "NormalTabControlStyle";
        public static ComponentResourceKey NormalTabControlStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalTabControlStyle); }
        }

        public const string NormalTabItemStyle = "NormalTabItemStyle";
        public static ComponentResourceKey NormalTabItemStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalTabItemStyle); }
        }

        #endregion

        #region UserWindow

        public const string NormalUserWindowStyle = "NormalUserWindowStyle";
        public static ComponentResourceKey NormalUserWindowStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalUserWindowStyle); }
        }

        #endregion

        #region ScrollViewer

        public const string NormalScrollBarStyle = "NormalScrollBarStyle";
        public static ComponentResourceKey NormalScrollBarStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalScrollBarStyle); }
        }

        public const string NormalScrollViewerStyle = "NormalScrollViewerStyle";
        public static ComponentResourceKey NormalScrollViewerStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalScrollViewerStyle); }
        }

        #endregion

        #region Slider

        public const string NormalSliderStyle = "NormalSliderStyle";
        public static ComponentResourceKey NormalSliderStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalSliderStyle); }
        }

        #endregion

        #region GridSplitter

        public const string NormalGridSplitterStyle = "NormalGridSplitterStyle";
        public static ComponentResourceKey NormalGridSplitterStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalGridSplitterStyle); }
        }

        #endregion

        #region TextBox

        public const string NormalTextBoxStyle = "NormalTextBoxStyle";
        public static ComponentResourceKey NormalTextBoxStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalTextBoxStyle); }
        }

        #endregion

        #region Brush

        public const string AlphaVisualBrush = "AlphaVisualBrush";
        public static ComponentResourceKey AlphaVisualBrushKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), AlphaVisualBrush); }
        }

        #endregion
    }
}
