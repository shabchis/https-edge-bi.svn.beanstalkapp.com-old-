﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1433
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Easynet.Edge.Services.Yahoo.Panama.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
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
        [global::System.Configuration.DefaultSettingValueAttribute("https://global.marketing.ews.yahooapis.com/services/V4/LocationService")]
        public string Edge_Services_Yahoo_Panama_PanamaLocationServiceV4_LocationServiceService {
            get {
                return ((string)(this["Edge_Services_Yahoo_Panama_PanamaLocationServiceV4_LocationServiceService"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://USE_ADDRESS_RETURNED_BY_LOCATION_SERVICE/services/V4/BasicReportService")]
        public string Edge_Services_Yahoo_Panama_PanamaReportServiceV4_BasicReportServiceService {
            get {
                return ((string)(this["Edge_Services_Yahoo_Panama_PanamaReportServiceV4_BasicReportServiceService"]));
            }
        }
    }
}