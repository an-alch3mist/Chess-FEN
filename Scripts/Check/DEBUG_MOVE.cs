using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SPACE_CHESS
{
	public class DEBUG_MOVE : MonoBehaviour
	{
		[SerializeField] float speed = 1f;
		private void Update()
		{
			float dt = Time.deltaTime;

			Vector3 vel = Vector3.right * speed;
			transform.position += vel * dt; 
		}
	}
}
