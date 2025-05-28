using API.Data;

namespace API.Interfaces;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IMessageRepository MessageRepository { get; }
    ILikesRepository LikesRepository { get; }
    IPhotoRepository PhotoRepository { get; }
    ITagsRepository TagRepository { get; }
    Task<bool> Complete();
    bool HasChanges();
}
