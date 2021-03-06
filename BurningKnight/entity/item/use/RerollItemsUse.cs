using BurningKnight.assets.items;
using BurningKnight.entity.component;
using BurningKnight.entity.item.stand;
using BurningKnight.state;
using BurningKnight.util;
using ImGuiNET;
using Lens.entity;
using Lens.lightJson;
using Lens.util.math;

namespace BurningKnight.entity.item.use {
	public class RerollItemsUse : ItemUse {
		private bool rerollStands;
		private bool spawnNewItems;
		private bool ignore;
		private ItemType[] types;
		private float consumeChance;
		private bool d2;
		
		public override void Use(Entity entity, Item self) {
			var room = entity.GetComponent<RoomComponent>().Room;

			if (room == null) {
				return;
			}
			
			Reroller.Reroll(entity.Area, room, rerollStands, spawnNewItems, ignore, types, ProcessItem, d2);
		}

		protected virtual void ProcessItem(Item item) {
			if (consumeChance > 0 && Rnd.Chance(consumeChance * 100)) {
				item.Done = true;
			}
		}
		
		public override void Setup(JsonValue settings) {
			base.Setup(settings);

			rerollStands = settings["r_stands"].Bool(true);
			spawnNewItems = settings["s_new"].Bool(true);
			ignore = settings["ignore"].Bool(true);
			consumeChance = settings["cc"].Number(0);
			d2 = settings["d2"].Bool(false);

			var tps = settings["types"];

			if (tps.IsJsonArray) {
				var tp = tps.AsJsonArray;

				if (tp.Count == 0) {
					return;
				}
				
				types = new ItemType[tp.Count];

				for (var i = 0; i < tp.Count; i++) {
					types[i] = (ItemType) tp[i].Int(0);
				}
			}
		}

		public static void RenderDebug(JsonValue root) {
			var rerollStands = root["r_stands"].Bool(true);
			var spawnNew = root["s_new"].Bool(true);
			var ignore = root["ignore"].Bool(true);

			root.InputFloat("Consume Chance", "cc", 0);
			root.Checkbox("D2", "d2", false);
			
			if (ImGui.Checkbox("Reroll stands", ref rerollStands)) {
				root["r_stands"] = rerollStands;
			}

			if (ImGui.Checkbox("Spawn new", ref spawnNew)) {
				root["s_new"] = spawnNew;
			}

			var tps = root["types"];

			if (!tps.IsJsonArray) {
				tps = root["types"] = new JsonArray();
			}

			if (ImGui.TreeNode("Item types")) {
				if (ImGui.Checkbox("Ignore those types", ref ignore)) {
					root["ignore"] = ignore;
				}
				
				ImGui.Separator();
				
				var tp = tps.AsJsonArray;
				var toRemove = -1;
				var toAdd = -1;

				for (var i = 0; i < ItemEditor.Types.Length; i++) {
					var v = tp.Contains(i);

					if (ImGui.Checkbox(ItemEditor.Types[i], ref v)) {
						if (v) {
							toAdd = i;
						} else {
							toRemove = tp.IndexOf(i);
						}
					}
				}

				if (toRemove != -1) {
					tp.Remove(toRemove);
				} else if (toAdd != -1) {
					tp.Add(toAdd);
				}
				
				ImGui.TreePop();
			}
		}
	}
}