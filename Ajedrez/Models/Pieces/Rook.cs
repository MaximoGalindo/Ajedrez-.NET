namespace Ajedrez.Models.Pieces
{
    public class Rook : PieceBase
    {
        public (int, int)[] PosibleMoves = new (int, int)[]
        {
            (-1, 0),
            (1, 0),
            (0, -1),
            (0, 1)
        };

        public Rook(PieceColor color, Position position)
        {
            Color = color;
            Icon = color == PieceColor.White ? Properties.Resources.WhiteRook : Properties.Resources.BlackRook;
            Position = position;
        }
    }
}
