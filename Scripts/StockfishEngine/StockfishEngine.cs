using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace SPACE_CHESS
{
	/*
        Usage:
        - Add this script to a Empty GameObject(name: StockfishEngineManager ) in your scene
        - Use StockfishEngine.Ins.SuggestAtDepth(string fen, int depth(optional)) to get moves
        - Use StockfishEngine.Ins.SuggestAtDepthCoroutine(string fen, int depth(optional)) in a Coroutine to yield until ready
        - After coroutine finishes, read StockfishEngine.Ins.SuggestedMove for the best move
    */
	public class StockfishEngine : MonoBehaviour
	{
		public static StockfishEngine Ins { get; private set; }

		[Header("loc_file")]
		[Tooltip("/loc_file.exe")]
		[SerializeField] string win_path_inside_streamingAssets = "/stockfish-windows-x86-64-avx2.exe";

		[Header("config")]
		[Header("Engine Settings")]
		[Header("Depth Based Search")]
		[Range(1, 20)]
		[SerializeField] int defaultDepth = 5;
		[Range(16, 128)]
		[SerializeField] int hashSizeMB = 64;

		[Header("Time Based Search also require hashMB")]
		[Range(50, 5000)]
		[SerializeField] int defaultTimeMs = 200;

		[Range(1, 8)]
		[SerializeField] int threads = 1;

		// Stores the last suggested move (algebraic notation), updated after any search
		public string SuggestedMove { get; private set; }

		private Process _engine;
		private StreamWriter _input;
		private StreamReader _output;
		private bool _uciInitialized = false;
		private bool _isEngineRunning = false;
		private bool _inProgress = false;

		private async void Awake()
		{
			await InitializeEngine();

			// Singleton pattern
			if (Ins == null)
			{
				Ins = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
				return;
			}
			UnityEngine.Debug.Log("Awake(): " + this);
		}

		/// <summary>
		/// Initialize the Stockfish engine with optimized settings
		/// </summary>
		private async Task InitializeEngine()
		{
			try
			{
				string exe = Application.streamingAssetsPath + this.win_path_inside_streamingAssets;
				if (!File.Exists(exe))
				{
					UnityEngine.Debug.LogError($"Stockfish executable not found at: {exe}");
					return;
				}

				_engine = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = exe,
						UseShellExecute = false,
						RedirectStandardInput = true,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						CreateNoWindow = true
					}
				};

				_engine.Start();
				_input = _engine.StandardInput;
				_output = _engine.StandardOutput;
				_isEngineRunning = true;

				// UCI handshake
				_input.WriteLine("uci");
				await WaitForKeyword("uciok");

				// Configure performance
				_input.WriteLine($"setoption name Threads value {threads}");
				_input.WriteLine($"setoption name Hash value {hashSizeMB}");
				_input.WriteLine("setoption name Ponder value false");

				_input.WriteLine("isready");
				await WaitForKeyword("readyok");

				_uciInitialized = true;
				UnityEngine.Debug.Log("Stockfish engine initialized successfully!");
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError($"Failed to initialize Stockfish: {ex.Message}");
			}
		}

		/// <summary>
		/// Gets a suggestion at a specified depth search asynchronously.
		/// </summary>
		public async Task<string> SuggestAtDepth(string fen, int depth = 0)
		{
			if (depth == 0) depth = defaultDepth;
			return await SearchPosition(fen, $"go depth {depth}");
		}

		/// <summary>
		/// Coroutine wrapper to yield until the engine provides a move.
		/// After yielding, read StockfishEngine.Ins.SuggestedMove to get the best move.
		/// </summary>
		public IEnumerator SuggestAtDepthCoroutine(string fen, int depth = 0)
		{
			// Kick off the async search
			var task = SuggestAtDepth(fen, depth);
			// Wait until task completes
			while (!task.IsCompleted)
				yield return null;
			// Task is complete, SuggestedMove has been updated
		}

		/// <summary>
		/// Core search method; sends commands and reads 'bestmove'.
		/// </summary>
		private async Task<string> SearchPosition(string fen, string goCommand)
		{
			if (_inProgress)
			{
				UnityEngine.Debug.LogWarning("Engine is busy, please wait...");
				return "Engine busy";
			}

			if (!_isEngineRunning || !_uciInitialized)
			{
				UnityEngine.Debug.LogError("Engine not initialized!");
				return "Engine not ready";
			}

			_inProgress = true;
			try
			{
				// Send position and start search
				_input.WriteLine($"position fen {fen}");
				_input.WriteLine(goCommand);

				var response = new System.Text.StringBuilder();
				string bestMoveLine = string.Empty;

				while (true)
				{
					string line = await _output.ReadLineAsync();
					if (line == null)
					{
						UnityEngine.Debug.LogError("Engine closed unexpectedly");
						break;
					}

					response.AppendLine(line);

					if (line.StartsWith("bestmove", StringComparison.Ordinal))
					{
						bestMoveLine = line;
						break;
					}
				}

				// Parse and store the move (e.g. "bestmove e2e4" -> "e2e4")
				var parts = bestMoveLine.Split(' ');
				SuggestedMove = parts.Length >= 2 ? parts[1] : string.Empty;

				return response.ToString();
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError($"Error during search: {ex.Message}");
				return $"Error: {ex.Message}";
			}
			finally
			{
				_inProgress = false;
			}
		}

		private async Task WaitForKeyword(string keyword)
		{
			string line;
			while ((line = await _output.ReadLineAsync()) != null)
			{
				if (line.Contains(keyword))
					return;
			}
		}

		public void StopSearch()
		{
			if (_isEngineRunning && _inProgress)
			{
				_input.WriteLine("stop");
			}
		}

		public bool IsThinking()
		{
			return _inProgress;
		}

		private void QuitEngine()
		{
			if (_engine != null)
			{
				try
				{
					if (!_engine.HasExited)
					{
						_input?.WriteLine("quit");
						_engine.WaitForExit(1000);
					}
				}
				catch (InvalidOperationException)
				{
					UnityEngine.Debug.Log("Process was already disposed or never started");
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogWarning($"Error closing engine: {ex.Message}");
				}
				finally
				{
					try { _engine.Close(); _engine.Dispose(); } catch { }
					_engine = null;
				}

				UnityEngine.Debug.Log("Stockfish engine closed");
			}

			_isEngineRunning = false;
			_input = null;
			_output = null;
		}

		private void OnDestroy() => QuitEngine();
		private void OnApplicationQuit() => QuitEngine();
	}

	public static class StockEngineUtil
	{
		public static float GetEvaluationScore(string engineResponse)
		{
			string[] lines = engineResponse.Split('\n');
			float score = 0f;

			foreach (string line in lines)
			{
				if (line.Contains("score cp"))
				{
					string[] parts = line.Split(' ');
					for (int i = 0; i < parts.Length - 1; i++)
					{
						if (parts[i] == "cp" && float.TryParse(parts[i + 1], out float centipawns))
						{
							score = centipawns / 100f;
							break;
						}
					}
				}
			}

			return score;
		}
	}
}
