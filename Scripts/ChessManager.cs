using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using SPACE_UTIL;
using SPACE_UISystem;

namespace SPACE_CHESS
{
	public class ChessManager : MonoBehaviour
	{
		// Example: Black to move in the starting position is "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1"
		[TextArea(minLines: 3, maxLines: 5)]
		[SerializeField] string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1";

		public static ChessManager Ins;
		private void Awake()
		{
			Ins = this;
			Debug.Log("Awake(): " + this);
		}

		private void Update()
		{
			if (INPUT.M.InstantDown(0))
			{
				v2 pos_I = C.round(INPUT.M.getPos3D);
				//Debug.Log(ChessManager.Ins.getAllAvailablePos(C_E.get_coord("e2")).findIndex(pos => pos == pos_I));
			}

			#region UIToolTip
			// UIToolTip >>
			UIToolTip.Ins.Show();
			UIToolTip.Ins.SetText(((v2)INPUT.M.getPos3D).get_chess_coord());
			// << UIToolTip 
			#endregion
		}


		async void ASYNC()
		{
			/*
			// it works
			Debug.Log("a message");
			await C.delay(1000);
			Debug.Log("b message");
			await C.delay(2000);
			Debug.Log("c message");
			await C.delay(3000);
			Debug.Log("d message");
			return;
			*/
		}

		[SerializeField] GameObject[] PIECE;
		private void Start()
		{
			this.Gather();
		}

		// called externally
		[SerializeField] TMPro.TextMeshProUGUI tm_fen;
		public void MakeMove(string move = "e2e4", char just_to_log_side = 'w')
		{
			(v2 from, v2 to) = Ce.get_delta_coord(move);
			Debug.Log($"move of {from} {to} has been made for {move}");

			char from_char = B[from.y][from.x];
			char to_char = B[to.y][to.x];

			GameObject from_obj = B_OBJ[from.y][from.x];
			GameObject to_obj = B_OBJ[to.y][to.x];


			if (from_char == ' ')
				return;

			if(to_char != ' ') // there is a pawn
			{
				if ((from_char.fmatch(@"[rnbqkp]", "g") && to_char.fmatch(@"[rnbqkp]", "g")) == false) // different kind
				{
					// capture the to piece
					// alter the B >>
					B[to.y][to.x] = from_char;
					B[from.y][from.x] = ' ';
					// << alter the B

					// alter the B_OBJ >>
					B_OBJ[from.y][from.x] = null;
					B_OBJ[to.y][to.x] = from_obj;
					to_obj.SetActive(false); // (deactivate) and (un_ref the to_obj from B_OBJ)
					from_obj.transform.position = to;
					// << alter the B_OBJ
				}
				else
					return; // do nothing
			}
			else
			{
				// alter the B >>
				B[to.y][to.x] = from_char;
				B[from.y][from.x] = ' ';
				// << alter the B

				// alter the B_OBJ >>
				B_OBJ[from.y][from.x] = null;
				B_OBJ[to.y][to.x] = from_obj;
				from_obj.transform.position = to;
				// << alter the B_OBJ
			}

			Debug.Log(Ce.B_to_str(B));

			LOG.H(just_to_log_side + '-'.repeat(100));
			LOG.SaveLog(Ce.B_to_str(B));
			LOG.SaveLog(Ce.B_to_FEN(B));
			this.tm_fen.text = Ce.B_to_FEN(B);
			LOG.HEnd(just_to_log_side + '-'.repeat(100));
		}

		// called externally
		public List<v2> getAllAvailablePos(v2 from_coord)
		{
			List<v2> AllAvailPos = new List<v2>();

			char piece = this.B[from_coord.y][from_coord.x];
			// 'w' or 'b' or ' '
			char get_side_at_coord(v2 piece_coord) 
			{
				if(piece_coord.inrange((0, 0), (7, 7)))
				{
					char piece_at_coord = B[piece_coord.y][piece_coord.x];
					if (piece_at_coord == ' ')
						return ' ';
					else
						return piece_at_coord.fmatch(@"[RNBQKP]", "g")? 'w' : 'b';
				}
				return 'z'; // out of bound
			}
			char curr_side = (piece.ToString() == piece.ToString().ToUpper())? 'w' : 'b';
			char oppo_side = (curr_side == 'w')? 'b': 'w';

			// P
			if (piece == 'P')
			{
				v2 a = from_coord + (0, +1),
				   b = from_coord + (-1, +1),
				   c = from_coord + (+1, +1),
				   d = from_coord + (0, +2);

				if (a.inrange((0, 0), (7, 7)))
					if (get_side_at_coord(a) == ' ') // a has to be empty
					{
						AllAvailPos.Add(a);
						if (from_coord.y == 1) // only when from_coord.y == 1
							if (get_side_at_coord(d) == ' ') // d has to be empty
								AllAvailPos.Add(d);
					}
				if (b.inrange((0, 0), (7, 7)))
					if (get_side_at_coord(b) == oppo_side) // b has to be opp_side
						AllAvailPos.Add(b);
				if (c.inrange((0, 0), (7, 7)))
					if (get_side_at_coord(c) == oppo_side) // c has to be opp_side
						AllAvailPos.Add(c);
			}
			// N
			else if (piece == 'N')
			{
				v2[] PossibleDelta = new v2[]
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

				foreach (v2 n in PossibleDelta)
				{
					v2 coord = from_coord + n;
					if (coord.inrange((0, 0), (7, 7)))
						if (get_side_at_coord(coord) != oppo_side) // coord has to be opp_side
							AllAvailPos.Add(coord);
				}
			}
			// R
			else if (piece == 'R')
			{
				foreach (v2 dir in v2.getDIR(diagonal: false))
				{
					for (int i0 = 1; i0 <= 7; i0 += 1)
					{
						v2 coord = from_coord + dir * i0;
						if (coord.inrange(Ce.bound.m, Ce.bound.M) == false)
							break;
						if (get_side_at_coord(coord) == curr_side)
							break;
						if (get_side_at_coord(coord) == oppo_side)
						{
							AllAvailPos.Add(coord);
							break;
						}
						AllAvailPos.Add(coord);
					}
				}
			}
			// B
			else if (piece == 'B')
			{
				v2[] DIR = new v2[]
				{
					v2.getdir("ru"),
					v2.getdir("lu"),
					v2.getdir("rd"),
					v2.getdir("ld"),
				};

				foreach (v2 dir in DIR)
				{
					for (int i0 = 1; i0 <= 7; i0 += 1)
					{
						v2 coord = from_coord + dir * i0;
						if (coord.inrange(Ce.bound.m, Ce.bound.M) == false)
							break;
						if (get_side_at_coord(coord) == curr_side)
							break;
						if (get_side_at_coord(coord) == oppo_side)
						{
							AllAvailPos.Add(coord);
							break;
						}
						AllAvailPos.Add(coord);
					}
				}
			}
			// Q
			else if (piece == 'Q')
			{
				foreach (v2 dir in v2.getDIR(diagonal: true))
				{
					for (int i0 = 1; i0 <= 7; i0 += 1)
					{
						v2 coord = from_coord + dir * i0;
						if (coord.inrange(Ce.bound.m, Ce.bound.M) == false)
							break;
						if (get_side_at_coord(coord) == curr_side)
							break;
						if (get_side_at_coord(coord) == oppo_side)
						{
							AllAvailPos.Add(coord);
							break;
						}
						AllAvailPos.Add(coord);
					}
				}
			}
			else if (piece == 'K')
			{
				foreach (v2 dir in v2.getDIR(diagonal: true))
				{
					v2 coord = from_coord + dir * 1;
					if (coord.inrange(Ce.bound.m, Ce.bound.M) == false)
						continue; // break;
					if (get_side_at_coord(coord) == curr_side)
						continue; // break;
					if (get_side_at_coord(coord) == oppo_side)
					{
						AllAvailPos.Add(coord);
						continue; // break;
					}
					// when empty space
					AllAvailPos.Add(coord);
				}
			}
			else
			{
				// every coord except the from_coord within bounds
				for (int y = 0; y < 8; y += 1)
					for (int x = 0; x < 8; x += 1)
						if ((x, y) != from_coord && get_side_at_coord((x, y)) != curr_side)
						{
							AllAvailPos.Add((x, y));
						}
			}

			return AllAvailPos;
		}

		// called externally, depends on getAllAvailablePos
		public bool IsInAvailablePos(v2 from_coord, v2 to_coord)
		{
			return this.getAllAvailablePos(from_coord).findIndex(coord => coord == to_coord) != -1;
		}

		// called externally, 
		public bool IsCheckToKing(char side = 'w')
		{
			/*
			foreach(piece in B)
				if its a blackpiece 
					getAllAvailablePos(piece.pos).contains(king at side.pos)
						return true
			*/
			return false;
		}

		// called externally, 
		public bool IsInAvailablePosWithCheck(v2 from_coord, v2 to_coord)
		{
			return false;
		}

		//
		string IN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1";
		Dictionary<char, GameObject> MAP_type_prefab; // prefab library
		public List<List<char>> B;
		List<List<GameObject>> B_OBJ;

		void Gather()
		{
			// parse IN here >>
			MAP_type_prefab = new Dictionary<char, GameObject>();
			foreach (GameObject piece in this.PIECE)
			{
				string color = piece.name.split(@" ")[0];
				string type = piece.name.split(@" ")[1];

				MAP_type_prefab[type.toChar()] = piece;
			}

			B = Ce.FEN_to_B();
			B_OBJ = Ce.B_to_OBJ(B, MAP_type_prefab);

			/* checked
			foreach(var line in B)
				Debug.Log(line.map(_char => _char.ToString()).join(", "));
			Debug.Log(C_E.B_to_FEN_arrangement(B));
			*/
			// << parse IN here
		}
	}
}
