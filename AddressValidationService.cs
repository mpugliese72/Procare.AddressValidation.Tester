//-----------------------------------------------------------------------
// <copyright file="AddressValidationService.cs" company="Procare Software, LLC">
//     Copyright © 2021-2024 Procare Software, LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Procare.AddressValidation.Tester;

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class AddressValidationService : BaseHttpService
{
    public string? errorMsg { get; set; } = default!;
    public AddressValidationService(IHttpClientFactory httpClientFactory, bool disposeFactory, Uri baseUrl)
        : this(httpClientFactory, disposeFactory, baseUrl, null, false)
    {
    }

    protected AddressValidationService(IHttpClientFactory httpClientFactory, bool disposeFactory, Uri baseUrl, HttpMessageHandler? httpMessageHandler, bool disposeHandler)
        : base(httpClientFactory, disposeFactory, baseUrl, httpMessageHandler, disposeHandler)
    {
    }

    public async Task<string> GetAddressesAsync(AddressValidationRequest request, CancellationToken token = default)
    {
        using var httpRequest = request.ToHttpRequest(this.BaseUrl);
        using var response = await this.CreateClient().SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);

        var start = Stopwatch.GetTimestamp();
        var remainingTries = 3;

        do
        {
            try
            {
                if ((int)response.StatusCode >= 500 && (int)response.StatusCode < 600)
                {
                    // retry the request
                    await GetAddressesAsync(request).ConfigureAwait(false);
                    remainingTries--;
                }
                else if (start > 750.00000)
                {
                    // cancel the token and retry
                    CancellationTokenSource source = new();
                    token = source.Token;
                    source.Cancel();
                    remainingTries--;
                }
                else
                {
                    // return successful status code result
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("The following exception has occurred: ", ex);
            }
        }
        while (remainingTries > 0);

        if (remainingTries == 0)
        {
            throw new InvalidOperationException("Your call has reached the maximum number of retries.");
        }

        return await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
    }
}
