// Must install NuGet package NAudio.Midi
// Instuments are 0 - 127
// Volume is also 0 - 127

using NAudio.Midi;

namespace Asteroids
{
    internal class MidiKeyboard
    {
        private MidiOut _midiOut;

        public MidiKeyboard()
        {
            // 0 is usually the default Windows Synth
            _midiOut = new MidiOut(0);
        }

        // Instrument codes: 0 = Piano, 19 = Church Organ, 24 = Nylon Guitar, etc.
        public void ChangeInstrument(int instrumentCode)
        {
            _midiOut.Send(MidiMessage.ChangePatch(instrumentCode, 1).RawData);
        }

        public async Task PlayNote(int noteNumber, int durationMs, int volume = 100)
        {
            // Start the note
            _midiOut.Send(MidiMessage.StartNote(noteNumber, volume, 1).RawData);

            // Wait for the duration
            await Task.Delay(durationMs);

            // Stop the note
            _midiOut.Send(MidiMessage.StopNote(noteNumber, 0, 1).RawData);
        }

        public async Task NoteDown(int noteNumber, int volume = 100)
        {
            // Start the note
            _midiOut.Send(MidiMessage.StartNote(noteNumber, volume, 1).RawData);
        }

        public async Task NoteUp(int noteNumber, int volume = 100)
        {
            // Stop the note
            _midiOut.Send(MidiMessage.StopNote(noteNumber, 0, 1).RawData);
        }
    }
}
