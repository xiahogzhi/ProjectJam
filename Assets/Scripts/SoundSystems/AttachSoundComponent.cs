using System.Collections.Generic;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace SoundSystems
{
    /// <summary>
    /// 附加音效组件
    /// </summary>
    public class AttachSoundComponent : MonoBehaviour
    {
        private List<EventInstance> _eventInstances = new List<EventInstance>();


        public void Attach(EventInstance ins)
        {
            _eventInstances.Add(ins);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _eventInstances.Count; i++)
            {
                var ins = _eventInstances[i];
                if (ins.isValid())
                    ins.release();
            }
        }

        private void LateUpdate()
        {
            if (_eventInstances.Count == 0)
                return;

            var p = transform.position.To3DAttributes();

            for (int i = _eventInstances.Count - 1; i >= 0; i--)
            {
                var ins = _eventInstances[i];
                if (ins.isValid())
                {
                    var r = ins.getPlaybackState(out var p2);
                    if (r == RESULT.OK)
                    {
                        if (p2 == PLAYBACK_STATE.PLAYING)
                        {
                            ins.set3DAttributes(p);
                        }
                        else if (p2 == PLAYBACK_STATE.STOPPED)
                        {
                            _eventInstances.RemoveAt(i);
                            ins.release();
                        }
                    }
                }
                else
                {
                    _eventInstances.RemoveAt(i);
                    ins.release();
                }
            }
        }
    }
}