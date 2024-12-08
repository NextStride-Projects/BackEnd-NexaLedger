namespace MailerAPI.Utils;

public static class EmailTemplateUtils
{
    public static string GetTemplate(string template, object data)
    {
        return template switch
        {
            "AdminLogin" => GetAdminLoginTemplate(data),
            "NewEmpresaRegistration" => GetNewEmpresaRegistrationTemplate(data),
            "NewUserRegistration" => GetNewUserRegistrationTemplate(data),
            "UserLogin" => GetUserLoginTemplate(data),
            _ => "<p>Template not found.</p>"
        };
    }

    private static string GetAdminLoginTemplate(object data)
    {
        var ip = data?.GetType()?.GetProperty("Ip")?.GetValue(data)?.ToString() ?? "Unknown";
        return $@"
            <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h1 style='color: #006400;'>NexaLedger Admin Login</h1>
                    <p>Dear Admin,</p>
                    <p>There has been a login to the Admin Dashboard.</p>
                    <p><strong>IP Address:</strong> {ip}</p>
                    <p style='color: #888;'>If this was not you, please take immediate action.</p>
                </body>
            </html>";
    }

    private static string GetNewEmpresaRegistrationTemplate(object data)
    {
        var empresaName = data?.GetType()?.GetProperty("EmpresaName")?.GetValue(data)?.ToString() ?? "Your Empresa";
        return $@"
            <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h1 style='color: #006400;'>Welcome to NexaLedger!</h1>
                    <p>Dear {empresaName},</p>
                    <p>Your registration has been completed successfully.</p>
                    <p>We are thrilled to have you onboard.</p>
                </body>
            </html>";
    }

    private static string GetNewUserRegistrationTemplate(object data)
    {
        var userName = data?.GetType()?.GetProperty("UserName")?.GetValue(data)?.ToString() ?? "User";
        return $@"
            <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h1 style='color: #006400;'>Welcome to NexaLedger!</h1>
                    <p>Dear {userName},</p>
                    <p>Your account has been created successfully.</p>
                    <p>You can now log in and start managing your tasks efficiently.</p>
                </body>
            </html>";
    }

    private static string GetUserLoginTemplate(object data)
    {
        var ip = data?.GetType()?.GetProperty("Ip")?.GetValue(data)?.ToString() ?? "Unknown";
        var loginTime = data?.GetType()?.GetProperty("LoginTime")?.GetValue(data)?.ToString() ?? "Unknown";
        return $@"
            <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h1 style='color: #006400;'>NexaLedger Login Notification</h1>
                    <p>Dear User,</p>
                    <p>You have logged in to your NexaLedger account.</p>
                    <p><strong>IP Address:</strong> {ip}</p>
                    <p><strong>Login Time:</strong> {loginTime}</p>
                    <p>If this was not you, please secure your account immediately.</p>
                </body>
            </html>";
    }
}
