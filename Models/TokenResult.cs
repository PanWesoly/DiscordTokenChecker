using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DiscordTokenChecker.Models;

public enum CheckStatus
{
    Pending,
    Checking,
    Valid,
    Invalid,
    Error
}

public class TokenResult : INotifyPropertyChanged
{
    private CheckStatus _status = CheckStatus.Pending;
    private string _token = "";
    private string _username = "";
    private string _email = "";
    private string _phone = "";
    private string _nitroType = "";
    private string _nitroSince = "";
    private string _locale = "";
    private bool _verified;
    private string _twoFA = "";
    private string _connectedAccounts = "";
    private string _paymentInfo = "";
    private string _nitroBalance = "";
    private string _id = "";

    public string Token { get => _token; set => SetField(ref _token, value); }
    public string TokenShort => Token.Length > 20 ? Token[..20] + "..." : Token;
    public string Id { get => _id; set => SetField(ref _id, value); }
    public string Username { get => _username; set => SetField(ref _username, value); }
    public string Email { get => _email; set => SetField(ref _email, value); }
    public string Phone { get => _phone; set => SetField(ref _phone, value); }
    public string NitroType { get => _nitroType; set => SetField(ref _nitroType, value); }
    public string NitroSince { get => _nitroSince; set => SetField(ref _nitroSince, value); }
    public string Locale { get => _locale; set => SetField(ref _locale, value); }
    public bool Verified { get => _verified; set => SetField(ref _verified, value); }
    public string TwoFA { get => _twoFA; set => SetField(ref _twoFA, value); }
    public string ConnectedAccounts { get => _connectedAccounts; set => SetField(ref _connectedAccounts, value); }
    public string PaymentInfo { get => _paymentInfo; set => SetField(ref _paymentInfo, value); }
    public string NitroBalance { get => _nitroBalance; set => SetField(ref _nitroBalance, value); }
    public CheckStatus Status { get => _status; set => SetField(ref _status, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            if (name == nameof(Token))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TokenShort)));
        }
    }
}
