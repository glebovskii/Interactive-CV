using UnityEngine;

[CreateAssetMenu(fileName = "NetworkSessionConfig", menuName = "Networking/Network Session Config")]
public sealed class NetworkSessionConfig : ScriptableObject
{
    [SerializeField]
    private string defaultRoomName = "MainRoom";

    [SerializeField, Min(1)]
    private int maxPlayers = 20;

    public string DefaultRoomName => defaultRoomName;
    public int MaxPlayers => maxPlayers;
}