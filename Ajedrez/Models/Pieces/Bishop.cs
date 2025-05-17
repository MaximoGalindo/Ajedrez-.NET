namespace Ajedrez.Models.Pieces
{
    public class Bishop : PieceBase
    {
        public (int, int)[] PosibleMoves = new (int, int)[]
        {
            (1, 1 ),
            (1, -1),
            (-1, 1),
            (-1, -1)
        };

        public Bishop(PieceColor color, Position position)
        {
            Color = color;
            Icon = color == PieceColor.White ? Properties.Resources.WhiteBishop : Properties.Resources.BlackBishop;
            Position = position;
        }
    }
}
