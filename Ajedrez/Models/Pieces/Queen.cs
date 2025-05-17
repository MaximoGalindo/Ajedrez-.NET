namespace Ajedrez.Models.Pieces
{
    public class Queen : PieceBase
    {
        public (int, int)[] PosibleMoves = new (int, int)[]
        {
            (1, 0),
            (-1, 0),
            (0, 1),
            (0, -1),
            (1, 1),
            (-1, -1),
            (1, -1),
            (-1, 1)
        };

        public Queen(PieceColor color, Position position)
        {
            Color = color;
            Icon = color == PieceColor.White ? Properties.Resources.WhiteQueen : Properties.Resources.BlackQueen;
            Position = position;
        }
    }
}
