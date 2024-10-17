﻿/*
 * CSArp 1.4
 * An arpspoofing program
 * Author : globalpolicy
 * Contact : yciloplabolg@gmail.com
 * Blog : c0dew0rth.blogspot.com
 * Github : globalpolicy
 * Time : May 9, 2017 @ 09:07PM
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SharpPcap;
using System.Net;
using System.Net.NetworkInformation;
using CSArp.Model;
using CSArp.View;
using CSArp.Model.Utilities;
using CSArp.Model.Extensions;
using System.Diagnostics;
using System.Threading.Tasks;
using SharpPcap.LibPcap;

namespace CSArp.Presenter;

public class Presenter
{
    #region Public properties
    public Spoofer ArpSpoofer { get; set; }

    public NetworkScanner NetworkScanner { get; set; }
    public string SelectedInterfaceFriendlyName
    {
        get => selectedInterfaceFriendlyName;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(value);

            selectedInterfaceFriendlyName = value;
            if (selectedDevice != null && selectedDevice.Opened)
            {
                try
                {
                    selectedDevice.StopCapture(); //stop previous capture
                    selectedDevice.Close(); //close previous instances
                }
                catch (PcapException ex)
                {
                    DebugOutput.Print($"Exception at StartForegroundScan while trying to capturedevice.StopCapture() or capturedevice.Close() [{ex.Message}]");
                }
            }
            selectedDevice = NetworkAdapterManager.WinPcapDevices.Where(dev => dev.Interface.FriendlyName != null)
                                                                 .FirstOrDefault(dev => dev.Interface.FriendlyName.Equals(selectedInterfaceFriendlyName));
        }
    }
    #endregion

    #region Private fields
    private readonly IView _view;
    private IPAddress gatewayIpAddress = null;
    private PhysicalAddress gatewayPhysicalAddress;
    private LibPcapLiveDevice selectedDevice = null;
    private string selectedInterfaceFriendlyName;
    #endregion

    #region constructor
    public Presenter(IView view)
    {
        _view = view;
        ArpSpoofer = new();
        NetworkScanner = new();
    }
    #endregion

    /// <summary>
    /// Populate the LAN clients
    /// </summary>
    /// // TODO: Use the device interface, not the name
    public async ValueTask StartNetworkScan()
    {
        if (string.IsNullOrEmpty(SelectedInterfaceFriendlyName)) //if a network interface has been selected
        {
            _ = MessageBox.Show("Please select a network interface!", "Interface", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            return;
        }

        if (_view.ToolStripStatusScan.Text.IndexOf("Scanning") != -1) //if a scan isn't active already
            return;

        await ArpSpoofer.StopAll(_view); // first disengage spoofing tasks
        NetworkScanner.StartScan(_view, selectedDevice, gatewayIpAddress);
    }

    public ValueTask StopNetworkScan() => NetworkScanner.StopScan(_view);

    /// <summary>
    /// Disconnects clients selected in the listview
    /// </summary>
    public void DisconnectSelectedClients()
    {
        // Guard clause
        if (_view.ClientListView.SelectedItems.Count == 0)
            return;

        foreach (ListViewItem item in _view.ClientListView.Items)
        {
            if (item.SubItems[0].Text == gatewayIpAddress.ToString())
                gatewayPhysicalAddress = item.SubItems[1].Text.Parse();
        }

        if (gatewayPhysicalAddress == null)
        {
            _ = MessageBox.Show("Gateway Physical Address still undiscovered. Please wait and try again.", "Warning", MessageBoxButtons.OK);
            return;
        }

        _view.MainForm.Invoke(() =>
        {
            _view.ToolStripStatus.Text = "Arpspoofing active...";
        });

        var targetlist = new Dictionary<IPAddress, PhysicalAddress>();
        var parseindex = 0;
        foreach (ListViewItem listitem in _view.ClientListView.SelectedItems)
        {
            targetlist.Add(IPAddress.Parse(listitem.SubItems[0].Text), listitem.SubItems[1].Text.Parse());
            _ = _view.MainForm.BeginInvoke(new Action(() =>
              {
                  _view.ClientListView.SelectedItems[parseindex++].SubItems[2].Text = "Off";
              }));
        }
        ArpSpoofer.Start(_view, targetlist, gatewayIpAddress, gatewayPhysicalAddress, selectedDevice);
    }

    /// <summary>
    /// Reconnects clients by stopping fake ARP requests
    /// </summary>
    public async ValueTask ReconnectClients() //selective reconnection not availabe at this time and frankly, not that useful
    {
        await ArpSpoofer.StopAll(_view);
        foreach (ListViewItem entry in _view.ClientListView.Items)
        {
            entry.SubItems[2].Text = "On";
        }
        _view.ToolStripStatus.Text = "Stopped";
    }

    public void GetGatewayInformation()
    {
        var gatewayInfo = NetworkInterface
            .GetAllNetworkInterfaces()
            .First(i => i.Name == SelectedInterfaceFriendlyName)
            .GetIPProperties().GatewayAddresses
            .First(g => g.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        gatewayIpAddress = gatewayInfo.Address;
    }

    public void StartCapture() => selectedDevice.Open(DeviceModes.Promiscuous, 1000); //open device with 1000ms timeout

    public void StopCapture()
    {
        if (selectedDevice?.Opened ?? false)
        {
            try
            {
                selectedDevice.StopCapture();
                selectedDevice.Close();
            }
            catch (Exception ex)
            {
                Debug.Print($"Exception in Presenter.StopCapture\n{ex.Message}");
            }
        }
    }
}
