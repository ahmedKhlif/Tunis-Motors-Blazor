namespace webappAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendOrderConfirmationAsync(string to, string customerName, int orderId, decimal totalAmount);
        Task SendOrderStatusUpdateAsync(string to, string customerName, int orderId, string status);
        Task SendCarSoldNotificationAsync(string to, string sellerName, string carName, decimal price);
        Task SendApprovalRequiredNotificationAsync(string to, string adminName, string carName, string sellerName);
        Task SendEmailConfirmationAsync(string to, string userName, string confirmationLink);
        Task SendPasswordResetAsync(string to, string userName, string resetLink);
        Task SendRentalRequestNotificationAsync(string to, string customerName, string carName, DateTime pickupDate, DateTime returnDate);
        Task SendRentalApprovedNotificationAsync(string to, string customerName, string carName, DateTime pickupDate, DateTime returnDate);
        Task SendRentalRejectedNotificationAsync(string to, string customerName, string carName, string reason);
        Task SendRentalReturnedNotificationAsync(string to, string customerName, string carName, decimal totalCost);
        Task SendRentalOverdueNotificationAsync(string to, string customerName, string carName, DateTime returnDueDate);
        Task SendListingApprovedAsync(string to, string sellerName, string carName, string approvalNote);
        Task SendListingRejectedAsync(string to, string sellerName, string carName, string rejectionReason);
    }

    public interface IPaymentService
    {
        Task<string> CreatePaymentIntent(decimal amount, string currency = "usd");
        Task<bool> ConfirmPayment(string paymentIntentId);
        Task<string> CreateCheckoutSession(decimal amount, string currency = "usd", string successUrl = "", string cancelUrl = "");
    }
}
