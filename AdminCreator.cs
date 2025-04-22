using EPiServer.Authorization;
using EPiServer.Shell.Security;
using EPiServer.Web;

namespace alloy_bestbets
{
    public class AdminCreator : IBlockingFirstRequestInitializer
    {
        private readonly UIUserProvider _uIUserProvider;
        private readonly UIRoleProvider _uIRoleProvider;
        private readonly UISignInManager _uISignInManager;

        public AdminCreator(UIUserProvider uIUserProvider,
            UISignInManager uISignInManager,
            UIRoleProvider uIRoleProvider)
        {
            _uIUserProvider = uIUserProvider;
            _uISignInManager = uISignInManager;
            _uIRoleProvider = uIRoleProvider;
        }

        public bool CanRunInParallel => false;

        public async Task InitializeAsync(HttpContext httpContext)
        {
            const string username = "alloyadmin";
            const string password = "XcvF1$/o79e*u*l4(*U/";

            if (await IsUserRegistered(username))
            {
                return;
            }

            await CreateUser(username, "admin@admin.com", password, new[] { Roles.Administrators, Roles.WebAdmins });
        }

        private async Task CreateUser(string username, string email, string password, IEnumerable<string> roles)
        {
            var result = await _uIUserProvider.CreateUserAsync(username, password, email, null, null, true);
            if (result.Status == UIUserCreateStatus.Success)
            {
                foreach (var role in roles)
                {
                    var exists = await _uIRoleProvider.RoleExistsAsync(role);
                    if (!exists)
                    {
                        await _uIRoleProvider.CreateRoleAsync(role);
                    }
                }

                await _uIRoleProvider.AddUserToRolesAsync(result.User.Username, roles);
                var resFromSignIn = await _uISignInManager.SignInAsync(username, password);
            }
        }

        private async Task<bool> IsUserRegistered(string username)
        {
            var res = await _uIUserProvider.GetUserAsync(username);
            return res != null;
        }
    }
}
