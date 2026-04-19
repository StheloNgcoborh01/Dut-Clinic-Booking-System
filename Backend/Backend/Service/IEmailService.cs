using System;

namespace Backend.Service;

public interface IEmailService
{
Task SendVerificationEmail(string config, string code);

}
