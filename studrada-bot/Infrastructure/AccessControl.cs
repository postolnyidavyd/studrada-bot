using studrada_bot.Data.Entities;
using studrada_bot.Data.Enums;

namespace studrada_bot.Infrastructure;

public static class AccessControl
{
    public static bool IsAllowed(Member? member) => member?.IsActive ?? false;
    
    public static bool CanExecute(BotAction a, Member m, Post p) => a switch {
        BotAction.Claim   => p.Status == PostStatus.Open,
        BotAction.Release => p.Status == PostStatus.Claimed && p.OwnerId == m.Id,
        BotAction.Submit  => p.Status is PostStatus.Claimed or PostStatus.ChangesRequested
                             && p.OwnerId == m.Id,
        BotAction.Rework  => p.Status == PostStatus.ChangesRequested && p.OwnerId == m.Id,
        BotAction.Approve or
        BotAction.Reject  => p.Status == PostStatus.Pending && m.Role == Role.Admin,
        BotAction.Cancel  => m.Role == Role.Admin
                             || (p.OwnerId == m.Id && p.Status == PostStatus.ChangesRequested),
        _ => false
    };
}