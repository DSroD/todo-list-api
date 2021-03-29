using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;

using Orleans;

using TodoListApi.Interfaces;

namespace TodoListApi.Client.Controllers {

    [ApiController]
    [Route("[controller]")]
    public class NoteController : ControllerBase {

        private IClusterClient _clusterClient;
        private readonly ILogger<NoteController> _logger;
        public NoteController(IClusterClient clusterClient, ILogger<NoteController> logger) {
            this._clusterClient = clusterClient;
            this._logger = logger;
            
        }

        /// <summary>
        /// Get all todo note items
        /// </summary>
        [HttpGet]
        //[EnableCors("AnyOrigin")]
        public IEnumerable<NoteState> Get() {
            // TODO: This is not fine. We need to implement method to return arbitrary grains (or at least select N of returned or range)
            return Enumerable.Range(0, 200).Select(index =>
                new NoteState {
                    note_id = index,
                    noteText = _clusterClient.GetGrain<INoteGrain>(index).getNote().Result,
                    noteDate = _clusterClient.GetGrain<INoteGrain>(index).getDate().Result,
                    finished = _clusterClient.GetGrain<INoteGrain>(index).getFinished().Result
                }).Where(state => state.noteDate != "").ToArray();
        }


        /// <summary>
        /// Deletes a specific todo note items.
        /// </summary>
        [HttpDelete("{id}")]
        //[EnableCors("AnyOrigin")]
        public IActionResult Delete(int id)
        {
            var tsk = _clusterClient.GetGrain<INoteGrain>(id).removeData();
            tsk.Wait();
            return NoContent();
        }


        /// <summary>
        /// Creates a todo list note.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Note
        ///     {
        ///        "noteText": "My first todo-list note!",
        ///        "noteDate": "27.3.2021",
        ///        "finished": false
        ///     }
        ///
        /// </remarks>
        /// <param name="item"></param>
        /// <returns>A newly created NoteState</returns>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null or already exists</response>   
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[EnableCors("AnyOrigin")]
        public ActionResult<NoteState> Create(NoteState item) {
            var gr = _clusterClient.GetGrain<INoteGrain>(item.note_id);
            if(gr.getNote().Result != "") {
                return BadRequest();
            }
            gr.setData(item.finished, item.noteText, item.noteDate);
            return StatusCode(201);
        }


        [HttpPost("{id}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[EnableCors("AnyOrigin")]
        public ActionResult<NoteUpdate> UpdateText(int id, NoteUpdate item) {
            if((item.noteText == null || item.noteText == "") && item.finished == null) {
                return BadRequest();
            }

            var gr = _clusterClient.GetGrain<INoteGrain>(id);
            if(gr.getNote().Result == "") {
                return StatusCode(304);
            }
            if(item.noteText != null && item.noteText != "") { gr.setNote(item.noteText); }
            if(item.finished != null) { gr.setFinished(item.finished ?? gr.getFinished().Result); }

            return Accepted();
        }
    }
}