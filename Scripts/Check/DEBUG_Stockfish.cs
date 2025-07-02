using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPACE_UTIL;
using SPACE_Stockfish;

namespace SPACE_CHESS
{
	public class DEBUG_Stockfish : MonoBehaviour
	{
		private void Update()
		{
			if (INPUT.M.InstantDown(0))
			{
				StopAllCoroutines();
				StartCoroutine(STIMULATE());
			}
		}

		[TextArea(minLines: 3, maxLines: 5)]
		[SerializeField] string FEN_Arrangement = "k6R/7B/8/8/8/8/8/K7";
		[SerializeField] bool castle_Allowed_k = true,
							  castle_Allowed_q = true,
							  castle_Allowed_K = true,
							  castle_Allowed_Q = true;
		[SerializeField] char oppo_side = 'b';
		[SerializeField] v2 coord = (3, 7);

		IEnumerator STIMULATE()
		{
			#region frame_rate
			QualitySettings.vSyncCount = 1;
			yield return null;
			yield return null;
			#endregion

			//string FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b - - 0 1";
			string castling_str = "";
			if (this.castle_Allowed_K) castling_str += 'K'; 
			if (this.castle_Allowed_Q) castling_str += 'Q'; 
			if (this.castle_Allowed_k) castling_str += 'k'; 
			if (this.castle_Allowed_q) castling_str += 'q';
			
			string FEN = $"{this.FEN_Arrangement} {this.oppo_side} {castling_str} - 0 1";
			Debug.Log("FEN: " + FEN);

			//  "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1"
			// return $"{placement} {oppo_side} - - 0 1";

			Debug.Log("calculating...");
			yield return StockfishManager.Ins.SuggestAtDepthCoroutine(fen: FEN);
			Debug.Log("SuggestedMove: " + StockfishManager.Ins.SuggestedMove);
			Debug.Log("SquareUnderAttack: " + ChessManager_1.SquareUnderAttack(Ce.FEN_to_B(FEN), coord: this.coord, oppo_side: 'w'));

			yield return null;
		}
	}
}
