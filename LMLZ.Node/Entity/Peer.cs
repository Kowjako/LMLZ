namespace LMLZ.Node.Entity;

public record Peer (int Id, string IP, int Port)
{
    public override string ToString() => $"{IP}:{Port}";
}