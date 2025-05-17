using Ajedrez.Models;
using Ajedrez.Models.Pieces;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Ajedrez
{
    partial class ChessGame
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        /// 

        private const int tileSize = 60;
        private const int gridSize = 8;
        private Button startButton;
        private Button restartButton;
        private PieceBase SelectedPiece { get; set; }
        private Button[,] boardButtons = new Button[gridSize, gridSize];
        private Turn Turn = Turn.White;
        private bool KingInCheck = false;
        private PieceBase CheckingPiece {  get; set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ResumeLayout(false);
            CreateBoard();
            LoadPieces();
        }

        private void CreateBoard()
        {
            this.Controls.Clear();
            Turn = Turn.White;
            KingInCheck = false;
            SelectedPiece = null;

            boardButtons = new Button[gridSize, gridSize];

            Panel boardPanel = new Panel();
            boardPanel.Name = "BoardPanel";
            boardPanel.Width = gridSize * tileSize;
            boardPanel.Height = gridSize * tileSize;
            boardPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            for (int fila = 0; fila < gridSize; fila++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    Button casilla = new Button();
                    casilla.Width = tileSize;
                    casilla.Height = tileSize;
                    casilla.Left = col * tileSize;
                    casilla.Top = fila * tileSize;

                    casilla.BackColor = (fila + col) % 2 == 0 ? Color.Beige : Color.Sienna;
                    casilla.Tag = (fila, col);
                    casilla.Enabled = false;
                    casilla.Click += Casilla_Click;

                    boardButtons[fila, col] = casilla;
                    boardPanel.Controls.Add(casilla);
                }
            }

            this.Controls.Add(boardPanel);

            // Botón Start
            startButton = new Button();
            startButton.Text = "Start";
            startButton.Width = 100;
            startButton.Height = 40;
            startButton.Location = new Point(boardPanel.Right + 20, boardPanel.Top);
            startButton.Click += StartButton_Click;
            this.Controls.Add(startButton);

            // Botón Restart
            restartButton = new Button();
            restartButton.Text = "Restart";
            restartButton.Width = 100;
            restartButton.Height = 40;
            restartButton.Location = new Point(boardPanel.Right + 20, boardPanel.Top + 60);
            restartButton.Click += RestartButton_Click;
            this.Controls.Add(restartButton);

            int widthWithButtons = boardPanel.Width + 150;
            int heightWithButtons = boardPanel.Height + 40;
            this.ClientSize = new Size(widthWithButtons, heightWithButtons);
            this.MinimumSize = new Size(widthWithButtons, heightWithButtons);

            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.Text = "Tablero de Ajedrez";
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            EnableTurnPieces();
        }

        private void RestartButton_Click(object sender, EventArgs e)
        {
            CreateBoard();
            LoadPieces();
            EnableTurnPieces();
        }

        private void Casilla_Click(object sender, EventArgs e)
        {
            var boton = sender as Button;
            var piece = boton.Tag as PieceBase;

            if (piece != null && SelectedPiece != null)
                CleanBoard();

            if (piece != null && piece.Color.ToString() == Turn.ToString())
            {
                SelectedPiece = piece;
                List<(int,int)> avaibleMoves = new List<(int, int)>();
                avaibleMoves = GetAvaibleMoves(piece);

                if (KingInCheck)
                    avaibleMoves = GetAvaibleMovesWhenKingIsInChek(piece);               

                if(piece is King)
                    avaibleMoves = GetKingAvaibleMoves();

                ShowAvaibleMoves(avaibleMoves);
            }
            else if (SelectedPiece != null)
            {
                if (TryGetTargetPosition(boton, out var targetPosition))
                {
                    MoveSelectedPiece(targetPosition);
                    CleanBoard();
                    IsKingInChek();
                    ChangeTurn();                   
                }
            }
        }

        private void ShowAvaibleMoves(List<(int, int)> avaibleMoves)
        {
            foreach(var move in avaibleMoves)
            {
               var cell = boardButtons[move.Item1, move.Item2];
               if (cell.Tag is PieceBase enemyPiece && enemyPiece.Color.ToString() != Turn.ToString())
               {
                    cell.BackColor = Color.Green;
                    cell.Enabled = true;
                    continue;
               }
               cell.Text = "⚫";
               cell.Enabled = true;
            }
        }

        private bool TryGetTargetPosition(Button boton, out Position targetPosition)
        {
            targetPosition = null;

            if (boton.Tag is PieceBase piezaEnemiga && piezaEnemiga.Color != SelectedPiece.Color)
            {
                targetPosition = piezaEnemiga.Position;
                return true;
            }

            if (boton.Tag is ValueTuple<int, int> tuple)
            {
                targetPosition = new Position(tuple.Item1, tuple.Item2);
                return true;
            }

            return false;
        }

        private void MoveSelectedPiece(Position targetPosition)
        {
            var (prevRow, prevCol) = (SelectedPiece.Position.Row, SelectedPiece.Position.Column);

            boardButtons[prevRow, prevCol].BackgroundImage = null;
            boardButtons[prevRow, prevCol].Tag = (prevRow, prevCol);

            SelectedPiece.Position = targetPosition;
            boardButtons[targetPosition.Row, targetPosition.Column].BackgroundImage = ByteArrayToImage(SelectedPiece.Icon);
            boardButtons[targetPosition.Row, targetPosition.Column].Text = string.Empty;
            boardButtons[targetPosition.Row, targetPosition.Column].Tag = SelectedPiece;
            KingInCheck = false;
        }

        private List<(int,int)> GetAvaibleMoves(PieceBase piece)
        {
            List<(int, int)> avaibleMoves = new List<(int, int)>();
            var field = piece.GetType().GetField("PosibleMoves");
            var posibleMoves = ((int, int)[])field.GetValue(piece);

            if (piece is Pawn)
            {
                avaibleMoves = GetPawnMoves(piece);
                return avaibleMoves;
            }
           
            foreach (var dir in posibleMoves)
            {                
                int newRow = piece.Position.Row + dir.Item1;
                int newCol = piece.Position.Column + dir.Item2;

                int loop = gridSize;
                if (piece is Knight || piece is King) loop = 1;

                for (int i = 0; i < loop; i++)
                {
                    if (newRow < 0 || newRow >= gridSize || newCol < 0 || newCol >= gridSize)
                        continue;

                    var cellDestination = boardButtons[newRow, newCol];

                    if (cellDestination.Tag is PieceBase piezaEnemiga && piezaEnemiga.Color != piece.Color)
                    {
                        avaibleMoves.Add((newRow, newCol));
                        break;
                    }

                    if (cellDestination.Tag is PieceBase piezaAliada && piezaAliada.Color == piece.Color)
                        break;

                    avaibleMoves.Add((newRow, newCol));
                    newRow += dir.Item1;
                    newCol += dir.Item2;
                }
            }
            return avaibleMoves;
        }

        private List<(int, int)> GetPosibleCheckEnemyMoves()
        {
            var movimientoReina = new List<(int, int)> { (1, 3) }; 

            var posibleMoves = new List<(int, int)>();

            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    if (boardButtons[row, col].Tag is PieceBase enemyPiece && enemyPiece.Color.ToString() != Turn.ToString())
                    {
                        posibleMoves.AddRange(GetAvaibleMoves(enemyPiece));
                        break;
                    }
                }
            }
            return posibleMoves;
        }

        private List<(int, int)> GetKingAvaibleMoves()
        {
            var kingPosition = GetKingPosition(GetColorTurn());
            King king = (King)boardButtons[kingPosition.Item1, kingPosition.Item2].Tag;
            var kingAvaibleMoves = GetAvaibleMoves(king);

            var posibleEnemyMoves = new List<(int, int)>();
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    if (boardButtons[row, col].Tag is PieceBase piece && piece.Color.ToString() != Turn.ToString())
                    {
                        posibleEnemyMoves.AddRange(GetAvaibleMoves(piece));
                    }                        
                }
            }

            var allowedKingMoves = new List<(int, int)>(kingAvaibleMoves);
            foreach (var kingMove in kingAvaibleMoves)
            {
                if (posibleEnemyMoves.Contains(kingMove))
                    allowedKingMoves.Remove(kingMove);
            }

            return allowedKingMoves;
        }

        private void IsKingInChek()
        {
            var selectedPieceMoves = GetAvaibleMoves(SelectedPiece);

            foreach (var move in selectedPieceMoves)
            {
                var cell = boardButtons[move.Item1, move.Item2];
                if (cell.Tag is PieceBase piece && piece is King)
                {
                    CheckingPiece = SelectedPiece;
                    SelectedPiece = null;
                    KingInCheck = true;
                }
            }

            if (KingInCheck)
            {
                string nextTurnColor = GetNextTurn();
                var posibleEnemyMoves = new List<(int, int)>();
                for (int row = 0; row < gridSize; row++)
                {
                    for (int col = 0; col < gridSize; col++)
                    {
                        if (boardButtons[row, col].Tag is PieceBase piece && piece.Color.ToString() == nextTurnColor)
                        {
                            posibleEnemyMoves.AddRange(GetAvaibleMovesWhenKingIsInChek(piece));
                        }
                    }
                }

                if (posibleEnemyMoves.Count == 0)
                {
                    MessageBox.Show("JAQUE MATE PETE");
                    return;
                }
            }


            if (KingInCheck)
            {
                MessageBox.Show($"El rey está en jaque por {CheckingPiece.GetType().Name}");
                return;
            }

            SelectedPiece = null;
            KingInCheck = false;
            CheckingPiece = null;
        }

        private List<(int, int)> GetAvaibleMovesWhenKingIsInChek(PieceBase piece)
        {
            var kingAvaibleMoves = GetKingAvaibleMoves();
            var chekingPieceMoves = GetAvaibleMoves(CheckingPiece);
            var pieceMoves = GetAvaibleMoves(piece);

            // Intersección entre las tres listas
            var commonMoves = kingAvaibleMoves
                .Intersect(chekingPieceMoves)
                .Intersect(pieceMoves)
                .ToList();

            foreach (var move in pieceMoves)
            {
                var cell = boardButtons[move.Item1, move.Item2];
                if (cell.Tag is PieceBase enemyPiece && enemyPiece == CheckingPiece)
                    commonMoves.Add(move);
            }

            return commonMoves;
        }

        private (int, int) GetKingPosition(PieceColor kingColor)
        {
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    if (boardButtons[row, col].Tag is PieceBase piece && piece.Color == kingColor && piece is King)
                    {
                        return (row, col);
                    }
                }
            }
            return (-1, -1);
        }

        private List<(int,int)> GetPawnMoves(PieceBase piece)
        {
            List<(int, int)> avaibleMoves = new List<(int, int)>();
            int newRow = piece.Position.Row + (piece.Color == PieceColor.White ? -1 : 1);
            int newCol = piece.Position.Column;

            if (newRow < 0 || newRow >= gridSize || newCol < 0 || newCol >= gridSize)
                return avaibleMoves;

            var cellDestination = boardButtons[newRow, newCol];
            int[] posibleEnemyDirection = { 1, -1 };

            foreach (var dir in posibleEnemyDirection)
            {
                if (newCol + dir > gridSize || newCol + dir < 0) continue;

                int targetCol = newCol + dir;

                if (targetCol >= 0 && targetCol < 8)
                {
                    if (boardButtons[newRow, targetCol].Tag is PieceBase enemyPieceDiagonal &&
                        enemyPieceDiagonal.Color != piece.Color)
                    {
                        avaibleMoves.Add((newRow, targetCol));
                    }
                }
            }

            if (cellDestination.Tag is PieceBase enemyPieceDirect && enemyPieceDirect.Color != piece.Color)
                return avaibleMoves;

            if (cellDestination.Tag is PieceBase alliedPiece && alliedPiece.Color == piece.Color)
                return avaibleMoves;

            avaibleMoves.Add((newRow, newCol));
            return avaibleMoves;
        }

        private void LoadPieces()
        {
            SetPiece(0, 0, new Rook(PieceColor.Black, new Position(0, 0)), Properties.Resources.BlackRook);
            SetPiece(0, 1, new Knight(PieceColor.Black, new Position(0, 1)), Properties.Resources.BlackKnight);
            SetPiece(0, 2, new Bishop(PieceColor.Black, new Position(0, 2)), Properties.Resources.BlackBishop);
            SetPiece(0, 3, new Queen(PieceColor.Black, new Position(0, 3)), Properties.Resources.BlackQueen);
            SetPiece(0, 4, new King(PieceColor.Black, new Position(0, 4)), Properties.Resources.BlackKing);
            SetPiece(0, 5, new Bishop(PieceColor.Black, new Position(0, 5)), Properties.Resources.BlackBishop);
            SetPiece(0, 6, new Knight(PieceColor.Black, new Position(0, 6)), Properties.Resources.BlackKnight);
            SetPiece(0, 7, new Rook(PieceColor.Black, new Position(0, 7)), Properties.Resources.BlackRook);

            for (int col = 0; col < 8; col++)
            {
                SetPiece(1, col, new Pawn(PieceColor.Black, new Position(1, col)), Properties.Resources.BlackPawn);
            }

            // Piezas blancas
            for (int col = 0; col < 8; col++)
            {
                SetPiece(6, col, new Pawn(PieceColor.White, new Position(6, col)), Properties.Resources.WhitePawn);
            }

            SetPiece(7, 0, new Rook(PieceColor.White, new Position(7, 0)), Properties.Resources.WhiteRook);
            SetPiece(7, 1, new Knight(PieceColor.White, new Position(7, 1)), Properties.Resources.WhiteKnight);
            SetPiece(7, 2, new Bishop(PieceColor.White, new Position(7, 2)), Properties.Resources.WhiteBishop);
            SetPiece(7, 3, new Queen(PieceColor.White, new Position(7, 3)), Properties.Resources.WhiteQueen);
            SetPiece(7, 4, new King(PieceColor.White, new Position(7, 4)), Properties.Resources.WhiteKing);
            SetPiece(7, 5, new Bishop(PieceColor.White, new Position(7, 5)), Properties.Resources.WhiteBishop);
            SetPiece(7, 6, new Knight(PieceColor.White, new Position(7, 6)), Properties.Resources.WhiteKnight);
            SetPiece(7, 7, new Rook(PieceColor.White, new Position(7, 7)), Properties.Resources.WhiteRook);
        }

        private void SetPiece(int row, int col, PieceBase piece, byte[] imageBytes)
        {
            boardButtons[row, col].Tag = piece;
            boardButtons[row, col].BackgroundImage = ByteArrayToImage(imageBytes);
        }

        private void CleanBoard()
        {
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    var boardPosition = boardButtons[row, col];
                    if (boardPosition.BackgroundImage == null || boardPosition.Text == "⚫" || boardPosition.BackColor == Color.Green)
                    {
                        boardPosition.Enabled = false;
                        boardPosition.Text = string.Empty;
                    }
                    boardPosition.BackColor = (row + col) % 2 == 0 ? Color.Beige : Color.Sienna;
                }
            }
            //EnableTurnPieces();
        }

        private void ChangeTurn()
        {
            Turn = Turn == Turn.White ? Turn.Black : Turn.White;
            EnableTurnPieces();
        }

        private PieceColor GetColorTurn()
        {
            return PieceColor.White.ToString() == Turn.ToString() ? PieceColor.White : PieceColor.Black;
        }

        private void EnableTurnPieces()
        {
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    var boardPosition = boardButtons[row, col];
                    if (boardPosition.Tag is PieceBase piece && piece.Color.ToString() == Turn.ToString())
                        boardPosition.Enabled = true;
                    else
                        boardPosition.Enabled = false;
                }
            }
        }

        private string GetNextTurn()
        {
            if (Turn is Turn.White) return Turn.Black.ToString();
            else return Turn.White.ToString();
        }

        private static Image ByteArrayToImage(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                Image img = Image.FromStream(ms);
                return new Bitmap(img);
            }
        }
    }
}
