public interface ICommandReceiver
{
    void SetCommand(ICommand command);
    void CancelCommand();
    ICommand CurrentCommand { get; }
}
