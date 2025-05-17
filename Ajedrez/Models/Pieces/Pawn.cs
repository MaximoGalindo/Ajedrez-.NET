namespace Ajedrez.Models.Pieces
{
    public class Pawn : PieceBase
    {
        public (int, int)[] PosibleMoves = new (int, int)[]
        {

        };

        public Pawn(PieceColor color, Position position)
        {
            Color = color;
            Icon = color == PieceColor.White ? Properties.Resources.WhitePawn : Properties.Resources.BlackPawn;
            Position = position;
        }
    }
}
