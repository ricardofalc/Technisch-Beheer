// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Trackable.Services
{
    public interface INotificationService
    {
        Task<bool> NotifyViaEmail(string email, string subject, string textContent, string htmlContent);

        Task<bool> NotifyViaWebhook(string webhookUrl, GeofenceWebhookNotification notification);
    }
}
