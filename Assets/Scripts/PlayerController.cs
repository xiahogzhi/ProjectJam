using System;
using System.Collections.Generic;
using Azathrix.EzInput.Core;
using Azathrix.EzInput.Events;
using Azathrix.EzUI.Events;
using Azathrix.Framework.Core;
using Azathrix.Framework.Tools;
using Azathrix.GameKit.Runtime.Behaviours;
using Azathrix.GameKit.Runtime.Builder.PrefabBuilders;
using Azathrix.GameKit.Runtime.Extensions;
using Azcel;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Framework.Games;
using Game.Tables;
using Sirenix.OdinInspector;
using SoundSystems;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Profiling;

public class PlayerController : GameScript
{
    [LabelText("移动速度")] [SerializeField] private float _speed = 3;
    [LabelText("跳跃力")] [SerializeField] private float _jumpForce = 3;
    [SerializeField] private GroundChecker _groundChecker;
    [SerializeField] private WallChecker _leftWallChecker;
    [SerializeField] private WallChecker _rightWallChecker;
    [SerializeField] private HurtChecker _hurtChecker;
    [SerializeField] private Transform _bubble;
    [SerializeField] private TMP_Text _bubbleText;


    [Title("卡墙推力配置")] [LabelText("最小推力")] [Tooltip("穿透刚超过阈值时的推力")] [SerializeField]
    private float _minPushForce = 2f;

    [LabelText("最大推力")] [Tooltip("穿透达到最大深度时的推力")] [SerializeField]
    private float _maxPushForce = 8f;

    [LabelText("穿透阈值")] [Tooltip("只有穿透深度超过此值才施加推力")] [SerializeField]
    private float _penetrationThreshold = 0.1f;

    [LabelText("最大穿透深度")] [Tooltip("用于插值计算的最大穿透深度")] [SerializeField]
    private float _maxPenetrationDepth = 1f;

    [LabelText("Layer A 碰撞层")] [Tooltip("mask取消时检测的碰撞层（原始层级）")] [SerializeField]
    private LayerMask _layerACheckLayer;

    [LabelText("Layer B 碰撞层")] [Tooltip("mask激活时检测的碰撞层（切换后层级）")] [SerializeField]
    private LayerMask _layerBCheckLayer;

    private Animator _animator;

    private SpriteRenderer _renderer;

    private CollisionMask _mask;
    private Rigidbody2D _rigidbody2D;
    private Collider2D _playerCollider;

    private bool _moveState;

    private bool _clearXVelocityFlag;

    private bool _onGroundState;
    private bool _onLeftWallState;
    private bool _onRightWallState;
    private Vector2 _mousePos;

    private Vector2 _opDir;

    private Camera _gameCamera;

    private int _jumpCount;

    private bool _isFollow = true;

    private bool _isDead;
    private bool _isStart;

    public bool isDead => _isDead;


    public struct OnPlayerDead
    {
    }

    public struct OnPlayerRebirth
    {
    }

    public struct OnPlayerEnd
    {
    }

    async UniTask ShowDialogue(int id)
    {
        if (id == -1)
        {
            _bubble.gameObject.SetActive(false);
            return;
        }

        var azcel = AzathrixFramework.GetSystem<AzcelSystem>();
        var d = azcel.GetConfig<DialogueConfig>(id);
        if (d == null)
        {
            _bubble.gameObject.SetActive(false);
            return;
        }

        _bubble.gameObject.SetActive(true);

        _bubbleText.text = d.text;

        _bubble.localScale = Vector3.zero;
        _bubble.DOScale(1, 0.5f).SetUpdate(false).SetEase(Ease.OutCirc);
        await UniTask.WaitForSeconds(0.5f, true);
        await UniTask.WaitForSeconds(d.duration, true);
        await ShowDialogue(d.next);
    }

    // 用于检测重叠的缓冲区
    private List<Collider2D> _overlapBuffer = new();

    async void EndPoint()
    {
        _isStart = false;
        // Time.timeScale = 0;
        AzathrixFramework.Dispatcher.Dispatch<OnPlayerEnd>();
        var config = AzathrixFramework.GetSystem<GamePlaySystem>().currentLevel;
        if (config.NextLevel == -1)
        {
            await UniTask.WaitForSeconds(0.3f, true);
            await ShowDialogue(200);

            await UniTask.WaitForSeconds(0.3f, true);
        }

        AzathrixFramework.GetSystem<GamePlaySystem>().NextLevel();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EndPoint"))
        {
            EndPoint();
        }
        else if (other.CompareTag("Bubble"))
        {
            _rigidbody2D.linearVelocityY = 0;
            _rigidbody2D.AddForceY(_jumpForce, ForceMode2D.Impulse);
            AzathrixFramework.GetSystem<SoundSystem>().PlaySoundEffect("event:/特效音效/Jump");
        }
    }

    async void Dead()
    {
        if (_isDead) return;
        Move(false);
        _isDead = true;

        var child = GetComponentsInChildren<BoxCollider2D>(true);

        foreach (var variable in child)
        {
            variable.enabled = false;
        }

        Time.timeScale = 0;
        _rigidbody2D.FreezeAll();

        AzathrixFramework.Dispatcher.Dispatch<OnPlayerDead>();


        AzathrixFramework.GetSystem<SoundSystem>().PlaySoundEffect("event:/特效音效/Dead");
        await _animator.PlayAsync("dead");

        AzathrixFramework.GetSystem<GamePlaySystem>().Replay();
    }


    void Move(bool flag)
    {
        _moveState = flag;
        if (_isDead || !_isStart)
            return;

        if (_onGroundState)
        {
            if (flag)
                _animator.Play("move");
            else
                _animator.Play("idle");
        }


        if (!flag)
            _clearXVelocityFlag = true;
    }

    void Jump(float velocity = 0)
    {
        if (_jumpCount <= 0)
            return;
        _jumpCount--;
        if (velocity == 0)
            velocity = _jumpForce;
        // 先清空Y轴速度，避免与其他力叠加
        _rigidbody2D.linearVelocityY = 0;
        _rigidbody2D.AddForceY(velocity, ForceMode2D.Impulse);
        AzathrixFramework.GetSystem<SoundSystem>().PlaySoundEffect("event:/特效音效/Jump");
    }

    public void FootstepSound()
    {
        AzathrixFramework.GetSystem<SoundSystem>().PlaySoundEffect("event:/特效音效/Move");
    }

    async void Rebirth()
    {
        // Time.timeScale = 0;
        _rigidbody2D.gravityScale = 0;
        var cancel = gameObject.GetCancellationTokenOnDestroy();
        await UniTask.WaitForSeconds(0.5f, true, cancellationToken: cancel);

        AzathrixFramework.GetSystem<SoundSystem>().PlaySoundEffect("event:/特效音效/Rebirth");
        await _animator.PlayAsync("rebirth", cancel);
        // _rigidbody2D.Unfreeze();
        // _rigidbody2D.FreezeRotation();
        _rigidbody2D.gravityScale = 1;
        if (!_onGroundState)
        {
            _animator.Play("jump");
        }
        else
        {
            if (_moveState)
            {
                _animator.Play("move");
            }
            else
            {
                _animator.Play("idle");
            }
        }

        var config = AzathrixFramework.GetSystem<GamePlaySystem>().currentLevel;
        var current = config.Id;
        if (current == 1 && !ES3.KeyExists("tutorial"))
        {
            ES3.Save("tutorial", true);
            await UniTask.WaitForSeconds(1f, true, cancellationToken: cancel);
            _animator.Play("idle");
            await ShowDialogue(100);
            await UniTask.WaitForSeconds(0.3f, true, cancellationToken: cancel);
        }


        AzathrixFramework.Dispatcher.Dispatch<OnPlayerRebirth>();

        // Time.timeScale = 1;
        _isStart = true;
    }

    protected override void OnScriptInitialize()
    {
        base.OnScriptInitialize();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _playerCollider = GetComponent<Collider2D>();
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
        // _rigidbody2D.FreezeAll();
        Rebirth();

        _gameCamera = SystemEnvironment.instance.systemConfig.gameCamera;

        // _endPointChecker.OnEnterEndPointEvent += () =>
        // {
        //     AzathrixFramework.GetSystem<GamePlaySystem>().NextLevel();
        //
        // };

        _hurtChecker.OnHurtEvent += () => { Dead(); };

        _groundChecker.OnPlatformEnterEvent += transform1 => { transform.SetParent(transform1); };


        _groundChecker.OnGroundStateChangedEvent += b =>
        {
            _onGroundState = b;
            if (_jumpCount <= 0 && b)
                _jumpCount = 1;
            if (!_isStart || isDead)
                return;
            if (!b)
            {
                _animator.Play("jump");
            }
            else
            {
                if (_moveState)
                {
                    _animator.Play("move");
                }
                else
                {
                    _animator.Play("idle");
                }
            }

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

        _mask = PrefabBuilder.Get().SetPrefab("Prefabs/Mask".LoadPrefab()).SetScale(Vector3.one * 3).Build()
            .GetComponent<CollisionMask>();
    }


    protected override void OnEventRegister()
    {
        base.OnEventRegister();

        AzathrixFramework.Dispatcher.Subscribe((ref InputActionEvent evt) =>
        {
            var input = evt.Data;
            if (input.MapName != "Game")
                return;

            if (_isDead || !_isStart) return;

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
                        SwitchFollow();
                    }
                    else if (input.Phase == InputActionPhase.Canceled)
                    {
                    }
                }
                    break;
            }
        }).AddTo(this);
    }

    void SwitchFollow()
    {
        _isFollow = !_isFollow;

        if (_isFollow)
        {
            _mask.SetActive(false);
            // mask取消，检测Layer A（恢复的碰撞）
            CheckAndPushPlayerFromWall(false).Forget();
        }
        else
        {
            _mask.SetActive(true);
            // mask激活，检测Layer B（新出现的碰撞）
            CheckAndPushPlayerFromWall(true).Forget();
        }
    }

    /// <summary>
    /// 检测玩家是否卡在墙里，如果是则根据速度反向施加推力
    /// </summary>
    /// <param name="isMaskActive">mask是否激活，true检测LayerB，false检测LayerA</param>
    async UniTaskVoid CheckAndPushPlayerFromWall(bool isMaskActive)
    {
        if (_playerCollider == null || _rigidbody2D == null)
            return;

        // 等待物理更新
        await UniTask.WaitForFixedUpdate();

        // 根据mask状态选择检测的层级
        LayerMask checkLayer = isMaskActive ? _layerBCheckLayer : _layerACheckLayer;

        // 检测玩家是否与墙体重叠
        _overlapBuffer.Clear();
        var filter = new ContactFilter2D
        {
            layerMask = checkLayer,
            useLayerMask = true,
            useTriggers = false
        };

        int count = _playerCollider.Overlap(filter, _overlapBuffer);

        if (count > 0)
        {
            // 检查最大穿透深度
            float maxPenetration = 0f;
            foreach (var otherCollider in _overlapBuffer)
            {
                var distance = _playerCollider.Distance(otherCollider);
                if (distance.isOverlapped)
                {
                    // distance.distance 为负值表示穿透深度
                    float penetration = -distance.distance;
                    if (penetration > maxPenetration)
                    {
                        maxPenetration = penetration;
                    }
                }
            }

            // 只有穿透深度超过阈值才施加推力
            if (maxPenetration > _penetrationThreshold)
            {
                // 根据穿透深度插值计算推力
                float t = Mathf.InverseLerp(_penetrationThreshold, _maxPenetrationDepth, maxPenetration);
                float pushForce = Mathf.Lerp(_minPushForce, _maxPushForce, t);

                // 玩家卡在墙里，先重置Y轴速度，然后向上施加推力
                _rigidbody2D.linearVelocityY = 0;
                _rigidbody2D.AddForce(new Vector2(0, pushForce), ForceMode2D.Impulse);
                Log.Info($"玩家卡墙，穿透深度: {maxPenetration}，施加Y轴推力: {pushForce}");
            }
        }
    }

    private void Update()
    {
        if (!_mask.IsActive && !isDead && _isStart)
        {
            // 跟随鼠标
            Vector3 mousePos = _gameCamera.ScreenToWorldPoint(_mousePos);
            _mask.SetPosition(mousePos);
        }
    }

    void OnMoveUpdate()
    {
        if (_moveState && !isDead && _isStart)
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
        if (!_isDead && _isStart)
            _renderer.flipX = _opDir.x < 0;

        if (_clearXVelocityFlag)
        {
            _clearXVelocityFlag = false;
            _rigidbody2D.linearVelocityX = 0;
        }
    }
}