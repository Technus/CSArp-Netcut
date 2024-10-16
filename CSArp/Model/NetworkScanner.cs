﻿using System;
using System.Collections.Generic;
using System.Net;
using SharpPcap;
using PacketDotNet;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using CSArp.View;
using CSArp.Model.Utilities;
using CSArp.Model.Extensions;
using System.Threading.Tasks;
using SharpPcap.Npcap;
using System.Linq;

/*
 Reference:
 http://stackoverflow.com/questions/14114971/sending-my-own-arp-packet-using-sharppcap-and-packet-net
 https://www.codeproject.com/Articles/12458/SharpPcap-A-Packet-Capture-Framework-for-NET
*/

namespace CSArp.Model;

// TODO: Add a scanning bool, to set the state for cancellation.
// TODO: Remove GUI related code out of the class.
public class NetworkScanner
{
    private const string prefix = "Scan";

    /// <summary>
    /// Populates listview with machines connected to the LAN
    /// </summary>
    /// <param name="view"></param>
    /// <param name="networkAdapter"></param>
    public void StartScan(IView view, NpcapDevice networkAdapter, IPAddress gatewayIp)
    {
        DebugOutput.Print("Refresh client list");
        #region initialization
        view.MainForm.Invoke(() =>
        {
            view.ToolStripStatus.Text = "Scanning";
            view.ToolStripStatusScan.Text = "Please wait...";
            view.ToolStripProgressBarScan.Value = 0;
        });
        view.ClientListView.Items.Clear();
        #endregion


        // Clear ARP table
        ArpTable.Instance.Clear();

        // Start Foreground Scan with Timeout involved
        StartForegroundScan(view, networkAdapter, gatewayIp, 5000);
    }

    private static void StartForegroundScan(IView view, NpcapDevice networkAdapter, IPAddress gatewayIp, int foregroundScanTimeout)
    {
        view.MainForm.Invoke(() =>
        {
            view.ToolStripStatus.Text = "Scanning In Foreground";
            view.ToolStripStatusScan.Text = "Please wait...";
            view.ToolStripProgressBarScan.Value = 0;
        });
        // Obtain subnet information
        var subnet = networkAdapter.ReadCurrentSubnet();

        // Obtain current IP address
        //var sourceAddress = networkAdapter.ReadCurrentIpV4Address();

        // TODO: Send and capture ICMP packages for both MAC address and alive status.
        #region Sending ARP requests to probe for all possible IP addresses on LAN
        var cts1 = new CancellationTokenSource();
        TaskBuffer.Add(cts1, () => InitiateArpRequestQueue(view, networkAdapter, gatewayIp, false, cts1.Token), prefix);
        #endregion

        #region Retrieving ARP packets floating around and finding out the senders' IP and MACs
        networkAdapter.Filter = "arp";
        var cts = new CancellationTokenSource();
        TaskBuffer.Add(cts, () => AwaitArpRequestQueue(view, networkAdapter, gatewayIp, foregroundScanTimeout, subnet, cts.Token), prefix);
        #endregion
    }

    private static async Task AwaitArpRequestQueue(IView view, NpcapDevice networkAdapter, IPAddress gatewayIp, int foregroundScanTimeout, IPV4Subnet subnet, CancellationToken token = default)
    {
        view.MainForm.Invoke(() =>
        {
            view.ToolStripStatus.Text = "Awaiting ARP";
            view.ToolStripStatusScan.Text = "Please wait...";
            view.ToolStripProgressBarScan.Value = 0;
        });
        try
        {
            RawCapture rawcapture = null;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!token.IsCancellationRequested && (rawcapture = networkAdapter.GetNextPacket()) != null && stopwatch.ElapsedMilliseconds <= foregroundScanTimeout)
            {
                var packet = Packet.ParsePacket(rawcapture.LinkLayerType, rawcapture.Data);
                var arppacket = packet.Extract<ArpPacket>();
                if (!ArpTable.Instance.ContainsKey(arppacket.SenderProtocolAddress) && arppacket.SenderProtocolAddress.ToString() != "0.0.0.0" && subnet.Contains(arppacket.SenderProtocolAddress))
                {
                    var isGateway = false;
                    if (arppacket.SenderProtocolAddress.Equals(gatewayIp))
                    {
                        DebugOutput.Print("Found gateway!");
                        isGateway = true;
                    }
                    DebugOutput.Print($"Added {arppacket.SenderProtocolAddress} @ {arppacket.SenderHardwareAddress.ToString("-")}");
                    _ = ArpTable.Instance.Add(arppacket.SenderProtocolAddress, arppacket.SenderHardwareAddress);
                    view.ClientListView.Invoke(new Action(() =>
                    {
                        _ = isGateway
                        ? view.ClientListView.Items.Add(new ListViewItem([
                                arppacket.SenderProtocolAddress.ToString(),
                                    arppacket.SenderHardwareAddress.ToString("-"),
                                    "On",
                                    "GATEWAY",
                                    DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss:fff"),
                            ]))
                        : view.ClientListView.Items.Add(new ListViewItem([
                                arppacket.SenderProtocolAddress.ToString(),
                                    arppacket.SenderHardwareAddress.ToString("-"),
                                    "On",
                                    ApplicationSettings.GetSavedClientNameFromMAC(arppacket.SenderHardwareAddress.ToString("-")),
                                    DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss:fff"),
                            ]));
                    }));
                    //Debug.Print("{0} @ {1}", arppacket.SenderProtocolAddress, arppacket.SenderHardwareAddress);
                }
                //int percentageprogress = (int)((float)stopwatch.ElapsedMilliseconds / scanduration * 100);
                //view.MainForm.Invoke(new Action(() => view.ToolStripStatusScan.Text = "Scanning " + percentageprogress + "%"));
                //view.MainForm.Invoke(new Action(() => view.ToolStripProgressBarScan.Value = percentageprogress));
                //Debug.Print(packet.ToString() + "\n");
            }
            stopwatch.Stop();
            view.MainForm.Invoke(() =>
            {
                view.ToolStripStatus.Text = "Done Awaiting";
                view.ToolStripStatusScan.Text = $"{ArpTable.Instance.Count} device(s) found";
                view.ToolStripProgressBarScan.Value = 100;
            });
            await StartBackgroundScan(view, networkAdapter, gatewayIp, token); //start passive monitoring
        }
        catch (PcapException ex)
        {
            DebugOutput.Print($"PcapException @ GetClientList.StartForegroundScan() @ new Thread(()=>{{}}) while retrieving packets [{ex.Message}]");
            _ = view.MainForm.Invoke(() => view.ToolStripStatusScan.Text = "Refresh for scan");
            _ = view.MainForm.Invoke(() => view.ToolStripProgressBarScan.Value = 0);
        }
        catch (Exception ex)
        {
            DebugOutput.Print(ex.Message);
        }
    }

    /// <summary>
    /// Actively monitor ARP packets for signs of new clients after StartForegroundScan active scan is done
    /// </summary>
    private static Task StartBackgroundScan(IView view, NpcapDevice networkAdapter, IPAddress gatewayIp, CancellationToken token = default)
    {
        view.MainForm.Invoke(() =>
        {
            view.ToolStripStatus.Text = "Scanning In Background";
            view.ToolStripStatusScan.Text = "...";
            view.ToolStripProgressBarScan.Value = 0;
        });
        try
        {
            // Obtain current IP address
            var sourceAddress = networkAdapter.ReadCurrentIpV4Address();

            #region Sending ARP requests to probe for all possible IP addresses on LAN
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            TaskBuffer.Add(cts, () => InitiateArpRequestQueue(view, networkAdapter, gatewayIp, true, cts.Token), prefix);
            #endregion

            #region Assign OnPacketArrival event handler and start capturing
            networkAdapter.OnPacketArrival += (sender, e) => ParseArpResponse(view, networkAdapter.ReadCurrentSubnet(), gatewayIp, e, sourceAddress, token);
            #endregion
            networkAdapter.StartCapture();
        }
        catch (Exception ex)
        {
            DebugOutput.Print($"Exception at GetClientList.BackgroundScanStart() [{ex.Message}]");
        }
        return Task.CompletedTask;
    }

    public static ValueTask StopScan(IView view)
    {
        view.MainForm.Invoke(() =>
        {
            view.ToolStripStatus.Text = "Stopped Scanning";
            view.ToolStripStatusScan.Text = "...";
            view.ToolStripProgressBarScan.Value = 0;
        });
        return TaskBuffer.StopThreadByPrefix(prefix); // kill existing threads
    }

    // TODO: Start spoofing for devices regarding online status.
    private static Task InitiateArpRequestQueue(IView view, NpcapDevice networkAdapter, IPAddress gatewayIp, bool loop, CancellationToken token = default)
    {
        try
        {
            // Obtain subnet information
            var subnet = networkAdapter.ReadCurrentSubnet();

            // Obtain current IP address
            var sourceAddress = networkAdapter.ReadCurrentIpV4Address();

            _ = ArpTable.Instance.Add(sourceAddress, networkAdapter.MacAddress);
            _ = view.ClientListView.Items.Add(new ListViewItem([
                    sourceAddress.ToString(),
                    networkAdapter.MacAddress.ToString("-"),
                    "On",
                    ApplicationSettings.GetSavedClientNameFromMAC(networkAdapter.MacAddress.ToString("-")),
                    DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss:fff"),
                ]));

            // Start
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _ = TaskBuffer.Add(cts, async () =>
            {
                do
                {
                    var i = 0;
                    foreach (var targetIpAddress in subnet.AsEnumerable().Where(x => !x.Equals(sourceAddress) && !x.Equals(gatewayIp)).Prepend(gatewayIp))
                    {
                        if (token.IsCancellationRequested)
                            break;
                        i++;
                        if (i % 255 == 0)
                            await Task.Delay(10);
                        await SendArpRequest(networkAdapter, targetIpAddress, cts.Token);
                    }
                    await Task.Delay(1000);
                }
                while (!token.IsCancellationRequested && loop);
            }, prefix);
        }
        catch (PcapException ex)
        {
            DebugOutput.Print($"PcapException @ GetClientList.InitiateArpRequestQueue() probably due to capturedevice being closed by refreshing or by exiting application [{ex.Message}]");
        }
        catch (OutOfMemoryException ex)
        {
            DebugOutput.Print($"PcapException @ GetClientList.InitiateArpRequestQueue() out of memory. \nTotal number of threads {TaskBuffer.Count}\nTotal number of alive threads {TaskBuffer.AliveCount}\n[{ex.Message}]");
        }
        catch (Exception ex)
        {
            DebugOutput.Print($"Exception at GetClientList.InitiateArpRequestQueue() inside new Thread(()=>{{}}) while sending packets [{ex.Message}]");
        }
        return Task.CompletedTask;
    }

    private static async Task SendArpRequest(NpcapDevice networkAdapter, IPAddress targetIpAddress, CancellationToken token = default)
    {
        await Task.Yield();
        var arprequestpacket = new ArpPacket(ArpOperation.Request, "00-00-00-00-00-00".Parse(), targetIpAddress, networkAdapter.MacAddress, networkAdapter.ReadCurrentIpV4Address());
        var ethernetpacket = new EthernetPacket(networkAdapter.MacAddress, "FF-FF-FF-FF-FF-FF".Parse(), EthernetType.Arp);
        ethernetpacket.PayloadPacket = arprequestpacket;
        token.ThrowIfCancellationRequested();
        networkAdapter.SendPacket(ethernetpacket);
        Debug.WriteLine("ARP request is sent to: {0}", targetIpAddress);
    }

    private static void ParseArpResponse(IView view, IPV4Subnet subnet, IPAddress gatewayIp, CaptureEventArgs e, IPAddress sourceAddress, CancellationToken token = default)
    {
        var packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
        var arppacket = packet.Extract<ArpPacket>();
        if (!token.IsCancellationRequested && !arppacket.SenderProtocolAddress.Equals(sourceAddress) && !arppacket.SenderProtocolAddress.Equals(IPAddress.None) && subnet.Contains(arppacket.SenderProtocolAddress))
        {
            var contains = ArpTable.Instance.ContainsKey(arppacket.SenderProtocolAddress);

            var isGateway = false;
            if (arppacket.SenderProtocolAddress.Equals(gatewayIp))
            {
                if (!contains)
                    DebugOutput.Print("Found gateway!");
                isGateway = true;
            }

            if (!contains)
            {
                DebugOutput.Print($"Added {arppacket.SenderProtocolAddress} @ {arppacket.SenderHardwareAddress.ToString("-")} from background scan!");
                _ = ArpTable.Instance.Add(arppacket.SenderProtocolAddress, arppacket.SenderHardwareAddress);
            }

            token.ThrowIfCancellationRequested();

            view.ClientListView.Invoke(() =>
            {
                string[] data = [
                    arppacket.SenderProtocolAddress.ToString(),
                    arppacket.SenderHardwareAddress.ToString("-"),
                    "On",
                    isGateway ? "GATEWAY" : ApplicationSettings.GetSavedClientNameFromMAC(arppacket.SenderHardwareAddress.ToString("-")),
                    DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss:fff"),
                ];

                if (!contains)
                    view.ClientListView.Items.Add(new ListViewItem(data));
                else
                {
                    for (int i = 0; i < view.ClientListView.Items.Count; i++)
                    {
                        var item = view.ClientListView.Items[i];
                        if (item.SubItems[1].Text == data[1] && item.SubItems[0].Text == data[0])
                        {
                            item.SubItems[4].Text = data[4];
                            break;
                        }
                    }
                }
            });

            token.ThrowIfCancellationRequested();

            _ = view.MainForm.Invoke(() => view.ToolStripStatusScan.Text = ArpTable.Instance.Count + " device(s) found");
        }
    }
}
