public class Ownable {
    public Player Owner { get; set; }

    public bool Is_Owned_By(Player player)
    {
        if(Owner == null) {
            return false;
        }
        return Owner.Id == player.Id;
    }
    
    public bool Is_Owned_By_Current_Player
    {
        get {
            return Is_Owned_By(Main.Instance.Current_Player);
        }
    }

    public bool Has_Owner
    {
        get {
            return Owner != null;
        }
    }
}
