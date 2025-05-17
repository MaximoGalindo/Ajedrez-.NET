namespace Ajedrez.Models.Pieces
{
    public class PieceBase
    {
        public PieceColor Color { get; set; }
        public byte[] Icon { get; set; }
        public Position Position { get; set; }
    }
}
