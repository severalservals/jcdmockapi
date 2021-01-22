using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using PrinterManager.Helpers;
using System.Management;
using PrinterManager.Database.Repositories;

namespace PrinterManager.Printing
{
    // Triple-slashed XML comments with reserved keywords lend themsevles to auto-generated documentation
    // (see here: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/). Even if you don't 
    // plan now on auto-generating the documentation, it's a nice, readable format for comments and you're keeping your options open.
    // Generally I see them on public members, and that's what a lot of style guide software enforces, but I'll drop them on the internal
    // classes here. 

    /// <summary>
    /// Retrieves, or if necessary configures, a default printer and then allows consumers to print Zebra Printer Language (ZPL)-formatted data to it. 
    /// </summary>
    /// <remarks>See here for an overview of the Zebra Printer Language (ZPL): 
    /// https://www.zebra.com/us/en/support-downloads/knowledge-articles/zpl-command-information-and-details.html
    /// </remarks>
    class Printer
    // QUESTION/COMMENT - It's a little more readable to spell out that the access modifier for the class above is "internal" 
    // and spare someone a Google search for the default value.
    {
        // The name of the default printer we found or set, which is also the only printer this class will use. 
        String defaultPrinter = "";

        /// <summary>Create a new printer instance and set it to use the default printer or have it set a default printer if there isn't one.</summary>
        /// <remarks>
        /// The constructor will get the name of the default printer set in the printer repository. If there isn't one,
        /// it will search the list of installed printers for the first Zebra-named printer it can find and set that as
        /// the default printer in the printer repository. 
        /// </remarks>
        public Printer()
        {
            this.SetDefaultPrinter();
        }

        /// <summary>Get the name of the printer the instance is using to print. </summary>
        // QUESTION/COMMENT - You might want to call this GetDefaultPrinterName(), since that's the only printer you're ever going to see.
        public String GetName()
        {
            return defaultPrinter;
        }

        /// <summary>Prints a ZPL-formatted string to the default printer.</summary>
        /// <remarks>See here for an overview of the Zebra Printer Language (ZPL): 
        /// https://www.zebra.com/us/en/support-downloads/knowledge-articles/zpl-command-information-and-details.html
        /// </remarks>
        // QQUESTION/COMMENT - what happens if we coudln't find an installed Zebra printer? Will SendStringToPrinter() throw an exception?
        // That's good to note. 
        public void Print(String zplString)
        {
            
            PrintHelper.SendStringToPrinter(this.defaultPrinter, zplString);
        }

        // QUESTION/COMMENT - If part of what you're concerned about is code readability for new programmers coming on, then I think 
        // SetDefaultPrinter() could use a better name. Usually when you have a method that's called something like SetX, you're able to pass a 
        // value that X will take on. This method doesn't give the user any ability to say what the default printer should be.
        // Instead it discovers the configured default printer, if there is one, and otherwise tries to set the first valid Zebra printer
        // in its list as the default. 
        // GetDefaultPrinter() isn't a great name either since if there isn't a default printer configured, the code tries to set one
        // in the printer repository, which seems like it is an external data source where that change will live on past this class instance. 
        // A lot of people would not expect a Get method to have any data side-effects. 
        // RetrieveOrImplicitlySetDefaultPrinter() is a clunky name but it is accurate. 

        // Sets this instance to use the printer configured in the repository as the default. If no default is found,
        // it sets the first Zebra-named printer in the list of installed printers as the default printer in the repository 
        // and then uses that printer. 
        private void SetDefaultPrinter()
        {
            // Get the name of the default printer, if one has been configured, from the printer database repositorty. 
            // Question - could SettingsRepository.Get() return a null? 
            String printer = SettingsRepository.Get("DefaultPrinter");

            // If there is a default printer name and it's a valid name, then set it as the default printer
            // and get out. Otherwise, keep going. 
            if (printer != "" && this.Validate(printer))
            {
                this.defaultPrinter = printer;
                return;
            }

            // If we got this far, there was no default printer configured, so we're going to set one. 
            // Loop through all the installed printers. If you find one whose name contains "zebra" 
            // and whose name is also generally valid, set that as the default printer for this class
            // instance and then save it to the printer settings repository as the default printer. 
            // Zebra printers are (presumably) our stock printer brand. 

            // If there is no printer with a valid name that contains the string "zebra," then we will not
            // configure any default printer. 
            // Should that be an exception? There's no way in this class to specify a printer to use and the Print()
            // method presumes that you do have a default printer configured. If there's no default configured
            // and the class can't find one using the logic below, the class instance will essentially be broken.
            foreach (String printerName in this.GetInstalledPrinters())
            {
                if (printerName.ToLower().Contains("zebra") && this.Validate(printerName))
                {
                    this.defaultPrinter = printerName;
                    SettingsRepository.Save("DefaultPrinter", this.defaultPrinter);
                    break;
                }
            }
        }

        /// <summary>Deterime if the printer specified by printerName is a valid printer to use for printing.</summary>
        // QUESTION/COMMENT - I can tell you what this method does, but not why, so I'd have some questions about that. Please see below.
        private bool Validate(String printerName)
        {
            // QUESTION/COMMENT - I'd like to figure out why we set printer.PrinterName to printerName, so I could explain that. Maybe doing so means 
            // that when we check printer.IsValid, PrinterSettings will apply some additional logic of its own in determing if the  
            // printer called printerName is in fact a valid printer.
            PrinterSettings printer = new PrinterSettings();
            printer.PrinterName = printerName;

            // QUESTION/COMMENT - This is an area where I'd want to do some digging if this were real code, to explain where that magic string comes from and what it means. 
            // It looks from the order of the code like objsearcher needs the connection that mgtscope creates, but since those two objects don't interact 
            // in any obvious way I'm not sure how objsearcher is finding out about that connection. I'd want to know more about that.  
            ManagementScope mgmtscope = new ManagementScope(@"\root\cimv2");
            mgmtscope.Connect();
            // QUESTION/COMMENT - Do we need to worry about terminating the connection above? Looks like it's talking to an external resource.

            // QUESTION/COMMENT - I can tell you what the code below is doing, but I can't tell you why, so I have some questions about that. 
            // The code looks like it is getting a list of every printer whose name contains printerName anywhere within it. printerName will be
            // a printer from the list of installed printers whose name contains "zebra" (case-insensitive). The code here is getting a list of
            // printers from a different source than the GetInstalledPrinters() method, and I'm not sure what's different about the two 
            // different printer lists. 
            // Once the code gets the list of printers from its ManagemenObject query, it loops through it 
            // and if it finds any printer in the list with a WorkOffline property that is true, 
            // then it has the Validate() method return false. I guess if a printer can work offline, or has to work offline, you don't want it 
            // to be used as the default? Though any given printer you're looking at in the list return by the ManagementObject query might 
            // not even be the same printer as the one specified by printerName. 
            // We're just getting the management object list of printers from a wildcard search on printerName. That confuses me.
            ManagementObjectSearcher objsearcher = new ManagementObjectSearcher("Select * from Win32_Printer where Name like '%" + printerName + "%'");
            foreach (ManagementObject prntr in objsearcher.Get())
            {
                // QUESTION/COMMENT - We never use t. Why do we even declare it? It's not for the informative name.
                String t = prntr["WorkOffline"].ToString();
                if (prntr["WorkOffline"].ToString().Equals("true"))
                {
                    return false;
                }
            }

            // QUESTION/COMMENT - I'd want to do a little research to figure out what the criteria for being valid are, and maybe mention them. 
            return printer.IsValid;
        }

        /// <summary>Return a list of the names of all installed printers.</summary>
        private List<String> GetInstalledPrinters()
        {
            List<String> installedPrinters = new List<String>();
            
            String printer;

            // QUESTION/COMMENT - If we can use PrinterSettings here without instantiating it, then 
            // why do we instantiate it in Validate() above? 
            for (int i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
            { 
                printer = PrinterSettings.InstalledPrinters[i];
                installedPrinters.Add(printer);
            }

            return installedPrinters;
        }
    }
}
