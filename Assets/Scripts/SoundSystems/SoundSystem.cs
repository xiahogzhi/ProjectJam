using System;
using System.Collections.Generic;
using System.Threading;
using Azathrix.Framework.Interfaces;
using Azathrix.Framework.Interfaces.SystemEvents;
using Azathrix.GameKit.Runtime.Extensions;
using Cysharp.Threading.Tasks;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Debug = UnityEngine.Debug;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace SoundSystems
{
    public class SoundSystem : ISystem, ISystemInitialize, ISystemUpdate
    {
        public Bus sfxBus { private set; get; }
        public Bus masterBus { private set; get; }
        public Bus bgmBus { private set; get; }


        private readonly string[] _musics = new string[10];
        private string _currentMusic;
        private EventInstance _currentPlayMusic;

        private bool _isHiding;
        private bool _isShowing;

        private readonly Queue<Action> _postActions = new Queue<Action>();

        private string topMusic
        {
            get
            {
                for (int i = _musics.Length - 1; i >= 0; i--)
                {
                    if (_musics[i] != null)
                        return _musics[i];
                }

                return null;
            }
        }

        public void ClearBackground()
        {
            _postActions.Enqueue(() =>
            {
                if (topMusic != null)
                {
                    for (int i = 0; i < _musics.Length; i++)
                        _musics[i] = null;
                    HideBackground().Forget();
                }
            });
        }

        public void PlayBackground(string evt, int bus = 0)
        {
            if (string.IsNullOrEmpty(evt))
                return;
            _postActions.Enqueue(() =>
            {
                var tp = _musics[bus];
                if (tp == null || tp != evt)
                {
                    if (tp != null)
                    {
                        _musics[bus] = null;
                        HideBackground().Forget();
                    }

                    if (!string.IsNullOrEmpty(evt))
                    {
                        _musics[bus] = evt;
                        PlayTop();
                    }
                }
            });
        }

        void PlayTop()
        {
            var tp = topMusic;
            if (tp != null && !string.IsNullOrEmpty(tp))
            {
                if (_currentMusic == null || _currentMusic != tp)
                {
                    _currentMusic = tp;
                    _currentPlayMusic = RuntimeManager.CreateInstance(tp);
                    ShowMusic().Forget();
                }
            }
        }


        private async UniTask HideBackground()
        {
            _isHiding = true;
            var cur = _currentPlayMusic;
            if (cur.isValid())
            {
                cur.setVolume(1);
                float duration = 1;
                while (duration > 0.1f)
                {
                    duration -= Time.unscaledDeltaTime;
                    cur.setVolume(duration / 1f);
                    await UniTask.Yield();
                }

                await UniTask.Yield();
                cur.stop(STOP_MODE.IMMEDIATE);
                await UniTask.Yield();
                cur.release();
                _currentMusic = null;
            }

            _isHiding = false;
            // PlayTop();
        }

        async UniTask ShowMusic()
        {
            _isShowing = true;
            var cur = _currentPlayMusic;
            if (cur.isValid())
            {
                cur.start();
                cur.setVolume(0);
                float duration = 0;
                while (duration < 1)
                {
                    duration += Time.unscaledDeltaTime;
                    cur.setVolume(duration / 1f);
                    await UniTask.Yield();
                }
            }

            _isShowing = false;
        }

        public void SetSoundEffectVolume(float v)
        {
            sfxBus.setVolume(v);
        }

        public void SetMusicVolume(float v)
        {
            bgmBus.setVolume(v);
        }

        public void PlaySoundEffect(EventReference evt, Vector3 pos)
        {
            if (evt.IsNull)
                return;

            try
            {
                EventInstance ins = RuntimeManager.CreateInstance(evt);

                //如果状态可用
                if (ins.isValid())
                {
                    ins.set3DAttributes(pos.To3DAttributes());
                    ins.start();
                }

                ins.release();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        public void PlaySoundEffect(string evt, Vector3 pos)
        {
            if (string.IsNullOrEmpty(evt))
                return;

            try
            {
                EventInstance ins = RuntimeManager.CreateInstance(evt);

                //如果状态可用
                if (ins.isValid())
                {
                    ins.set3DAttributes(pos.To3DAttributes());
                    ins.start();
                }

                ins.release();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void PlaySoundEffect(EventReference evt)
        {
            if (evt.IsNull)
                return;

            try
            {
                EventInstance ins = RuntimeManager.CreateInstance(evt);
                if (ins.isValid())
                    ins.start();

                ins.release();
            }
            catch (Exception e)
            {
                // Log.Waring(e.Message);
            }
        }

        public async UniTask PlaySoundEffectAsync(string evt, GameObject attachTarget, CancellationToken token)
        {
            if (string.IsNullOrEmpty(evt) || attachTarget == null)
                return;

            try
            {
                EventInstance ins = RuntimeManager.CreateInstance(evt);

                if (ins.isValid())
                {
                    var ap = attachTarget.TryAddComponent<AttachSoundComponent>();
                    ap.Attach(ins);
                    ins.set3DAttributes(attachTarget.To3DAttributes());
                    ins.start();

                    while (true)
                    {
                        var result = ins.getPlaybackState(out var p);
                        if (result != RESULT.OK)
                            break;

                        if (p == PLAYBACK_STATE.STOPPED)
                            break;

                        await UniTask.Yield(cancellationToken: token);
                    }
                }

                ins.release();
            }
            catch (OperationCanceledException e)
            {
                throw;
            }
            catch (Exception e)
            {
                // Log.Waring(e.Message);
            }
        }

        public async UniTask PlaySoundEffectAsync(string evt, GameObject attachTarget)
        {
            if (string.IsNullOrEmpty(evt) || attachTarget == null)
                return;

            try
            {
                EventInstance ins = RuntimeManager.CreateInstance(evt);

                if (ins.isValid())
                {
                    var ap = attachTarget.TryAddComponent<AttachSoundComponent>();
                    ap.Attach(ins);
                    ins.set3DAttributes(attachTarget.To3DAttributes());
                    ins.start();

                    while (true)
                    {
                        var result = ins.getPlaybackState(out var p);
                        if (result != RESULT.OK)
                            break;

                        if (p == PLAYBACK_STATE.STOPPED)
                            break;

                        await UniTask.Yield();
                    }
                }

                ins.release();
            }
            catch (Exception e)
            {
                // Log.Waring(e.Message);
            }
        }

        public async UniTask PlaySoundEffectAsync(string evt, Vector3 position)
        {
            if (string.IsNullOrEmpty(evt))
                return;

            try
            {
                EventInstance ins = RuntimeManager.CreateInstance(evt);

                if (ins.isValid())
                {
                    ins.set3DAttributes(position.To3DAttributes());
                    ins.start();

                    while (true)
                    {
                        var result = ins.getPlaybackState(out var p);
                        if (result != RESULT.OK)
                            break;

                        if (p == PLAYBACK_STATE.STOPPED)
                            break;

                        await UniTask.Yield();
                    }
                }

                ins.release();
            }
            catch (Exception e)
            {
                // Log.Waring(e.Message);
            }
        }

        public async UniTask PlaySoundEffectAsync(string evt)
        {
            if (string.IsNullOrEmpty(evt))
                return;

            try
            {
                EventInstance ins = RuntimeManager.CreateInstance(evt);

                if (ins.isValid())
                {
                    ins.start();


                    while (true)
                    {
                        var result = ins.getPlaybackState(out var p);
                        if (result != RESULT.OK)
                            break;

                        if (p == PLAYBACK_STATE.STOPPED)
                            break;

                        await UniTask.Yield();
                    }
                }

                ins.release();
            }
            catch (Exception e)
            {
                // Log.Waring(e.Message);
            }
        }

        public void PlaySoundEffect(EventReference evt, GameObject attachTarget)
        {
            if (evt.IsNull || !attachTarget)
                return;

            try
            {
                EventInstance ins = RuntimeManager.CreateInstance(evt);
                //如果状态可用
                if (ins.isValid())
                {
                    var ap = attachTarget.TryAddComponent<AttachSoundComponent>();
                    ap.Attach(ins);
                    ins.set3DAttributes(attachTarget.To3DAttributes());
                    // ins.set3DAttributes(gameObject.To3DAttributes());

                    ins.start();
                }

                ins.release();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void PlaySoundEffect(string evt)
        {
            if (string.IsNullOrEmpty(evt))
                return;

            try
            {
                EventInstance ins = RuntimeManager.CreateInstance(evt);
                //如果状态可用
                if (ins.isValid())
                {
                    // RuntimeManager.AttachInstanceToGameObject(ins, gameObject.transform);
                    // ins.set3DAttributes(gameObject.To3DAttributes());
                    ins.start();
                }

                ins.release();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void PlaySoundEffect(string evt, GameObject attachTarget)
        {
            if (string.IsNullOrEmpty(evt) || !attachTarget)
                return;

            try
            {
                EventInstance ins = RuntimeManager.CreateInstance(evt);
                //如果状态可用
                if (ins.isValid())
                {
                    var ap = attachTarget.TryAddComponent<AttachSoundComponent>();
                    ap.Attach(ins);

                    // RuntimeManager.AttachInstanceToGameObject(ins, gameObject.transform);
                    // ins.set3DAttributes(gameObject.To3DAttributes());
                    ins.start();
                }

                ins.release();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }


        public void OnUpdate(float deltaTime)
        {
            if (!_isShowing && !_isHiding)
            {
                if (_postActions.Count > 0)
                {
                    var act = _postActions.Dequeue();
                    act();
                }
            }
        }

        public UniTask OnInitializeAsync()
        {
            bgmBus = RuntimeManager.GetBus("bus:/Master/Background");
            sfxBus = RuntimeManager.GetBus("bus:/Master/Effect");
            masterBus = RuntimeManager.GetBus("bus:/Master");
            return UniTask.CompletedTask;
        }
    }
}