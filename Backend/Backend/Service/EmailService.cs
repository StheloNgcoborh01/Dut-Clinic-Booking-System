using Backend.Service;
using MailKit.Net.Smtp;
using MimeKit;


namespace Backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _fromEmail;
        private readonly string _fromPassword;

        public EmailService(IConfiguration configuration)
        {
            
            _fromEmail = configuration["EmailSettings:FromEmail"] 
                ?? throw new InvalidOperationException("EmailSettings:FromEmail is missing in appsettings.json");
            
            _fromPassword = configuration["EmailSettings:FromPassword"] 
                ?? throw new InvalidOperationException("EmailSettings:FromPassword is missing in appsettings.json");
        }

        public async Task SendVerificationEmail(string toEmail, string code)
        {

            try
            {
              
              
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Dut Clinic Booking", _fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = "Verify Your Email - Dut Clinic";

                message.Body = new TextPart("html")
                {
                    Text = $@"
                        <h2>Dut Clinic!</h2>
                        <p>Your verification code is: <strong>{code}</strong></p>
                        <p>Enter this code to verify your email address.</p>
                    "
                };

                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_fromEmail, _fromPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                Console.WriteLine($" Email sent to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Failed to send email: {ex.Message}");
            }
        }
    }
}