﻿//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Procare Software, LLC">
//     Copyright © 2021-2024 Procare Software, LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Procare.AddressValidation.Tester;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

internal sealed class Program
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Main should not leak any exceptions.")]
    private static async Task Main()
    {
        Uri addressValidationBaseUrl = new("https://addresses.dev-procarepay.com");

        using HttpClientFactory factory = new();
        using AddressValidationService addressService = new(factory, false, addressValidationBaseUrl);

        // AddressValidationRequest request = new() { Line1 = "1 W Main", City = "Medford", StateCode = "OR", ZipCodeLeading5 = "97501" };
        // AddressValidationRquest request = new();
        AddressValidationRequest request = new() { Line1 = "1125 17th St Ste 1800", City = "Denver", StateCode = "CO", ZipCodeLeading5 = "80202" };

        var start = Stopwatch.GetTimestamp();
        try
        {
            var response = await addressService.GetAddressesAsync(request).ConfigureAwait(false);
            Console.WriteLine(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        Console.WriteLine(Stopwatch.GetElapsedTime(start));
        Console.WriteLine();

        // Added to view exception during testing
        Console.ReadKey();
    }
}
