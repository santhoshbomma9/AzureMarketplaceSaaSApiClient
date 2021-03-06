﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SaaSFulfillmentClient.AzureAD;
using SaaSFulfillmentClient.Models;

namespace SaaSFulfillmentClient
{
    public class FulfillmentClient : RestClient<FulfillmentClient>, IFulfillmentClient
    {
        private const string mockApiVersion = "2018-09-15";
        private readonly IOperationsStore operationsStore;

        public FulfillmentClient(IOptionsMonitor<SecuredFulfillmentClientConfiguration> optionsMonitor,
                                 ILogger<FulfillmentClient> logger) : this(null,
            optionsMonitor.CurrentValue,
            null,
            logger)
        {
        }

        public FulfillmentClient(IOptionsMonitor<SecuredFulfillmentClientConfiguration> optionsMonitor,
                                 IOperationsStore operationsStore,
                                ILogger<FulfillmentClient> logger) : this(null,
                                optionsMonitor.CurrentValue,
                                operationsStore,
                                logger)
        {
        }

        public FulfillmentClient(SecuredFulfillmentClientConfiguration options,
                                 IOperationsStore operationsStore,
                                 ILogger<FulfillmentClient> logger) : this(null,
            options,
            operationsStore,
            logger)
        {
        }

        public FulfillmentClient(
            HttpMessageHandler httpMessageHandler,
            SecuredFulfillmentClientConfiguration options,
            IOperationsStore operationsStore,
            ILogger<FulfillmentClient> logger) : base(options, logger, httpMessageHandler)
        {
            this.operationsStore = operationsStore;
        }

        public async Task<FulfillmentRequestResult> ActivateSubscriptionAsync(Guid subscriptionId,
            ActivatedSubscription subscriptionDetails, Guid requestId, Guid correlationId,
            CancellationToken cancellationToken)
        {
            var requestUrl = FluentUriBuilder
                .Start(this.baseUri)
                .AddPath("subscriptions")
                .AddPath(subscriptionId.ToString())
                .AddPath("activate")
                .AddQuery(DefaultApiVersionParameterName, this.apiVersion)
                .Uri;

            requestId = requestId == default ? Guid.NewGuid() : requestId;
            correlationId = correlationId == default ? Guid.NewGuid() : correlationId;

            var response = await this.SendRequestAndReturnResult(
                HttpMethod.Post,
                requestUrl,
                requestId,
                correlationId,
                null,
                JsonConvert.SerializeObject(subscriptionDetails),
                cancellationToken);

            return await FulfillmentRequestResult.ParseAsync<FulfillmentRequestResult>(response);
        }

        public async Task<UpdateOrDeleteSubscriptionRequestResult> DeleteSubscriptionAsync(Guid subscriptionId,
            Guid requestId, Guid correlationId, CancellationToken cancellationToken)
        {
            var requestUrl = FluentUriBuilder
                .Start(this.baseUri)
                .AddPath("subscriptions")
                .AddPath(subscriptionId.ToString())
                .AddQuery(DefaultApiVersionParameterName, this.apiVersion)
                .Uri;

            requestId = requestId == default ? Guid.NewGuid() : requestId;
            correlationId = correlationId == default ? Guid.NewGuid() : correlationId;

            var response = await this.SendRequestAndReturnResult(
                HttpMethod.Delete,
                requestUrl,
                requestId,
                correlationId,
                null,
                "",
                cancellationToken);

            var result = await FulfillmentRequestResult.ParseAsync<UpdateOrDeleteSubscriptionRequestResult>(response);

            if (this.operationsStore != default)
            {
                await this.operationsStore.RecordAsync(subscriptionId, result, cancellationToken);
            }

            return result;
        }

        public async Task<IEnumerable<SubscriptionOperation>> GetOperationsAsync(Guid requestId, Guid correlationId,
            CancellationToken cancellationToken)
        {
            var requestUrl = FluentUriBuilder
                .Start(this.baseUri)
                .AddPath("operations")
                .AddQuery(DefaultApiVersionParameterName, this.apiVersion)
                .Uri;

            requestId = requestId == default ? Guid.NewGuid() : requestId;
            correlationId = correlationId == default ? Guid.NewGuid() : correlationId;

            var response = await this.SendRequestAndReturnResult(HttpMethod.Get,
                requestUrl,
                requestId,
                correlationId,
                null,
                "",
                cancellationToken);

            return await FulfillmentRequestResult.ParseMultipleAsync<SubscriptionOperation>(response);
        }

        public async Task<Subscription> GetSubscriptionAsync(Guid subscriptionId, Guid requestId, Guid correlationId,
            CancellationToken cancellationToken)
        {
            var requestUrl = FluentUriBuilder
                .Start(this.baseUri)
                .AddPath("subscriptions")
                .AddPath(subscriptionId.ToString())
                .AddQuery(DefaultApiVersionParameterName, this.apiVersion)
                .Uri;

            requestId = requestId == default ? Guid.NewGuid() : requestId;
            correlationId = correlationId == default ? Guid.NewGuid() : correlationId;

            var response = await this.SendRequestAndReturnResult(HttpMethod.Get,
                requestUrl,
                requestId,
                correlationId,
                null,
                "",
                cancellationToken);

            return await FulfillmentRequestResult.ParseAsync<Subscription>(response);
        }

        public async Task<SubscriptionOperation> GetSubscriptionOperationAsync(Guid subscriptionId, Guid operationId,
            Guid requestId, Guid correlationId, CancellationToken cancellationToken)
        {
            var requestUrl = FluentUriBuilder
                .Start(this.baseUri)
                .AddPath("subscriptions")
                .AddPath(subscriptionId.ToString())
                .AddPath("operations")
                .AddPath(operationId.ToString())
                .AddQuery(DefaultApiVersionParameterName, this.apiVersion)
                .Uri;

            requestId = requestId == default ? Guid.NewGuid() : requestId;
            correlationId = correlationId == default ? Guid.NewGuid() : correlationId;

            var response = await this.SendRequestAndReturnResult(
                HttpMethod.Get,
                requestUrl,
                requestId,
                correlationId,
                null,
                "",
                cancellationToken);

            return await FulfillmentRequestResult.ParseAsync<SubscriptionOperation>(response);
        }

        public async Task<IEnumerable<SubscriptionOperation>> GetSubscriptionOperationsAsync(Guid subscriptionId,
            Guid requestId, Guid correlationId, CancellationToken cancellationToken)
        {
            var requestUrl = FluentUriBuilder
                .Start(this.baseUri)
                .AddPath("subscriptions")
                .AddPath(subscriptionId.ToString())
                .AddPath("operations")
                .AddQuery(DefaultApiVersionParameterName, this.apiVersion)
                .Uri;

            requestId = requestId == default ? Guid.NewGuid() : requestId;
            correlationId = correlationId == default ? Guid.NewGuid() : correlationId;

            var response = await this.SendRequestAndReturnResult(HttpMethod.Get,
                requestUrl,
                requestId,
                correlationId,
                null,
                "",
                cancellationToken);

            if (this.apiVersion == mockApiVersion)
            {
                return await FulfillmentRequestResult.ParseMultipleAsync<SubscriptionOperation>(response);
            }

            return (await FulfillmentRequestResult.ParseAsync<SubscriptionOperationResult>(response)).Operations;
        }

        public async Task<SubscriptionPlans> GetSubscriptionPlansAsync(Guid subscriptionId, Guid requestId,
            Guid correlationId, CancellationToken cancellationToken)
        {
            var requestUrl = FluentUriBuilder
                .Start(this.baseUri)
                .AddPath("subscriptions")
                .AddPath(subscriptionId.ToString())
                .AddPath("listAvailablePlans")
                .AddQuery(DefaultApiVersionParameterName, this.apiVersion)
                .Uri;

            requestId = requestId == default ? Guid.NewGuid() : requestId;
            correlationId = correlationId == default ? Guid.NewGuid() : correlationId;

            var response = await this.SendRequestAndReturnResult(HttpMethod.Get,
                requestUrl,
                requestId,
                correlationId,
                null,
                "",
                cancellationToken);

            return await FulfillmentRequestResult.ParseAsync<SubscriptionPlans>(response);
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsAsync(
            Guid requestId,
            Guid correlationId,
            CancellationToken cancellationToken)
        {
            var requestUrl = FluentUriBuilder
                .Start(this.baseUri)
                .AddPath("subscriptions")
                .AddQuery(DefaultApiVersionParameterName, this.apiVersion)
                .Uri;

            requestId = requestId == default ? Guid.NewGuid() : requestId;
            correlationId = correlationId == default ? Guid.NewGuid() : correlationId;

            var response = await this.SendRequestAndReturnResult(HttpMethod.Get,
                requestUrl,
                requestId,
                correlationId,
                null,
                "",
                cancellationToken);

            var result = await FulfillmentRequestResult.ParseAsync<SubscriptionResult>(response);
            var subscriptions = new List<Subscription>(result.Subscriptions);

            while (!string.IsNullOrEmpty(result.NextLink))
            {
                requestId = Guid.NewGuid();
                response = await this.SendRequestAndReturnResult(HttpMethod.Get,
                    new Uri(result.NextLink),
                    requestId,
                    correlationId,
                    null,
                    "",
                    cancellationToken);
                
                result = await FulfillmentRequestResult.ParseAsync<SubscriptionResult>(response);
                subscriptions.AddRange(result.Subscriptions);
            }

            return subscriptions;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="marketplaceToken">
        /// Token query parameter in the URL when the user is redirected to the SaaS ISV’s website from Azure.
        /// Note: The URL decodes the token value from the browser before using it.
        /// This token is valid only for 1 hour
        /// </param>
        /// <param name="requestId"></param>
        /// <param name="correlationId"></param>
        /// <param name="bearerToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ResolvedSubscription> ResolveSubscriptionAsync(string marketplaceToken, Guid requestId,
            Guid correlationId, CancellationToken cancellationToken)
        {
            var requestUrl = FluentUriBuilder
                .Start(this.baseUri)
                .AddPath("subscriptions")
                .AddPath("resolve")
                .AddQuery(DefaultApiVersionParameterName, this.apiVersion)
                .Uri;

            requestId = requestId == default ? Guid.NewGuid() : requestId;
            correlationId = correlationId == default ? Guid.NewGuid() : correlationId;

            var response = await this.SendRequestAndReturnResult(HttpMethod.Post,
                requestUrl,
                requestId,
                correlationId,
                r =>
                {
                    r.Headers.Add("x-ms-marketplace-token", marketplaceToken);
                },
                "",
                cancellationToken);

            return await FulfillmentRequestResult.ParseAsync<ResolvedSubscription>(response);
        }

        public async Task<FulfillmentRequestResult> UpdateSubscriptionOperationAsync(
            Guid subscriptionId,
            Guid operationId,
            OperationUpdate update,
            Guid requestId,
            Guid correlationId,
            CancellationToken cancellationToken)
        {
            var requestUrl = FluentUriBuilder
                .Start(this.baseUri)
                .AddPath("subscriptions")
                .AddPath(subscriptionId.ToString())
                .AddPath("operations")
                .AddPath(operationId.ToString())
                .AddQuery(DefaultApiVersionParameterName, this.apiVersion)
                .Uri;

            requestId = requestId == default ? Guid.NewGuid() : requestId;
            correlationId = correlationId == default ? Guid.NewGuid() : correlationId;

            var response = await this.SendRequestAndReturnResult(
                               new HttpMethod("PATCH"),
                               requestUrl,
                               requestId,
                               correlationId,
                               null,
                               JsonConvert.SerializeObject(update),
                               cancellationToken);

            return await FulfillmentRequestResult.ParseAsync<ResolvedSubscription>(response);
        }

        public async Task<UpdateOrDeleteSubscriptionRequestResult> UpdateSubscriptionPlanAsync(
            Guid subscriptionId,
            string planId,
            Guid requestId,
            Guid correlationId,
            CancellationToken cancellationToken)
        {
            var requestUrl = FluentUriBuilder
                .Start(this.baseUri)
                .AddPath("subscriptions")
                .AddPath(subscriptionId.ToString())
                .AddQuery(DefaultApiVersionParameterName, this.apiVersion)
                .Uri;

            requestId = requestId == default ? Guid.NewGuid() : requestId;
            correlationId = correlationId == default ? Guid.NewGuid() : correlationId;
            var updateContent = JsonConvert.SerializeObject(new { planId = planId });

            var response = await this.SendRequestAndReturnResult(
                               new HttpMethod("PATCH"),
                               requestUrl,
                               requestId,
                               correlationId,
                               null,
                               updateContent,
                               cancellationToken);
            var result = await FulfillmentRequestResult.ParseAsync<UpdateOrDeleteSubscriptionRequestResult>(response);

            if (this.operationsStore != default)
            {
                await this.operationsStore.RecordAsync(subscriptionId, result, cancellationToken);
            }

            return result;
        }

        public async Task<UpdateOrDeleteSubscriptionRequestResult> UpdateSubscriptionQuantityAsync(
            Guid subscriptionId,
            int quantity,
            Guid requestId,
            Guid correlationId,
            CancellationToken cancellationToken)
        {
            var requestUrl = FluentUriBuilder
                .Start(this.baseUri)
                .AddPath("subscriptions")
                .AddPath(subscriptionId.ToString())
                .AddQuery(DefaultApiVersionParameterName, this.apiVersion)
                .Uri;

            requestId = requestId == default ? Guid.NewGuid() : requestId;
            correlationId = correlationId == default ? Guid.NewGuid() : correlationId;
            var updateContent = JsonConvert.SerializeObject(new { quantity = quantity });

            var response = await this.SendRequestAndReturnResult(
                               new HttpMethod("PATCH"),
                               requestUrl,
                               requestId,
                               correlationId,
                               null,
                               updateContent,
                               cancellationToken);

            return await FulfillmentRequestResult.ParseAsync<UpdateOrDeleteSubscriptionRequestResult>(response);
        }
    }
}
