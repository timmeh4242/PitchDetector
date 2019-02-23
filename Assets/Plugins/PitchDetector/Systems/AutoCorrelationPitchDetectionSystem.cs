using Unity.Entities;
using Pitch;

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

                var buffer = new float[detector.Source.clip.samples];
                detector.Source.clip.GetData(buffer, 0);
                pitchTracker.ProcessBuffer(buffer);

                detector.Pitch = pitchTracker.CurrentPitchRecord.Pitch;
                detector.MidiNote = pitchTracker.CurrentPitchRecord.MidiNote;
                detector.MidiCents = pitchTracker.CurrentPitchRecord.MidiCents;
                //}, pitchDetectors);
            });
        }
    }

}