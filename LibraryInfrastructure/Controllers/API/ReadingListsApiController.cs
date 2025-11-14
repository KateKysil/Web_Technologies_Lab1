using LibraryDomain.Model;
using LibraryInfrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using LibraryDomain.Model;

namespace LibraryInfrastructure.Controllers.API
{
    [Route("api/readinglists")]
    [ApiController]
    public class ReadingListsApiController : ControllerBase
    {
        private readonly LibraryContext _db;

        public ReadingListsApiController(LibraryContext db)
        {
            _db = db;
        }

        [HttpPost("{listId}/items")]
        public async Task<IActionResult> AddItem(int listId, [FromBody] ReadingListItem dto)
        {
            var item = new ReadingListItem
            {
                ReadingListId = listId,
                Text = dto.Text,
                IsDone = false
            };

            _db.ReadingListItems.Add(item);
            await _db.SaveChangesAsync();

            return Ok(item);
        }

        [HttpPut("items/{id}/toggle")]
        public async Task<IActionResult> Toggle(int id)
        {
            var item = await _db.ReadingListItems.FindAsync(id);
            item.IsDone = !item.IsDone;
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpDelete("items/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.ReadingListItems.FindAsync(id);
            _db.ReadingListItems.Remove(item);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }

}
