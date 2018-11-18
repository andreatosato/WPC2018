// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DFWebJobsSample
{
    public static class Program
    {
        static void Main(string[] args)
        {
            using (var loggerFactory = new LoggerFactory())
            {
                var config = new JobHostConfiguration();
                config.DashboardConnectionString = "UseDevelopmentStorage=true";                
                config.LoggerFactory = loggerFactory
                    .AddConsole();

                config.UseTimers();
                config.UseDurableTask(new DurableTaskExtension
                {
                    HubName = "DemoDurableLocal",
                });
                var host = new JobHost(config);
                host.RunAndBlock();
            }
        }
    }
}
