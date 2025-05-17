namespace Ajedrez.Models.Pieces
{
    public class Knight : PieceBase
    {
        public (int, int)[] PosibleMoves = new (int, int)[]
        {
            (2 , -1),
            (2 , 1),
            (1, 2 ),
            (-1, 2),
            (-2, 1),
            (-2, -1),
            (-1, -2),
            (1, -2)
        };  

        public Knight(PieceColor color, Position position)
        {
            Color = color;
            Icon = color == PieceColor.White ? Properties.Resources.WhiteKnight : Properties.Resources.BlackKnight;
            Position = position;
        }
    }
}
