public interface ISoldierFollowerRegistratorProvider
{
    public bool RegisterSoldier(SoldierFollower soldier);
    public void UnregisterSoldier(SoldierFollower soldier);

}