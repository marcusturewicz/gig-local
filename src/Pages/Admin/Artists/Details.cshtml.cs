﻿namespace GigLocal.Pages.Admin.Artists;

public class DetailsModel : PageModel
{
    private readonly GigContext _context;

    public DetailsModel(GigContext context)
    {
        _context = context;
    }

    public ArtistDetialsModel Artist { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (!ModelState.IsValid)
            return Page();

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

        Artist = new ArtistDetialsModel
        {
            ID = artist.ID,
            Name = artist.Name,
            Description = artist.Description,
            Website = artist.Website,
            ImageUrl = artist.ImageUrl
        };

        return Page();
    }
}

public class ArtistDetialsModel : ArtistReadModel
{
    public int ID { get; set; }
}
