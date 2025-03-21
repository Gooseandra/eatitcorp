using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class NetworkManager : MonoBehaviour
{
    [SerializeField] private string playerId = "Player1";
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform clientTransfotm;
    private WebSocket ws;

    private Dictionary<string, PlayerObject> playerDirectory = new Dictionary<string, PlayerObject>();
    private Queue<System.Action> mainThreadActions = new Queue<System.Action>();

    private void Start()
    {
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        ws = new WebSocket("ws://localhost:8080/ws");

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("Connected to server.");
            SendPlayerMessage("connect", new PositionData());
        };

        ws.OnMessage += (sender, e) =>
        {
            lock (mainThreadActions)
            {
                mainThreadActions.Enqueue(() => HandleServerMessage(e.Data));
            }
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("Connection closed: " + e.Reason);
        };

        ws.Connect();

        InvokeRepeating(nameof(SendPositionUpdate), 0f, 0.015625f);
    }

    private void Update()
    {
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count > 0)
            {
                var action = mainThreadActions.Dequeue();
                action?.Invoke();
            }
        }

        foreach (var player in playerDirectory.Values)
        {
            if (player.GameObject != null)
            {
                player.GameObject.transform.position = Vector3.Lerp(
                    player.GameObject.transform.position,
                    player.TargetPosition,
                    Time.deltaTime * player.InterpolationSpeed);
            }
        }
    }

    private void SendPositionUpdate()
    {
        if (ws.ReadyState == WebSocketState.Open)
        {
            var pos = new PositionData
            {
                x = clientTransfotm.position.x,
                y = clientTransfotm.position.y,
                z = clientTransfotm.position.z
            };

            SendPlayerMessage("position", pos);
        }
    }

    private void HandleServerMessage(string jsonData)
    {
        try
        {
            Debug.Log("Received JSON: " + jsonData);

            PlayerDataListWrapper playerDataList = JsonUtility.FromJson<PlayerDataListWrapper>(jsonData);

            foreach (var playerData in playerDataList.players)
            {
                if (playerData.id == playerId) continue;

                if (!playerDirectory.ContainsKey(playerData.id))
                {
                    AddPlayerToDirectory(playerData);
                }
                else
                {
                    UpdatePlayerPosition(playerData);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error parsing server message: " + ex.Message);
        }
    }

    private void AddPlayerToDirectory(PlayerData playerData)
    {
        Debug.Log($"Adding player: {playerData.id}");
        GameObject newPlayer = Instantiate(playerPrefab);
        newPlayer.name = playerData.id;
        newPlayer.transform.position = new Vector3(playerData.position.x, playerData.position.y, playerData.position.z);

        playerDirectory.Add(playerData.id, new PlayerObject
        {
            GameObject = newPlayer,
            TargetPosition = new Vector3(playerData.position.x, playerData.position.y, playerData.position.z)
        });
    }

    private void UpdatePlayerPosition(PlayerData playerData)
    {
        if (playerDirectory.TryGetValue(playerData.id, out PlayerObject playerObj))
        {
            Debug.Log($"Updating position for player {playerData.id} to ({playerData.position.x}, {playerData.position.y}, {playerData.position.z})");
            playerObj.TargetPosition = new Vector3(playerData.position.x, playerData.position.y, playerData.position.z);
        }
        else
        {
            Debug.LogWarning($"Player {playerData.id} not found in directory for position update.");
        }
    }

    private void SendPlayerMessage(string messageType, PositionData pos)
    {
        PlayerData messageData = new PlayerData
        {
            id = playerId,
            message = messageType,
            position = pos
        };
        string jsonMessage = JsonUtility.ToJson(messageData);
        ws.Send(jsonMessage);
    }

    private void OnApplicationQuit()
    {
        if (ws != null && ws.ReadyState == WebSocketState.Open)
        {
            SendPlayerMessage("disconnect", new PositionData());
            ws.Close();
        }
    }

    [System.Serializable]
    private class PlayerData
    {
        public string message;
        public string id;
        public PositionData position;
    }

    [System.Serializable]
    private class PositionData
    {
        public float x;
        public float y;
        public float z;
    }

    [System.Serializable]
    private class PlayerDataListWrapper
    {
        public List<PlayerData> players;
    }

    private class PlayerObject
    {
        public GameObject GameObject { get; set; }
        public Vector3 TargetPosition { get; set; }
        public float InterpolationSpeed { get; set; } = 50f;
    }
}
