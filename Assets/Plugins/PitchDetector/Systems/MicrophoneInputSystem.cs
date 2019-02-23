using Unity.Entities;
using UnityEngine;

namespace FinerGames.PitchDetector
{
    public class MicrophoneInputSystem : ComponentSystem
    {
        ComponentGroup microphoneInputs;

        protected override void OnCreateManager()
        {
            base.OnCreateManager();

            var query = new EntityArchetypeQuery()
            {
                All = new ComponentType[] { typeof(MicrophoneInput), typeof(AudioSource), },
            };
            microphoneInputs = GetComponentGroup(query);
        }

        protected override void OnUpdate()
        {
            ForEach((MicrophoneInput input) =>
            {
                if (input.Source == null)
                    return;

                if (input.IsRecording && !Microphone.IsRecording(input.DeviceName))
                {
                    input.Source.Stop();
                    input.Source.loop = true;
                    input.Source.clip = Microphone.Start(input.DeviceName, true, 1, input.SampleRate);
                    input.Source.Play();
                }
                else if (!input.IsRecording && Microphone.IsRecording(input.DeviceName))
                {
                    input.Source.Stop();
                    Microphone.End(input.DeviceName);
                }
                //}, microphoneInputs);
            });
        }

        //void OnGUI()
        //{
        //    if (GUILayout.Button(isRecording ? "Disable microphone" : "Enable microphone"))
        //        isRecording = !isRecording;
        //}
    }
}
