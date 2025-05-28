using System;
using API.Entities;

namespace API.Interfaces;

public interface ITagsRepository
{
    Task<List<Tag>> GetTagsByNamesAsync(List<string> tags);
    Task<List<Tag>> GetAllTagsAsync();
    Task<Tag?> GetTagByNameAsync(string name);
    Task RemoveTagByName(string name);
}
