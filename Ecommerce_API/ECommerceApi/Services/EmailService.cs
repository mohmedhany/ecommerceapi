using System.Net.Mail;
using System.Net;
using ECommerceApi.Entities;

namespace ECommerceApi.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(User user);
    Task SendOrderConfirmationAsync(Order order, User user);
    Task SendOrderStatusUpdateAsync(Order order, User user);
    Task SendPasswordResetAsync(User user, string resetToken);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    
    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task SendWelcomeEmailAsync(User user)
    {
        var subject = "Welcome to E-Commerce!";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Welcome, {user.FullName}!</h2>
                <p>Thank you for registering with us.</p>
                <p>You can now browse products, add items to cart, and make purchases.</p>
                <p>Happy shopping!</p>
            </body>
            </html>
        ";
        
        await SendEmailAsync(user.Email, subject, body);
    }
    
    public async Task SendOrderConfirmationAsync(Order order, User user)
    {
        var subject = $"Order Confirmation - #{order.Id}";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Order Confirmed!</h2>
                <p>Thank you for your order, {user.FullName}!</p>
                <p><strong>Order Number:</strong> #{order.Id}</p>
                <p><strong>Total Amount:</strong> {order.TotalAmount} EGP</p>
                <p><strong>Shipping Address:</strong> {order.ShippingAddress}</p>
            </body>
            </html>
        ";
        
        await SendEmailAsync(user.Email, subject, body);
    }
    
    public async Task SendOrderStatusUpdateAsync(Order order, User user)
    {
        var subject = $"Order #{order.Id} - Status Update";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Order Status Update</h2>
                <p>Hi {user.FullName},</p>
                <p>Your order #{order.Id} status is: {order.Status}</p>
            </body>
            </html>
        ";
        
        await SendEmailAsync(user.Email, subject, body);
    }
    
    public async Task SendPasswordResetAsync(User user, string resetToken)
    {
        var appUrl = _configuration["AppUrl"] ?? "http://localhost:5000";
        var resetLink = $"{appUrl}/reset-password?token={resetToken}";
        
        var subject = "Reset Your Password";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Reset Password</h2>
                <p>Hi {user.FullName},</p>
                <p>Click to reset your password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
            </body>
            </html>
        ";
        
        await SendEmailAsync(user.Email, subject, body);
    }
    
    private async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = _configuration["Email:SmtpPort"];
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPass = _configuration["Email:SmtpPass"];
            var fromEmail = _configuration["Email:FromEmail"];
            
            if (string.IsNullOrEmpty(smtpHost))
            {
                Console.WriteLine($"[EMAIL] To: {to}, Subject: {subject}");
                return;
            }
            
            using var client = new SmtpClient(smtpHost, int.Parse(smtpPort ?? "587"));
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(smtpUser, smtpPass);
            
            var message = new MailMessage
            {
                From = new MailAddress(fromEmail ?? "noreply@ecommerce.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);
            
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL ERROR] {ex.Message}");
        }
    }
}