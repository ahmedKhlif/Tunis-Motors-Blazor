using Stripe;

namespace webappAPI.Services
{
    public class StripePaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            var secretKey = configuration["Stripe:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogError("Stripe SecretKey is not configured in appsettings.json");
                throw new InvalidOperationException("Stripe SecretKey is not configured");
            }
            StripeConfiguration.ApiKey = secretKey;
            _logger.LogInformation("Stripe API key configured successfully");
        }

        public async Task<string> CreatePaymentIntent(decimal amount, string currency = "usd")
        {
            try
            {
                if (amount <= 0)
                {
                    throw new ArgumentException("Amount must be greater than zero", nameof(amount));
                }

                if (amount > 999999.99m)
                {
                    throw new ArgumentException("Amount exceeds maximum allowed value", nameof(amount));
                }

                // Note: Stripe doesn't support TND, so we use USD
                // In production, you should convert TND to USD using current exchange rate
                // For now, we'll use USD and note that amount is in TND
                var amountInCents = (long)(amount * 100);
                
                _logger.LogInformation("Creating payment intent: Amount={Amount} ({AmountCents} cents), Currency={Currency}", 
                    amount, amountInCents, currency);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = currency.ToLower(),
                    PaymentMethodTypes = new List<string> { "card" },
                    Metadata = new Dictionary<string, string>
                    {
                        { "integration_check", "accept_a_payment" },
                        { "original_currency", "tnd" },
                        { "original_amount", amount.ToString("F2") }
                    }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);
                
                _logger.LogInformation("Payment intent created successfully: {PaymentIntentId}", paymentIntent.Id);
                return paymentIntent.ClientSecret;
            }
            catch (StripeException stripeEx)
            {
                _logger.LogError(stripeEx, "Stripe API error creating payment intent: {StripeError}", stripeEx.Message);
                throw new Exception($"Stripe error: {stripeEx.Message}", stripeEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent: {Error}", ex.Message);
                throw;
            }
        }

        public async Task<bool> ConfirmPayment(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);
                return paymentIntent.Status == "succeeded";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error confirming payment: {ex.Message}");
                return false;
            }
        }

        public async Task<string> CreateCheckoutSession(decimal amount, string currency = "usd", string successUrl = "", string cancelUrl = "")
        {
            // TODO: Implement Stripe checkout session creation when Stripe package is properly configured
            _logger.LogWarning("Stripe checkout session creation is not implemented yet");
            throw new NotImplementedException("Stripe checkout session creation is not implemented yet");
        }
    }
}
