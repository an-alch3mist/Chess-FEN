using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using SPACE_UTIL;

namespace SPACE_CHESS
{
	public class ChessManager : MonoBehaviour
	{
		// Example: Black to move in the starting position is "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1"
		[TextArea(minLines: 3, maxLines: 5)]
		[SerializeField] string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1";

		private async void Update()
		{
			if (INPUT.M.InstantDown(0))
				ASYNC();
		}

		async void ASYNC()
		{
			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			Debug.Log("calculating...");
			string best = await StockfishEngine.Ins.SuggestAtDepth(this.fen);
			sw.Stop();
			Debug.Log("elapsed milliseconds: " + sw.ElapsedMilliseconds);
			LOG.SaveLog(best);

			/* // checked
			Debug.Log(C_E.chess_coord("e2"));
			Debug.Log("A".fmatch(@"[a-g]", "gi"));
			Debug.Log(C_E.chess_move("e2e4").from + " // " + C_E.chess_move("g7g5").to);
			*/
		}
	}
}
