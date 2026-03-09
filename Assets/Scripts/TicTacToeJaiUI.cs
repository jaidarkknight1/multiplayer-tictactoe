using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class TicTacToeJaiUI : MonoBehaviour
{
    [Header("LAN Connect Settings")]
    public string connectAddress = "127.0.0.1"; // default ip (when you wanna runt he game on the same device and on two termianls )
    public ushort connectPort = 7777;          // default port i used

    private TicTacToeGame game;

    void Start()
    {
        game = FindFirstObjectByType<TicTacToeGame>(); 
    }

    void OnGUI()
    {
        int x = 20;
        int y = 20;

        GUI.Label(new Rect(x, y, 400, 25), "Multiplayer Tic-Tac-Toe");
        y += 30;

        if (NetworkManager.Singleton == null)
        {
            GUI.Label(new Rect(x, y, 600, 25), "NetworkManager not found in scene.");
            return;
        }

        // this will show the start buttons used before two players are connetced to each other in my game 
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            GUI.Label(new Rect(x, y, 100, 25), "Address:");
            // base case is 127.0.0.1 (local ip) but player can type a different ip here (like hotspot / lan ip)
            connectAddress = GUI.TextField(new Rect(x + 100, y, 200, 25), connectAddress);
            y += 30;

            
            GUI.Label(new Rect(x, y, 600, 25),
                "127.0.0.1 = same device, use host PC IP for hotspot/wifi (like 192.168.x.x)");
            y += 25;

            GUI.Label(new Rect(x, y, 100, 25), "Port:");
            string portStr = GUI.TextField(new Rect(x + 100, y, 200, 25), connectPort.ToString());
            if (ushort.TryParse(portStr, out var parsedPort))
                connectPort = parsedPort;
            y += 40;

            if (GUI.Button(new Rect(x, y, 240, 30), "Start Host"))
            {
                SetTransport();                    
                NetworkManager.Singleton.StartHost();
            }
            y += 35;

            if (GUI.Button(new Rect(x, y, 240, 30), "Start Client"))
            {
                SetTransport();                    
                NetworkManager.Singleton.StartClient();
            }
            y += 35;

            if (GUI.Button(new Rect(x, y, 240, 30), "Start Server"))
            {
                SetTransport();                    
                NetworkManager.Singleton.StartServer();
            }

            return;
        }

        // show connected and find game if needed
        if (game == null)
            game = FindFirstObjectByType<TicTacToeGame>();

        if (game == null)
        {
            GUI.Label(new Rect(x, y, 600, 25), "TicTacToeGame not found in scene.");
            return;
        }

        if (!game.IsSpawned)
        {
            GUI.Label(new Rect(x, y, 600, 25), "Waiting for TicTacToeGame to spawn...");
            return;
        }

        y += 10;

        GUI.Label(new Rect(x, y, 600, 25), "Status: " + game.StatusText);
        y += 25;

        GUI.Label(new Rect(x, y, 600, 25), $"Score  X: {game.XScoreValue}   O: {game.OScoreValue}");
        y += 40;

        int size = 60;

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int index = row * 3 + col;
                string label = game.GetCellLabel(index);

                Rect rect = new Rect(x + col * size, y + row * size, size, size);

                if (GUI.Button(rect, label))
                {
                    game.TryPlaceMark(index);
                }
            }
        }
    }

    private void SetTransport()
    {
        // this will set the IP and port that the network will use
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData(connectAddress, connectPort);
        }
        else
        {
            Debug.LogError("UnityTransport component not found on NetworkManager.");
        }
    }
}
