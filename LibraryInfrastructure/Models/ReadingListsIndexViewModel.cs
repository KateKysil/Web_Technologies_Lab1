using LibraryDomain.Model;
using Microsoft.AspNetCore.Mvc;

namespace LibraryInfrastructure.Models
{
    public class ReadingListsIndexViewModel
    {
        public List<ReadingList> MyLists { get; set; }
        public List<ReadingList> PublicLists { get; set; }
    }
}
