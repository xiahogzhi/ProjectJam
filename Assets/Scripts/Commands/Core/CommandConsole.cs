using System;
using System.Text;
using Framework.Commands.Suggest;
using Framework.Commands.View;
using Framework.Games;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework.Commands.Core
{
    public class CommandConsole : MonoBehaviour
    {
        [SerializeField] private ConsoleContent _consoleContent;
        [SerializeField] private TMP_InputField _input;

        [SerializeField] private RectTransform _popWindow;
        [SerializeField] private TextMeshProUGUI _popText;

        public static CommandConsole instance { set; get; }

        public CommandTextEditor commandTextEditor { get; } = new CommandTextEditor();

        public SuggestBuilder currentSuggestBuilder { private get; set; }
        public Command currentCommand { private get; set; }
        
        
        public event Action<bool> OnActiveChanged;


        // SuggestBuilder<ISuggestItem> GetParamSuggestItem()
        // {
        //     var cur = commandLine.command;
        //     if (cur == null)
        //         return null;
        //
        //     var paramIndex = GetEditParamIndex();
        //     for (int i = 0; i < cur.@params.Length; i++)
        //     {
        //         var p = cur.@params[i];
        //         if (i == paramIndex)
        //             return GetParamSuggestItem(p);
        //     }
        //
        //     return null;
        // }

        private bool _isCtrl;
        private bool _isShift;

        private bool _isActive;

        private int _lastCaretPosition;

        public bool isActive
        {
            get => _isActive;
            set
            {
                // if (value)
                // {
                //     if (!Game.launcher.useConsole)
                //         return;
                // }

                _isActive = value;

                _consoleContent.SetActive(value);
                if (value)
                {
                    ClearCommand();
                    _input.SetTextWithoutNotify(null);
                    _input.ActivateInputField();
                    // Game.inputSystem.DisableInput(token);
                    
                }
                else
                {
                    // Game.inputSystem.EnableInput(token);
                    CommandManager.instance.commandHistory.ResetIndex();
                }
                
                OnActiveChanged?.Invoke(value);
            }
        }

        private void Awake()
        {
            Application.logMessageReceived += MessageReceived;
            instance = this;
            CommandManager.instance.Load();
            CommandManager.instance.ScanCommand();
        }


        // private void FixedUpdate()
        // {
        //     if (_lastCaretPosition != _input.caretPosition)
        //     {
        //         _lastCaretPosition = _input.caretPosition;
        //         OnCaretPositionChanged();
        //     }
        // }
        //
        // void OnCaretPositionChanged()
        // {
        //     
        // }


        public void ExecuteCommand(string commandStr)
        {
            CommandManager.instance.Execute(commandStr);
        }

        private void Start()
        {

            _input.onSubmit.AddListener((x) =>
            {
                if (!string.IsNullOrEmpty(x))
                {
                    _input.SetTextWithoutNotify(null);
                    _input.ActivateInputField();
                    Submit(x);
                }
            });

            _input.onValueChanged.AddListener(OnInputTextChanged);
        }

        private void OnInputTextChanged(string text)
        {
            RefreshSuggest();
        }

        void SetPopActive(bool active)
        {
            _popWindow.gameObject.SetActive(active);
        }

        void SetPopText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                SetPopActive(false);
                return;
            }

            SetPopActive(true);
            _popText.text = text;
            _popWindow.sizeDelta = new Vector2(_popText.preferredWidth, _popText.preferredHeight);
        }

        void ClearCommand()
        {
            SetPopActive(false);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                _isShift = false;
                _isCtrl = false;
            }
        }

        void RefreshSuggest()
        {
            commandTextEditor.Update(_input.text, _input.stringPosition);
            var result = CommandManager.instance.GetCommandWithSuggest(commandTextEditor);

            currentSuggestBuilder = result.Item2;
            currentCommand = result.Item1;
            RefreshPopText();
        }

        void RefreshPopText()
        {
            StringBuilder sb = new StringBuilder();
            if (currentCommand != null)
                MakeCommandInfoPopText(sb, currentCommand, commandTextEditor.editingIndex - 1);

            if (currentSuggestBuilder != null)
            {
                sb.Append(currentSuggestBuilder.BuildText(commandTextEditor.TryGetCurrentData()));
            }

            SetPopText(sb.ToString());
        }

        void MakeCommandInfoPopText(StringBuilder sb, Command cur, int editParamIndex = -1)
        {
            if (cur == null)
                return;


            sb.Append($"<b><size=16><color=#6BFFF9>{cur.fullName}</color></size></b>");

            if (!string.IsNullOrEmpty(cur.attribute.comment))
            {
                var c = $"<size=13><i><color=#D9D9D9>  {cur.attribute.comment}</color></i></size>";
                sb.Append(c);
            }


            if (cur.@params != null)
            {
                for (int i = 0; i < cur.@params.Length; i++)
                {
                    var p = cur.@params[i];
                    //默认参数
                    string dv = string.Empty;
                    if (p.paramInfo.HasDefaultValue)
                    {
                        dv = $"  <i><b><color=#AED200>({p.paramInfo.DefaultValue})</color></b></i>";
                    }

                    //注释
                    string comment = string.Empty;
                    if (!string.IsNullOrEmpty(p.comment))
                        comment = $"<i><color=#7EFF6C>  {p.comment}</color></i>";


                    //选中效果
                    if (i == editParamIndex)
                    {
                        sb.Append($"\n  <b><size=14><color=#FFF46C> > {p.name}</color>{dv}</size></b>{comment}");
                    }
                    else
                    {
                        sb.Append($"\n  <size=14><color=#FFF46C>{p.name}</color>{dv}</size>{comment}");
                    }
                }

                // sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine();
        }

        private void Update()
        {
            if (Keyboard.current.backquoteKey.wasPressedThisFrame)
                isActive = !isActive;

            if (!isActive)
                return;
            if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
            {
                _isShift = true;
            }

            if (Keyboard.current.leftShiftKey.wasReleasedThisFrame)
            {
                _isShift = false;
            }

            if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
            {
                _isCtrl = true;
                // _input.readOnly = true;
            }

            if (Keyboard.current.leftCtrlKey.wasReleasedThisFrame)
            {
                _isCtrl = false;
                // _input.readOnly = false;
            }

            if (_isCtrl && Keyboard.current.leftCtrlKey.wasReleasedThisFrame)
            {
                GUIUtility.systemCopyBuffer = _input.text;
            }

            if (_isCtrl && Keyboard.current.vKey.wasReleasedThisFrame)
            {
                _input.SetTextWithoutNotify(GUIUtility.systemCopyBuffer);
                SetPopActive(false);
            }

            if (Keyboard.current.upArrowKey.wasReleasedThisFrame)
            {
                _input.SetTextWithoutNotify(CommandManager.instance.commandHistory.PreviousCommandLine());
                _input.MoveTextEnd(false);
                SetPopActive(false);
            }

            if (Keyboard.current.downArrowKey.wasReleasedThisFrame)
            {
                _input.SetTextWithoutNotify(CommandManager.instance.commandHistory.NextCommandLine());
                _input.MoveTextEnd(false);
                SetPopActive(false);
            }

            if (Keyboard.current.leftArrowKey.wasReleasedThisFrame ||
                Keyboard.current.rightArrowKey.wasReleasedThisFrame)
            {
                RefreshSuggest();
            }


            if (Keyboard.current.backspaceKey.wasReleasedThisFrame && _isCtrl)
            {
                commandTextEditor.Backspace();
                _input.SetTextWithoutNotify(commandTextEditor.RebuildString());
                RefreshSuggest();
                _input.readOnly = true;
            }

            if (Keyboard.current.backspaceKey.wasReleasedThisFrame)
            {
                _input.readOnly = false;
            }

            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                if (currentSuggestBuilder != null)
                {
                    if (_isShift)
                        currentSuggestBuilder.MoveLast();
                    else
                        currentSuggestBuilder.MoveNext();

                    commandTextEditor.SetData(commandTextEditor.editingIndex,
                        currentSuggestBuilder.currentSuggestString);
                    _input.SetTextWithoutNotify(commandTextEditor.RebuildString());
                    _input.MoveTextEnd(false);
                    currentCommand = CommandManager.instance.FindCommand(commandTextEditor.commandName);
                    RefreshPopText();
                }
            }
        }

        public void Submit(string x)
        {
            _consoleContent.AddMessage($"<color=#3BFF63>> {x}</color>");
            ExecuteCommand(x);
            SetPopActive(false);
        }

        // private Token token { get; } = Token.NewToken();


        // private void OnDisable()
        // {
        //     Game.inputSystem.EnableInput(token);
        // }

        private void OnDestroy()
        {
            Application.logMessageReceived -= MessageReceived;
            CommandManager.instance.Save();
        }

        private void OnApplicationQuit()
        {
            CommandManager.instance.Save();
        }

        private void MessageReceived(string condition, string stacktrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    _consoleContent.AddMessage($"<color=#FF3B3B>{condition}\n{stacktrace}</color>");
                    break;
                case LogType.Assert:
                    _consoleContent.AddMessage($"<color=#FF3B88>{condition}\n{stacktrace}</color>");
                    break;
                case LogType.Warning:
                    // _consoleContent.AddMessage($"<color=#FF3B88>{condition}</color>");
                    break;
                case LogType.Log:
                    _consoleContent.AddMessage(condition);
                    break;
                case LogType.Exception:
                    _consoleContent.AddMessage($"<color=#3BFFF3>{condition}\n{stacktrace}</color>");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}