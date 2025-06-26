using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class StockfishConnector
{
	private static Process _engine;
	private static StreamWriter _input;
	private static StreamReader _output;
	private static bool _uciInitialized = false;

	/// <summary>
	/// Initialize the engine once (launch + UCI handshake).
	/// </summary>
	private static async Task EnsureEngineStarted()
	{
		if (_engine != null && !_engine.HasExited)
			return;

		// 1) Launch
		string exe = Path.Combine(Application.streamingAssetsPath, "Stockfish/stockfish-windows-x86-64-avx2");
		_engine = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = exe,
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				CreateNoWindow = true
			}
		};
		_engine.Start();
		_input = _engine.StandardInput;
		_output = _engine.StandardOutput;

		// 2) UCI handshake
		if (!_uciInitialized)
		{
			_input.WriteLine("uci");
			await WaitForKeyword("uciok");

			_input.WriteLine("isready");
			await WaitForKeyword("readyok");

			_uciInitialized = true;
		}
	}

	/// <summary>
	/// Asks Stockfish for the best move from the given FEN at the specified depth.
	/// </summary>
	/// <param name="fen">FEN string describing the position (must include side-to-move)</param>
	/// <param name="depth">Search depth (e.g. 10)</param>
	/// <returns>UCI bestmove token (e.g. “e7e5”)</returns>
	/// 
	static bool InProgress = false;
	public static async Task<string> SuggestBestMove(string fen, int depth)
	{
		if (InProgress == true)
			return "still in progress";

		InProgress = true;
		await EnsureEngineStarted();

		// Tell the engine the position
		_input.WriteLine($"position fen {fen}");

		// Start search
		_input.WriteLine($"go depth {depth}");

		// Read until bestmove appears
		
		while (true)
		{
			string line = await _output.ReadLineAsync();
			if (line == null) break;

			if (line.StartsWith("bestmove"))
			{
				// format: “bestmove e7e5 ponder a7a6”
				var parts = line.Split(' ');
				string best = parts.Length > 1 ? parts[1] : string.Empty;
				InProgress = false;
				return best;
			}
		}

		InProgress = false;
		UnityEngine.Debug.LogWarning("[Stockfish] No bestmove received.");
		return string.Empty;
	}

	private static async Task WaitForKeyword(string kw)
	{
		string line;
		while ((line = await _output.ReadLineAsync()) != null)
		{
			if (line.Contains(kw))
				return;
		}
	}

	public static void Quit()
	{
		if (_engine != null && !_engine.HasExited)
		{
			_input.WriteLine("quit");
			_engine.WaitForExit(200);
			_engine.Close();
		}
	}
}
