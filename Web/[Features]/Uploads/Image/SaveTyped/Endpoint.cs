﻿namespace Uploads.Image.SaveTyped;

public class Endpoint : Endpoint<Request>
{
    public override void Configure()
    {
        Verbs(Http.POST, Http.PUT);
        AllowAnonymous(Http.POST);
        Routes("uploads/image/save-typed");
        Permissions(Allow.Image_Update);
        Claims(Claim.AdminID);
        AllowFileUploads();
        Options(b =>
            b.Produces(200, contentType: "image/png", additionalContentTypes: "test/image")
             .Produces(204, contentType: "text/plain", additionalContentTypes: "test/notcontent"));
    }

    public override async Task HandleAsync(Request r, CancellationToken ct)
    {
        if (r.File1 is not null && r.File2 is not null)
        {
            await SendStreamAsync(r.File1.OpenReadStream(), "test.png", r.File1.Length, "image/png", cancellation: ct);
            return;
        }

        await SendNoContentAsync();
    }
}
