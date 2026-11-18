

namespace USPSimGame.Data.Models;

public record JoinRequest(
    string PlayerId,
    string UserName,
    int TeamId
);