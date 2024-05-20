[System.Serializable]
public class StatAffecter
{
    public string name;
    /// <summary>
    /// What do you think
    /// </summary>
    public string description;
    /// <summary>
    /// Affected stat
    /// </summary>
    public Stat stat;
    /// <summary>
    /// How much the stat increases or decreases per second
    /// </summary>
    public float changeRate;
    /// <summary>
    /// The amount of time the affecter has left, display as percent per second
    /// </summary>
    public float timeLeft;
    /// <summary>
    /// If the affecter lasts forever
    /// </summary>
    public bool isInfinite = false;
    public bool display = true;

    public enum Stat
    {
        Health = 0,
        Nutrition = 1,
        Sanity = 2,
    }

    public StatAffecter(string name, string description, Stat stat, float changeRate, float timeLeft, bool isInfinite = false, bool display = false)
    {
        this.name = name;
        this.description = description;
        this.stat = stat;
        this.changeRate = changeRate;
        this.timeLeft = timeLeft;
        this.isInfinite = isInfinite;
        this.display = display; 
    }
}
