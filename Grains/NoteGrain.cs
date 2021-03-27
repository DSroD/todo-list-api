using System;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

using System.Text.Json.Serialization;

using TodoListApi.Interfaces;

namespace TodoListApi.Grains {

    [Serializable]
    public class NoteState {
        [JsonInclude]
        public string noteText { get; set; } = "";
        [JsonInclude]
        public string noteDate { get; set; } = "";
        [JsonInclude]
        public bool finished { get; set; } = false;
    }

    public class NoteGrain : Grain, INoteGrain {

        private readonly IPersistentState<NoteState> _note;

        public NoteGrain([PersistentState("note", "mongo_db")] IPersistentState<NoteState> note) {
            _note = note;
        }

        public Task<string> getNote() {
            return Task.FromResult(_note.State.noteText);
        }
        public Task<string> getDate() {
            return Task.FromResult(_note.State.noteDate);
        }
        public Task<bool> getFinished() {
            return Task.FromResult(_note.State.finished);
        }

        public async Task setFinished(bool finished) {
            _note.State.finished = finished;
            await _note.WriteStateAsync();
        }

        public async Task setNote(string note) {
            _note.State.noteText = note;
            await _note.WriteStateAsync();
        }

        public async Task setDate(string date) {
            _note.State.noteDate = date;
            await _note.WriteStateAsync();
        }

        public async Task removeData() {
            _note.State.finished = false;
            _note.State.noteDate = "";
            _note.State.noteText = "";
            await _note.ClearStateAsync();
        }

        public async Task setData(bool finished, string note, string date) {
            _note.State.finished = finished;
            _note.State.noteDate = date;
            _note.State.noteText = note;
            await _note.WriteStateAsync();
        }

    }

}