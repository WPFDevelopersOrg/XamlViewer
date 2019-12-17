using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;

namespace XamlService.Commands
{
    public interface IApplicationCommands
    {
        //Toolbar
        CompositeCommand NewCommand { get; }
        CompositeCommand OpenCommand { get; } 
        CompositeCommand SaveCommand { get; }
        CompositeCommand SaveAllCommand { get; }

        CompositeCommand UndoCommand { get; }
        CompositeCommand RedoCommand { get; }

        CompositeCommand CompileCommand { get; }

        //
        CompositeCommand RefreshCommand { get; }
        CompositeCommand HelpCommand { get; }
    }

    public class ApplicationCommands : IApplicationCommands
    {
        private CompositeCommand _newCommand = new CompositeCommand();
        public CompositeCommand NewCommand
        {
            get { return _newCommand; }
        }

        private CompositeCommand _openCommand = new CompositeCommand();
        public CompositeCommand OpenCommand
        {
            get { return _openCommand; }
        } 

        private CompositeCommand _saveCommand = new CompositeCommand();
        public CompositeCommand SaveCommand
        {
            get { return _saveCommand; }
        }

        private CompositeCommand _saveAllCommand = new CompositeCommand();
        public CompositeCommand SaveAllCommand
        {
            get { return _saveAllCommand; }
        }

        private CompositeCommand _undoCommand = new CompositeCommand();
        public CompositeCommand UndoCommand
        {
            get { return _undoCommand; }
        }
		
        private CompositeCommand _redoCommand = new CompositeCommand();
        public CompositeCommand RedoCommand
        {
            get { return _redoCommand; }
        }

        private CompositeCommand _compileCommand = new CompositeCommand();
        public CompositeCommand CompileCommand
        {
            get { return _compileCommand; }
        }

        private CompositeCommand _refreshCommand = new CompositeCommand();
        public CompositeCommand RefreshCommand
        {
            get { return _refreshCommand; }
        }

        private CompositeCommand _helpCommand = new CompositeCommand();
        public CompositeCommand HelpCommand
        {
            get { return _helpCommand; }
        }
    }
}
