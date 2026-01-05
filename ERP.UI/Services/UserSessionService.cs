using ERP.Core.Models;
using ERP.DAL.Repositories;

namespace ERP.UI.Services
{
    public static class UserSessionService
    {
        private static User _currentUser;
        private static UserPermissionRepository _permissionRepository;

        static UserSessionService()
        {
            _permissionRepository = new UserPermissionRepository();
        }

        public static User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                _currentUser = value;
                if (value != null)
                {
                    _permissionRepository = new UserPermissionRepository();
                }
            }
        }

        public static bool IsLoggedIn => _currentUser != null;

        public static bool HasPermission(string permissionKey)
        {
            if (_currentUser == null)
                return false;

            // Admin kullanıcısı tüm izinlere sahip
            if (_currentUser.IsAdmin)
                return true;

            return _permissionRepository.HasPermission(_currentUser.Id, permissionKey);
        }

        public static void Logout()
        {
            _currentUser = null;
        }
    }
}

