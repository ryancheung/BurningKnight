using BurningKnight.entity.component;
using BurningKnight.entity.item;
using Lens.entity;

namespace BurningKnight.entity.events {
	public class ItemAddedEvent : Event {
		public Item Item;
		public Item Old;
		public ItemComponent Component;
		public Entity Who;
	}
}