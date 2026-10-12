public class TicTacToeEngine
{
    // 0 = Empty, 1 = P1 (X), 2 = P2 (O)
    public int[,] Board { get; private set; } = new int[3, 3];
    public int CurrentTurn { get; private set; } = 1;

    public event Action<int>? OnGameWon; // Returns winning player ID
    public event Action? OnDraw;

    public bool MakeMove(int row, int col, int playerId)
    {
        if (row < 0 || row > 2 || col < 0 || col > 2 || Board[row, col] != 0) return false;
        if (playerId != CurrentTurn) return false;

        Board[row, col] = playerId;

        if (CheckWin(playerId))
        {
            OnGameWon?.Invoke(playerId);
        }
        else if (IsDraw())
        {
            OnDraw?.Invoke();
        }
        else
        {
            CurrentTurn = CurrentTurn == 1 ? 2 : 1;
        }
        return true;
    }

    private bool CheckWin(int p)
    {
        // Rows & Cols
        for (int i = 0; i < 3; i++)
        {
            if (Board[i, 0] == p && Board[i, 1] == p && Board[i, 2] == p) return true;
            if (Board[0, i] == p && Board[1, i] == p && Board[2, i] == p) return true;
        }
        // Diagonals
        if (Board[0, 0] == p && Board[1, 1] == p && Board[2, 2] == p) return true;
        if (Board[0, 2] == p && Board[1, 1] == p && Board[2, 0] == p) return true;

        return false;
    }

    private bool IsDraw()
    {
        foreach (var cell in Board)
            if (cell == 0) return false;
        return true;
    }
    public void Reset()
    {
        // Clear the 2D array back to zeros
        Board = new int[3, 3];
        // Reset turn back to Player 1
        CurrentTurn = 1;
    }
}