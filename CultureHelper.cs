using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Web;

namespace Localize
{
    class CultureHelper : IHttpModule
    {
        private NameValueCollection _appSettings = ConfigurationManager.AppSettings;

        public void Dispose() {
            
        }

        public void Init(HttpApplication context) {                    
            context.PreRequestHandlerExecute += new EventHandler(Context_PreRequestHandlerExecute);
        }

        private void Context_PreRequestHandlerExecute(object sender, EventArgs e) {                                    
            Thread currentThread = Thread.CurrentThread;
            
            string cultureName = _appSettings[$"culture.cultureName"] ?? currentThread.CurrentCulture.Name;

            // Make a copy of the current culture to prevent modifying read-only values
            currentThread.CurrentCulture = new CultureInfo(cultureName);

            // Set the application domain thread culture in case the code creates new threads
            CultureInfo.DefaultThreadCurrentCulture = currentThread.CurrentCulture;

            // Override the properties of the NumberFormatInfo object
            OverrideCultureProperties(cultureName, currentThread.CurrentCulture.NumberFormat);
            
            // Override the properties of the DateTimeFormatInfo object
            OverrideCultureProperties(cultureName, currentThread.CurrentCulture.DateTimeFormat);            
        }

        /// <summary>
        /// Overrides the default culture format for dates and numbers. 
        /// Most properties of the NumberFormatInfo or DateFormatInfo objects can be overridden via the web.config file.
        /// </summary>
        /// <param name="cultureName">The (case-insensitive) name of the culture, eg: en-ZA, en-US, en-FR.</param>
        /// <param name="targetObject">The object whose properties will be reflected upon to find a match in the config file ie: NumberFormat, DateTimeFormat</param>
        private void OverrideCultureProperties(string cultureName, object targetObject) {
            // Get all the properties that can be set
            PropertyInfo[] propertyInfos = targetObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            
            // Iterate through each property
            foreach(var propertyInfo in propertyInfos) {
                // See if we have a corresponding value for this in the appSettings section of
                // the config file, eg:
                // <add key="culture.en-US.CurrencySymbol" value="$" />

                // First check for the global override for this property, eg:
                // <add key="culture..CurrencySymbol" value="Z" />
                string value = _appSettings[$"culture..{propertyInfo.Name}"];

                // If there is no global override then check for the culture-specific override for this property
                if(string.IsNullOrEmpty(value)) {                    
                    value = _appSettings[$"culture.{cultureName}.{propertyInfo.Name}"];
                }

                // If there is a value configured for this property, attempt to set it
                if(!string.IsNullOrEmpty(value)) {
                    try {
                        // Attempt to determine the type of the property
                        Type type = propertyInfo.PropertyType;

                        // If it is a complex type, attempt to determine the base type for the property (string, int etc)
                        Type uType = Nullable.GetUnderlyingType(type);
                        if(uType != null) {
                            type = uType;
                        }

                        // Set the value according to the type
                        switch(type.FullName) {                            
                            case "System.Int32":
                                propertyInfo.SetValue(targetObject, Int32.Parse(value), null);
                                break;
                            case "System.Boolean":
                                propertyInfo.SetValue(targetObject, Boolean.Parse(value), null);
                                break;
                            case "System.Int32[]": //NumberGroupSizes                                
                                propertyInfo.SetValue(targetObject, Array.ConvertAll(value.Split(','), int.Parse), null);
                                break;
                            default:
                                propertyInfo.SetValue(targetObject, value, null);
                                break;
                        }
                    } catch(Exception ex) {
                        // Log the error to the event log
                        string source = this.GetType().Name;

                        using(EventLog eventLog = new EventLog("Application")) {
                            eventLog.Source = "Application";
                            eventLog.WriteEntry(
                                $"{source} - Unable to apply value {value} to setting '{propertyInfo.Name}': {ex.GetBaseException()}",
                                EventLogEntryType.Error);
                        }                        
                    }
                }
            }
        }
    }
}
