using System;
using System.Collections;
using Pitch;
using UnityEngine;
using UnityEngine.UI;

namespace FinerGames.PitchDetector.Demo
{
    public class DemoUI : MonoBehaviour
    {
        private PitchTracker m_pitchTracker;
        //private DispatcherTimer m_timer;
        [HideInInspector]
        public float[] m_audioBuffer;
        private int m_timeInterval;
        [SerializeField] int sampleRate = 44100;
        private double m_curWaveAngle;

        private readonly float m_minPitch = 55.0f;
        private readonly float m_maxPitch = 1500.0f;

        [SerializeField] Slider pitchSlider;
        [SerializeField] Slider amplitudeSlider;

        [SerializeField] Text generatorPitch;
        [SerializeField] Text generatorUnits;
        [SerializeField] Text generatorLevel;

        [SerializeField] Text detectorPitch;
        [SerializeField] Text detectorUnits;
        [SerializeField] Text detectorMidiNote;
        [SerializeField] Text detectorMidiCents;
        [SerializeField] Text detectorError;

        //[SerializeField] AudioSource audioSource;

        public AudioSource source;

        void Awake()
        {
            //m_sampleRate = 44100.0f;
            m_timeInterval = 100;  // 100ms

            //InitializeComponent();

            this.GeneratorPitch = 200.0f;
            this.GeneratorAmplitude = 0.1f;

            m_pitchTracker = new PitchTracker();
        }

        void OnEnable()
        {
            m_pitchTracker.SampleRate = sampleRate;
            m_pitchTracker.PitchDetected += OnPitchDetected;

            m_audioBuffer = new float[(int)Math.Round((float)sampleRate * (float)m_timeInterval / 1000.0f)];

            UpdateDisplay();

            StartCoroutine(Tick());
        }

        void OnDisable()
        {
            StopCoroutine(Tick());
            m_pitchTracker.PitchDetected -= OnPitchDetected;
        }

        IEnumerator Tick()
        {
            while (true)
            {
                yield return new WaitForSeconds((float)m_timeInterval / 1000f);
                OnTimerTick(null, null);
            }
        }

        private float GeneratorPitch
        {
            get
            {
                var sliderRatio = pitchSlider.value / pitchSlider.maxValue;
                var maxVal = Math.Log10(m_maxPitch / m_minPitch);
                var pitch = (float)Math.Pow(10.0, sliderRatio * maxVal) * m_minPitch;

                return pitch;
            }

            set
            {
                if (value <= m_minPitch)
                {
                    pitchSlider.value = pitchSlider.minValue;
                }
                else if (value >= m_maxPitch)
                {
                    pitchSlider.value = pitchSlider.maxValue;
                }
                else
                {
                    var maxVal = Math.Log10(m_maxPitch / m_minPitch);
                    var curVal = Math.Log10(value / m_minPitch);
                    var slider = pitchSlider.maxValue * curVal / maxVal;

                    pitchSlider.value = (float)slider;
                }
            }
        }

        private float GeneratorAmplitude
        {
            get { return (float)Math.Pow(10.0, amplitudeSlider.value / 20.0); }
            set { amplitudeSlider.value = (float)(20.0 * Math.Log10(value)); }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            m_curWaveAngle = PitchDsp.CreateSineWave(m_audioBuffer, m_audioBuffer.Length,
                (float)sampleRate, this.GeneratorPitch, this.GeneratorAmplitude, m_curWaveAngle);

            m_pitchTracker.ProcessBuffer(m_audioBuffer);

            var buffer = new float[source.clip.samples];
            source.clip.GetData(buffer, 0);
            m_pitchTracker.ProcessBuffer(buffer);

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            var curPitch = this.GeneratorPitch;

            if (curPitch >= 1000.0f)
            {
                generatorPitch.text = (curPitch / 1000.0f).ToString("F4");
                generatorUnits.text = "kHz";
            }
            else
            {
                generatorPitch.text = curPitch.ToString("F3");
                generatorUnits.text = "Hz";
            }

            // Show the generator amplitude
            generatorLevel.text = amplitudeSlider.value.ToString("F1");

            // Show the detector results
            var curPitchRecord = m_pitchTracker.CurrentPitchRecord;

            if (curPitchRecord.Pitch > 1.0f)
            {
                if (curPitchRecord.Pitch >= 1000.0f)
                {
                    detectorPitch.text = (curPitchRecord.Pitch / 1000.0f).ToString("F4");
                    detectorUnits.text = "kHz";
                }
                else
                {
                    detectorPitch.text = curPitchRecord.Pitch.ToString("F3");
                    detectorUnits.text = "Hz";
                }

                detectorMidiNote.text = PitchDsp.GetNoteName(curPitchRecord.MidiNote, true, true);
                detectorMidiCents.text = curPitchRecord.MidiCents.ToString();

                var diffPercent = 100.0 - (100.0f * this.GeneratorPitch / curPitchRecord.Pitch);

                if (diffPercent >= 0.0f)
                    detectorError.text = "+" + diffPercent.ToString("F3");
                else
                    detectorError.text = diffPercent.ToString("F3");
            }
            else
            {
                detectorPitch.text = "--";
                detectorUnits.text = "Hz";
                detectorError.text = "--";
                detectorMidiNote.text = "--";
                detectorMidiCents.text = "--";
            }
        }

        private void OnPitchDetected(PitchTracker sender, PitchTracker.PitchRecord pitchRecord)
        {
            // During the call to PitchTracker.ProcessBuffer, this event will be fired zero or more times,
            // depending how many pitch records will fit in the new and previously cached buffer.
            //
            // This means that there is no size restriction on the buffer that is passed into ProcessBuffer.
            // For instance, ProcessBuffer can be called with one large buffer that contains all of the
            // audio to be processed, or just a small buffer at a time which is more typical for realtime
            // applications. This PitchDetected event will only occur once enough data has been accumulated
            // to do another detect operation.
        }

        public double PitchToSlider(float pitch)
        {
            if (pitch <= m_minPitch)
                return pitchSlider.minValue;

            if (pitch >= m_maxPitch)
                return pitchSlider.maxValue;

            var maxVal = Math.Log10(m_maxPitch / m_minPitch);
            var curVal = Math.Log10(pitch / m_minPitch);
            var slider = pitchSlider.maxValue * curVal / maxVal;

            return slider;
        }

        public float SliderToPitch(double slider)
        {
            var sliderRatio = slider / pitchSlider.maxValue;
            var maxVal = Math.Log10(m_maxPitch / m_minPitch);
            var pitch = (float)Math.Pow(10.0, sliderRatio * maxVal) * m_minPitch;

            return pitch;
        }
    }
}
