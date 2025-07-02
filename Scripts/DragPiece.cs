using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPACE_UTIL;
using System.Threading.Tasks;

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
			if (this.CanMoveUnit == false)
				return;

			INPUT.M.up = Vector3.forward; v2 pos_I = C.round(INPUT.M.getPos3D); Vector3 pos_F = INPUT.M.getPos3D;
			if (INPUT.M.HeldDown(0) == true) // still held down
			{
				#region ghost
				Ghost_obj_ref.transform.position = pos_I;
				#endregion
				this.transform.position = pos_F;
			}
			else // released instant
			{
				// LOG.SaveLog(ChessManager.Ins.getAvailablePos(C_E.get_chess_coord(release_v2)).ToTable());
				if (ChessManager_1.IsAllowed(from_coord, to_coord: pos_I, king_side: 'w') && this.InProgress == false) // released at valid coord ?
				{
					ChessManager_1.MakeMoveOnBoard(from_coord, to_coord: pos_I); // could be 'w' or 'b'
					#region reach
					ChessManager_1.ShowReach(false, (0, 0));
					#endregion
					MakeMoveOnBoard_Async_Cpu(pos_I);
				}
				else // released at same place or out of bounds
				{
					this.transform.position = this.from_coord; // snap back to original from_coord
				}
				#region Ghost
				Ghost_obj_ref.SetActive(false);
				#endregion
				this.CanMoveUnit = false;
			}

		}

		async void MakeMoveOnBoard_Async_Cpu(v2 pos_I)
		{
			this.InProgress = true;
			//
			await C.delay(500);
			await ChessManager_1.MakeCpuMoveOnBoard(cpu_side: 'b'); // async
			#region CheckForKingFall
			if (ChessManager_1.CheckForKingFall('w')) Debug.Log($"{'w'} wins");
			if (ChessManager_1.CheckForKingFall('b')) Debug.Log($"{'b'} wins");
			#endregion
			//
			this.InProgress = false;
			#region reach
			ChessManager_1.ShowReach(false, (0, 0)); // hide everything
			ChessManager_1.ShowReach(true, this.from_coord); // show reach for this from_coord
			#endregion
		}

		bool InProgress = false;
		bool CanMoveUnit = false;
		v2 from_coord;
		private void OnMouseDown() // works: even when component disabled
		{
			from_coord = C.round(INPUT.M.getPos3D);
			this.CanMoveUnit = true;
			#region reach
			ChessManager_1.ShowReach(false, (0, 0)); // hide everything
			ChessManager_1.ShowReach(true, this.from_coord); // show reach for this from_coord
			#endregion
		}
	}
}