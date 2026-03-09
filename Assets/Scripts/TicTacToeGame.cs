using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
// mY NETWORK GAME OF TIC TAC TOE
public class TicTacToeGame : NetworkBehaviour
{
    // VARBLES I USED
    public NetworkList<int> Board;

    public NetworkVariable<int> CurrentMark = new NetworkVariable<int>(1); // X starts
    public NetworkVariable<int> XScore = new NetworkVariable<int>(0);
    public NetworkVariable<int> OScore = new NetworkVariable<int>(0);

    public NetworkVariable<FixedString64Bytes> Status =
        new NetworkVariable<FixedString64Bytes>(new FixedString64Bytes("Waiting for players..."));

   // MY PLAYERS 
    private ulong xClientId = ulong.MaxValue;
    private ulong oClientId = ulong.MaxValue;

    private bool roundOver = false;

    public string StatusText => Status.Value.ToString();
    public int XScoreValue => XScore.Value;
    public int OScoreValue => OScore.Value;

    public override void OnNetworkSpawn()
    {
        if (Board == null)
            Board = new NetworkList<int>();

        if (IsServer)
        {
            AssignPlayersIfPossible();
            ResetBoardServer();
        }
    }
// this will assign X and O 
    private void AssignPlayersIfPossible()
    {
        
        var ids = NetworkManager.Singleton.ConnectedClientsIds;

        if (ids.Count >= 1) xClientId = ids[0];
        if (ids.Count >= 2) oClientId = ids[1];
    }

    private void ResetBoardServer()
    {
        Board.Clear();
        for (int i = 0; i < 9; i++) Board.Add(0);

        roundOver = false;
        CurrentMark.Value = 1;
        UpdateStatusServer();
    }

    public void TryPlaceMark(int index)
    {
        PlaceMarkServerRpc(index);
    }

    [Rpc(SendTo.Server)]
    private void PlaceMarkServerRpc(int index, RpcParams rpcParams = default)
    {
        if (!IsServer) return;
        if (roundOver) return;
        if (index < 0 || index > 8) return;
        if (Board[index] != 0) return;

       
        if (NetworkManager.Singleton.ConnectedClientsIds.Count < 2)
        {
            Status.Value = new FixedString64Bytes("Need 2 players.");
            return;
        }

        if (xClientId == ulong.MaxValue || oClientId == ulong.MaxValue)
            AssignPlayersIfPossible();

        ulong senderId = rpcParams.Receive.SenderClientId;

       
        if (CurrentMark.Value == 1 && senderId != xClientId)
        {
            Status.Value = new FixedString64Bytes("Not your turn (X).");
            return;
        }
        if (CurrentMark.Value == 2 && senderId != oClientId)
        {
            Status.Value = new FixedString64Bytes("Not your turn (O).");
            return;
        }

       
        Board[index] = CurrentMark.Value;

        int winner = CheckWinner();
        if (winner != 0)
        {
            roundOver = true;
            if (winner == 1) XScore.Value++;
            else OScore.Value++;

            Status.Value = new FixedString64Bytes(winner == 1 ? "X Wins!" : "O Wins!");
            return;
        }

        if (IsBoardFull())
        {
            roundOver = true;
            Status.Value = new FixedString64Bytes("Draw!");
            return;
        }

        
        CurrentMark.Value = CurrentMark.Value == 1 ? 2 : 1;
        UpdateStatusServer();
    }
// change what the stats is 
    private void UpdateStatusServer()
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count < 2)
        {
            Status.Value = new FixedString64Bytes("Waiting for players...");
            return;
        }

        Status.Value = new FixedString64Bytes(CurrentMark.Value == 1 ? "X Turn" : "O Turn");
    }

    public string GetCellLabel(int index)
    {
        if (Board == null || Board.Count < 9) return "";
        return Board[index] == 1 ? "X" : Board[index] == 2 ? "O" : "";
    }
// chewck if all boxes are full 
    private bool IsBoardFull()
    {
        for (int i = 0; i < 9; i++)
            if (Board[i] == 0) return false;
        return true;
    }
// win conditions 
    private int CheckWinner()
    {
        int[,] lines = {
            {0,1,2},{3,4,5},{6,7,8},
            {0,3,6},{1,4,7},{2,5,8},
            {0,4,8},{2,4,6}
        };

        for (int i = 0; i < lines.GetLength(0); i++)
        {
            int a = lines[i,0], b = lines[i,1], c = lines[i,2];
            int v = Board[a];
            if (v != 0 && v == Board[b] && v == Board[c])
                return v;
        }
        return 0;
    }
}