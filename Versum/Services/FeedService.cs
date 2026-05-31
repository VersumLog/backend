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

        private const float BASE_PRIORITY = 100.0f;
        private const float FOLLOW_BONUS = 20.0f;
        private const float VIEW_PENALTY = 10.0f;

        public FeedService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PostGetDto>> GetSmartFeedAsync(int? currentUserId, int limit = 20, int skip = 0)
        {
            // ЛОГІКА ДЛЯ ГОСТЕЙ
            if (!currentUserId.HasValue)
            {
                return await _context.Posts
                    .AsNoTracking()
                    .OnlyPublished()
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip(skip)
                    .Take(limit)
                    .ProjectToPostDto(currentUserId)
                    .ToListAsync();
            }
            // ЛОГІКА ДЛЯ ЗАРЕЄСТРОВАНИХ
            int userId = currentUserId.Value;

            var metadata = await _context.Posts
                .AsNoTracking()
                .OnlyPublished()
                .Where(p => p.AuthorId != userId)
                .Select(p => new
                {
                    PostId = p.Id,
                    Reaction = _context.PostReactions.FirstOrDefault(pr => pr.PostId == p.Id && pr.UserId == userId),
                    IsFollowed = _context.Follows.Any(f => f.FollowerId == userId && f.FollowingId == p.AuthorId),
                    CreatedAt = p.CreatedAt
                })
                .OrderByDescending(x => x.Reaction != null
                    ? x.Reaction.PriorityScore
                    : (BASE_PRIORITY + (x.IsFollowed ? FOLLOW_BONUS : 0.0f)))
                .ThenByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            if (!metadata.Any())
            {
                return new List<PostGetDto>();
            }

            var postIds = metadata.Select(x => x.PostId).ToList();

            var dtos = await _context.Posts
                .AsNoTracking()
                .Where(p => postIds.Contains(p.Id))
                .ProjectToPostDto(currentUserId)
                .ToListAsync();

            var feedDtos = postIds
                .Select(id => dtos.First(d => d.PostId == id))
                .ToList();

            var postsToUpdate = new List<PostReaction>();
            var postsToAdd = new List<PostReaction>();

            foreach (var item in metadata)
            {
                if (item.Reaction == null)
                {
                    float initialScore = BASE_PRIORITY + (item.IsFollowed ? FOLLOW_BONUS : 0.0f);

                    postsToAdd.Add(new PostReaction
                    {
                        UserId = userId,
                        PostId = item.PostId,
                        ViewCount = 1,
                        PriorityScore = initialScore - VIEW_PENALTY,
                        LastInteractedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    item.Reaction.ViewCount += 1;
                    item.Reaction.PriorityScore -= VIEW_PENALTY;
                    item.Reaction.LastInteractedAt = DateTime.UtcNow;

                    postsToUpdate.Add(item.Reaction);
                }
            }

            if (postsToAdd.Any()) _context.PostReactions.AddRange(postsToAdd);
            if (postsToUpdate.Any()) _context.PostReactions.UpdateRange(postsToUpdate);

            await _context.SaveChangesAsync();

            return feedDtos;
        }
    }
}