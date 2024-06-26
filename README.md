# Bitvantage.Cisco.IosInterface
Parse Cisco IOS interface names, such as GigabitEthernet0/0, Gi1/0/1, HundredGigE10/0/1.37, Vlan33, etc.

## Installing via NuGet Package Manager
```
PM> NuGet\Install-Package Bitvantage.Cisco.IosInterface
```
## Quick Start

```csharp
var iosInterface1 = IosInterface.Parse("GigabitEthernet0/0");
var iosInterface2 = IosInterface.Parse("Gi0/0");

var iosInterface3 = IosInterface.Parse("Fa0/1");
var iosInterface4 = IosInterface.Parse("Fa0/0");

var equal = iosInterface1 == iosInterface2; // true
var ordered = new[] { iosInterface1, iosInterface2, iosInterface3, iosInterface4 }.Order().Distinct(); // Fa0/0, Fa0/1, Gi0/0
```