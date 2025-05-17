namespace Ajedrez.Models.Pieces
{
    public class King : PieceBase
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

        public King(PieceColor color, Position position)
        {
            Color = color;
            Icon = color == PieceColor.White ? Properties.Resources.WhiteKing : Properties.Resources.BlackKing;
            Position = position;
        }
    }
}
