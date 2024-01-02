namespace Rook01_08.Services.EMail
{
    public interface IEMailer
    {

        public static abstract Task SendEMailAsync(string emailTo, string subject, string htmlMessage);

    }
}
