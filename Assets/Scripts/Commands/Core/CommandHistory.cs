using System.Collections.Generic;
using System.IO;
using Sirenix.Serialization;
using UnityEngine;

namespace Framework.Commands.Core
{
    public class CommandHistory
    {
        private CommandManager commandManager { get; }

        public CommandHistory(CommandManager commandManager)
        {
            this.commandManager = commandManager;
            commandManager.OnSaveEvt += Save;
            commandManager.OnLoadEvt += Load;
        }

        private List<string> _history = new List<string>();

        private int _index = -1;

        public string NextCommandLine()
        {
            if (_history.Count == 0)
                return null;

            _index--;
            if (_index < 0)
                _index = _history.Count - 1;
            return _history[_index];
        }

        public string PreviousCommandLine()
        {
            if (_history.Count == 0)
                return null;

            _index++;
            if (_index >= _history.Count)
                _index = 0;
            return _history[_index];
        }


        public void AddHistory(string command)
        {
            // if (_history.Count > 0)
            // {
            //     if (_history[^1] == command)
            //         return;
            // }

            _history.Remove(command);
            _history.Insert(0, command);

            _index = -1;

            if (_history.Count > 50)
            {
                while (_history.Count > 50)
                    _history.RemoveAt(_history.Count - 1);
            }
        }

        string filePath => Path.Combine(Application.persistentDataPath, "command_history.dat");

        public void Save()
        {
            var data = SerializationUtility.SerializeValue(_history, DataFormat.Binary);
            File.WriteAllBytes(filePath, data);
            // ES3.Save("history", _history, CommandManager.CommandSaveFileName);
        }

        public void Load()
        {
            // if (ES3.KeyExists("history", CommandManager.CommandSaveFileName))
            //     _history = ES3.Load<List<string>>("history", CommandManager.CommandSaveFileName);

            if (File.Exists(filePath))
            {
                var data = File.ReadAllBytes(filePath);
                var h = SerializationUtility.DeserializeValue<List<string>>(data, DataFormat.Binary);
                if (h != null)
                    _history = h;
            }
        }

        public void ResetIndex()
        {
            _index = -1;
        }
    }
}