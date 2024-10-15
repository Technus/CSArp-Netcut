using System;
using System.Collections.Generic;
using System.Linq;
using SharpPcap;
using SharpPcap.WinPcap;
using SharpPcap.AirPcap;

namespace CSArp.Model;

public static class NetworkAdapterManager
{
    private static CaptureDeviceList _networkAdapters;
    public static CaptureDeviceList NetworkAdapters => _networkAdapters ??= CaptureDeviceList.Instance;

    public static IReadOnlyList<WinPcapDevice> WinPcapDevices => 
        NetworkAdapters
            .OfType<WinPcapDevice>()
            .ToList()
            .AsReadOnly();

    [Obsolete("Since AirPcap is obsolete, it will not be used at first.")]
    public static IReadOnlyList<AirPcapDevice> AirPcapDevices =>
        NetworkAdapters
            .OfType<AirPcapDevice>()
            .ToList()
            .AsReadOnly();
}
