+++
date = "2016-10-12T22:01:20+02:00"
draft = false
title = "Knapsack Bitwise"
Description = "A solution and discussion of the 0/1 knapsack problem, in C# using bit masks and bitwise operators."
Keywords = ["knapsack problem", "bitwise operators", "brute force", "algorithms", "TDD", "C#"]
tags = ["knapsack problem", "bitwise operators", "brute force", "algorithms", "TDD", "C#"]
categories = ["algorithms", "computer science", "optimisation", "complexity"]
+++

You can read this page on [my blog[[goodfast] as well if you'd like.

An interesting bit of computer science, the [knapsack problem][WikipediaKnapsack] has been studied for over a century, and according to Wikipedia, seems to be quite  popular - as these sort of things go.
For the first post in this series I'll present a solution to the 1/0, or binary version of this famous problem
I designed in 2015.

I was immediately intrigued by it when I first read the problem statement.
It's application to anything requireing optimal resource allocation was 
very clear, and my mind started obsessively thinking of how to solve this efficiently.
Now with a heavily studied problem like this, there are of course already many algorithms
developed, but since this problem was a test, I did not look anything up, and just started coding a solution as soon as I had one.

The full solution is [available here on GitHub][GithubKnapsack], but you are encouraged to copy and paste from this article, into your own project, to understand the whole thing bit by bit.

## What is it?

The 0/1 or binary variant is very simple. Given a set of items, where each item has a weight and value, determine the optimal 
selection of items such that the sum of the weight of all the items do not exceed some limit,
while the sum of the value of all of the items is maximised.

The 0/1 or binary part of the name comes from the restriction that only one of each item may be chosen.
The "knapsack" in the name refers to a ficticious rugsack or bag that can only contain a given weight.
The complexity of the problem lies in the exponential explosion of all the possible selection of items.

## A concrete example

Suppose we have the following set of 15 items:

 Number | Name | Weight Grams | Value
--------|------|--------------|-------
 0      | map | 90 | 150 
 1 | compass | 130 | 35 
 2 | water | 1530 | 300 
 3 | Gold bar | 3000 | 130 
 4 | sandwich | 500 | 160 
 5 | glucose | 150 | 60 
 6 | tin | 680 | 45 
 7 | banana | 270 | 60 
 8 | apple | 390 | 40 
 9 | cheese | 230 | 30 
 10 | beer | 620 | 10 
 11 | suntan cream | 110 | 70 
 12 | camera | 320 | 30 
 13 | T-shirt | 240 | 15 
 14 | trousers | 480 | 10 

Our knapsack can only hold a maximum of 4 Kg or 4000 grams, but we want to choose  a selection of items (or inventory) with the highest possible value.

## How many possible inventories exist?

Given we have `n` items to choose from, each with a weight `w` and value `v`, we observe that:

1. We will never be choosing zero items.
1. We can either choose to include an item or not.

We take a stab at a formula for calculating the number of choices: `c = 2^n - 1`. The `2^n` is because each item can either be chosen or not chosen, and the `- 1` is to eliminate the selection of not choosing anything.

Let's see how it holds for one item: `c = 2^1 - 1 = 2 - 1 = 1`, obvious, since with one item you only have one choice - this can be our base case, so for `n = 1` the formula holds. 
Now we consider `n + 1`, so for 2: `c = 2^2 - 1 = 4 - 1 = 3`, and this makes sense since you can either choose one of the items or both, so for `n + 1` the formula also holds.
So, by mathematical induction, our formula is proven, although we knew it was going to be right intuetively.

Using our formula, we conclude that there are `c = 2^15 - 1 = 32768 - 1 = 32767` possible inventories.  
This number is neglegeable in computer terms, yet already far too big for by hand calculation.

## Brute force solution

Since our particular senario is so small we will simply do a brute force search to find the best inventories (if any) that satisfy the 4000 grams weight limit.

Any inventory can only have a maximum of 15 items, so we will represent an arbitrary inventory with a 
bit mask, where each bit will represent a particular item, with 0 meaning the item is not chosen, and 1 meaning the item was chosen.

The bit mask will drive a calculation function to determine the total weight and value of a given inventory.
Considering all possible inventories is now reduced to iterating over the number 1 through 32767 possible options, 
calculating the sum of the weights and values each time, and retaining those inventories where the weight limit is satisfied.

## Complexity

Using our formula, we can say that our solution will have `O(2^n - 1)` time complexity, and similarly, worst case `O(2^n - 1)` 
space complexity.

## Implementation

### Belonging and Inventory class

Looking at the table of belongings, we realise we would need a class to 
represent individual belongings (we'll call it `class Belonging`), as well as the set of all   of them (`class Inventory`).

``` C#
namespace Knapsack
{
public class Belonging
	{
		public byte Number { get; set; }
		public string Name { get; set; }
		public int GramsWeight { get; set; }
		public int Value { get; set; }
	}

	public class Inventory
	{
		public static List<Belonging> AllGear { get; set; }

		public uint Gear { get; set; }
	}
}
```

### Functions for Belonging class

Now that we've dealt with the data needs, let's add some functionality to each of the classes.
In true [test driven development][TDD] style, let's first create unit tests for each function, followed by an implementation.

We'll wrap all the test classes in a general `TestKnapsack` class which we can use to do setup and teardown actions if required.

``` C#
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
	}
}
```

Returning to our `Belonging` class,  let's implement the methods we described in our unit test class.

We will make heavy use of the bitwise operators:

* `<<` and `>>` : bit shifts, which moves all the bits in a number left or right by the given count
* `&` and `|` : bitwise AND and OR which combines the two numbers bit by bit and returns the resulting number
* `~` : the compliment or negation unary operator that inverts all bits

Make sure you are completely familiar with all the [C# bit wise operators][CSBitwise] before proceeding.

Here are the functions we add to `Belonging`: 

``` C#
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
```

Success! Our tests all pass, and we can proceed to testing and developing `Inventory`.


### Functions for Inventory

We start by testing the very simple summation functions.
Our test creates a sample set of belongings (`allKit`) with 3 items in it.
We then add the first and last item from `allKit` and ensure we get the correct totals when calling the summation functions on 
`Inventory`:

``` C#
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
		}
```

Here are the summation functions to add to `Inventory`:

``` C#

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
```

Nothing too complicated, and the tests all still pass. Next we move on to the actual search  
for the valid inventories:

### Search functions for Inventory

``` C#

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
```

We implement the new functions on `Invetory` as follows:

``` C#
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
```

### Final wrapup

After testing everything again and getting all tests to still pass, we are very nearly done.
Our final work will be to create nice printout `ToString()` methods, and putting it all together in a `main` method which will be the entry point for the console application.

We add the following to `Belonging`:

``` C#

		public override string ToString()
		{
			return string.Format("{0}, {1} g, valued at {2}",
				this.Name,
				this.GramsWeight,
				this.Value);
		}
```

We want nice printout for `Inventory` also, so we add the following to it:

``` C#
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
```

Finally, we add the `main` method, along with a little helper method for setting up our set of all kit:

``` C#
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
```

## Finished! Let's give it a spin...

We are done! Let's see what we get if we run it all:

```
Best 5 inventories in descending order of value are:
---- Inventory Start:
map, 90 g, valued at 150
compass, 130 g, valued at 35
water, 1530 g, valued at 300
sandwich, 500 g, valued at 160
glucose, 150 g, valued at 60
banana, 270 g, valued at 60
apple, 390 g, valued at 40
cheese, 230 g, valued at 30
suntan cream, 110 g, valued at 70
camera, 320 g, valued at 30
T-shirt, 240 g, valued at 15
---- Inventory End: Total Weight: 3960 g, Total Value: 950
---- Inventory Start:
map, 90 g, valued at 150
compass, 130 g, valued at 35
water, 1530 g, valued at 300
sandwich, 500 g, valued at 160
glucose, 150 g, valued at 60
banana, 270 g, valued at 60
apple, 390 g, valued at 40
cheese, 230 g, valued at 30
suntan cream, 110 g, valued at 70
camera, 320 g, valued at 30
---- Inventory End: Total Weight: 3720 g, Total Value: 935
---- Inventory Start:
map, 90 g, valued at 150
compass, 130 g, valued at 35
water, 1530 g, valued at 300
sandwich, 500 g, valued at 160
glucose, 150 g, valued at 60
tin, 680 g, valued at 45
banana, 270 g, valued at 60
cheese, 230 g, valued at 30
suntan cream, 110 g, valued at 70
T-shirt, 240 g, valued at 15
---- Inventory End: Total Weight: 3930 g, Total Value: 925
---- Inventory Start:
map, 90 g, valued at 150
compass, 130 g, valued at 35
water, 1530 g, valued at 300
sandwich, 500 g, valued at 160
glucose, 150 g, valued at 60
tin, 680 g, valued at 45
banana, 270 g, valued at 60
apple, 390 g, valued at 40
suntan cream, 110 g, valued at 70
---- Inventory End: Total Weight: 3850 g, Total Value: 920
---- Inventory Start:
map, 90 g, valued at 150
compass, 130 g, valued at 35
water, 1530 g, valued at 300
sandwich, 500 g, valued at 160
glucose, 150 g, valued at 60
banana, 270 g, valued at 60
apple, 390 g, valued at 40
cheese, 230 g, valued at 30
suntan cream, 110 g, valued at 70
T-shirt, 240 g, valued at 15
---- Inventory End: Total Weight: 3640 g, Total Value: 920
Best inventory found: 
---- Inventory Start:
map, 90 g, valued at 150
compass, 130 g, valued at 35
water, 1530 g, valued at 300
sandwich, 500 g, valued at 160
glucose, 150 g, valued at 60
banana, 270 g, valued at 60
apple, 390 g, valued at 40
cheese, 230 g, valued at 30
suntan cream, 110 g, valued at 70
camera, 320 g, valued at 30
T-shirt, 240 g, valued at 15
---- Inventory End: Total Weight: 3960 g, Total Value: 950

```

## Conclusion

We have solved the 0/1  version of the knapsack problem, and it has been fun!
Not only is our solution well tested, but it runs fast, for small numbers of items.

In subsequent articles in this series we will expand on our solution, test it for larger number of items, and hopefully tackle the 
other, harder versions of this interesting computer science problem.

Who knows, perhaps we'll even take a stab at a quantum algorithm!

If only we could do this type of programming all day long...

The full solution is [available here, on GitHub][GithubKnapsack]

[WikipediaKnapsack]: https://en.wikipedia.org/wiki/Knapsack_problem
[CSBitwise]: https://www.tutorialspoint.com/csharp/csharp_bitwise_operators.htm
[GithubKnapsack]: https://github.com/Kerneels/knapsack
[goodfast]: http://goodfast.info/post/knapsack-bitwise/
