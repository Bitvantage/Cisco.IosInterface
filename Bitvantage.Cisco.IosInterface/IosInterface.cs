/*
   Bitvantage.Cisco.IosInterface
   Copyright (C) 2024 Michael Crino
   
   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU Affero General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.
   
   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU Affero General Public License for more details.
   
   You should have received a copy of the GNU Affero General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace Bitvantage.Cisco;

public enum InterfaceType
{
    //BDI, // Bridge Domain Interface
    //Cellular,
    //CEM,
    //EmbeddedServiceEngine //Embedded-Service-Engine
    //FiveGigabitEthernet,
    //Multilink,
    //nve,
    //overlay,
    //pseudowire,
    //SM,
    AppGigabitEthernet,
    Bluetooth,
    Ethernet,
    FastEthernet,
    FortyGigabitEthernet,
    GigabitEthernet,
    HundredGigabitEthernet,
    Loopback,
    Management,
    PortChannel, // Port-channel
    Serial,
    TenGigabitEthernet,
    Tunnel,
    TwentyFiveGigabitEthernet,
    TwoGigabitEthernet,
    Vlan
}

public record IosInterface : IComparable<IosInterface>
{
    public enum FormatType
    {
        Default,
        Long,
        Short,
    }

    private static readonly ReadOnlyDictionary<InterfaceType, string> InterfaceNameLookup;
    private static readonly ReadOnlyDictionary<InterfaceType, bool> InterfacePhysicalLookup;
    private static readonly Regex InterfaceRegex;
    private static readonly ReadOnlyDictionary<string, InterfaceType> InterfaceTypeLookup;
    private static readonly ReadOnlyDictionary<InterfaceType, string> InterfaceAbbreviations;
    public int? Channel { get; init; }
    public int? Chassis { get; init; }
    public bool IsPhysical => InterfacePhysicalLookup[Type];
    public int? Module { get; init; }
    public string Name => InterfaceNameLookup[Type];
    public int? Port { get; init; }
    public int? Slot { get; init; }
    public int? SubInterface { get; init; }

    public InterfaceType Type { get; init; }

    static IosInterface()
    {
        var interfaceDefinitions = new List<InterfaceDefinition>
        {
            new(InterfaceType.AppGigabitEthernet, "AppGigabitEthernet", new List<string> { "Ap" }, true),
            new(InterfaceType.Bluetooth, "Bluetooth", new List<string> { "Bl" }, true),
            new(InterfaceType.Ethernet, "Ethernet", new List<string> { "Et" }, true),
            new(InterfaceType.FastEthernet, "FastEthernet", new List<string> { "Fa" }, true),
            new(InterfaceType.FortyGigabitEthernet, "FortyGigabitEthernet", new List<string> { "Fo" }, true),
            new(InterfaceType.GigabitEthernet, "GigabitEthernet", new List<string> { "Gi" }, true),
            new(InterfaceType.HundredGigabitEthernet, "HundredGigE", new List<string> { "Hu" }, true),
            new(InterfaceType.Loopback, "Loopback", new List<string> { "Lo" }, false),
            new(InterfaceType.Management, "mgmt", new List<string>() {"mgmt"}, true),
            new(InterfaceType.PortChannel, "Port-channel", new List<string> { "Po" }, false),
            new(InterfaceType.Serial, "Serial", new List<string> { "Se" }, true),
            new(InterfaceType.TenGigabitEthernet, "TenGigabitEthernet", new List<string> { "Te" }, true),
            new(InterfaceType.Tunnel, "Tunnel", new List<string> { "Tu" }, false),
            new(InterfaceType.TwentyFiveGigabitEthernet, "TwentyFiveGigE", new List<string> { "Twe" }, true),
            new(InterfaceType.TwoGigabitEthernet, "TwoGigabitEthernet", new List<string> { "Tw" }, true),
            new(InterfaceType.Vlan, "Vlan", new List<string> { "Vl" }, false)
        };

        var interfaceTypeLookup = new Dictionary<string, InterfaceType>();

        var interfaceNameLookup = new Dictionary<InterfaceType, string>();
        var interfacePhysicalLookup = new Dictionary<InterfaceType, bool>();

        foreach (var interfaceDefinition in interfaceDefinitions)
        {
            interfaceNameLookup.Add(interfaceDefinition.Type, interfaceDefinition.Name);
            interfacePhysicalLookup.Add(interfaceDefinition.Type, interfaceDefinition.IsPhysical);

            interfaceTypeLookup.Add(interfaceDefinition.Name, interfaceDefinition.Type);

            foreach (var name in interfaceDefinition.Abbreviations)
                interfaceTypeLookup.TryAdd(name, interfaceDefinition.Type);
        }

        InterfaceAbbreviations = interfaceDefinitions
            .Select(item => new { Type = item.Type, Abbreviation = item.Abbreviations.First() })
            .ToDictionary(item => item.Type, item => item.Abbreviation)
            .AsReadOnly();

        InterfaceTypeLookup = interfaceTypeLookup.AsReadOnly();
        InterfaceNameLookup = interfaceNameLookup.AsReadOnly();
        InterfacePhysicalLookup = interfacePhysicalLookup.AsReadOnly();

        var interfaceNamePatterns = InterfaceTypeLookup
            .Keys
            .Select(Regex.Escape);

        var interfaceNamePattern = string.Join("|", interfaceNamePatterns);

        // interface formats:
        // xx1
        // xx1/0
        // xx1/0/1
        // xx1/0/1.nnn
        // xx1/0.nnn
        // xx1.nnn
        // xx1/10:22

        InterfaceRegex = new Regex($"""
            ^
                (interface\ )?
            	(?<type>({interfaceNamePattern}))[ ]?
            	(
            		(
            			(
            				(?<chassis>\d+)/)?
            				(?<module>\d+)/)?
            				(?<slot>\d+)/)?
            				(?<port>\d+)
            				(\.(?<subInterface>\d+))?
            				(:(?<channel>\d+))?
            $
            """,
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
    }

    public IosInterface(InterfaceType type, int port)
    {
        Type = type;
        Port = port;
    }

    public IosInterface(InterfaceType type, int? module, int? slot, int? port)
    {
        Type = type;
        Module = module;
        Slot = slot;
        Port = port;
    }

    public IosInterface(InterfaceType type, int? module, int? slot, int? port, int? subInterface)
    {
        Type = type;
        Module = module;
        Slot = slot;
        Port = port;
        SubInterface = subInterface;
    }

    public IosInterface(InterfaceType type, int? chassis, int? module, int? slot, int? port, int? subInterface = null, int? channel = null)
    {
        Type = type;
        Chassis = chassis;
        Module = module;
        Slot = slot;
        Port = port;
        SubInterface = subInterface;
        Channel = channel;
    }

    public int CompareTo(IosInterface? other)
    {
        if (ReferenceEquals(this, other))
            return 0;

        if (ReferenceEquals(null, other))
            return 1;

        var typeComparison = Type.CompareTo(other.Type);
        if (typeComparison != 0)
            return typeComparison;

        var chassisComparison = Nullable.Compare(Chassis, other.Chassis);
        if (chassisComparison != 0)
            return chassisComparison;

        var moduleComparison = Nullable.Compare(Module, other.Module);
        if (moduleComparison != 0)
            return moduleComparison;

        var portComparison = Nullable.Compare(Port, other.Port);
        if (portComparison != 0)
            return portComparison;

        var slotComparison = Nullable.Compare(Slot, other.Slot);
        if (slotComparison != 0)
            return slotComparison;

        var subInterfaceComparison = Nullable.Compare(SubInterface, other.SubInterface);
        if (subInterfaceComparison != 0)
            return subInterfaceComparison;

        var channelComparison = Nullable.Compare(Channel, other.Channel);
        if (channelComparison != 0)
            return channelComparison;

        return Type.CompareTo(other.Type);
    }

    public static IosInterface Parse(string @interface)
    {
        if (TryParse(@interface, out var result))
            return result;

        throw new ArgumentException($"Could not parse interface: {@interface}");
    }

    public override string ToString()
    {
        return ToString(FormatType.Long);
    }

    public string ToString(FormatType format)
    {
        var sb = new StringBuilder();

        if (Chassis != null)
        {
            if (sb.Length > 0)
                sb.Append("/");

            sb.Append(Chassis);
        }

        if (Module != null)
        {
            if (sb.Length > 0)
                sb.Append("/");

            sb.Append(Module);
        }

        if (Slot != null)
        {
            if (sb.Length > 0)
                sb.Append("/");

            sb.Append(Slot);
        }

        if (Port != null)
        {
            if (sb.Length > 0)
                sb.Append("/");

            sb.Append(Port);
        }

        if (SubInterface != null)
        {
            sb.Append(".");
            sb.Append(SubInterface);
        }

        if (Channel != null)
        {
            sb.Append(":");
            sb.Append(Channel);
        }

        switch (format)
        {
            case FormatType.Default:
            case FormatType.Long:
                sb.Insert(0, InterfaceNameLookup[Type]);
                break;

            case FormatType.Short:
                sb.Insert(0, InterfaceAbbreviations[Type]);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }

        return sb.ToString();
    }

    public static bool TryParse(string @interface, [NotNullWhen(true)] out IosInterface result)
    {
        var match = InterfaceRegex.Match(@interface);
        if (!match.Success)
        {
            result = null;
            return false;
        }

        int? chassis = null;
        int? module = null;
        int? slot = null;
        int? port = null;
        int? subInterface = null;
        int? channel = null;

        if (match.Groups["chassis"].Success)
            chassis = int.Parse(match.Groups["chassis"].Value);

        if (match.Groups["module"].Success)
            module = int.Parse(match.Groups["module"].Value);

        if (match.Groups["slot"].Success)
            slot = int.Parse(match.Groups["slot"].Value);

        if (match.Groups["port"].Success)
            port = int.Parse(match.Groups["port"].Value);

        if (match.Groups["subInterface"].Success)
            subInterface = int.Parse(match.Groups["subInterface"].Value);

        if (match.Groups["channel"].Success)
            channel = int.Parse(match.Groups["channel"].Value);

        var name = match.Groups["type"].Value;

        var type = InterfaceTypeLookup[name];

        result = new IosInterface(type, chassis, module, slot, port, subInterface, channel);
        return true;
    }
    
    private record InterfaceDefinition(InterfaceType Type, string Name, List<string> Abbreviations, bool IsPhysical);
}