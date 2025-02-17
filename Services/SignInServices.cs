// using SuppliesManagement.IServices;

// namespace SuppliesManagement.Services
// {
//     public class SignInService : ISignInService
//     {
//         private readonly IUserRepository _userRepository;

//         public SignInService(IUserRepository userRepository)
//         {
//             _userRepository = userRepository;
//         }

//         public async Task<bool> OnGet(string username, string password)
//         {
//             var user = await _userRepository.GetUserByUsernameAsync(username);
//             if (user == null)
//             {
//                 return false; // User not found
//             }
//             return user.Password == password;
//         }
//     }
// }
