using QuickDrop.Data;
using QuickDrop.Models;

namespace QuickDrop.Services;

public static class AuthService
{
    public static AppUser? Current(QuickDropDbContext db, HttpContext context)
    {
        var id = context.Session.GetInt32("UserId");
        return id.HasValue ? db.Users.FirstOrDefault(x => x.Id == id.Value) : null;
    }

    public static void SignIn(HttpContext context, AppUser user)
        => context.Session.SetInt32("UserId", user.Id);

    public static void SignOut(HttpContext context)
        => context.Session.Clear();
}
