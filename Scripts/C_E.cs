using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using SPACE_UTIL;

public static class C_E
{
	public static string loc_stockfish = Application.streamingAssetsPath + "/stockfish-windows-x86-64-avx2.exe";


	public static (Vector2 m, Vector2 M) bound
	{
		get
		{
			return (new Vector2(0, 0), new Vector2(7, 7));
		}
	}

	// a1, e5, g8 //
	public static v2 v2_coord(this string chess_coord)
	{
		if (chess_coord.fmatch(@"[a-h][1-8]", "gi") == false)
			return (-1, -1);

		v2 p = new v2()
		{
			x = ("abcdefgh").IndexOf(chess_coord[0]),
			y = ("12345678").IndexOf(chess_coord[1]),
		};
		return p;
	}

	// a1e5, e5g8, g8h1 //
	public static (v2 from, v2 to) from_to_v2_coord(this string move)
	{
		string[] moves = move.match(@"[a-h][1-8]", "gi");
		if (moves.Length != 2)
			return ((-1, -1), (-1, -1));
		return
		(
			moves[0].v2_coord(),
			moves[1].v2_coord()
		);
	}

	// (0, 0)
	public static string chess_coord(this v2 @v2)
	{
		if(@v2.inrange((0, 0), (7, 7)) == true)
			return "" + "abcdefgh"[@v2.x] + "12345678"[@v2.y]; // "" at begining to make char + char = string, otherwise char + char = int
		return "none";
	}

	public static List<List<char>> fen_to_B(string fen)
	{
		// TODO fen_to_B
		return new List<List<char>>();
	}
}