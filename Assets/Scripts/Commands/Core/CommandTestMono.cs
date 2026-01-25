using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.Commands.Core
{
    public class CommandTestMono : MonoBehaviour
    {
        private CommandManager commandManager { set; get; }

        [SerializeField] private string _content;
        [SerializeField] private int _editIndex;

        public void Init()
        {
            commandManager = new CommandManager();
            commandManager.ScanCommand();
            
        }

        [Button]
        public void Suggest()
        {
           
        }

        [Button]
        public void Submit()
        {
            if (commandManager == null)
                Init();

            commandManager.Execute(_content);
        }
    }
}