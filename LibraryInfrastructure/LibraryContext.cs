using Humanizer.Localisation;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LibraryDomain.Model;

namespace LibraryInfrastructure
{
    public class LibraryContext : IdentityDbContext<User>
    {
        public LibraryContext(DbContextOptions<LibraryContext> options)
            : base(options)
        {
        }

        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Book> Books => Set<Book>();
        public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
        public DbSet<BookGenre> BookGenres => Set<BookGenre>();
        public DbSet<Genre> Genres => Set<Genre>();
        public DbSet<Publisher> Publishers => Set<Publisher>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Shelf> Shelves => Set<Shelf>();
        public DbSet<ShelfBook> ShelfBooks => Set<ShelfBook>();
        public DbSet<UserLibrary> UserLibraries => Set<UserLibrary>();
        public DbSet<ReadingList> ReadingLists =>Set<ReadingList>();
        public DbSet<ReadingListTheme> ReadingListThemes => Set<ReadingListTheme>();
        public DbSet<ReadingListItem> ReadingListItems => Set<ReadingListItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite Keys
            modelBuilder.Entity<BookAuthor>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });

            modelBuilder.Entity<BookGenre>()
                .HasKey(bg => new { bg.BookId, bg.GenreId });

            // Relationships
            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Book)
                .WithMany(b => b.BookAuthors)
                .HasForeignKey(ba => ba.BookId);

            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Author)
                .WithMany(a => a.BookAuthors)
                .HasForeignKey(ba => ba.AuthorId);

            modelBuilder.Entity<BookGenre>()
                .HasOne(bg => bg.Book)
                .WithMany(b => b.BookGenres)
                .HasForeignKey(bg => bg.BookId);

            modelBuilder.Entity<BookGenre>()
                .HasOne(bg => bg.Genre)
                .WithMany(g => g.BookGenres)
                .HasForeignKey(bg => bg.GenreId);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Book)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BookId);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Shelf>()
                .HasOne(s => s.User)
                .WithMany(u => u.Shelves)
                .HasForeignKey(s => s.UserId);

            modelBuilder.Entity<UserLibrary>()
                .HasOne(ul => ul.User)
                .WithMany(u => u.UserLibraries)
                .HasForeignKey(ul => ul.UserId);

            modelBuilder.Entity<UserLibrary>()
                .HasOne(ul => ul.Book)
                .WithMany(b => b.UserLibraries)
                .HasForeignKey(ul => ul.BookId);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Publisher)
                .WithMany(p => p.Books)
                .HasForeignKey(b => b.PublisherId);

            modelBuilder.Entity<ReadingList>()
    .HasMany(rl => rl.Themes)
    .WithOne(t => t.ReadingList)
    .HasForeignKey(t => t.ReadingListId)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReadingList>()
                .HasOne(rl => rl.Owner)
                .WithMany(u => u.ReadingLists)
                .HasForeignKey(rl => rl.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReadingListTheme>()
                .HasMany(t => t.Items)
                .WithOne(i => i.Theme)
                .HasForeignKey(i => i.ThemeId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
