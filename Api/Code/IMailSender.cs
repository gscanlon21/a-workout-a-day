namespace Api.Code;

public interface IMailSender
{
    Task SendMail(string from, string to, string subject, string body, CancellationToken cancellationToken);
}
