using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPACE_UTIL;
using SPACE_UISystem;
using System.Threading.Tasks;
using SPACE_Stockfish;

namespace SPACE_CHESS
{
	public class ChessManager_1 : MonoBehaviour
	{
		/*
		static main_B
		static L<v2> reachable_COORD_from_unit(B, coord)
		static L<v2> opp_units_threatned_king(B, king_side) // depend on: reachable_COORD_from_unit
		static B get_new_B_suppose_move_was_made(from_coord,to_coord)
		static Doc<v2, L<v2>> MAP_from_availableTo(main_B, king_side)

		// when MAP_from_availableTo(main_B, king_side) got .Count == 0, gameOver for king_side
		*/

		#region OBJ
		[SerializeField] List<GameObject> PREFAB_ref;
		static List<GameObject> PREFAB; 
		#endregion
		private void Awake()
		{
			#region OBJ
			PREFAB = this.PREFAB_ref; 
			#endregion
			Gather();
			Debug.Log("Awake(): " + this);
		}

		[SerializeField] v2 check_coord_0 = (4, 0);
		[SerializeField] v2 check_coord_1 = (4, 4);
		private void Update()
		{
			if(INPUT.M.InstantDown(0))
			{
				// LOG.SaveLog(reachable_COORD_from_unit(main_B, this.check_coord_0).ToTable(name: "LIST<v2>: "));
				// LOG.SaveLog(opp_units_threatned_king(main_B, 'w').ToTable());
				// LOG.SaveLog(main_B.ToBoard('.'));
				// LOG.SaveLog(get_new_B_suppose_move_was_made(main_B, this.check_coord_0, this.check_coord_1).ToBoard('.'));
				// LOG.SaveLog(MAP_from_availableTo(main_B, king_side: 'w').ToTable());
			}

			#region UIToolTip.Ins.Show()
			INPUT.M.up = Vector3.forward;
			UIToolTip.Ins.Show();
			UIToolTip.Ins.SetPos(INPUT.UI.pos);
			UIToolTip.Ins.SetText(Ce.get_chess_coord(INPUT.M.getPos3D)); 
			#endregion
		}

		static string IN = "n2pk3/8/8/8/8/8/P7/RNBQKBNR b"; // initial FEN
		static List<List<char>> main_B;
		#region castling
		static Dictionary<v2, bool> MAP_hasMoved; 
		#endregion
		#region OBJ
		static List<List<GameObject>> main_OBJ_unit;
		static List<List<GameObject>> main_OBJ_reach;
		#endregion
		static void Gather()
		{
			// parse IN here >>
			main_B = Ce.FEN_to_B(IN);
			#region casting
			MAP_hasMoved = new Dictionary<v2, bool>()
			{
				[Ce.get_coord("e1")] = !(main_B.GT(Ce.get_coord("e1")) == 'K'),
				[Ce.get_coord("a1")] = !(main_B.GT(Ce.get_coord("a1")) == 'R'),
				[Ce.get_coord("h1")] = !(main_B.GT(Ce.get_coord("h1")) == 'R'),

				[Ce.get_coord("e8")] = !(main_B.GT(Ce.get_coord("e8")) == 'k'),
				[Ce.get_coord("a8")] = !(main_B.GT(Ce.get_coord("a8")) == 'r'),
				[Ce.get_coord("h8")] = !(main_B.GT(Ce.get_coord("h8")) == 'r'),
			};
			LOG.SaveLog(MAP_hasMoved.ToTable(name: "MAP_hasMoved<>"));
			#endregion

			#region OBJ
			Dictionary<char, GameObject> MAP_unitType_prefab = new Dictionary<char, GameObject>();
			foreach (GameObject obj in PREFAB)
			{
				char unit = obj.name.split(@" ")[1].toChar();
				MAP_unitType_prefab[unit] = obj;
			}

			main_OBJ_unit = new List<List<GameObject>>();
			for (int y = 0; y < Ce.h; y += 1)
			{
				main_OBJ_unit.Add(new List<GameObject>());
				for (int x = 0; x < Ce.w; x += 1)
				{
					char unit = main_B[y][x];
					// skip
					if (unit == ' ')
					{
						main_OBJ_unit[y].Add(null);
					}
					else
					{
						GameObject obj = GameObject.Instantiate(MAP_unitType_prefab[unit], C.PrefabHolder);
						obj.transform.position = new Vector3(x, y, 0);
						main_OBJ_unit[y].Add(obj);
					}
				}
			}

			GameObject reach_prefab = Resources.Load<GameObject>("pf_reach");
			main_OBJ_reach = new List<List<GameObject>>();
			for (int y = 0; y < Ce.h; y += 1)
			{
				main_OBJ_reach.Add(new List<GameObject>());
				for (int x = 0; x < Ce.w; x += 1)
				{
					GameObject obj = GameObject.Instantiate(reach_prefab, C.PrefabHolder);
					obj.transform.position = new v2(x, y);
					obj.SetActive(false);
					main_OBJ_reach[y].Add(obj);
				}
			}
			#endregion

			LOG.SaveLog(MAP_from_availableTo(main_B, 'b').ToTable(name: "MAP_from_availableTo<> 'b'"));
			// << parse IN here
		}

		#region core
		#region ad
		static char get_side_at_coord(List<List<char>> B, v2 coord)
		{
			char unit_1 = B[coord.y][coord.x];
			if (unit_1 == ' ')
				return ' ';
			return ((unit_1.fmatch(@"[RNBQKP]", "g") == true) ? 'w' : 'b');
		}
		#endregion
		static List<v2> reachable_COORD_from_unit(List<List<char>> B, v2 from_coord)
		{
			char unit = B[from_coord.y][from_coord.x];
			if (unit == ' ') // exit
				return new List<v2>();

			char curr_side = (unit.fmatch(@"[RNBQKP]", "g") == true) ? 'w' : 'b';
			char oppo_side = (unit.fmatch(@"[RNBQKP]", "g") == true) ? 'b' : 'w';

			List<v2> _reachable_COORD_from_unit = new List<v2>();
			// P
			if (unit.fmatch(@"[P]", "gi"))
			{
				v2[] DELTA = new v2[]
				{
					(0, +1),
					(-1, +1),
					(+1, +1),
					(0, +2),
				};

				if (curr_side == 'b')
					for (int i0 = 0; i0 < DELTA.Length; i0 += 1)
						DELTA[i0].y *= -1;

				v2 a = from_coord + DELTA[0],
				   b = from_coord + DELTA[1],
				   c = from_coord + DELTA[2],
				   d = from_coord + DELTA[3];

				if (a.inrange(Ce.m, Ce.M))
					if (get_side_at_coord(B, a) == ' ') // a has to be empty
					{
						_reachable_COORD_from_unit.Add(a);
						if (from_coord.y == ((curr_side == 'w') ? 1 : 6)) // only when from_coord.y == 1( for 'w' ) or 6( for 'b')
							if (get_side_at_coord(B, d) == ' ') // d has to be empty
								_reachable_COORD_from_unit.Add(d);
					}
				if (b.inrange(Ce.m, Ce.M))
					if (get_side_at_coord(B, b) == oppo_side) // b has to be opp_side
						_reachable_COORD_from_unit.Add(b);
				if (c.inrange(Ce.m, Ce.M))
					if (get_side_at_coord(B, c) == oppo_side) // c has to be opp_side
						_reachable_COORD_from_unit.Add(c);
			}
			// N
			else if (unit.fmatch(@"[N]", "gi"))
			{
				v2[] DELTA = new v2[]
				{
					(+1, +2),
					(-1, +2),
					(+1, -2),
					(-1, -2),

					(+2, +1),
					(-2, +1),
					(+2, -1),
					(-2, -1),
				};

				foreach (v2 n in DELTA)
				{
					v2 coord = from_coord + n;
					if (coord.inrange((0, 0), (7, 7)))
						if (get_side_at_coord(B, coord) == oppo_side || get_side_at_coord(B, coord) == ' ') // coord has to be opp_side or empty
							_reachable_COORD_from_unit.Add(coord);
				}
			}
			// R
			else if (unit.fmatch(@"[R]", "gi"))
			{
				foreach (v2 dir in v2.getDIR(diagonal: false))
					for (int i0 = 1; i0 <= (Ce.M - Ce.m).x; i0 += 1)
					{
						v2 coord = from_coord + dir * i0;
						if (coord.inrange(Ce.m, Ce.M) == false) // out of bounds
							break;
						if (get_side_at_coord(B, coord) == curr_side) // curr_side
							break;
						if (get_side_at_coord(B, coord) == oppo_side) // opp_side
						{
							_reachable_COORD_from_unit.Add(coord);
							break;
						}
						_reachable_COORD_from_unit.Add(coord); // empty
					}
			}
			// B
			else if (unit.fmatch(@"[B]", "gi"))
			{
				v2[] DIR = new v2[]
				{
					v2.getdir("ru"),
					v2.getdir("lu"),
					v2.getdir("rd"),
					v2.getdir("ld"),
				};

				foreach (v2 dir in DIR)
					for (int i0 = 1; i0 <= (Ce.M - Ce.m).x; i0 += 1)
					{
						v2 coord = from_coord + dir * i0;
						if (coord.inrange(Ce.m, Ce.M) == false)
							break;
						if (get_side_at_coord(B, coord) == curr_side)
							break;
						if (get_side_at_coord(B, coord) == oppo_side)
						{
							_reachable_COORD_from_unit.Add(coord);
							break;
						}
						_reachable_COORD_from_unit.Add(coord);
					}
			}
			// Q
			else if (unit.fmatch(@"[Q]", "gi"))
			{
				foreach (v2 dir in v2.getDIR(diagonal: true))
					for (int i0 = 1; i0 <= (Ce.M - Ce.m).x; i0 += 1)
					{
						v2 coord = from_coord + dir * i0;
						if (coord.inrange(Ce.m, Ce.M) == false)
							break;
						if (get_side_at_coord(B, coord) == curr_side)
							break;
						if (get_side_at_coord(B, coord) == oppo_side)
						{
							_reachable_COORD_from_unit.Add(coord);
							break;
						}
						_reachable_COORD_from_unit.Add(coord);
					}
			}
			// K
			else if (unit.fmatch(@"[K]", "gi")) // 'K' or 'k'
			{
				foreach (v2 dir in v2.getDIR(diagonal: true))
				{
					v2 coord = from_coord + dir * 1;
					if (coord.inrange(Ce.m, Ce.M) == false)
						continue; // break;
					if (get_side_at_coord(B, coord) == curr_side)
						continue; // break;
					if (get_side_at_coord(B, coord) == oppo_side)
					{
						_reachable_COORD_from_unit.Add(coord);
						continue; // break;
					}
					// when empty space
					_reachable_COORD_from_unit.Add(coord);
				}
				#region castling
				//is resulting in stack overflow when a even a non king unit eg: 'B' is make InstantDown(0)
				char unit_Q = (get_side_at_coord(B, from_coord) == 'w') ? 'R' : 'r';

				if (MAP_hasMoved.ContainsKey(from_coord))
					if (MAP_hasMoved[from_coord] == false) // 'K'
					{
						Debug.Log(MAP_hasMoved.ToTable(name: "MAP_hasMoved check inside availableTo"));
						/*
						v2 coord = from_coord + (2, 0);
						Debug.Log("entered square under attack check without method");
						LOG.SaveLog(MAP_from_availableTo(B, king_side: 'w').ToTable(name: "MAP_from_availableTo"));
						// LOG.SaveLog(MAP_from_availableTo(B, king_side: 'b').ToTable(name: "MAP_from_availableTo"));

						// Debug.Log(SquareUnderAttack(B, from_coord + (0, 0), oppo_side: 'b'));
							if (!SquareUnderAttack(B, from_coord + (0, 0), oppo_side: 'b') &&
								!SquareUnderAttack(B, from_coord + (1, 0), oppo_side: 'b') &&
								!SquareUnderAttack(B, from_coord + (2, 0), oppo_side: 'b'))
							{
								// if all 3 squares including king unit square is safe from oppo_side: 'b'
								if (MAP_hasMoved[from_coord + (3, 0)] == false) // 'R' on kingside
									_reachable_COORD_from_unit.Add(coord);
							}
						*/
					}
				#endregion
			}
			// any
			else
			{
				// every coord except the from_coord within bounds
				for (int y = 0; y < 8; y += 1)
					for (int x = 0; x < 8; x += 1)
						if (get_side_at_coord(B, (x, y)) != curr_side) // any coord as long as not the same side, (which is either ' ' or oppo_side )
						{
							_reachable_COORD_from_unit.Add((x, y));
						}
			}

			return _reachable_COORD_from_unit;
		}

		/*
			+ depends on: reachable_COORD_from_unit
		*/
		static List<v2> opp_units_threatned_king(List<List<char>> B, char king_side = 'w')
		{
			int w = B.Count,
				h = B[0].Count;

			char oppo_side = (king_side == 'w') ? 'b' : 'w';
			char king_unit = (king_side == 'w') ? 'K' : 'k';

			v2 king_coord = (0, 0);
			for (int y = 0; y < h; y += 1)
				for (int x = 0; x < w; x += 1)
					if (B[y][x] == king_unit)
					{
						king_coord = (x, y);
					}


			List<v2> UNIT = new List<v2>();
			for (int y = 0; y < h; y += 1)
				for (int x = 0; x < w; x += 1)
					if (get_side_at_coord(B, (x, y)) == oppo_side)
					{
						List<v2> COORD = reachable_COORD_from_unit(B, (x, y));
						if (COORD.findIndex(coord => coord == king_coord) != -1) // found somthng
						{
							UNIT.Add((x, y));
							continue;
						}
					}
			return UNIT;
		}
		static List<List<char>> get_new_B_suppose_move_was_made(List<List<char>> B, v2 from_coord, v2 to_coord)
		{
			List<List<char>> new_B = new List<List<char>>();
			#region clone
			int w = B.Count,
				h = B[0].Count;

			for (int y = 0; y < h; y += 1)
			{
				new_B.Add(new List<char>());
				for (int x = 0; x < w; x += 1)
					new_B[y].Add(B[y][x]);
			}
			#endregion

			char from_side = get_side_at_coord(new_B, from_coord);
			char to_side = get_side_at_coord(new_B, to_coord);

			if (to_side == from_side || from_side == ' ') // no move is made
			{
				Debug.Log($"similar side/from_side = ' ' \n{from_coord} {to_coord}, from_side: {from_side}");
				return new_B;
			}

			// make the move
			new_B[to_coord.y][to_coord.x] = new_B[from_coord.y][from_coord.x];
			new_B[from_coord.y][from_coord.x] = ' ';
			return new_B;
		}

		/*
			+ depends on: reachable_COORD_from_unit, opp_units_threatned_king
			+ depends on: opp_units_threatned_king
			+ depends on: get_new_B_suppose_move_was_made
		*/
		// when MAP_from_availableTo(main_B, king_side) got .Count == 0, **gameOver** for king_side
		// return all available pos each unit in king_side can make, so that: king won't be in danger
		static Dictionary<v2, List<v2>> MAP_from_availableTo(List<List<char>> main_B, char king_side = 'w')
		{
			Dictionary<v2, List<v2>> _MAP_from_availableTo = new Dictionary<v2, List<v2>>();

			int w = main_B[0].Count,
				h = main_B.Count;

			char curr_side = king_side;
			char oppo_side = (king_side == 'w') ? 'b' : 'w';

			for (int y = 0; y < h; y += 1)
				for (int x = 0; x < w; x += 1)
				{
					char unit = main_B[y][x];
					v2 unit_coord = (x, y);
					char unit_side = get_side_at_coord(main_B, (x, y));

					if (unit_side == king_side)
					{
						List<v2> COORD = reachable_COORD_from_unit(main_B, unit_coord);
						List<v2> new_COORD = new List<v2>();
						foreach (v2 to_coord in COORD)
						{
							List<v2> UNIT_threat_to_king = opp_units_threatned_king(get_new_B_suppose_move_was_made(main_B, unit_coord, to_coord), king_side: king_side);
							if (UNIT_threat_to_king.Count == 0)
								new_COORD.Add(to_coord);
						}
						_MAP_from_availableTo[unit_coord] = new_COORD;
					}
				}
			return _MAP_from_availableTo;
		}
		#endregion

		#region called externally
		/*
			+ depends on: Doc<v2, L<v2>> MAP_from_availableTo
		*/
		public static bool SquareUnderAttack(List<List<char>> B, v2 coord, char oppo_side = 'b')
		{
			Debug.Log("entered square under attack check");
			LOG.SaveLog(MAP_from_availableTo(B, king_side: 'w').ToTable(name: "MAP_from_availableTo"));

			/*
			foreach (var kvp in MAP_from_availableTo(B, king_side: oppo_side))
				foreach (v2 to_coord in kvp.Value)
					if (to_coord == coord)
						return true;
			*/
			return false;
		}
		// called externally when unit is drag and dropped >>
		public static bool IsAllowed(v2 from_coord, v2 to_coord, char king_side)
		{
			LOG.H("check");
			LOG.SaveLog(MAP_from_availableTo(main_B, king_side).ToTable(name: "MAP_v2__v2_L<v2>"));
			LOG.HEnd("check");
			List<v2> availableTo = MAP_from_availableTo(main_B, king_side)[from_coord];
			if (availableTo.findIndex(coord => coord == to_coord) != -1) // exist
				return true;
			return false;
		}
		
		public static void MakeMoveOnBoard(v2 from_coord, v2 to_coord)
		{
			#region OBJ
			char curr_side = get_side_at_coord(main_B, from_coord),
				 oppo_side = get_side_at_coord(main_B, to_coord);
			//
			main_OBJ_unit[from_coord.y][from_coord.x].transform.position = to_coord; // move curr unit
			if (oppo_side != ' ')
			{
				/*
				Debug.Log(oppo_side);
				Debug.Log(to_coord);
				Debug.Log(main_OBJ_unit[to_coord.y][to_coord.x]);
				*/			
				main_OBJ_unit[to_coord.y][to_coord.x].SetActive(false); // capture opponent unit if not empty
			}
			main_OBJ_unit[to_coord.y][to_coord.x] = main_OBJ_unit[from_coord.y][from_coord.x]; // ref to unit = from unit
			main_OBJ_unit[from_coord.y][from_coord.x] = null; // empty unit = null obj
			//
			#endregion

			// board move >>
			main_B[to_coord.y][to_coord.x] = main_B[from_coord.y][from_coord.x];
			main_B[from_coord.y][from_coord.x] = ' ';
			// << board move
		}
		public static async Task MakeCpuMoveOnBoard(char cpu_side = 'b')
		{
			await C.delay(500);
			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			Debug.Log("calculating...");
			string best = await SPACE_Stockfish.StockfishManager.Ins.SuggestAtDepth(Ce.B_to_FEN(main_B, oppo_side: cpu_side));
			sw.Stop();
			//
			#region log
			string chess_move = best.split(@"\n").gl(0).split(@" ")[1];
			Debug.Log("elapsed ms for best move: " + sw.ElapsedMilliseconds);
			LOG.H("calculating" + '-'.repeat(100));
			LOG.SaveLog(main_B.ToBoard('.'));
			LOG.SaveLog(Ce.B_to_FEN(main_B, oppo_side: cpu_side));
			LOG.SaveLog(best, $"move: {chess_move}");
			LOG.HEnd("calculating" + '-'.repeat(100));
			Debug.Log("calculating" + "best move after split: " + chess_move);
			#endregion

			if (chess_move.fmatch(@"[a-h][1-8][a-h][1-8]") == false)
			{
				if (chess_move.fmatch(@"none") == true) { Debug.Log("Best move is none"); return; }
				else									{ Debug.LogError("Best move is neither [a-h][1-8][a-h][1-8] nor its 'none'"); return; }
			}
			(v2 from_coord, v2 to_coord) = Ce.get_delta_coord(chess_move);
			MakeMoveOnBoard(from_coord, to_coord);
		}

		#region OBJ
		public static void ShowReach(bool enable, v2 from_coord, char king_side = 'w')
		{
			if(enable == true)
			{
				char oppo_side = (king_side == 'w') ? 'b' : 'w';
				foreach (v2 to_coord in MAP_from_availableTo(main_B, king_side: king_side)[from_coord])
				{
					/*
						obj
							empty unit sprite
							oppo unit sprite
					*/
					main_OBJ_reach[to_coord.y][to_coord.x].SetActive(true);
					if (get_side_at_coord(main_B, to_coord) == oppo_side) // found oppo side
						main_OBJ_reach[to_coord.y][to_coord.x].NameStartsWith("oppo").SetActive(true);
					else if(get_side_at_coord(main_B, to_coord) == ' ') // empty
						main_OBJ_reach[to_coord.y][to_coord.x].NameStartsWith("empty").SetActive(true);
				}
			}
			else
			{
				foreach (var OBJ in main_OBJ_reach)
					foreach (var obj in OBJ)
					{
						for (int i2 = 0; i2 < obj.transform.childCount; i2 += 1)
							obj.transform.GetChild(i2).gameObject.SetActive(false);
						obj.SetActive(false);
					}
			}
		}
		#endregion

		public static bool CheckForKingFall(char king_side = 'w')
		{
			foreach (var kvp in MAP_from_availableTo(main_B, king_side))
				if (kvp.Value.Count > 0) // found a move that can be made
					return false;
			// no moves found
			return true;
		}

		#region caslting
		public static void UpdateHasMoved_MAP(v2 from_coord, v2 to_coord)
		{
			if (MAP_hasMoved.ContainsKey(from_coord) == true)
				if (to_coord != from_coord)
					MAP_hasMoved[from_coord] = true;
		} 
		#endregion
		#endregion
	}
}


/* ```cs
	// make sure: coord is in range of B.w, B.h

	// 0.
	static char get_state_of_coord( B, coord ) => one of 'w' or 'b' or ' '

	// 1.
	static List<v2> reachable_COORD_from_unit( B, v2 from_coord )
	static List<v2> opp_units_threatned_king ( B, char king_side = 'w' )

	// 2.
	static List<List<char>> get_new_B_suppose_move_was_made( B, v2 from_coord, v2 to_coord )
	static Dictionary<v2, List<v2>> MAP_from_availableTo   ( main_B, char king_side = 'w' )

	// 3. Called externally 
		// depends on: MAP_from_availableTo( main_B, 'b' )
		public static bool SquareUnderAttack( B, v2 coord, char oppo_side = 'b' )
		// depends on: MAP_from_availableTo( main_B, 'w' )
		public static bool IsAllowed( v2 from_coord, v2 to_coord, char king_side = 'w')
``` */
