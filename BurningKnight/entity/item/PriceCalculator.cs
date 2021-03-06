using System;
using BurningKnight.state;

namespace BurningKnight.entity.item {
	public static class PriceCalculator {
		public static int BasePrice(ItemType type) {
			switch (type) {
				case ItemType.Lamp:
				case ItemType.Artifact: return 15;
				
				case ItemType.Active:
				case ItemType.Coin: return 10;
				
				case ItemType.Bomb:
				case ItemType.Key:
				case ItemType.Battery:
				case ItemType.Heart: return 3;
				
				case ItemType.Hat: return 1;
				
				default: return 15;
			}
		}

		public static float GetPriceModifier(this ItemQuality quality) {
			switch (quality) {
				case ItemQuality.Wooden: default: return 1;
				case ItemQuality.Iron: return 1.333f;
				case ItemQuality.Golden: return 2f;
				case ItemQuality.Trash: return 0.5f;
			}	
		}

		public static float GetModifier(Item item) {
			return Math.Max(1, Math.Min(99, (Run.Loop > 0 ? 7 : 1) * (Scourge.IsEnabled(Scourge.OfGreed) ? 2 : 1) * item.Data.Quality.GetPriceModifier()));
		}
		
		public static int Calculate(Item item) {
			return (int) Math.Round(BasePrice(item.Type) * GetModifier(item));
		}
	}
}