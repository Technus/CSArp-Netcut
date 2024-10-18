using Ardalis.SmartEnum;

namespace CSArp.Service;

public class Columns : SmartEnum<Columns>
{
    public static Columns None { get; } = new(nameof(None), -1);
    public static Columns Address { get; } = new(nameof(Address), 0);
    public static Columns PhysicalAddess { get; } = new(nameof(PhysicalAddess), 1);
    public static Columns Status { get; } = new(nameof(Status), 2);
    public static Columns Spoof { get; } = new(nameof(Spoof), 3);
    public static Columns HostName { get; } = new(nameof(HostName), 4);
    public static Columns Time { get; } = new(nameof(Time), 5);

    private Columns(string name, int value) : base(name, value)
    {
    }
}