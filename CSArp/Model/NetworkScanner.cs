using System;
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
using System.Linq;
using System.Net.NetworkInformation;
using SharpPcap.LibPcap;

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
    private const string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

    /// <summary>
    /// Populates listview with machines connected to the LAN
    /// </summary>
    /// <param name="view"></param>
    /// <param name="networkAdapter"></param>
    public void StartScan(IView view, LibPcapLiveDevice networkAdapter, IPAddress gatewayIp)
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

    private static void StartForegroundScan(IView view, LibPcapLiveDevice networkAdapter, IPAddress gatewayIp, int foregroundScanTimeout)
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

    private static Task AwaitArpRequestQueue(IView view, LibPcapLiveDevice networkAdapter, IPAddress gatewayIp, int foregroundScanTimeout, IPV4Subnet subnet, CancellationToken token = default)
    {
        view.MainForm.Invoke(() =>
        {
            view.ToolStripStatus.Text = "Awaiting ARP";
            view.ToolStripStatusScan.Text = "Please wait...";
            view.ToolStripProgressBarScan.Value = 0;
        });
        try
        {
            var sourceAddress = networkAdapter.ReadCurrentIpV4Address();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!token.IsCancellationRequested &&
                networkAdapter.GetNextPacket(out var e) is not GetPacketStatus.Error &&
                stopwatch.ElapsedMilliseconds <= foregroundScanTimeout)
            {
                ParseArpResponse(view, subnet, gatewayIp, ref e, sourceAddress, token);
            }
            stopwatch.Stop();
            view.MainForm.Invoke(() =>
            {
                view.ToolStripStatus.Text = "Done Awaiting";
                view.ToolStripStatusScan.Text = $"{ArpTable.Instance.Count} device(s) found";
                view.ToolStripProgressBarScan.Value = 100;
            });
            return StartBackgroundScan(view, networkAdapter, gatewayIp, token); //start passive monitoring
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
        return Task.CompletedTask;
    }

    /// <summary>
    /// Actively monitor ARP packets for signs of new clients after StartForegroundScan active scan is done
    /// </summary>
    private static Task StartBackgroundScan(IView view, LibPcapLiveDevice networkAdapter, IPAddress gatewayIp, CancellationToken token = default)
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
            networkAdapter.OnPacketArrival += (sender, e) => ParseArpResponse(view, networkAdapter.ReadCurrentSubnet(), gatewayIp, ref e, sourceAddress, token);
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
    private static Task InitiateArpRequestQueue(IView view, LibPcapLiveDevice networkAdapter, IPAddress gatewayIp, bool loop, CancellationToken token = default)
    {
        try
        {
            // Obtain subnet information
            var subnet = networkAdapter.ReadCurrentSubnet();

            // Obtain current IP address
            var sourceAddress = networkAdapter.ReadCurrentIpV4Address();
            
            var oldMAC = ArpTable.Instance.Add(sourceAddress, networkAdapter.MacAddress);

            var contains = networkAdapter.MacAddress.Equals(oldMAC);

            if(!contains)
                _ = view.ClientListView.Items.Add(new ListViewItem([
                        sourceAddress.ToString(),
                        networkAdapter.MacAddress.ToString("-"),
                        "On",
                        "Off",
                        ApplicationSettings.GetSavedClientNameFromMAC(networkAdapter.MacAddress.ToString("-")),
                        DateTime.MaxValue.ToString(_dateTimeFormat),
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

                    var now = DateTime.Now; 
                    view.MainForm.Invoke(() =>
                    {
                        for (i = 0; i < view.ClientListView.Items.Count; i++)
                        {
                            var item = view.ClientListView.Items[i];
                            var t = now.Subtract(DateTime.ParseExact(item.SubItems[5].Text, _dateTimeFormat, default));
                            if (t > TimeSpan.FromSeconds(4.5))
                                item.SubItems[2].Text = "Off";
                        }
                    });
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

    private static async Task SendArpRequest(LibPcapLiveDevice networkAdapter, IPAddress targetIpAddress, CancellationToken token = default)
    {
        await Task.Yield();
        var arprequestpacket = new ArpPacket(ArpOperation.Request, "00-00-00-00-00-00".Parse(), targetIpAddress, networkAdapter.MacAddress, networkAdapter.ReadCurrentIpV4Address());
        var ethernetpacket = new EthernetPacket(networkAdapter.MacAddress, "FF-FF-FF-FF-FF-FF".Parse(), EthernetType.Arp);
        ethernetpacket.PayloadPacket = arprequestpacket;
        if (token.IsCancellationRequested)
            return;
        networkAdapter.SendPacket(ethernetpacket);
        Debug.WriteLine("ARP request is sent to: {0}", targetIpAddress);
    }

    private static void ParseArpResponse(IView view, IPV4Subnet subnet, IPAddress gatewayIp,
        ref readonly PacketCapture e, IPAddress sourceAddress, CancellationToken token = default)
    {
        var ep = e.GetPacket();
        if(ep.Data.Length > 0)
            ParseArpResponse(view, subnet, gatewayIp, Packet.ParsePacket(ep.LinkLayerType, ep.Data), sourceAddress, e.Device.MacAddress, token);
    }

    private static void ParseArpResponse(IView view, IPV4Subnet subnet, IPAddress gatewayIp,
        Packet packet, IPAddress sourceAddress, PhysicalAddress sourceMac, CancellationToken token = default)
    {
        var arppacket = packet.Extract<ArpPacket>();
        if (!token.IsCancellationRequested && 
            !arppacket.SenderProtocolAddress.Equals(sourceAddress) && 
            !arppacket.SenderHardwareAddress.Equals(sourceMac) && 
            !arppacket.SenderProtocolAddress.Equals(IPAddress.None) && 
            subnet.Contains(arppacket.SenderProtocolAddress))
        {
            var oldMAC = ArpTable.Instance.Add(arppacket.SenderProtocolAddress, arppacket.SenderHardwareAddress);

            var contains = arppacket.SenderHardwareAddress.Equals(oldMAC);

            if (!contains)
            {
                DebugOutput.Print($"Added {arppacket.SenderProtocolAddress} @ {arppacket.SenderHardwareAddress.ToString("-")} from background scan!");
            }

            var isGateway = false;
            if (arppacket.SenderProtocolAddress.Equals(gatewayIp))
            {
                if (!contains)
                    DebugOutput.Print("Found gateway!");
                isGateway = true;
            }

            if (token.IsCancellationRequested)
                return;

            view.MainForm.Invoke(() =>
            {
                view.ToolStripStatusScan.Text = ArpTable.Instance.Count + " device(s) found";

                string[] data = [
                    arppacket.SenderProtocolAddress.ToString(),
                    arppacket.SenderHardwareAddress.ToString("-"),
                    "On",
                    "Off",
                    isGateway ? "GATEWAY" : ApplicationSettings.GetSavedClientNameFromMAC(arppacket.SenderHardwareAddress.ToString("-")),
                    DateTime.Now.ToString(_dateTimeFormat),
                ];

                if (!contains)
                    _ = view.ClientListView.Items.Add(new ListViewItem(data));
                else
                {
                    for (var i = 0; i < view.ClientListView.Items.Count; i++)
                    {
                        var item = view.ClientListView.Items[i];
                        if (item.SubItems[1].Text == data[1] && item.SubItems[0].Text == data[0])
                        {
                            item.SubItems[2].Text = "On";
                            item.SubItems[5].Text = data[5];
                            break;
                        }
                    }
                }
            });
        }
    }
}
