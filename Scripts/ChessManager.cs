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
			/*
			if (INPUT.M.InstantDown(0))
				ASYNC();
			*/

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

			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			Debug.Log("calculating...");
			string best = await StockfishEngine.Ins.SuggestAtDepth(this.fen);
			sw.Stop();
			Debug.Log("elapsed ms for best move: " + sw.ElapsedMilliseconds);
			LOG.SaveLog(best, best.split(@"\n").gl(0).split(@" ")[1]);
			Debug.Log("best move after split: " + best.split(@"\n").gl(0).split(@" ")[1]);
		}

		[SerializeField] GameObject[] PIECE;
		private void Start()
		{
			this.Gather();
		}

		// called externally
		public void MakeMove(string move = "e2e4", char ad_turn = 'w')
		{
			(v2 from, v2 to) = C_E.get_delta_coord(move);
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

			Debug.Log(C_E.B_to_str(B));

			LOG.H(ad_turn + '-'.repeat(100));
			LOG.SaveLog(C_E.B_to_str(B));
			LOG.SaveLog(C_E.B_to_FEN(B));
			LOG.HEnd(ad_turn + '-'.repeat(100));
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

			B = C_E.FEN_to_B();
			B_OBJ = C_E.B_to_OBJ(B, MAP_type_prefab);

			/* checked
			foreach(var line in B)
				Debug.Log(line.map(_char => _char.ToString()).join(", "));
			Debug.Log(C_E.B_to_FEN_arrangement(B));
			*/
			// << parse IN here
		}
	}
}
