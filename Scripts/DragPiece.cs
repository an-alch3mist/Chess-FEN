using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPACE_UTIL;

namespace SPACE_CHESS
{
	public class DragPiece : MonoBehaviour
	{
		static GameObject Ghost_obj_ref;

		private void Awake()
		{
			if (Ghost_obj_ref == null)
			{
				Ghost_obj_ref = GameObject.Instantiate(Resources.Load<GameObject>("ghost"), C.PrefabHolder);
				Ghost_obj_ref.SetActive(false);
			}
		}

		private void Update()
		{
			INPUT.M.up = Vector3.forward;
			v2 pos_I = C.round(INPUT.M.getPos3D);
			Vector3 pos_F = INPUT.M.getPos3D;

			if (this.need_to_move == true)
			{
				if (INPUT.M.HeldDown(0))
				{
					Ghost_obj_ref.transform.position = pos_I;
					this.transform.position = pos_F;
				}
				else // released instant
				{
					// LOG.SaveLog(ChessManager.Ins.getAvailablePos(C_E.get_chess_coord(release_v2)).ToTable());
					if (ChessManager.Ins.IsInAvailablePos(this.from_v2, pos_I) == true)
					{
						this.transform.position = pos_I;
						// make move white 
						string move = C_E.get_chess_coord(this.from_v2) + C_E.get_chess_coord(pos_I);
						ChessManager.Ins.MakeMove(move, 'w');

						// make move black (cpu)
						make_move_black();
					}
					else // released at same place or out of bounds
					{
						this.transform.position = this.from_v2;
					}
					Ghost_obj_ref.SetActive(false);
					this.need_to_move = false;
				}
			}
		}

		// TODO: v0.2
		// drag or capture only where its allowed for a piece
		// when checked, the incoming check should be either blocked by other piece or king should be moved, nothing else is allowed

		async void make_move_black()
		{
			await C.delay(500);
			//
			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			Debug.Log("calculating...");
			string best = await StockfishEngine.Ins.SuggestAtDepth(C_E.B_to_FEN(ChessManager.Ins.B));
			sw.Stop();
			Debug.Log("elapsed ms for best move: " + sw.ElapsedMilliseconds);

			string move = best.split(@"\n").gl(0).split(@" ")[1];
			 
			LOG.H("calculating" + '-'.repeat(100));
			LOG.SaveLog(C_E.B_to_str(ChessManager.Ins.B));
			LOG.SaveLog(C_E.B_to_FEN(ChessManager.Ins.B));
			LOG.SaveLog(best, $"move: {move}");
			LOG.HEnd("calculating" + '-'.repeat(100));

			Debug.Log("calculating" + "best move after split: " + move);

			ChessManager.Ins.MakeMove(move, 'b');
		}

		bool need_to_move = false;
		v2 from_v2;
		private void OnMouseDown()
		{
			Ghost_obj_ref.SetActive(true);
			this.need_to_move = true;

			this.from_v2 = this.transform.position;
		}
	}
}
