using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using PrinterManager.Helpers;
using System.Management;
using PrinterManager.Database.Repositories;

namespace PrinterManager.Printing
{
    class Printer
    {
        String defaultPrinter = "";

        public Printer()
        {
            this.SetDefaultPrinter();
        }

        public String GetName()
        {
            return defaultPrinter;
        }

        public void Print(String zplString)
        {
            PrintHelper.SendStringToPrinter(this.defaultPrinter, zplString);
        }

        private void SetDefaultPrinter()
        {
            String printer = SettingsRepository.Get("DefaultPrinter");

            if (printer != "" && this.Validate(printer))
            {
                this.defaultPrinter = printer;
                return;
            }

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

        private bool Validate(String printerName)
        {
            PrinterSettings printer = new PrinterSettings();
            printer.PrinterName = printerName;

            ManagementScope mgmtscope = new ManagementScope(@"\root\cimv2");
            mgmtscope.Connect();
            ManagementObjectSearcher objsearcher = new ManagementObjectSearcher("Select * from Win32_Printer where Name like '%" + printerName + "%'");
            foreach (ManagementObject prntr in objsearcher.Get())
            {
                String t = prntr["WorkOffline"].ToString();
                if (prntr["WorkOffline"].ToString().Equals("true"))
                {
                    return false;
                }
            }

            return printer.IsValid;
        }

        private List<String> GetInstalledPrinters()
        {
            List<String> installedPrinters = new List<String>();

            String printer;

            for (int i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
            {
                printer = PrinterSettings.InstalledPrinters[i];
                installedPrinters.Add(printer);
            }

            return installedPrinters;
        }
    }
}
