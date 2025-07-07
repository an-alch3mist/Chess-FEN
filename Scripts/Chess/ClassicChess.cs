using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPACE_UTIL;
using System;

namespace SPACE_CHESS
{
	public class ClassicChess
	{
		static int w = 8,
				   h = 8;
		static v2 m = (0, 0),
				  M = (7, 7);

		Board<char> main_B;
		#region for castling
		Board<(char unitBegin, bool hasMoved)> castle_B; 
		#endregion

		public void Init(string Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1")
		{
			this.main_B = CHESS_UTIL.fromFen(Fen);
			//
			#region for castling
			castle_B = new Board<(char unit, bool hasMoved)>((w, h), (' ', false));
			for (int y = 0; y < h; y += 1)
				for (int x = 0; x < w; x += 1)
					castle_B.B[y][x].unitBegin = main_B.GT((x, y)); 
			#endregion
		}

		#region require
		static char GetUnitType(Board<char> B, v2 coord) // coord should be inrange
		{
			#region error
			if (!coord.in_range((0, 0), (B.w - 1, B.h - 1)))
				Debug.LogError($"{coord} is not in range of B ({B.m}, {B.M})"); 
			#endregion

			if (B.GT(coord) == ' ') return ' ';
			return B.GT(coord).fmatch(@"[RNBQKP]", "g") ? 'w' : 'b';
		}
		static ToUnitType GetToUnitType(Board<char> B, v2 fromCoord, v2 toCoord)
		{
			// not in range or from coord is ' '
			if (fromCoord.in_range(B.m, B.M) == false) return ToUnitType.none;
			if (toCoord.in_range(B.m, B.M) == false) return ToUnitType.none;
			if (B.GT(fromCoord) == ' ') return ToUnitType.none;

			if (B.GT(toCoord) == ' ') return ToUnitType.Empty;
			if (GetUnitType(B, fromCoord) == GetUnitType(B, toCoord))
				return ToUnitType.SameSide;
			else
				return ToUnitType.OppoSide;

		}

		static List<(v2 toCoord, ToUnitType toUnitType)> GetAvailableTo(Board<char> B, v2 fromCoord)
		{
			#region error
			if (!fromCoord.in_range((0, 0), (B.w - 1, B.h - 1)))
				Debug.LogError($"{fromCoord} is not in range of B ({B.m}, {B.M})");
			#endregion

			if (B.GT(fromCoord) == ' ') return new List<(v2 toCoord, ToUnitType coordType)>(); // if B.GT(fromCoord) is empty ' ' => .Count == 0

			var AvaiableTo = new List<(v2 toCoord, ToUnitType toUnitType)>();
			// P
			if (B.GT(fromCoord).fmatch(@"P", "gi")) // regardless of white or black
			{
				int yDir = (GetUnitType(B, fromCoord) == 'w') ? +1 : -1;

				v2 toCoord = fromCoord + new v2(0, +1 * yDir);
				ToUnitType toUnitType = GetToUnitType(B, fromCoord, toCoord);
				if (toUnitType == ToUnitType.Empty)
					AvaiableTo.Add((toCoord, toUnitType));

				toCoord = fromCoord + new v2(0, +2 * yDir);
				toUnitType = GetToUnitType(B, fromCoord, toCoord);
				if (toUnitType == ToUnitType.Empty)
					AvaiableTo.Add((toCoord, toUnitType));

				toCoord = fromCoord + new v2(+1, +1 * yDir);
				toUnitType = GetToUnitType(B, fromCoord, toCoord);
				if (toUnitType == ToUnitType.OppoSide)
					AvaiableTo.Add((toCoord, toUnitType));

				toCoord = fromCoord + new v2(-1, +1 * yDir);
				toUnitType = GetToUnitType(B, fromCoord, toCoord);
				if (toUnitType == ToUnitType.OppoSide)
					AvaiableTo.Add((toCoord, toUnitType));
				return AvaiableTo;
			}
			// N
			else if (B.GT(fromCoord).fmatch(@"N", "gi")) // regardless of white or black
			{
				v2[] DELTA = new v2[]
				{
					(+1, +2),
					(-1, +2),
					(+1, -2),
					(-1, -2),

					(+2, +1),
					(+2, -1),
					(-2, +1),
					(-2, -1),
				};
				foreach(v2 delta in DELTA)
				{
					v2 toCoord = fromCoord + delta;
					ToUnitType toUnitType = GetToUnitType(B, fromCoord, toCoord);
					if (toUnitType == ToUnitType.Empty || toUnitType == ToUnitType.OppoSide)
						AvaiableTo.Add((toCoord, toUnitType));
				}
				return AvaiableTo;
			}
			// R
			else if (B.GT(fromCoord).fmatch(@"R", "gi")) // regardless of white or black
			{
				foreach (v2 dir in v2.getDIR(diagonal: false))
				{
					int length = 1;
					ITER.reset();
					while(true)
					{
						if (ITER.iter_inc(1e2)) break;

						v2 toCoord = fromCoord + dir * length;
						ToUnitType toUnitType = GetToUnitType(B, fromCoord, toCoord);
						if (toUnitType == ToUnitType.Empty || toUnitType == ToUnitType.OppoSide) // out of range is handled in toUnitType
							AvaiableTo.Add((toCoord, toUnitType));
						length += 1;
					}
				}
				return AvaiableTo;
			}
			// B
			else if (B.GT(fromCoord).fmatch(@"B", "gi")) // regardless of white or black
			{
				v2[] DIR = new v2[]
				{
					(+1, +1),
					(-1, +1),
					(-1, -1),
					(+1, -1),
				};
				foreach (v2 dir in DIR)
				{
					int length = 1;
					ITER.reset();
					while (true)
					{
						if (ITER.iter_inc(1e2)) break;

						v2 toCoord = fromCoord + dir * length;
						ToUnitType toUnitType = GetToUnitType(B, fromCoord, toCoord);
						if (toUnitType == ToUnitType.Empty || toUnitType == ToUnitType.OppoSide) // out of range is handled in toUnitType
							AvaiableTo.Add((toCoord, toUnitType));
						length += 1;
					}
				}
				return AvaiableTo;
			}
			// Q
			else if (B.GT(fromCoord).fmatch(@"Q", "gi")) // regardless of white or black
			{
				foreach (v2 dir in v2.getDIR(diagonal : true))
				{
					int length = 1;
					ITER.reset();
					while (true)
					{
						if (ITER.iter_inc(1e2)) break;

						v2 toCoord = fromCoord + dir * length;
						ToUnitType toUnitType = GetToUnitType(B, fromCoord, toCoord);
						if (toUnitType == ToUnitType.Empty || toUnitType == ToUnitType.OppoSide) // out of range is handled in toUnitType
							AvaiableTo.Add((toCoord, toUnitType));
						length += 1;
					}
				}
				return AvaiableTo;
			}
			// K
			else if(B.GT(fromCoord).fmatch(@"K", "gi"))
			{
				foreach(v2 dir in v2.getDIR(diagonal: true))
				{
					v2 toCoord = fromCoord + dir;
					ToUnitType toUnitType = GetToUnitType(B, fromCoord, toCoord);
					if (toUnitType == ToUnitType.Empty || toUnitType == ToUnitType.OppoSide)
						AvaiableTo.Add((toCoord, toUnitType));
				}
				return AvaiableTo;
			}
			// any
			else
			{
				// every coord except the from_coord within bounds
				for (int y = 0; y < h; y += 1)
					for (int x = 0; x < w; x += 1)
					{
						v2 toCoord = fromCoord + (x, y);
						ToUnitType toUnitType = GetToUnitType(B, fromCoord, toCoord);
						if (toUnitType == ToUnitType.Empty || toUnitType == ToUnitType.OppoSide) // any coord as long as not the same side, (which is either ' ' or oppo_side )
							AvaiableTo.Add((toCoord, toUnitType));
					}
				return AvaiableTo;
			}

		}
		static bool IsCoordInDanger(Board<char> B, v2 atCoord, char oppoKingUnitType = 'b')
		{
			for (int y = 0; y < B.h; y += 1)
				for (int x = 0; x < B.w; x += 1)
				{
					if (GetUnitType(B, (x, y)) == oppoKingUnitType)
						if (GetAvailableTo(B, fromCoord: (x, y)).findIndex(unit => unit.toCoord == atCoord) != -1)
							return true;
				}
			return false;
		}
		#endregion
		/*
			require - GetCoordType(B, coord)		=> CoordType
			require - GetAvailableTo(B, from_coord) => L<toCoord, coordType>
			require - IsCoordInDanger(B, atCoord, oppoKingUnitType) => true/false

			UnitType => char
			ToUnitType => enum Empty/SameSide/OppoSide/none
		*/
		Dictionary<v2, List<(v2 toCoord, ToUnitType coordType)>> MAP_from_NonDangerAvailableTo(char fromKingSide = 'w') // Board<char> B
		{

			return null;
		}
	}

	enum ToUnitType
	{
		Empty,
		// EmptyCastle,
		SameSide,
		OppoSide,
		none,
	}

	public static class CHESS_UTIL
	{
		// "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1"
		// "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b - - 0 1"
		public static Board<char> fromFen(string Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b - - 0 1")
		{
			var B = new Board<char>((8, 8), ' ');
			int x = 0; int y = 0;
			foreach(string line in Fen.split(@" ")[0].split(@"\/"))
			{
				foreach(char _char in line)
				{
					if (_char.fmatch(@"[1-8]", "g") == true) // is a num between 1 to 8
						x += _char.parseInt();
					else if (_char.fmatch(@"[RNBQKP]", "gi") == true) // one of those char with ignore-case flag
					{
						B.ST((x, y), _char);
						x += 1;
					}
				}
				x = 0; y += 1; // reset x, incr y by 1
			}
			return B;
		}
	}
}
