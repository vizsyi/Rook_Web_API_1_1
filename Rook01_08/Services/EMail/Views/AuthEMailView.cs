using Microsoft.AspNetCore.Http.HttpResults;

namespace Rook01_08.Services.EMail.Views
{
    public static class AuthEMailView
    {
        public static Task SendConfirmationAsync(string emailTo, string userName, string confirmationLink)
        {
            var htmlMessage = "Dear " + userName + ",<br><br>" + confirmationLink
                + "<br>Oldrook team";

            GMailer.SendEMailAsync(emailTo, "Confirm your e-mail address", htmlMessage);

            return Task.CompletedTask;
        }

    }
}
