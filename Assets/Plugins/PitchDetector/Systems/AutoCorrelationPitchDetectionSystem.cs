using Unity.Entities;
using Pitch;
using UnityEngine;

namespace FinerGames.PitchDetector
{
    public class AutoCorrelationPitchDetectionSystem : ComponentSystem
    {
        ComponentGroup pitchDetectors;

        PitchTracker pitchTracker;

        protected override void OnCreateManager()
        {
            base.OnCreateManager();

            var query = new EntityArchetypeQuery()
            {
                All = new ComponentType[] { typeof(PitchDetector), },
            };
            pitchDetectors = GetComponentGroup(query);

            pitchTracker = new PitchTracker();
            pitchTracker.SampleRate = 44100;
        }

        protected override void OnUpdate()
        {
            ForEach((PitchDetector detector) =>
            {
                if (detector.Source == null)
                    return;

                if (detector.Source.clip == null)
                    return;

                if (!Mathf.Approximately(detector.Source.clip.frequency, (float)pitchTracker.SampleRate))
                {
                    Debug.Log("upating sample rate to " + detector.Source.clip.frequency);
                    pitchTracker.SampleRate = detector.Source.clip.frequency;
                }

                //TODO -> figure out the details here...
                //...ideally we can just remove notion of having to set "BufferSource"...
                //...and monitor directly the clip data from the current playback position

                //this seems to work well, but then we can't so freely mute tracks...
                var buffer = new float[1024];
                detector.Source.GetOutputData(buffer, 0);

                //var buffer = new float[detector.Source.clip.frequency * detector.Source.clip.channels];
                //////var offset = detector.Source.timeSamples - detector.Source.clip.frequency;
                ////var offset = detector.Source.timeSamples - 1024;
                ////if (offset < 0)
                ////{
                ////    offset = detector.Source.clip.samples + offset;
                ////}
                //////var offset = detector.Source.timeSamples;
                ////detector.Source.clip.GetData(buffer, offset);
                //detector.Source.clip.GetData(buffer, 0);

                //TODO -> jobify + burst
                pitchTracker.ProcessBuffer(buffer);

                detector.Pitch = pitchTracker.CurrentPitchRecord.Pitch;
                detector.MidiNote = pitchTracker.CurrentPitchRecord.MidiNote;
                detector.MidiCents = pitchTracker.CurrentPitchRecord.MidiCents;
            //}, pitchDetectors);
            });
        }
    }

}