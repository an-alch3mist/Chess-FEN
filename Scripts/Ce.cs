using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using SPACE_UTIL;

public static class Ce
{
	// Constants
	public static string loc_stockfish = Application.streamingAssetsPath + "/stockfish-windows-x86-64-avx2.exe";
	public static int w = 8, h = 8;
	public static v2 m = (0, 0), M = (7, 7);
	public static (v2 m, v2 M) bound
	{
		get
		{
			return ((0, 0), (7, 7));
		}
	}


	#region coord
	// a1, e5, g8 //
	public static v2 get_coord(this string chess_coord)
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
	public static (v2 from, v2 to) get_delta_coord(this string move)
	{
		string[] moves = move.match(@"[a-h][1-8]", "gi");
		if (moves.Length != 2)
			return ((-1, -1), (-1, -1));
		return
		(
			moves[0].get_coord(),
			moves[1].get_coord()
		);
	}
	// (0, 0)
	public static string get_chess_coord(this v2 @v2)
	{
		if (@v2.inrange((0, 0), (7, 7)) == true)
			return "" + "abcdefgh"[@v2.x] + "12345678"[@v2.y]; // "" at begining to make char + char = string, otherwise char + char = int
		return "none";
	}
	#endregion

	#region FEN, B
	public static string B_to_str(List<List<char>> B)
	{
		string STR = "";
		for (int y = B.Count - 1; y >= 0; y -= 1)
		{
			var row = B[y];
			STR += row.map(c => c.ToString()).join(" ") + '\n';
		}
		return STR;
	}

	public static List<List<char>> get_empty_board(int w = 8, int h = 8)
	{
		List<List<char>> B = new List<List<char>>();
		for (int y = 0; y < h; y += 1)
		{
			B.Add(new List<char>());
			for (int x = 0; x < w; x += 1)
				B[y].Add(' ');
		}
		return B;
	}
	// checked-0
	public static List<List<char>> FEN_to_B(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1")
	{
		// "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1";
		string arrangement = fen.split(@" ")[0];
		string[] LINE = arrangement.split(@"\/");
		//Debug.Log(LINE.ToTable(true, name: "LIST<> LINE"));

		var B = get_empty_board(8, 8);
		int x = 0, y = 7;

		foreach (string line in LINE)
		{
			foreach (char _char in line)
			{
				// Debug.Log("_char at line" + _char);
				if (_char.fmatch(@"[1-8]"))
					x += _char.parseInt();
				else if (_char.fmatch(@"[rnbqkp]", "gi"))
				{
					// Debug.Log(_char + " passed fmatch");
					B[y][x] = _char;
					x += 1;
				}
			}
			x = 0;
			y -= 1;
		}
		return B;
	}
	// checked-0
	public static string B_to_FEN_arrangement(List<List<char>> B)
	{
		int w = B[0].Count;
		int h = B.Count;

		string str = "";
		for (int y = h - 1; y >= 0; y -= 1)
		{
			int empty_spaces = 0;
			for (int x = 0; x < w; x += 1)
			{
				if (B[y][x] == ' ')
					empty_spaces += 1;
				else if (B[y][x].fmatch(@"[rnbqkp]", "gi"))
				{
					// (append and reset) empty spaces when a piece is found >>
					if (empty_spaces != 0)
						str += empty_spaces.ToString();
					empty_spaces = 0;
					// << (append and reset) empty spaces when a piece is found
					str += B[y][x];
				}
			}
			if (empty_spaces != 0) // empty spaces before line ending
				str += empty_spaces.ToString();

			if (y != 0)
				str += "/";
		}
		return str;
	}

	public static string B_to_FEN(List<List<char>> B, char oppo_side = 'b')
	{
		// 1) Generate only the piece‑placement
		var placement = Ce.B_to_FEN_arrangement(B);
		// 2) Statically append the four “dummy” fields:
		return $"{placement} {oppo_side} - - 0 1";
	}
	#endregion

	#region B_OBJ
	public static List<List<GameObject>> B_to_OBJ(List<List<char>> B, Dictionary<char, GameObject> MAP_type_prefab)
	{
		int w = B[0].Count;
		int h = B.Count;

		List<List<GameObject>> B_OBJ = new List<List<GameObject>>();
		for (int y = 0; y < h; y += 1)
		{
			B_OBJ.Add(new List<GameObject>());
			for (int x = 0; x < w; x += 1)
				B_OBJ[y].Add(null);
		}

		//
		for (int y = 0; y < h; y += 1)
			for (int x = 0; x < w; x += 1)
			{
				if (B[y][x] != ' ')
				{
					GameObject obj = GameObject.Instantiate(MAP_type_prefab[B[y][x]], C.PrefabHolder);
					obj.transform.position = new v2(x, y);
					B_OBJ[y][x] = obj;
				}
			}
		return B_OBJ;
	}
	#endregion

	#region ext
	public static T GT<T>(this IEnumerable<IEnumerable<T>> B, v2 coord)
	{
		try
		{
			return B.ElementAt(coord.y).ElementAt(coord.x);
		}
		catch (System.Exception e)
		{
			if(B.Count() != 0)
				Debug.LogError($"no element at {coord} with B dimension h: {B.Count()}, w: {B.ElementAt(0).Count()}");
			else
				Debug.LogError($"no element at {coord} with B dimension h: 0, w: 0");

			throw;
		}
	}
	#endregion
}