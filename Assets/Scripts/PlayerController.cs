using System;
using Azathrix.EzInput.Events;
using Azathrix.Framework.Core;
using Azathrix.Framework.Tools;
using Azathrix.GameKit.Runtime.Behaviours;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : GameScript
{
    [LabelText("移动速度")] [SerializeField] private float _speed = 3;
    [LabelText("跳跃力")] [SerializeField] private float _jumpForce = 3;
    private GroundChecker _groundChecker;

    private Rigidbody2D _rigidbody2D;

    private bool _moveState;

    private bool _clearXVelocityFlag;

    private bool _onGroundState;

    private Vector2 _opDir;


    void Move(bool flag)
    {
        _moveState = flag;
        if (!flag)
            _clearXVelocityFlag = true;
    }

    void Jump()
    {
        if (!_onGroundState)
            return;
        _rigidbody2D.AddForceY(_jumpForce, ForceMode2D.Impulse);
    }

    protected override void OnScriptInitialize()
    {
        base.OnScriptInitialize();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _groundChecker = GetComponentInChildren<GroundChecker>();

        _groundChecker.OnGroundStateChangedEvent += OnGroundStateChangedEvent;
    }

    private void OnGroundStateChangedEvent(bool obj)
    {
        _onGroundState = obj;
        Log.Info("地面状态改变:" + _onGroundState);
    }

    protected override void OnEventRegister()
    {
        base.OnEventRegister();

        AzathrixFramework.Dispatcher.Subscribe((ref InputActionEvent evt) =>
        {
            var input = evt.Data;
            if (input.MapName != "Game")
                return;

            switch (input.ActionName)
            {
                case "Move":
                {
                    switch (input.Phase)
                    {
                        case InputActionPhase.Performed:
                            _opDir = input.ReadValue<Vector2>();
                            break;
                        case InputActionPhase.Started:
                            Move(true);
                            break;
                        case InputActionPhase.Canceled:
                            Move(false);
                            break;
                    }
                }
                    break;
                case "Jump":
                {
                    if (input.Phase == InputActionPhase.Performed)
                    {
                        Jump();
                    }
                }
                    break;
            }
        }).AddTo(this);
    }

    private void FixedUpdate()
    {
        if (_moveState)
        {
            _rigidbody2D.linearVelocityX = _opDir.x * _speed;
        }

        if (_clearXVelocityFlag)
        {
            _clearXVelocityFlag = false;
            _rigidbody2D.linearVelocityX = 0;
        }
    }
}