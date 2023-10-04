using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotepadWPF.misc
{
    public static class Utils
    {
        public static string GetBatteryPercentage()
        {
            System.Management.ManagementClass wmi = new System.Management.ManagementClass("Win32_Battery");
            var allBatteries = wmi.GetInstances();

            int estimatedChargeRemaining = 0;
            foreach (var battery in allBatteries)
            {
                estimatedChargeRemaining+= Convert.ToInt32(battery["EstimatedChargeRemaining"]);
            }

            return (estimatedChargeRemaining/allBatteries.Count).ToString() + " %";
        }
    }
}
