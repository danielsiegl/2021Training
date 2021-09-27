﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ModelMetric.Test.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.10.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Models\\PWC.eapx")]
        public string ModelPath {
            get {
                return ((string)(this["ModelPath"]));
            }
            set {
                this["ModelPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"SELECT DISTINCT t_object.ea_guid,t_object.Name, t_object.Object_Type FROM t_object , t_connector WHERE t_object.Object_Type = 'Requirement' AND t_object.Object_ID NOT IN(SELECT t_connector.End_Object_ID FROM t_connector WHERE t_connector.Connector_Type = 'Realisation' OR(t_connector.Connector_Type = 'Dependency' AND t_connector.Stereotype = 'trace'))")]
        public string All_Requirements_NOT_connected_with_Realisation_OR_trace_to_other_elements {
            get {
                return ((string)(this["All_Requirements_NOT_connected_with_Realisation_OR_trace_to_other_elements"]));
            }
            set {
                this["All_Requirements_NOT_connected_with_Realisation_OR_trace_to_other_elements"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://test.lieberlieber.com/webea?m=1&o={0}")]
        public string WebEAURL {
            get {
                return ((string)(this["WebEAURL"]));
            }
            set {
                this["WebEAURL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"SELECT DISTINCT  a.ea_guid as CLASSGUID,   a.Name as Name, a.Alias as Alias_Name, a.Object_Type as CLASSTYPE, t_package.Name as Package_Name FROM t_object as a, t_object as b,  t_package WHERE a.Name = b.Name AND a.ea_guid <> b.ea_guid AND a.Object_Type = b.Object_Type AND a.Package_ID = b.Package_ID AND a.Package_ID = t_package.Package_ID AND a.name <> ""External Reference"" order by a.name")]
        public string duplicate_Name_in_Package {
            get {
                return ((string)(this["duplicate_Name_in_Package"]));
            }
            set {
                this["duplicate_Name_in_Package"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"SELECT t_object.ea_guid, t_object.Name, count(t_object.Name) as ""Incomming ControlFlows"" FROM t_object INNER JOIN t_connector on (t_object.Object_ID = t_connector.End_Object_ID) WHERE t_object.Object_Type = 'Action' AND t_connector.Connector_Type = 'ControlFlow' GROUP BY t_object.ea_guid,t_object.Name")]
        public string All_Actions_with_more_than_one_incomming_ControlFlow {
            get {
                return ((string)(this["All_Actions_with_more_than_one_incomming_ControlFlow"]));
            }
            set {
                this["All_Actions_with_more_than_one_incomming_ControlFlow"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("select t_object.ea_guid,t_object.Name from t_object where (t_object.Note IS NULL " +
            "or t_object.Note = \'\') and t_object.Object_Type = \'UseCase\' Order by t_object.Na" +
            "me")]
        public string All_UseCases_without_Notes {
            get {
                return ((string)(this["All_UseCases_without_Notes"]));
            }
            set {
                this["All_UseCases_without_Notes"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://customers.lieberlieber.com/openinea/?eaguid={0}")]
        public string LocalEAUrl {
            get {
                return ((string)(this["LocalEAUrl"]));
            }
            set {
                this["LocalEAUrl"] = value;
            }
        }
    }
}
