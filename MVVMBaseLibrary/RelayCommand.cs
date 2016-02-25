using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MVVMBaseLibrary
{
    public class RelayCommand : ICommand
    {
        readonly Action<object> m_execute;
        readonly Predicate<object> m_canExecute;

        public RelayCommand(Action<object> execute): this(execute,null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            m_execute = execute;
            m_canExecute = canExecute;

        }

        public bool CanExecute(object parameters)
        {
            return m_canExecute==null? true:m_canExecute(parameters);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameters)
        {
            if (parameters != null)
            {
                m_execute(parameters);
            }
            else
            {
                m_execute("No action speciied");
            }
        }
    }
}
