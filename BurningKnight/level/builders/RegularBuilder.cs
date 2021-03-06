using System;
using System.Collections.Generic;
using BurningKnight.level.rooms;
using BurningKnight.level.rooms.boss;
using BurningKnight.level.rooms.connection;
using BurningKnight.level.rooms.entrance;
using BurningKnight.level.rooms.granny;
using BurningKnight.level.rooms.oldman;
using BurningKnight.level.rooms.regular;
using BurningKnight.level.rooms.shop;
using BurningKnight.level.rooms.shop.sub;
using BurningKnight.save;
using BurningKnight.util;
using Lens.util;
using Lens.util.math;

namespace BurningKnight.level.builders {
	public class RegularBuilder : Builder {
		protected float[] BranchTunnelChances = {1, 0, 0};
		protected EntranceRoom Entrance;
		protected ExitRoom Exit;
		protected BossRoom Boss;
		protected GrannyRoom Granny;
		protected OldManRoom OldMan;
		protected List<RoomDef> SubShop = new List<RoomDef>();
		protected float ExtraConnectionChance = 0.2f;
		protected List<RoomDef> MultiConnection = new List<RoomDef>();
		protected float PathLength = 0.5f;
		protected float[] PathLenJitterChances = {0, 1, 0};
		protected float[] PathTunnelChances = {1, 3, 1};
		protected float PathVariance = 45f;
		protected List<RoomDef> SingleConnection = new List<RoomDef>();

		public void SetupRooms(List<RoomDef> Rooms) {
			Entrance = null;
			Exit = null;
			Boss = null;
			Granny = null;
			OldMan = null;
			MultiConnection.Clear();
			SingleConnection.Clear();
			SubShop.Clear();

			foreach (var Room in Rooms) {
				Room.SetEmpty();
			}

			foreach (var Room in Rooms) {
				if (Room is BossRoom b) {
					Boss = b;
				} else if (Room is OldManRoom) {
					OldMan = (OldManRoom) Room;
				} else if (Room is GrannyRoom) {
					Granny = (GrannyRoom) Room;
				} else if (Room is EntranceRoom) {
					Entrance = (EntranceRoom) Room;
				} else if (Room is ExitRoom) {
					Exit = (ExitRoom) Room;
				} else if (Room is SubShopRoom) {
					SubShop.Add(Room);
				} else if (Room.GetMaxConnections(RoomDef.Connection.All) == 1) {
					SingleConnection.Add(Room);
				} else if (Room.GetMaxConnections(RoomDef.Connection.All) > 1) {
					MultiConnection.Add(Room);
				}
			}

			WeightRooms(MultiConnection);
			MultiConnection = new List<RoomDef>(MultiConnection);
		}

		protected void WeightRooms(List<RoomDef> Rooms) {
			/*foreach (var Room in Rooms) {
				if (Room is RegularRoom room) {
					for (var I = 1; I < room.GetSize().GetConnectionWeight(); I++) {
						Rooms.Add(room);
					}
					
					Rooms.Add(room);
				}
			}*/
		}

		public override List<RoomDef> Build(List<RoomDef> Init) {
			return Init;
		}

		public RegularBuilder SetPathVariance(float Var) {
			PathVariance = Var;

			return this;
		}

		public RegularBuilder SetPathLength(float Len, float[] Jitter) {
			PathLength = Len;
			PathLenJitterChances = Jitter;

			return this;
		}

		public RegularBuilder SetTunnelLength(float[] Path, float[] Branch) {
			PathTunnelChances = Path;
			BranchTunnelChances = Branch;

			return this;
		}

		public RegularBuilder SetExtraConnectionChance(float Chance) {
			ExtraConnectionChance = Chance;
			return this;
		}

		protected bool CreateBranches(List<RoomDef> Rooms, List<RoomDef> Branchable, List<RoomDef> RoomsToBranch, float[] ConnChances) {
			var I = 0;
			var N = 0;
			float Angle;
			int Tries;
			RoomDef Curr;
			
			var ConnectingRoomsThisBranch = new List<RoomDef>();
			var ConnectionChances = ArrayUtils.Clone(ConnChances);
			
			while (I < RoomsToBranch.Count) {
				var R = RoomsToBranch[I];
				N++;
				ConnectingRoomsThisBranch.Clear();

				do {
					Curr = Branchable[Rnd.Int(Branchable.Count)];
				} while (Curr is ConnectionRoom);

				var ConnectingRooms = Rnd.Chances(ConnectionChances);

				if (ConnectingRooms == -1) {
					ConnectionChances = ArrayUtils.Clone(ConnChances);
					ConnectingRooms = Rnd.Chances(ConnectionChances);
				}

				ConnectionChances[ConnectingRooms]--;

				for (var J = 0; J < ConnectingRooms; J++) {
					var T = RoomRegistry.Generate(RoomType.Connection, LevelSave.BiomeGenerated);
					Tries = 3;

					do {
						Angle = PlaceRoom(Rooms, Curr, T, RandomBranchAngle(Curr));
						Tries--;
					} while (Math.Abs(Angle - (-1)) < 0.01f && Tries > 0);

					if (Math.Abs(Angle - (-1)) < 0.01f) {
						foreach (var C in ConnectingRoomsThisBranch) {
							C.ClearConnections();
							Rooms.Remove(C);
						}

						ConnectingRoomsThisBranch.Clear();

						break;
					}

					ConnectingRoomsThisBranch.Add(T);
					Rooms.Add(T);


					Curr = T;
				}

				if (ConnectingRoomsThisBranch.Count != ConnectingRooms) {
					if (N > 30) {
						return false;
					}

					continue;
				}

				Tries = 10;

				do {
					Angle = PlaceRoom(Rooms, Curr, R, RandomBranchAngle(Curr));
					Tries--;
				} while (Math.Abs(Angle - (-1)) < 0.01f && Tries > 0);

				if (Math.Abs(Angle - (-1)) < 0.01f) {
					foreach (var T in ConnectingRoomsThisBranch) {
						T.ClearConnections();
						Rooms.Remove(T);
					}

					ConnectingRoomsThisBranch.Clear();

					if (N > 30) {
						return false;
					}

					continue;
				}

				foreach (var AConnectingRoomsThisBranch in ConnectingRoomsThisBranch) {
					if (Rnd.Int(3) <= 1) {
						Branchable.Add(AConnectingRoomsThisBranch);
					}
				}

				if (R.GetMaxConnections(RoomDef.Connection.All) > 1 && Rnd.Int(3) == 0) {
					if (R is RegularRoom room) {
						/*for (var J = 0; J < room.GetSize().GetConnectionWeight(); J++) {
							Branchable.Add(room);
						}*/
						
						Branchable.Add(room);
					} else {
						Branchable.Add(R);
					}
				}

				I++;
			}

			return true;
		}

		protected virtual float RandomBranchAngle(RoomDef R) {
			return Rnd.Angle();
		}

		protected override float PlaceRoom(List<RoomDef> Collision, RoomDef Prev, RoomDef Next, float Angle) {
			var v = base.PlaceRoom(Collision, Prev, Next, Angle);

			if (v > -1 && Next is ShopRoom) {
				Log.Info("Placing sub shop rooms");
				
				foreach (var r in SubShop) {
					var a = Rnd.Angle();
					var i = 0;

					while (true) {
						var an = PlaceRoom(Collision, Next, r, a % 360);

						if ((int) an != -1) {
							break;
						}

						i++;

						if (i > 36) {
							Log.Error("Failed.");
							return -1;
						}

						a += 10;
					}
				}

				/*if (SubShop.Count > 0) {
					for (var i = 0; i < SubShop.Count - 1; i++) {
						for (var j = i + 1; j < SubShop.Count; j++) {
							SubShop[i].ConnectTo(SubShop[j]);
						}
					}
				}*/

				Log.Info("Failed to fail.");
			}

			return v;
		}
	}
}