using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace OB.Services.Jobs.Server
{
    public class IPAddressUtility
    {
        // get local IP addresses
        private static IPAddress[] _localIPs = Dns.GetHostAddresses(Dns.GetHostName());
        private static string localhostName = "localhost";
        private static string localhostInterface = "127.0.0.1";
        private static string localhostInterfaceIPV6 = "::1";

        public static bool IsLocalIpAddress(string host, bool checkAllDnsHostAddresses = false)
        {
            return IsLocalIpAddress(IPAddress.Parse(host), checkAllDnsHostAddresses);
        }

        public static bool IsLocalIpAddress(IPAddress ip, bool checkAllDnsHostAddresses = false)
        {
            string host = ip.ToString();
            if (host.Equals(localhostName, StringComparison.InvariantCultureIgnoreCase) 
                || host.Equals(localhostInterface, StringComparison.InvariantCultureIgnoreCase) 
                || host.Equals(localhostInterfaceIPV6, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            try
            {
                if (checkAllDnsHostAddresses)
                {
                    //get host IP addresses
                    IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                    return IsLocalIpAddress(hostIPs);
                }
                else return IsLocalIpAddress(new IPAddress[] { ip });
            }
            catch { }
            return false;
        }

        public static bool IsLocalIpAddress(IPAddress[] hostIPs)
        {
            try
            {                 
                //test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIP in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    // is local address
                    foreach (IPAddress localIP in _localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch { }
            return false;
        }

  
        public static bool IsIPAddressInSubnet(string ipAddress, IPAddress[] subnets, IPAddress[] subnetmasks)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            return IsIPAddressInSubnet(ip, subnets, subnetmasks);
        }

        public static bool IsIPAddressInSubnet(IPAddress ipAddress, IPAddress[] subnets, IPAddress[] subnetmasks)
        {
            var ipBytes = ipAddress.GetAddressBytes();
            for (int i = 0; i < subnets.Length; i++)
            {
                if (ipAddress.AddressFamily == subnets[i].AddressFamily)
                {
                    var subnetBytes = subnets[i].GetAddressBytes();
                    var subnetmaskBytes = subnetmasks[i].GetAddressBytes();
                    int b;
                    for (b = 0; b < ipBytes.Length; b++)
                    {
                        if ((ipBytes[b] & subnetmaskBytes[b]) != (subnetBytes[b] & subnetmaskBytes[b]))
                        {
                            break;
                        }
                    }
                    if (b == ipBytes.Length)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static IPAddress ParseMask(IPAddress subnet, string maskString)
        {
            IPAddress mask;
            if (maskString.StartsWith("/"))
            {
                var maskBits = Int32.Parse(maskString.Substring(1), CultureInfo.InvariantCulture);
                if (maskBits < 0)
                {
                    throw new InvalidOperationException("Address mask prefix size is negative.");
                }
                byte[] maskBytes;
                switch (subnet.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        if (maskBits > 32)
                        {
                            throw new InvalidOperationException("Address mask prefix size is too big.");
                        }
                        maskBytes = new byte[4];
                        break;
                    case AddressFamily.InterNetworkV6:
                        if (maskBits > 128)
                        {
                            throw new InvalidOperationException("Address mask prefix size is too big.");
                        }
                        maskBytes = new byte[16];
                        break;
                    default:
                        throw new InvalidOperationException("Only IPv4 and IPv6 subnets are supported.");
                }
                for (int b = 0; maskBits > 0; b += 1, maskBits -= 8)
                {
                    byte addressByte = 0xFF;
                    if (maskBits < 8)
                    {
                        addressByte <<= 8 - maskBits;
                    }
                    maskBytes[b] = addressByte;
                }
                mask = new IPAddress(maskBytes);
            }
            else
            {
                mask = IPAddress.Parse(maskString);
                if (subnet.AddressFamily != mask.AddressFamily)
                {
                    throw new InvalidOperationException("Mismatch between subnet and mask address family.");
                }
            }
            return mask;
        }
    }
}
