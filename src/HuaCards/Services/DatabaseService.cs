
using HuaCards.Data;

namespace HuaCards.Services;

public static class DatabaseService
{
    private static HuaCardsDbContext? _ctx;
    public static HuaCardsDbContext Context => _ctx ??= new HuaCardsDbContext();
}
