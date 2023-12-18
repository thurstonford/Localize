# Localize.NET

IIS module that enables granular control of culture-specific formats via the config file.

By default, ASP.NET only supports globalization in the web.config file, ie: forcing the application to a specific culture eg: en-US, en-FR, en-RU etc.
The application then uses the regional settings of the host for displaying currency, number and date time information.
Issues can arise with how currency, number and date/time information is handled if these regional settings differ between hosts (eg: migrating sites/services from one server to another).
This IIS module allows the developer to override the format of the individual NumberFormatInfo and DateTimeFormatInfo properties in the web.config file.

## Features
- Granular control of currency, number and date/time formats for multiple cultures.
- Multi-process support (worker threads also honour your specified formats).
- Override values of all public properties of the NumberFormatInfo and DateTimeFormatInfo objects.
- No code changes required.

## Getting Started

Add the Localize.dll file to the bin directory of your ASP.NET web application or web service.
Add config (see below).
Test.

## Add Config

Register the module with IIS via your web.config file:

    <!-- Required -->
    <system.webServer>
        <modules>
            <add name="Localize" type="Localize.CultureHelper" />            
        </modules>        
    </system.webServer>

Override the hosts regional settings with custom values defined in your web.config file, eg:         

    <!-- Force a specific culture (optional) -->
    <add key="culture.cultureName" value="en-FR" />
    
    <!-- Override culture-specific settings (optional, case-insensitive) -->
    <add key="culture.en-za.NumberDecimalSeparator" value="." />
    <add key="culture.en-za.numberdecimalDigits" value="99" />
    <add key="culture.en-za.currencySymbol" value="#" />
    <add key="culture.en-za.ShortDatePattern" value="MM-dd-yyyy" />
    <add key="culture.en-za.ShortTimePattern" value="HHmm" />
    <add key="culture.en-za.MonthDayPattern" value="MMM d" />
    
    <add key="culture.en-US.currencySymbol" value="#" />
    
    <add key="culture.en-fr.currencySymbol" value="@" />


## Logging:

Exceptions are handled gracefully and logged to the Windows Event Log.

## Additional documentation

- [Microsoft Globalization documentation](https://learn.microsoft.com/en-us/dotnet/api/system.globalization?view=netframework-4.8.1) 
- [Microsoft CultureInfo documentation](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo?view=netframework-4.8.1)

## Feedback

I welcome comments, suggestions, feature requests and honest criticism :)  

 
- [Github Repo](https://github.com/thurstonford?tab=repositories)  
- Email: lance@cogware.co.za

## Want to show your appreciation?
That's mighty generous - thank you!  
[Buy me a coffee](https://www.buymeacoffee.com/cogware)