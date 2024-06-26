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
        public void Parse01()
        {
            var success = IosInterface.TryParse("Fa0/0/1.123", out var result);
            Assert.That(success, Is.True);

            Assert.That(result.ToString(), Is.EqualTo("FastEthernet0/0/1.123"));
        }

        [Test]
        public void Parse02()
        {
            var success = IosInterface.TryParse("mgmt0", out var result);
            Assert.That(success, Is.True);

            Assert.That(result.ToString(), Is.EqualTo("mgmt0"));
        }

        [Test]
        public void Parse03()
        {
            var success = IosInterface.TryParse("Serial0/1:22", out var result);
            Assert.That(success, Is.True);

            Assert.That(result.ToString(), Is.EqualTo("Serial0/1:22"));
            Assert.That(result.Type, Is.EqualTo(InterfaceType.Serial));
            Assert.That(result.Slot, Is.EqualTo(0));
            Assert.That(result.Port, Is.EqualTo(1));
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
    }
}