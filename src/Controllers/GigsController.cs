﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GigLocal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GigLocal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GigsController : ControllerBase
    {
        private readonly ILogger<GigsController> _logger;
        private readonly GigContext _context;
        private readonly int _pageSize;

        public GigsController(ILogger<GigsController> logger,
            GigContext context)
        {
            _logger = logger;
            _context = context;
            _pageSize = 20;
        }

        [HttpGet]
        public async Task<IEnumerable<GigRecord>> Get(string startDate, string endDate, int page)
        {
            var startDateTime = DateTime.Parse(startDate);
            var endDateTime = DateTime.Parse(endDate);
            var result = await _context.Gigs
                            .Include(g => g.Artist)
                            .Include(g => g.Venue)
                            .Where(g => g.Date >= startDateTime && g.Date <= endDateTime)
                            .Select(g => new {
                                Date = g.Date,
                                TicketPrice = g.TicketPrice,
                                TicketWebsite = g.TicketWebsite,
                                ArtistName = g.Artist.Name,
                                ArtistGenre = g.Artist.Genre,
                                ArtistWebsite = g.Artist.Website,
                                VenueName = g.Venue.Name,
                                VenueWebsite = g.Venue.Website,
                                VenueAddress = g.Venue.Address
                            })
                            .OrderBy(g => g.Date)
                            .Skip(page * _pageSize)
                            .Take(_pageSize)
                            .ToArrayAsync();

            return result.Select(g => new GigRecord(
                g.Date.ToDayOfWeekDateMonthName(),
                g.Date.ToTimeHourMinuteAmPm(),
                g.TicketPrice,
                g.TicketWebsite,
                g.ArtistName,
                g.ArtistGenre,
                g.ArtistWebsite,
                g.VenueName,
                g.VenueWebsite,
                g.VenueAddress));
        }
    }

    public record GigRecord
    (
        string Date,
        string Time,
        Decimal TicketPrice,
        string TicketWebsite,
        string ArtistName,
        string ArtistGenre,
        string ArtistWebsite,
        string VenueName,
        string VenueWebsite,
        string VenueAddres
    );
}