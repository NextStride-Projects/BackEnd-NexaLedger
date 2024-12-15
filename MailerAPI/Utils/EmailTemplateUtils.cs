namespace MailerAPI.Utils;

public static class EmailTemplateUtils
{
    public static string GetTemplate(string template, object data)
    {
        if (!ValidateEmailData(template, data))
        {
            LogData(template, data, isValid: false);
            return "<p>Invalid data for the template.</p>";
        }

        LogData(template, data, isValid: true);

        return template switch
        {
            "AdminLogin" => GetAdminLoginTemplate(data),
            "FailedAdminLogin" => GetFailedAdminLoginTemplate(data),
            "NewEmpresaRegistration" => GetNewEmpresaRegistrationTemplate(data),
            "NewUserRegistration" => GetNewUserRegistrationTemplate(data),
            "UserLogin" => GetUserLoginTemplate(data),
            "AdminDeactivatedEmpresa" => GetAdminDeactivatedEmpresaTemplate(data),
            "UserDeactivatedEmpresa" => GetUserDeactivatedEmpresaTemplate(data),
            "UserReactivatedEmpresa" => GetUserReactivatedEmpresaTemplate(data),
            "AdminReactivatedEmpresa" => GetAdminReactivatedEmpresaTemplate(data),
            _ => "<p>Template not found.</p>"
        };
    }

    private static bool ValidateEmailData(string template, object data)
    {
        if (data is not System.Text.Json.JsonElement jsonData)
        {
            return false;
        }

        return template switch
        {
            "AdminLogin" => jsonData.TryGetProperty("Ip", out _),
            "FailedAdminLogin" => jsonData.TryGetProperty("Ip", out _) && jsonData.TryGetProperty("LoginTime", out _),
            "NewEmpresaRegistration" => jsonData.TryGetProperty("EmpresaName", out _),
            "NewUserRegistration" => jsonData.TryGetProperty("UserName", out _),
            "UserLogin" => jsonData.TryGetProperty("Ip", out _) && jsonData.TryGetProperty("LoginTime", out _),
            "AdminDeactivatedEmpresa" => jsonData.TryGetProperty("EmpresaName", out _) && jsonData.TryGetProperty("Motive", out _),
            "UserDeactivatedEmpresa" => jsonData.TryGetProperty("EmpresaName", out _) && jsonData.TryGetProperty("Motive", out _),
            "UserReactivatedEmpresa" => jsonData.TryGetProperty("EmpresaName", out _) && jsonData.TryGetProperty("Motive", out _),
            "AdminReactivatedEmpresa" => jsonData.TryGetProperty("EmpresaName", out _) && jsonData.TryGetProperty("Motive", out _),
            _ => false,
        };
    }

    private static void LogData(string template, object data, bool isValid)
    {
        var status = isValid ? "Valid" : "Invalid";
        var properties = data is System.Text.Json.JsonElement jsonData
            ? string.Join(", ", jsonData.EnumerateObject().Select(p => p.Name))
            : "Unknown";
        Console.WriteLine($"[EmailTemplateUtils] Template: {template}, Status: {status}, Properties: {properties}, Data: {System.Text.Json.JsonSerializer.Serialize(data)}");
    }

    private static string GetAdminLoginTemplate(object data)
    {
        if (data is not System.Text.Json.JsonElement jsonData)
        {
            return "<p>Invalid data structure.</p>";
        }

        var ip = jsonData.TryGetProperty("Ip", out var ipProperty) ? ipProperty.GetString() : "Unknown";

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
        if (data is not System.Text.Json.JsonElement jsonData)
        {
            return "<p>Invalid data structure.</p>";
        }

        var empresaName = jsonData.TryGetProperty("EmpresaName", out var nameProperty) ? nameProperty.GetString() : "Your Empresa";

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
        if (data is not System.Text.Json.JsonElement jsonData)
        {
            return "<p>Invalid data structure.</p>";
        }

        var userName = jsonData.TryGetProperty("UserName", out var nameProperty) ? nameProperty.GetString() : "User";

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
        if (data is not System.Text.Json.JsonElement jsonData)
        {
            return "<p>Invalid data structure.</p>";
        }

        var ip = jsonData.TryGetProperty("Ip", out var ipProperty) ? ipProperty.GetString() : "Unknown";
        var loginTime = jsonData.TryGetProperty("LoginTime", out var timeProperty) ? timeProperty.GetString() : "Unknown";

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

    private static string GetFailedAdminLoginTemplate(object data)
    {
        if (data is not System.Text.Json.JsonElement jsonData)
        {
            return "<p>Invalid data structure.</p>";
        }

        var ip = jsonData.TryGetProperty("Ip", out var ipProperty) ? ipProperty.GetString() : "Unknown";
        var loginTime = jsonData.TryGetProperty("LoginTime", out var timeProperty) ? timeProperty.GetString() : "Unknown";

        return $@"
        <html>
            <body style='font-family: Arial, sans-serif;'>
                <h1 style='color: #FF0000;'>Failed Admin Login Attempt</h1>
                <p>Dear Admin,</p>
                <p>There was a failed attempt to log in to the Admin Dashboard.</p>
                <p><strong>IP Address:</strong> {ip}</p>
                <p><strong>Time:</strong> {loginTime}</p>
                <p style='color: #888;'>If this was not you, please investigate immediately.</p>
            </body>
        </html>";
    }

    private static string GetAdminDeactivatedEmpresaTemplate(object data)
    {
        if (data is not System.Text.Json.JsonElement jsonData)
        {
            return "<p>Invalid data structure.</p>";
        }

        var empresaName = jsonData.TryGetProperty("EmpresaName", out var nameProperty) ? nameProperty.GetString() : "Your Empresa";
        var motive = jsonData.TryGetProperty("Motive", out var motiveProperty) ? motiveProperty.GetString() : "Unknown motive";

        return $@"
        <html>
            <body style='font-family: Arial, sans-serif;'>
                <h1 style='color: #FF0000;'>Your Empresa has been deactivated</h1>
                <p>Dear {empresaName},</p>
                <p>Your Empresa has been deactivated by an admin for the following reason:</p>
                <p><strong>Reason:</strong> {motive}</p>
                <p>If you believe this is a mistake, please contact support immediately.</p>
            </body>
        </html>";
    }

    private static string GetUserDeactivatedEmpresaTemplate(object data)
    {
        if (data is not System.Text.Json.JsonElement jsonData)
        {
            return "<p>Invalid data structure.</p>";
        }

        var empresaName = jsonData.TryGetProperty("EmpresaName", out var nameProperty) ? nameProperty.GetString() : "An Empresa";
        var motive = jsonData.TryGetProperty("Motive", out var motiveProperty) ? motiveProperty.GetString() : "Unknown motive";
        var responsiblePerson = jsonData.TryGetProperty("ResponsiblePerson", out var personProperty) ? personProperty.GetString() : "Unknown";

        return $@"
        <html>
            <body style='font-family: Arial, sans-serif;'>
                <h1 style='color: #FF0000;'>An Empresa has been deactivated</h1>
                <p>Dear Admin,</p>
                <p>The Empresa <strong>{empresaName}</strong> has been deactivated by <strong>{responsiblePerson}</strong> for the following reason:</p>
                <p><strong>Reason:</strong> {motive}</p>
                <p>If this action was unauthorized, please take immediate action.</p>
            </body>
        </html>";
    }

    private static string GetAdminReactivatedEmpresaTemplate(object data)
    {
        if (data is not System.Text.Json.JsonElement jsonData)
        {
            return "<p>Invalid data structure.</p>";
        }

        var empresaName = jsonData.TryGetProperty("EmpresaName", out var nameProperty) ? nameProperty.GetString() : "Unknown Empresa";
        var responsiblePerson = jsonData.TryGetProperty("ResponsiblePerson", out var personProperty) ? personProperty.GetString() : "Unknown";
        var motive = jsonData.TryGetProperty("Motive", out var motiveProperty) ? motiveProperty.GetString() : "No motive provided";

        return $@"
        <html>
            <body style='font-family: Arial, sans-serif;'>
                <h1 style='color: #006400;'>Empresa Reactivated</h1>
                <p>Dear {responsiblePerson},</p>
                <p>Your Empresa <strong>{empresaName}</strong> has been reactivated.</p>
                <p><strong>Reason:</strong> {motive}</p>
            </body>
        </html>";
    }

    private static string GetUserReactivatedEmpresaTemplate(object data)
    {
        if (data is not System.Text.Json.JsonElement jsonData)
        {
            return "<p>Invalid data structure.</p>";
        }

        var empresaName = jsonData.TryGetProperty("EmpresaName", out var nameProperty) ? nameProperty.GetString() : "Unknown Empresa";
        var responsiblePerson = jsonData.TryGetProperty("ResponsiblePerson", out var personProperty) ? personProperty.GetString() : "Unknown";
        var motive = jsonData.TryGetProperty("Motive", out var motiveProperty) ? motiveProperty.GetString() : "No motive provided";

        return $@"
        <html>
            <body style='font-family: Arial, sans-serif;'>
                <h1 style='color: #006400;'>Empresa Reactivated</h1>
                <p>Dear Admin,</p>
                <p>The Empresa <strong>{empresaName}</strong>, managed by <strong>{responsiblePerson}</strong>, has been reactivated.</p>
                <p><strong>Reason:</strong> {motive}</p>
            </body>
        </html>";
    }


}
