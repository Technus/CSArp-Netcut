﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using SharpPcap;
using PacketDotNet;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

/*
 Reference:
 http://stackoverflow.com/questions/14114971/sending-my-own-arp-packet-using-sharppcap-and-packet-net
 https://www.codeproject.com/Articles/12458/SharpPcap-A-Packet-Capture-Framework-for-NET
*/

namespace CSArp
{
    public static class GetClientList
    {
        private static ICaptureDevice capturedevice;
        private static IPAddress currentAddress;
        private static IPV4Subnet subnet;
        private static Dictionary<IPAddress, PhysicalAddress> clientlist;

        /// <summary>
        /// Populates listview with machines connected to the LAN
        /// </summary>
        /// <param name="view"></param>
        /// <param name="interfacefriendlyname"></param>
        public static void GetAllClients(IView view, string interfacefriendlyname)
        {
            DebugOutputClass.Print(view, "Refresh client list");
            #region initialization
            view.MainForm.Invoke(new Action(() => view.ToolStripStatusScan.Text = "Please wait..."));
            view.MainForm.Invoke(new Action(() => view.ToolStripProgressBarScan.Value = 0));
            view.ListView1.Items.Clear();
            #endregion

            #region Sending ARP requests to probe for all possible IP addresses on LAN
            new Thread(() =>
             {
                 try
                 {
                     foreach (var ipAddress in subnet.ToList())
                     {
                         var arprequestpacket = new ARPPacket(ARPOperation.Request, PhysicalAddress.Parse("00-00-00-00-00-00"), ipAddress, capturedevice.MacAddress, currentAddress);
                         var ethernetpacket = new EthernetPacket(capturedevice.MacAddress, PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF"), EthernetPacketType.Arp);
                         ethernetpacket.PayloadPacket = arprequestpacket;
                         capturedevice.SendPacket(ethernetpacket);
                         Debug.WriteLine("ARP request is sent to: {0}", ipAddress);
                     }
                 }
                 catch (Exception ex)
                 {
                     DebugOutputClass.Print(view, "Exception at GetClientList.GetAllClients() inside new Thread(()=>{}) while sending packets probably because old thread was still running while capturedevice was closed due to subsequent refresh [" + ex.Message + "]");
                 }
             }).Start();
            #endregion

            #region Retrieving ARP packets floating around and finding out the senders' IP and MACs
            capturedevice.Filter = "arp";
            RawCapture rawcapture = null;
            long scanduration = 5000;
            new Thread(() =>
            {
                try
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while ((rawcapture = capturedevice.GetNextPacket()) != null && stopwatch.ElapsedMilliseconds <= scanduration)
                    {
                        var packet = Packet.ParsePacket(rawcapture.LinkLayerType, rawcapture.Data);
                        var arppacket = (ARPPacket)packet.Extract(typeof(ARPPacket));
                        if (!clientlist.ContainsKey(arppacket.SenderProtocolAddress) && arppacket.SenderProtocolAddress.ToString() != "0.0.0.0" && subnet.Contains(arppacket.SenderProtocolAddress))
                        {
                            DebugOutputClass.Print(view, "Added " + arppacket.SenderProtocolAddress.ToString() + " @ " + GetMACString(arppacket.SenderHardwareAddress));
                            clientlist.Add(arppacket.SenderProtocolAddress, arppacket.SenderHardwareAddress);
                            view.ListView1.Invoke(new Action(() =>
                            {
                                view.ListView1.Items.Add(new ListViewItem(new string[] { clientlist.Count.ToString(), arppacket.SenderProtocolAddress.ToString(), GetMACString(arppacket.SenderHardwareAddress), "On", ApplicationSettingsClass.GetSavedClientNameFromMAC(GetMACString(arppacket.SenderHardwareAddress)) }));
                            }));
                            //Debug.Print("{0} @ {1}", arppacket.SenderProtocolAddress, arppacket.SenderHardwareAddress);
                        }
                        //int percentageprogress = (int)((float)stopwatch.ElapsedMilliseconds / scanduration * 100);
                        //view.MainForm.Invoke(new Action(() => view.ToolStripStatusScan.Text = "Scanning " + percentageprogress + "%"));
                        //view.MainForm.Invoke(new Action(() => view.ToolStripProgressBarScan.Value = percentageprogress));
                        //Debug.Print(packet.ToString() + "\n");
                    }
                    stopwatch.Stop();
                    view.MainForm.Invoke(new Action(() => view.ToolStripStatusScan.Text = clientlist.Count.ToString() + " device(s) found"));
                    view.MainForm.Invoke(new Action(() => view.ToolStripProgressBarScan.Value = 100));
                    BackgroundScanStart(view, interfacefriendlyname); //start passive monitoring
                }
                catch (PcapException ex)
                {
                    DebugOutputClass.Print(view, "PcapException @ GetClientList.GetAllClients() @ new Thread(()=>{}) while retrieving packets [" + ex.Message + "]");
                    view.MainForm.Invoke(new Action(() => view.ToolStripStatusScan.Text = "Refresh for scan"));
                    view.MainForm.Invoke(new Action(() => view.ToolStripProgressBarScan.Value = 0));
                }
                catch (Exception ex)
                {
                    DebugOutputClass.Print(view, ex.Message);
                }

            }).Start();
            #endregion
        }

        /// <summary>
        /// Actively monitor ARP packets for signs of new clients after GetAllClients active scan is done
        /// </summary>
        public static void BackgroundScanStart(IView view, string interfacefriendlyname)
        {
            try
            {
                #region Sending ARP requests to probe for all possible IP addresses on LAN
                new Thread(() =>
                {
                    try
                    {
                        while (capturedevice != null)
                        {
                            foreach (var ipAddress in subnet.ToList())
                            {
                                var arprequestpacket = new ARPPacket(ARPOperation.Request, PhysicalAddress.Parse("00-00-00-00-00-00"), ipAddress, capturedevice.MacAddress, currentAddress);
                                var ethernetpacket = new EthernetPacket(capturedevice.MacAddress, PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF"), EthernetPacketType.Arp);
                                ethernetpacket.PayloadPacket = arprequestpacket;
                                capturedevice.SendPacket(ethernetpacket);
                                Debug.WriteLine("ARP request is sent to: {0}", ipAddress);
                            }
                        }
                    }
                    catch (PcapException ex)
                    {
                        DebugOutputClass.Print(view, "PcapException @ GetClientList.BackgroundScanStart() probably due to capturedevice being closed by refreshing or by exiting application [" + ex.Message + "]");
                    }
                    catch (Exception ex)
                    {
                        DebugOutputClass.Print(view, "Exception at GetClientList.BackgroundScanStart() inside new Thread(()=>{}) while sending packets [" + ex.Message + "]");
                    }
                }).Start();
                #endregion

                #region Assign OnPacketArrival event handler and start capturing
                capturedevice.OnPacketArrival += (object sender, CaptureEventArgs e) =>
                {
                    var packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                    var arppacket = (ARPPacket)packet.Extract(typeof(ARPPacket));
                    if (!clientlist.ContainsKey(arppacket.SenderProtocolAddress) && arppacket.SenderProtocolAddress.ToString() != "0.0.0.0" && subnet.Contains(arppacket.SenderProtocolAddress))
                    {
                        DebugOutputClass.Print(view, "Added " + arppacket.SenderProtocolAddress.ToString() + " @ " + GetMACString(arppacket.SenderHardwareAddress) + " from background scan!");
                        clientlist.Add(arppacket.SenderProtocolAddress, arppacket.SenderHardwareAddress);
                        view.ListView1.Invoke(new Action(() => view.ListView1.Items.Add(new ListViewItem(new string[] { (clientlist.Count).ToString(), arppacket.SenderProtocolAddress.ToString(), GetMACString(arppacket.SenderHardwareAddress), "On", ApplicationSettingsClass.GetSavedClientNameFromMAC(GetMACString(arppacket.SenderHardwareAddress)) }))));
                        view.MainForm.Invoke(new Action(() => view.ToolStripStatusScan.Text = clientlist.Count + " device(s) found"));
                    }
                };
                capturedevice.StartCapture();
                #endregion

            }
            catch (Exception ex)
            {
                DebugOutputClass.Print(view, "Exception at GetClientList.BackgroundScanStart() [" + ex.Message + "]");
            }

        }

        /// <summary>
        /// Stops any ongoing capture and closes capturedevice if open
        /// </summary>
        public static void CloseAllCaptures()
        {
            try
            {
                capturedevice.StopCapture();
                capturedevice.Close();
            }
            catch { }
        }

        public static void PopulateCaptureDeviceInfo(IView view, string interfacefriendlyname)
        {
            if (capturedevice != null)
            {
                try
                {
                    capturedevice.StopCapture(); //stop previous capture
                    capturedevice.Close(); //close previous instances
                }
                catch (PcapException ex)
                {
                    DebugOutputClass.Print(view, "Exception at GetAllClients while trying to capturedevice.StopCapture() or capturedevice.Close() [" + ex.Message + "]");
                }
            }
            clientlist = new Dictionary<IPAddress, PhysicalAddress>(); //this is preventing redundant entries into listview and for counting total clients
            var capturedevicelist = CaptureDeviceList.Instance;
            capturedevicelist.Refresh(); //crucial for reflection of any network changes
            capturedevice = (from devicex in capturedevicelist where ((SharpPcap.WinPcap.WinPcapDevice)devicex).Interface.FriendlyName == interfacefriendlyname select devicex).ToList()[0];
            capturedevice.Open(DeviceMode.Promiscuous, 1000); //open device with 1000ms timeout
            var addresses = ((SharpPcap.WinPcap.WinPcapDevice)capturedevice).Addresses[1];
            currentAddress = addresses.Addr.ipAddress; //possible critical point : Addresses[1] in hardcoding the index for obtaining ipv4 address
            if (subnet == null)
            {
                subnet = new IPV4Subnet(currentAddress, new IPAddress(addresses.Netmask.ipAddress.GetAddressBytes().Reverse().ToArray())); // Sharppcap returns reversed mask
            }
        }

        /// <summary>
        /// Converts a PhysicalAddress to colon delimited string like FF:FF:FF:FF:FF:FF
        /// </summary>
        /// <param name="physicaladdress"></param>
        /// <returns></returns>
        private static string GetMACString(PhysicalAddress physicaladdress)
        {
            try
            {
                var retval = "";
                for (var i = 0; i <= 5; i++)
                {
                    retval += physicaladdress.GetAddressBytes()[i].ToString("X2") + ":";
                }

                return retval.Substring(0, retval.Length - 1);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
