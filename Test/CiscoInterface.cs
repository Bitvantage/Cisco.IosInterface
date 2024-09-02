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

using Bitvantage.Cisco;

namespace Test
{
    internal class CiscoInterface
    {
        [Test]
        public void Constructor01()
        {
            var @interface = new IosInterface(InterfaceType.Vlan, 10);

            Assert.That(@interface.ToString(), Is.EqualTo("Vlan10"));
        }

        [Test]
        public void Constructor02()
        {
            var @interface = new IosInterface(InterfaceType.GigabitEthernet, 1,0,1);

            Assert.That(@interface.ToString(), Is.EqualTo("GigabitEthernet1/0/1"));
        }

        [Test]
        public void Constructor03()
        {
            var @interface = new IosInterface(InterfaceType.GigabitEthernet, 1, 0, 1, 2000);

            Assert.That(@interface.ToString(), Is.EqualTo("GigabitEthernet1/0/1.2000"));
        }

        [Test]
        public void Constructor04()
        {
            var @interface = new IosInterface(InterfaceType.Serial, 1, 0, 1, null, 20);

            Assert.That(@interface.ToString(), Is.EqualTo("Serial1/0/1.20"));
        }

        [Test]
        public void Parse01()
        {
            var success = IosInterface.TryParse("Fa0/0/1.123", out var result);

            Assert.That(success, Is.True);

            Assert.IsNull(result.Channel);
            Assert.IsNull(result.Chassis);
            Assert.IsTrue(result.IsPhysical);
            Assert.That(result.Module, Is.EqualTo(0));
            Assert.That(result.Name, Is.EqualTo("FastEthernet"));
            Assert.That(result.Port, Is.EqualTo(1));
            Assert.That(result.Slot, Is.EqualTo(0));
            Assert.That(result.SubInterface, Is.EqualTo(123));
            Assert.That(result.Type, Is.EqualTo(InterfaceType.FastEthernet));

            Assert.That(result.ToString(), Is.EqualTo("FastEthernet0/0/1.123"));

        }

        [Test]
        public void Parse02()
        {
            var success = IosInterface.TryParse("mgmt0", out var result);

            Assert.That(success, Is.True);

            Assert.IsNull(result.Channel);
            Assert.IsNull(result.Chassis);
            Assert.IsTrue(result.IsPhysical);
            Assert.IsNull(result.Module);
            Assert.That(result.Name, Is.EqualTo("mgmt"));
            Assert.That(result.Port, Is.EqualTo(0));
            Assert.IsNull(result.Slot);
            Assert.IsNull(result.SubInterface);
            Assert.That(result.Type, Is.EqualTo(InterfaceType.Management));

            Assert.That(result.ToString(), Is.EqualTo("mgmt0"));
        }

        [Test]
        public void Parse03()
        {
            var success = IosInterface.TryParse("Serial0/1:22", out var result);
            Assert.That(success, Is.True);

            Assert.That(result.Channel, Is.EqualTo(22));
            Assert.IsNull(result.Chassis);
            Assert.IsTrue(result.IsPhysical);
            Assert.IsNull(result.Module);
            Assert.That(result.Name, Is.EqualTo("Serial"));
            Assert.That(result.Port, Is.EqualTo(1));
            Assert.That(result.Slot, Is.EqualTo(0));
            Assert.IsNull(result.SubInterface);
            Assert.That(result.Type, Is.EqualTo(InterfaceType.Serial));

            Assert.That(result.ToString(), Is.EqualTo("Serial0/1:22"));
        }

        [Test]
        public void Parse04()
        {
            var success = IosInterface.TryParse("interface TenGigabitEthernet1/2", out var result);
            Assert.That(success, Is.True);

            Assert.IsNull(result.Channel);
            Assert.IsNull(result.Chassis);
            Assert.IsTrue(result.IsPhysical);
            Assert.IsNull(result.Module);
            Assert.That(result.Name, Is.EqualTo("TenGigabitEthernet"));
            Assert.That(result.Port, Is.EqualTo(2));
            Assert.That(result.Slot, Is.EqualTo(1));
            Assert.IsNull(result.SubInterface);
            Assert.That(result.Type, Is.EqualTo(InterfaceType.TenGigabitEthernet));

            Assert.That(result.ToString(), Is.EqualTo("TenGigabitEthernet1/2"));
        }

        [Test]
        public void CompareTo01()
        {
            var keyValuePairs = new List<KeyValuePair<IosInterface, int>>
            {
                new(new IosInterface(InterfaceType.GigabitEthernet, 1, 0, 2, null), 4),
                new(new IosInterface(InterfaceType.Vlan, 200), 9),
                new(new IosInterface(InterfaceType.FastEthernet, 1, 0, 2, null), 1),
                new(new IosInterface(InterfaceType.GigabitEthernet, 1, 0, 1, null), 2),
                new(new IosInterface(InterfaceType.GigabitEthernet, 1, 1, 1, null), 3),
                new(new IosInterface(InterfaceType.GigabitEthernet, 3, 0, 1, null), 6),
                new(new IosInterface(InterfaceType.Vlan, 100), 8),
                new(new IosInterface(InterfaceType.FastEthernet, 1, 0, 1, null), 0),
                new(new IosInterface(InterfaceType.Vlan, 99), 7),
                new(new IosInterface(InterfaceType.GigabitEthernet, 1, 0, 12, null), 5),
            };

            var orderedByKey = keyValuePairs
                .OrderBy(item => item.Key)
                .ToList();

            var orderedByValue = keyValuePairs
                .OrderBy(item => item.Value)
                .ToList();

            Assert.That(orderedByKey, Is.EqualTo(orderedByValue));
        }

        [Test]
        public void ToString01()
        {
            var @interface = new IosInterface(InterfaceType.GigabitEthernet, 0, 0, 0,null);

            Assert.That(@interface.ToString(), Is.EqualTo("GigabitEthernet0/0/0"));
        }

        [Test]
        public void ToString02()
        {
            var @interface = new IosInterface(InterfaceType.GigabitEthernet, 0, 0, 0, null);

            Assert.That(@interface.ToString(IosInterface.FormatType.Short), Is.EqualTo("Gi0/0/0"));
        }
    }
}