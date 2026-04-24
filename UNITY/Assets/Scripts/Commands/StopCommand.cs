public class StopCommand : ICommand
{
    private ITankComponents _tank;
    public bool IsComplete => true;

    public StopCommand(ITankComponents tank)
    {
        _tank = tank;
    }

    public void Start() 
    {
        _tank.Controller.Stop(); 
    }
    public void Tick() { }
    public void Cancel() { }
}
