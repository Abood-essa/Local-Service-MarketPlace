using System.Net;
using System.Net.Mail;

namespace Local_Service_marketPlace.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        private string Host => _config["EmailSettings:Host"]!;
        private int Port => _config.GetValue<int>("EmailSettings:Port");
        private string Username => _config["EmailSettings:Username"]!;
        private string Password => _config["EmailSettings:Password"]!;
        private string FromName => _config["EmailSettings:FromName"]!;
        private string FromEmail => _config["EmailSettings:FromEmail"]!;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // ── Core send method ──────────────────────────────────────────────────
        private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            try
            {
                using var client = new SmtpClient(Host, Port)
                {
                    Credentials = new NetworkCredential(Username, Password),
                    EnableSsl = true,
                    Timeout = 20000
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(FromEmail, FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mail.To.Add(new MailAddress(toEmail, toName));
                await client.SendMailAsync(mail);

                _logger.LogInformation("Email sent to {Email} | Subject: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email} | Subject: {Subject}", toEmail, subject);
            }
        }

        // ── HTML wrapper ─────────────────────────────────────────────────────
        private static string Wrap(string accentColor, string iconEmoji, string preheader, string bodyContent)
        {
            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
  <title>{preheader}</title>
</head>
<body style=""margin:0;padding:0;background:#f1f5f9;font-family:'Segoe UI',Arial,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f1f5f9;padding:32px 0;"">
    <tr><td align=""center"">
      <table width=""600"" cellpadding=""0"" cellspacing=""0""
             style=""background:#ffffff;border-radius:12px;overflow:hidden;
                    box-shadow:0 2px 8px rgba(0,0,0,.08);max-width:600px;width:100%;"">

        <!-- Header -->
        <tr>
          <td style=""background:{accentColor};padding:28px 40px;text-align:center;"">
            <p style=""margin:0;font-size:28px;"">{iconEmoji}</p>
            <h1 style=""margin:8px 0 0;color:#ffffff;font-size:22px;font-weight:700;
                        letter-spacing:-0.5px;"">Khidmati</h1>
          </td>
        </tr>

        <!-- Body -->
        <tr>
          <td style=""padding:36px 40px 28px;"">
            {bodyContent}
          </td>
        </tr>

        <!-- Footer -->
        <tr>
          <td style=""background:#f8fafc;padding:20px 40px;text-align:center;
                      border-top:1px solid #e2e8f0;"">
            <p style=""margin:0;font-size:12px;color:#94a3b8;"">
              This email was sent by Khidmati. Please do not reply to this email.
            </p>
          </td>
        </tr>

      </table>
    </td></tr>
  </table>
</body>
</html>";
        }

        // ── Reusable snippets ─────────────────────────────────────────────────
        private static string InfoRow(string label, string value) => $@"
<tr>
  <td style=""padding:6px 0;color:#64748b;font-size:14px;width:140px;"">{label}</td>
  <td style=""padding:6px 0;color:#1e293b;font-size:14px;font-weight:600;"">{value}</td>
</tr>";

        private static string Button(string url, string label, string color) => $@"
<div style=""text-align:center;margin-top:28px;"">
  <a href=""{url}"" style=""background:{color};color:#ffffff;text-decoration:none;
     padding:12px 32px;border-radius:8px;font-size:15px;font-weight:600;
     display:inline-block;"">{label}</a>
</div>";

        private static string Greeting(string name) =>
            $@"<p style=""margin:0 0 6px;font-size:16px;color:#64748b;"">Hi <strong style=""color:#1e293b;"">{name}</strong>,</p>";

        private static string InfoTable(string rows) =>
            $@"<table cellpadding=""0"" cellspacing=""0"" style=""width:100%;margin-top:20px;
               background:#f8fafc;border-radius:8px;padding:16px;border:1px solid #e2e8f0;"">
               {rows}
               </table>";

        // ─────────────────────────────────────────────────────────────────────
        // 1. Welcome Email
        // ─────────────────────────────────────────────────────────────────────
        public async Task SendWelcomeEmailAsync(string toEmail, string fullName, string role)
        {
            var roleNote = role == "Provider"
                ? "You can now set up your profile and start receiving service requests."
                : "You can now post service requests and find trusted providers near you.";

            var body = $@"
{Greeting(fullName)}
<h2 style=""margin:16px 0 8px;font-size:20px;color:#1e293b;"">Welcome to Khidmati! 🎉</h2>
<p style=""margin:0 0 16px;font-size:15px;color:#475569;line-height:1.6;"">
  Your account has been created successfully as a <strong>{role}</strong>.
  {roleNote}
</p>
{InfoTable(InfoRow("Account", toEmail) + InfoRow("Role", role))}
<p style=""margin:24px 0 0;font-size:14px;color:#94a3b8;"">
  Welcome aboard — we're glad to have you!
</p>";

            var html = Wrap("#2563eb", "🏠", "Welcome to Khidmati", body);
            await SendAsync(toEmail, fullName, "Welcome to Khidmati!", html);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 2. Booking Created
        // ─────────────────────────────────────────────────────────────────────
        public async Task SendBookingCreatedEmailAsync(
            string toEmail, string fullName, string serviceTitle,
            int bookingId, decimal price)
        {
            var body = $@"
{Greeting(fullName)}
<h2 style=""margin:16px 0 8px;font-size:20px;color:#1e293b;"">Your Booking is Confirmed ✅</h2>
<p style=""margin:0 0 16px;font-size:15px;color:#475569;line-height:1.6;"">
  Great news! Your booking has been created and the provider will be in touch soon.
</p>
{InfoTable(
    InfoRow("Booking #", bookingId.ToString()) +
    InfoRow("Service", serviceTitle) +
    InfoRow("Final Price", price + " JD")
)}
<p style=""margin:24px 0 0;font-size:14px;color:#94a3b8;"">
  You can track the status of your booking at any time from the My Bookings page.
</p>";

            var html = Wrap("#16a34a", "📅", "Booking Confirmed", body);
            await SendAsync(toEmail, fullName, $"Booking Confirmed — {serviceTitle}", html);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 3. Booking Status Changed
        // ─────────────────────────────────────────────────────────────────────
        public async Task SendBookingStatusChangedEmailAsync(
            string toEmail, string fullName, string serviceTitle,
            int bookingId, string newStatus, string message)
        {
            var (accent, emoji) = newStatus switch
            {
                "InProgress" => ("#d97706", "⚙️"),
                "PendingCompletion" => ("#0891b2", "⏳"),
                "Completed" => ("#16a34a", "✅"),
                _ => ("#64748b", "📋")
            };

            var body = $@"
{Greeting(fullName)}
<h2 style=""margin:16px 0 8px;font-size:20px;color:#1e293b;"">Booking Status Updated {emoji}</h2>
<p style=""margin:0 0 16px;font-size:15px;color:#475569;line-height:1.6;"">
  {message}
</p>
{InfoTable(
    InfoRow("Booking #", bookingId.ToString()) +
    InfoRow("Service", serviceTitle) +
    InfoRow("New Status", newStatus)
)}
<p style=""margin:24px 0 0;font-size:14px;color:#94a3b8;"">
  Log in to view full details and take any required action.
</p>";

            var html = Wrap(accent, emoji, $"Booking Update — {newStatus}", body);
            await SendAsync(toEmail, fullName, $"Booking Update: {newStatus} — {serviceTitle}", html);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 4. New Offer Received (→ Customer)
        // ─────────────────────────────────────────────────────────────────────
        public async Task SendNewOfferEmailAsync(
            string toEmail, string fullName, string serviceTitle,
            int requestId, string providerName, decimal offeredPrice)
        {
            var body = $@"
{Greeting(fullName)}
<h2 style=""margin:16px 0 8px;font-size:20px;color:#1e293b;"">You Have a New Offer 📨</h2>
<p style=""margin:0 0 16px;font-size:15px;color:#475569;line-height:1.6;"">
  A provider has submitted an offer on your service request. Review it and accept or reject.
</p>
{InfoTable(
    InfoRow("Service", serviceTitle) +
    InfoRow("Provider", providerName) +
    InfoRow("Offered Price", offeredPrice + " JD")
)}
<p style=""margin:24px 0 0;font-size:14px;color:#94a3b8;"">
  Log in to My Requests to review the offer details.
</p>";

            var html = Wrap("#2563eb", "📨", "New Offer Received", body);
            await SendAsync(toEmail, fullName, $"New Offer on '{serviceTitle}'", html);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 5. Offer Accepted (→ Provider)
        // ─────────────────────────────────────────────────────────────────────
        public async Task SendOfferAcceptedEmailAsync(
            string toEmail, string fullName, string serviceTitle,
            int bookingId, decimal finalPrice)
        {
            var body = $@"
{Greeting(fullName)}
<h2 style=""margin:16px 0 8px;font-size:20px;color:#1e293b;"">Your Offer Was Accepted! 🎉</h2>
<p style=""margin:0 0 16px;font-size:15px;color:#475569;line-height:1.6;"">
  The customer has accepted your offer. A booking has been created — head to your
  Bookings page to get started.
</p>
{InfoTable(
    InfoRow("Booking #", bookingId.ToString()) +
    InfoRow("Service", serviceTitle) +
    InfoRow("Final Price", finalPrice + " JD")
)}
<p style=""margin:24px 0 0;font-size:14px;color:#94a3b8;"">
  Log in to your Provider Dashboard to view the full booking details.
</p>";

            var html = Wrap("#0f766e", "🎉", "Offer Accepted", body);
            await SendAsync(toEmail, fullName, $"Offer Accepted — {serviceTitle}", html);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 6. Complaint Status Updated (→ User)
        // ─────────────────────────────────────────────────────────────────────
        public async Task SendComplaintStatusUpdatedEmailAsync(
            string toEmail, string fullName, string serviceTitle,
            string newStatus, string? adminNotes)
        {
            var (accent, emoji, statusMessage) = newStatus switch
            {
                "UnderReview" => ("#d97706", "🔍", "Our team is currently reviewing your complaint."),
                "Resolved" => ("#16a34a", "✅", "Your complaint has been resolved."),
                "Dismissed" => ("#64748b", "❌", "After review, your complaint has been dismissed."),
                _ => ("#64748b", "📋", "Your complaint status has been updated.")
            };

            var notesRow = !string.IsNullOrWhiteSpace(adminNotes)
                ? InfoRow("Admin Notes", adminNotes)
                : string.Empty;

            var body = $@"
{Greeting(fullName)}
<h2 style=""margin:16px 0 8px;font-size:20px;color:#1e293b;"">Complaint Update {emoji}</h2>
<p style=""margin:0 0 16px;font-size:15px;color:#475569;line-height:1.6;"">
  {statusMessage}
</p>
{InfoTable(
    InfoRow("Service", serviceTitle) +
    InfoRow("New Status", newStatus) +
    notesRow
)}
<p style=""margin:24px 0 0;font-size:14px;color:#94a3b8;"">
  Log in to My Complaints to view the full details.
</p>";

            var html = Wrap(accent, emoji, $"Complaint Update — {newStatus}", body);
            await SendAsync(toEmail, fullName, $"Your Complaint Status: {newStatus}", html);
        }
    }
}