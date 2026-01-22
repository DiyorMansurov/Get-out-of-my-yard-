public interface IUpgradable
{
    bool CanUpgrade();
    int UpgradeCost();
    void Upgrade();
}
