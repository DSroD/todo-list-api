using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

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
        public IEnumerable<NoteState> Get() {
            // TODO: This is not fine. We need to implement method to return arbitrary grains (or at least select N of returned or range)
            return Enumerable.Range(0, 200).Select(index =>
                new NoteState {
                    id = index,
                    noteText = _clusterClient.GetGrain<INoteGrain>(index).getNote().Result,
                    noteDate = _clusterClient.GetGrain<INoteGrain>(index).getDate().Result,
                    finished = _clusterClient.GetGrain<INoteGrain>(index).getFinished().Result
                }).Where(state => state.noteDate != "").ToArray();
        }


        /// <summary>
        /// Deletes a specific todo note items.
        /// </summary>
        [HttpDelete("{id}")]
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
        public ActionResult<NoteState> Create(NoteState item) {
            var gr = _clusterClient.GetGrain<INoteGrain>(item.id);
            if(gr.getNote().Result == "") {
                gr.setData(item.finished, item.noteText, item.noteDate);
                return CreatedAtRoute("[controller]", new {id = item.id}, item);
            }

            return BadRequest();

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Update(int id, string noteText) {
            if(noteText == null || noteText == "") {
                return BadRequest();
            }

            var gr = _clusterClient.GetGrain<INoteGrain>(id);
            if(gr.getNote().Result == "") {
                return StatusCode(304);
            }

            gr.setNote(noteText);

            return Ok();
        }
    }
}