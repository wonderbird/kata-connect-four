using System;
using System.Collections.Generic;
using System.Linq;

namespace Kata.Logic
{
    public static class ConnectFour
    {
        public static string WhoIsWinner(List<string> piecesPositionList)
        {
            var board = new Board();
            var engine = new Engine();

            foreach (var piecePosition in piecesPositionList)
            {
                var column = ParsePieceColumn(piecePosition);
                var player = ParsePlayer(piecePosition);
                board.AddPieceToColumn(column, player);
            }

            var winner = engine.CalculateWinner(board);
            return winner == Player.None ? "Draw" : winner.ToString();
        }

        private static Player ParsePlayer(string piecePosition)
        {
            var colorString = piecePosition[2..];
            var piece = colorString == "Red" ? Player.Red : Player.Yellow;
            return piece;
        }

        private static int ParsePieceColumn(string piecePosition) => piecePosition[0] - 'A';
    }

    public class Engine
    {
        private List<IWinnerCalculator> WinnerCalculators = new();

        public Engine()
        {
            WinnerCalculators.Add(new ColumnWinnerCalculator());
            WinnerCalculators.Add(new RowWinnerCalculator());
            WinnerCalculators.Add(new AscendingDiagonalWinnerCalculator());
        }

        public Player CalculateWinner(Board board)
        {
            var winner = Player.None;
            using var winnerCalculator = WinnerCalculators.GetEnumerator();

            while (winner == Player.None && winnerCalculator.MoveNext())
            {
                winner = winnerCalculator.Current.CalculateWinner(board);
            }

            return winner;
        }
    }

    internal interface IWinnerCalculator
    {
        Player CalculateWinner(Board board);
    }

    public class AscendingDiagonalWinnerCalculator : AbstractWinnerCalculator
    {
        protected override IEnumerable<IEnumerable<Player>> GetGroupedPieces(Board board)
        {
            for (var diagonal = 0; diagonal < AscendingDiagonalCoordinateSystem.Diagonals; diagonal++)
            {
                throw new NotImplementedException();
            }
            for (var row = 0; row < Board.Rows; ++row)
            {
                yield return Enumerable
                    .Range(0, Board.Columns)
                    .Select(i => board.GetPieceAt(row, i))
                    .ToList();
            }
        }
    }

    public static class AscendingDiagonalCoordinateSystem
    {
        public const int Diagonals = 12;

        public static int Positions(int diagonal) => diagonal <= 5 ? diagonal + 1 : 12 - diagonal;
    }

    public class RowWinnerCalculator : AbstractWinnerCalculator
    {
        protected override IEnumerable<IEnumerable<Player>> GetGroupedPieces(Board board)
        {
            for (var row = 0; row < Board.Rows; ++row)
            {
                yield return Enumerable
                    .Range(0, Board.Columns)
                    .Select(i => board.GetPieceAt(row, i))
                    .ToList();
            }
        }
    }

    public class ColumnWinnerCalculator : AbstractWinnerCalculator
    {
        protected override IEnumerable<IEnumerable<Player>> GetGroupedPieces(Board board)
        {
            for (var column = 0; column < Board.Columns; column++)
            {
                yield return Enumerable
                    .Range(0, Board.Rows)
                    .Select(i => board.GetPieceAt(i, column))
                    .ToList();
            }
        }
    }

    public abstract class AbstractWinnerCalculator : IWinnerCalculator
    {
        private const int SameColorPiecesRequiredToWin = 4;

        public Player CalculateWinner(Board board)
        {
            using var columns = GetGroupedPieces(board).GetEnumerator();
            return CalculateWinnerByGroupedBoard(columns);
        }

        protected abstract IEnumerable<IEnumerable<Player>> GetGroupedPieces(Board board);

        private static Player CalculateWinnerByGroupedBoard(IEnumerator<IEnumerable<Player>> groupedBoard)
        {
            var winner = Player.None;
            while (winner == Player.None && groupedBoard.MoveNext())
            {
                winner = CalculateWinnerByLine(groupedBoard.Current);
            }

            return winner;
        }

        private static Player CalculateWinnerByLine(IEnumerable<Player> piecesOfColumn)
        {
            var winner = Player.None;
            var consecutiveRed = 0;
            var consecutiveYellow = 0;

            foreach (var piece in piecesOfColumn)
            {
                if (piece == Player.Red)
                {
                    consecutiveYellow = 0;
                    ++consecutiveRed;
                }
                else if (piece == Player.Yellow)
                {
                    consecutiveRed = 0;
                    ++consecutiveYellow;
                }

                if (consecutiveRed >= SameColorPiecesRequiredToWin)
                {
                    winner = Player.Red;
                }

                if (consecutiveYellow >= SameColorPiecesRequiredToWin)
                {
                    winner = Player.Yellow;
                }
            }

            return winner;
        }
    }

    public class Board
    {
        public const int Columns = 7;
        public const int Rows = 6;

        private readonly Player[,] _pieces = new Player[Rows, Columns];
        private readonly int[] _piecesPerColumn = new int[Columns];

        public void AddPieceToColumn(int column, Player player)
        {
            var numberOfPiecesInColumn = _piecesPerColumn[column];

            _pieces[numberOfPiecesInColumn, column] = player;
            _piecesPerColumn[column]++;
        }

        public Player GetPieceAt(int row, int column) => _pieces[row, column];

        public IEnumerable<IEnumerable<Player>> GetPiecesByColumn()
        {
            for (var column = 0; column < Columns; column++)
            {
                yield return Enumerable
                    .Range(0, Rows)
                    .Select(i => _pieces[i, column])
                    .ToList();
            }
        }
    }

    public enum Player
    {
        None = 0,
        Red,
        Yellow,
    }
}