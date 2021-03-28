using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TodoListApi.Client {
    public class NoteUpdate {
        [DefaultValue("")]
        public string noteText { get; set; }
        [DefaultValue(null)]
        public bool? finished { get; set; }
        
    }
}