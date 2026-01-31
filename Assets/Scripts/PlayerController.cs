using System;
using Azathrix.EzInput.Events;
using Azathrix.Framework.Core;
using Azathrix.Framework.Tools;
using Azathrix.GameKit.Runtime.Behaviours;
using Azathrix.GameKit.Runtime.Builder.PrefabBuilders;
using Azathrix.GameKit.Runtime.Extensions;
using Framework.Games;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : GameScript
{
    [LabelText("移动速度")] [SerializeField] private float _speed = 3;
    [LabelText("跳跃力")] [SerializeField] private float _jumpForce = 3;
    [SerializeField] private GroundChecker _groundChecker;
    [SerializeField] private WallChecker _leftWallChecker;
    [SerializeField] private WallChecker _rightWallChecker;
    private CollisionMask _mask;
    private Rigidbody2D _rigidbody2D;

    private bool _moveState;

    private bool _clearXVelocityFlag;

    private bool _onGroundState;
    private bool _onLeftWallState;
    private bool _onRightWallState;
    private Vector2 _mousePos;

    private Vector2 _opDir;

    private Camera _gameCamera;


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

        _gameCamera = SystemEnvironment.instance.systemConfig.gameCamera;

        _groundChecker.OnGroundStateChangedEvent += b =>
        {
            _onGroundState = b;
            Log.Info("地面状态改变:" + _onGroundState);
        };
        _leftWallChecker.OnWallStateChangedEvent += b =>
        {
            _onLeftWallState = b;
            Log.Info("左墙状态改变:" + _onLeftWallState);
            if (b) _clearXVelocityFlag = true;
        };
        _rightWallChecker.OnWallStateChangedEvent += b =>
        {
            _onRightWallState = b;
            Log.Info("右墙状态改变:" + _onRightWallState);
            if (b) _clearXVelocityFlag = true;
        };

        // AzathrixFramework.GetSystem<GamePlaySystem>().FocusCamera(transform);

        _mask = PrefabBuilder.Get().SetPrefab("Prefabs/Mask".LoadPrefab()).SetScale(Vector3.one*3).Build().GetComponent<CollisionMask>();
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
                case "Pointer":
                {
                    if (input.Phase == InputActionPhase.Performed)
                    {
                        _mousePos = input.ReadValue<Vector2>();
                     
                        // Log.Info("鼠标位置更新：" + mousePos);
                    }
                }
                    break;
                case "LeftButton":
                {
                    if (input.Phase == InputActionPhase.Started)
                    {
                        _mask.SetActive(true);
                    }
                    else if (input.Phase == InputActionPhase.Canceled)
                    {
                        _mask.SetActive(false);
                    }
                }
                    break;
            }
        }).AddTo(this);
    }

    private void Update()
    {
        if (!_mask.IsActive)
        {
            // 跟随鼠标                                                                                                                                                                                            
            Vector3 mousePos = _gameCamera.ScreenToWorldPoint(_mousePos);
            _mask.SetPosition(mousePos);
        }
    }

    void OnMoveUpdate()
    {
        if (_moveState)
        {
            if (_opDir.x > 0 && _onRightWallState)
                return;
            if (_opDir.x < 0 && _onLeftWallState)
                return;
            _rigidbody2D.linearVelocityX = _opDir.x * _speed;
        }
    }


    private void FixedUpdate()
    {
        OnMoveUpdate();
        if (_clearXVelocityFlag)
        {
            _clearXVelocityFlag = false;
            _rigidbody2D.linearVelocityX = 0;
        }
    }
}