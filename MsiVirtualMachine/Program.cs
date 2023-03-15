// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Abstractions;

internal class Program
{
    private static async Task Main(string[] args)
    {
        IIdentityLogger identityLogger = new IdentityLogger();

        string? scope = "https://management.azure.com";

        do
        {
            try
            {
                Console.WriteLine("Enter the scope to acquire token.");
                scope = Console.ReadLine();

                Console.WriteLine("1. Acquire token for system assigned managed identity.");
                Console.WriteLine("2. Acquire token for user assigned managed identity.");
                Console.WriteLine("Select an option.");

                string? input = Console.ReadLine() ?? throw new ArgumentNullException("input");
                int option = int.Parse(input);
                
                switch (option)
                {
                    case 1:
                        Console.WriteLine("Acquiring token for system assigned managed identity");

                        IManagedIdentityApplication sami = ManagedIdentityApplicationBuilder.Create()
                            .WithExperimentalFeatures()
                            .WithLogging(identityLogger, true)
                            .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                            .Build();

                        var result = await sami.AcquireTokenForManagedIdentity(scope)
                            .ExecuteAsync().ConfigureAwait(false);

                        DisplaySuccessfulResult(result);
                        break;

                    case 2:
                        Console.WriteLine("Acquiring token for user assigned managed identity");
                        Console.WriteLine("Enter a value for user assigned client id or resource id as displayed in azure portal.");
                        string? userAssignedId = Console.ReadLine();

                        IManagedIdentityApplication uami = ManagedIdentityApplicationBuilder.Create(userAssignedId)
                            .WithExperimentalFeatures()
                            .WithLogging(identityLogger, true)
                            .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                            .Build();

                        var uamiResult = await uami.AcquireTokenForManagedIdentity(scope)
                            .ExecuteAsync().ConfigureAwait(false);

                        DisplaySuccessfulResult(uamiResult);
                        break;

                    default:
                        Console.WriteLine("Invalid option selected, try again!");
                        break;
                }
            }
            catch (MsalServiceException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.ErrorCode);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
        } while (!string.IsNullOrEmpty(scope));
    }

    private static void DisplaySuccessfulResult(AuthenticationResult result)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Success");
        Console.WriteLine("Token Source: " + result.AuthenticationResultMetadata.TokenSource);
        Console.WriteLine("Expires on: " + result.ExpiresOn);
    }
}

class IdentityLogger : IIdentityLogger
{
    public EventLogLevel MinLogLevel { get; }

    public IdentityLogger()
    {
        MinLogLevel = EventLogLevel.Verbose;
    }

    public bool IsEnabled(EventLogLevel eventLogLevel)
    {
        return eventLogLevel <= MinLogLevel;
    }

    public void Log(LogEntry entry)
    {
        //Log Message here:
        Console.WriteLine(entry.Message);
    }
}
