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
			if (this.need_to_move == true)
			{
				if (INPUT.M.HeldDown(0))
				{
					Ghost_obj_ref.transform.position = this.get_round_clamp_tr_pos();
					INPUT.M.up = Vector3.forward;
					this.transform.position = INPUT.M.getPos3D;
				}
				else // released instant
				{
					if( C.inrange(this.transform.position , new Vector3(-0.5f, -0.5f, 0f), new Vector3(+7.5f, +7.5f, 0f))
						&& !C.zero(C.round(this.transform.position) - C.round(this.from_vec3)))
					{
						this.transform.position = this.get_round_clamp_tr_pos();
						// make move white 
						string move = C_E.get_chess_coord(this.from_vec3) + C_E.get_chess_coord(this.transform.position);
						ChessManager.Ins.MakeMove(move, 'w');

						// make move black (cpu)
						make_move_black();
					}
					else // released at same place or out of bounds
					{
						this.transform.position = C.round(this.from_vec3);
					}
					Ghost_obj_ref.SetActive(false);
					this.need_to_move = false;
				}
			}
		}

		async void make_move_black()
		{
			// await C.delay(2000);
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


		Vector3 get_round_clamp_tr_pos()
		{
			return C.round(C.clamp(this.transform.position, C_E.bound.m, C_E.bound.M)); ;
		}

		bool need_to_move = false;
		Vector3 from_vec3;
		private void OnMouseDown()
		{
			Ghost_obj_ref.SetActive(true);
			this.need_to_move = true;

			this.from_vec3 = this.transform.position;
		}
	}
}
