using System.Net;
using System.Net.Mail;

namespace webappAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");

                using var client = new SmtpClient(smtpSettings["SmtpServer"], int.Parse(smtpSettings["SmtpPort"]!))
                {
                    Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"]!)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["SenderEmail"]!, smtpSettings["SenderName"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"[EmailService] Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[EmailService] Email sending failed: {ex.Message}");
                // Log the error but don't throw - allow graceful degradation
            }
        }

        public async Task SendOrderConfirmationAsync(string to, string customerName, int orderId, decimal totalAmount)
        {
            var subject = $"Order Confirmation - Order #{orderId} - Tunisia Motors";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                                    <!-- Header with Logo -->
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #dc2626 0%, #991b1b 100%); padding: 30px; text-align: center;'>
                                            <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunisia Motors Logo' style='max-width: 180px; height: auto;' />
                                        </td>
                                    </tr>
                                    <!-- Content -->
                                    <tr>
                                        <td style='padding: 40px 30px;'>
                                            <h2 style='color: #10b981; margin: 0 0 20px 0; font-size: 28px;'>✓ Order Confirmed!</h2>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>Dear <strong>{customerName}</strong>,</p>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>Thank you for your order! Your order has been successfully placed and is being processed.</p>
                                            
                                            <!-- Order Details Box -->
                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #f9fafb; border-radius: 8px; border: 2px solid #e5e7eb; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 25px;'>
                                                        <h3 style='color: #dc2626; margin: 0 0 15px 0; font-size: 18px;'>📦 Order Details</h3>
                                                        <table width='100%' cellpadding='8' cellspacing='0'>
                                                            <tr>
                                                                <td style='color: #6b7280; font-size: 14px; padding: 8px 0;'><strong>Order ID:</strong></td>
                                                                <td style='color: #111827; font-size: 14px; text-align: right; padding: 8px 0;'>#{orderId}</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #6b7280; font-size: 14px; padding: 8px 0; border-top: 1px solid #e5e7eb;'><strong>Total Amount:</strong></td>
                                                                <td style='color: #dc2626; font-size: 18px; font-weight: bold; text-align: right; padding: 8px 0; border-top: 1px solid #e5e7eb;'>{totalAmount:N0} TND</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #6b7280; font-size: 14px; padding: 8px 0; border-top: 1px solid #e5e7eb;'><strong>Status:</strong></td>
                                                                <td style='text-align: right; padding: 8px 0; border-top: 1px solid #e5e7eb;'><span style='background: #fef3c7; color: #92400e; padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: 600;'>⏱ PENDING</span></td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #6b7280; font-size: 14px; padding: 8px 0; border-top: 1px solid #e5e7eb;'><strong>Order Date:</strong></td>
                                                                <td style='color: #111827; font-size: 14px; text-align: right; padding: 8px 0; border-top: 1px solid #e5e7eb;'>{DateTime.Now:dd/MM/yyyy HH:mm}</td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>

                                            <p style='color: #374151; font-size: 14px; line-height: 1.6; margin: 0 0 10px 0;'>✉️ You will receive updates on your order status via email.</p>
                                            <p style='color: #374151; font-size: 14px; line-height: 1.6; margin: 0 0 30px 0;'>💬 If you have any questions, please contact our support team.</p>
                                            
                                            <!-- CTA Button -->
                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td align='center' style='padding: 20px 0;'>
                                                        <a href='http://localhost:5271/profile/orders' style='display: inline-block; background: #dc2626; color: #ffffff; padding: 14px 40px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;'>View Order Details</a>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <!-- Footer -->
                                    <tr>
                                        <td style='background: #f9fafb; padding: 25px 30px; border-top: 1px solid #e5e7eb; text-align: center;'>
                                            <p style='color: #6b7280; font-size: 14px; margin: 0 0 10px 0;'>Best regards,<br><strong style='color: #dc2626;'>Tunisia Motors Team</strong></p>
                                            <p style='color: #9ca3af; font-size: 12px; margin: 0;'>© 2024 Tunisia Motors. All rights reserved.</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendOrderStatusUpdateAsync(string to, string customerName, int orderId, string status)
        {
            var subject = $"Order Status Update - Order #{orderId} - Tunisia Motors";
            var statusColor = status.ToLower() switch
            {
                "delivered" => "#10b981",
                "shipped" => "#3b82f6",
                "processing" => "#f59e0b",
                "cancelled" => "#ef4444",
                _ => "#6b7280"
            };
            var statusIcon = status.ToLower() switch
            {
                "delivered" => "✓",
                "shipped" => "🚚",
                "processing" => "⏱",
                "cancelled" => "✕",
                _ => "ℹ"
            };
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #dc2626 0%, #991b1b 100%); padding: 30px; text-align: center;'>
                                            <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunisia Motors Logo' style='max-width: 180px; height: auto;' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 40px 30px;'>
                                            <h2 style='color: {statusColor}; margin: 0 0 20px 0; font-size: 28px;'>{statusIcon} Order Status Update</h2>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>Dear <strong>{customerName}</strong>,</p>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>Your order status has been updated!</p>
                                            
                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #f9fafb; border-radius: 8px; border: 2px solid #e5e7eb; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 25px;'>
                                                        <h3 style='color: #dc2626; margin: 0 0 15px 0; font-size: 18px;'>📦 Order Information</h3>
                                                        <table width='100%' cellpadding='8' cellspacing='0'>
                                                            <tr>
                                                                <td style='color: #6b7280; font-size: 14px; padding: 8px 0;'><strong>Order ID:</strong></td>
                                                                <td style='color: #111827; font-size: 14px; text-align: right; padding: 8px 0;'>#{orderId}</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #6b7280; font-size: 14px; padding: 8px 0; border-top: 1px solid #e5e7eb;'><strong>New Status:</strong></td>
                                                                <td style='text-align: right; padding: 8px 0; border-top: 1px solid #e5e7eb;'><span style='background: {statusColor}; color: #ffffff; padding: 6px 16px; border-radius: 20px; font-size: 13px; font-weight: 600;'>{status.ToUpper()}</span></td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #6b7280; font-size: 14px; padding: 8px 0; border-top: 1px solid #e5e7eb;'><strong>Updated At:</strong></td>
                                                                <td style='color: #111827; font-size: 14px; text-align: right; padding: 8px 0; border-top: 1px solid #e5e7eb;'>{DateTime.Now:dd/MM/yyyy HH:mm}</td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>

                                            <p style='color: #374151; font-size: 14px; line-height: 1.6; margin: 0 0 30px 0;'>You can track your order status in your account dashboard.</p>
                                            
                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td align='center' style='padding: 20px 0;'>
                                                        <a href='http://localhost:5271/profile/orders' style='display: inline-block; background: #dc2626; color: #ffffff; padding: 14px 40px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;'>Track Order</a>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='background: #f9fafb; padding: 25px 30px; border-top: 1px solid #e5e7eb; text-align: center;'>
                                            <p style='color: #6b7280; font-size: 14px; margin: 0 0 10px 0;'>Best regards,<br><strong style='color: #dc2626;'>Tunisia Motors Team</strong></p>
                                            <p style='color: #9ca3af; font-size: 12px; margin: 0;'>© 2024 Tunisia Motors. All rights reserved.</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetAsync(string to, string userName, string resetLink)
        {
            var subject = "🔐 Password Reset Request - Tunisia Motors";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #dc2626 0%, #991b1b 100%); padding: 30px; text-align: center;'>
                                            <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunisia Motors Logo' style='max-width: 180px; height: auto;' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 40px 30px;'>
                                            <h2 style='color: #dc2626; margin: 0 0 20px 0; font-size: 28px;'>🔐 Password Reset Request</h2>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>Dear <strong>{userName}</strong>,</p>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>You have requested to reset your password for your Tunisia Motors account. Click the button below to proceed with resetting your password.</p>
                                            
                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td align='center' style='padding: 30px 0;'>
                                                        <a href='{resetLink}' style='display: inline-block; background: #dc2626; color: #ffffff; padding: 16px 50px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 4px 6px rgba(220, 38, 38, 0.3);'>Reset Password</a>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #fef2f2; border-radius: 8px; border-left: 4px solid #dc2626; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 20px;'>
                                                        <p style='color: #991b1b; font-size: 14px; font-weight: 600; margin: 0 0 10px 0;'>⚠️ Important Security Notice:</p>
                                                        <ul style='color: #7f1d1d; font-size: 14px; margin: 0; padding-left: 20px; line-height: 1.8;'>
                                                            <li>This link will expire in <strong>24 hours</strong> for security reasons</li>
                                                            <li>If you didn't request a password reset, please ignore this email</li>
                                                            <li>Never share this link with anyone</li>
                                                        </ul>
                                                    </td>
                                                </tr>
                                            </table>

                                            <div style='background: #f9fafb; padding: 20px; border-radius: 8px; margin: 0 0 20px 0;'>
                                                <p style='color: #6b7280; font-size: 13px; margin: 0 0 10px 0;'><strong>If the button doesn't work, copy and paste this link:</strong></p>
                                                <p style='color: #3b82f6; font-size: 12px; word-break: break-all; margin: 0;'>{resetLink}</p>
                                            </div>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #dbeafe; border-radius: 8px; margin: 0 0 20px 0;'>
                                                <tr>
                                                    <td style='padding: 15px;'>
                                                        <p style='color: #1e40af; font-size: 13px; margin: 0;'><strong>💡 Password Security Tips:</strong></p>
                                                        <p style='color: #1e3a8a; font-size: 13px; margin: 10px 0 0 0; line-height: 1.6;'>Choose a strong password with a mix of letters (uppercase & lowercase), numbers, and special characters.</p>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='background: #f9fafb; padding: 25px 30px; border-top: 1px solid #e5e7eb; text-align: center;'>
                                            <p style='color: #6b7280; font-size: 14px; margin: 0 0 10px 0;'>Best regards,<br><strong style='color: #dc2626;'>Tunisia Motors Team</strong></p>
                                            <p style='color: #9ca3af; font-size: 12px; margin: 0;'>© 2024 Tunisia Motors. All rights reserved.</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendCarSoldNotificationAsync(string to, string sellerName, string carName, decimal price)
        {
            var subject = $"🎉 Congratulations! Your Car Has Been Sold - {carName}";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #10b981 0%, #059669 100%); padding: 30px; text-align: center;'>
                                            <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunisia Motors Logo' style='max-width: 180px; height: auto; filter: brightness(0) invert(1);' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 40px 30px; text-align: center;'>
                                            <div style='font-size: 60px; margin-bottom: 20px;'>🎉</div>
                                            <h2 style='color: #10b981; margin: 0 0 20px 0; font-size: 32px;'>Congratulations!</h2>
                                            <p style='color: #374151; font-size: 18px; line-height: 1.6; margin: 0 0 10px 0;'>Dear <strong>{sellerName}</strong>,</p>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>Great news! Your car has been successfully sold on Tunisia Motors.</p>
                                            
                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%); border-radius: 8px; border: 2px solid #10b981; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 25px;'>
                                                        <h3 style='color: #047857; margin: 0 0 15px 0; font-size: 18px;'>🚗 Sale Details</h3>
                                                        <table width='100%' cellpadding='8' cellspacing='0'>
                                                            <tr>
                                                                <td style='color: #065f46; font-size: 14px; padding: 8px 0;'><strong>Vehicle:</strong></td>
                                                                <td style='color: #064e3b; font-size: 14px; text-align: right; padding: 8px 0; font-weight: 600;'>{carName}</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #065f46; font-size: 14px; padding: 8px 0; border-top: 1px solid #a7f3d0;'><strong>Sale Price:</strong></td>
                                                                <td style='color: #10b981; font-size: 24px; font-weight: bold; text-align: right; padding: 8px 0; border-top: 1px solid #a7f3d0;'>{price:N0} TND</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #065f46; font-size: 14px; padding: 8px 0; border-top: 1px solid #a7f3d0;'><strong>Sale Date:</strong></td>
                                                                <td style='color: #064e3b; font-size: 14px; text-align: right; padding: 8px 0; border-top: 1px solid #a7f3d0;'>{DateTime.Now:dd/MM/yyyy HH:mm}</td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #eff6ff; border-radius: 8px; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 20px; text-align: left;'>
                                                        <p style='color: #1e40af; font-size: 14px; font-weight: 600; margin: 0 0 10px 0;'>📋 Next Steps:</p>
                                                        <ul style='color: #1e3a8a; font-size: 14px; margin: 0; padding-left: 20px; line-height: 1.8;'>
                                                            <li>The buyer will contact you soon to arrange payment and delivery</li>
                                                            <li>Check your messages in the platform for buyer details</li>
                                                            <li>Prepare all necessary documents for the transfer</li>
                                                        </ul>
                                                    </td>
                                                </tr>
                                            </table>
                                            
                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td align='center' style='padding: 20px 0;'>
                                                        <a href='http://localhost:5271/profile/messages' style='display: inline-block; background: #dc2626; color: #ffffff; padding: 14px 40px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;'>Check Messages</a>
                                                    </td>
                                                </tr>
                                            </table>

                                            <p style='color: #6b7280; font-size: 13px; line-height: 1.6; margin: 20px 0 0 0;'>Thank you for choosing Tunisia Motors to sell your vehicle!</p>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='background: #f9fafb; padding: 25px 30px; border-top: 1px solid #e5e7eb; text-align: center;'>
                                            <p style='color: #6b7280; font-size: 14px; margin: 0 0 10px 0;'>Best regards,<br><strong style='color: #dc2626;'>Tunisia Motors Team</strong></p>
                                            <p style='color: #9ca3af; font-size: 12px; margin: 0;'>© 2024 Tunisia Motors. All rights reserved.</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendApprovalRequiredNotificationAsync(string to, string adminName, string carName, string sellerName)
        {
            var subject = $"⏱ New Car Listing Requires Approval - {carName}";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); padding: 30px; text-align: center;'>
                                            <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunisia Motors Logo' style='max-width: 180px; height: auto;' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 40px 30px;'>
                                            <h2 style='color: #f59e0b; margin: 0 0 20px 0; font-size: 28px;'>⏱ New Listing Requires Approval</h2>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>Dear <strong>{adminName}</strong>,</p>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>A new car listing has been submitted and requires your approval to be published on the platform.</p>
                                            
                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #fffbeb; border-radius: 8px; border: 2px solid #f59e0b; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 25px;'>
                                                        <h3 style='color: #d97706; margin: 0 0 15px 0; font-size: 18px;'>🚗 Listing Details</h3>
                                                        <table width='100%' cellpadding='8' cellspacing='0'>
                                                            <tr>
                                                                <td style='color: #92400e; font-size: 14px; padding: 8px 0;'><strong>Vehicle:</strong></td>
                                                                <td style='color: #78350f; font-size: 14px; text-align: right; padding: 8px 0; font-weight: 600;'>{carName}</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #92400e; font-size: 14px; padding: 8px 0; border-top: 1px solid #fde68a;'><strong>Seller:</strong></td>
                                                                <td style='color: #78350f; font-size: 14px; text-align: right; padding: 8px 0; border-top: 1px solid #fde68a;'>{sellerName}</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #92400e; font-size: 14px; padding: 8px 0; border-top: 1px solid #fde68a;'><strong>Submitted:</strong></td>
                                                                <td style='color: #78350f; font-size: 14px; text-align: right; padding: 8px 0; border-top: 1px solid #fde68a;'>{DateTime.Now:dd/MM/yyyy HH:mm}</td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #dbeafe; border-radius: 8px; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 20px;'>
                                                        <p style='color: #1e40af; font-size: 14px; font-weight: 600; margin: 0 0 10px 0;'>📋 Action Required:</p>
                                                        <p style='color: #1e3a8a; font-size: 14px; margin: 0; line-height: 1.8;'>Please review and approve or reject this listing in the admin dashboard. Ensure all details are accurate before approval.</p>
                                                    </td>
                                                </tr>
                                            </table>
                                            
                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td align='center' style='padding: 20px 0;'>
                                                        <a href='http://localhost:5271/admin/approvals' style='display: inline-block; background: #dc2626; color: #ffffff; padding: 14px 40px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;'>Review Listing</a>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='background: #f9fafb; padding: 25px 30px; border-top: 1px solid #e5e7eb; text-align: center;'>
                                            <p style='color: #6b7280; font-size: 14px; margin: 0 0 10px 0;'>Best regards,<br><strong style='color: #dc2626;'>Tunisia Motors System</strong></p>
                                            <p style='color: #9ca3af; font-size: 12px; margin: 0;'>© 2024 Tunisia Motors. All rights reserved.</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendEmailConfirmationAsync(string to, string userName, string confirmationLink)
        {
            var subject = "✉️ Confirm Your Email - Welcome to Tunisia Motors!";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #dc2626 0%, #991b1b 100%); padding: 30px; text-align: center;'>
                                            <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunisia Motors Logo' style='max-width: 180px; height: auto;' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 40px 30px; text-align: center;'>
                                            <div style='font-size: 60px; margin-bottom: 20px;'>👋</div>
                                            <h2 style='color: #10b981; margin: 0 0 20px 0; font-size: 32px;'>Welcome to Tunisia Motors!</h2>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 10px 0;'>Dear <strong>{userName}</strong>,</p>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>Thank you for joining Tunisia Motors! We're excited to have you on board. To complete your registration and activate your account, please confirm your email address.</p>
                                            
                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td align='center' style='padding: 30px 0;'>
                                                        <a href='{confirmationLink}' style='display: inline-block; background: #10b981; color: #ffffff; padding: 16px 50px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 4px 6px rgba(16, 185, 129, 0.3);'>Confirm Email Address</a>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #fef3c7; border-radius: 8px; border-left: 4px solid #f59e0b; margin: 0 0 20px 0;'>
                                                <tr>
                                                    <td style='padding: 20px;'>
                                                        <p style='color: #92400e; font-size: 14px; font-weight: 600; margin: 0 0 8px 0; text-align: left;'>⚠️ Important:</p>
                                                        <p style='color: #78350f; font-size: 13px; margin: 0; line-height: 1.6; text-align: left;'>This link will expire in <strong>24 hours</strong> for security reasons. If you didn't create an account with Tunisia Motors, please ignore this email.</p>
                                                    </td>
                                                </tr>
                                            </table>

                                            <div style='background: #f9fafb; padding: 20px; border-radius: 8px; margin: 0 0 30px 0;'>
                                                <p style='color: #6b7280; font-size: 13px; margin: 0 0 10px 0; text-align: left;'><strong>If the button doesn't work, copy and paste this link:</strong></p>
                                                <p style='color: #3b82f6; font-size: 12px; word-break: break-all; margin: 0; text-align: left;'>{confirmationLink}</p>
                                            </div>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%); border-radius: 8px;'>
                                                <tr>
                                                    <td style='padding: 20px; text-align: left;'>
                                                        <p style='color: #1e40af; font-size: 14px; font-weight: 600; margin: 0 0 10px 0;'>🚗 What's Next?</p>
                                                        <ul style='color: #1e3a8a; font-size: 14px; margin: 0; padding-left: 20px; line-height: 1.8;'>
                                                            <li>Browse thousands of quality vehicles</li>
                                                            <li>List your own car for sale</li>
                                                            <li>Connect with verified buyers and sellers</li>
                                                            <li>Enjoy secure transactions</li>
                                                        </ul>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='background: #f9fafb; padding: 25px 30px; border-top: 1px solid #e5e7eb; text-align: center;'>
                                            <p style='color: #6b7280; font-size: 14px; margin: 0 0 10px 0;'>Best regards,<br><strong style='color: #dc2626;'>Tunisia Motors Team</strong></p>
                                            <p style='color: #9ca3af; font-size: 12px; margin: 0;'>© 2024 Tunisia Motors. All rights reserved.</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendListingApprovedAsync(string to, string sellerName, string carName, string approvalNote)
        {
            var subject = $"✅ Your Listing Has Been Approved - {carName}";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #10b981 0%, #059669 100%); padding: 30px; text-align: center;'>
                                            <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunisia Motors Logo' style='max-width: 180px; height: auto; filter: brightness(0) invert(1);' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 40px 30px; text-align: center;'>
                                            <div style='font-size: 60px; margin-bottom: 20px;'>✅</div>
                                            <h2 style='color: #10b981; margin: 0 0 20px 0; font-size: 32px;'>Listing Approved!</h2>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 10px 0;'>Dear <strong>{sellerName}</strong>,</p>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>Great news! Your car listing has been approved and is now live on Tunisia Motors.</p>
                                            
                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%); border-radius: 8px; border: 2px solid #10b981; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 25px;'>
                                                        <h3 style='color: #047857; margin: 0 0 15px 0; font-size: 18px;'>🚗 Listing Details</h3>
                                                        <table width='100%' cellpadding='8' cellspacing='0'>
                                                            <tr>
                                                                <td style='color: #065f46; font-size: 14px; padding: 8px 0;'><strong>Vehicle:</strong></td>
                                                                <td style='color: #064e3b; font-size: 14px; text-align: right; padding: 8px 0; font-weight: 600;'>{carName}</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #065f46; font-size: 14px; padding: 8px 0; border-top: 1px solid #a7f3d0;'><strong>Status:</strong></td>
                                                                <td style='text-align: right; padding: 8px 0; border-top: 1px solid #a7f3d0;'><span style='background: #10b981; color: #ffffff; padding: 6px 16px; border-radius: 20px; font-size: 13px; font-weight: 600;'>LIVE</span></td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #065f46; font-size: 14px; padding: 8px 0; border-top: 1px solid #a7f3d0;'><strong>Approved:</strong></td>
                                                                <td style='color: #064e3b; font-size: 14px; text-align: right; padding: 8px 0; border-top: 1px solid #a7f3d0;'>{DateTime.Now:dd/MM/yyyy HH:mm}</td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>

                                            {(string.IsNullOrEmpty(approvalNote) ? "" : $@"
                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #dbeafe; border-radius: 8px; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 20px; text-align: left;'>
                                                        <p style='color: #1e40af; font-size: 14px; font-weight: 600; margin: 0 0 10px 0;'>💬 Admin Note:</p>
                                                        <p style='color: #1e3a8a; font-size: 14px; margin: 0; line-height: 1.6;'>{approvalNote}</p>
                                                    </td>
                                                </tr>
                                            </table>
                                            ")}

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #eff6ff; border-radius: 8px; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 20px; text-align: left;'>
                                                        <p style='color: #1e40af; font-size: 14px; font-weight: 600; margin: 0 0 10px 0;'>🎯 What's Next?</p>
                                                        <ul style='color: #1e3a8a; font-size: 14px; margin: 0; padding-left: 20px; line-height: 1.8;'>
                                                            <li>Your listing is now visible to all buyers</li>
                                                            <li>You'll receive notifications for any inquiries</li>
                                                            <li>Keep an eye on your messages for buyer contacts</li>
                                                        </ul>
                                                    </td>
                                                </tr>
                                            </table>
                                            
                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td align='center' style='padding: 20px 0;'>
                                                        <a href='http://localhost:5271/profile/listings' style='display: inline-block; background: #dc2626; color: #ffffff; padding: 14px 40px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;'>View My Listings</a>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='background: #f9fafb; padding: 25px 30px; border-top: 1px solid #e5e7eb; text-align: center;'>
                                            <p style='color: #6b7280; font-size: 14px; margin: 0 0 10px 0;'>Best regards,<br><strong style='color: #dc2626;'>Tunisia Motors Team</strong></p>
                                            <p style='color: #9ca3af; font-size: 12px; margin: 0;'>© 2024 Tunisia Motors. All rights reserved.</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendListingRejectedAsync(string to, string sellerName, string carName, string rejectionReason)
        {
            var subject = $"❌ Listing Not Approved - {carName}";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #dc2626 0%, #991b1b 100%); padding: 30px; text-align: center;'>
                                            <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunisia Motors Logo' style='max-width: 180px; height: auto;' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 40px 30px;'>
                                            <h2 style='color: #dc2626; margin: 0 0 20px 0; font-size: 28px;'>❌ Listing Not Approved</h2>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>Dear <strong>{sellerName}</strong>,</p>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>We regret to inform you that your car listing could not be approved at this time.</p>
                                            
                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #fef2f2; border-radius: 8px; border: 2px solid #dc2626; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 25px;'>
                                                        <h3 style='color: #991b1b; margin: 0 0 15px 0; font-size: 18px;'>🚗 Listing Details</h3>
                                                        <table width='100%' cellpadding='8' cellspacing='0'>
                                                            <tr>
                                                                <td style='color: #7f1d1d; font-size: 14px; padding: 8px 0;'><strong>Vehicle:</strong></td>
                                                                <td style='color: #7f1d1d; font-size: 14px; text-align: right; padding: 8px 0; font-weight: 600;'>{carName}</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #7f1d1d; font-size: 14px; padding: 8px 0; border-top: 1px solid #fecaca;'><strong>Status:</strong></td>
                                                                <td style='text-align: right; padding: 8px 0; border-top: 1px solid #fecaca;'><span style='background: #dc2626; color: #ffffff; padding: 6px 16px; border-radius: 20px; font-size: 13px; font-weight: 600;'>REJECTED</span></td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #7f1d1d; font-size: 14px; padding: 8px 0; border-top: 1px solid #fecaca;'><strong>Date:</strong></td>
                                                                <td style='color: #7f1d1d; font-size: 14px; text-align: right; padding: 8px 0; border-top: 1px solid #fecaca;'>{DateTime.Now:dd/MM/yyyy HH:mm}</td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #fef3c7; border-radius: 8px; border-left: 4px solid #f59e0b; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 20px; text-align: left;'>
                                                        <p style='color: #92400e; font-size: 14px; font-weight: 600; margin: 0 0 10px 0;'>📋 Reason for Rejection:</p>
                                                        <p style='color: #78350f; font-size: 14px; margin: 0; line-height: 1.6;'>{(string.IsNullOrEmpty(rejectionReason) ? "Your listing did not meet our quality standards or guidelines." : rejectionReason)}</p>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #dbeafe; border-radius: 8px; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 20px; text-align: left;'>
                                                        <p style='color: #1e40af; font-size: 14px; font-weight: 600; margin: 0 0 10px 0;'>💡 What You Can Do:</p>
                                                        <ul style='color: #1e3a8a; font-size: 14px; margin: 0; padding-left: 20px; line-height: 1.8;'>
                                                            <li>Review our listing guidelines</li>
                                                            <li>Update your listing with correct information</li>
                                                            <li>Add clear, high-quality photos</li>
                                                            <li>Provide complete and accurate vehicle details</li>
                                                            <li>Contact support if you need assistance</li>
                                                        </ul>
                                                    </td>
                                                </tr>
                                            </table>
                                            
                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td align='center' style='padding: 20px 0;'>
                                                        <a href='http://localhost:5271/product/create' style='display: inline-block; background: #dc2626; color: #ffffff; padding: 14px 40px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;'>Create New Listing</a>
                                                    </td>
                                                </tr>
                                            </table>

                                            <p style='color: #6b7280; font-size: 13px; line-height: 1.6; margin: 20px 0 0 0; text-align: center;'>If you believe this was a mistake or have questions, please contact our support team.</p>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='background: #f9fafb; padding: 25px 30px; border-top: 1px solid #e5e7eb; text-align: center;'>
                                            <p style='color: #6b7280; font-size: 14px; margin: 0 0 10px 0;'>Best regards,<br><strong style='color: #dc2626;'>Tunisia Motors Team</strong></p>
                                            <p style='color: #9ca3af; font-size: 12px; margin: 0;'>© 2024 Tunisia Motors. All rights reserved.</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendRentalRequestNotificationAsync(string to, string customerName, string carName, DateTime pickupDate, DateTime returnDate)
        {
            var subject = $"🚗 New Rental Request - {carName}";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%); padding: 30px; text-align: center;'>
                                            <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunisia Motors Logo' style='max-width: 180px; height: auto; filter: brightness(0) invert(1);' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 40px 30px;'>
                                            <h2 style='color: #3b82f6; margin: 0 0 20px 0; font-size: 28px;'>🚗 New Rental Request</h2>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>Dear Admin,</p>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>You have received a new rental request that requires your approval.</p>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #eff6ff; border-radius: 8px; border: 2px solid #3b82f6; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 25px;'>
                                                        <h3 style='color: #1e40af; margin: 0 0 15px 0; font-size: 18px;'>📋 Rental Details</h3>
                                                        <table width='100%' cellpadding='8' cellspacing='0'>
                                                            <tr>
                                                                <td style='color: #1e3a8a; font-size: 14px; padding: 8px 0;'><strong>Customer:</strong></td>
                                                                <td style='color: #1e3a8a; font-size: 14px; text-align: right; padding: 8px 0; font-weight: 600;'>{customerName}</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #1e3a8a; font-size: 14px; padding: 8px 0; border-top: 1px solid #bfdbfe;'><strong>Vehicle:</strong></td>
                                                                <td style='color: #1e3a8a; font-size: 14px; text-align: right; padding: 8px 0; border-top: 1px solid #bfdbfe; font-weight: 600;'>{carName}</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #1e3a8a; font-size: 14px; padding: 8px 0; border-top: 1px solid #bfdbfe;'><strong>Pickup Date:</strong></td>
                                                                <td style='color: #1e3a8a; font-size: 14px; text-align: right; padding: 8px 0; border-top: 1px solid #bfdbfe;'>{pickupDate.ToString("MMM dd, yyyy")}</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #1e3a8a; font-size: 14px; padding: 8px 0; border-top: 1px solid #bfdbfe;'><strong>Return Date:</strong></td>
                                                                <td style='color: #1e3a8a; font-size: 14px; text-align: right; padding: 8px 0; border-top: 1px solid #bfdbfe;'>{returnDate.ToString("MMM dd, yyyy")}</td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td align='center' style='padding: 20px 0;'>
                                                        <a href='http://localhost:5271/admin/rentals' style='display: inline-block; background: #dc2626; color: #ffffff; padding: 14px 40px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;'>Review Rental Request</a>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='background: #f9fafb; padding: 25px 30px; border-top: 1px solid #e5e7eb; text-align: center;'>
                                            <p style='color: #6b7280; font-size: 14px; margin: 0 0 10px 0;'>Best regards,<br><strong style='color: #dc2626;'>Tunisia Motors System</strong></p>
                                            <p style='color: #9ca3af; font-size: 12px; margin: 0;'>© 2024 Tunisia Motors. All rights reserved.</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendRentalApprovedNotificationAsync(string to, string customerName, string carName, DateTime pickupDate, DateTime returnDate)
        {
            var subject = $"✅ Rental Request Approved - {carName}";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #10b981 0%, #059669 100%); padding: 30px; text-align: center;'>
                                            <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunisia Motors Logo' style='max-width: 180px; height: auto; filter: brightness(0) invert(1);' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 40px 30px; text-align: center;'>
                                            <div style='font-size: 60px; margin-bottom: 20px;'>✅</div>
                                            <h2 style='color: #10b981; margin: 0 0 20px 0; font-size: 32px;'>Rental Approved!</h2>
                                            <p style='color: #374151; font-size: 18px; line-height: 1.6; margin: 0 0 10px 0;'>Hello <strong>{customerName}</strong>,</p>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>Great news! Your rental request has been approved. You can now proceed with your booking.</p>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%); border-radius: 8px; border: 2px solid #10b981; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 25px;'>
                                                        <h3 style='color: #047857; margin: 0 0 15px 0; font-size: 18px;'>🚗 Approved Rental Details</h3>
                                                        <table width='100%' cellpadding='8' cellspacing='0'>
                                                            <tr>
                                                                <td style='color: #065f46; font-size: 14px; padding: 8px 0;'><strong>Vehicle:</strong></td>
                                                                <td style='color: #064e3b; font-size: 14px; text-align: right; padding: 8px 0; font-weight: 600;'>{carName}</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #065f46; font-size: 14px; padding: 8px 0; border-top: 1px solid #a7f3d0;'><strong>Pickup Date:</strong></td>
                                                                <td style='color: #064e3b; font-size: 14px; text-align: right; padding: 8px 0; border-top: 1px solid #a7f3d0;'>{pickupDate.ToString("MMM dd, yyyy")}</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #065f46; font-size: 14px; padding: 8px 0; border-top: 1px solid #a7f3d0;'><strong>Return Date:</strong></td>
                                                                <td style='color: #064e3b; font-size: 14px; text-align: right; padding: 8px 0; border-top: 1px solid #a7f3d0;'>{returnDate.ToString("MMM dd, yyyy")}</td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #eff6ff; border-radius: 8px; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 20px; text-align: left;'>
                                                        <p style='color: #1e40af; font-size: 14px; font-weight: 600; margin: 0 0 10px 0;'>📋 Next Steps:</p>
                                                        <ul style='color: #1e3a8a; font-size: 14px; margin: 0; padding-left: 20px; line-height: 1.8;'>
                                                            <li>Arrive at the pickup location on time</li>
                                                            <li>Bring your ID and driving license</li>
                                                            <li>Make sure you have payment ready</li>
                                                            <li>Contact the seller if you have questions</li>
                                                        </ul>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td align='center' style='padding: 20px 0;'>
                                                        <a href='http://localhost:5271/my-rentals' style='display: inline-block; background: #10b981; color: #ffffff; padding: 14px 40px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;'>View My Rentals</a>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='background: #f9fafb; padding: 25px 30px; border-top: 1px solid #e5e7eb; text-align: center;'>
                                            <p style='color: #6b7280; font-size: 14px; margin: 0 0 10px 0;'>Best regards,<br><strong style='color: #dc2626;'>Tunisia Motors Team</strong></p>
                                            <p style='color: #9ca3af; font-size: 12px; margin: 0;'>© 2024 Tunisia Motors. All rights reserved.</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendRentalRejectedNotificationAsync(string to, string customerName, string carName, string reason)
        {
            var subject = $"❌ Rental Request Update - {carName}";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #dc2626 0%, #991b1b 100%); padding: 30px; text-align: center;'>
                                            <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunisia Motors Logo' style='max-width: 180px; height: auto;' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 40px 30px;'>
                                            <h2 style='color: #dc2626; margin: 0 0 20px 0; font-size: 28px;'>❌ Rental Request Update</h2>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>Hello <strong>{customerName}</strong>,</p>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>We regret to inform you that your rental request for <strong>{carName}</strong> could not be approved at this time.</p>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #fef2f2; border-radius: 8px; border: 2px solid #dc2626; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 25px;'>
                                                        <h3 style='color: #991b1b; margin: 0 0 15px 0; font-size: 18px;'>📋 Reason</h3>
                                                        <p style='color: #7f1d1d; font-size: 14px; margin: 0; line-height: 1.6;'>{reason}</p>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #dbeafe; border-radius: 8px; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 20px; text-align: left;'>
                                                        <p style='color: #1e40af; font-size: 14px; font-weight: 600; margin: 0 0 10px 0;'>💡 What You Can Do:</p>
                                                        <ul style='color: #1e3a8a; font-size: 14px; margin: 0; padding-left: 20px; line-height: 1.8;'>
                                                            <li>Browse other available rental cars</li>
                                                            <li>Try different dates or vehicles</li>
                                                            <li>Contact us for assistance</li>
                                                        </ul>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td align='center' style='padding: 20px 0;'>
                                                        <a href='http://localhost:5271/rentals' style='display: inline-block; background: #007bff; color: #ffffff; padding: 14px 40px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;'>Browse Other Cars</a>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='background: #f9fafb; padding: 25px 30px; border-top: 1px solid #e5e7eb; text-align: center;'>
                                            <p style='color: #6b7280; font-size: 14px; margin: 0 0 10px 0;'>Best regards,<br><strong style='color: #dc2626;'>Tunisia Motors Team</strong></p>
                                            <p style='color: #9ca3af; font-size: 12px; margin: 0;'>© 2024 Tunisia Motors. All rights reserved.</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendRentalReturnedNotificationAsync(string to, string customerName, string carName, decimal totalCost)
        {
            var subject = $"✅ Rental Completed Successfully - {carName}";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #10b981 0%, #059669 100%); padding: 30px; text-align: center;'>
                                            <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunisia Motors Logo' style='max-width: 180px; height: auto; filter: brightness(0) invert(1);' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 40px 30px; text-align: center;'>
                                            <div style='font-size: 60px; margin-bottom: 20px;'>✅</div>
                                            <h2 style='color: #10b981; margin: 0 0 20px 0; font-size: 32px;'>Rental Completed!</h2>
                                            <p style='color: #374151; font-size: 18px; line-height: 1.6; margin: 0 0 10px 0;'>Thank you <strong>{customerName}</strong>,</p>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>Your rental has been successfully completed. We hope you had a great experience!</p>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%); border-radius: 8px; border: 2px solid #10b981; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 25px;'>
                                                        <h3 style='color: #047857; margin: 0 0 15px 0; font-size: 18px;'>📊 Rental Summary</h3>
                                                        <table width='100%' cellpadding='8' cellspacing='0'>
                                                            <tr>
                                                                <td style='color: #065f46; font-size: 14px; padding: 8px 0;'><strong>Vehicle:</strong></td>
                                                                <td style='color: #064e3b; font-size: 14px; text-align: right; padding: 8px 0; font-weight: 600;'>{carName}</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #065f46; font-size: 14px; padding: 8px 0; border-top: 1px solid #a7f3d0;'><strong>Total Cost:</strong></td>
                                                                <td style='color: #10b981; font-size: 18px; font-weight: bold; text-align: right; padding: 8px 0; border-top: 1px solid #a7f3d0;'>{totalCost:N0} TND</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #065f46; font-size: 14px; padding: 8px 0; border-top: 1px solid #a7f3d0;'><strong>Status:</strong></td>
                                                                <td style='text-align: right; padding: 8px 0; border-top: 1px solid #a7f3d0;'><span style='background: #10b981; color: #ffffff; padding: 6px 16px; border-radius: 20px; font-size: 13px; font-weight: 600;'>COMPLETED</span></td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #eff6ff; border-radius: 8px; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 20px; text-align: left;'>
                                                        <p style='color: #1e40af; font-size: 14px; font-weight: 600; margin: 0 0 10px 0;'>⭐ We Value Your Feedback:</p>
                                                        <p style='color: #1e3a8a; font-size: 14px; margin: 0; line-height: 1.6;'>Your review helps other customers make informed decisions. Please consider leaving a rating for the vehicle and our service.</p>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td align='center' style='padding: 20px 0;'>
                                                        <a href='http://localhost:5271/my-rentals' style='display: inline-block; background: #10b981; color: #ffffff; padding: 14px 40px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;'>View Rental History</a>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='background: #f9fafb; padding: 25px 30px; border-top: 1px solid #e5e7eb; text-align: center;'>
                                            <p style='color: #6b7280; font-size: 14px; margin: 0 0 10px 0;'>Thank you for choosing Tunisia Motors,<br><strong style='color: #dc2626;'>Tunisia Motors Team</strong></p>
                                            <p style='color: #9ca3af; font-size: 12px; margin: 0;'>© 2024 Tunisia Motors. All rights reserved.</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendRentalOverdueNotificationAsync(string to, string customerName, string carName, DateTime returnDueDate)
        {
            var subject = $"⚠️ Rental Overdue Notice - {carName}";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #dc2626 0%, #991b1b 100%); padding: 30px; text-align: center;'>
                                            <img src='https://i.postimg.cc/HkDwTh4X/logo.png' alt='Tunisia Motors Logo' style='max-width: 180px; height: auto;' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 40px 30px;'>
                                            <h2 style='color: #dc2626; margin: 0 0 20px 0; font-size: 28px;'>⚠️ Rental Overdue Notice</h2>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>Hello <strong>{customerName}</strong>,</p>
                                            <p style='color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>This is an important notice that your rental for <strong>{carName}</strong> is currently overdue.</p>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #fff3cd; border-radius: 8px; border: 2px solid #f59e0b; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 25px;'>
                                                        <h3 style='color: #92400e; margin: 0 0 15px 0; font-size: 18px;'>⏰ Overdue Details</h3>
                                                        <table width='100%' cellpadding='8' cellspacing='0'>
                                                            <tr>
                                                                <td style='color: #78350f; font-size: 14px; padding: 8px 0;'><strong>Vehicle:</strong></td>
                                                                <td style='color: #78350f; font-size: 14px; text-align: right; padding: 8px 0; font-weight: 600;'>{carName}</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #78350f; font-size: 14px; padding: 8px 0; border-top: 1px solid #fde68a;'><strong>Due Date:</strong></td>
                                                                <td style='color: #dc2626; font-size: 14px; text-align: right; padding: 8px 0; border-top: 1px solid #fde68a; font-weight: 600;'>{returnDueDate.ToString("MMM dd, yyyy")}</td>
                                                            </tr>
                                                            <tr>
                                                                <td style='color: #78350f; font-size: 14px; padding: 8px 0; border-top: 1px solid #fde68a;'><strong>Status:</strong></td>
                                                                <td style='text-align: right; padding: 8px 0; border-top: 1px solid #fde68a;'><span style='background: #dc2626; color: #ffffff; padding: 6px 16px; border-radius: 20px; font-size: 13px; font-weight: 600;'>OVERDUE</span></td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #fef2f2; border-radius: 8px; border-left: 4px solid #dc2626; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 20px; text-align: left;'>
                                                        <p style='color: #991b1b; font-size: 14px; font-weight: 600; margin: 0 0 10px 0;'>🚨 Action Required:</p>
                                                        <p style='color: #7f1d1d; font-size: 14px; margin: 0; line-height: 1.6;'>Please return the vehicle immediately to avoid additional charges and penalties. Contact us right away if there are any issues.</p>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0' style='background: #f9fafb; border-radius: 8px; margin: 0 0 30px 0;'>
                                                <tr>
                                                    <td style='padding: 20px; text-align: left;'>
                                                        <p style='color: #374151; font-size: 14px; font-weight: 600; margin: 0 0 10px 0;'>📞 Emergency Contact:</p>
                                                        <p style='color: #6b7280; font-size: 14px; margin: 0; line-height: 1.6;'>Phone: +216 XX XXX XXX<br>Email: support@tunisia-motors.com</p>
                                                    </td>
                                                </tr>
                                            </table>

                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td align='center' style='padding: 20px 0;'>
                                                        <a href='tel:+216XXXXXXXX' style='display: inline-block; background: #dc2626; color: #ffffff; padding: 14px 40px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; margin-right: 10px;'>Call Now</a>
                                                        <a href='mailto:support@tunisia-motors.com' style='display: inline-block; background: #007bff; color: #ffffff; padding: 14px 40px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;'>Email Support</a>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='background: #f9fafb; padding: 25px 30px; border-top: 1px solid #e5e7eb; text-align: center;'>
                                            <p style='color: #6b7280; font-size: 14px; margin: 0 0 10px 0;'>Best regards,<br><strong style='color: #dc2626;'>Tunisia Motors Team</strong></p>
                                            <p style='color: #9ca3af; font-size: 12px; margin: 0;'>© 2024 Tunisia Motors. All rights reserved.</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }
    }
}
