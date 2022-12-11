using System;

public static class PlayerCache {
    public static PlayerCore CurrentPlayer { get; private set; }

    public static void SetCurrentPlayer(PlayerCore player) {
        CurrentPlayer = player;
    }

    public static PlayerCore CreateAndSetNewPlayer(string playerName) {
        var newPlayer = new PlayerCore(playerName);
        CurrentPlayer = newPlayer;
        return CurrentPlayer;
    }
}

public class PlayerCore : IPlayerCore {
    public string Name { get; private set; }
    public Guid Guid { get; private set; }

    public PlayerCore(string name) {
        this.Name = name;
        Guid = Guid.NewGuid();
    }

    public PlayerCore(string name, Guid guid) : this(name) {
        Guid = guid;
    }
}

public interface IPlayerCore {
    public string Name { get; }
    public Guid Guid { get; }
}
