using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Midi
{
    public class MidiEventHandler : MonoBehaviour, IMidiEventHandler
    {
        [SerializeField] private Text text;

        private void Awake()
        {
            gameObject.AddComponent<MidiManager>();
        }

        private void Start()
        {
            MidiManager.Instance.RegisterEventHandler(this);
        }

        public void RawMidi(sbyte a, sbyte b, sbyte c)
        {
            Debug.Log("RawMidi a " + a + " b " + b + " c " + c);
            text.text += "RawMidi a " + a + " b " + b + " c " + c + Environment.NewLine;
        }

        public void NoteOn(int note, int velocity)
        {
            Debug.Log("Note On " + note + " velocity " + velocity);
            text.text += "Note On " + note + " velocity " + velocity + Environment.NewLine;
        }

        public void NoteOff(int note)
        {
            Debug.Log("Note off " + note);
            text.text += "Note off " + note + Environment.NewLine;
        }

        public void DeviceAttached(string deviceName)
        {
            Debug.Log("Device Attached " + deviceName);
            text.text += "Device Attached " + deviceName + Environment.NewLine;
        }

        public void DeviceDetached(string deviceName)
        {
            Debug.Log("Device Detached " + deviceName);
            text.text += "Device Detached " + deviceName + Environment.NewLine;
        }
    }
}