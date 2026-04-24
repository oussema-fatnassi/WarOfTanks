public interface ICommand
{
    void Start();
    void Tick();
    void Cancel();
    bool IsComplete { get; }
}
