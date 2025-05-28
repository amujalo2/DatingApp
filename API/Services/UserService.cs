using System;
using System.Threading.Tasks;
using API.Interfaces;

namespace API.Services;

public class UserService(IUnitOfWork unitOfWork)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task UpdateLastActive(int userId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        user.LastActive = DateTime.UtcNow;

        if (!await _unitOfWork.Complete())
        {
            throw new Exception("Failed to update user's last active time");
        }
    }
}