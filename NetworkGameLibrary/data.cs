namespace NetworkGameLibrary
{
    public enum PacketType
    {
        AcceptedConnection = 1,
        Disconnected,
        Login,
        NewPlayer,
        AllPlayers,
        Input,
        UPDATEPosition
    }
}