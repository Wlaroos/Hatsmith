[System.Serializable]
public abstract class HatTrigger
{
    public abstract void Initialize(HatInstance hat);
    public abstract void Terminate(HatInstance hat);
}