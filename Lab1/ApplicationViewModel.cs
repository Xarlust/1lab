using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Data.SQLite;
using Dapper;
using System.Windows.Data;
using System.Globalization;

namespace Lab1
{
    public enum States { NotCompleted, Completed };

    internal class ApplicationViewModel : INotifyPropertyChanged
    {
        string connectionString = @"Data Source=.\ToDoListBase.db";

        public ObservableCollection<Task> tasksCompleted;
        public ObservableCollection<Task> tasksNotCompleted;

        public ObservableCollection<Task> TasksCompleted
        {
            get { return tasksCompleted; }
            set
            {
                tasksCompleted = value;
                OnPropertyChanged("TasksComplete");
            }
        }

        public ObservableCollection<Task> TasksNotCompleted
        {
            get { return tasksNotCompleted; }
            set
            {
                tasksNotCompleted = value;
                OnPropertyChanged("TasksNotComplete");
            }

        }

        private Task selectedTask;
        public Task SelectedTask
        {
            get { return selectedTask; }
            set
            {
                selectedTask = value;
                OnPropertyChanged("SelectedTask");
            }
        }

        public ApplicationViewModel()
        {
            TasksCompleted = new ObservableCollection<Task>();
            TasksNotCompleted = new ObservableCollection<Task>();


            var connection = new SQLiteConnection(connectionString);
            connection.Open();

            connection.Query<Task>("CREATE TABLE IF NOT EXISTS \"Tasks\" (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, \"Name\"  TEXT, \"Discription\"   TEXT, \"State\" TEXT)");

            var TasksTemp = connection.Query<Task>("SELECT * FROM Tasks");
            foreach (Task localtask in TasksTemp)
            {
                if (localtask.State == States.NotCompleted) TasksNotCompleted.Add(localtask);
                else TasksCompleted.Add(localtask);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private RelayCommand addCommand;

        public RelayCommand AddCommand
        {
            get
            {
                return addCommand ??
                    (addCommand = new RelayCommand(obj =>
                    {
                        Task newTask = new Task();
                        newTask.Name = "Название задания";
                        newTask.Discription = "Описание задания";

                        var connection = new SQLiteConnection(connectionString);
                        connection.Open();

                        connection.Query<Task>("INSERT INTO Tasks (Name, Discription, State) VALUES('Новое задание', 'Описание', 0)");
                        var id = connection.Query<long>("SELECT last_insert_rowid();");

                        foreach (var result in id)
                            newTask.Id = result;

                        TasksNotCompleted.Insert(0, newTask);
                        SelectedTask = newTask;
                    }));
            }
        }


        private RelayCommand removeCommand;
        public RelayCommand RemoveCommand
        {
            get
            {
                return removeCommand ??
                    (removeCommand = new RelayCommand(obj =>
                    {
                        Task newTask = obj as Task;
                        if (newTask != null)
                        {
                            var connection = new SQLiteConnection(connectionString);
                            connection.Open();

                            connection.Query<Task>("DELETE FROM Tasks WHERE id = @taskId", new { taskId = newTask.Id });
                            if (newTask.State == States.NotCompleted) TasksNotCompleted.Remove(newTask);
                            else TasksCompleted.Remove(newTask);
                        }
                    },
                    (obj) => SelectedTask != null));
            }
        }

        private RelayCommand setState;
        public RelayCommand SetState
        {
            get
            {
                return setState ??
                    (setState = new RelayCommand(obj =>
                    {
                        var st = obj;

                        Task newTask = SelectedTask;
                        if (newTask != null)
                        {
                            var connection = new SQLiteConnection(connectionString);
                            connection.Open();

                            connection.Query<Task>("UPDATE Tasks SET State = @taskState WHERE id = @taskId", new { taskState = st, taskId = newTask.Id });

                            switch (newTask.State)
                            {
                                case States.NotCompleted:
                                    TasksNotCompleted.Remove(newTask);
                                    break;
                                case States.Completed:
                                    TasksCompleted.Remove(newTask);
                                    break;
                            }

                            switch (st)
                            {
                                case "0":
                                    TasksCompleted.Insert(0, newTask);
                                    newTask.State = States.Completed;
                                    break;
                                case "1":
                                    TasksNotCompleted.Insert(0, newTask);
                                    newTask.State = States.NotCompleted;
                                    break;

                            }
                        }
                    },
                    (obj) => SelectedTask != null));
            }
        }

        private RelayCommand saveCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ??
                    (saveCommand = new RelayCommand(obj =>
                    {
                        Task newTask = obj as Task;
                        if (newTask != null)
                        {
                            var connection = new SQLiteConnection(connectionString);
                            connection.Open();

                            connection.Query<Task>("UPDATE Tasks SET Name = @taskName, Discription = @taskDiscription, State = @taskState WHERE id = @taskId",
                                                    new { taskName = newTask.Name, taskDiscription = newTask.Discription, taskState = newTask.State, taskId = newTask.Id });
                        }
                    },
                    (obj) => (TasksCompleted.Any() || TasksNotCompleted.Any()) && SelectedTask != null));
            }
        }

    }

    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }

}