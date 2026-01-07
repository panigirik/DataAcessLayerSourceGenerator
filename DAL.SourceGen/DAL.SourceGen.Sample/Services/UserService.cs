using System.Threading.Tasks;
using DAL.SourceGen.Sample.Entities;
using DAL.SourceGen.Sample.Repositories;

namespace DAL.SourceGen.Sample.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task Add()
    {
        var newUser = new User { Name = "username"};
        _userRepository.Add(newUser);
        return Task.CompletedTask;
    }
}