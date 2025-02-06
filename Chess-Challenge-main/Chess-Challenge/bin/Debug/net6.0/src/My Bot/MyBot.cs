using System;
using System.Linq;
using ChessChallenge.API;

public class MyBot : IChessBot {
    enum GamePhases {
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
    GamePhases GamePhase; 

    // ! give a bonus to pro-played moves
    // ! give a bonus to more squares covered/attacked

    /* DEVELOPMENT BOARDS */

    bool[] OpeningDevelopmentTable = {
        false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false,
        true, true, true, true, true, true, true, true,
        true, true, true, true, true, true, true, true,
        true, true, true, true, true, true, true, true,
        true, true, true, true, true, true, true, true,
        false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false
    };

    int[] OpeningDevelopmentBonusTable = {

    };

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
        -50, -60, -30, -30, -30, -30, -60, -50
    };
    int[] BishopOpeningSquareTable = {
        -20, -10, -10, -10, -10, -10, -10, -20,
        -10, 0, 0, 0, 0, 0, 0, -10,
        -10, 0, 5, 10, 10, 5, 0, -10,
        -10, 15, 10, 15, 15, 10, 15, -10,
        -10, 0, 10, 20, 20, 10, 0, -10,
        0, 10, 15, 10, 10, 15, 10, 0,
        -10, 15, 0, 10, 10, 0, 15, -10,
        -20, -10, -30, -10, -10, -30, -10, -20
    };

    int[] RookOpeningSquareTable = {
        0, 0, 0, 5, 5, 0, 0, 0,
        5, 10, 10, 10, 10, 10, 10, 5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        0, 0, 0, 0, 0, 0, 0, 0,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 5, 10, 15, 15, 10, 5, -5
    };

    int[] QueenOpeningSquareTable = {
        -20, -10, -10, -5, -5, -10, -10, -20,
        -10, 0, 0, 0, 0, 0, 0, -10,
        -10, 0, 5, 5, 5, 5, 0, -10,
        -5, 10, 15, 15, 15, 15, 10, -5,
        10, 10, 15, 15, 15, 15, 10, 10,
        -10, 15, 15, 15, 15, 15, 10, -10,
        -10, 10, 15, 10, 10, 10, 10, -10,
        -20, -10, -10, -15, -15, -10, -10, -20
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

    int[] OpponentPawnSquareTable;
    int[] OpponentKnightSquareTable;
    int[] OpponentBishopSquareTable;
    int[] OpponentRookSquareTable;
    int[] OpponentQueenSquareTable;
    int[] OpponentKingSquareTable;
    int[] OpponentOpeningPawnSquareTable;
    int[] OpponentOpeningKnightSquareTable;
    int[] OpponentOpeningBishopSquareTable;
    int[] OpponentOpeningRookSquareTable;
    int[] OpponentOpeningQueenSquareTable;
    int[] OpponentOpeningKingSquareTable;
    int[] OpponentKingEndgameSquareTable;
    bool PreInitFinished = false;
    void PreInit() {
        OpponentPawnSquareTable = PawnSquareTable.Reverse().ToArray();
        OpponentKnightSquareTable = KnightSquareTable.Reverse().ToArray();
        OpponentBishopSquareTable = BishopSquareTable.Reverse().ToArray();
        OpponentRookSquareTable = RookSquareTable.Reverse().ToArray();
        OpponentQueenSquareTable = QueenSquareTable.Reverse().ToArray();
        OpponentKingSquareTable = KingSquareTable.Reverse().ToArray();
        OpponentOpeningPawnSquareTable = PawnOpeningSquareTable.Reverse().ToArray();
        OpponentOpeningKnightSquareTable = KnightOpeningSquareTable.Reverse().ToArray();
        OpponentOpeningBishopSquareTable = BishopOpeningSquareTable.Reverse().ToArray();
        OpponentOpeningRookSquareTable = RookOpeningSquareTable.Reverse().ToArray();
        OpponentOpeningQueenSquareTable = QueenOpeningSquareTable.Reverse().ToArray();
        OpponentOpeningKingSquareTable = KingOpeningSquareTable.Reverse().ToArray();
        OpponentKingEndgameSquareTable = KingEndgameSquareTable.Reverse().ToArray();
        PreInitFinished = true;
    }

    public Move Think(Board board, Timer timer) {
        if (!PreInitFinished) {
            PreInit();
        }
        // this is the only function called from this class.
        BestEvalThisIteration = BestEval = 0;
        BestMoveThisIteration = BestMove = Move.NullMove; 
        Search(board, Depth, int.MinValue, int.MaxValue);
        BestMove = BestMoveThisIteration;
        BestEval = BestEvalThisIteration;
        Console.WriteLine($"Best move {BestMove} with evaluation {BestEval}");
        Console.WriteLine($"Game Weight: {GamePhase}");
        return BestMove;
    }
    int GetPieceValue(PieceType piece) {
        // gets the value of the piece
        return piece switch {
            PieceType.Pawn => PawnValue,
            PieceType.Knight => KnightValue,
            PieceType.Bishop => BishopValue,
            PieceType.Rook => RookValue,
            PieceType.Queen => QueenValue,
            _ => 0,
        };

    }
    void DetermineGamePhase(Board board) {
        // how strong the weight should be in determining evaluation during the game
        int MaterialLeft = CountMaterial(board, true) + CountMaterial(board, false);
        // Endgame Function for considering endgame values
        // I want this to be more of a smooth interpolation because that would
        // suit the bot better
        if (MaterialLeft >= 5850) {
            GamePhase = GamePhases.OPENING;
        } else if (MaterialLeft < 5850 && MaterialLeft >= 2730) {
            GamePhase = GamePhases.MIDDLEGAME;
        } else {
            GamePhase = GamePhases.ENDGAME;
        }
    }
    
    int CountMaterial(Board board, bool isWhite) {
        //Counts the material for one side and returns the total value of the pieces
        int material = 0;
        material += board.GetPieceList(PieceType.Pawn, isWhite).Count * PawnValue;
        material += board.GetPieceList(PieceType.Knight, isWhite).Count * KnightValue;
        material += board.GetPieceList(PieceType.Bishop, isWhite).Count * BishopValue;
        material += board.GetPieceList(PieceType.Rook, isWhite).Count * RookValue;
        material += board.GetPieceList(PieceType.Queen, isWhite).Count * QueenValue;
        return material;
    }
    int ForceKingToCornerEndgameEvaluation(Board board) {
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
        return Eval * (GamePhase == GamePhases.ENDGAME ? 2 : 1);
    }
    int[] GetSquareTable(GamePhases phase, bool isSelf, PieceType type) {
        switch (type) {
            case PieceType.Pawn:
                return isSelf
                    ? (phase == GamePhases.OPENING
                        ? PawnOpeningSquareTable
                        : PawnSquareTable)
                    : (phase == GamePhases.OPENING
                        ? OpponentOpeningPawnSquareTable
                        : OpponentPawnSquareTable);

            case PieceType.Knight:
                return isSelf
                    ? (phase == GamePhases.OPENING
                        ? KnightOpeningSquareTable
                        : KnightSquareTable)
                    : (phase == GamePhases.OPENING
                        ? OpponentOpeningKnightSquareTable
                        : OpponentKnightSquareTable);

            case PieceType.Bishop:
                return isSelf
                    ? (phase == GamePhases.OPENING
                        ? BishopOpeningSquareTable
                        : BishopSquareTable)
                    : (phase == GamePhases.OPENING
                        ? OpponentOpeningBishopSquareTable
                        : OpponentBishopSquareTable);

            case PieceType.Rook:
                return isSelf
                    ? (phase == GamePhases.OPENING
                        ? RookOpeningSquareTable
                        : RookSquareTable)
                    : (phase == GamePhases.OPENING
                        ? OpponentOpeningRookSquareTable
                        : OpponentRookSquareTable);

            case PieceType.Queen:
                return isSelf
                    ? (phase == GamePhases.OPENING
                        ? QueenOpeningSquareTable
                        : QueenSquareTable)
                    : (phase == GamePhases.OPENING
                        ? OpponentOpeningQueenSquareTable
                        : OpponentQueenSquareTable);

            case PieceType.King:
                return isSelf
                    ? (phase == GamePhases.OPENING
                        ? KingOpeningSquareTable
                        : KingSquareTable)
                    : (phase == GamePhases.OPENING
                        ? OpponentOpeningKingSquareTable
                        : OpponentKingSquareTable);

            default:
                throw new ArgumentException("Invalid piece type");
        }
    }
    int GetSquareTableValue(Board board, Piece piece, int squareIndex, GamePhases phase) {
        int[] squareTable = GetSquareTable(phase, board.IsWhiteToMove, piece.PieceType);
        return squareTable[squareIndex]; 
    }
    public int Evaluate(Board board) {
        // looks at both piece material values for white and black and returns
        // white - black and changed for whether the bot is playing black or white
        int Eval = 0;

        int whiteMaterial = CountMaterial(board, board.IsWhiteToMove);
        int blackMaterial = -CountMaterial(board, !board.IsWhiteToMove);
        Eval += whiteMaterial + blackMaterial;

        DetermineGamePhase(board);
        Eval += ForceKingToCornerEndgameEvaluation(board);

        if (GamePhase == GamePhases.OPENING) {
            Eval += DeterminePieceDevelopmentBonuses();
        }

        foreach (var pieceList in board.GetAllPieceLists()) {
            foreach (var piece in pieceList) {
                Eval += GetSquareTableValue(board, piece, piece.Square.Index, GamePhase) * (board.IsWhiteToMove ? 1 : -1);
            }
        }

        return Eval;
    }

    public int DeterminePieceDevelopmentBonuses() {
        return 0;
    }

    public void OrderMoves(Board board, Move[] moves) {
        // guesses how good a move is
        // orders moves by whichever have the highest scores
        // else - penalizes player for moving piece to a square attacked by an opponent
        int[] scores = new int[moves.Length];
        for (int i = 0; i < moves.Length; i++) {
            int score = 0;
            PieceType moveType = board.GetPiece(moves[i].StartSquare).PieceType;
            PieceType captureType = board.GetPiece(moves[i].TargetSquare).PieceType;
            if (captureType != PieceType.None) {
                if (board.SquareIsAttackedByOpponent(moves[i].TargetSquare)) {
                    int NetValue = GetPieceValue(moveType) - GetPieceValue(captureType);
                    if (NetValue >= 0) {
                        score -= 10 * GetPieceValue(captureType);
                    } else {
                        score += 10 * NetValue;
                    }
                } else {
                    score -= 10 * GetPieceValue(captureType);
                }
            }
            if (moveType == PieceType.Pawn && moves[i].IsPromotion) {
                score -= GetPieceValue(moves[i].PromotionPieceType);
            } else {
                if (board.SquareIsAttackedByOpponent(moves[i].TargetSquare)) {
                    score += GetPieceValue(moveType);
                }
            }
            scores[i] = score;
        }
        Array.Sort(scores, moves);
    }
    public int Search(Board board, int depth, int alpha, int beta) {
        if (depth == 0) {
            return Evaluate(board);
        }
        Move[] moves = board.GetLegalMoves();
        OrderMoves(board, moves);
        if (moves.Length == 0) {
            if (board.IsInCheck()) {
                return Evaluate(board) + -ImmediateMateScore;
            }
            if (board.IsInCheckmate()) {
                return -MateScore;
            }
            return 0;
        }
        int BestScore = int.MinValue;
        Move BestMoveTemp = Move.NullMove;
        for (int i = 0; i < moves.Length; i++) {
            board.MakeMove(moves[i]);
            int score = -Search(board, depth - 1, -beta, -alpha);
            board.UndoMove(moves[i]);
            if (score > BestScore) {
                BestScore = score;
                BestMoveTemp = moves[i];
            }
            if (score > alpha) {
                alpha = score;

            }
            if (alpha >= beta) {
                break;
            }
        }
        BestMoveThisIteration = BestMoveTemp;
        BestEvalThisIteration = BestScore;
        return alpha;
    }
}