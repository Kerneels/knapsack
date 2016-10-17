using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Knapsack
{
	[TestClass]
	public class TestKnapsack
	{
		[TestClass]
		public class TestBelonging : TestKnapsack
		{
			[TestMethod]
			public void ShouldAddRemoveAndConfirmItIsInGear()
			{
				uint gear = 0; // empty set of gear
				var testBelonging = new Belonging { Number = 0, Name = "map", GramsWeight = 90, Value = 150 };

				Assert.IsFalse(testBelonging.IsInGear(gear));
				uint gearAfterAdd = testBelonging.AddToGear(gear);
				Assert.IsFalse(testBelonging.IsInGear(gear));
				Assert.IsTrue(testBelonging.IsInGear(gearAfterAdd));
				Assert.AreNotEqual(gearAfterAdd, gear);

				uint gearAfterRemove= testBelonging.RemoveFromGear(gearAfterAdd);
				Assert.AreEqual(gearAfterRemove, gear);
				Assert.IsFalse(testBelonging.IsInGear(gearAfterRemove));
			}
		}

		[TestClass]
		public class TestInventory : TestKnapsack
		{
			[TestMethod]
			public void ShouldSumProperly()
			{
				var allKit = new List<Belonging>();
				allKit.Add(new Belonging { Number = 0, Name = "map", GramsWeight = 90, Value = 150 });
				allKit.Add(new Belonging { Number = 1, Name = "compass", GramsWeight = 130, Value = 35 });
				allKit.Add(new Belonging { Number = 2, Name = "water", GramsWeight = 1530, Value = 300 });

				Inventory.AllGear = allKit;
				var testInventory = new Inventory { Gear = 0 };
				testInventory.Gear = allKit[0].AddToGear(
					testInventory.Gear);
				testInventory.Gear = allKit[2].AddToGear(
					testInventory.Gear);

				Assert.AreEqual(allKit[0].GramsWeight + allKit[2].GramsWeight,
					testInventory.TotalGramsWeight);
				Assert.AreEqual(allKit[0].Value + allKit[2].Value,
					testInventory.TotalValue);
			}

			[TestMethod]
			public void TestFirstBestInventory()
			{
				var allKit = new List<Belonging>();
				allKit.Add(new Belonging { Number = 0, Name = "map", GramsWeight = 90, Value = 150 });
				allKit.Add(new Belonging { Number = 1, Name = "compass", GramsWeight = 130, Value = 35 });
				allKit.Add(new Belonging { Number = 2, Name = "water", GramsWeight = 1530, Value = 300 });
				Inventory.AllGear = allKit;

				Assert.IsTrue(
					allKit[1].IsInGear(
					Inventory.FirstBestInventory(220).Gear));

				Assert.IsFalse(
					allKit[2].IsInGear(
					Inventory.FirstBestInventory(220).Gear));
			}

			[TestMethod]
			public void TestValidInventories()
			{
				var allKit = new List<Belonging>();
				allKit.Add(new Belonging { Number = 0, Name = "map", GramsWeight = 90, Value = 150 });
				allKit.Add(new Belonging { Number = 1, Name = "compass", GramsWeight = 130, Value = 35 });
				allKit.Add(new Belonging { Number = 2, Name = "water", GramsWeight = 1530, Value = 300 });
				Inventory.AllGear = allKit;

				// nothing except the empty inventory - so only one possible empty inventory
				Assert.AreEqual(1, Inventory.ValidInventories(5).Count());

				// since the upper weight limit is so large we end up with all possible inventories over 3 belongings, which is 8 including the empty inventory
				Assert.AreEqual(8, Inventory.ValidInventories(5000).Count());
			}

		}
		}
	}
