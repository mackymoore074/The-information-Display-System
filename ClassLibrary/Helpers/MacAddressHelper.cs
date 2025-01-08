using System.Net.NetworkInformation;

namespace ClassLibrary.Helpers
{
    public static class MacAddressHelper
    {
        public static string GetMacAddress()
        {
            string macAddress = string.Empty;
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                var activeNic = nics.FirstOrDefault(x => 
                    x.OperationalStatus == OperationalStatus.Up && 
                    (x.NetworkInterfaceType == NetworkInterfaceType.Ethernet || 
                     x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211));

                if (activeNic != null)
                {
                    macAddress = string.Join(":", (
                        from z in activeNic.GetPhysicalAddress().GetAddressBytes() 
                        select z.ToString("X2")).ToArray());
                }
            }
            catch
            {
                macAddress = "Unknown";
            }

            return macAddress;
        }
    }
} 