﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34011
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BigData.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("XYBOEZiodAgSpDI9gLiQcv6o4r78ZuHOELWT2c7F5F9iqIx7VXnbXrt4a2HTpUYCDSKOwoD25joHpkVy")]
        public string WSKey {
            get {
                return ((string)(this["WSKey"]));
            }
            set {
                this["WSKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://bucknell.worldcat.org/profiles/librarybookdisplay/lists/3233826/")]
        public string RSSUri {
            get {
                return ((string)(this["RSSUri"]));
            }
            set {
                this["RSSUri"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("librarydisplaydonotreply@gmail.com")]
        public string MailFrom {
            get {
                return ((string)(this["MailFrom"]));
            }
            set {
                this["MailFrom"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Library Digital Display")]
        public string MailName {
            get {
                return ((string)(this["MailName"]));
            }
            set {
                this["MailName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("readingisgood")]
        public string MailPassword {
            get {
                return ((string)(this["MailPassword"]));
            }
            set {
                this["MailPassword"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2000")]
        public int Count {
            get {
                return ((int)(this["Count"]));
            }
            set {
                this["Count"] = value;
            }
        }
    }
}
