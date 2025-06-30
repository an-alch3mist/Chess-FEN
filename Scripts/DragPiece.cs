using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPACE_UTIL;

namespace SPACE_CHESS
{
	public class DragPiece : MonoBehaviour
	{
		#region Ghost
		static GameObject Ghost_obj_ref;
		#endregion
		private void Awake()
		{
			#region Ghost
			if (Ghost_obj_ref == null)
			{
				Ghost_obj_ref = GameObject.Instantiate(Resources.Load<GameObject>("pf_ghost"), C.PrefabHolder);
				Ghost_obj_ref.SetActive(false);
			}
			#endregion
		}

		private void Update()
		{
			INPUT.M.up = Vector3.forward;
			v2 pos_I = C.round(INPUT.M.getPos3D);
			Vector3 pos_F = INPUT.M.getPos3D;

			if (this.need_to_move == true)
			{
				if (INPUT.M.HeldDown(0) == true) // still held down
				{
					#region Ghost
					Ghost_obj_ref.transform.position = pos_I;
					#endregion
					this.transform.position = pos_F;
				}
				else // released instant
				{
					// LOG.SaveLog(ChessManager.Ins.getAvailablePos(C_E.get_chess_coord(release_v2)).ToTable());
					if (ChessManager_1.IsAllowed(from_coord, to_coord: pos_I)) // released at valid coord ?
					{
						ChessManager_1.MakeMoveOnBoard(from_coord, to_coord: pos_I);
						#region reach
						ChessManager_1.ShowReach(false, (0, 0));
						#endregion

						// make move black (cpu)
						ChessManager_1.make_move_oppo(oppo_side: 'b'); // async
					}
					else // released at same place or out of bounds
					{
						this.transform.position = this.from_coord; // snap back to original from_coord
					}
					#region Ghost
					Ghost_obj_ref.SetActive(false);
					#endregion
					this.need_to_move = false;
				}
			}
		}


		

		bool need_to_move = false;
		v2 from_coord;
		private void OnMouseDown() // works: even when component disabled
		{
			#region Ghost
			if (this.isActiveAndEnabled == true)
				Ghost_obj_ref.SetActive(true);
			#endregion
			this.need_to_move = true;
			this.from_coord = this.transform.position;
			#region reach
			ChessManager_1.ShowReach(false, (0, 0)); // hide everything
			ChessManager_1.ShowReach(true, transform.position);
			#endregion
		}
	}
}