using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FiveChesses
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public enum PlayerFlag
		{
			None=0,
			Red=1,
			Blue=2
		};

		public struct Point2DInt32{
			public int X;
			public int Y;

			public Point2DInt32(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}
		}

		private int Rule_WinChess = 5;
		private int chessBoard_Columns = 19;
		private int chessBoard_Rows = 19;

		private SolidColorBrush solidColorBrush_Red;
		private SolidColorBrush solidColorBrush_Blue;
		private SolidColorBrush solidColorBrush_Gery;

		private Button[,] buttons;
		private PlayerFlag currentPlayer;
		private PlayerFlag CurrentPlayer
		{
			get { return currentPlayer; }
			set
			{
				currentPlayer = value;

				string playerName = ToString(currentPlayer);
				this.label_CurrentPlayer.Content = $"当前玩家：{playerName}";
			}
		}
		private PlayerFlag[,] playerFlags;
		private List<Point2DInt32> history;

		public static string ToString(PlayerFlag player)
		{
			switch (player)
			{
				case PlayerFlag.Red:
					return "红方";
				case PlayerFlag.Blue:
					return "蓝方";
				default:
					return "?";
			}
		}

		public MainWindow()
		{
			int i, j;

			InitializeComponent();

			solidColorBrush_Red = new SolidColorBrush(Color.FromRgb(222, 63, 79));
			solidColorBrush_Blue = new SolidColorBrush(Color.FromRgb(90, 63, 222));
			solidColorBrush_Gery = new SolidColorBrush(Color.FromRgb(247, 196, 41));

			this.BeginInit();
			this.Width = 512;
			this.Height = 512;
			for (i = 0; i < chessBoard_Columns; i++)
			{
				this.ChessBoard.ColumnDefinitions.Add(new ColumnDefinition()
				{
					Width = new GridLength(1, GridUnitType.Star)
				});
			}
			for (i = 0; i < chessBoard_Rows; i++)
			{
				this.ChessBoard.RowDefinitions.Add(new RowDefinition()
				{
					Height = new GridLength(1, GridUnitType.Star)
				});
			}
			buttons = new Button[chessBoard_Columns, chessBoard_Rows];
			for (i = 0; i < chessBoard_Columns; i++)
			{
				for (j = 0; j < chessBoard_Rows; j++)
				{
					Button button = new Button();
					long coordinate = (((long)i) << 32) | (long)j;
					button.Name = $"Button_{coordinate}";
					button.BeginInit();
					this.ChessBoard.Children.Add(button);
					button.SetValue(Grid.ColumnProperty, i);
					button.SetValue(Grid.RowProperty, j);
					button.Click += Button_Click;
					button.EndInit();
					buttons[i, j] = button;
				}
			}
			this.EndInit();

			playerFlags = new PlayerFlag[chessBoard_Columns, chessBoard_Rows];
			history = new List<Point2DInt32>();

			ResetGame();
		}

		public void ResetGame()
		{
			int i, j;

			for (i = 0; i < chessBoard_Columns; i++)
			{
				for (j = 0; j < chessBoard_Rows; j++)
				{
					Button button = buttons[i, j];

					button.BeginInit();
					button.Background = solidColorBrush_Gery;
					button.EndInit();

					playerFlags[i, j] = PlayerFlag.None;
				}
			}

			this.CurrentPlayer = PlayerFlag.Red;
			this.isWin = false;
			history.Clear();
		}

		private void Button_Click(object sender, EventArgs args)
		{
			if (isWin)
			{
				return;
			}

			Button button = (Button)sender;
			int x, y;

			string coordinate_str = new string(
				button.Name
				.SkipWhile((char ch) => { return ch != '_'; })
				.Skip(1)
				.ToArray());
			long coordinate = Convert.ToInt64(coordinate_str);
			x = (int)(coordinate >> 32);
			y = (int)(coordinate & 0xFFFF_FFFF);

			if (playerFlags[x, y] == PlayerFlag.None)
			{
				SolidColorBrush solidColorBrush;
				PlayerFlag nextPlayer;
				switch (currentPlayer)
				{
					case PlayerFlag.Red:
						nextPlayer = PlayerFlag.Blue;
						solidColorBrush = solidColorBrush_Red;
						break;
					case PlayerFlag.Blue:
						nextPlayer = PlayerFlag.Red;
						solidColorBrush = solidColorBrush_Blue;
						break;
					default:
						throw new Exception("Unknown error.");
				}

				button.BeginInit();
				button.Background = solidColorBrush;
				button.EndInit();

				playerFlags[x, y] = currentPlayer;
				history.Add(new Point2DInt32(x, y));

				bool isWin = CheckWin(history.Last(), currentPlayer);
				
				if (isWin)
				{
					this.isWin = true;

					this.label_CurrentPlayer.Content = $"{ToString(currentPlayer)}赢了！";

					this.currentPlayer = nextPlayer;
				}
				else
				{
					this.CurrentPlayer = nextPlayer;
				}
			}
		}

		private void button_Reset_Click(object sender, RoutedEventArgs e)
		{
			ResetGame();
		}

		private void button_Back_Click(object sender, RoutedEventArgs e)
		{
			BackSet();
		}

		public void BackSet()
		{
			if (history.Count > 0)
			{
				Point2DInt32 point = history.Last();
				playerFlags[point.X, point.Y] = PlayerFlag.None;
				history.RemoveAt(history.Count - 1);
				Button button = buttons[point.X, point.Y];

				button.BeginInit();
				button.Background = solidColorBrush_Gery;
				button.EndInit();

				PlayerFlag lastPlayer;
				switch (currentPlayer)
				{
					case PlayerFlag.Red:
						lastPlayer = PlayerFlag.Blue;
						break;
					case PlayerFlag.Blue:
						lastPlayer = PlayerFlag.Red;
						break;
					default:
						throw new Exception("Unknown error.");
				}
				this.CurrentPlayer = lastPlayer;

				this.isWin = false;
			}
		}

		private bool isWin;
		private bool CheckWin(Point2DInt32 lastChess, PlayerFlag lastPlayer)
		{
			if (playerFlags[lastChess.X, lastChess.Y] != lastPlayer)
			{
				throw new Exception("Unknown error.");
			}

			return
				CheckLine(1, 0, lastPlayer, lastChess) ||
				CheckLine(0, 1, lastPlayer, lastChess) ||
				CheckLine(1, 1, lastPlayer, lastChess) ||
				CheckLine(1, -1, lastPlayer, lastChess);
		}

		private bool CheckLine(int dx, int dy, PlayerFlag player, Point2DInt32 origin)
		{
			int count = 0;
			int x = origin.X, y = origin.Y;

			for(;(0<=x&&x<chessBoard_Columns)&&(0 <= y && y < chessBoard_Rows); x += dx, y += dy)
			{
				if (playerFlags[x, y] == player)
				{
					count++;
					if (count >= Rule_WinChess)
					{
						return true;
					}
				}
				else
				{
					break;
				}
			}
			for (x = origin.X - dx ,y = origin.Y - dy; (0 <= x && x < chessBoard_Columns) && (0 <= y && y < chessBoard_Rows); x -= dx, y -= dy)
			{
				if (playerFlags[x, y] == player)
				{
					count++;
					if (count >= Rule_WinChess)
					{
						return true;
					}
				}
				else
				{
					break;
				}
			}

			return false;
		}
	}
}
