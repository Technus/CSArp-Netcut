using System;
using System.Collections.Generic;
using System.Linq;
using SharpPcap;
using SharpPcap.Npcap;

namespace CSArp.Model;

public static class NetworkAdapterManager
{
    private static CaptureDeviceList _networkAdapters;
    public static CaptureDeviceList NetworkAdapters => _networkAdapters ??= CaptureDeviceList.Instance;

    public static IReadOnlyList<NpcapDevice> WinPcapDevices => 
        NetworkAdapters
            .OfType<NpcapDevice>()
            .ToList()
            .AsReadOnly();
}
