﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Easynet.Edge.Services.DataRetrieval.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://adwords.google.com/api/adwords/v13/AccountService")]
        public string Edge_Services_DataRetrieval_GAdWordsAccountServiceV13_AccountService {
            get {
                return ((string)(this["Edge_Services_DataRetrieval_GAdWordsAccountServiceV13_AccountService"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://adwords.google.com/api/adwords/v13/ReportService")]
        public string Edge_Services_DataRetrieval_GAdWordsReportServiceV13_ReportService {
            get {
                return ((string)(this["Edge_Services_DataRetrieval_GAdWordsReportServiceV13_ReportService"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://classic.easy-forex.com/BackOffice/API/Marketing.asmx")]
        public string Edge_Services_DataRetrieval_EasyForexBackOfficeAPI_Marketing {
            get {
                return ((string)(this["Edge_Services_DataRetrieval_EasyForexBackOfficeAPI_Marketing"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://classic.easy-forex.com/BackOffice/API/Marketing.asmx")]
        public string Edge_Services_DataRetrieval_com_prp1_ezfx_classic_Marketing {
            get {
                return ((string)(this["Edge_Services_DataRetrieval_com_prp1_ezfx_classic_Marketing"]));
            }
        }
    }
}
