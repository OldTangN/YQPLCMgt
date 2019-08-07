using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace YQPLCMgt.UI.ViewModel
{
    public class RelayCommand : ICommand
    {
        private Action _execute;
        private Func<bool> _canExecute;

        public RelayCommand(Action exec)
        {
            this._execute = exec;
        }

        public RelayCommand(Action exec, Func<bool> canExec)
        {
            this._execute = exec;
            this._canExecute = canExec;
        }

        private event EventHandler CanExecuteChangedInternal;//事件
        public event EventHandler CanExecuteChanged        //CanExecuteChanged事件处理方法
        {
            add
            {
                CommandManager.RequerySuggested += value;
                this.CanExecuteChangedInternal += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                this.CanExecuteChangedInternal -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
            {
                return _canExecute.Invoke();
            }
            return true;
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke();
        }
    }

    public class RelayCommand<T> : ICommand where T : class
    {
        private Action<T> _execute;
        private Func<T, bool> _canExecute;
        public RelayCommand(Action<T> exec)
        {
            this._execute = exec;
        }
        public RelayCommand(Action<T> exec, Func<T, bool> canExec)
        {
            this._execute = exec;
            this._canExecute = canExec;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
            {
                return _canExecute.Invoke(parameter as T);
            }
            return true;
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke(parameter as T);
        }
    }
}
