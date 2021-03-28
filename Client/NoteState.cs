
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TodoListApi.Client {
    public class NoteState {
        [Required]
        public int note_id { get; set; }
        [Required]
        public string noteText { get; set; }
        [Required]
        public string noteDate { get; set; }
        [DefaultValue(false)]
        public bool finished { get; set; }
        
    }
}