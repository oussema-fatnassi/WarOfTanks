using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using UnityEngine;

public interface ICommandReceiver
{
    void SetCommand(ICommand command);
    void CancelCommand();
    ICommand CurrentCommand { get; }
}
