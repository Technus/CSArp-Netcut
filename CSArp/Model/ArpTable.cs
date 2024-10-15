﻿using System;
using System.Net.NetworkInformation;
using System.Net;
using System.Collections.Concurrent;

namespace CSArp.Model;

public sealed class ArpTable
{
    public static ArpTable Instance => lazy.Value;

    private static readonly Lazy<ArpTable> lazy = new(() => new ArpTable());

    public int Count => _dictionary.Count;

    private readonly ConcurrentDictionary<IPAddress, PhysicalAddress> _dictionary = new();

    private ArpTable()
    {
    }

    public bool Add(IPAddress ipAddress, PhysicalAddress physicalAddress) => 
        _dictionary.TryAdd(ipAddress, physicalAddress);

    public bool ContainsKey(IPAddress ipAddress) => 
        _dictionary.ContainsKey(ipAddress);

    public void Clear() => 
        _dictionary.Clear();
}
