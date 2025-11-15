using LibraryDomain.Model;
using LibraryInfrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // GET: api/readinglists/{listId}/themes?skip=0&limit=5
        [HttpGet("{listId}/themes")]
        public async Task<IActionResult> GetThemes(int listId, int skip = 0, int limit = 10)
        {
            var themes = await _db.ReadingListThemes
                .Where(t => t.ReadingListId == listId)
                .Include(t => t.Items)
                .Skip(skip)
                .Take(limit)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    Items = t.Items.Select(i => new { i.Id, i.Text, i.IsDone })
                })
                .ToListAsync();

            return Ok(themes);
        }

        // POST: api/readinglists/{listId}/themes
        [HttpPost("{listId}/themes")]
        public async Task<IActionResult> AddTheme(int listId, [FromBody] AddThemeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { error = "Title is required" });

            var theme = new ReadingListTheme
            {
                ReadingListId = listId,
                Name = dto.Title
            };

            _db.ReadingListThemes.Add(theme);
            await _db.SaveChangesAsync();

            return Ok(new { theme.Id, theme.Name });
        }

        // POST: api/readinglists/themes/{themeId}/items
        [HttpPost("themes/{themeId}/items")]
        public async Task<IActionResult> AddItemToTheme(int themeId, [FromBody] AddThemeItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Text))
                return BadRequest(new { error = "Text is required" });

            var theme = await _db.ReadingListThemes.FindAsync(themeId);
            if (theme == null) return NotFound(new { error = "Theme not found" });

            var item = new ReadingListItem
            {
                ThemeId = themeId,
                Text = dto.Text,
                IsDone = false
            };

            _db.ReadingListItems.Add(item);
            await _db.SaveChangesAsync();

            return Ok(new { item.Id, item.Text, item.IsDone });
        }

        // PUT: api/readinglists/items/{id}/toggle
        [HttpPut("items/{id}/toggle")]
        public async Task<IActionResult> ToggleItem(int id)
        {
            var item = await _db.ReadingListItems.FindAsync(id);
            if (item == null) return NotFound(new { error = "Item not found" });

            item.IsDone = !item.IsDone;
            await _db.SaveChangesAsync();

            return Ok(new { item.Id, item.IsDone });
        }

        // DELETE: api/readinglists/items/{id}
        [HttpDelete("items/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _db.ReadingListItems.FindAsync(id);
            if (item == null) return NotFound(new { error = "Item not found" });

            _db.ReadingListItems.Remove(item);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
