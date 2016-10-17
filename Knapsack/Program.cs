using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knapsack
{
	public class Belonging
	{
		public byte Number { get; set; }
		public string Name { get; set; }
		public int GramsWeight { get; set; }
		public int Value { get; set; }

		public uint AddToGear(uint gear)
		{
			return gear | (uint)(1 << this.Number);
		}
		public uint RemoveFromGear(uint gear)
		{
			return gear & ~(uint)(1 << this.Number);
		}
		public bool IsInGear(uint gear)
		{
			return (gear & (1 << this.Number)) == (1 << this.Number);
		}
		public int GramsWeightInGear(uint gear)
		{
			return this.IsInGear(gear) ? this.GramsWeight : 0;
		}
		public int ValueInGear (uint gear)
		{
			return this.IsInGear(gear) ? this.Value : 0;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1} g, valued at {2}",
				this.Name,
				this.GramsWeight,
				this.Value);
		}
	}

	public class Inventory
	{
		public static List<Belonging> AllGear { get; set; }

		public uint Gear { get; set; }

		public int TotalGramsWeight { get { return Inventory.TotalGramsWeightForGear(this.Gear); } }
		public int TotalValue { get { return Inventory.TotalValueForGear(this.Gear); } }

		public static int TotalGramsWeightForGear (uint gear)
		{
				return AllGear.Select(o => 
					o.GramsWeightInGear(gear))
					.Sum();
			}

		public static int TotalValueForGear(uint gear)
		{
				return AllGear.Select(o => 
					o.ValueInGear(gear))
					.Sum();
			}


		public static uint NumberOfCombinations
		{
			get
			{
				return (~(uint)0 % (uint)(1 << AllGear.Count()));
			}
		}

		public static Inventory FirstBestInventory(int maxGramsWeight)
		{
			var numberOfCombinations = Inventory.NumberOfCombinations;
			int currentMaxValue = 0, overallBestValue = 0;
			var bestInventory = new Inventory();

			for (uint g = 0; g <= numberOfCombinations; g++)
				if (Inventory.TotalGramsWeightForGear(g) <= maxGramsWeight &&
					(currentMaxValue = Inventory.TotalValueForGear(g)) > overallBestValue)
				{
					bestInventory = new Inventory
					{
						Gear = g
					};
					overallBestValue = currentMaxValue;
				}

			return bestInventory;
		}

		public static IQueryable<Inventory> ValidInventories(int maxGramsWeight)
		{
			var numberOfCombinations = Inventory.NumberOfCombinations;
			var validInventories = new List<Inventory>();

			for (uint g = 0; g <= numberOfCombinations; g++)
				if (Inventory.TotalGramsWeightForGear(g) <= maxGramsWeight)
					validInventories.Add(new Inventory
					{
						Gear = g
					}
					);

			return validInventories.AsQueryable();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("---- Inventory Start:");
			AllGear.Where(o => o.IsInGear(this.Gear))
				.ToList()
				.ForEach(o => sb.AppendLine(o.ToString()));
			sb.AppendFormat("---- Inventory End: Total Weight: {0} g, Total Value: {1}",
				this.TotalGramsWeight,
				this.TotalValue);
			return sb.ToString();
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var allKit = LoadData();

			Inventory.AllGear = allKit;

			var bestInventories = Inventory
				.ValidInventories(maxGramsWeight: 4000)
				.OrderByDescending(o => o.TotalValue)
				.Take(5)
				.ToList();

			if (bestInventories.Count() == 0)
				Console.WriteLine("No inventory match the requirements.");
			else
			Console.WriteLine(string.Format("Best {0} inventories in descending order of value are:",bestInventories.Count()));
			bestInventories.ForEach(o =>
				Console.WriteLine(o)
				);

			var firstBestInventory = Inventory.FirstBestInventory( maxGramsWeight: 4000 );
			if (firstBestInventory == null)
				Console.WriteLine("No first best inventory found.");
			else
			{
				Console.WriteLine("Best inventory found: ");
				Console.WriteLine(firstBestInventory);
			}
		}

		private static List<Belonging> LoadData()
		{
			var allKit = new List<Belonging>();
			allKit.Add(new Belonging { Number = 0, Name = "map", GramsWeight = 90, Value = 150 });
			allKit.Add(new Belonging { Number = 1, Name = "compass", GramsWeight = 130, Value = 35 });
			allKit.Add(new Belonging { Number = 2, Name = "water", GramsWeight = 1530, Value = 300 });
			allKit.Add(new Belonging { Number = 3, Name = "Gold bar", GramsWeight = 3000, Value = 130 });
			allKit.Add(new Belonging { Number = 4, Name = "sandwich", GramsWeight = 500, Value = 160 });
			allKit.Add(new Belonging { Number = 5, Name = "glucose", GramsWeight = 150, Value = 60 });
			allKit.Add(new Belonging { Number = 6, Name = "tin", GramsWeight = 680, Value = 45 });
			allKit.Add(new Belonging { Number = 7, Name = "banana", GramsWeight = 270, Value = 60 });
			allKit.Add(new Belonging { Number = 8, Name = "apple", GramsWeight = 390, Value = 40 });
			allKit.Add(new Belonging { Number = 9, Name = "cheese", GramsWeight = 230, Value = 30 });
			allKit.Add(new Belonging { Number = 10, Name = "beer", GramsWeight = 620, Value = 10 });
			allKit.Add(new Belonging { Number = 11, Name = "suntan cream", GramsWeight = 110, Value = 70 });
			allKit.Add(new Belonging { Number = 12, Name = "camera", GramsWeight = 320, Value = 30 });
			allKit.Add(new Belonging { Number = 13, Name = "T-shirt", GramsWeight = 240, Value = 15 });
			allKit.Add(new Belonging { Number = 14, Name = "trousers", GramsWeight = 480, Value = 10 });

			return allKit;
		}
	}
}
