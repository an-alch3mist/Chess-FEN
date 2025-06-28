using System;
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
*/
public class StockfishEngine : MonoBehaviour
{
	public static StockfishEngine Ins { get; private set; }

	[Header("Engine Settings")]
	[Header("Depth Based Search")]
	[Range(1, 20)]
	public int defaultDepth = 10;
	[Range(16, 256)]
	public int hashSizeMB = 64;

	[Header("Time Based Search also require hashMB" +
			"")]
	[Range(50, 5000)]
	public int defaultTimeMs = 200;


	[Range(1, 8)]
	public int threads = 1;

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
		//
		UnityEngine.Debug.Log("Awake(): " + this);
	}

	private async void Start()
	{
	}

	/// <summary>
	/// Initialize the Stockfish engine with optimized settings
	/// </summary>
	private async Task InitializeEngine()
	{
		try
		{
			// UnityEngine.Debug.Log("Starting Stockfish engine...");

			// 1) Launch process
			string exe = C_E.loc_stockfish;
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

			// 2) UCI handshake
			_input.WriteLine("uci");
			await WaitForKeyword("uciok");

			// 3) Configure engine for optimal performance
			_input.WriteLine($"setoption name Threads value {threads}");
			_input.WriteLine($"setoption name Hash value {hashSizeMB}");

			// Optional: Disable some time-consuming features for faster play
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
	/// <para>Gets a suggestion at a specified depth search.</para>
	/// <para> <param name="fen">FEN notaion,</param> example: "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1"</para>
	/// <para><param name="depth">The search depth. Defaults to 0, which uses the default depth.</param></para>
	/// <para><returns>returns: a string result(A task representing the asynchronous operation, )</returns></para>
	/// </summary>
	public async Task<string> SuggestAtDepth(string fen, int depth = 0)
	{
		if (depth == 0) depth = defaultDepth;
		return await SearchPosition(fen, $"go depth {depth}");
	}

	/// <summary>
	/// Get best move with time-based search (faster for real-time play)
	/// </summary>
	public async Task<string> SuggestAtDuration(string fen, int timeMs = -1)
	{
		if (timeMs == -1) timeMs = defaultTimeMs;
		return await SearchPosition(fen, $"go movetime {timeMs}");
	}

	/// <summary>
	/// Core search method
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
			// Send position and search commands
			_input.WriteLine($"position fen {fen}");
			_input.WriteLine(goCommand);

			// Read response until we get the bestmove
			var response = new System.Text.StringBuilder();
			string bestMove = "";

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
					bestMove = line;
					break;
				}
			}

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

	/// <summary>
	/// Stop the current search (if any)
	/// </summary>
	public void StopSearch()
	{
		if (_isEngineRunning && _inProgress)
		{
			_input.WriteLine("stop");
		}
	}

	/// <summary>
	/// Check if engine is currently thinking
	/// </summary>
	public bool IsThinking()
	{
		return _inProgress;
	}

	// quit engine
	private void QuitEngine()
	{
		if (_engine != null)
		{
			try
			{
				// Check if the process is still valid before accessing HasExited
				if (!_engine.HasExited)
				{
					_input?.WriteLine("quit");
					_engine.WaitForExit(1000);
				}
			}
			catch (InvalidOperationException)
			{
				// Process was already disposed or never started properly
				UnityEngine.Debug.Log("Process was already disposed or never started");
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogWarning($"Error closing engine: {ex.Message}");
			}
			finally
			{
				// Always dispose of the process object
				try
				{
					_engine.Close();
					_engine.Dispose();
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogWarning($"Error disposing engine: {ex.Message}");
				}
				_engine = null;
			}

			UnityEngine.Debug.Log("Stockfish engine closed");
		}

		_isEngineRunning = false;
		_input = null;
		_output = null;
	}
	private void OnDestroy()
	{
		QuitEngine();
	}
	private void OnApplicationQuit()
	{
		QuitEngine();
	}
	// quit engine
}

public static class StockEngineUtil
{
	/// <summary>
	/// Get engine evaluation score from response
	/// </summary>
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
						score = centipawns / 100f; // Convert centipawns to pawns
						break;
					}
				}
			}
		}

		return score;
	}

}

}