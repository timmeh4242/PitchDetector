using UnityEngine;
using Unity.Entities;
using FinerGames.PitchDetector;

public class FastYinSystem : ComponentSystem
{
    ComponentGroup pitchDetectors;

    FastYin fastYin;

    protected override void OnCreateManager()
    {
        base.OnCreateManager();

        var query = new EntityArchetypeQuery()
        {
            All = new ComponentType[] { typeof(PitchDetector), },
        };
        pitchDetectors = GetComponentGroup(query);

        fastYin = new FastYin(44100, 1024);
    }
    protected override void OnUpdate()
    {
        ForEach((PitchDetector detector) =>
        {
            if (detector.Source == null)
                return;

            if (detector.Source.clip == null)
                return;

            var buffer = new float[1024];
            detector.Source.GetOutputData(buffer, 0);

            //TODO -> jobify + burst
            var result = fastYin.getPitch(buffer);
            
            var pitch = result.getPitch();
            var midiNote = 0;
            var midiCents = 0;

            Pitch.PitchDsp.PitchToMidiNote(pitch, out midiNote, out midiCents);

            detector.Pitch = pitch;
            detector.MidiNote = midiNote;
            detector.MidiCents = midiCents;
        //}, pitchDetectors);
        });
    }

}
