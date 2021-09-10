using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
namespace Tetris
{
	class Program
	{
		//Settings
		static int tetrisRows = 20;
		static int tetrisCols = 15;
		static int infoCols = 10;
		static List<bool[,]> TetrisFigures = new List<bool[,]>
		{
			new bool[,]
			{ //I
                { true,true,true,true },
			},
			new bool[,]
			{ //O
                { true, true },
				{ true, true }

			},
			new bool[,]
			{ //T
                { false,true,false },
				{ true,true,true }
			},
			new bool[,]
			{ //Z
                {true,true,false },
				{false,true,true }
			},
			new bool[,]
			{ //S
                {false,true,true },
				{true,true,false }
			},
			new bool[,]
			{ //J
                {true,false,false },
				{true,true,true }
			},
			new bool[,]
			{ //L
                { true,true,true},
				{ false,false,true }
			}
		};
		static int[] ScorePerLines = { 0, 40, 100, 300, 1200 };
		static int[] LevelSpeed = { 15, 12, 9, 7, 5, 3 }; //FramesToMoveFigure
		static string HighScoreFIleName = "highscore.txt";
		static int level = 0;
		//Game Info
		static int Score = 0;
		static int Frame = 0;
		static int FramesToMoveFigure = 15;
		static int CurrentFigureRow = 0;
		static int CurrentFigureCol = 0;
		static string Highscore = "";
		static bool[,] TetrisField = new bool[tetrisRows, tetrisCols];
		static bool[,] NextFigure = null;
		static bool[,] CurrentFigure = null;
		static Random Random = new Random();

		static void Main()
		{
			Console.Title = "Tetris";
			Console.WindowHeight = tetrisRows + 5;
			Console.WindowWidth = tetrisCols * 2 + 5;
			Console.BufferHeight = Console.WindowHeight;
			Console.BufferWidth = Console.WindowWidth;
			Console.CursorVisible = false;
			CurrentFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
			NextFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
			DrawMainMenu();
			Console.Clear();
			GetHighScore();
			while (true)
			{
				Frame++;
				//User Input
				if (Console.KeyAvailable)
				{
					var key = Console.ReadKey();
					if (key.Key == ConsoleKey.Escape)
					{
						TetrisField = new bool[tetrisRows, tetrisCols];
						CurrentFigure = NextFigure;
						NextFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
						Frame = 0;
						Score = 0;
						CurrentFigureRow = 0;
						CurrentFigureCol = 0;
						DrawMainMenu();
						Console.Clear();
					}
					if (key.Key == ConsoleKey.Spacebar || key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.W)
					{
						RotateCurrentFigure();
					}
					if (key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.A)
					{
						if (CurrentFigureCol >= 1)
						{
							CurrentFigureCol--;
						}
					}
					if (key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.D)
					{
						if (CurrentFigureCol < tetrisCols - CurrentFigure.GetLength(1))
						{
							CurrentFigureCol++;
						}
					}
					if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.S)
					{
						Frame = 1;
						Score++;
						CurrentFigureRow++;
					}
				}
				if (Frame % FramesToMoveFigure == 0)
				{
					Frame = 0;
					CurrentFigureRow++;
				}
				if (Collision(CurrentFigure))
				{
					AddCurrentFigureToTetrisField();
					int lines = CheckForFullLine();
					Score += ScorePerLines[lines] * level;
					level = ChangingLevel(Score);
					CurrentFigure = NextFigure;
					CurrentFigureRow = 0;
					CurrentFigureCol = 0;
					NextFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
					if (Collision(CurrentFigure))
					{ //TODO: col to be presented by tetrisCols/2 or something
						var scoreAsString = Score.ToString();
						scoreAsString += new string(' ', 12 - scoreAsString.Length);
						Write("╔════════════════╗", 7, 6, ConsoleColor.Red);
						Write("║   GAME OVER!   ║", 8, 6, ConsoleColor.Red);
						Write($"║    {scoreAsString}║", 9, 6, ConsoleColor.Red);
						Write("║                ║", 10, 6, ConsoleColor.Red);
						Write("╚════════════════╝", 11, 6, ConsoleColor.Red);
						if (Score > int.Parse(Highscore))
						{
							File.WriteAllText(HighScoreFIleName, Score.ToString());
						}
						while (true)
						{
							if (Console.KeyAvailable)
							{
								var key = Console.ReadKey();
								if (key.Key == ConsoleKey.Escape)  //Restart The GAME
								{
									TetrisField = new bool[tetrisRows, tetrisCols];
									Frame = 0;
									Score = 0;
									GetHighScore();
									DrawMainMenu();
									Console.Clear();
									break;
								}
							}
						}
					}
				}
				DrawBorder();
				DrawInfo();
				DrawTetrisField();
				DrawCurrentFigure();
				DrawNextFigure();
				Thread.Sleep(30);
			}
		}

		private static void GetHighScore()
		{
			if (File.Exists(HighScoreFIleName))
			{
				Highscore = File.ReadAllText(HighScoreFIleName);
			}
			else
			{
				Highscore = "0";
			}
		}
		static void Write(string text, int x, int y, ConsoleColor color)
		{
			Console.SetCursorPosition(y, x);
			Console.ForegroundColor = color;
			Console.Write(text);
			Console.ResetColor();
		}
		private static void RotateCurrentFigure()
		{
			var newFigure = new bool[CurrentFigure.GetLength(1), CurrentFigure.GetLength(0)];
			for (int row = 0; row < CurrentFigure.GetLength(0); row++)
			{
				for (int col = 0; col < CurrentFigure.GetLength(1); col++)
				{
					newFigure[col, CurrentFigure.GetLength(0) - row - 1] = CurrentFigure[row, col];
				}
			}
			if (!Collision(newFigure))
			{
				CurrentFigure = newFigure;
			}

		}
		static int CheckForFullLine() //0,1,2,3,4 //
		{
			int lines = 0;
			for (int row = 0; row < TetrisField.GetLength(0); row++)
			{
				bool rowIsFull = true;
				for (int col = 0; col < TetrisField.GetLength(1); col++)
				{
					if (TetrisField[row, col] == false)
					{
						rowIsFull = false;
						break;
					}
				}
				if (rowIsFull)
				{
					for (int rowTomMove = row; rowTomMove >= 1; rowTomMove--)
					{
						for (int col = 0; col < TetrisField.GetLength(1); col++)
						{
							TetrisField[rowTomMove, col] = TetrisField[rowTomMove - 1, col];

						}
					}
					lines++;
				}
			}
			return lines;
		}
		static void AddCurrentFigureToTetrisField()
		{
			for (int row = 0; row < CurrentFigure.GetLength(0); row++)
			{
				for (int col = 0; col < CurrentFigure.GetLength(1); col++)
				{
					if (CurrentFigure[row, col])
					{
						TetrisField[CurrentFigureRow + row, CurrentFigureCol + col] = true;
					}
				}
			}
		}
		static void DrawBorder()
		{
			Console.SetCursorPosition(0, 0);
			string firstLine = "╔";
			firstLine += new string('═', tetrisCols);
			firstLine += "╦";
			firstLine += new string('═', infoCols);
			firstLine += "╗";
			Console.WriteLine(firstLine);


			for (int i = 0; i < tetrisRows; i++)
			{
				string middleLine = "║";
				middleLine += new string(' ', tetrisCols);
				middleLine += "║";
				middleLine += new string(' ', infoCols);
				middleLine += "║";
				Console.WriteLine(middleLine);
			}
			string lastLine = "╚";
			lastLine += new string('═', tetrisCols);
			lastLine += "╩";
			lastLine += new string('═', infoCols);
			lastLine += "╝";
			Console.WriteLine(lastLine);
		}
		static void DrawTetrisField()
		{
			for (int row = 0; row < tetrisRows; row++)
			{
				for (int col = 0; col < tetrisCols; col++)
				{
					if (TetrisField[row, col])
					{
						Write("o", row + 1, col + 1, ConsoleColor.Yellow);
					}
				}
			}
		}
		static void DrawCurrentFigure()
		{

			for (int row = 0; row < CurrentFigure.GetLength(0); row++) //GetLength(0) vzima bori redove
			{
				for (int col = 0; col < CurrentFigure.GetLength(1); col++) //GetLength(1) vzima broi koloni
				{
					if (CurrentFigure[row, col])
					{
						Write("o", row + 1 + CurrentFigureRow, col + 1 + CurrentFigureCol, ConsoleColor.Green);
					}
					else
					{
						// Write(" ", row + 1 + CurrentFigureRow, col + 1 + CurrentFigureCol);
					}
				}
			}
		}
		static void DrawNextFigure()
		{

			for (int row = 0; row < NextFigure.GetLength(0); row++) //GetLength(0) vzima bori redove
			{
				for (int col = 0; col < NextFigure.GetLength(1); col++) //GetLength(1) vzima broi koloni
				{
					if (NextFigure[row, col])
					{
						Write("o", 8 + row, tetrisCols + 5 + col, ConsoleColor.DarkCyan);
					}
					else
					{
						Write(" ", 8 + row, tetrisCols + 5 + col, ConsoleColor.Yellow);
					}
				}
			}
		}
		static void DrawRandomFigure()
		{
			int n = Random.Next(0, TetrisFigures.Count);
			bool[,] randomFigure = TetrisFigures[n];
			ConsoleColor randomColor = (ConsoleColor)(new Random().Next(Enum.GetNames(typeof(ConsoleColor)).Length));

			for (int row = 0; row < randomFigure.GetLength(0); row++) //GetLength(0) vzima bori redove
			{
				for (int col = 0; col < randomFigure.GetLength(1); col++) //GetLength(1) vzima broi koloni
				{
					if (randomFigure[row, col])
					{
						Write("o", 6 + row, 15 + col, randomColor);
					}
					else
					{
						Write(" ", 6 + row, 15 + col, randomColor);
					}
				}
			}
		}
		static void DrawInfo()
		{
			Write("HighScore:", 1, tetrisCols + 2, ConsoleColor.Yellow);
			Write(Highscore, 2, tetrisCols + 2, ConsoleColor.Yellow);
			Write("Score:", 4, tetrisCols + 2, ConsoleColor.Red);
			Write(Score.ToString(), 5, tetrisCols + 2, ConsoleColor.Red);
			Write("Next:", 7, tetrisCols + 2, ConsoleColor.DarkCyan);
			Write("Keys:", 11, tetrisCols + 2, ConsoleColor.DarkYellow);
			Write("    ^", 12, tetrisCols + 2, ConsoleColor.DarkYellow);
			Write("  < v >", 13, tetrisCols + 2, ConsoleColor.DarkYellow);
			Write($"Level: {level}", 15, tetrisCols + 2, ConsoleColor.White);
		}
		static void MainMenuInfo()
		{
			Console.Clear();
			DrawRandomFigure();
			Write("Select Level: 1 2 3 4 5 6", 9, 5, ConsoleColor.Yellow);
			Write("Then Press ENTER To START", 10, 5, ConsoleColor.Yellow);
			Console.WriteLine();
			Console.SetCursorPosition(15, 12);
			Thread.Sleep(100);
		}
		static void DrawMainMenu()
		{
		Start:
			while (true)
			{
				MainMenuInfo();
				if (Console.KeyAvailable)
				{
					level = int.Parse(Console.ReadLine());
					try
					{
						FramesToMoveFigure = LevelSpeed[level];
						break;
					}
					catch
					{
						Console.WriteLine("Invalid Level! Try Again.");
						goto Start;
					}
				}
			}
		}
		static bool Collision(bool[,] figure)
		{

			if (CurrentFigureCol > tetrisCols - figure.GetLength(1))
			{
				return true;
			}
			if (CurrentFigureRow + figure.GetLength(0) == tetrisRows)
			{
				return true;
			}
			else
			{
				for (int row = 0; row < figure.GetLength(0); row++)
				{
					for (int col = 0; col < figure.GetLength(1); col++)
					{
						if (figure[row, col] &&
							TetrisField[CurrentFigureRow + row + 1, CurrentFigureCol + col])
						{
							return true;
						}
					}
				}
			}

			return false;
		}
		static int ChangingLevel(int Score)
		{
			if (level < 6)
			{
				if (Score >= 550 && Score < 1100)
				{
					level++;
				}
				if (Score >= 1100 && Score < 1650)
				{
					level++;
				}
				if (Score >= 1650 && Score < 2200)
				{
					level++;
				}
				if (Score >= 2200 && Score < 2750)
				{
					level++;
				}
				if (Score >= 3500)
				{
					level++;
				}
			}
			FramesToMoveFigure = LevelSpeed[level-1];
			return level;
		} //TODO: FIX
	}
}
