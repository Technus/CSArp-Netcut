using System;
using System.Collections.Generic;
using System.Linq;
using SharpPcap;
using SharpPcap.LibPcap;

namespace CSArp.Service.Model;

public static class NetworkAdapterManager
{
    private static CaptureDeviceList _networkAdapters;
    public static CaptureDeviceList NetworkAdapters => _networkAdapters ??= CaptureDeviceList.Instance;

    public static IReadOnlyList<LibPcapLiveDevice> WinPcapDevices =>
        NetworkAdapters
            .OfType<LibPcapLiveDevice>()
            .ToList()
            .AsReadOnly();
}
