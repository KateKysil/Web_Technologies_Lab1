using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryDomain.Model
{
    public class ReadingList
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool isPublic { get; set; }
        public string OwnerId { get; set; }
        public User Owner { get; set; }

        public ICollection<ReadingListTheme> Themes { get; set; }
    }


    public class ReadingListTheme
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int ReadingListId { get; set; }
        public ReadingList ReadingList { get; set; }

        public ICollection<ReadingListItem> Items { get; set; }
    }


    public class ReadingListItem
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public bool IsDone { get; set; }
        public int ThemeId { get; set; }
        public ReadingListTheme Theme { get; set; }

    }

    public class AddThemeDto
    {
        public string Title { get; set; }
    }

    public class AddThemeItemDto
    {
        public string Text { get; set; }
    }


}
