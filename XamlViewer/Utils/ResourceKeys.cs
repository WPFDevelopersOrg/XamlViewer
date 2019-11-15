using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace XamlViewer.Themes
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

        public const string FgWhiteCloseBtnStyle = "FgWhiteCloseBtnStyle";
        public static ComponentResourceKey FgWhiteCloseBtnStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), FgWhiteCloseBtnStyle); }
        }

        public const string FgRedCloseBtnStyle = "FgRedCloseBtnStyle";
        public static ComponentResourceKey FgRedCloseBtnStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), FgRedCloseBtnStyle); }
        }

        #endregion

        #region RepeatButton

        public const string NormalRepeatButtonStyle = "NormalRepeatButtonStyle";
        public static ComponentResourceKey NormalRepeatButtonStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NormalRepeatButtonStyle); }
        }

        #endregion

        #region ToggleStatus

        public const string NoBgToggleStatusStyle = "NoBgToggleStatusStyle";
        public static ComponentResourceKey NoBgToggleStatusStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NoBgToggleStatusStyle); }
        }

        #endregion

        #region SpitButton

        public const string MenuItemStyle = "MenuItemStyle";
        public static ComponentResourceKey MenuItemStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), MenuItemStyle); }
        }

        public const string SeparatorStyle = "SeparatorStyle";
        public static ComponentResourceKey SeparatorStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), SeparatorStyle); }
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
    }
}
