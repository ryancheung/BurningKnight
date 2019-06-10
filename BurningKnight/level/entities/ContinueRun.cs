using BurningKnight.save;
using BurningKnight.state;
using Lens;
using Lens.assets;
using Lens.entity;

namespace BurningKnight.level.entities {
	public class ContinueRun : Exit {
		private int depth;
		
		public ContinueRun() {
			if (Engine.EditingLevel) {
				return;
			}

			if (SaveManager.ExistsAndValid(SaveType.Game, s => {
				    depth = GameSave.PeekDepth(s);
			    })
			    
			    && SaveManager.ExistsAndValid(SaveType.Level, null, $"{SaveManager.SlotDir}level-1.lvl")
			    && SaveManager.ExistsAndValid(SaveType.Player)) {

				return;
			}
			
			Done = true;
		}

		protected override bool Interact(Entity entity) {
			Run.Depth = 1;
			return true;
		}

		protected override string GetFxText() {
			return $"{Locale.Get("continue_run")} (depth {depth})";
		}

		protected override bool CanInteract(Entity e) {
			return true;
		}
	}
}