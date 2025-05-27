using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class TagsRepository(DataContext context) : ITagsRepository
{
    private readonly DataContext _context = context;
    public async Task<List<Tag>> GetAllTagsAsync()
    {
        return await _context.Tags.ToListAsync();
    }

    public async Task<List<Tag>> GetTagsByNamesAsync(List<string> tags)
    {
        if (tags == null || !tags.Any())
                return new List<Tag>();

            var distinctNames = tags
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var allTags = await _context.Tags.ToListAsync();

            var matchingTags = allTags
                .Where(t => distinctNames.Contains(t.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var existingNames = matchingTags
                .Select(t => t.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var name in distinctNames)
            {
                if (!existingNames.Contains(name))
                {
                    matchingTags.Add(new Tag { Name = name });
                    existingNames.Add(name);
                }
            }
            return matchingTags;
    }
}
