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
                message.From.Add(new MailboxAddress("Ntuzuma Clinic Booking", _fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = "Verify Your Email - Ntuzuma Clinic";

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


// for sending the Bokking cornfimation
        public async Task SendBookingConfirmation(string toEmail, string name, string reference, DateTime date, TimeSpan time, string appointmentType)
{
    try
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Clinic Booking", _fromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "Booking Confirmation - Ntuzuma Clinic";
var dateFormatted = date.ToString("yyyy-MM-dd");
var timeFormatted = $"{time.Hours:D2}:{time.Minutes:D2}";


        var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; }}
                    .container {{ padding: 20px; background-color: #f4f4f4; }}
                    .reference {{ font-size: 24px; font-weight: bold; color: #2c3e50; }}
                    .details {{ margin-top: 20px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>Booking Confirmed!</h2>
                    <p>Dear {name},</p>
                    <p>Your appointment has been confirmed.</p>
                    <p class='reference'>Reference: {reference}</p>
                    <div class='details'>
                        <p><strong>Date:</strong> {dateFormatted}</p>
                       <p><strong>Time:</strong> {timeFormatted}</p>
                        <p><strong>Type:</strong> {appointmentType}</p>
                    </div>
                    <p>Please bring your ID to the appointment.</p>
                    <hr>
                    <p>Thank you for choosing Ntuzuma Clinic.</p>
                </div>
            </body>
            </html>
        ";

        message.Body = new TextPart("html")
        {
            Text = body
        };

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_fromEmail, _fromPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        Console.WriteLine($" Booking confirmation email sent to {toEmail}");
    }
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}
}
    }
}