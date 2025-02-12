﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net.Http;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using System.Collections.Generic;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples
{
    public class AdapterWithErrorHandler : CloudAdapter
    {
        private MultiLanguageLG _lgManager;
        public AdapterWithErrorHandler(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<IBotFrameworkHttpAdapter> logger, ConversationState conversationState = null)
            : base(configuration, httpClientFactory, logger)
        {
            Dictionary<string, string> lgFilesPerLocale = new Dictionary<string, string>()
            {
                {"", Path.Combine(".", "Resources", "AdapterWithErrorHandler.lg")},
                {"fr", Path.Combine(".", "Resources", "AdapterWithErrorHandler.fr-fr.lg")}
            };
            _lgManager = new MultiLanguageLG(lgFilesPerLocale);
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync(ActivityFactory.FromObject(_lgManager.Generate("SomethingWentWrong", exception, turnContext.Activity.Locale)));

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Exception caught on attempting to Delete ConversationState : {e.Message}");
                    }
                }
            };
        }
    }
}
