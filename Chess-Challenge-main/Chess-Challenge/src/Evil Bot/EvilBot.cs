using System;
using System.Linq;
using ChessChallenge.API;

namespace ChessChallenge.Example
{
    public class EvilBot : IChessBot
    {
        enum GamePhases
        {
            OPENING = -1,
            MIDDLEGAME = 0,
            ENDGAME = 1
        }
        int PawnValue = 100; // 1600 total 
        int KnightValue = 300; // 1200 total
        int BishopValue = 300; // 1200 total
        int RookValue = 500; // 2000 total
        int QueenValue = 900; // 1800 total == 7800 TOTAL 
        Move BestMove;
        int BestEval;
        Move BestMoveThisIteration;
        int BestEvalThisIteration;
        int ImmediateMateScore = 100000;
        const int MateScore = int.MaxValue;
        int Depth = 5;
        GamePhases GameWeight;

        /* OPENING SQUARE TABLES */

        int[] PawnOpeningSquareTable = {
        70, 70, 70, 70, 70, 70, 70, 70,
        50, 50, 50, 60, 60, 50, 50, 50,
        20, 20, 30, 50, 50, 30, 20, 20,
        10, 10, 20, 40, 40, 20, 10, 10,
         0,  0, 10, 25, 25,  0,  0,  0,
         5, 15,  0, 20, 20,  0, 15,  5,
         5, 10, 10, -10, -10, 10, 10, 5,
         0, 0, 0, 0, 0, 0, 0, 0
    };

        int[] KnightOpeningSquareTable = {
        -50, -40, -30, -30, -30, -30, -40, -50,
        -40, -20, 0, 0, 0, 0, -20, -40,
        -30, 10, 20, 25, 25, 20, 10, -30,
        -30, 15, 25, 30, 30, 25, 15, -30,
        -30, 10, 25, 30, 30, 25, 10, -30,
        -30, 15, 20, 25, 25, 20, 15, -30,
        -40, -20, 15, 20, 20, 15, -20, -40,
        -50, -40, -30, -30, -30, -30, -40, -50
    };
        int[] BishopOpeningSquareTable = {
        -20, -10, -10, -10, -10, -10, -10, -20,
        -10, 0, 0, 0, 0, 0, 0, -10,
        -10, 0, 5, 10, 10, 5, 0, -10,
        -10, 15, 10, 15, 15, 10, 15, -10,
        -10, 0, 10, 20, 20, 10, 0, -10,
        0, 10, 15, 10, 10, 15, 10, 0,
        -10, 15, 0, 10, 10, 0, 15, -10,
        -20, -10, -10, -10, -10, -10, -10, -20
    };

        int[] RookOpeningSquareTable = {
        0, 0, 0, 5, 5, 0, 0, 0,
        5, 10, 10, 10, 10, 10, 10, 5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        0, 0, 0, 0, 0, 0, 0, 0,
        -5, 0, 0, 0, 0, 0, 0, -5,
        0, 5, 10, 15, 15, 10, 5, 0
    };

        int[] QueenOpeningSquareTable = {
        -20, -10, -10, -5, -5, -10, -10, -20,
        -10, 0, 0, 0, 0, 0, 0, -10,
        -10, 0, 5, 5, 5, 5, 0, -10,
        -5, 10, 15, 15, 15, 15, 10, -5,
        10, 10, 15, 15, 15, 15, 10, 10,
        -10, 15, 15, 15, 15, 15, 10, -10,
        -10, 10, 15, 10, 10, 10, 10, -10,
        -20, -10, -10, -5, -5, -10, -10, -20
    };

        int[] KingOpeningSquareTable = {
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -20, -30, -30, -40, -40, -30, -30, -20,
        -10, -20, -20, -20, -20, -20, -20, -10,
        0, 0, 0, 0, 0, 0, 1, 1,
        20, 50, 15, 5, 5, 15, 50, 20
    };

        /* SQUARE TABLES */

        int[] PawnSquareTable = {
        70, 70, 70, 70, 70, 70, 70, 70,
        50, 50, 50, 50, 50, 50, 50, 50,
        10, 10, 20, 30, 30, 20, 10, 10,
         5,  5, 10, 25, 25, 10,  5,  5,
         0,  0,  0, 20, 20,  0,  0,  0,
         5, 15,  0, 10, 10,  0, 15,  5,
         5, 10, 10,-20,-20, 10, 10,  5,
         0,  0,  0,  0,  0,  0,  0,  0
    };

        int[] KnightSquareTable = {
        -50, -40, -30, -30, -30, -30, -40, -50,
        -40, -20, 0, 0, 0, 0, -20, -40,
        -30, 0, 10, 15, 15, 10, 0, -30,
        -30, 5, 15, 20, 20, 15, 5, -30,
        -30, 0, 15, 20, 20, 15, 0, -30,
        -30, 5, 10, 15, 15, 10, 5, -30,
        -40, -20, 0, 5, 5, 0, -20, -40,
        -50, -40, -30, -30, -30, -30, -40, -50
    };

        int[] BishopSquareTable = {
        -20, -10, -10, -10, -10, -10, -10, -20,
        -10, 0, 0, 0, 0, 0, 0, -10,
        -10, 0, 5, 10, 10, 5, 0, -10,
        -10, 5, 5, 10, 10, 5, 5, -10,
        -10, 0, 10, 10, 10, 10, 0, -10,
        -10, 10, 10, 10, 10, 10, 10, -10,
        -10, 5, 0, 0, 0, 0, 5, -10,
        -20, -10, -10, -10, -10, -10, -10, -20
    };

        int[] RookSquareTable = {
        0, 0, 0, 5, 5, 0, 0, 0,
        5, 10, 10, 10, 10, 10, 10, 5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        0, 0, 0, 5, 5, 0, 0, 0
    };

        int[] QueenSquareTable = {
        -20, -10, -10, -5, -5, -10, -10, -20,
        -10, 0, 0, 0, 0, 0, 0, -10,
        -10, 0, 5, 5, 5, 5, 0, -10,
        -5, 0, 5, 5, 5, 5, 0, -5,
        0, 0, 5, 5, 5, 5, 0, 0,
        -10, 5, 5, 5, 5, 5, 0, -10,
        -10, 0, 5, 0, 0, 0, 0, -10,
        -20, -10, -10, -5, -5, -10, -10, -20
    };

        int[] KingSquareTable = {
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -20, -30, -30, -40, -40, -30, -30, -20,
        -10, -20, -20, -20, -20, -20, -20, -10,
        20, 20, 0, 0, 0, 0, 20, 20,
        20, 30, 10, 0, 0, 10, 30, 20
    };
        /* ENDGAME TABLES */
        int[] KingEndgameSquareTable = {
        -20, -20, -20, -20, -20, -20, -20, -20,
        -20, -10, -10, -10, -10, -10, -10, -10,
        -20, -10, -5, -5, -5, -5, -10, -20,
        -15, -5, 0, 20, 20, 0, -5, -15,
        -15, -5, 0, 20, 20, 0, -5, -10,
        -20, -10, -5, -5, -5, -5, -10, -20,
        -20, -10, -10, -10, -10, -10, -10, -10,
        -20, -20, -20, -20, -20, -20, -20, -20
    };

        public Move Think(Board board, Timer timer)
        {
            // this is the only function called from this class.
            BestEvalThisIteration = BestEval = 0;
            BestMoveThisIteration = BestMove = Move.NullMove;
            Search(board, Depth, int.MinValue, int.MaxValue);
            BestMove = BestMoveThisIteration;
            BestEval = BestEvalThisIteration;
            return BestMove;
        }
        int GetPieceValue(PieceType piece)
        {
            // gets the value of the piece
            return piece switch
            {
                PieceType.Pawn => PawnValue,
                PieceType.Knight => KnightValue,
                PieceType.Bishop => BishopValue,
                PieceType.Rook => RookValue,
                PieceType.Queen => QueenValue,
                _ => 0,
            };

        }
        void DetermineGameWeight(Board board)
        {
            // how strong the weight should be in determining evaluation during the game
            int MaterialLeft = CountMaterial(board, true) + CountMaterial(board, false);
            // Endgame Function for considering endgame values
            if (MaterialLeft >= 5850)
            {
                GameWeight = GamePhases.OPENING;
            }
            else if (MaterialLeft < 5850 && MaterialLeft >= 2730)
            {
                GameWeight = GamePhases.MIDDLEGAME;
            }
            else
            {
                GameWeight = GamePhases.ENDGAME;
            }
        }

        int CountMaterial(Board board, bool isWhite)
        {
            //Counts the material for one side and returns the total value of the pieces
            int material = 0;
            material += board.GetPieceList(PieceType.Pawn, isWhite).Count * PawnValue;
            material += board.GetPieceList(PieceType.Knight, isWhite).Count * KnightValue;
            material += board.GetPieceList(PieceType.Bishop, isWhite).Count * BishopValue;
            material += board.GetPieceList(PieceType.Rook, isWhite).Count * RookValue;
            material += board.GetPieceList(PieceType.Queen, isWhite).Count * QueenValue;
            return material;
        }
        int ForceKingToCornerEndgameEvaluation(Board board)
        {
            // makes an evaluation to force the king to the corner (may not work?)
            int Eval = 0;
            int OpponentKingRank = board.GetKingSquare(!board.IsWhiteToMove).Rank;
            int OpponentKingFile = board.GetKingSquare(!board.IsWhiteToMove).File;
            int OpponentKingDistanceToCenterFile = Math.Max(3 - OpponentKingFile, OpponentKingFile - 4);
            int OpponentKingDistanceToCenterRank = Math.Max(3 - OpponentKingRank, OpponentKingRank - 4);
            int OpponentKingDistanceToCenter = OpponentKingDistanceToCenterFile + OpponentKingDistanceToCenterRank;
            Eval += OpponentKingDistanceToCenter;
            int KingRank = board.GetKingSquare(board.IsWhiteToMove).Rank;
            int KingFile = board.GetKingSquare(board.IsWhiteToMove).File;
            int KingDistanceToCenterFile = Math.Max(3 - KingFile, KingFile - 4);
            int KingDistanceToCenterRank = Math.Max(3 - KingRank, KingRank - 4);
            int KingDistanceToCenter = KingDistanceToCenterFile + KingDistanceToCenterRank;
            Eval += KingDistanceToCenter;
            int DistanceBetweenKingsFile = Math.Abs(KingFile - OpponentKingFile);
            int DistanceBetweenKingsRank = Math.Abs(KingRank - OpponentKingRank);
            int DistanceBetweenKings = DistanceBetweenKingsFile + DistanceBetweenKingsRank;
            Eval += 14 - DistanceBetweenKings;
            return Eval * (GameWeight == GamePhases.ENDGAME ? 2 : 1);
        }
        public int Evaluate(Board board)
        {
            // looks at both piece material values for white and black and returns
            // white - black and changed for whether the bot is playing black or white
            int Eval = 0;

            int whiteMaterial = CountMaterial(board, board.IsWhiteToMove);
            int blackMaterial = -CountMaterial(board, !board.IsWhiteToMove);
            Eval += whiteMaterial + blackMaterial;

            DetermineGameWeight(board);
            Eval += ForceKingToCornerEndgameEvaluation(board);

            int[] OpponentPawnSquareTable = PawnSquareTable.Reverse().ToArray();
            int[] OpponentKnightSquareTable = KnightSquareTable.Reverse().ToArray();
            int[] OpponentBishopSquareTable = BishopSquareTable.Reverse().ToArray();
            int[] OpponentRookSquareTable = RookSquareTable.Reverse().ToArray();
            int[] OpponentQueenSquareTable = QueenSquareTable.Reverse().ToArray();
            int[] OpponentKingSquareTable = KingSquareTable.Reverse().ToArray();

            int[] OpponentOpeningPawnSquareTable = PawnOpeningSquareTable.Reverse().ToArray();
            int[] OpponentOpeningKnightSquareTable = KnightOpeningSquareTable.Reverse().ToArray();
            int[] OpponentOpeningBishopSquareTable = BishopOpeningSquareTable.Reverse().ToArray();
            int[] OpponentOpeningRookSquareTable = RookOpeningSquareTable.Reverse().ToArray();
            int[] OpponentOpeningQueenSquareTable = QueenOpeningSquareTable.Reverse().ToArray();
            int[] OpponentOpeningKingSquareTable = KingOpeningSquareTable.Reverse().ToArray();

            int[] OpponentKingEndgameSquareTable = KingEndgameSquareTable.Reverse().ToArray();

            PieceList Pawns = board.GetPieceList(PieceType.Pawn, true);
            PieceList BlackPawns = board.GetPieceList(PieceType.Pawn, false);
            for (int i = 0; i < Pawns.Count; i++)
            {
                if (GameWeight == GamePhases.OPENING)
                {
                    Eval += PawnOpeningSquareTable[(Pawns[i].Square.Rank * 8) + Pawns[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
                else
                {
                    Eval += PawnSquareTable[(Pawns[i].Square.Rank * 8) + Pawns[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
            }
            for (int i = 0; i < BlackPawns.Count; i++)
            {
                if (GameWeight == GamePhases.OPENING)
                {
                    Eval += OpponentOpeningPawnSquareTable[(BlackPawns[i].Square.Rank * 8) + BlackPawns[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
                else
                {
                    Eval += OpponentPawnSquareTable[(BlackPawns[i].Square.Rank * 8) + BlackPawns[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
            }

            PieceList Knights = board.GetPieceList(PieceType.Knight, true);
            PieceList BlackKnights = board.GetPieceList(PieceType.Knight, false);
            for (int i = 0; i < Knights.Count; i++)
            {
                if (GameWeight == GamePhases.OPENING)
                {
                    Eval += KnightOpeningSquareTable[(Knights[i].Square.Rank * 8) + Knights[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
                else
                {
                    Eval += KnightSquareTable[(Knights[i].Square.Rank * 8) + Knights[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
            }
            for (int i = 0; i < BlackKnights.Count; i++)
            {
                if (GameWeight == GamePhases.OPENING)
                {
                    Eval += OpponentOpeningKnightSquareTable[(BlackKnights[i].Square.Rank * 8) + BlackKnights[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
                else
                {
                    Eval += OpponentKnightSquareTable[(BlackKnights[i].Square.Rank * 8) + BlackKnights[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
            }

            PieceList Bishops = board.GetPieceList(PieceType.Knight, true);
            PieceList BlackBishops = board.GetPieceList(PieceType.Knight, false);
            for (int i = 0; i < Bishops.Count; i++)
            {
                if (GameWeight == GamePhases.OPENING)
                {
                    Eval += BishopOpeningSquareTable[(Bishops[i].Square.Rank * 8) + Bishops[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
                else
                {
                    Eval += BishopSquareTable[(Bishops[i].Square.Rank * 8) + Bishops[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
            }
            for (int i = 0; i < BlackBishops.Count; i++)
            {
                if (GameWeight == GamePhases.OPENING)
                {
                    Eval += OpponentOpeningBishopSquareTable[(BlackBishops[i].Square.Rank * 8) + BlackBishops[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
                else
                {
                    Eval += OpponentBishopSquareTable[(BlackBishops[i].Square.Rank * 8) + BlackBishops[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
            }

            PieceList Rooks = board.GetPieceList(PieceType.Rook, true);
            PieceList BlackRooks = board.GetPieceList(PieceType.Rook, false);
            for (int i = 0; i < Rooks.Count; i++)
            {
                if (GameWeight == GamePhases.OPENING)
                {
                    Eval += RookOpeningSquareTable[(Rooks[i].Square.Rank * 8) + Rooks[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
                else
                {
                    Eval += RookSquareTable[(Rooks[i].Square.Rank * 8) + Rooks[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
            }
            for (int i = 0; i < BlackRooks.Count; i++)
            {
                if (GameWeight == GamePhases.OPENING)
                {
                    Eval += OpponentOpeningRookSquareTable[(BlackRooks[i].Square.Rank * 8) + BlackRooks[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
                else
                {
                    Eval += OpponentRookSquareTable[(BlackRooks[i].Square.Rank * 8) + BlackRooks[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
            }

            PieceList Queen = board.GetPieceList(PieceType.Queen, true);
            PieceList BlackQueen = board.GetPieceList(PieceType.Queen, false);
            for (int i = 0; i < Queen.Count; i++)
            {
                if (GameWeight == GamePhases.OPENING)
                {
                    Eval += QueenOpeningSquareTable[(Queen[i].Square.Rank * 8) + Queen[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
                else
                {
                    Eval += QueenSquareTable[(Queen[i].Square.Rank * 8) + Queen[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
            }
            for (int i = 0; i < BlackQueen.Count; i++)
            {
                if (GameWeight == GamePhases.OPENING)
                {
                    Eval += OpponentOpeningQueenSquareTable[(BlackQueen[i].Square.Rank * 8) + BlackQueen[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
                else
                {
                    Eval += OpponentQueenSquareTable[(BlackQueen[i].Square.Rank * 8) + BlackQueen[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
            }

            PieceList King = board.GetPieceList(PieceType.King, true);
            PieceList BlackKing = board.GetPieceList(PieceType.King, false);
            if (GameWeight == GamePhases.OPENING)
            {
                for (int i = 0; i < King.Count; i++)
                {
                    Eval += KingOpeningSquareTable[(King[i].Square.Rank * 8) + King[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
                for (int i = 0; i < BlackKing.Count; i++)
                {
                    Eval += OpponentOpeningKingSquareTable[(BlackKing[i].Square.Rank * 8) + BlackKing[i].Square.File] * (board.IsWhiteToMove ? -1 : 1);
                }
            }
            else if (GameWeight == GamePhases.MIDDLEGAME)
            {
                for (int i = 0; i < King.Count; i++)
                {
                    Eval += KingSquareTable[(King[i].Square.Rank * 8) + King[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
                for (int i = 0; i < BlackKing.Count; i++)
                {
                    Eval += OpponentKingSquareTable[(BlackKing[i].Square.Rank * 8) + BlackKing[i].Square.File] * (board.IsWhiteToMove ? -1 : 1);
                }
            }
            else if (GameWeight == GamePhases.ENDGAME)
            {
                for (int i = 0; i < King.Count; i++)
                {
                    Eval += KingEndgameSquareTable[(King[i].Square.Rank * 8) + King[i].Square.File] * (board.IsWhiteToMove ? 1 : -1);
                }
                for (int i = 0; i < BlackKing.Count; i++)
                {
                    Eval += OpponentKingEndgameSquareTable[(BlackKing[i].Square.Rank * 8) + BlackKing[i].Square.File] * (board.IsWhiteToMove ? -1 : 1);
                }
            }

            return Eval;
        }

        public void OrderMoves(Board board, Move[] moves)
        {
            // guesses how good a move is
            // orders moves by whichever have the highest scores
            // else - penalizes player for moving piece to a square attacked by an opponent
            int[] scores = new int[moves.Length];
            for (int i = 0; i < moves.Length; i++)
            {
                int score = 0;
                PieceType moveType = board.GetPiece(moves[i].StartSquare).PieceType;
                PieceType captureType = board.GetPiece(moves[i].TargetSquare).PieceType;
                if (captureType != PieceType.None)
                {
                    if (board.SquareIsAttackedByOpponent(moves[i].TargetSquare))
                    {
                        int NetValue = GetPieceValue(moveType) - GetPieceValue(captureType);
                        if (NetValue >= 0)
                        {
                            score -= 10 * GetPieceValue(captureType);
                        }
                        else
                        {
                            score += 10 * NetValue;
                        }
                    }
                    else
                    {
                        score -= 10 * GetPieceValue(captureType);
                    }
                }
                if (moveType == PieceType.Pawn && moves[i].IsPromotion)
                {
                    score -= GetPieceValue(moves[i].PromotionPieceType);
                }
                else
                {
                    if (board.SquareIsAttackedByOpponent(moves[i].TargetSquare))
                    {
                        score += GetPieceValue(moveType);
                    }
                }
                scores[i] = score;
            }
            Array.Sort(scores, moves);
        }
        public int Search(Board board, int depth, int alpha, int beta)
        {
            if (depth == 0)
            {
                return Evaluate(board);
            }
            Move[] moves = board.GetLegalMoves();
            OrderMoves(board, moves);
            if (moves.Length == 0)
            {
                if (board.IsInCheck())
                {
                    return Evaluate(board) + -ImmediateMateScore;
                }
                if (board.IsInCheckmate())
                {
                    return -MateScore;
                }
                return 0;
            }
            int BestScore = int.MinValue;
            Move BestMoveTemp = Move.NullMove;
            for (int i = 0; i < moves.Length; i++)
            {
                board.MakeMove(moves[i]);
                int score = -Search(board, depth - 1, -beta, -alpha);
                board.UndoMove(moves[i]);
                if (score > BestScore)
                {
                    BestScore = score;
                    BestMoveTemp = moves[i];
                }
                if (score > alpha)
                {
                    alpha = score;

                }
                if (alpha >= beta)
                {
                    break;
                }
            }
            BestMoveThisIteration = BestMoveTemp;
            BestEvalThisIteration = BestScore;
            return alpha;
        }
    }
}