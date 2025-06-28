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

		private void Update()
		{
			if (INPUT.M.InstantDown(0))
				ASYNC();

			// UIToolTip >>
			UIToolTip.Ins.Show();
			UIToolTip.Ins.SetText(((v2)INPUT.M.getPos3D).chess_coord());
			// << UIToolTip
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


		[SerializeField] GameObject[] PIECE;
		private void Start()
		{
			this.Gather();
			LOG.SaveLog(this.MAP_objtype.ToTable(name: "MAP_obj vs type<> "));
		}


		Dictionary<GameObject, char> MAP_objtype;
		List<List<char>> B;
		string IN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1";




		void Gather()
		{
			// parse IN here >>
			MAP_objtype = new Dictionary<GameObject, char>();
			foreach (GameObject piece in this.PIECE)
			{
				string color = piece.name.split(@" ")[0];
				string type = piece.name.split(@" ")[1];

				MAP_objtype[piece] = type.tochar();
			}

			B = new List<List<char>>();
			for(int y = 0; y < 8; y += 1)
			{
				B.Add(new List<char>());
				for (int x = 0; x < 8; x += 1)
					B[y].Add(' ');
			}
			// << parse IN here
		}

	}
}
