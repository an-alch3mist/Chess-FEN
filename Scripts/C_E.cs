using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using SPACE_UTIL;

public static class C_E
{
	public static string loc_stockfish = Application.streamingAssetsPath + "/stockfish-windows-x86-64-avx2.exe";

	// a1, e5, g8 //
	public static v2 chess_coord(string coord)
	{
		if (coord.fmatch(@"[a-h][1-8]", "gi") == false)
			return (-1, -1);

		v2 p = new v2()
		{
			x = ("abcdefgh").IndexOf(coord[0]),
			y = ("12345678").IndexOf(coord[1]),
		};
		return p;
	}

	public static (v2 from, v2 to) chess_move(string move)
	{
		string[] moves = move.match(@"[a-h][1-8]", "gi");
		if (moves.Length != 2)
			return ((-1, -1), (-1, -1));
		return
		(
			chess_coord(moves[0]),
			chess_coord(moves[1])
		);
	}
}