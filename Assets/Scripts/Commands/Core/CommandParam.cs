using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Framework.Commands.Attributes;
using Framework.Commands.Suggest;

namespace Framework.Commands.Core
{
    public class CommandParam
    {
        public CommandParamAttribute paramAttribute { get; }

        public ParameterInfo paramInfo { get; }

        public string name { get; }

        public string comment { get; }

        public MethodInfo suggestAction { get; }

        public Command currentCommand { get; }

        private List<ISuggestItem> _params = null;


        void InitParams()
        {
            if (_params == null)
            {
                _params = new List<ISuggestItem>();
                if (suggestAction != null)
                {
                    try
                    {
                        var r = suggestAction.Invoke(null, null) as IList;
                        foreach (ISuggestItem variable in r)
                            _params.Add(variable);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }

        public IEnumerable<ISuggestItem> @params
        {
            get
            {
                InitParams();

                foreach (var variable in _params)
                    yield return variable;
            }
        }

        public bool hasSuggest
        {
            get { return _params != null && _params.Count > 0; }
        }

        public ISuggestItem FindParamSuggest(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;
            InitParams();
            foreach (var variable in _params)
            {
                if (variable.GetName() == str)
                {
                    return variable;
                }
            }

            return null;
        }

        public object GetParamObject(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;
            InitParams();
            foreach (var variable in _params)
            {
                if (variable.GetName() == str)
                {
                    return variable.GetValue();
                }
            }

            return null;
        }


        public CommandParam(Command command, ParameterInfo paramInfo)
        {
            currentCommand = command;
            this.paramInfo = paramInfo;
            paramAttribute = paramInfo.GetCustomAttribute<CommandParamAttribute>();
            if (paramAttribute != null)
            {
                if (!string.IsNullOrEmpty(paramAttribute.suggestAction))
                {
                    suggestAction = command.type.GetMethod(paramAttribute.suggestAction,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                }

                comment = paramAttribute.comment;
                var n = paramAttribute.name;
                if (string.IsNullOrEmpty(n))
                    n = paramInfo.Name;
                name = n + ":" + paramInfo.ParameterType.Name;
            }
            else
            {
                name = paramInfo.Name + ":" + paramInfo.ParameterType.Name;
            }
        }
    }
}