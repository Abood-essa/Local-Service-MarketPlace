namespace Local_Service_marketPlace.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string fullName, string role);
        Task SendBookingCreatedEmailAsync(string toEmail, string fullName, string serviceTitle, int bookingId, decimal price);
        Task SendBookingStatusChangedEmailAsync(string toEmail, string fullName, string serviceTitle, int bookingId, string newStatus, string message);
        Task SendNewOfferEmailAsync(string toEmail, string fullName, string serviceTitle, int requestId, string providerName, decimal offeredPrice);
        Task SendOfferAcceptedEmailAsync(string toEmail, string fullName, string serviceTitle, int bookingId, decimal finalPrice);
        Task SendComplaintStatusUpdatedEmailAsync(string toEmail, string fullName, string serviceTitle, string newStatus, string? adminNotes);
        Task SendContactEmailAsync(string fromName, string fromEmail, string subject, string message);
    }
}