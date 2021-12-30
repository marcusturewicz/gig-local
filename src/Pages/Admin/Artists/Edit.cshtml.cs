﻿namespace GigLocal.Pages.Admin.Artists;

public class EditModel : PageModel
{
    private readonly GigContext _context;
    private readonly IStorageService _storageService;

    public EditModel(GigContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    [BindProperty]
    public ArtistCreateModel Artist { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var artist = await _context.Artists
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(m => m.ID == id);

        if (artist == null)
        {
            return NotFound();
        }

        Artist = new ArtistCreateModel
        {
            Name = artist.Name,
            Description = artist.Description,
            Website = artist.Website
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
            return Page();

        var artistToUpdate = await _context.Artists.FindAsync(id);

        if (artistToUpdate == null)
        {
            return NotFound();
        }

        artistToUpdate.Name = Artist.Name;
        artistToUpdate.Description = Artist.Description;
        artistToUpdate.Website = Artist.Website;

        if (Artist.FormFile?.Length > 0)
        {
            using var formFileStream = Artist.FormFile.OpenReadStream();
            var imageUrl = await _storageService.UploadArtistImageAsync(artistToUpdate.ID, formFileStream);

            artistToUpdate.ImageUrl = imageUrl;
        }

        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
}
