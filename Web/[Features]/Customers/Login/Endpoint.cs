﻿namespace Customers.Login;

public class Endpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/customer/login");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken t)
    {
        var token = JWTBearer.CreateToken(
            signingKey: Config["TokenKey"],
            permissions: new[] { Allow.Customers_Create, Allow.Customers_Update, Allow.Customers_Retrieve },
            roles: new[] { Role.Customer },
            claims: new[] { (Claim.CustomerID, "CST001") });

        return SendAsync(token);
    }
}
