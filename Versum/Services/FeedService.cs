using Microsoft.EntityFrameworkCore;
using Versum.Context;
using Versum.Dtos;
using Versum.Extensions;
using Versum.Models;

namespace Versum.Services
{
    public class FeedService : IFeedService
    {
        private readonly ApplicationDbContext _context;

        public FeedService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PostGetDto>> GetSmartFeedAsync(int currentUserId, int limit = 20)
        {
            var feed = await _context.Posts
                .AsNoTracking()
                .OnlyPublished()
                .Where(p => _context.Follows.Any(f => f.FollowerId == currentUserId && f.FollowingId == p.AuthorId))
                .Where(p => !_context.PostReactions.Any(pr => pr.PostId == p.Id && pr.UserId == currentUserId))
                .OrderByDescending(p => p.CreatedAt)
                .Take(limit)
                .ProjectToPostDto()
                .ToListAsync();

            // добираємо загальні пости, якщо від підписок замало
            if (feed.Count < limit)
            {
                int remainingCount = limit - feed.Count;

                // Звертаємось до PostId, оскільки ProjectToPostDto повертає PostGetDto
                var existingIds = feed.Select(p => p.PostId).ToList();

                var globalTrendingPosts = await _context.Posts
                    .AsNoTracking()
                    .OnlyPublished()
                    .Where(p => !existingIds.Contains(p.Id))
                    .Where(p => !_context.PostReactions.Any(pr => pr.PostId == p.Id && pr.UserId == currentUserId))
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(remainingCount)
                    .ProjectToPostDto()
                    .ToListAsync();

                feed.AddRange(globalTrendingPosts);
            }

            // Записуємо "Перегляди" для всіх відібраних постів
            if (feed.Any())
            {
                var viewsToSave = feed.Select(dto => new PostReaction
                {
                    UserId = currentUserId,
                    PostId = dto.PostId,
                    Type = ReactionType.View,
                    ReactedAt = DateTime.UtcNow
                }).ToList();

                _context.PostReactions.AddRange(viewsToSave);
                await _context.SaveChangesAsync();
            }

            return feed;
        }
    }
}