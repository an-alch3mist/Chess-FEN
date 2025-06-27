using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;


/*
	used as: 
		string best = await StockfishEngine.SuggestBestMove(
			fen: "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1",
			depth: 10
		);
*/
public class StockfishEngine
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
		string exe = C_E.loc_stockfish;
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

	static bool InProgress = false;
	public static async Task<string> SuggestBestMove(string fen, int depth)
	{
		if (InProgress)
			return "still in progress";

		InProgress = true;
		try
		{
			await EnsureEngineStarted().ConfigureAwait(false);

			// send the position & search commands
			_input.WriteLine($"position fen {fen}");
			_input.WriteLine($"go depth {depth}");

			// read everything up through the "bestmove" line
			var sb = new System.Text.StringBuilder();
			while (true)
			{
				string line = await _output.ReadLineAsync().ConfigureAwait(false);
				if (line == null)
					break;   // engine closed unexpectedly

				sb.AppendLine(line);

				if (line.StartsWith("bestmove", StringComparison.Ordinal))
					break;   // we have the full search transcript
			}

			return sb.ToString();
		}
		finally
		{
			InProgress = false;
		}
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
