using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SPACE_UTIL;

public class DragPiece : MonoBehaviour
{
	[SerializeField] GameObject Ghost_obj_ref;

	private void Update()
	{
		if (this.need_to_move == true)
		{
			if (INPUT.M.HeldDown(0))
			{
				this.Ghost_obj_ref.transform.position = this.get_walkable_pos();
				INPUT.M.up = Vector3.forward;
				this.transform.position = INPUT.M.getPos3D;
			}
			else
			{
				this.transform.position = this.get_walkable_pos();
				this.Ghost_obj_ref.SetActive(false);
				this.need_to_move = false;
			}
		}
	}

	Vector3 get_walkable_pos()
	{
		return C.round(C.clamp(this.transform.position, C_E.bound.m, C_E.bound.M)); ;
	}


	bool need_to_move = false;
	private void OnMouseDown()
	{
		this.Ghost_obj_ref.SetActive(true);
		this.need_to_move = true;
	}
}
