

public class Quest
{
    public string title;
    public string description;
    public int current;
    public int target;

    public bool IsCompleted => current >= target;

    public Quest(string title, string description, int target)
    {
        this.title = title;
        this.description = description;
        this.target = target;
        current = 0;
    }

    public void AddProgress(int amount = 1)
    {
        current += amount;
    }
}
