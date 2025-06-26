using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using SPACE_UTIL;

namespace SPACE_CHESS
{
	public class ChessManager : MonoBehaviour
	{
		[Range(1, 20)]
		[SerializeField] int depth = 10;
		// Example: Black to move in the starting position is "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1"
		[TextArea(minLines: 3, maxLines: 5)]
		[SerializeField] string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1";

		private async void Update()
		{
			if (INPUT.M.InstantDown(0))
			{
				string best = await StockfishEngine.SuggestBestMove(this.fen, this.depth);
				Debug.Log(best);

				Debug.Log(C_E.chess_coord("e2"));
				Debug.Log("A".fmatch(@"[a-g]", "gi"));
				Debug.Log(C_E.chess_move("e2e4").from +  " // " + C_E.chess_move("g7g5").to);
			}
		}

		#region ad
		private void OnApplicationQuit()
		{
			StockfishEngine.Quit();
		}
		#endregion
	}

	// a1, e5, g8 //
	public static class C_E
	{
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
			if(moves.Length != 2)
				return ((-1, -1), (-1, -1));
			return
			(
				chess_coord(moves[0]),
				chess_coord(moves[1])
			);
		}
	}
}
